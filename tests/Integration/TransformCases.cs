using IndirectFunctions;

namespace IndirectFunctions.Integration;

internal static partial class Program
{
    private static void DeclarationCaptures()
    {
        using var context = new NContext();
        int first = 1, second = 2;
        var multiple = NFunc<int>.Create(context, "tests.declarations.multiple", () => first + ++second);
        Equal(4, multiple.Invoke(context));
        first = 5;
        Equal(9, multiple.Invoke(context));

        var (left, right) = (3, 4);
        Func<int> ordinary = () => left + right;
        var deconstructed = NFunc<int>.Create(context, "tests.declarations.deconstruct", () => ++left + right);
        Equal(8, deconstructed.Invoke(context));
        Equal(8, ordinary());

        int.TryParse("6", out var parsed);
        var output = NFunc<int>.Create(context, "tests.declarations.out", () => ++parsed);
        Equal(7, output.Invoke(context));

        using var resource = new CaptureResource();
        var usingCapture = NFunc<int>.Create(context, "tests.declarations.using", () => resource.Read());
        Equal(2, usingCapture.Invoke(context));
        using (var nestedResource = new CaptureResource())
        {
            var captured = NFunc<int>.Create(context, "tests.declarations.using-statement", () => nestedResource.Read());
            Equal(2, captured.Invoke(context));
        }
        int Identity<T>(T input) => input!.GetHashCode() + first;
        var generic = NFunc<int, int>.Create(context, "tests.declarations.generic-local", Identity<int>);
        Equal(8, generic.Invoke(context, 3));
        Equal(6, ExpressionPattern(5).Invoke());
        string LocalName() => "ignored";
        var names = NFunc<int, string>.Create(context, "tests.declarations.nameof", parameter => nameof(parameter) + ":" + nameof(first) + ":" + nameof(LocalName));
        Equal("parameter:first:LocalName", names.Invoke(context, 1));
        Equal("first", nameof(first));
        int Sum(int n) => n == 0 ? first : n + Sum(n - 1);
        var conversion = NFunc<Func<int, int>>.Create(context, "tests.local.ordinary-method-group", () =>
        {
            Func<int, int> callback = Sum;
            return callback;
        });
        var retained = conversion.Invoke(context);
        Equal(11, retained(3));
        first = 10;
        Equal(16, retained(3));
        IDefaultCaptured defaultInterface = new DefaultCaptured();
        Equal(7, defaultInterface.Make(context).Invoke(context));
        CatchFilterCapture();
    }

    private static void PatternCaptures()
    {
        using var context = new NContext();
        object input = 4;
        if (input is int matched)
        {
            var pattern = NFunc<int>.Create(context, "tests.patterns.if", () => ++matched);
            Equal(5, pattern.Invoke(context));
        }
        var callbacks = new List<NFunc<int>>();
        var ordinary = new List<Func<int>>();
        int i = 0;
        while (++i is int item && item < 4)
        {
            callbacks.Add(NFunc<int>.Create(context, "tests.patterns.while", () => item));
            ordinary.Add(() => item);
        }
        for (int n = 0; n < callbacks.Count; ++n)
        {
            Equal(n + 1, callbacks[n].Invoke(context));
            Equal(ordinary[n](), callbacks[n].Invoke(context));
        }
        var switched = input switch
        {
            int value => ManagedNFunc<int>.Create("tests.patterns.switch-expression", () => ++value),
            _ => throw new Exception()
        };
        Equal(5, switched.Invoke());
        switch (input)
        {
            case int value when value > 0:
                var callback = NFunc<int>.Create(context, "tests.patterns.switch-statement", () => ++value);
                Equal(5, callback.Invoke(context));
                break;
        }
        var foreachCallbacks = new List<NFunc<int>>();
        foreach (var (a, b) in new[] { (1, 2), (3, 4) })
            foreachCallbacks.Add(NFunc<int>.Create(context, "tests.patterns.foreach-deconstruct", () => a + b));
        Equal(3, foreachCallbacks[0].Invoke(context));
        Equal(7, foreachCallbacks[1].Invoke(context));

        var outCallbacks = new List<NFunc<int>>();
        int count = 0;
        while (int.TryParse((++count).ToString(), out var value) && value < 4)
            outCallbacks.Add(NFunc<int>.Create(context, "tests.patterns.while-out", () => value));
        for (int n = 0; n < outCallbacks.Count; ++n) Equal(n + 1, outCallbacks[n].Invoke(context));
        var forCallbacks = new List<NFunc<int>>();
        var ordinaryFor = new List<Func<int>>();
        for (int n = 0; int.TryParse(n.ToString(), out var result) && result < 3; n++)
        {
            forCallbacks.Add(NFunc<int>.Create(context, "tests.patterns.for-out", () => result));
            ordinaryFor.Add(() => result);
            continue;
        }
        for (int n = 0; n < forCallbacks.Count; ++n)
        {
            Equal(n, forCallbacks[n].Invoke(context));
            Equal(ordinaryFor[n](), forCallbacks[n].Invoke(context));
        }
        var doCallbacks = new List<NFunc<int>>();
        int cycle = 0;
        do { ++cycle; continue; }
        while (cycle is int snapshot && snapshot < 3 && RetainDoCallback(NFunc<int>.Create(context, "tests.patterns.do-condition", () => snapshot), doCallbacks));
        Equal(1, doCallbacks[0].Invoke(context));
        Equal(2, doCallbacks[1].Invoke(context));
    }

    private static bool RetainDoCallback(NFunc<int> callback, List<NFunc<int>> callbacks) { callbacks.Add(callback); return true; }

    private static ManagedNFunc<int> ExpressionPattern(object input) => input switch
    {
        int value => ManagedNFunc<int>.Create("tests.expression.pattern", () => ++value),
        _ => throw new Exception()
    };

    private static bool RetainFiltered(ManagedNFunc<string> callback, List<ManagedNFunc<string>> callbacks) { callbacks.Add(callback); return true; }

    private static void CatchFilterCapture()
    {
        var callbacks = new List<ManagedNFunc<string>>();
        Func<string> ordinary = null!;
        try { throw new Exception("before"); }
        catch (Exception ex) when (RetainFiltered(ManagedNFunc<string>.Create("tests.catch.filter", () => ex.Message), callbacks))
        {
            ordinary = () => ex.Message;
            ex = new Exception("after");
        }
        Equal("after", callbacks[0].Invoke());
        Equal("after", ordinary());
    }

    private sealed class CaptureResource : IDisposable
    {
        public int Read() => 2;
        public void Dispose() { }
    }

    private interface IDefaultCaptured
    {
        int Read();
        NFunc<int> Make(NContext context) => NFunc<int>.Create(context, "tests.owners.default-interface", () => Read());
    }

    private sealed class DefaultCaptured : IDefaultCaptured { public int Read() => 7; }
}
