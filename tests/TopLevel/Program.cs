using IndirectFunctions;

using var context = new NContext();
int left = 1, right = 2;
var sum = NFunc<int>.Create(context, "tests.top.multi", () => ++left + right);
left = 4;
if (sum.Invoke(context) != 7) throw new Exception("Top-level multi declaration lost live state.");

var (a, b) = (2, 3);
var tuple = ManagedNFunc<int>.Create("tests.top.tuple", () => ++a + b);
if (tuple.Invoke() != 6) throw new Exception("Top-level deconstruction capture failed.");

int.TryParse("5", out var parsed);
var output = ManagedNFunc<int>.Create("tests.top.out", () => ++parsed);
if (output.Invoke() != 6) throw new Exception("Top-level out-variable capture failed.");

var arguments = ManagedNFunc<int>.Create("tests.top.args", () => args.Length);
if (arguments.Invoke() != args.Length) throw new Exception("Implicit args capture failed.");

int Local<T>(T value) => value!.GetHashCode() + left;
var local = NFunc<int, int>.Create(context, "tests.top.generic-local", Local<int>);
if (local.Invoke(context, 2) != 7) throw new Exception("Closed generic local function capture failed.");

await Task.Yield();
left = 10;
if (local.Invoke(context, 2) != 12) throw new Exception("Async top-level capture failed.");

var owner = new SetterOwner();
owner.Value = 3;
if (owner.Callback.Invoke() != 4) throw new Exception("Implicit setter value capture failed.");
owner.Value = 8;
if (owner.Callback.Invoke() != 9) throw new Exception("Setter capture lifetime failed.");
Console.WriteLine("PASS top-level and accessor captures");

internal sealed class SetterOwner
{
    public ManagedNFunc<int> Callback;
    public int Value
    {
        set { Callback = ManagedNFunc<int>.Create("tests.top.setter", () => ++value); }
    }
}
