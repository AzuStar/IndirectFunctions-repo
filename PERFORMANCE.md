# Implementation and performance

Usage and short examples: [README.md](README.md).

Stable code identities and ahead-of-time indirect callbacks for .NET 8+. Build tooling requires .NET SDK 10.

`NFunc<...>` and `NAction<...>` are unmanaged holders. Generated managed function-pointer tables contain executable code; holders store checked runtime slots and capture tokens. `ManagedNFunc<...>` and `ManagedNAction<...>` retain capture state directly.

## Use

Reference `IndirectFunctions` NuGet package. Package includes runtime, analyzer/generator, and automatic MSBuild transformation. Generated code enables `AllowUnsafeBlocks`; user code stays safe.

See [integration examples](tests/Integration/Program.cs) for typed `Create`, explicit shared state, `.Invoke`, profile conversions, and persistence. Use globally unique immutable keys. Named method groups and literal lambdas are accepted; arbitrary delegate variables are rejected. Different closed generic registrations require distinct keys.

Creation markers are valid C# in editor. Real builds replace calls and lower shared captured variables into library-owned cells. Original source stays unchanged; transformed source lives under `obj/indirect-functions`. Calling an untransformed marker fails explicitly.

## Persistence

`ToBytes()` returns exactly 16 canonical bytes identifying compiled code. It intentionally omits capture state and runtime binding. Any backend supporting byte arrays can persist this identity. `FromBytes()` checks target/signature; captured callbacks remain unbound.

Save capture state separately with `NContext.ExportCaptures`. Export related roots together to preserve shared cells. Neutral graph records contain schema IDs, named fields, typed leaf data, and node references. Backend owns leaf encoding and identity shared with application objects. Quiesce capture mutation while exporting.

Implement `INCaptureLeafCodec.Encode<T>` / `Decode<T>` to connect your serializer. Pass codec to export and import; node references bypass leaf encoding. [JSON fixture](tests/Integration/JsonPersistence.cs) saves code IDs and capture graph to disk, then restores both in a fresh process using source-generated JSON metadata.

Restore graph through `ImportCaptures`, then call decoded pointer's `Bind(context, restoredCaptureRef)`. Restored cells are independent of original live locals. Unknown IDs and incompatible capture schemas fail; migrate application state before importing. Never persist `NCaptureRef` or runtime slots.

Preserve graph `RootIds` alongside `Roots`: repeated root identities restore aliases with one release lifetime; separate roots sharing cells retain independent lifetimes. An `NCell<T>` hidden behind an opaque application object needs its closed `SchemaId` registered by application before cold import.

## Lifetime and performance

Contexts own capture roots. `Release` invalidates unmanaged aliases of released root; `Dispose` invalidates all context tokens. Managed callbacks and other contexts retain shared cells. Invocation already holding a validated root may finish. Re-import creates fresh token; stale tokens never revive.

Unmanaged guarantee applies to holder layout. Capture creation may allocate GC-tracked cells. Dispatch performs checked array access and exact-signature managed function-pointer calls, without per-invocation allocation, reflection, dictionary lookup, or locks. Callback code keeps normal allocation and threading semantics.

## Build and validation

`bash tests/validate.sh` builds solution and runs CoreCLR, analyzer, package, top-level, and fresh-process persistence checks. JSON, MessagePack, and protobuf-net fixtures cover records containing `NFunc`, function lists, producer/consumer bootstrap, managed/unmanaged Func/Action identities, capture graph aliasing, independent imports, released roots, unknown IDs, signature mismatches, and unknown schemas. Each serializer writes snapshots in one process and restores them in another. It also verifies source renames and registration reorder across two independent builds. Add `--aot` for native runtime and source-generated JSON checks; MessagePack/protobuf example adapters are checked on CoreCLR. Set `GODOT_BINARY` to a Godot .NET executable to include headless smoke. Validation builds benchmarks without running measurements.

- `dotnet build IndirectFunctions.slnx`
- `dotnet run --project tests/Integration -c Release`
- `dotnet run --project tests/Serialization -c Release`
- `dotnet run --project IndirectFunctions.Generators.Tests -c Release`
- `dotnet run --project IndirectFunctions.Runtime.Tests -c Release`
- `dotnet pack IndirectFunctions.Package.csproj -c Release -o artifacts/packages`
- `dotnet publish tests/NativeAot -c Release -r linux-x64 -o artifacts/aot`, then run `artifacts/aot/NativeAot`.
- Build `tests/GodotSmoke`; run Godot .NET with `--headless --path tests/GodotSmoke --script Runner.gd`.
- `dotnet run --project benchmarks -c Release -- --filter '*Dispatch*' --job short`

Godot smoke uses ordinary .NET library; no engine integration. Core runtime targets net8.0; NativeAOT fixture uses SDK 10 runtime/compiler. BenchmarkDotNet suite covers homogeneous/mixed targets, capture sizes, binding/profile conversion, persistence costs, and allocations against cached delegates and direct switch.

Other source generators run against original source paths before capture transformation; their emitted declarations remain compiler inputs. Callback catalog runs afterward. Original source locations remain available in diagnostics and caller information.

Design references: [C# capture semantics](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/expressions), [Roslyn generator constraints](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md).

V1 excludes dynamic/existing delegate inputs, open generic registration sites, by-ref/custom signatures, callbacks emitted later by unrelated generators, and collectible plugins. Async/iterator capture data can persist; running continuations and enumerator positions cannot.

<!-- indirect-functions-comparison-docs:start -->
## Compare normal delegate with indirect callback

Run comparison yourself. Counts use `long`; 10 billion or more calls work without count truncation. Each row executes exactly `--iterations` calls per run. Warmup and binding stay outside measured loops.

```sh
dotnet run --project benchmarks/Comparison -c Release -- --iterations 10000000000 --runs 5 --warmup 1000000 --profile unmanaged --readme PERFORMANCE.md
dotnet run --project benchmarks/Comparison -c Release -- --iterations 10000000000 --runs 5 --warmup 1000000 --profile managed --capture --readme PERFORMANCE.md
```

Defaults: 100 million calls per row/run, five paired runs, 100,000 warmup calls, stateless unmanaged profile. `--capture` accepts an optional `true`/`false` value; `--warmup 0` disables warmup. `--help` lists options without executing loops. Omit `--readme` for console output only.

Startup, warmup, and each row log progress. During execution, updates about once per second show completed calls, row/overall percentage, elapsed time, and approximate remaining time. ETA refines as both callback profiles run. Adaptive chunks target 250 ms; timing and allocation totals sum measured chunks, excluding progress output. Hot loops contain no progress checks. Restart an already-running benchmark to use this update; Ctrl+C stops it without replacing performance results.

Exactly two rows compare ordinary `Func<long, long>` with selected `NFunc` or `ManagedNFunc`. Stateless rows use same named method. Captured rows use equal values and arithmetic in separate helpers; normal delegate retains native C# closure without library cell lowering. Run order alternates; report shows median elapsed time, ns/call, paired indirect/normal ratio, current-thread allocations per call, checksums, and runtime/OS details. Lower ratio means faster indirect calls. Every paired checksum must match.

`--readme` updates results between markers below and preserves surrounding documentation. Initial values remain `Not run` until you execute comparison.

<!-- indirect-functions-comparison-results:start -->
Run: 2026-10-03T14:14:47.1312289+00:00; .NET 8.0.22; Ubuntu 24.04.5 LTS; X64; logical processors: 32.

Iterations per row/run: 10000000000; paired runs: 5; warmup calls per row: 100000; capture: false; Stopwatch frequency: 1000000000 Hz.

| Callback | Median total (s) | ns/call | Ratio vs Func | Allocated bytes/call | Checksum |
|---|---:|---:|---:|---:|---:|
| `Func<long, long> (normal)` | 13.168236 | 1.317 | 1.000 | 0.000000 | 87025000000000 |
| `NFunc<long, long> (unmanaged)` | 13.831102 | 1.383 | 1.051 | 0.000000 | 87025000000000 |

Ratio = median paired indirect/normal elapsed time. Timing sums adaptive chunks and excludes setup, binding, warmup, checksum validation, progress reporting, and output; checksums match each run. Allocation counts cover current-thread calls.
<!-- indirect-functions-comparison-results:end -->
<!-- indirect-functions-comparison-docs:end -->
