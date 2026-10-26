using BenchmarkDotNet.Attributes;
using IndirectFunctions;

namespace IndirectFunctions.Benchmarks;

/// <summary>Identical payload reads through native delegates and both holder profiles.</summary>
[MemoryDiagnoser]
public class CapturedDispatchBenchmarks
{
    private readonly NContext context = new();
    private Func<int, int> ordinary = null!;
    private ManagedNFunc<int, int> managed;
    private NFunc<int, int> unmanaged;

    [Params(0, 64, 4096, 65536)]
    public int CaptureBytes { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var payload = CaptureOperations.CreatePayload(CaptureBytes);
        ordinary = CaptureOperations.CreateOrdinary(payload, 31);
        managed = CaptureOperations.CreateManaged(payload, 31);
        unmanaged = context.Import(managed);
    }

    [GlobalCleanup]
    public void Cleanup() => context.Dispose();

    [Benchmark(Baseline = true, OperationsPerInvoke = 1000)]
    public int OrdinaryCapturedDelegate()
    {
        int result = 0;
        for (int i = 0; i < 1000; i++) result += ordinary(i);
        return result;
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public int ManagedCapturedHolder()
    {
        int result = 0;
        for (int i = 0; i < 1000; i++) result += managed.Invoke(i);
        return result;
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public int UnmanagedCapturedHolder()
    {
        int result = 0;
        for (int i = 0; i < 1000; i++) result += unmanaged.Invoke(context, i);
        return result;
    }
}

/// <summary>
/// Capture construction, profile conversion, identity decoding, and capture-graph materialization.
/// Methods naming Release include its cost and keep reusable contexts bounded across invocations.
/// Payload arrays are prepared once; capture-graph leaves retain these arrays without backend serialization.
/// </summary>
[MemoryDiagnoser]
public class CapturedBindingBenchmarks
{
    private readonly NContext sourceContext = new();
    private readonly NContext transientContext = new();
    private byte[] payload = [];
    private byte[] codeBytes = [];
    private ManagedNFunc<int, int> managed;
    private NFunc<int, int> unmanaged;
    private NCaptureGraph graph = null!;

    [Params(0, 64, 4096, 65536)]
    public int CaptureBytes { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        payload = CaptureOperations.CreatePayload(CaptureBytes);
        managed = CaptureOperations.CreateManaged(payload, 31);
        unmanaged = sourceContext.Import(managed);
        codeBytes = unmanaged.ToBytes();
        graph = sourceContext.ExportCaptures(unmanaged.CaptureRef);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        sourceContext.Dispose();
        transientContext.Dispose();
    }

    [Benchmark]
    public Func<int, int> ConstructOrdinaryClosure() => CaptureOperations.CreateOrdinary(payload, 31);

    [Benchmark]
    public ManagedNFunc<int, int> ConstructManagedCapture() => CaptureOperations.CreateManaged(payload, 31);

    [Benchmark]
    public NFunc<int, int> ConstructUnmanagedCaptureAndRelease()
    {
        var callback = CaptureOperations.CreateUnmanaged(transientContext, payload, 31);
        transientContext.Release(callback.CaptureRef);
        return callback;
    }

    [Benchmark]
    public NFunc<int, int> ManagedToUnmanagedAndRelease()
    {
        var callback = transientContext.Import(managed);
        transientContext.Release(callback.CaptureRef);
        return callback;
    }

    [Benchmark]
    public ManagedNFunc<int, int> UnmanagedToManaged() => unmanaged.Bind(sourceContext);

    [Benchmark]
    public NFunc<int, int> DecodeCodeIdentity() => NFunc<int, int>.FromBytes(codeBytes);

    [Benchmark]
    public NFunc<int, int> DecodeAndBindExistingRoot()
        => NFunc<int, int>.FromBytes(codeBytes).Bind(sourceContext, unmanaged.CaptureRef);

    [Benchmark]
    public ManagedNFunc<int, int> DecodeManagedAndBindExistingState()
        => ManagedNFunc<int, int>.FromBytes(codeBytes).Bind(managed.CaptureState!);

    [Benchmark]
    public byte[] EncodeCodeIdentity() => unmanaged.ToBytes();

    [Benchmark]
    public NCaptureGraph ExportCaptureGraph() => sourceContext.ExportCaptures(unmanaged.CaptureRef);

    [Benchmark]
    public NCaptureRef ImportCaptureGraphAndRelease()
    {
        var captures = transientContext.ImportCaptures(graph);
        transientContext.Release(captures[0]);
        return captures[0];
    }
}

internal static class CaptureOperations
{
    public static byte[] CreatePayload(int size)
    {
        var bytes = new byte[size];
        for (int i = 0; i < bytes.Length; i++) bytes[i] = unchecked((byte)(i * 31));
        return bytes;
    }

    private static int Read(byte[] bytes, int bias, int value)
        => value + bias + (bytes.Length == 0 ? 0 : bytes[unchecked(value * 0x1F123BB5) & (bytes.Length - 1)]);

    // Keep the ordinary delegate in a marker-free lexical scope. Its closure remains compiler-native.
    public static Func<int, int> CreateOrdinary(byte[] bytes, int bias) => value => Read(bytes, bias, value);

    public static ManagedNFunc<int, int> CreateManaged(byte[] bytes, int bias)
        => ManagedNFunc<int, int>.Create("bench.capture.managed", value => Read(bytes, bias, value));

    public static NFunc<int, int> CreateUnmanaged(NContext context, byte[] bytes, int bias)
        => NFunc<int, int>.Create(context, "bench.capture.unmanaged", value => Read(bytes, bias, value));
}
