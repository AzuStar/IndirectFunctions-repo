using IndirectFunctions;

namespace IndirectFunctions.PackageTests.CallbackLibrary;

public static class Callbacks
{
    public static int FactoriesExecuted;

    public static NFunc<int, int> CreateNamed(NContext context)
    {
        ++FactoriesExecuted;
        return NFunc<int, int>.Create(context, "package.library.double", Double);
    }

    private static int Double(int value) => value * 2;

    public static (NFunc<int> Read, NFunc<int> Increment, ManagedNFunc<int> ManagedRead, Func<int> Ordinary, Action<int> Set)
        CreateCaptured(NContext context, int initial)
    {
        ++FactoriesExecuted;
        int value = initial;
        var read = NFunc<int>.Create(context, "package.library.read", () => value);
        var increment = NFunc<int>.Create(context, "package.library.increment", () => ++value);
        var managedRead = ManagedNFunc<int>.Create("package.library.managed-read", () => value);
        Func<int> ordinary = () => value;
        Action<int> set = next => value = next;
        return (read, increment, managedRead, ordinary, set);
    }
}
