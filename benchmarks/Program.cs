using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using IndirectFunctions;

namespace IndirectFunctions.Benchmarks;

internal static class Program
{
    private static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}

[MemoryDiagnoser]
public class DispatchBenchmarks
{
    private readonly NContext context = new();
    private NFunc<int, int>[] pointers = [];
    private ManagedNFunc<int, int>[] managed = [];
    private readonly Func<int, int>[] delegates = [AddOne, AddTwo];
    [Params(false, true)] public bool MixedTargets { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        pointers = [
            NFunc<int, int>.Create(context, "bench.add-one", AddOne),
            NFunc<int, int>.Create(context, "bench.add-two", AddTwo)
        ];
        managed = [pointers[0].Bind(context), pointers[1].Bind(context)];
    }

    [GlobalCleanup] public void Cleanup() => context.Dispose();
    private static int AddOne(int n) => n + 1;
    private static int AddTwo(int n) => n + 2;

    [Benchmark(Baseline = true, OperationsPerInvoke = 1000)]
    public int CachedDelegate()
    {
        int result = 0;
        for (int i = 0; i < 1000; i++) result += delegates[MixedTargets ? i & 1 : 0](i);
        return result;
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public int UnmanagedHolder()
    {
        int result = 0;
        for (int i = 0; i < 1000; i++) result += pointers[MixedTargets ? i & 1 : 0].Invoke(context, i);
        return result;
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public int ManagedHolder()
    {
        int result = 0;
        for (int i = 0; i < 1000; i++) result += managed[MixedTargets ? i & 1 : 0].Invoke(i);
        return result;
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public int GeneratedSwitchBaseline()
    {
        int result = 0;
        for (int i = 0; i < 1000; i++) result += (MixedTargets ? i & 1 : 0) switch { 0 => AddOne(i), _ => AddTwo(i) };
        return result;
    }
}
