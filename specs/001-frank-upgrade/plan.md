# Implementation Plan: Frank 7.0 Framework Upgrade

**Branch**: `001-frank-upgrade` | **Date**: 2026-02-07 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-frank-upgrade/spec.md`

## Summary

Upgrade the TicTacToe web application from Frank 6.5.0 to Frank 7.0.0, Frank.Datastar 7.1.0, Frank.Auth 7.0.0, and Oxpecker.ViewEngine 2.0.0. Replace the custom `requiresAuth` handler wrapper with Frank.Auth's declarative `requireAuth` resource builder operation, expanding auth to all game mutation endpoints. Move cookie authentication configuration into Frank.Auth's WebHostBuilder extensions. Refactor HTML rendering to use streaming for direct responses (`Render.toHtmlDocStreamAsync`) and deferred callbacks for SSE broadcasts. Measure and report performance before and after.

## Technical Context

**Language/Version**: F# targeting .NET 10.0
**Primary Dependencies**: Frank 7.0.0, Frank.Datastar 7.1.0 (native SSE, no StarFederation dependency), Frank.Auth 7.0.0, Oxpecker.ViewEngine 2.0.0
**Storage**: In-memory via MailboxProcessor (GameSupervisor, PlayerAssignmentManager)
**Testing**: Expecto (unit tests, 43 tests), Playwright + NUnit (24 integration tests)
**Target Platform**: ASP.NET Core web server (.NET 10.0)
**Project Type**: Web application (server-rendered hypermedia)
**Performance Goals**: >=40% memory allocation reduction for rendering, >=15% response time improvement
**Constraints**: All 43 unit tests must pass; all 24 integration tests must pass; identical HTML output
**Scale/Scope**: 4 source files modified (Program.fs, Handlers.fs, SseBroadcast.fs, .fsproj), 1 new benchmark project

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Assessment |
|-----------|--------|------------|
| I. Functional-First F# | PASS | All changes use idiomatic F#. Deferred rendering callbacks are pure functions. MailboxProcessor unchanged. |
| II. Hypermedia Architecture | PASS | SSE broadcasting pattern preserved. Datastar interactions unchanged. No client JS changes. |
| III. Test-First Development | PASS | Existing tests validate behavior preservation. New benchmark project uses Expecto conventions. |
| IV. Simplicity & Focus | PASS | Framework upgrade is justified by concrete benefits (declarative auth, streaming perf). No speculative features. |
| Protected: TicTacToe.Engine | PASS | Engine is not modified. All changes are in TicTacToe.Web. |
| Technology Stack | NOTE | Frank 7.0.0, Frank.Auth 7.0.0, Oxpecker.ViewEngine 2.0.0 are version upgrades of approved technologies. Constitution tech stack should be updated after merge. |

**Post-Phase 1 Re-check**: All principles pass. The design adds no new abstractions, patterns, or dependencies beyond version upgrades of already-approved technologies plus Frank.Auth (which replaces custom auth code, net reducing complexity).

## Project Structure

### Documentation (this feature)

```text
specs/001-frank-upgrade/
├── plan.md              # This file
├── research.md          # Phase 0 output - API research findings
├── data-model.md        # Phase 1 output - entity changes
├── quickstart.md        # Phase 1 output - implementation guide
├── contracts/           # Phase 1 output
│   ├── auth-endpoints.md    # Auth behavior contract
│   └── rendering-contract.md # Streaming render contract
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── TicTacToe.Engine/        # UNCHANGED - protected component
│   ├── Model.fs
│   └── Engine.fs
└── TicTacToe.Web/
    ├── TicTacToe.Web.fsproj # MODIFY: package version updates + Frank.Auth
    ├── Model.fs             # unchanged
    ├── Extensions.fs        # unchanged
    ├── Auth.fs              # unchanged
    ├── SseBroadcast.fs      # MODIFY: SseEvent DU callback refactoring
    ├── templates/
    │   ├── shared/layout.fs # unchanged
    │   ├── game.fs          # unchanged
    │   └── home.fs          # unchanged
    ├── Handlers.fs          # MODIFY: remove requiresAuth, streaming renders, deferred callbacks
    └── Program.fs           # MODIFY: Frank.Auth CE operations, resource-level auth

test/
├── TicTacToe.Engine.Tests/  # unchanged
├── TicTacToe.Web.Tests/     # unchanged (tests validate behavior)
└── TicTacToe.Benchmarks/    # NEW: BenchmarkDotNet performance tests
    ├── TicTacToe.Benchmarks.fsproj
    └── RenderBenchmarks.fs
```

**Structure Decision**: Existing project structure preserved. Only one new project added (`TicTacToe.Benchmarks`) for performance measurement — justified by explicit spec requirement FR-007/FR-008/FR-009 to measure and report performance.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| New benchmark project | Required by FR-007/FR-008/FR-009 to measure performance before/after | Inline timing is not statistically rigorous; BenchmarkDotNet provides allocation tracking |

## Implementation Phases

### Phase A: Package Upgrade & Build Verification

**Goal**: Get the project building with new package versions. No behavior changes.

**Changes**:
1. Update `TicTacToe.Web.fsproj`:
   - `Frank` 6.5.0 → 7.0.0
   - `Frank.Datastar` 6.5.0 → 7.1.0
   - Add `Frank.Auth` 7.0.0
   - `Oxpecker.ViewEngine` 1.1.0 → 2.0.0
2. Fix any compilation errors from breaking changes (expected: none based on research)
3. Run unit tests to verify no regressions

**Verification**: `dotnet build` succeeds, `dotnet test test/TicTacToe.Engine.Tests/` passes all 43 tests.

### Phase B: Performance Baseline

**Goal**: Capture pre-optimization performance metrics.

**Changes**:
1. Create `test/TicTacToe.Benchmarks/` project with BenchmarkDotNet
2. Write benchmarks for:
   - `renderGameBoard |> Render.toString` (current SSE pattern)
   - `homePage |> layout.html |> Render.toString` (current page pattern)
3. Run benchmarks and record baseline results

**Verification**: Benchmark project builds and produces baseline metrics.

### Phase C: Authentication Modernization

**Goal**: Replace custom auth with Frank.Auth declarative auth.

**Changes**:
1. `Program.fs`:
   - Remove auth-related code from `configureServices` (AddAuthentication, AddCookie, AddAuthorization)
   - Add `useAuthentication` CE operation with cookie config callback
   - Add `useAuthorization` CE operation
   - Remove `plugBeforeRouting AuthAppBuilderExtensions.UseAuthentication`
   - Remove `plugBeforeRouting AuthorizationAppBuilderExtensions.UseAuthorization`
   - Add `requireAuth` to resource definitions for `/`, `/games`, `/games/{id}`, `/games/{id}/reset`
   - Remove `Handlers.requiresAuth` wrapper from `get`/`post` calls
2. `Handlers.fs`:
   - Remove the `requiresAuth` function entirely
   - Update `home` handler doc comment (no longer "use with requiresAuth wrapper")

**Verification**:
- Unit tests pass (unaffected)
- Manual test: unauthenticated access to `/` redirects to `/login`
- Manual test: unauthenticated POST to `/games` redirects to `/login`
- Manual test: authenticated access works normally

### Phase D: Streaming Optimization

**Goal**: Replace `Render.toString` with true zero-copy streaming for both direct responses and SSE broadcasts using Frank.Datastar 7.1.0's native stream API.

**Changes**:
1. `SseBroadcast.fs`:
   - Change `SseEvent` DU: `PatchElements of render: (TextWriter -> Task)` and `PatchElementsAppend of selector: string * render: (TextWriter -> Task)`
   - Update `writeSseEvent` to use `Datastar.streamPatchElements render ctx` and `Datastar.streamPatchElementsWithOptions opts render ctx`
   - Remove `open StarFederation.Datastar.FSharp` (types now from Frank.Datastar)
2. `Handlers.fs`:
   - `home`: Replace `Render.toString` + `WriteAsync` with `Render.toHtmlDocStreamAsync ctx.Response.Body`
   - `getGame`: Replace `Render.toString` + `WriteAsync` with `Render.toHtmlDocStreamAsync ctx.Response.Body`
   - `sse`: Replace `Render.toString` with `Render.toTextWriterAsync` inside `TextWriter -> Task` callback
   - `subscribeToGame`: Wrap render in callback `fun tw -> renderGameBoard ... |> Render.toTextWriterAsync tw`
   - `createGame`: Wrap render in `TextWriter -> Task` callback
   - `resetGame`: Wrap render in `TextWriter -> Task` callback
   - Remove `open StarFederation.Datastar.FSharp` (types now from Frank.Datastar)

**Verification**:
- Unit tests pass
- Integration tests pass (HTML output identical)
- Visual verification of game board rendering

### Phase E: Performance Measurement & Report

**Goal**: Capture post-optimization metrics and generate comparison report.

**Changes**:
1. Update benchmarks to include streaming variants:
   - `renderGameBoard |> Render.toStreamAsync` (new streaming pattern)
   - `homePage |> layout.html |> Render.toHtmlDocStreamAsync` (new page pattern)
2. Run benchmarks and compare with baseline
3. Generate performance report at `specs/001-frank-upgrade/performance-report.md`

**Verification**: Report includes before/after metrics for allocations, throughput, and GC pressure.

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Frank.Auth `useAuthentication` default scheme behavior differs | Low | High | Test auth flow immediately after Phase C; verify cookie is set correctly |
| Oxpecker 2.0 byte encoding changes affect HTML output | Very Low | Medium | Compare HTML output before/after with snapshot tests |
| `requireAuth` on game mutations breaks existing integration tests | Medium | Low | Tests already authenticate via TestBase; update any that don't |
| Performance doesn't improve as expected | Low | Medium | Baseline captured first; can report actual results regardless |
| Frank.Auth adds `UseAuthentication` middleware at wrong pipeline position | Low | High | Verify middleware order matches current: compression → static files → auth → authz → antiforgery |
