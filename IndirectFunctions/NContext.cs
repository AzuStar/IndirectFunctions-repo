using System.Runtime.CompilerServices;

namespace IndirectFunctions;

/// <summary>Process-local root token. Never serialize this token; export its graph through NContext instead.</summary>
public readonly struct NCaptureRef : IEquatable<NCaptureRef>
{
    internal readonly long Owner;
    internal readonly int Index;
    internal readonly long Generation;
    internal NCaptureRef(long owner, int index, long generation) => (Owner, Index, Generation) = (owner, index, generation);
    public bool IsEmpty => Owner == 0;
    public bool Equals(NCaptureRef other) => Owner == other.Owner && Index == other.Index && Generation == other.Generation;
    public override bool Equals(object? obj) => obj is NCaptureRef other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Owner, Index, Generation);
    public static bool operator ==(NCaptureRef left, NCaptureRef right) => left.Equals(right);
    public static bool operator !=(NCaptureRef left, NCaptureRef right) => !left.Equals(right);
}

/// <summary>Owns capture roots. Reads are allocation-free and lock-free; mutation/export require application coordination.</summary>
public sealed partial class NContext : IDisposable
{
    private sealed class CaptureSlot(long generation, INCaptureState state)
    {
        public readonly long Generation = generation;
        public INCaptureState? State = state;
    }

    private static long _nextOwner;
    private readonly long _owner = NextOwner();
    private readonly object _gate = new();
    private CaptureSlot?[] _slots = [];
    private readonly Stack<int> _freeSlots = [];
    private int _nextSlot;
    private long _nextGeneration;
    private int _disposed;

    private static long NextOwner()
    {
        var owner = Interlocked.Increment(ref _nextOwner);
        if (owner <= 0)
            throw new InvalidOperationException("Capture owner identities exhausted.");
        return owner;
    }

    public bool IsDisposed => Volatile.Read(ref _disposed) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void EnsureUsable()
    {
        if (Volatile.Read(ref _disposed) != 0)
            throw new ObjectDisposedException(nameof(NContext));
    }

    public NCaptureRef ImportCapture(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        lock (_gate)
        {
            EnsureUsable();
            var generation = checked(++_nextGeneration);
            var slots = _slots;
            int index;
            if (_freeSlots.TryPop(out index))
            {
                Volatile.Write(ref slots[index], new(generation, capture));
            }
            else
            {
                index = _nextSlot++;
                if (index == slots.Length)
                {
                    var expanded = new CaptureSlot?[slots.Length == 0 ? 16 : checked(slots.Length * 2)];
                    slots.CopyTo(expanded, 0);
                    expanded[index] = new(generation, capture);
                    Volatile.Write(ref _slots, expanded);
                }
                else
                    Volatile.Write(ref slots[index], new(generation, capture));
            }
            return new(_owner, index, generation);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public INCaptureState ResolveCapture(NCaptureRef capture)
    {
        EnsureUsable();
        if (capture.Owner != _owner)
            throw new InvalidOperationException("Capture token belongs to a different context or is unbound.");
        var slots = Volatile.Read(ref _slots);
        if ((uint)capture.Index >= (uint)slots.Length)
            throw new InvalidOperationException("Capture token has an invalid slot.");
        var slot = Volatile.Read(ref slots[capture.Index]) ?? throw new InvalidOperationException("Capture token has an invalid slot.");
        var state = Volatile.Read(ref slot.State);
        if (slot.Generation != capture.Generation || state is null)
            throw new InvalidOperationException("Capture token is stale or released.");
        return state;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal object? ResolveForInvocation(NCaptureRef capture, NCodeId? schema)
    {
        if (schema is not { } expected)
        {
            EnsureUsable();
            if (!capture.IsEmpty)
                throw new InvalidOperationException("Stateless callback cannot bind captured state.");
            return null;
        }
        var state = ResolveCapture(capture);
        if (state.CaptureSchemaId != expected)
            throw new InvalidOperationException("Capture schema does not match callback.");
        return state;
    }

    internal NCaptureRef ValidateBinding(NCaptureRef capture, NCodeId? schema)
    {
        ResolveForInvocation(capture, schema);
        return capture;
    }

    internal NCaptureRef BindCapture(INCaptureState? capture, NCodeId? schema)
    {
        EnsureUsable();
        if (schema is null)
        {
            if (capture is not null)
                throw new InvalidOperationException("Stateless callback cannot bind captured state.");
            return default;
        }
        if (capture is null || capture.CaptureSchemaId != schema)
            throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        return ImportCapture(capture);
    }

    public void Release(NCaptureRef capture)
    {
        lock (_gate)
        {
            ResolveCapture(capture);
            Volatile.Write(ref _slots[capture.Index]!.State, null);
            _freeSlots.Push(capture.Index);
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;
            foreach (var slot in _slots)
                if (slot is not null)
                    Volatile.Write(ref slot.State, null);
            Volatile.Write(ref _slots, []);
            _freeSlots.Clear();
        }
    }

    /// <summary>Export roots together to preserve shared cells. Captures must remain quiescent during export.</summary>
    public NCaptureGraph ExportCaptures(params NCaptureRef[] roots) => ExportCaptures((IReadOnlyList<NCaptureRef>)roots);

    /// <summary>
    /// Export generated capture nodes and encode typed leaves with an optional caller-owned codec session.
    /// Export roots together to preserve shared cells. Captures must remain quiescent during export.
    /// </summary>
    public NCaptureGraph ExportCaptures(IReadOnlyList<NCaptureRef> roots, INCaptureLeafCodec? codec = null)
    {
        ArgumentNullException.ThrowIfNull(roots);
        EnsureUsable();
        var identities = new Dictionary<INCaptureState, int>(ReferenceEqualityComparer.Instance);
        var nodes = new List<NCaptureNode>();
        var pending = new Queue<(INCaptureState State, List<NCaptureField> Fields)>();
        int Identify(INCaptureState state)
        {
            if (identities.TryGetValue(state, out var known))
                return known;
            var id = nodes.Count + 1;
            identities.Add(state, id);
            var fields = new List<NCaptureField>();
            nodes.Add(new(id, state.CaptureSchemaId, fields));
            pending.Enqueue((state, fields));
            return id;
        }
        var mapped = new int[roots.Count];
        var rootIds = new int[roots.Count];
        var rootIdentities = new Dictionary<NCaptureRef, int>();
        for (var i = 0; i < roots.Count; i++)
        {
            mapped[i] = Identify(ResolveCapture(roots[i]));
            if (!rootIdentities.TryGetValue(roots[i], out var rootId))
                rootIdentities.Add(roots[i], rootId = rootIdentities.Count + 1);
            rootIds[i] = rootId;
        }
        while (pending.TryDequeue(out var node))
            node.State.Write(new NCaptureWriter((name, value, isNodeReference) => node.Fields.Add(new(name,
                isNodeReference ? NCaptureValue.FromReference(Identify((INCaptureState)value!)) : NCaptureValue.FromLeaf(value))), codec));
        return new(nodes, mapped) { RootIds = rootIds };
    }

    /// <summary>
    /// Materialize every node first, then connect references and decode typed leaves with an optional codec session.
    /// Return fresh tokens in exported root order. Caller owns ordinary-object and external-reference identity maps.
    /// </summary>
    public NCaptureRef[] ImportCaptures(NCaptureGraph graph, INCaptureLeafCodec? codec = null)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(graph.Nodes);
        ArgumentNullException.ThrowIfNull(graph.Roots);
        EnsureUsable();
        var states = new Dictionary<int, INCaptureState>();
        var nodeIdentities = new HashSet<INCaptureState>(ReferenceEqualityComparer.Instance);
        foreach (var node in graph.Nodes)
        {
            if (node is null || node.NodeId <= 0 || states.ContainsKey(node.NodeId))
                throw new InvalidOperationException("Capture graph node identities must be unique positive integers.");
            var state = NGenerated.CreateCapture(node.SchemaId);
            if (!nodeIdentities.Add(state))
                throw new InvalidOperationException("Capture schema factories must return distinct fresh nodes.");
            states.Add(node.NodeId, state);
        }
        foreach (var root in graph.Roots)
            if (!states.ContainsKey(root))
                throw new InvalidOperationException("Capture graph root references a missing node.");
        if (graph.RootIds is { } rootIds)
        {
            if (rootIds.Count != graph.Roots.Count)
                throw new InvalidOperationException("Capture graph root identities must match root mapping count.");
            var rootNodes = new Dictionary<int, int>();
            for (var i = 0; i < rootIds.Count; i++)
            {
                if (rootIds[i] <= 0)
                    throw new InvalidOperationException("Capture graph root identities must be positive integers.");
                if (rootNodes.TryGetValue(rootIds[i], out var known) && known != graph.Roots[i])
                    throw new InvalidOperationException("Capture graph root aliases must reference the same node.");
                rootNodes[rootIds[i]] = graph.Roots[i];
            }
        }
        foreach (var node in graph.Nodes)
        {
            ArgumentNullException.ThrowIfNull(node.Fields);
            var fields = new Dictionary<string, NCaptureReadValue>(StringComparer.Ordinal);
            foreach (var field in node.Fields)
            {
                if (field is null || string.IsNullOrEmpty(field.Name))
                    throw new InvalidOperationException("Capture graph field name is missing.");
                object? value;
                if (field.Value.Kind == NCaptureValueKind.Leaf)
                    value = field.Value.Leaf;
                else if (field.Value.Kind == NCaptureValueKind.NodeReference && states.TryGetValue(field.Value.NodeId, out var reference))
                    value = reference;
                else
                    throw new InvalidOperationException("Capture graph field references a missing node or invalid value kind.");
                if (!fields.TryAdd(field.Name, new(value, field.Value.Kind == NCaptureValueKind.NodeReference)))
                    throw new InvalidOperationException($"Capture graph contains duplicate field '{field.Name}'.");
            }
            states[node.NodeId].Read(new NCaptureReader(fields, codec));
        }
        var result = new NCaptureRef[graph.Roots.Count];
        lock (_gate)
        {
            EnsureUsable();
            var importedRoots = new Dictionary<int, NCaptureRef>();
            for (var i = 0; i < result.Length; i++)
            {
                if (graph.RootIds is { } boundRootIds && importedRoots.TryGetValue(boundRootIds[i], out var imported))
                    result[i] = imported;
                else
                {
                    result[i] = ImportCapture(states[graph.Roots[i]]);
                    if (graph.RootIds is { } identities)
                        importedRoots.Add(identities[i], result[i]);
                }
            }
        }
        return result;
    }
}
