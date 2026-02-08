# Research: Frank 7.0 Framework Upgrade

**Date**: 2026-02-07
**Branch**: `001-frank-upgrade`

## R1: Frank 7.0 Breaking Changes from 6.5

**Decision**: Upgrade is safe with minimal code changes.

**Rationale**: The only breaking change in Frank 7.0.0 is the addition of a `Metadata` field to `ResourceSpec`. This is additive and does not affect existing code unless it directly constructs or pattern-matches on `ResourceSpec`. The TicTacToe project only uses `ResourceBuilder` CE syntax, which is unaffected.

**Alternatives considered**:
- Stay on Frank 6.5: Rejected because it doesn't include Frank.Auth or the metadata system needed for declarative auth.

**Key findings**:
- `ResourceSpec` gains `Metadata: (EndpointBuilder -> unit) list` field
- All HTTP method custom operations unchanged in signature
- `WebHostSpec` unchanged
- `WebHostBuilder` custom operations unchanged
- New: `ResourceBuilder.AddMetadata` static method (used by Frank.Auth)

## R2: Frank.Auth API Surface

**Decision**: Use `requireAuth` on ResourceBuilder and `useAuthentication` on WebHostBuilder to replace manual auth.

**Rationale**: Frank.Auth provides exactly the pattern needed:
- `requireAuth` adds `[Authorize]` metadata to endpoints at the resource level
- `useAuthentication` configures `AuthenticationBuilder` in services AND adds `UseAuthentication()` middleware automatically
- `useAuthorization` adds `AddAuthorization()` and `UseAuthorization()` automatically

**Key design considerations**:
- `useAuthentication` calls `services.AddAuthentication()` (no default scheme arg), then passes the `AuthenticationBuilder` to the callback. Cookie registration via `.AddCookie()` sets cookie as the only scheme, making it the default.
- Current explicit default scheme configuration (`options.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme` etc.) is effectively duplicated by having only one scheme. However, to preserve exact behavior, the callback should configure defaults explicitly via the `AuthenticationBuilder`.
- `useAuthentication` also adds `UseAuthentication()` middleware, which is currently done manually via `plugBeforeRouting AuthAppBuilderExtensions.UseAuthentication`. This `plugBeforeRouting` must be removed to avoid double registration.
- Similarly, `useAuthorization` adds both service and middleware, replacing `plugBeforeRouting AuthorizationAppBuilderExtensions.UseAuthorization`.

**API available**:
```fsharp
// ResourceBuilder extensions
requireAuth                    // adds [Authorize] to all endpoints in resource
requireClaim "type" "value"    // adds claim-based auth
requireRole "roleName"         // adds role-based auth
requirePolicy "policyName"     // adds policy-based auth

// WebHostBuilder extensions
useAuthentication (fun builder -> builder.AddCookie(...))
useAuthorization
authorizationPolicy "name" (fun policy -> ...)
```

## R3: Oxpecker.ViewEngine 2.x Changes

**Decision**: Upgrade to Oxpecker.ViewEngine 2.0.0.

**Rationale**: No breaking API changes in public surface. All render functions (`toString`, `toStreamAsync`, `toTextWriterAsync`, `toBytes`) maintain identical signatures. Internal improvements include better byte encoding (no ArrayPool, uses `GC.AllocateUninitializedArray`). New `IntNode` feature is backward-compatible.

**Alternatives considered**:
- Stay on 1.1.0: Rejected because the Frank sample targets 2.x and internal perf improvements benefit this project.

**Key findings**:
- Target framework: .NET 10.0 only (project already targets net10.0)
- `Render.toStreamAsync(stream, view)` — writes to Stream via StreamWriter
- `Render.toTextWriterAsync(textWriter, view)` — writes to TextWriter
- Both use `StringBuilderPool.Get()` internally, render to StringBuilder, then write to stream/writer
- No breaking changes in HtmlElement, attribute API, or builder DSL

## R4: Streaming Architecture for SseEvent DU

**Decision**: Refactor `SseEvent` DU to carry `TextWriter -> Task` callbacks, using Frank.Datastar 7.1.0's native stream overloads for true zero-copy streaming.

**Rationale**: Frank.Datastar 7.1.0 (branch `015-datastar-streaming-html`) has its own native SSE implementation — no external `StarFederation.Datastar.FSharp` dependency. It provides `TextWriter -> Task` and `Stream -> Task` overloads for all SSE operations. Combined with Oxpecker's `Render.toTextWriterAsync`, this enables true zero-copy streaming from view engine through SSE to HTTP response with no intermediate string allocation.

**Design**:
```
Current:  View -> Render.toString -> string -> Channel -> string -> Datastar.patchElements -> Response
Proposed: View -> (TextWriter -> Task) callback -> Channel -> Datastar.streamPatchElements -> Response
```

The SseEvent DU changes from `PatchElements of html: string` to `PatchElements of render: (TextWriter -> Task)`. At write-time, `writeSseEvent` calls `Datastar.streamPatchElements render ctx`, which passes the response's internal TextWriter to the callback, which invokes `Render.toTextWriterAsync`. No string is allocated at any point.

**Key Frank.Datastar 7.1.0 stream API** (from `015-datastar-streaming-html`):
- `Datastar.streamPatchElements (writer: TextWriter -> Task) (ctx: HttpContext)` — TextWriter callback
- `Datastar.streamPatchElementsWithOptions (options) (writer: TextWriter -> Task) (ctx: HttpContext)` — with options
- `Datastar.streamPatchElementsToStream (writer: Stream -> Task) (ctx: HttpContext)` — Stream callback
- Equivalent `stream*` variants for `removeElement`, `patchSignals`, `executeScript`

**Frank.Datastar 7.1.0 native SSE source files**: `Consts.fs`, `Types.fs`, `ServerSentEvent.fs`, `SseDataLineWriter.fs`, `SseDataLineStream.fs`, `ServerSentEventGenerator.fs`, `Frank.Datastar.fs`

**Important**: Frank.Datastar 7.1.0 removes the `StarFederation.Datastar.FSharp` dependency entirely — the `open StarFederation.Datastar.FSharp` import in current code must be removed, and any types from that namespace (e.g., `PatchElementsOptions`, `Selector`, `ElementPatchMode`) now come from Frank.Datastar's own types.

## R5: Performance Measurement Approach

**Decision**: Use BenchmarkDotNet for microbenchmarks and manual timing for integration metrics.

**Rationale**: BenchmarkDotNet provides precise, statistically rigorous measurement of memory allocations and throughput for isolated render functions. Integration metrics (full request/response) can be measured with simple stopwatch timing and `dotnet-counters` for GC pressure.

**Measurement plan**:
1. Create `test/TicTacToe.Benchmarks/` project with BenchmarkDotNet
2. Benchmark: `renderGameBoard |> Render.toString` vs `renderGameBoard |> Render.toStreamAsync`
3. Benchmark: `layout.html |> Render.toString` vs `layout.html |> Render.toHtmlDocStreamAsync`
4. Capture: allocations, throughput (ops/sec), GC gen0/gen1/gen2 collections
5. Integration: time full page load and SSE game update under load

## R6: Auth Scope Expansion Impact

**Decision**: Expand `requireAuth` to all game mutation endpoints.

**Rationale**: Game mutations (create, move, delete) already check for `userId` and return 401 if absent. Adding `requireAuth` formalizes this at the framework level, providing consistent challenge/redirect behavior instead of bare 401 status codes.

**Impact on existing tests**:
- Integration tests that make unauthenticated game mutations may need to authenticate first
- Unit tests (Expecto) are unaffected — they test engine logic, not HTTP auth
- Playwright tests already authenticate via TestBase
