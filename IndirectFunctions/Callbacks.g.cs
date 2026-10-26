// Generated mechanically: standard Action/Func arities 0 through 16.
#nullable enable
namespace IndirectFunctions;


public readonly struct NFunc<TResult>
{
    private readonly NHandle _handle;
    internal NFunc(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NFunc<TResult>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NFunc<TResult>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NFunc<TResult> Create(NContext context, string key, Func<TResult> callback) => throw NGenerated.MissingTransformation();
    public static NFunc<TResult> Create<TState>(NContext context, string key, TState state, Func<TState, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, TResult> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NFunc<TResult>>.Register(key, (nint)thunk, captureSchemaId);
    public static NFunc<TResult> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NFunc<TResult>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NFunc<TResult>>.Get(slot).SchemaId)));
    }
    public unsafe TResult Invoke(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NFunc<TResult>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        return ((delegate* managed<object?, TResult>)entry.Thunk)(capture);
    }
    public NFunc<TResult> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NFunc<TResult>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNFunc<TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NFunc<TResult>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NFunc<TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NFunc<TResult> FromCodeId(NCodeId id) => new(new(NCodeTable<NFunc<TResult>>.Resolve(id), default));
}


public readonly struct ManagedNFunc<TResult>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNFunc(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NFunc<TResult>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NFunc<TResult>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNFunc<TResult> Create(string key, Func<TResult> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<TResult> Create<TState>(string key, TState state, Func<TState, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<TResult> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NFunc<TResult>>.Resolve(id);
        ValidateCapture(NCodeTable<NFunc<TResult>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe TResult Invoke()
    {
        ref readonly var entry = ref NCodeTable<NFunc<TResult>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        return ((delegate* managed<object?, TResult>)entry.Thunk)(capture);
    }
    public NFunc<TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NFunc<TResult>>.Get(_slot).SchemaId)));
    }
    public ManagedNFunc<TResult> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NFunc<TResult>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNFunc<TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNFunc<TResult> FromCodeId(NCodeId id) => new(NCodeTable<NFunc<TResult>>.Resolve(id), null);
}


public readonly struct NFunc<T1, TResult>
{
    private readonly NHandle _handle;
    internal NFunc(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NFunc<T1, TResult>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NFunc<T1, TResult>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NFunc<T1, TResult> Create(NContext context, string key, Func<T1, TResult> callback) => throw NGenerated.MissingTransformation();
    public static NFunc<T1, TResult> Create<TState>(NContext context, string key, TState state, Func<TState, T1, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, TResult> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NFunc<T1, TResult>>.Register(key, (nint)thunk, captureSchemaId);
    public static NFunc<T1, TResult> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NFunc<T1, TResult>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NFunc<T1, TResult>>.Get(slot).SchemaId)));
    }
    public unsafe TResult Invoke(NContext context, T1 arg1)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NFunc<T1, TResult>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        return ((delegate* managed<object?, T1, TResult>)entry.Thunk)(capture, arg1);
    }
    public NFunc<T1, TResult> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NFunc<T1, TResult>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNFunc<T1, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NFunc<T1, TResult>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NFunc<T1, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NFunc<T1, TResult> FromCodeId(NCodeId id) => new(new(NCodeTable<NFunc<T1, TResult>>.Resolve(id), default));
}


public readonly struct ManagedNFunc<T1, TResult>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNFunc(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NFunc<T1, TResult>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NFunc<T1, TResult>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNFunc<T1, TResult> Create(string key, Func<T1, TResult> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, TResult> Create<TState>(string key, TState state, Func<TState, T1, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, TResult> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NFunc<T1, TResult>>.Resolve(id);
        ValidateCapture(NCodeTable<NFunc<T1, TResult>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe TResult Invoke(T1 arg1)
    {
        ref readonly var entry = ref NCodeTable<NFunc<T1, TResult>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        return ((delegate* managed<object?, T1, TResult>)entry.Thunk)(capture, arg1);
    }
    public NFunc<T1, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NFunc<T1, TResult>>.Get(_slot).SchemaId)));
    }
    public ManagedNFunc<T1, TResult> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NFunc<T1, TResult>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNFunc<T1, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNFunc<T1, TResult> FromCodeId(NCodeId id) => new(NCodeTable<NFunc<T1, TResult>>.Resolve(id), null);
}


public readonly struct NFunc<T1, T2, TResult>
{
    private readonly NHandle _handle;
    internal NFunc(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, TResult>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, TResult>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NFunc<T1, T2, TResult> Create(NContext context, string key, Func<T1, T2, TResult> callback) => throw NGenerated.MissingTransformation();
    public static NFunc<T1, T2, TResult> Create<TState>(NContext context, string key, TState state, Func<TState, T1, T2, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, TResult> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NFunc<T1, T2, TResult>>.Register(key, (nint)thunk, captureSchemaId);
    public static NFunc<T1, T2, TResult> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NFunc<T1, T2, TResult>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NFunc<T1, T2, TResult>>.Get(slot).SchemaId)));
    }
    public unsafe TResult Invoke(NContext context, T1 arg1, T2 arg2)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, TResult>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        return ((delegate* managed<object?, T1, T2, TResult>)entry.Thunk)(capture, arg1, arg2);
    }
    public NFunc<T1, T2, TResult> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NFunc<T1, T2, TResult>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NFunc<T1, T2, TResult>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NFunc<T1, T2, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NFunc<T1, T2, TResult> FromCodeId(NCodeId id) => new(new(NCodeTable<NFunc<T1, T2, TResult>>.Resolve(id), default));
}


public readonly struct ManagedNFunc<T1, T2, TResult>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNFunc(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, TResult>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, TResult>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNFunc<T1, T2, TResult> Create(string key, Func<T1, T2, TResult> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, TResult> Create<TState>(string key, TState state, Func<TState, T1, T2, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, TResult> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NFunc<T1, T2, TResult>>.Resolve(id);
        ValidateCapture(NCodeTable<NFunc<T1, T2, TResult>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe TResult Invoke(T1 arg1, T2 arg2)
    {
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, TResult>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        return ((delegate* managed<object?, T1, T2, TResult>)entry.Thunk)(capture, arg1, arg2);
    }
    public NFunc<T1, T2, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NFunc<T1, T2, TResult>>.Get(_slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, TResult> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NFunc<T1, T2, TResult>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNFunc<T1, T2, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNFunc<T1, T2, TResult> FromCodeId(NCodeId id) => new(NCodeTable<NFunc<T1, T2, TResult>>.Resolve(id), null);
}


public readonly struct NFunc<T1, T2, T3, TResult>
{
    private readonly NHandle _handle;
    internal NFunc(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, TResult>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, TResult>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NFunc<T1, T2, T3, TResult> Create(NContext context, string key, Func<T1, T2, T3, TResult> callback) => throw NGenerated.MissingTransformation();
    public static NFunc<T1, T2, T3, TResult> Create<TState>(NContext context, string key, TState state, Func<TState, T1, T2, T3, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, TResult> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NFunc<T1, T2, T3, TResult>>.Register(key, (nint)thunk, captureSchemaId);
    public static NFunc<T1, T2, T3, TResult> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NFunc<T1, T2, T3, TResult>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NFunc<T1, T2, T3, TResult>>.Get(slot).SchemaId)));
    }
    public unsafe TResult Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, TResult>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        return ((delegate* managed<object?, T1, T2, T3, TResult>)entry.Thunk)(capture, arg1, arg2, arg3);
    }
    public NFunc<T1, T2, T3, TResult> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NFunc<T1, T2, T3, TResult>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NFunc<T1, T2, T3, TResult>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NFunc<T1, T2, T3, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NFunc<T1, T2, T3, TResult> FromCodeId(NCodeId id) => new(new(NCodeTable<NFunc<T1, T2, T3, TResult>>.Resolve(id), default));
}


public readonly struct ManagedNFunc<T1, T2, T3, TResult>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNFunc(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, TResult>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, TResult>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNFunc<T1, T2, T3, TResult> Create(string key, Func<T1, T2, T3, TResult> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, TResult> Create<TState>(string key, TState state, Func<TState, T1, T2, T3, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, TResult> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NFunc<T1, T2, T3, TResult>>.Resolve(id);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, TResult>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe TResult Invoke(T1 arg1, T2 arg2, T3 arg3)
    {
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, TResult>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        return ((delegate* managed<object?, T1, T2, T3, TResult>)entry.Thunk)(capture, arg1, arg2, arg3);
    }
    public NFunc<T1, T2, T3, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NFunc<T1, T2, T3, TResult>>.Get(_slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, TResult> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, TResult>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNFunc<T1, T2, T3, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNFunc<T1, T2, T3, TResult> FromCodeId(NCodeId id) => new(NCodeTable<NFunc<T1, T2, T3, TResult>>.Resolve(id), null);
}


public readonly struct NFunc<T1, T2, T3, T4, TResult>
{
    private readonly NHandle _handle;
    internal NFunc(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, TResult>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, TResult>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NFunc<T1, T2, T3, T4, TResult> Create(NContext context, string key, Func<T1, T2, T3, T4, TResult> callback) => throw NGenerated.MissingTransformation();
    public static NFunc<T1, T2, T3, T4, TResult> Create<TState>(NContext context, string key, TState state, Func<TState, T1, T2, T3, T4, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, TResult> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NFunc<T1, T2, T3, T4, TResult>>.Register(key, (nint)thunk, captureSchemaId);
    public static NFunc<T1, T2, T3, T4, TResult> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, TResult>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NFunc<T1, T2, T3, T4, TResult>>.Get(slot).SchemaId)));
    }
    public unsafe TResult Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, TResult>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        return ((delegate* managed<object?, T1, T2, T3, T4, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4);
    }
    public NFunc<T1, T2, T3, T4, TResult> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NFunc<T1, T2, T3, T4, TResult>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NFunc<T1, T2, T3, T4, TResult>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NFunc<T1, T2, T3, T4, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NFunc<T1, T2, T3, T4, TResult> FromCodeId(NCodeId id) => new(new(NCodeTable<NFunc<T1, T2, T3, T4, TResult>>.Resolve(id), default));
}


public readonly struct ManagedNFunc<T1, T2, T3, T4, TResult>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNFunc(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, TResult>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, TResult>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNFunc<T1, T2, T3, T4, TResult> Create(string key, Func<T1, T2, T3, T4, TResult> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, TResult> Create<TState>(string key, TState state, Func<TState, T1, T2, T3, T4, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, TResult> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, TResult>>.Resolve(id);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, TResult>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe TResult Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, TResult>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        return ((delegate* managed<object?, T1, T2, T3, T4, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4);
    }
    public NFunc<T1, T2, T3, T4, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NFunc<T1, T2, T3, T4, TResult>>.Get(_slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, TResult> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, TResult>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNFunc<T1, T2, T3, T4, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNFunc<T1, T2, T3, T4, TResult> FromCodeId(NCodeId id) => new(NCodeTable<NFunc<T1, T2, T3, T4, TResult>>.Resolve(id), null);
}


public readonly struct NFunc<T1, T2, T3, T4, T5, TResult>
{
    private readonly NHandle _handle;
    internal NFunc(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, TResult>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, TResult>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NFunc<T1, T2, T3, T4, T5, TResult> Create(NContext context, string key, Func<T1, T2, T3, T4, T5, TResult> callback) => throw NGenerated.MissingTransformation();
    public static NFunc<T1, T2, T3, T4, T5, TResult> Create<TState>(NContext context, string key, TState state, Func<TState, T1, T2, T3, T4, T5, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, TResult> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NFunc<T1, T2, T3, T4, T5, TResult>>.Register(key, (nint)thunk, captureSchemaId);
    public static NFunc<T1, T2, T3, T4, T5, TResult> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, TResult>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, TResult>>.Get(slot).SchemaId)));
    }
    public unsafe TResult Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, TResult>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5);
    }
    public NFunc<T1, T2, T3, T4, T5, TResult> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NFunc<T1, T2, T3, T4, T5, TResult>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, TResult>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NFunc<T1, T2, T3, T4, T5, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NFunc<T1, T2, T3, T4, T5, TResult> FromCodeId(NCodeId id) => new(new(NCodeTable<NFunc<T1, T2, T3, T4, T5, TResult>>.Resolve(id), default));
}


public readonly struct ManagedNFunc<T1, T2, T3, T4, T5, TResult>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNFunc(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, TResult>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, TResult>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNFunc<T1, T2, T3, T4, T5, TResult> Create(string key, Func<T1, T2, T3, T4, T5, TResult> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, TResult> Create<TState>(string key, TState state, Func<TState, T1, T2, T3, T4, T5, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, TResult> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, TResult>>.Resolve(id);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, TResult>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe TResult Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, TResult>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5);
    }
    public NFunc<T1, T2, T3, T4, T5, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, TResult>>.Get(_slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, TResult> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, TResult>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNFunc<T1, T2, T3, T4, T5, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNFunc<T1, T2, T3, T4, T5, TResult> FromCodeId(NCodeId id) => new(NCodeTable<NFunc<T1, T2, T3, T4, T5, TResult>>.Resolve(id), null);
}


public readonly struct NFunc<T1, T2, T3, T4, T5, T6, TResult>
{
    private readonly NHandle _handle;
    internal NFunc(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, TResult>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, TResult>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NFunc<T1, T2, T3, T4, T5, T6, TResult> Create(NContext context, string key, Func<T1, T2, T3, T4, T5, T6, TResult> callback) => throw NGenerated.MissingTransformation();
    public static NFunc<T1, T2, T3, T4, T5, T6, TResult> Create<TState>(NContext context, string key, TState state, Func<TState, T1, T2, T3, T4, T5, T6, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, TResult> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, TResult>>.Register(key, (nint)thunk, captureSchemaId);
    public static NFunc<T1, T2, T3, T4, T5, T6, TResult> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, TResult>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, TResult>>.Get(slot).SchemaId)));
    }
    public unsafe TResult Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, TResult>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, TResult> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, TResult>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, TResult>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NFunc<T1, T2, T3, T4, T5, T6, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NFunc<T1, T2, T3, T4, T5, T6, TResult> FromCodeId(NCodeId id) => new(new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, TResult>>.Resolve(id), default));
}


public readonly struct ManagedNFunc<T1, T2, T3, T4, T5, T6, TResult>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNFunc(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, TResult>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, TResult>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, TResult> Create(string key, Func<T1, T2, T3, T4, T5, T6, TResult> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, TResult> Create<TState>(string key, TState state, Func<TState, T1, T2, T3, T4, T5, T6, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, TResult> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, TResult>>.Resolve(id);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, TResult>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe TResult Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, TResult>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, TResult>>.Get(_slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, TResult> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, TResult>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, TResult> FromCodeId(NCodeId id) => new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, TResult>>.Resolve(id), null);
}


public readonly struct NFunc<T1, T2, T3, T4, T5, T6, T7, TResult>
{
    private readonly NHandle _handle;
    internal NFunc(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, TResult>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, TResult>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, TResult> Create(NContext context, string key, Func<T1, T2, T3, T4, T5, T6, T7, TResult> callback) => throw NGenerated.MissingTransformation();
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, TResult> Create<TState>(NContext context, string key, TState state, Func<TState, T1, T2, T3, T4, T5, T6, T7, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, TResult> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, TResult>>.Register(key, (nint)thunk, captureSchemaId);
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, TResult> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, TResult>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, TResult>>.Get(slot).SchemaId)));
    }
    public unsafe TResult Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, TResult>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, T7, TResult> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, TResult>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, TResult>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, TResult> FromCodeId(NCodeId id) => new(new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, TResult>>.Resolve(id), default));
}


public readonly struct ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, TResult>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNFunc(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, TResult>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, TResult>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, TResult> Create(string key, Func<T1, T2, T3, T4, T5, T6, T7, TResult> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, TResult> Create<TState>(string key, TState state, Func<TState, T1, T2, T3, T4, T5, T6, T7, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, TResult> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, TResult>>.Resolve(id);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, TResult>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe TResult Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, TResult>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, T7, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, TResult>>.Get(_slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, TResult> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, TResult>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, TResult> FromCodeId(NCodeId id) => new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, TResult>>.Resolve(id), null);
}


public readonly struct NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>
{
    private readonly NHandle _handle;
    internal NFunc(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> Create(NContext context, string key, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> callback) => throw NGenerated.MissingTransformation();
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> Create<TState>(NContext context, string key, TState state, Func<TState, T1, T2, T3, T4, T5, T6, T7, T8, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, TResult> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>.Register(key, (nint)thunk, captureSchemaId);
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>.Get(slot).SchemaId)));
    }
    public unsafe TResult Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> FromCodeId(NCodeId id) => new(new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>.Resolve(id), default));
}


public readonly struct ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNFunc(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> Create(string key, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> Create<TState>(string key, TState state, Func<TState, T1, T2, T3, T4, T5, T6, T7, T8, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>.Resolve(id);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe TResult Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>.Get(_slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> FromCodeId(NCodeId id) => new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>.Resolve(id), null);
}


public readonly struct NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>
{
    private readonly NHandle _handle;
    internal NFunc(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> Create(NContext context, string key, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> callback) => throw NGenerated.MissingTransformation();
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> Create<TState>(NContext context, string key, TState state, Func<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>.Register(key, (nint)thunk, captureSchemaId);
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>.Get(slot).SchemaId)));
    }
    public unsafe TResult Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> FromCodeId(NCodeId id) => new(new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>.Resolve(id), default));
}


public readonly struct ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNFunc(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> Create(string key, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> Create<TState>(string key, TState state, Func<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>.Resolve(id);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe TResult Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
    {
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>.Get(_slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> FromCodeId(NCodeId id) => new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>.Resolve(id), null);
}


public readonly struct NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>
{
    private readonly NHandle _handle;
    internal NFunc(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> Create(NContext context, string key, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> callback) => throw NGenerated.MissingTransformation();
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> Create<TState>(NContext context, string key, TState state, Func<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>.Register(key, (nint)thunk, captureSchemaId);
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>.Get(slot).SchemaId)));
    }
    public unsafe TResult Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> FromCodeId(NCodeId id) => new(new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>.Resolve(id), default));
}


public readonly struct ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNFunc(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> Create(string key, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> Create<TState>(string key, TState state, Func<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>.Resolve(id);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe TResult Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
    {
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>.Get(_slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> FromCodeId(NCodeId id) => new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>.Resolve(id), null);
}


public readonly struct NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>
{
    private readonly NHandle _handle;
    internal NFunc(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> Create(NContext context, string key, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> callback) => throw NGenerated.MissingTransformation();
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> Create<TState>(NContext context, string key, TState state, Func<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>>.Register(key, (nint)thunk, captureSchemaId);
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>>.Get(slot).SchemaId)));
    }
    public unsafe TResult Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> FromCodeId(NCodeId id) => new(new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>>.Resolve(id), default));
}


public readonly struct ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNFunc(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> Create(string key, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> Create<TState>(string key, TState state, Func<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>>.Resolve(id);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe TResult Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
    {
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>>.Get(_slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> FromCodeId(NCodeId id) => new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>>.Resolve(id), null);
}


public readonly struct NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>
{
    private readonly NHandle _handle;
    internal NFunc(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> Create(NContext context, string key, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> callback) => throw NGenerated.MissingTransformation();
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> Create<TState>(NContext context, string key, TState state, Func<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>.Register(key, (nint)thunk, captureSchemaId);
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>.Get(slot).SchemaId)));
    }
    public unsafe TResult Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> FromCodeId(NCodeId id) => new(new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>.Resolve(id), default));
}


public readonly struct ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNFunc(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> Create(string key, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> Create<TState>(string key, TState state, Func<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>.Resolve(id);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe TResult Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
    {
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>.Get(_slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> FromCodeId(NCodeId id) => new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>.Resolve(id), null);
}


public readonly struct NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>
{
    private readonly NHandle _handle;
    internal NFunc(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> Create(NContext context, string key, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> callback) => throw NGenerated.MissingTransformation();
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> Create<TState>(NContext context, string key, TState state, Func<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>>.Register(key, (nint)thunk, captureSchemaId);
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>>.Get(slot).SchemaId)));
    }
    public unsafe TResult Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> FromCodeId(NCodeId id) => new(new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>>.Resolve(id), default));
}


public readonly struct ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNFunc(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> Create(string key, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> Create<TState>(string key, TState state, Func<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>>.Resolve(id);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe TResult Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
    {
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>>.Get(_slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> FromCodeId(NCodeId id) => new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>>.Resolve(id), null);
}


public readonly struct NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>
{
    private readonly NHandle _handle;
    internal NFunc(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> Create(NContext context, string key, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> callback) => throw NGenerated.MissingTransformation();
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> Create<TState>(NContext context, string key, TState state, Func<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>>.Register(key, (nint)thunk, captureSchemaId);
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>>.Get(slot).SchemaId)));
    }
    public unsafe TResult Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> FromCodeId(NCodeId id) => new(new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>>.Resolve(id), default));
}


public readonly struct ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNFunc(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> Create(string key, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> Create<TState>(string key, TState state, Func<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>>.Resolve(id);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe TResult Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
    {
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>>.Get(_slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> FromCodeId(NCodeId id) => new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>>.Resolve(id), null);
}


public readonly struct NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>
{
    private readonly NHandle _handle;
    internal NFunc(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> Create(NContext context, string key, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> callback) => throw NGenerated.MissingTransformation();
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> Create<TState>(NContext context, string key, TState state, Func<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>>.Register(key, (nint)thunk, captureSchemaId);
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>>.Get(slot).SchemaId)));
    }
    public unsafe TResult Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> FromCodeId(NCodeId id) => new(new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>>.Resolve(id), default));
}


public readonly struct ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNFunc(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> Create(string key, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> Create<TState>(string key, TState state, Func<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>>.Resolve(id);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe TResult Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
    {
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>>.Get(_slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> FromCodeId(NCodeId id) => new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>>.Resolve(id), null);
}


public readonly struct NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>
{
    private readonly NHandle _handle;
    internal NFunc(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> Create(NContext context, string key, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> callback) => throw NGenerated.MissingTransformation();
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> Create<TState>(NContext context, string key, TState state, NStateFunc<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>>.Register(key, (nint)thunk, captureSchemaId);
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>>.Get(slot).SchemaId)));
    }
    public unsafe TResult Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> FromCodeId(NCodeId id) => new(new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>>.Resolve(id), default));
}


public readonly struct ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNFunc(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> Create(string key, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> Create<TState>(string key, TState state, NStateFunc<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>>.Resolve(id);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe TResult Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
    {
        ref readonly var entry = ref NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        return ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
    }
    public NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>>.Get(_slot).SchemaId)));
    }
    public ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> FromCodeId(NCodeId id) => new(NCodeTable<NFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>>.Resolve(id), null);
}


public readonly struct NAction
{
    private readonly NHandle _handle;
    internal NAction(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NAction>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NAction>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NAction Create(NContext context, string key, Action callback) => throw NGenerated.MissingTransformation();
    public static NAction Create<TState>(NContext context, string key, TState state, Action<TState> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, void> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NAction>.Register(key, (nint)thunk, captureSchemaId);
    public static NAction CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NAction>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NAction>.Get(slot).SchemaId)));
    }
    public unsafe void Invoke(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NAction>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        ((delegate* managed<object?, void>)entry.Thunk)(capture);
    }
    public NAction Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NAction>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNAction Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NAction>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NAction FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NAction FromCodeId(NCodeId id) => new(new(NCodeTable<NAction>.Resolve(id), default));
}


public readonly struct ManagedNAction
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNAction(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NAction>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NAction>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNAction Create(string key, Action callback) => throw NGenerated.MissingTransformation();
    public static ManagedNAction Create<TState>(string key, TState state, Action<TState> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNAction CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NAction>.Resolve(id);
        ValidateCapture(NCodeTable<NAction>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe void Invoke()
    {
        ref readonly var entry = ref NCodeTable<NAction>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        ((delegate* managed<object?, void>)entry.Thunk)(capture);
    }
    public NAction Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NAction>.Get(_slot).SchemaId)));
    }
    public ManagedNAction Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NAction>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNAction FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNAction FromCodeId(NCodeId id) => new(NCodeTable<NAction>.Resolve(id), null);
}


public readonly struct NAction<T1>
{
    private readonly NHandle _handle;
    internal NAction(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NAction<T1>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NAction<T1>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NAction<T1> Create(NContext context, string key, Action<T1> callback) => throw NGenerated.MissingTransformation();
    public static NAction<T1> Create<TState>(NContext context, string key, TState state, Action<TState, T1> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, void> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NAction<T1>>.Register(key, (nint)thunk, captureSchemaId);
    public static NAction<T1> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NAction<T1>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NAction<T1>>.Get(slot).SchemaId)));
    }
    public unsafe void Invoke(NContext context, T1 arg1)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NAction<T1>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        ((delegate* managed<object?, T1, void>)entry.Thunk)(capture, arg1);
    }
    public NAction<T1> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NAction<T1>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNAction<T1> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NAction<T1>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NAction<T1> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NAction<T1> FromCodeId(NCodeId id) => new(new(NCodeTable<NAction<T1>>.Resolve(id), default));
}


public readonly struct ManagedNAction<T1>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNAction(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NAction<T1>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NAction<T1>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNAction<T1> Create(string key, Action<T1> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1> Create<TState>(string key, TState state, Action<TState, T1> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NAction<T1>>.Resolve(id);
        ValidateCapture(NCodeTable<NAction<T1>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe void Invoke(T1 arg1)
    {
        ref readonly var entry = ref NCodeTable<NAction<T1>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        ((delegate* managed<object?, T1, void>)entry.Thunk)(capture, arg1);
    }
    public NAction<T1> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NAction<T1>>.Get(_slot).SchemaId)));
    }
    public ManagedNAction<T1> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NAction<T1>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNAction<T1> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNAction<T1> FromCodeId(NCodeId id) => new(NCodeTable<NAction<T1>>.Resolve(id), null);
}


public readonly struct NAction<T1, T2>
{
    private readonly NHandle _handle;
    internal NAction(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NAction<T1, T2>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NAction<T1, T2>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NAction<T1, T2> Create(NContext context, string key, Action<T1, T2> callback) => throw NGenerated.MissingTransformation();
    public static NAction<T1, T2> Create<TState>(NContext context, string key, TState state, Action<TState, T1, T2> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, void> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NAction<T1, T2>>.Register(key, (nint)thunk, captureSchemaId);
    public static NAction<T1, T2> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NAction<T1, T2>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NAction<T1, T2>>.Get(slot).SchemaId)));
    }
    public unsafe void Invoke(NContext context, T1 arg1, T2 arg2)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NAction<T1, T2>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        ((delegate* managed<object?, T1, T2, void>)entry.Thunk)(capture, arg1, arg2);
    }
    public NAction<T1, T2> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NAction<T1, T2>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNAction<T1, T2> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NAction<T1, T2>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NAction<T1, T2> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NAction<T1, T2> FromCodeId(NCodeId id) => new(new(NCodeTable<NAction<T1, T2>>.Resolve(id), default));
}


public readonly struct ManagedNAction<T1, T2>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNAction(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NAction<T1, T2>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NAction<T1, T2>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNAction<T1, T2> Create(string key, Action<T1, T2> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2> Create<TState>(string key, TState state, Action<TState, T1, T2> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NAction<T1, T2>>.Resolve(id);
        ValidateCapture(NCodeTable<NAction<T1, T2>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe void Invoke(T1 arg1, T2 arg2)
    {
        ref readonly var entry = ref NCodeTable<NAction<T1, T2>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        ((delegate* managed<object?, T1, T2, void>)entry.Thunk)(capture, arg1, arg2);
    }
    public NAction<T1, T2> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NAction<T1, T2>>.Get(_slot).SchemaId)));
    }
    public ManagedNAction<T1, T2> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NAction<T1, T2>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNAction<T1, T2> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNAction<T1, T2> FromCodeId(NCodeId id) => new(NCodeTable<NAction<T1, T2>>.Resolve(id), null);
}


public readonly struct NAction<T1, T2, T3>
{
    private readonly NHandle _handle;
    internal NAction(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NAction<T1, T2, T3> Create(NContext context, string key, Action<T1, T2, T3> callback) => throw NGenerated.MissingTransformation();
    public static NAction<T1, T2, T3> Create<TState>(NContext context, string key, TState state, Action<TState, T1, T2, T3> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, void> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NAction<T1, T2, T3>>.Register(key, (nint)thunk, captureSchemaId);
    public static NAction<T1, T2, T3> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NAction<T1, T2, T3>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NAction<T1, T2, T3>>.Get(slot).SchemaId)));
    }
    public unsafe void Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        ((delegate* managed<object?, T1, T2, T3, void>)entry.Thunk)(capture, arg1, arg2, arg3);
    }
    public NAction<T1, T2, T3> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NAction<T1, T2, T3>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NAction<T1, T2, T3>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NAction<T1, T2, T3> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NAction<T1, T2, T3> FromCodeId(NCodeId id) => new(new(NCodeTable<NAction<T1, T2, T3>>.Resolve(id), default));
}


public readonly struct ManagedNAction<T1, T2, T3>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNAction(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNAction<T1, T2, T3> Create(string key, Action<T1, T2, T3> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3> Create<TState>(string key, TState state, Action<TState, T1, T2, T3> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NAction<T1, T2, T3>>.Resolve(id);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe void Invoke(T1 arg1, T2 arg2, T3 arg3)
    {
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        ((delegate* managed<object?, T1, T2, T3, void>)entry.Thunk)(capture, arg1, arg2, arg3);
    }
    public NAction<T1, T2, T3> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NAction<T1, T2, T3>>.Get(_slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNAction<T1, T2, T3> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNAction<T1, T2, T3> FromCodeId(NCodeId id) => new(NCodeTable<NAction<T1, T2, T3>>.Resolve(id), null);
}


public readonly struct NAction<T1, T2, T3, T4>
{
    private readonly NHandle _handle;
    internal NAction(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NAction<T1, T2, T3, T4> Create(NContext context, string key, Action<T1, T2, T3, T4> callback) => throw NGenerated.MissingTransformation();
    public static NAction<T1, T2, T3, T4> Create<TState>(NContext context, string key, TState state, Action<TState, T1, T2, T3, T4> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, void> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NAction<T1, T2, T3, T4>>.Register(key, (nint)thunk, captureSchemaId);
    public static NAction<T1, T2, T3, T4> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NAction<T1, T2, T3, T4>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NAction<T1, T2, T3, T4>>.Get(slot).SchemaId)));
    }
    public unsafe void Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        ((delegate* managed<object?, T1, T2, T3, T4, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4);
    }
    public NAction<T1, T2, T3, T4> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NAction<T1, T2, T3, T4>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NAction<T1, T2, T3, T4>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NAction<T1, T2, T3, T4> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NAction<T1, T2, T3, T4> FromCodeId(NCodeId id) => new(new(NCodeTable<NAction<T1, T2, T3, T4>>.Resolve(id), default));
}


public readonly struct ManagedNAction<T1, T2, T3, T4>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNAction(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNAction<T1, T2, T3, T4> Create(string key, Action<T1, T2, T3, T4> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4> Create<TState>(string key, TState state, Action<TState, T1, T2, T3, T4> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NAction<T1, T2, T3, T4>>.Resolve(id);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        ((delegate* managed<object?, T1, T2, T3, T4, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4);
    }
    public NAction<T1, T2, T3, T4> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NAction<T1, T2, T3, T4>>.Get(_slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNAction<T1, T2, T3, T4> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNAction<T1, T2, T3, T4> FromCodeId(NCodeId id) => new(NCodeTable<NAction<T1, T2, T3, T4>>.Resolve(id), null);
}


public readonly struct NAction<T1, T2, T3, T4, T5>
{
    private readonly NHandle _handle;
    internal NAction(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NAction<T1, T2, T3, T4, T5> Create(NContext context, string key, Action<T1, T2, T3, T4, T5> callback) => throw NGenerated.MissingTransformation();
    public static NAction<T1, T2, T3, T4, T5> Create<TState>(NContext context, string key, TState state, Action<TState, T1, T2, T3, T4, T5> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, void> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NAction<T1, T2, T3, T4, T5>>.Register(key, (nint)thunk, captureSchemaId);
    public static NAction<T1, T2, T3, T4, T5> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NAction<T1, T2, T3, T4, T5>>.Get(slot).SchemaId)));
    }
    public unsafe void Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        ((delegate* managed<object?, T1, T2, T3, T4, T5, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5);
    }
    public NAction<T1, T2, T3, T4, T5> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NAction<T1, T2, T3, T4, T5>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NAction<T1, T2, T3, T4, T5>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NAction<T1, T2, T3, T4, T5> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NAction<T1, T2, T3, T4, T5> FromCodeId(NCodeId id) => new(new(NCodeTable<NAction<T1, T2, T3, T4, T5>>.Resolve(id), default));
}


public readonly struct ManagedNAction<T1, T2, T3, T4, T5>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNAction(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNAction<T1, T2, T3, T4, T5> Create(string key, Action<T1, T2, T3, T4, T5> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5> Create<TState>(string key, TState state, Action<TState, T1, T2, T3, T4, T5> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5>>.Resolve(id);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        ((delegate* managed<object?, T1, T2, T3, T4, T5, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5);
    }
    public NAction<T1, T2, T3, T4, T5> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NAction<T1, T2, T3, T4, T5>>.Get(_slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNAction<T1, T2, T3, T4, T5> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNAction<T1, T2, T3, T4, T5> FromCodeId(NCodeId id) => new(NCodeTable<NAction<T1, T2, T3, T4, T5>>.Resolve(id), null);
}


public readonly struct NAction<T1, T2, T3, T4, T5, T6>
{
    private readonly NHandle _handle;
    internal NAction(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NAction<T1, T2, T3, T4, T5, T6> Create(NContext context, string key, Action<T1, T2, T3, T4, T5, T6> callback) => throw NGenerated.MissingTransformation();
    public static NAction<T1, T2, T3, T4, T5, T6> Create<TState>(NContext context, string key, TState state, Action<TState, T1, T2, T3, T4, T5, T6> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, void> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NAction<T1, T2, T3, T4, T5, T6>>.Register(key, (nint)thunk, captureSchemaId);
    public static NAction<T1, T2, T3, T4, T5, T6> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6>>.Get(slot).SchemaId)));
    }
    public unsafe void Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6);
    }
    public NAction<T1, T2, T3, T4, T5, T6> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NAction<T1, T2, T3, T4, T5, T6>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NAction<T1, T2, T3, T4, T5, T6> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NAction<T1, T2, T3, T4, T5, T6> FromCodeId(NCodeId id) => new(new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6>>.Resolve(id), default));
}


public readonly struct ManagedNAction<T1, T2, T3, T4, T5, T6>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNAction(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNAction<T1, T2, T3, T4, T5, T6> Create(string key, Action<T1, T2, T3, T4, T5, T6> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6> Create<TState>(string key, TState state, Action<TState, T1, T2, T3, T4, T5, T6> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6>>.Resolve(id);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6);
    }
    public NAction<T1, T2, T3, T4, T5, T6> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6>>.Get(_slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNAction<T1, T2, T3, T4, T5, T6> FromCodeId(NCodeId id) => new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6>>.Resolve(id), null);
}


public readonly struct NAction<T1, T2, T3, T4, T5, T6, T7>
{
    private readonly NHandle _handle;
    internal NAction(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NAction<T1, T2, T3, T4, T5, T6, T7> Create(NContext context, string key, Action<T1, T2, T3, T4, T5, T6, T7> callback) => throw NGenerated.MissingTransformation();
    public static NAction<T1, T2, T3, T4, T5, T6, T7> Create<TState>(NContext context, string key, TState state, Action<TState, T1, T2, T3, T4, T5, T6, T7> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, void> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7>>.Register(key, (nint)thunk, captureSchemaId);
    public static NAction<T1, T2, T3, T4, T5, T6, T7> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7>>.Get(slot).SchemaId)));
    }
    public unsafe void Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
    }
    public NAction<T1, T2, T3, T4, T5, T6, T7> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6, T7> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NAction<T1, T2, T3, T4, T5, T6, T7> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NAction<T1, T2, T3, T4, T5, T6, T7> FromCodeId(NCodeId id) => new(new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7>>.Resolve(id), default));
}


public readonly struct ManagedNAction<T1, T2, T3, T4, T5, T6, T7>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNAction(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7> Create(string key, Action<T1, T2, T3, T4, T5, T6, T7> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7> Create<TState>(string key, TState state, Action<TState, T1, T2, T3, T4, T5, T6, T7> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7>>.Resolve(id);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
    }
    public NAction<T1, T2, T3, T4, T5, T6, T7> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7>>.Get(_slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6, T7> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7> FromCodeId(NCodeId id) => new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7>>.Resolve(id), null);
}


public readonly struct NAction<T1, T2, T3, T4, T5, T6, T7, T8>
{
    private readonly NHandle _handle;
    internal NAction(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8> Create(NContext context, string key, Action<T1, T2, T3, T4, T5, T6, T7, T8> callback) => throw NGenerated.MissingTransformation();
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8> Create<TState>(NContext context, string key, TState state, Action<TState, T1, T2, T3, T4, T5, T6, T7, T8> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, void> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8>>.Register(key, (nint)thunk, captureSchemaId);
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8>>.Get(slot).SchemaId)));
    }
    public unsafe void Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
    }
    public NAction<T1, T2, T3, T4, T5, T6, T7, T8> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8> FromCodeId(NCodeId id) => new(new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8>>.Resolve(id), default));
}


public readonly struct ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNAction(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8> Create(string key, Action<T1, T2, T3, T4, T5, T6, T7, T8> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8> Create<TState>(string key, TState state, Action<TState, T1, T2, T3, T4, T5, T6, T7, T8> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8>>.Resolve(id);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
    }
    public NAction<T1, T2, T3, T4, T5, T6, T7, T8> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8>>.Get(_slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8> FromCodeId(NCodeId id) => new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8>>.Resolve(id), null);
}


public readonly struct NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
    private readonly NHandle _handle;
    internal NAction(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(NContext context, string key, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> callback) => throw NGenerated.MissingTransformation();
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create<TState>(NContext context, string key, TState state, Action<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, void> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Register(key, (nint)thunk, captureSchemaId);
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Get(slot).SchemaId)));
    }
    public unsafe void Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
    }
    public NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> FromCodeId(NCodeId id) => new(new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Resolve(id), default));
}


public readonly struct ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNAction(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create(string key, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create<TState>(string key, TState state, Action<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Resolve(id);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
    {
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
    }
    public NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Get(_slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> FromCodeId(NCodeId id) => new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Resolve(id), null);
}


public readonly struct NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
{
    private readonly NHandle _handle;
    internal NAction(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create(NContext context, string key, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> callback) => throw NGenerated.MissingTransformation();
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create<TState>(NContext context, string key, TState state, Action<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, void> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>.Register(key, (nint)thunk, captureSchemaId);
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>.Get(slot).SchemaId)));
    }
    public unsafe void Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
    }
    public NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> FromCodeId(NCodeId id) => new(new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>.Resolve(id), default));
}


public readonly struct ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNAction(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create(string key, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create<TState>(string key, TState state, Action<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>.Resolve(id);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
    {
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
    }
    public NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>.Get(_slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> FromCodeId(NCodeId id) => new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>.Resolve(id), null);
}


public readonly struct NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
{
    private readonly NHandle _handle;
    internal NAction(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Create(NContext context, string key, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> callback) => throw NGenerated.MissingTransformation();
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Create<TState>(NContext context, string key, TState state, Action<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, void> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>.Register(key, (nint)thunk, captureSchemaId);
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>.Get(slot).SchemaId)));
    }
    public unsafe void Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
    }
    public NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> FromCodeId(NCodeId id) => new(new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>.Resolve(id), default));
}


public readonly struct ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNAction(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Create(string key, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Create<TState>(string key, TState state, Action<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>.Resolve(id);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
    {
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
    }
    public NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>.Get(_slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> FromCodeId(NCodeId id) => new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>.Resolve(id), null);
}


public readonly struct NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
{
    private readonly NHandle _handle;
    internal NAction(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Create(NContext context, string key, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> callback) => throw NGenerated.MissingTransformation();
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Create<TState>(NContext context, string key, TState state, Action<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, void> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>.Register(key, (nint)thunk, captureSchemaId);
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>.Get(slot).SchemaId)));
    }
    public unsafe void Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
    }
    public NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> FromCodeId(NCodeId id) => new(new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>.Resolve(id), default));
}


public readonly struct ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNAction(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Create(string key, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Create<TState>(string key, TState state, Action<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>.Resolve(id);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
    {
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
    }
    public NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>.Get(_slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> FromCodeId(NCodeId id) => new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>.Resolve(id), null);
}


public readonly struct NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
{
    private readonly NHandle _handle;
    internal NAction(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Create(NContext context, string key, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> callback) => throw NGenerated.MissingTransformation();
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Create<TState>(NContext context, string key, TState state, Action<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, void> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>.Register(key, (nint)thunk, captureSchemaId);
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>.Get(slot).SchemaId)));
    }
    public unsafe void Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
    }
    public NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> FromCodeId(NCodeId id) => new(new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>.Resolve(id), default));
}


public readonly struct ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNAction(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Create(string key, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Create<TState>(string key, TState state, Action<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>.Resolve(id);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
    {
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
    }
    public NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>.Get(_slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> FromCodeId(NCodeId id) => new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>.Resolve(id), null);
}


public readonly struct NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
{
    private readonly NHandle _handle;
    internal NAction(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Create(NContext context, string key, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> callback) => throw NGenerated.MissingTransformation();
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Create<TState>(NContext context, string key, TState state, Action<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, void> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>.Register(key, (nint)thunk, captureSchemaId);
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>.Get(slot).SchemaId)));
    }
    public unsafe void Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
    }
    public NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> FromCodeId(NCodeId id) => new(new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>.Resolve(id), default));
}


public readonly struct ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNAction(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Create(string key, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Create<TState>(string key, TState state, Action<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>.Resolve(id);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
    {
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
    }
    public NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>.Get(_slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> FromCodeId(NCodeId id) => new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>.Resolve(id), null);
}


public readonly struct NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
{
    private readonly NHandle _handle;
    internal NAction(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create(NContext context, string key, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> callback) => throw NGenerated.MissingTransformation();
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create<TState>(NContext context, string key, TState state, Action<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, void> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>.Register(key, (nint)thunk, captureSchemaId);
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>.Get(slot).SchemaId)));
    }
    public unsafe void Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
    }
    public NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> FromCodeId(NCodeId id) => new(new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>.Resolve(id), default));
}


public readonly struct ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNAction(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create(string key, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create<TState>(string key, TState state, Action<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>.Resolve(id);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
    {
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
    }
    public NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>.Get(_slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> FromCodeId(NCodeId id) => new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>.Resolve(id), null);
}


public readonly struct NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
{
    private readonly NHandle _handle;
    internal NAction(NHandle handle) => _handle = handle;
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>.Get(_handle.Slot).Id;
    public NCaptureRef CaptureRef => _handle.Capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>.Get(_handle.Slot).SchemaId is null || !_handle.Capture.IsEmpty;
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create(NContext context, string key, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> callback) => throw NGenerated.MissingTransformation();
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create<TState>(NContext context, string key, TState state, NStateAction<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static unsafe void RegisterGenerated(string key, delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, void> thunk, NCodeId? captureSchemaId = null)
        => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>.Register(key, (nint)thunk, captureSchemaId);
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> CreateRegistered(NContext context, NCodeId id, INCaptureState? capture)
    {
        ArgumentNullException.ThrowIfNull(context);
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>.Resolve(id);
        return new(new(slot, context.BindCapture(capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>.Get(slot).SchemaId)));
    }
    public unsafe void Invoke(NContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
    {
        ArgumentNullException.ThrowIfNull(context);
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>.Get(_handle.Slot);
        var capture = context.ResolveForInvocation(_handle.Capture, entry.SchemaId);
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
    }
    public NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Bind(NContext context, NCaptureRef captureRef)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_handle.Slot, context.ValidateBinding(captureRef, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>.Get(_handle.Slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var capture = (INCaptureState?)context.ResolveForInvocation(_handle.Capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>.Get(_handle.Slot).SchemaId);
        return new(_handle.Slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> FromCodeId(NCodeId id) => new(new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>.Resolve(id), default));
}


public readonly struct ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
{
    private readonly int _slot;
    private readonly INCaptureState? _capture;
    internal ManagedNAction(int slot, INCaptureState? capture) => (_slot, _capture) = (slot, capture);
    public NCodeId CodeId => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>.Get(_slot).Id;
    public INCaptureState? CaptureState => _capture;
    public bool IsBound => NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>.Get(_slot).SchemaId is null || _capture is not null;
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create(string key, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> callback) => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create<TState>(string key, TState state, NStateAction<TState, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> callback) where TState : class => throw NGenerated.MissingTransformation();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> CreateRegistered(NCodeId id, INCaptureState? capture)
    {
        var slot = NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>.Resolve(id);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>.Get(slot), capture);
        return new(slot, capture);
    }
    private static void ValidateCapture(in NCodeEntry entry, INCaptureState? capture)
    {
        if (entry.SchemaId is { } expected)
        {
            if (capture is null || capture.CaptureSchemaId != expected)
                throw new InvalidOperationException("Capture state is missing or its schema does not match callback.");
        }
        else if (capture is not null)
            throw new InvalidOperationException("Stateless callback cannot bind captured state.");
    }
    public unsafe void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
    {
        ref readonly var entry = ref NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>.Get(_slot);
        ValidateCapture(entry, _capture);
        var capture = (object?)_capture;
        ((delegate* managed<object?, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, void>)entry.Thunk)(capture, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
    }
    public NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Bind(NContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new(new(_slot, context.BindCapture(_capture, NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>.Get(_slot).SchemaId)));
    }
    public ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Bind(INCaptureState capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ValidateCapture(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>.Get(_slot), capture);
        return new(_slot, capture);
    }
    public byte[] ToBytes() => CodeId.ToBytes();
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> FromBytes(ReadOnlySpan<byte> bytes) => FromCodeId(NCodeId.FromBytes(bytes));
    public static ManagedNAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> FromCodeId(NCodeId id) => new(NCodeTable<NAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>.Resolve(id), null);
}
