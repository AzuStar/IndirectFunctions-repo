using System.Text.Json;
using IndirectFunctions;
using MessagePack;

namespace IndirectFunctions.SerializationExamples;

public static class BasicExamples
{
    public static void Run()
    {
        using var context = new NContext();
        var fn = NFunc<int, int>.Create(context, "tests.identity.double", Double);
        Program.Equal(8, fn.Invoke(context, 4));

        string json = JsonSerializer.Serialize(fn.ToBytes());
        var jsonFn = NFunc<int, int>.FromBytes(JsonSerializer.Deserialize<byte[]>(json)!);
        Program.Equal(8, jsonFn.Invoke(context, 4));

        byte[] packed = MessagePackSerializer.Serialize(fn.ToBytes());
        var packedFn = NFunc<int, int>.FromBytes(MessagePackSerializer.Deserialize<byte[]>(packed));
        Program.Equal(8, packedFn.Invoke(context, 4));

        using var stream = new MemoryStream();
        ProtoBuf.Serializer.Serialize(stream, fn.ToBytes());
        stream.Position = 0;
        var protoFn = NFunc<int, int>.FromBytes(ProtoBuf.Serializer.Deserialize<byte[]>(stream));
        Program.Equal(8, protoFn.Invoke(context, 4));

        int value = 1;
        var next = NFunc<int>.Create(context, "app.counter.next.v1", () => ++value);
        var copy = next;
        Program.Equal(2, copy.Invoke(context));
        Program.Equal(2, value);

        var state = new NCell<int>(10);
        var add = NFunc<int, int>.Create(context, "app.counter.add.v1", state,
            static (cell, amount) => cell.Value += amount);
        Program.Equal(15, add.Invoke(context, 5));
        var retained = add.Bind(context);
        using var otherContext = new NContext();
        var imported = otherContext.Import(retained);
        Program.Equal(16, imported.Invoke(otherContext, 1));
        Program.Equal(17, retained.Invoke(1));
        Console.WriteLine("PASS quick start, raw code bytes in three formats, closure/state sharing");
    }

    private static int Double(int value) => value * 2;
}
