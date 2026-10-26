using System.Text.Json;
using System.Text.Json.Serialization;
using IndirectFunctions;

namespace IndirectFunctions.Integration;

internal static partial class Program
{
    private static void WritePersistent(string path)
    {
        using var context = new NContext();
        int value = 6;
        var read = NFunc<int>.Create(context, "tests.json.read", () => value);
        var increment = NFunc<int>.Create(context, "tests.json.increment", () => ++value);
        var graph = context.ExportCaptures([read.CaptureRef, increment.CaptureRef, read.CaptureRef], new IntLeafCodec());
        var payload = new PersistedData(read.ToBytes(), increment.ToBytes(), ToWire(graph));
        File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(payload, CaptureJsonContext.Default.PersistedData));
    }

    private static void ReadPersistent(string path)
    {
        var payload = JsonSerializer.Deserialize(File.ReadAllBytes(path), CaptureJsonContext.Default.PersistedData)!;
        using var context = new NContext();
        var roots = context.ImportCaptures(FromWire(payload.Graph), new IntLeafCodec());
        var read = NFunc<int>.FromBytes(payload.ReadCode).Bind(context, roots[0]);
        var increment = NFunc<int>.FromBytes(payload.IncrementCode).Bind(context, roots[1]);
        Equal(6, read.Invoke(context));
        Equal(7, increment.Invoke(context));
        Equal(7, read.Invoke(context));
        Equal(roots[0], roots[2]);
        context.Release(roots[2]);
        Fails(() => read.Invoke(context));
        Equal(8, increment.Invoke(context));
        Console.WriteLine("PASS cold JSON graph restore before any callback factory");
    }

    private static void JsonPersistence()
    {
        string path = Path.Combine(Path.GetTempPath(), $"indirect-functions-{Guid.NewGuid():N}.json");
        try { WritePersistent(path); ReadPersistent(path); }
        finally { File.Delete(path); }
    }

    private static WireGraph ToWire(NCaptureGraph graph) => new(
        graph.Nodes.Select(n => new WireNode(n.NodeId, n.SchemaId.ToBytes(), n.Fields.Select(f =>
            new WireField(f.Name, f.Value.Kind, f.Value.NodeId, (byte[]?)f.Value.Leaf)).ToArray())).ToArray(),
        graph.Roots.ToArray(), graph.RootIds?.ToArray());

    private static NCaptureGraph FromWire(WireGraph graph) => new(
        graph.Nodes.Select(n => new NCaptureNode(n.NodeId, NCodeId.FromBytes(n.SchemaId), n.Fields.Select(f =>
            new NCaptureField(f.Name, f.Kind == NCaptureValueKind.NodeReference
                ? NCaptureValue.FromReference(f.NodeId) : NCaptureValue.FromLeaf(f.Payload))).ToArray())).ToArray(),
        graph.Roots) { RootIds = graph.RootIds };
}

internal sealed class IntLeafCodec : INCaptureLeafCodec
{
    public object? Encode<T>(T value)
    {
        if (typeof(T) != typeof(int)) throw new NotSupportedException($"Fixture codec does not support {typeof(T)}.");
        return JsonSerializer.SerializeToUtf8Bytes((int)(object)value!, CaptureJsonContext.Default.Int32);
    }

    public T Decode<T>(object? encoded)
    {
        if (typeof(T) != typeof(int) || encoded is not byte[] bytes)
            throw new NotSupportedException($"Fixture codec does not support {typeof(T)}.");
        return (T)(object)JsonSerializer.Deserialize(bytes, CaptureJsonContext.Default.Int32);
    }
}

internal sealed record WireField(string Name, NCaptureValueKind Kind, int NodeId, byte[]? Payload);
internal sealed record WireNode(int NodeId, byte[] SchemaId, WireField[] Fields);
internal sealed record WireGraph(WireNode[] Nodes, int[] Roots, int[]? RootIds);
internal sealed record PersistedData(byte[] ReadCode, byte[] IncrementCode, WireGraph Graph);

[JsonSerializable(typeof(byte[]))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(PersistedData))]
internal partial class CaptureJsonContext : JsonSerializerContext { }
