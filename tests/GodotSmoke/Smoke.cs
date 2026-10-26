using Godot;
using IndirectFunctions;

public partial class Smoke : RefCounted
{
    public int Run()
    {
        using var context = new NContext();
        int bonus = 4;
        var fn = NFunc<int, int>.Create(context, "godot.smoke.add", n => n + bonus);
        bonus = 8;
        if (fn.Invoke(context, 2) != 10) return 1;
        var snapshot = context.ExportCaptures([fn.CaptureRef]);
        using var restored = new NContext();
        var captures = restored.ImportCaptures(snapshot);
        var loaded = NFunc<int, int>.FromBytes(fn.ToBytes()).Bind(restored, captures[0]);
        if (loaded.Invoke(restored, 3) != 11) return 2;
        GD.Print("PASS Godot C# indirect dispatch, shared capture, and rebinding");
        return 0;
    }
}
