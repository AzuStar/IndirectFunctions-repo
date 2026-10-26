using IndirectFunctions;
using MessagePack;
using ProtoBuf;

namespace IndirectFunctions.SerializationExamples;

public static class Program
{
    private static int checks;

    public static async Task Main(string[] args)
    {
        if (args is ["--write", var format, var path])
        {
            using var sender = new Endpoint();
            File.WriteAllBytes(path + ".job", RecordTransfer.Produce(sender, format));
            File.WriteAllBytes(path + ".captures", Formats.Serialize(format, CapturePersistence.Produce(format)));
            return;
        }
        if (args is ["--read", var readFormat, var readPath])
        {
            using var receiver = new Endpoint();
            RecordTransfer.Consume(receiver, readFormat, File.ReadAllBytes(readPath + ".job"));
            CapturePersistence.Consume(readFormat, Formats.Deserialize<CaptureSnapshot>(readFormat, File.ReadAllBytes(readPath + ".captures")));
            Console.WriteLine($"PASS cold {readFormat} record/list restore: {checks} checks");
            return;
        }
        BasicExamples.Run();
        foreach (string backend in new[] { "json", "messagepack", "protobuf" })
        {
            using var sender = new Endpoint();
            using var receiver = new Endpoint();
            RecordTransfer.Consume(receiver, backend, RecordTransfer.Produce(sender, backend));
            CapturePersistence.Consume(backend, Formats.Deserialize<CaptureSnapshot>(backend,
                Formats.Serialize(backend, CapturePersistence.Produce(backend))));
            CapturePersistence.Consume(backend, Formats.Deserialize<CaptureSnapshot>(backend,
                Formats.Serialize(backend, CapturePersistence.Produce(backend, 0))), 0);
            CapturePersistence.RejectBadSchema(backend, CapturePersistence.Produce(backend));
            IdentityChecks(backend, sender);
            await ProducerConsumer.Run(backend);
        }
        Console.WriteLine($"Serialization: {checks} checks passed across JSON, MessagePack, protobuf.");
    }

    private static void IdentityChecks(string format, Endpoint endpoint)
    {
        var managed = ManagedNFunc<int, int>.Create("samples.managed.double.v1", static value => value * 2);
        var action = NAction<int>.Create(endpoint.Context, "samples.action.write.v1", static value => Sink = value);
        var managedAction = ManagedNAction<int>.Create("samples.managed-action.write.v1", static value => Sink = value);
        var bundle = new CodeBundle { Codes = [endpoint.Double.ToBytes(), managed.ToBytes(), action.ToBytes(), managedAction.ToBytes()] };
        var decoded = Formats.Deserialize<CodeBundle>(format, Formats.Serialize(format, bundle));
        foreach (var code in decoded.Codes) Equal(16, code.Length);
        Equal(8, NFunc<int, int>.FromBytes(decoded.Codes[0]).Invoke(endpoint.Context, 4));
        Equal(8, ManagedNFunc<int, int>.FromBytes(decoded.Codes[1]).Invoke(4));
        NAction<int>.FromBytes(decoded.Codes[2]).Invoke(endpoint.Context, 7);
        Equal(7, Sink);
        ManagedNAction<int>.FromBytes(decoded.Codes[3]).Invoke(9);
        Equal(9, Sink);
        Reject(() => NFunc<int>.FromBytes(decoded.Codes[0]));
        Reject(() => NFunc<int, int>.FromBytes(decoded.Codes[0][..15]));
        var unknown = Formats.Deserialize<CodeBundle>(format, Formats.Serialize(format,
            new CodeBundle { Codes = [NCodeId.FromKey("samples.missing").ToBytes()] }));
        Reject(() => NFunc<int, int>.FromBytes(unknown.Codes[0]));
        foreach (var badCode in new[] { unknown.Codes[0], decoded.Codes[0][..15], decoded.Codes[2] })
        {
            // Exercise the actual record adapters, including their error wrapping.
            byte[] payload = format == "protobuf"
                ? Formats.Serialize(format, new IntFunctionSurrogate { Code = badCode })
                : Formats.Serialize(format, badCode);
            Reject(() => Formats.Deserialize<NFunc<int, int>>(format, payload));
        }
        Console.WriteLine($"PASS {format} unmanaged/managed Func/Action identities and invalid IDs");
    }

    private static int Sink;

    public static void Equal<T>(T expected, T actual)
    {
        ++checks;
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new InvalidOperationException($"Expected {expected}; got {actual}.");
    }

    public static void Reject(Action action)
    {
        ++checks;
        try { action(); }
        catch (InvalidOperationException) { return; }
        catch (ArgumentException) { return; }
        catch (MessagePackSerializationException e) when (e.InnerException is InvalidOperationException or ArgumentException) { return; }
        throw new Exception("Expected validation failure.");
    }
}

[MessagePackObject, ProtoContract]
public sealed class CodeBundle
{
    [Key(0), ProtoMember(1)] public List<byte[]> Codes { get; set; } = [];
}
