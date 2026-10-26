using System.Runtime.CompilerServices;
using System.Text.Json;
using IndirectFunctions;

namespace IndirectFunctions.Integration;

internal static partial class Program
{
    private static int passed;

    private static async Task Main(string[] args)
    {
        if (args is ["--write-snapshot", var writePath]) { WritePersistent(writePath); return; }
        if (args is ["--read-snapshot", var readPath]) { ReadPersistent(readPath); return; }
        Check("identity and static dispatch", Identity);
        Check("live sharing and owner lifetime", Sharing);
        Check("capture export and explicit rebinding", Persistence);
        Check("explicit shared state", ExplicitState);
        Check("loop variable lifetimes", Loops);
        Check("nested callbacks", Nested);
        Check("local functions", LocalFunctions);
        Check("method group receivers", Receivers);
        Check("caller expression", CallerExpressions);
        Check("closed generics and arity sixteen", Signatures);
        Check("iterator capture", IteratorCapture);
        Check("zero dispatch allocations", Allocations);
        Check("captured parameters", CapturedParameters);
        Check("virtual and base binding", VirtualAndBase);
        Check("private and file-local owners", PrivateOwners);
        Check("named arguments", NamedArguments);
        Check("checked capture context", CheckedContext);
        Check("differential traces", Differential);
        Check("JSON capture backend round trip", JsonPersistence);
        Check("declaration capture forms", DeclarationCaptures);
        Check("pattern and out-variable captures", PatternCaptures);
        Check("capture allocation at scope entry", ScopeEntries);
        await AsyncCapture();
        Console.WriteLine($"PASS async capture");
        Console.WriteLine($"Integration: {++passed} scenarios passed.");
    }

    private static void Check(string name, Action test)
    {
        test();
        ++passed;
        Console.WriteLine($"PASS {name}");
    }

    private static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new Exception($"Expected {expected}; got {actual}.");
    }

    private static void Fails(Action action)
    {
        try { action(); }
        catch (InvalidOperationException) { return; }
        catch (ArgumentException) { return; }
        throw new Exception("Expected managed validation failure.");
    }

    private static void Identity()
    {
        using var context = new NContext();
        var fn = NFunc<int, int>.Create(context, "tests.identity.double", Double);
        Equal(8, fn.Invoke(context, 4));
        var bytes = fn.ToBytes();
        Equal(16, bytes.Length);
        Equal(NCodeId.FromKey("tests.identity.double"), fn.CodeId);
        var backendBytes = JsonSerializer.Deserialize(JsonSerializer.Serialize(bytes, CaptureJsonContext.Default.ByteArray), CaptureJsonContext.Default.ByteArray)!;
        Equal(12, NFunc<int, int>.FromBytes(backendBytes).Invoke(context, 6));
        Fails(() => NFunc<int>.FromBytes(bytes));
        Fails(() => NFunc<int>.FromBytes(NCodeId.FromKey("tests.absent").ToBytes()));
        Fails(() => default(NFunc<int>).Invoke(context));
        RequireUnmanaged(fn);
    }

    private static int Double(int value) => value * 2;
    private static void RequireUnmanaged<T>(T value) where T : unmanaged { }

    private static void Sharing()
    {
        using var first = new NContext();
        using var second = new NContext();
        int value = 1;
        Func<int> ordinary = () => value;
        var fn = NFunc<int>.Create(first, "tests.sharing.read", () => value);
        var managed = ManagedNFunc<int>.Create("tests.sharing.increment", () => ++value);
        var copy = fn;
        value = 10;
        Equal(10, copy.Invoke(first));
        Equal(11, managed.Invoke());
        Equal(11, ordinary());
        var imported = second.Import(managed);
        Equal(12, imported.Invoke(second));
        Equal(12, fn.Invoke(first));
        Fails(() => fn.Invoke(second));
        var retained = fn.Bind(first);
        first.Release(fn.CaptureRef);
        Fails(() => copy.Invoke(first));
        Equal(12, retained.Invoke());
        first.Dispose();
        Equal(13, managed.Invoke());
        Equal(14, imported.Invoke(second));
    }

    private static void Persistence()
    {
        using var original = new NContext();
        int value = 4;
        var read = NFunc<int>.Create(original, "tests.persist.read", () => value);
        var increment = NFunc<int>.Create(original, "tests.persist.increment", () => ++value);
        var readCode = read.ToBytes();
        var incrementCode = increment.ToBytes();
        var snapshot = original.ExportCaptures([read.CaptureRef, increment.CaptureRef]);
        using var restored = new NContext();
        var roots = restored.ImportCaptures(snapshot);
        var decoded = NFunc<int>.FromBytes(readCode);
        Fails(() => decoded.Invoke(restored));
        var restoredRead = decoded.Bind(restored, roots[0]);
        var restoredIncrement = NFunc<int>.FromBytes(incrementCode).Bind(restored, roots[1]);
        Equal(5, restoredIncrement.Invoke(restored));
        Equal(5, restoredRead.Invoke(restored));
        Equal(4, value);
        value = 30;
        Equal(5, restoredRead.Invoke(restored));
        Equal(30, read.Invoke(original));
    }

    private static void ExplicitState()
    {
        using var context = new NContext();
        var state = new NCell<int>(2);
        var add = NFunc<int, int>.Create(context, "tests.explicit.add", state, static (s, n) => s.Value += n);
        var managed = ManagedNAction<int>.Create("tests.explicit.set", state, static (s, n) => s.Value = n);
        Equal(5, add.Invoke(context, 3));
        managed.Invoke(9);
        Equal(10, add.Invoke(context, 1));
        var copy = context.Import(add.Bind(context));
        Equal(12, copy.Invoke(context, 2));
    }

    private static void Loops()
    {
        using var context = new NContext();
        var shared = new List<NFunc<int>>();
        for (int i = 0; i < 3; i++)
            shared.Add(NFunc<int>.Create(context, "tests.loops.for", () => i));
        foreach (var fn in shared) Equal(3, fn.Invoke(context));
        var separate = new List<NFunc<int>>();
        foreach (int i in new[] { 1, 2, 3 })
            separate.Add(NFunc<int>.Create(context, "tests.loops.foreach", () => i));
        for (int i = 0; i < separate.Count; i++) Equal(i + 1, separate[i].Invoke(context));
    }

    private static void Nested()
    {
        using var context = new NContext();
        int value = 7;
        var outer = NFunc<int, ManagedNFunc<int>>.Create(context, "tests.nested.outer", n =>
        {
            int inner = n;
            return ManagedNFunc<int>.Create("tests.nested.inner", () => value + ++inner);
        });
        var first = outer.Invoke(context, 1);
        var second = outer.Invoke(context, 10);
        Equal(9, first.Invoke());
        Equal(18, second.Invoke());
        value = 20;
        Equal(23, first.Invoke());
    }

    private static void LocalFunctions()
    {
        using var context = new NContext();
        int bias = 2;
        int Sum(int n) => n == 0 ? bias : n + Sum(n - 1);
        var fn = NFunc<int, int>.Create(context, "tests.local.recursive", n => Sum(n));
        Equal(8, fn.Invoke(context, 3));
        bias = 5;
        Equal(11, fn.Invoke(context, 3));
    }

    private static void Receivers()
    {
        using var context = new NContext();
        var receiver = new Counter { Value = 3 };
        int evaluations = 0;
        Counter GetReceiver() { ++evaluations; return receiver; }
        var fn = NFunc<int>.Create(context, "tests.receivers.reference", GetReceiver().Increment);
        Equal(1, evaluations);
        Equal(4, fn.Invoke(context));
        receiver = new Counter { Value = 100 };
        Equal(5, fn.Invoke(context));
        var value = new ValueCounter { Value = 4 };
        var copied = NFunc<int>.Create(context, "tests.receivers.struct", value.Increment);
        Equal(5, copied.Invoke(context));
        Equal(6, copied.Invoke(context));
        Equal(4, value.Value);
        Counter? missing = null;
        try
        {
            _ = NFunc<int>.Create(context, "tests.receivers.null", missing!.Increment);
            throw new Exception("Null receiver must fail during binding.");
        }
        catch (NullReferenceException) { }
    }

    private sealed class Counter { public int Value; public int Increment() => ++Value; }
    private struct ValueCounter { public int Value; public int Increment() => ++Value; }

    private static string Expression(int value, [CallerArgumentExpression(nameof(value))] string text = "") => text;
    private static void CallerExpressions()
    {
        using var context = new NContext();
        int value = 3;
        var fn = NFunc<string>.Create(context, "tests.caller.expression", () => Expression(value));
        value = 4;
        Equal("value", fn.Invoke(context));
    }

    private static T IdentityOf<T>(T value) => value;
    private static void Signatures()
    {
        using var context = new NContext();
        var closed = NFunc<int, int>.Create(context, "tests.signatures.generic-int", IdentityOf<int>);
        Equal(9, closed.Invoke(context, 9));
        var sixteen = NFunc<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>
            .Create(context, "tests.signatures.sixteen", static (a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p) => a+b+c+d+e+f+g+h+i+j+k+l+m+n+o+p);
        Equal(16, sixteen.Invoke(context, 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1));
    }

    private static IEnumerable<NFunc<int>> YieldCallbacks(NContext context)
    {
        int value = 1;
        var fn = NFunc<int>.Create(context, "tests.iterator.read", () => value);
        yield return fn;
        value = 5;
        yield return fn;
    }

    private static void IteratorCapture()
    {
        using var context = new NContext();
        using var iterator = YieldCallbacks(context).GetEnumerator();
        iterator.MoveNext();
        var first = iterator.Current;
        Equal(1, first.Invoke(context));
        iterator.MoveNext();
        Equal(5, first.Invoke(context));
    }

    private static async Task AsyncCapture()
    {
        using var context = new NContext();
        int value = 1;
        var fn = NFunc<int>.Create(context, "tests.async.read", () => value);
        await Task.Yield();
        value = 8;
        Equal(8, fn.Invoke(context));
        var asyncFn = NFunc<Task<int>>.Create(context, "tests.async.callback", async () =>
        {
            await Task.Yield();
            return value;
        });
        Equal(8, await asyncFn.Invoke(context));
    }

    private static void Allocations()
    {
        using var context = new NContext();
        int value = 3;
        var fn = NFunc<int, int>.Create(context, "tests.allocations.capture", n => n + value);
        int sum = 0;
        for (int i = 0; i < 20_000; i++) sum += fn.Invoke(context, i);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 100_000; i++) sum += fn.Invoke(context, i);
        Equal(0L, GC.GetAllocatedBytesForCurrentThread() - before);
        GC.KeepAlive(sum);
    }
}
