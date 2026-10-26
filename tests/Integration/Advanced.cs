using IndirectFunctions;

namespace IndirectFunctions.Integration;

internal static partial class Program
{
    private static (NFunc<int> Read, Action Change) ParameterFactory(NContext context, int value)
    {
        var fn = NFunc<int>.Create(context, "tests.parameters.read", () => value);
        value += 2;
        return (fn, () => value++);
    }

    private static void CapturedParameters()
    {
        using var context = new NContext();
        var pair = ParameterFactory(context, 1);
        Equal(3, pair.Read.Invoke(context));
        pair.Change();
        Equal(4, pair.Read.Invoke(context));
        var outer = NFunc<int, ManagedNFunc<int>>.Create(context, "tests.parameters.outer", n =>
            ManagedNFunc<int>.Create("tests.parameters.inner", () => ++n));
        var first = outer.Invoke(context, 3);
        var second = outer.Invoke(context, 8);
        Equal(4, first.Invoke());
        Equal(9, second.Invoke());
        Equal(5, first.Invoke());

        var localFactory = NFunc<ManagedNFunc<int>>.Create(context, "tests.parameters.local-factory", () =>
        {
            int state = 1;
            int Read() => ++state;
            return ManagedNFunc<int>.Create("tests.parameters.nested-local", Read);
        });
        var child = localFactory.Invoke(context);
        Equal(2, child.Invoke());
        Equal(3, child.Invoke());
    }

    private class BaseValue
    {
        public virtual int Read() => 2;
    }

    private sealed class DerivedValue : BaseValue
    {
        public override int Read() => 4;
        public NFunc<int> BindBase(NContext context) => NFunc<int>.Create(context, "tests.receivers.base-method", base.Read);
        public NFunc<int> BindBaseLambda(NContext context) => NFunc<int>.Create(context, "tests.receivers.base-lambda", () => base.Read());
        public NFunc<int> BindVirtual(NContext context) => NFunc<int>.Create(context, "tests.receivers.virtual-method", Read);
    }

    private static void VirtualAndBase()
    {
        using var context = new NContext();
        var value = new DerivedValue();
        Equal(2, value.BindBase(context).Invoke(context));
        Equal(2, value.BindBaseLambda(context).Invoke(context));
        Equal(4, value.BindVirtual(context).Invoke(context));
    }

    private sealed class PrivateOwner
    {
        private int value = 9;
        public NFunc<int> Make(NContext context) => NFunc<int>.Create(context, "tests.owners.private", () => ++value);
    }

    private static void PrivateOwners()
    {
        using var context = new NContext();
        Equal(10, new PrivateOwner().Make(context).Invoke(context));
        Equal(3, FileOwner.Make(context).Invoke(context));
    }

    private static void NamedArguments()
    {
        using var context = new NContext();
        int value = 2;
        var fn = NFunc<int>.Create(callback: () => value, key: "tests.named.read", context: context);
        value = 7;
        Equal(7, fn.Invoke(context));

        var evaluation = new List<string>();
        Counter PickReceiver() { evaluation.Add("receiver"); return new Counter { Value = 3 }; }
        NContext PickContext() { evaluation.Add("context"); return context; }
        var ordered = NFunc<int>.Create(callback: PickReceiver().Increment,
            context: PickContext(), key: "tests.named.receiver-order");
        Equal("receiver,context", string.Join(',', evaluation));
        Equal(4, ordered.Invoke(context));

        evaluation.Clear();
        Counter? PickNull() { evaluation.Add("receiver"); return null; }
        try
        {
            _ = NFunc<int>.Create(callback: PickNull()!.Increment,
                context: PickContext(), key: "tests.named.null-order");
            throw new Exception("Null receiver must fail before evaluating following arguments.");
        }
        catch (NullReferenceException) { }
        Equal("receiver", string.Join(',', evaluation));
    }

    private static void CheckedContext()
    {
        using var context = new NContext();
        int value = int.MaxValue;
        NFunc<int> fn;
        checked { fn = NFunc<int>.Create(context, "tests.checked.read", () => value + 1); }
        try { fn.Invoke(context); throw new Exception("Captured checked context was lost."); }
        catch (OverflowException) { }
    }

    private static void Differential()
    {
        using var context = new NContext();
        int expectedState = 2;
        Func<int, int> expected = n => expectedState += n;
        int actualState = 2;
        var actual = NFunc<int, int>.Create(context, "tests.differential.add", n => actualState += n);
        foreach (int delta in new[] { 1, -2, 9, 0 })
            Equal(expected(delta), actual.Invoke(context, delta));
        expectedState = 40;
        actualState = 40;
        Equal(expected(2), actual.Invoke(context, 2));
        Equal(expectedState, actualState);
    }

    private static void ScopeEntries()
    {
        using var context = new NContext();
        int initializerCalls = 0;
        int InitialValue() { ++initializerCalls; return 41; }
        if (string.Equals("skip", "skip", StringComparison.Ordinal)) goto ready;
        int value = InitialValue();
    ready:
        value = 3;
        Func<int> ordinary = () => value;
        var callback = NFunc<int>.Create(context, "tests.scope.goto", () => ++value);
        Equal(0, initializerCalls);
        Equal(4, callback.Invoke(context));
        Equal(4, ordinary());
        value = 8;
        Equal(9, callback.Invoke(context));
        Equal(9, ordinary());
    }
}

file static class FileOwner
{
    public static NFunc<int> Make(NContext context)
    {
        int value = 2;
        return NFunc<int>.Create(context, "tests.owners.file-local", () => ++value);
    }
}
