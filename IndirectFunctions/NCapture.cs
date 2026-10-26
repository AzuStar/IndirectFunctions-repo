namespace IndirectFunctions;

/// <summary>Serializer-neutral generated capture node. Read runs after every graph node exists.</summary>
public interface INCaptureState
{
    NCodeId CaptureSchemaId { get; }
    void Write(NCaptureWriter writer);
    void Read(NCaptureReader reader);
}

/// <summary>Versioned capture-node schema registered without reflection-based materialization.</summary>
public sealed class NCaptureSchema
{
    /// <param name="stableKey">Stable schema key; change its version when fields or their types change.</param>
    /// <param name="create">Factory returning a fresh node on every call. Read populates fields after all nodes exist.</param>
    public NCaptureSchema(string stableKey, Func<INCaptureState> create)
    {
        ArgumentException.ThrowIfNullOrEmpty(stableKey);
        ArgumentNullException.ThrowIfNull(create);
        Key = stableKey;
        Id = NCodeId.FromKey(stableKey);
        Factory = create;
    }

    public string Key { get; }
    public NCodeId Id { get; }
    internal Func<INCaptureState> Factory { get; }
}

/// <summary>A shared mutable cell. Capture schemas reference the cell, preserving aliasing.</summary>
public sealed class NCell<T> : INCaptureState
{
    private static readonly NCodeId Id = RegisterSchema();
    public T Value = default!;
    public NCell() { }
    public NCell(T value) => Value = value;
    public static NCodeId SchemaId => Id;
    public NCodeId CaptureSchemaId => Id;

    private static NCodeId RegisterSchema()
    {
        var key = "IndirectFunctions.NCell.v1:" + TypeIdentity(typeof(T));
        var schema = new NCaptureSchema(key, static () => new NCell<T>());
        NGenerated.RegisterCaptureSchema(schema);
        return schema.Id;
    }

    private static string TypeIdentity(Type type)
    {
        if (type.IsArray)
            return TypeIdentity(type.GetElementType()!) + "[" + new string(',', type.GetArrayRank() - 1) + "]";
        var name = type.IsGenericType ? type.GetGenericTypeDefinition().FullName : type.FullName;
        var identity = type.Assembly.GetName().Name + ":" + name;
        return type.IsGenericType ? identity + "[" + string.Join(",", type.GetGenericArguments().Select(TypeIdentity)) + "]" : identity;
    }

    public void Write(NCaptureWriter writer) => writer.Field(nameof(Value), Value);
    public void Read(NCaptureReader reader) => Value = reader.Field<T>(nameof(Value));
}

/// <summary>
/// Converts typed capture leaves to and from an application's persistence representation.
/// Caller owns the codec session and any ordinary-object or external-reference identity maps.
/// Capture-node references bypass this codec; null leaves are included.
/// </summary>
public interface INCaptureLeafCodec
{
    object? Encode<T>(T value);
    T Decode<T>(object? encoded);
}

public sealed class NCaptureWriter
{
    private readonly Action<string, object?, bool> _write;
    private readonly INCaptureLeafCodec? _codec;
    private readonly HashSet<string> _names = new(StringComparer.Ordinal);
    internal NCaptureWriter(Action<string, object?, bool> write, INCaptureLeafCodec? codec) => (_write, _codec) = (write, codec);

    public void Field<T>(string name, T value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (!_names.Add(name))
            throw new InvalidOperationException($"Capture writer contains duplicate field '{name}'.");
        if (value is INCaptureState state)
            _write(name, state, true);
        else
            _write(name, _codec is null ? value : _codec.Encode(value), false);
    }
}

internal readonly record struct NCaptureReadValue(object? Value, bool IsNodeReference);

public sealed class NCaptureReader
{
    private readonly IReadOnlyDictionary<string, NCaptureReadValue> _fields;
    private readonly INCaptureLeafCodec? _codec;
    internal NCaptureReader(IReadOnlyDictionary<string, NCaptureReadValue> fields, INCaptureLeafCodec? codec) => (_fields, _codec) = (fields, codec);

    public T Field<T>(string name)
    {
        if (!_fields.TryGetValue(name, out var field))
            throw new InvalidOperationException($"Capture field '{name}' is missing.");
        if (!field.IsNodeReference && _codec is not null)
            return _codec.Decode<T>(field.Value);
        var value = field.Value;
        if (value is T typed)
            return typed;
        if (value is null && default(T) is null)
            return default!;
        throw new InvalidOperationException($"Capture field '{name}' does not match expected type '{typeof(T)}'.");
    }
}

public enum NCaptureValueKind { Leaf, NodeReference }

/// <summary>Leaves are handed to the application's serializer; references identify generated graph nodes.</summary>
public readonly record struct NCaptureValue(NCaptureValueKind Kind, object? Leaf, int NodeId)
{
    public static NCaptureValue FromLeaf(object? value) => new(NCaptureValueKind.Leaf, value, 0);
    public static NCaptureValue FromReference(int nodeId) => new(NCaptureValueKind.NodeReference, null, nodeId);
}

public sealed record NCaptureField(string Name, NCaptureValue Value);
public sealed record NCaptureNode(int NodeId, NCodeId SchemaId, IReadOnlyList<NCaptureField> Fields);
public sealed record NCaptureGraph(IReadOnlyList<NCaptureNode> Nodes, IReadOnlyList<int> Roots)
{
    /// <summary>
    /// Optional parallel root identities. Equal identities denote aliases of the same context root;
    /// distinct identities can reference one shared capture node while retaining independent release lifetimes.
    /// Persistence backends must preserve this mapping. Legacy graphs omitting it import each root independently.
    /// </summary>
    public IReadOnlyList<int>? RootIds { get; init; }
}
