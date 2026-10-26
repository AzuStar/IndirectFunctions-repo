using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;
using IndirectFunctions;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using ProtoBuf;
using ProtoBuf.Meta;

namespace IndirectFunctions.SerializationExamples;

// These adapters persist code identity only. Captured callbacks need CapturePersistence too.
public sealed class IntFunctionJsonConverter : JsonConverter<NFunc<int, int>>
{
    public override NFunc<int, int> Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) =>
        NFunc<int, int>.FromBytes(reader.GetBytesFromBase64());

    public override void Write(Utf8JsonWriter writer, NFunc<int, int> value, JsonSerializerOptions options) =>
        writer.WriteBase64StringValue(value.ToBytes());
}

public sealed class IntFunctionFormatter : IMessagePackFormatter<NFunc<int, int>>
{
    public void Serialize(ref MessagePackWriter writer, NFunc<int, int> value, MessagePackSerializerOptions options) =>
        writer.Write(value.ToBytes());

    public NFunc<int, int> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) =>
        NFunc<int, int>.FromBytes((reader.ReadBytes() ?? throw new MessagePackSerializationException("Missing code ID.")).ToArray());
}

[ProtoContract]
public sealed class IntFunctionSurrogate
{
    [ProtoMember(1)] public byte[] Code { get; set; } = [];

    // protobuf-net converts the default destination value before populating its surrogate.
    public static implicit operator IntFunctionSurrogate(NFunc<int, int> value) => new()
    {
        Code = value.Equals(default(NFunc<int, int>)) ? [] : value.ToBytes()
    };
    public static implicit operator NFunc<int, int>(IntFunctionSurrogate value) => NFunc<int, int>.FromBytes(value.Code);
}

public static class Formats
{
    public static JsonSerializerOptions Json { get; } = new()
    {
        Converters = { new IntFunctionJsonConverter() }
    };

    public static MessagePackSerializerOptions MessagePack { get; } = MessagePackSerializerOptions.Standard.WithResolver(
        CompositeResolver.Create([new IntFunctionFormatter()], [StandardResolver.Instance]));

    public static RuntimeTypeModel Protobuf { get; } = CreateProtobuf();

    private static RuntimeTypeModel CreateProtobuf()
    {
        var model = RuntimeTypeModel.Create();
        model.Add(typeof(NFunc<int, int>), false).SetSurrogate(typeof(IntFunctionSurrogate));
        return model;
    }

    public static byte[] Serialize<T>(string format, T value)
    {
        if (format == "json") return JsonSerializer.SerializeToUtf8Bytes(value, Json);
        if (format == "messagepack") return MessagePackSerializer.Serialize(value, MessagePack);
        if (format != "protobuf") throw new ArgumentException("Unknown format.", nameof(format));
        using var stream = new MemoryStream();
        Protobuf.Serialize(stream, value);
        return stream.ToArray();
    }

    public static T Deserialize<T>(string format, byte[] bytes)
    {
        if (format == "json") return JsonSerializer.Deserialize<T>(bytes, Json)!;
        if (format == "messagepack") return MessagePackSerializer.Deserialize<T>(bytes, MessagePack);
        if (format != "protobuf") throw new ArgumentException("Unknown format.", nameof(format));
        using var stream = new MemoryStream(bytes);
        return Protobuf.Deserialize<T>(stream);
    }
}
