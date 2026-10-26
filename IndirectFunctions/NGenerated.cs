using System.Runtime.CompilerServices;

namespace IndirectFunctions;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class NGeneratedBridgeAttribute(Type bridgeType) : Attribute
{
    public Type BridgeType { get; } = bridgeType;
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class NGeneratedCatalogAttribute(string fullTypeName) : Attribute
{
    public string FullTypeName { get; } = fullTypeName;
}

/// <summary>Registration entry points used by generated catalogs. Registration runs outside invocation.</summary>
public static class NGenerated
{
    private sealed record CodeRegistration(string Key, Type Signature, nint Thunk, NCodeId? Schema);
    private static readonly object Gate = new();
    private static readonly Dictionary<NCodeId, CodeRegistration> Codes = [];
    private static readonly Dictionary<NCodeId, NCaptureSchema> Schemas = [];
    [ThreadStatic] private static RegistrationBatch? _batch;

    /// <summary>Publish each affected signature table once after generated catalog registration completes.</summary>
    public static IDisposable BeginRegistrationBatch() => _batch = new RegistrationBatch(_batch);

    private sealed class RegistrationBatch(RegistrationBatch? parent) : IDisposable
    {
        private readonly RegistrationBatch? _parent = parent;
        private readonly int _thread = Environment.CurrentManagedThreadId;
        internal readonly Dictionary<Type, Action> Publications = [];
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;
            if (Environment.CurrentManagedThreadId != _thread || _batch != this)
                throw new InvalidOperationException("Registration batches must be disposed in nesting order on their creating thread.");
            _disposed = true;
            _batch = _parent;
            if (_parent is not null)
                foreach (var publication in Publications)
                    _parent.Publications.TryAdd(publication.Key, publication.Value);
            else
                foreach (var publish in Publications.Values)
                    publish();
        }
    }

    internal static bool DeferPublication(Type signature, Action publish)
    {
        if (_batch is not { } batch)
            return false;
        batch.Publications.TryAdd(signature, publish);
        return true;
    }

    internal static NCodeId RegisterCode(string key, Type signature, nint thunk, NCodeId? schema)
    {
        if (thunk == 0)
            throw new ArgumentException("Generated callback pointer cannot be null.", nameof(thunk));
        var id = NCodeId.FromKey(key);
        lock (Gate)
        {
            if (Codes.TryGetValue(id, out var existing))
            {
                if (existing.Key != key)
                    throw new InvalidOperationException($"Code identity hash collision between '{existing.Key}' and '{key}'.");
                if (existing.Signature != signature || existing.Thunk != thunk || existing.Schema != schema)
                    throw new InvalidOperationException($"Callback key '{key}' has conflicting registrations or signatures.");
                return id;
            }
            Codes.Add(id, new(key, signature, thunk, schema));
        }
        return id;
    }

    public static void RegisterCaptureSchema(NCaptureSchema schema)
    {
        ArgumentNullException.ThrowIfNull(schema);
        lock (Gate)
        {
            if (Schemas.TryGetValue(schema.Id, out var existing))
            {
                if (existing.Key != schema.Key)
                    throw new InvalidOperationException($"Capture schema identity hash collision between '{existing.Key}' and '{schema.Key}'.");
                if (existing.Factory != schema.Factory)
                    throw new InvalidOperationException($"Capture schema '{schema.Key}' has conflicting factories.");
                return;
            }
            Schemas.Add(schema.Id, schema);
        }
    }

    internal static INCaptureState CreateCapture(NCodeId schemaId)
    {
        NCaptureSchema schema;
        lock (Gate)
        {
            if (!Schemas.TryGetValue(schemaId, out schema!))
                throw new InvalidOperationException($"Capture schema '{schemaId}' is not registered.");
        }
        var capture = schema.Factory() ?? throw new InvalidOperationException("Capture schema factory returned null.");
        if (capture.CaptureSchemaId != schemaId)
            throw new InvalidOperationException("Capture schema factory returned an incompatible state.");
        return capture;
    }

    internal static Exception MissingTransformation() => new InvalidOperationException(
        "Callback creation marker was not transformed. Install IndirectFunctions package build assets and perform a real build.");
}

internal readonly record struct NCodeEntry(NCodeId Id, nint Thunk, NCodeId? SchemaId);

internal static class NCodeTable<TSignature>
{
    private static readonly object Gate = new();
    private static readonly Dictionary<NCodeId, int> Slots = [];
    private static readonly List<NCodeEntry> Entries = [default];
    private static NCodeEntry[] _entries = new NCodeEntry[1];

    public static void Register(string key, nint thunk, NCodeId? schema)
    {
        lock (Gate)
        {
            var id = NGenerated.RegisterCode(key, typeof(TSignature), thunk, schema);
            if (Slots.ContainsKey(id))
                return;
            Slots.Add(id, Entries.Count);
            Entries.Add(new(id, thunk, schema));
            if (!NGenerated.DeferPublication(typeof(TSignature), Publish))
                Publish();
        }
    }

    private static void Publish()
    {
        lock (Gate)
            if (_entries.Length != Entries.Count)
                Volatile.Write(ref _entries, Entries.ToArray());
    }

    public static int Resolve(NCodeId id)
    {
        lock (Gate)
        {
            // Cold binding during an open registration batch must still observe newly registered slots.
            Publish();
            return Slots.TryGetValue(id, out var slot) ? slot : throw new InvalidOperationException(
                $"Callback '{id}' is missing or does not match requested signature '{typeof(TSignature)}'.");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly NCodeEntry Get(int slot)
    {
        var entries = Volatile.Read(ref _entries);
        if ((uint)slot >= (uint)entries.Length || slot == 0)
            throw new InvalidOperationException("Callback holder is uninitialized or its runtime slot is invalid.");
        return ref entries[slot];
    }
}

internal readonly struct NHandle(int slot, NCaptureRef capture)
{
    public readonly int Slot = slot;
    public readonly NCaptureRef Capture = capture;
}
