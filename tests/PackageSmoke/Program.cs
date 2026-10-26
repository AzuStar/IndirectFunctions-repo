using IndirectFunctions;
using IndirectFunctions.PackageTests.CallbackLibrary;
using System.Text.Json;

namespace IndirectFunctions.PackageTests;

internal static class Program
{
    private static int checks;

    private static void Main()
    {
        using var original = new NContext();

        // No producer factory has executed. Consumer catalog must root referenced registrations.
        var decodedNamed = NFunc<int, int>.FromBytes(NCodeId.FromKey("package.library.double").ToBytes());
        Equal(14, decodedNamed.Invoke(original, 7));
        var decodedRead = NFunc<int>.FromBytes(NCodeId.FromKey("package.library.read").ToBytes());
        var decodedIncrement = NFunc<int>.FromBytes(NCodeId.FromKey("package.library.increment").ToBytes());
        var decodedManaged = ManagedNFunc<int>.FromBytes(NCodeId.FromKey("package.library.managed-read").ToBytes());
        Equal(false, decodedRead.IsBound);
        Equal(false, decodedManaged.IsBound);
        Fails(() => decodedRead.Invoke(original));
        Equal(0, Callbacks.FactoriesExecuted);

        var named = Callbacks.CreateNamed(original);
        Equal(decodedNamed.CodeId, named.CodeId);
        Equal(16, named.ToBytes().Length);
        Equal(18, named.Invoke(original, 9));

        var callbacks = Callbacks.CreateCaptured(original, 4);
        callbacks.Set(10);
        Equal(10, callbacks.Read.Invoke(original));
        Equal(11, callbacks.Increment.Invoke(original));
        Equal(11, callbacks.Ordinary());
        Equal(11, callbacks.ManagedRead.Invoke());
        var managedRoot = original.Import(callbacks.ManagedRead);
        var codec = new JsonLeafCodec();
        var snapshot = original.ExportCaptures([callbacks.Read.CaptureRef, callbacks.Increment.CaptureRef, managedRoot.CaptureRef, callbacks.Read.CaptureRef], codec);
        Equal(true, codec.Encoded > 0);
        Equal(true, snapshot.Nodes.SelectMany(node => node.Fields).All(field =>
            field.Value.Kind != NCaptureValueKind.Leaf || field.Value.Leaf is string));
        var saved = new SavedGraph(snapshot.Nodes.Select(node => new SavedNode(node.NodeId, node.SchemaId.ToBytes(),
            node.Fields.Select(field => new SavedField(field.Name, field.Value.Kind, field.Value.NodeId, field.Value.Leaf)).ToArray())).ToArray(),
            snapshot.Roots.ToArray(), snapshot.RootIds?.ToArray());
        var wire = JsonSerializer.Deserialize<SavedGraph>(JsonSerializer.SerializeToUtf8Bytes(saved))!;
        var reloaded = new NCaptureGraph(wire.Nodes.Select(node => new NCaptureNode(node.NodeId, NCodeId.FromBytes(node.SchemaId),
            node.Fields.Select(field => new NCaptureField(field.Name, new NCaptureValue(field.Kind, field.Leaf, field.NodeId))).ToArray())).ToArray(), wire.Roots)
            { RootIds = wire.RootIds };

        using var restored = new NContext();
        var roots = restored.ImportCaptures(reloaded, codec);
        Equal(true, codec.Decoded > 0);
        Equal(4, roots.Length);
        Equal(roots[0], roots[3]);
        var restoredRead = decodedRead.Bind(restored, roots[0]);
        var restoredIncrement = decodedIncrement.Bind(restored, roots[1]);
        var restoredManaged = decodedManaged.Bind(restored.ResolveCapture(roots[2]));
        Equal(12, restoredIncrement.Invoke(restored));
        Equal(12, restoredRead.Invoke(restored));
        Equal(12, restoredManaged.Invoke());
        Equal(11, callbacks.Ordinary());
        callbacks.Set(30);
        Equal(30, callbacks.Read.Invoke(original));
        Equal(12, restoredRead.Invoke(restored));

        int local = 5;
        var literal = NFunc<int>.Create(original, "package.consumer.literal", () => ++local);
        Equal(6, literal.Invoke(original));
        Equal(6, local);
        var state = new NCell<int>(2);
        var explicitState = ManagedNFunc<int, int>.Create("package.consumer.explicit", state, static (cell, amount) => cell.Value += amount);
        Equal(7, explicitState.Invoke(5));
        Equal(7, state.Value);
        RequireUnmanaged(named);
        RequireUnmanaged(literal);

        original.Dispose();
        Equal(30, callbacks.ManagedRead.Invoke());
        Fails(() => callbacks.Read.Invoke(original));
        restored.Release(roots[0]);
        Fails(() => restoredRead.Invoke(restored));
        Fails(() => decodedRead.Bind(restored, roots[3]));
        Equal(12, restoredManaged.Invoke());
        Equal(13, restoredIncrement.Invoke(restored));
        Equal(13, restoredManaged.Invoke());
        Console.WriteLine($"Package smoke: {checks} checks passed.");
    }

    private static void Equal<T>(T expected, T actual)
    {
        ++checks;
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new InvalidOperationException($"Expected {expected}; got {actual}.");
    }

    private static void Fails(Action action)
    {
        ++checks;
        try { action(); }
        catch (InvalidOperationException) { return; }
        throw new InvalidOperationException("Expected managed validation failure.");
    }

    private static void RequireUnmanaged<T>(T value) where T : unmanaged => ++checks;

    private sealed record SavedGraph(SavedNode[] Nodes, int[] Roots, int[]? RootIds);
    private sealed record SavedNode(int NodeId, byte[] SchemaId, SavedField[] Fields);
    private sealed record SavedField(string Name, NCaptureValueKind Kind, int NodeId, object? Leaf);

    private sealed class JsonLeafCodec : INCaptureLeafCodec
    {
        public int Encoded;
        public int Decoded;
        public object? Encode<T>(T value)
        {
            ++Encoded;
            return JsonSerializer.Serialize(value);
        }
        public T Decode<T>(object? encoded)
        {
            ++Decoded;
            var json = encoded is JsonElement element ? element.GetString() : (string?)encoded;
            return JsonSerializer.Deserialize<T>(json!)!;
        }
    }
}
