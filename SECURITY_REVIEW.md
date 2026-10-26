# Function serialization security review

Reviewed: 2026-10-15. Scope: current local source, runtime, generated catalogs,
source transformation, serializer examples, and relevant tests. No deployed
server, application authorization code, or transport was supplied. Severity below
assumes untrusted clients can submit callback IDs or capture graphs for execution.
This is a source review with targeted local probes, not a penetration test or
dependency vulnerability audit. Recommendations describe proposed behavior;
security tiers and policy enforcement are **not implemented** in current runtime.

## Decision

Treat serialized function references as requests to invoke server capabilities.
Define policy in trusted server code, keyed by callback identity and signature.
Deny undeclared functions. Require explicit permission for each request kind,
principal, tenant/resource, capture shape, and transport direction.

**Current runtime cannot enforce requested security tiers.** It verifies identity,
signature, capture compatibility, and lifetime. It does not authorize callbacks.
Client knowing a privileged registered callback's key can derive its ID, deserialize
it into matching holder type, and invoke it if application forwards that holder.
No original callback instance or capture is needed for a stateless callback.

Recommended first network boundary: accept inert IDs plus typed argument DTOs;
authorize through one dispatcher; allow only explicitly declared stateless
callbacks initially. Add captured callbacks only with explicit schema/field rules.
Keep unrestricted persistence APIs within trusted application paths.

OWASP recommends default denial and permission checks on every request. Its
deserialization guidance recommends restricting allowable types before object
materialization. Those principles inform design below; API and tier names are
recommendations specific to this repository.
[Authorization guidance](https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html),
[deserialization guidance](https://cheatsheetseries.owasp.org/cheatsheets/Deserialization_Cheat_Sheet.html).

## Existing boundaries and guarantees

| Path | Current behavior | Security implication |
| --- | --- | --- |
| `NCodeId.FromKey` | First 16 bytes of domain-separated SHA-256 over key | Stable identifier; no secret, issuer proof, or authorization. Hash does not cover method body. |
| `FromBytes` / `FromCodeId` | Resolves registered ID in closed signature table | Rejects unknown IDs and wrong signatures; accepts registered privileged IDs with same signature. |
| `NFunc.Invoke(context)` | Looks up thunk, validates capture/context, calls pointer | Context ownership and schema checks provide memory/lifetime discipline, not caller permissions. |
| `ManagedNFunc.Invoke` | Validates retained capture and calls thunk | No context or request argument exists for authorization. Managed Actions behave likewise. |
| `ImportCaptures` | Creates all nodes, validates roots, then reads fields node by node | Factories/readers/codecs can run before entire payload is accepted. |
| `ExportCaptures` | Traverses roots and generated capture fields | Captured locals, `this`, state, and receiver can reach codec; no destination or disclosure policy. |
| Generated catalog | Module initializer registers callbacks and dependencies | Registration is process-wide availability; creating a fresh context does not narrow catalog. |

Evidence: [NCodeId.cs](IndirectFunctions/NCodeId.cs),
[NGenerated.cs](IndirectFunctions/NGenerated.cs),
[Callbacks.g.cs](IndirectFunctions/Callbacks.g.cs),
[NContext.cs](IndirectFunctions/NContext.cs),
[NCapture.cs](IndirectFunctions/NCapture.cs),
[CatalogGenerator.cs](IndirectFunctions.Generators/CatalogGenerator.cs).

Positive controls worth retaining: exactly 16 code bytes; registered-only lookup;
closed signature separation; conflicting registration detection; matching capture
schema; fresh/distinct factory nodes; unique positive node IDs; reference validity;
duplicate-field rejection; and capture owner/generation/disposal checks.
Payloads do not supply executable IL or native pointers. This review does not
establish arbitrary uploaded-code execution. Impact depends on capabilities of
registered methods and application codecs/factories.

## Findings

### F1 — High: callback substitution bypasses request restrictions

`Callbacks.g.cs:133-134` resolves bytes directly; `NGenerated.cs:146-154` only checks
registration under requested signature. `Callbacks.g.cs:114-119` then invokes
without route, principal, or permission checks. Same pattern spans Func/Action
families and managed holders.

Example: request permits `quote.preview.v1`, but `account.refund.v1` shares its
signature. Replacing code ID selects refund callback unless application independently
authorizes exact callback. Hiding keys, hashing keys, or refusing to serialize
refund callback does not block forged input. Different signatures reduce matches,
but are not authorization. Dependency catalogs can expand available callback set.

Fix: deny-by-default callback policy plus exact per-request allowlist. Authorize
before binding and again at invocation when execution can occur under a different
request or later time. Local probe confirmed derived registered ID executes in
fresh context, and managed holder executes without context.

### F2 — High: schema compatibility does not establish capture authority

`NContext.cs:102-134` validates schema identity. `NCaptureReader.Field<T>` validates
presence/type or delegates to codec. Neither validates ownership, business ranges,
authority, or permitted relationships. Schema-correct graph can forge quantities,
object IDs, flags, or state that callback previously assumed was server-created.

Generated environments include captured cells, `this`, explicit state, and receiver
(`Transformer.cs:250-264`). Leaf codec can resolve external objects, deserialize
nested callback holders, or instantiate application objects. Those are additional
application trust boundaries. A callback typed as read-only can still call code
with side effects; tier annotation is a reviewed contract, not a sandbox proof.

Fix: separate capture modes: none, validated client data, server-issued snapshot,
or server-owned state. Never trust client-supplied principal, role, tenant ownership,
service instance, or permission flags. Resolve external references with current
principal/tenant checks. Validate shared-node and cycle semantics where relevant.
Probe confirmed arbitrary integer state binds when expected schema matches.

### F3 — High when hooks have effects: import executes hooks before full validation

`ImportCaptures` calls factories at `NContext.cs:218`, before validating graph roots.
It invokes each node's `Read` at line 258 before checking later nodes' fields.
It materializes and reads all supplied nodes, including unreachable ones and graphs
with zero roots. `NGenerated.CreateCapture` checks global registration, not whether
schema is allowed for requested callback or request.

Generated factories allocate nodes and generated readers populate fields; review
found no built-in malicious hook. Custom factories, readers, or leaf codecs may
perform effects, however. Rejecting final graph cannot undo those effects.
Even allocation-only factories can amplify resource consumption.

Fix: validate bounded inert graph and every allowed schema/field/reference before
any factory. Reject unreachable nodes at untrusted boundary. Then materialize into
private staging context using side-effect-free factories/readers/codecs. Validate
decoded values before publishing handles. External authority lookups remain
authorization-sensitive even if read-only. Probe confirmed all three ordering
cases: factory before invalid root; unreachable reader; early reader before later
invalid value kind.

### F4 — Medium, potentially high availability impact: no application graph budgets

`ImportCaptures` has no configurable limits for nodes, roots, fields, total leaf
bytes, aliases, or aggregate work. Iterative traversal avoids recursive graph stack
growth but still allows large allocations and costly decoding. A small encoded
payload can also expand substantially through parser/codec behavior.

Fix: cap transport bytes before parsing; decompressed bytes; parser depth/string
and collection lengths; callback count; nodes/edges/roots; fields per node; total
leaf bytes; external lookups; import concurrency; and execution cost. Count repeated
invocations even when multiple roots alias one state. Charge whole batch, not each
subgraph separately. Validate before large allocation, not after building huge DTO.
Set workload-specific values and benchmark accepted maxima.

### F5 — High if used as revocation: managed binding escapes context lifetime

`NFunc.Bind(context)` retains actual capture in managed holder; managed invocation
has no originating context. Release/disposal therefore cannot revoke managed
copy's ability to run. Importing managed holder into another context can also share
same mutable state. These are documented lifetime semantics, not independently a
bug. They become security bugs if context disposal is treated as authority expiry.

Fix: require checked invocation scope for managed and unmanaged request execution.
Do not expose raw managed holder from security boundary. Recheck current policy for
queued/deferred work. Enforce revocation independently of capture lifetime.
Probe confirmed retained holder survives root release and context disposal.

### F6 — Medium: outbound capture disclosure and permissive field acceptance

No serialization permission distinguishes internal snapshots from client responses.
`ExportCaptures` visits every reachable generated capture field; codec determines
what ordinary objects expose. Capturing `this` can inadvertently expose more state
than function's intended arguments. `ToBytes()` itself exports identity only.

Importer rejects duplicate fields but accepts unconsumed extra fields: generated
reader fetches expected fields without checking all supplied fields were consumed.
This makes strict schema validation and consistent version handling unavailable.
Probe confirmed extra field beside `NCell<int>.Value` is ignored.

Fix: outbound destination rules, schema/field allowlists, explicit transfer DTOs,
strict field sets for network schemas, and independent read/write validators.
Treat client-delivered bytes as disclosed; a client SDK's export restriction cannot
protect server secrets or prevent attacker crafting bytes.

### F7 — Medium to high depending on operation: no replay, issuer, or semantic version binding

Code bytes carry no audience, issuer, expiry, nonce, tenant, resource binding, policy
version, or argument/capture integrity. Replaying a valid mutating request may repeat
effects. Reusing stable key after semantic change resolves old bytes to new code.
Current registration collision checks protect identity consistency within process;
they do not authenticate payload or detect cross-deployment semantic changes.

Fix: authenticate requests, authorize current meaning, version keys when semantics
change, and apply replay/idempotency controls to mutations. If server grants
transferable execution authority, use authenticated envelope described below.
TLS or a signature does not grant authorization for every callback.

## Proposed rule model

Assign each callback a trusted immutable descriptor during build/registration.
Do not accept descriptor, tier, required permissions, or request kind from payload.
Missing descriptor means remote import/export/invocation denied; trusted local use
can remain available through explicitly separate path.

| Descriptor field | Rule |
| --- | --- |
| Identity | Exact `NCodeId`, closed signature, semantic/security version, catalog provenance |
| Tier | Reviewed classification below; never inferred from ID/key prefix |
| Inbound channels | Exact allowed origins, e.g. client RPC, authenticated service queue, internal snapshot |
| Outbound destinations | Independent set, e.g. client response, internal queue, encrypted backup |
| Request kinds | Exact server-defined route/command and payload position; default empty |
| Required permissions | All required permissions must be granted by current authenticated authority |
| Resource validator | Current tenant, ownership, resource state, and argument checks |
| Capture mode | None, validated client DTO, authenticated server snapshot, or server-owned handle |
| Schema descriptors | Allowed reachable schemas, exact field names/types, edge rules, validators |
| Limits | Per-callback cost plus bounded request/batch/import limits |
| Freshness | Supported versions, expiry/replay rules where required |

Tier proposal:

| Tier | Intended exposure | Required restriction |
| --- | --- | --- |
| `RemoteSafe` | Explicitly exposed bounded computation | Allowed route, validated arguments, cost limits; authentication still follows route policy |
| `UserScoped` | Operation on caller's resources | Authenticated permission plus tenant/ownership checks |
| `Privileged` | Administrative or service operation | Dedicated administrative/service request and explicit permission; reject ordinary client routes |
| `InternalOnly` | Maintenance, local workflow, implementation callbacks | Deny client import/export/invocation regardless of caller's other permissions; optionally allow separately authenticated internal channel |

Use tiers for sensible defaults and review. **Do not authorize with `callerTier >=
functionTier`.** Administrator authority does not automatically make every request
route, data destination, resource, or capture acceptable. A simple read/write
hierarchy also misses operations that require unrelated permissions.

Request decision is intersection, with every condition required:

1. Callback known, signature/version supported, trusted descriptor present.
2. Server-selected channel, route, and field position explicitly allow callback.
3. Authenticated subject has every required permission where applicable.
4. Tenant/resource and argument checks succeed under current state.
5. Entire capture graph meets selected capture mode and schema rules.
6. Aggregate quotas, expiry, replay rules, and current policy permit execution.

Outbound decision separately requires allowed destination, recipient authorization,
and capture disclosure policy. Being importable never implies being exportable.

### Concrete policy example

These keys illustrate proposed policy; they are not existing repository callbacks.

| Callback | Tier | Inbound / outbound | Request allowlist | Capture / extra checks |
| --- | --- | --- | --- | --- |
| `pricing.preview.v1` | `RemoteSafe` | Client RPC / client response | `POST /quotes: Job.Function` and `Job.Functions[]` | None; bounded quantities and list length |
| `inventory.reserve.v1` | `UserScoped` | Client RPC / internal queue | `POST /reservations: Command.Function` | None; `inventory.reserve` permission and current tenant stock checks |
| `account.refund.v1` | `Privileged` | Admin RPC / internal audit | `POST /admin/refunds: Command.Function` | Server-owned state; `payments.refund` permission, resource checks, atomic idempotency |
| `maintenance.reindex.v1` | `InternalOnly` | Authenticated service queue / internal snapshot | `ReindexJob: Job.Function` | Server-owned state; dedicated service identity; never client channel |

Client sends refund ID to quote route -> reject before graph materialization,
even if ID is valid and request's user is administrator. Client claims
`tier=RemoteSafe` or embeds function in nested list -> same rejection. Check every
function-bearing field and collection element, not only top-level function.

## Enforcement architecture

### Import: parse data, authorize, then materialize

1. Enforce transport/framing/decompression limits; authenticate caller/channel.
2. Parse bounded inert wire contract containing IDs, typed arguments, and optional
   graph. No callback-resolving converters, arbitrary runtime type names, factories,
   or authority-bearing object construction during this step.
3. Server selects immutable request scope from actual endpoint and authenticated
   identity. Client's route/role/tier strings cannot create scope.
4. Resolve trusted metadata and authorize all function occurrences and payload
   positions. Reject whole batch before executing any callback if one is denied.
5. Snapshot privately owned graph data. Preflight all node/root IDs, schema/field
   sets, references, value kinds, reachability, expected root-schema bindings,
   alias/cycle rules, and budgets. A globally registered schema alone is insufficient.
6. Materialize approved nodes and decode explicitly typed leaves into staging
   context. Apply decoded-value/resource validators; publish bound handles only
   after acceptance. Dispose staging context on failure. Hooks must not rely on
   disposal to roll back effects.
7. Invoke through checked dispatcher; keep raw holders inside trusted boundary.

Current `NCaptureSchema` stores factory but no field/edge descriptors. Strict
preflight therefore needs generated schema metadata or separately authored wire
schema validators. Import wrapper checking root schema alone cannot validate
transitive nodes. Reject or recursively validate callbacks embedded inside leaf
DTOs/codecs as well. Freeze/copy mutable inputs so validation and materialization
cannot observe different graphs.

### Invoke: preserve request authority on every executable path

Security gateway owns raw callbacks and invokes only after current authorization.
Gateway API should take explicit request scope, callback descriptor/ID, and validated
arguments. Returning raw holder gives callers direct unchecked `Invoke` path.
If executable handles must escape dispatcher, introduce opaque checked holder that
requires explicit scope and cannot unwrap or bind into raw managed holder publicly.

Cover all Func/Action arities, managed forms, `FromBytes`, `FromCodeId`,
`CreateRegistered`, both binding directions, `NContext.Import`, capture graph APIs,
serializer adapters, and nested callback leaves. Adding policy only to `FromBytes`
or only to `NContext` leaves other paths unchecked. Security enforcement assumes
trusted server application code uses gateway; this library cannot contain malicious
in-process code with arbitrary .NET/unsafe access.

For async callbacks, do not store caller policy in thread-static registration state
or mutable global serializer options. Carry explicit request authority across await
and honor application cancellation. Queued work requires deliberate delegated
authority and fresh authorization when executed, not restored request-context bytes.
If immediate revocation during long-running operations is required, define resource
transaction/cancellation semantics; checking only entry cannot stop an active thunk.

Validate all batch items before first execution, but do not promise transactional
rollback of successful earlier operations. Use application transactions or explicit
partial-failure semantics. Make ownership checks and mutations atomic where races
would violate invariants.

### Serialize: destination policy before traversal

Check callback export permission and recipient/destination before writing identity
or traversing captures. Apply transitive graph and leaf disclosure rules. Prefer
dedicated snapshot DTOs to serializing service objects or receivers. Imported
snapshots must still pass current request policy; being emitted by server once does
not grant permanent permission to invoke in another request.

### Authenticated envelopes, when authority must travel

Ordinary stateless RPC can use transport identity plus current authorization without
signed callback tokens. Use server-issued authenticated envelope when allowing
client to return a server-approved workflow/snapshot or when delegating queued work.
Bind callback ID, signature/version, arguments/capture digest or immutable server
handle, issuer, audience, tenant/subject, permitted operation, expiry, and token ID.
Authenticate exact framed bytes or a specified canonical encoding; do not invent
ambiguous concatenation or reserialization rules. Verify signature/MAC before any
object materialization; current policy and resource checks still apply.

Keep signing/MAC keys off untrusted clients. An authenticated client can sign or
authenticate its own request and still request forbidden operation. Use separate
issuer/audience rules and key rotation. Encryption protects sensitive capture
contents; integrity alone does not. Mutations need atomic replay consumption or
idempotency tied to complete operation identity and tenant, including concurrent
delivery and retries. Verify policy freshness even for valid historical signatures.

## Serializer integration

Example adapters currently call unrestricted `FromBytes` during JSON/MessagePack/
protobuf decoding (`tests/Serialization/Serializers.cs:14-42`). They demonstrate
round trips, not secure request boundaries. Replace network DTO callback fields with
inert code IDs and use gateway after parse, or supply immutable request-specific
policy adapters. Never mutate a shared global converter's current caller field.

JSON needs bounded input/depth/collections, explicit DTOs, and rejection of unknown
security-relevant fields or polymorphic discriminators. Duplicate identity/authority
fields must have unambiguous rejection semantics. Protobuf contracts need explicit
field presence/version checks, size/recursion limits, and consistent unknown-field
handling; default values must not confer authority.

MessagePack example uses `Standard.WithResolver` without `UntrustedData` security.
Official guidance recommends `.WithSecurity(MessagePackSecurity.UntrustedData)`
and avoiding typeless resolvers for hostile input. Apply limits within custom
formatters too; secure options do not automatically authorize function IDs or make
every custom formatter safe. Check encoded code length before allocating large byte
arrays. [MessagePack security guidance](https://github.com/MessagePack-CSharp/MessagePack-CSharp#security).

Do not introduce `BinaryFormatter`, unrestricted polymorphic deserialization, or
wire-controlled `Type.GetType`/assembly loading in leaf codec. Microsoft identifies
these as unsafe patterns, including save files and apparently trusted storage.
[Microsoft serialization security guidance](https://learn.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-security-guide).

## Performance and implementation plan

Keep registration, reflection-free generated descriptors, signature resolution,
graph inspection, and expensive resource decisions outside tight invocation loops.
Compile immutable per-route/per-signature allowed-slot sets or bitsets at startup.
Combine static eligibility with current permission mask in dispatcher; resource and
tenant checks remain operation-specific. Do not cache successful authority globally
by code ID. Any cache needs all relevant identity/resource/policy dimensions and
defined invalidation. Benchmark before promising allocation or latency targets.

1. **Immediate application boundary:** inert DTOs, exact route callback allowlists,
   stateless-only client input, explicit permissions/resource checks, budgets, and
   per-request context disposal. Can wrap current runtime without changing ABI.
2. **Library policy metadata:** descriptors and schema fields/edges from transformer,
   trusted registration consistency checks including policy, and explicit gateway
   APIs for import/export/invocation. Missing policy denies remote access.
3. **Captured remote functions:** preflight/staging importer, strict codecs and
   destination-aware export, graph/value validators, optional authenticated snapshots.
4. **Hardening:** misuse diagnostics, adversarial matrix below, fuzzing bounded
   parsers/importer, concurrency checks, and security-overhead benchmarks.

Proposed extension points: `NGenerated.cs` for immutable policy/registration metadata;
`Transformer.cs` and generator tests for policy/schema descriptors; `NContext.cs`
for preflight/staging import; `NCapture.cs` for schema/field validation metadata;
holder APIs and serializer adapters for checked boundary. Avoid hand-editing repeated
generated holder patterns without maintaining their source/generation process.

Compatibility decision remains for implementation: additive checked gateway
preserves trusted callers, but unrestricted APIs remain usable by server code.
Making every existing holder enforce policy, changing managed invocation signatures,
or default-denying existing persistence is a breaking change requiring user approval.
This report makes no runtime/API changes.

## Required security tests before accepting client functions

| Test | Required result |
| --- | --- |
| Valid forbidden ID with matching signature | Denied before thunk and graph factories |
| Missing descriptor, spoofed tier/role/route, unsupported version | Denied |
| Authorized user sends correct function to wrong route/field position | Denied |
| Cross-tenant resource, capture ID, or external reference | Denied before effects |
| Forbidden function nested in list, graph leaf, or batch | Entire batch denied before any invocation |
| Unreachable/disallowed schema; oversized or malformed later node | Denied before factory/reader/codec |
| Extra/duplicate field, invalid kind, wrong root schema, forbidden alias/cycle | Denied consistently across formats |
| Invalid typed leaf/range after decoding | Staging discarded; no externally visible effects |
| Managed bind/import, root release, deferred invoke, permission revocation | Cannot escape current invocation policy |
| Wrong audience/subject/tenant, altered args/graph, expired signed token | Denied |
| Concurrent replay of mutating request | At most one committed operation under chosen idempotency contract |
| Response export of internal callback or secret capture | Denied before serialization/traversal |
| Concurrent request scopes and async continuations | No principal, tenant, or policy leakage |
| Accepted maximum payload and repeated allowed calls | Bounded memory/work and measured performance |

Report unknown IDs and denied known IDs through same external error category where
catalog enumeration matters. Keep detailed reason codes in bounded internal logs;
omit captures, secrets, and raw payloads. Rate-limit rejected work and logging.

## Validation evidence and limits

Existing checks run against current source:

- Runtime suite: `dotnet run --project IndirectFunctions.Runtime.Tests -c Release --no-restore`.
  Passed: `runtime assertions=20210; invocation allocations=0; holder bytes=32`.
- Generator/analyzer suite: `dotnet run --project IndirectFunctions.Generators.Tests -c Release --no-restore`.
  Passed: `Generator/analyzer checks passed: 40.`
- Serializer fixture: Release build passed with zero warnings/errors;
  `dotnet tests/Serialization/bin/Release/net8.0/Serialization.dll` passed:
  `Serialization: 174 checks passed across JSON, MessagePack, protobuf.`
- Isolated observational probe built against runtime in
  `/tmp/indirect-security-review.4ZFmiXnh/` with zero warnings/errors. Ten checks
  confirmed F1/F2/F3/F5/F6 behaviors and existing rejection of wrong signatures and
  unknown callbacks. Effects were counters and integers only. Probe is temporary,
  not a production or regression-test change; source remains there for this session.

Existing suites validate compatibility and structural defenses, not proposed tier
enforcement. No proposed mitigation was implemented or benchmarked. Full package,
NativeAOT, Godot, transport integration, fuzzing, load, and dependency advisory checks
were not run. No Git metadata was available in supplied directory, so evidence is
anchored to source fingerprints rather than a commit:

| Runtime source | SHA-256 |
| --- | --- |
| `NCodeId.cs` | `8a0709b0330d804c5ca4e15c091e0dd3956dc9ac208b019645b74fae09974320` |
| `NGenerated.cs` | `d2a79f5fd94bf7b57ca3c0a4d23adde42ba74a682cbe4c52d02b08485da913a1` |
| `NContext.cs` | `2c23d0db68a5be4854e3aea5a17f7d71d6f56b6cd13411b6243696d78ce8a05f` |
| `NCapture.cs` | `32f5db6b91a41a60e678ebc89854a3ba5828cf7372b6df51a74e7cb0d9e3fe53` |
| `Callbacks.g.cs` | `7e38b0faf8b4fc9e11544969d1320974edd47221ba316bb46106d29cfdc07a5b` |
