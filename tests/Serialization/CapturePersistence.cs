using IndirectFunctions;
using MessagePack;
using ProtoBuf;

namespace IndirectFunctions.SerializationExamples;

public static class CapturePersistence
{
    public static CaptureSnapshot Produce(string format, int initialValue = 6)
    {
        using var context = new NContext();
        var callbacks = Bootstrap(context, initialValue);
        var graph = context.ExportCaptures(
            [callbacks.Read.CaptureRef, callbacks.Increment.CaptureRef, callbacks.Read.CaptureRef], new IntLeafCodec(format));
        return new CaptureSnapshot
        {
            ReadCode = callbacks.Read.ToBytes(),
            IncrementCode = callbacks.Increment.ToBytes(),
            Graph = ToWire(graph)
        };
    }

    private static (NFunc<int> Read, NFunc<int> Increment) Bootstrap(NContext context, int value)
    {
        var read = NFunc<int>.Create(context, "samples.capture.read.v1", () => value);
        var increment = NFunc<int>.Create(context, "samples.capture.increment.v1", () => ++value);
        return (read, increment);
    }

    public static void Consume(string format, CaptureSnapshot snapshot, int initialValue = 6)
    {
        using var receiver = new NContext();
        // Cold import deliberately runs before Bootstrap: generated schemas register at assembly load.
        var decoded = NFunc<int>.FromBytes(snapshot.ReadCode);
        Program.Equal(false, decoded.IsBound);
        Program.Reject(() => decoded.Invoke(receiver));
        var roots = receiver.ImportCaptures(FromWire(snapshot.Graph), new IntLeafCodec(format));
        var read = decoded.Bind(receiver, roots[0]);
        var increment = NFunc<int>.FromBytes(snapshot.IncrementCode).Bind(receiver, roots[1]);
        Program.Equal(initialValue, read.Invoke(receiver));
        Program.Equal(initialValue + 1, increment.Invoke(receiver));
        Program.Equal(initialValue + 1, read.Invoke(receiver));
        Program.Equal(roots[0], roots[2]);

        var decodedManaged = ManagedNFunc<int>.FromBytes(snapshot.ReadCode);
        Program.Equal(false, decodedManaged.IsBound);
        Program.Reject(() => decodedManaged.Invoke());
        var retained = decodedManaged.Bind(receiver.ResolveCapture(roots[0]));
        Program.Equal(initialValue + 1, retained.Invoke());

        using var other = new NContext();
        Program.Reject(() => read.Invoke(other));
        var independentRoots = other.ImportCaptures(FromWire(snapshot.Graph), new IntLeafCodec(format));
        var independent = decoded.Bind(other, independentRoots[0]);
        Program.Equal(initialValue, independent.Invoke(other));
        receiver.Release(roots[2]);
        Program.Reject(() => read.Invoke(receiver));
        Program.Equal(initialValue + 2, increment.Invoke(receiver));
        Program.Equal(initialValue + 2, retained.Invoke());
        receiver.Dispose();
        Program.Reject(() => increment.Invoke(receiver));
        Program.Equal(initialValue + 2, retained.Invoke());
        Console.WriteLine($"PASS {format} capture graph, aliases, independent restore, lifetime");
    }

    public static void RejectBadSchema(string format, CaptureSnapshot snapshot)
    {
        snapshot.Graph.Nodes[0].SchemaId = NCodeId.FromKey("samples.unknown-schema").ToBytes();
        var decoded = Formats.Deserialize<CaptureSnapshot>(format, Formats.Serialize(format, snapshot));
        using var context = new NContext();
        Program.Reject(() => context.ImportCaptures(FromWire(decoded.Graph), new IntLeafCodec(format)));
    }

    public static WireGraph ToWire(NCaptureGraph graph) => new()
    {
        Nodes = graph.Nodes.Select(n => new WireNode
        {
            NodeId = n.NodeId,
            SchemaId = n.SchemaId.ToBytes(),
            Fields = n.Fields.Select(f => new WireField
            {
                Name = f.Name,
                Kind = f.Value.Kind,
                NodeId = f.Value.NodeId,
                Payload = (byte[]?)f.Value.Leaf
            }).ToArray()
        }).ToArray(),
        Roots = graph.Roots.ToArray(),
        RootIds = graph.RootIds?.ToArray()
    };

    public static NCaptureGraph FromWire(WireGraph graph) => new(
        graph.Nodes.Select(n => new NCaptureNode(n.NodeId, NCodeId.FromBytes(n.SchemaId), n.Fields.Select(f =>
            new NCaptureField(f.Name, f.Kind == NCaptureValueKind.NodeReference
                ? NCaptureValue.FromReference(f.NodeId) : NCaptureValue.FromLeaf(f.Payload))).ToArray())).ToArray(),
        graph.Roots)
    { RootIds = graph.RootIds };
}

// Extend leaf contracts for application types; this example captures only int values.
public sealed class IntLeafCodec(string format) : INCaptureLeafCodec
{
    public object Encode<T>(T value)
    {
        if (typeof(T) != typeof(int)) throw new NotSupportedException($"Unsupported leaf: {typeof(T)}.");
        return Formats.Serialize(format, new IntLeaf { Value = (int)(object)value! });
    }

    public T Decode<T>(object? encoded)
    {
        if (typeof(T) != typeof(int) || encoded is not byte[] bytes)
            throw new NotSupportedException($"Unsupported leaf: {typeof(T)}.");
        return (T)(object)Formats.Deserialize<IntLeaf>(format, bytes).Value;
    }
}

[MessagePackObject, ProtoContract]
public sealed class IntLeaf
{
    [Key(0), ProtoMember(1)] public int Value { get; set; }
}

[MessagePackObject, ProtoContract]
public sealed class CaptureSnapshot
{
    [Key(0), ProtoMember(1)] public byte[] ReadCode { get; set; } = [];
    [Key(1), ProtoMember(2)] public byte[] IncrementCode { get; set; } = [];
    [Key(2), ProtoMember(3)] public WireGraph Graph { get; set; } = new();
}

[MessagePackObject, ProtoContract]
public sealed class WireGraph
{
    [Key(0), ProtoMember(1)] public WireNode[] Nodes { get; set; } = [];
    [Key(1), ProtoMember(2)] public int[] Roots { get; set; } = [];
    [Key(2), ProtoMember(3)] public int[]? RootIds { get; set; }
}

[MessagePackObject, ProtoContract]
public sealed class WireNode
{
    [Key(0), ProtoMember(1)] public int NodeId { get; set; }
    [Key(1), ProtoMember(2)] public byte[] SchemaId { get; set; } = [];
    [Key(2), ProtoMember(3)] public WireField[] Fields { get; set; } = [];
}

[MessagePackObject, ProtoContract]
public sealed class WireField
{
    [Key(0), ProtoMember(1)] public string Name { get; set; } = "";
    [Key(1), ProtoMember(2)] public NCaptureValueKind Kind { get; set; }
    [Key(2), ProtoMember(3)] public int NodeId { get; set; }
    [Key(3), ProtoMember(4)] public byte[]? Payload { get; set; }
}
