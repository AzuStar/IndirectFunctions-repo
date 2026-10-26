using IndirectFunctions;
using MessagePack;
using ProtoBuf;

[assembly: MessagePackAssumedFormattable(typeof(NFunc<int, int>))]

namespace IndirectFunctions.SerializationExamples;

// Share this assembly and stable keys across every producer and consumer.
public sealed class Endpoint : IDisposable
{
    public NContext Context { get; } = new();
    public NFunc<int, int> Double { get; }
    public NFunc<int, int> Square { get; }

    public Endpoint()
    {
        Double = NFunc<int, int>.Create(Context, "samples.math.double.v1", DoubleValue);
        Square = NFunc<int, int>.Create(Context, "samples.math.square.v1", SquareValue);
    }

    private static int DoubleValue(int value) => value * 2;
    private static int SquareValue(int value) => value * value;
    public void Dispose() => Context.Dispose();
}

[MessagePackObject, ProtoContract]
public sealed record Job
{
    [Key(0), ProtoMember(1)] public int Input { get; init; }
    [Key(1), ProtoMember(2)] public NFunc<int, int> Function { get; init; }
    [Key(2), ProtoMember(3)] public List<NFunc<int, int>> Functions { get; set; } = [];
}

public sealed record Envelope(Job Job);
