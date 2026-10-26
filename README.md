# IndirectFunctions

Serializable function references for .NET 8+. Copy callbacks between records, save them, or send them to another process with the same compiled callback catalog.

Use for job queues, distributed commands, saved workflows, game save/replay data, callback tables, and shared mutable closures. Payloads carry stable code IDs and optional capture data; executable methods must already exist on every endpoint.

## Install

```sh
dotnet add package IndirectFunctions
```

Build with .NET SDK 10. Package includes runtime, analyzer, and automatic source transformation; application code stays safe C#.

## Quick start

```csharp
using IndirectFunctions;

using var context = new NContext();
var fn = NFunc<int, int>.Create(context, "tests.identity.double", Double);
Console.WriteLine(fn.Invoke(context, 4)); // 8

static int Double(int value) => value * 2;
```

Use globally unique, immutable keys. Pass named method groups or literal lambdas to `Create`. Supported signatures follow `Func`/`Action`, with 0–16 arguments, including closed generics, async callbacks, and captured instance methods.

| Holder | Call | Lifetime |
|---|---|---|
| `NFunc<...>` | `fn.Invoke(context, ...)` | Context owns captures; holder is unmanaged |
| `NAction<...>` | `action.Invoke(context, ...)` | Same, without return value |
| `ManagedNFunc<...>` | `fn.Invoke(...)` | Holder retains captures |
| `ManagedNAction<...>` | `action.Invoke(...)` | Same, without return value |

```csharp
int value = 1;
var next = NFunc<int>.Create(context, "app.counter.next.v1", () => ++value);
var copy = next;
Console.WriteLine(copy.Invoke(context)); // 2; value is also 2

var state = new NCell<int>(10);
var add = NFunc<int, int>.Create(context, "app.counter.add.v1", state,
    static (cell, amount) => cell.Value += amount);
Console.WriteLine(add.Invoke(context, 5)); // 15

var log = ManagedNAction<int>.Create("app.log.v1", static n => Console.WriteLine(n));
log.Invoke(15);

var retained = add.Bind(context); // Managed holder retains shared state.
using var otherContext = new NContext();
var imported = otherContext.Import(retained); // Another context shares state.
```

Dispose contexts after use. `context.Release(fn.CaptureRef)` invalidates copies of that captured root. [More callback forms](tests/Integration/Program.cs).

## JSON, MessagePack, protobuf

Every holder exposes `ToBytes()` / `FromBytes()`. Stateless callbacks need only their 16-byte code ID:

```csharp
string json = JsonSerializer.Serialize(fn.ToBytes());
var restored = NFunc<int, int>.FromBytes(JsonSerializer.Deserialize<byte[]>(json)!);
Console.WriteLine(restored.Invoke(context, 4)); // 8
```

```csharp
byte[] packed = MessagePackSerializer.Serialize(fn.ToBytes());
var restored = NFunc<int, int>.FromBytes(MessagePackSerializer.Deserialize<byte[]>(packed));
Console.WriteLine(restored.Invoke(context, 4)); // 8
```

```csharp
using var stream = new MemoryStream();
ProtoBuf.Serializer.Serialize(stream, fn.ToBytes());
stream.Position = 0;
var restored = NFunc<int, int>.FromBytes(ProtoBuf.Serializer.Deserialize<byte[]>(stream));
Console.WriteLine(restored.Invoke(context, 4)); // 8
```

Imports: `System.Text.Json`, `MessagePack`, and `System.IO`; binary examples require `MessagePack` and `protobuf-net` packages. For fields containing `NFunc` directly, register a converter, formatter, or surrogate. [Runnable adapters and serializer options](tests/Serialization/Serializers.cs) cover `NFunc<int, int>`; add adapters for other closed signatures or store their code bytes explicitly.

## Preinitialized endpoints, copied records, function lists

Each endpoint owns an independent context and runs shared bootstrap once. This class defines the available methods:

```csharp
public sealed class Endpoint : IDisposable
{
    public NContext Context { get; } = new();
    public NFunc<int, int> Double { get; }
    public NFunc<int, int> Square { get; }

    public Endpoint()
    {
        Double = NFunc<int, int>.Create(Context, "samples.math.double.v1", DoubleValue);
        Square = NFunc<int, int>.Create(Context, "samples.math.square.v1", SquareValue);
    }

    static int DoubleValue(int n) => n * 2;
    static int SquareValue(int n) => n * n;
    public void Dispose() => Context.Dispose();
}
```

The sample [Job record](tests/Serialization/Bootstrap.cs) contains `Input`, `Function`, and `List<NFunc<int, int>> Functions`, with MessagePack/protobuf member attributes. Its JSON converter stores each function as a base64 code ID.

```csharp
using var sender = new Endpoint();
var original = new Job { Input = 4, Function = sender.Double,
    Functions = [sender.Double, sender.Square] };
var envelope = new Envelope(original with { Input = 5 }); // Copy into another record.
byte[] payload = JsonSerializer.SerializeToUtf8Bytes(envelope.Job, Formats.Json);

// Another process/machine: same shared bootstrap, independent context.
using var receiver = new Endpoint();
var restored = JsonSerializer.Deserialize<Job>(payload, Formats.Json)!;
Console.WriteLine(restored.Function.Invoke(receiver.Context, restored.Input)); // 10
foreach (var function in restored.Functions)
    Console.WriteLine(function.Invoke(receiver.Context, restored.Input)); // 10, 25
```

`Formats.Json` belongs to the example adapters linked above. [Full example](tests/Serialization/RecordTransfer.cs) also runs with MessagePack and protobuf. Keys and signatures must match across endpoints; version keys when callback semantics change.

## Producer / consumer

Producer and consumer share `Endpoint` and serializer configuration. Queue carries serialized records:

```csharp
var queue = Channel.CreateBounded<byte[]>(1); // using System.Threading.Channels;
using var producer = new Endpoint();
using var consumer = new Endpoint();

var job = new Job { Input = 4, Function = producer.Double };
await queue.Writer.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(job, Formats.Json));
queue.Writer.Complete();

await foreach (var bytes in queue.Reader.ReadAllAsync())
{
    var received = JsonSerializer.Deserialize<Job>(bytes, Formats.Json)!;
    Console.WriteLine(received.Function.Invoke(consumer.Context, received.Input)); // 8
}
```

[Full producer/consumer example](tests/Serialization/ProducerConsumer.cs) uses separate producer and consumer tasks and checks all three formats. Replace channel with a broker or network transport to cross processes.

## Captured state

Code IDs omit captured values. Export related roots together, serialize graph alongside IDs, then import and bind:

```csharp
var graph = context.ExportCaptures([read.CaptureRef, increment.CaptureRef], codec);
// Persist read.ToBytes(), increment.ToBytes(), and a wire representation of graph.
// Receiver decodes the IDs and graph using the same leaf codec:
var roots = receiver.Context.ImportCaptures(restoredGraph, codec);
var restoredRead = NFunc<int>.FromBytes(readCode).Bind(receiver.Context, roots[0]);
var restoredIncrement = NFunc<int>.FromBytes(incrementCode).Bind(receiver.Context, roots[1]);
Console.WriteLine(restoredIncrement.Invoke(receiver.Context)); // 7, from saved value 6
Console.WriteLine(restoredRead.Invoke(receiver.Context));      // 7; shared cell preserved
```

Implement `INCaptureLeafCodec` for application leaf types. Preserve graph `Roots` and `RootIds`; never serialize runtime slots or `NCaptureRef`. Restored captures are independent of original live locals. [Complete JSON/MessagePack/protobuf capture example](tests/Serialization/CapturePersistence.cs), including wire contracts and codecs.

Run examples and checks: `dotnet run --project tests/Serialization -c Release`. Full validation: `bash tests/validate.sh`; add `--aot` for NativeAOT runtime/JSON checks. [PERFORMANCE.md](PERFORMANCE.md) contains benchmarks, lifetime rules, build details, persistence internals, and limitations.
