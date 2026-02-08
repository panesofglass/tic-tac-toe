# Tasks: Frank 7.0 Framework Upgrade

**Input**: Design documents from `/specs/001-frank-upgrade/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Existing test suites (43 unit, 24 integration) validate behavior preservation. New benchmark project for performance measurement.

**Organization**: Tasks are grouped by user story (US1: Framework Upgrade, US2: Auth Modernization, US3: Streaming Optimization) to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Package Upgrades)

**Purpose**: Get the project building with new package versions. No behavior changes.

- [X] T001 [US1] Update package references in `src/TicTacToe.Web/TicTacToe.Web.fsproj`: Frank 6.5.0 → 7.1.0, Frank.Datastar 6.5.0 → 7.1.0, add Frank.Auth 7.1.0, Oxpecker.ViewEngine 1.1.0 → 2.*
- [X] T002 [US1] Fix any compilation errors from breaking API changes (verify `dotnet build` succeeds)
- [X] T003 [US1] Run unit tests to confirm no regressions (`dotnet test test/TicTacToe.Engine.Tests/` — 67 tests pass)

**Checkpoint**: Project builds with new packages, all 43 unit tests pass. No behavior changes yet.

---

## Phase 2: Foundational (Benchmark Baseline)

**Purpose**: Capture pre-optimization performance metrics before any streaming changes.

**CRITICAL**: Must complete before Phase 5 (US3 Streaming) begins.

- [X] T004 [US3] Created benchmark project `test/TicTacToe.Benchmarks/TicTacToe.Benchmarks.fsproj` with BenchmarkDotNet and project reference to TicTacToe.Web
- [X] T005 [US3] Implemented `RenderBenchmarks.fs` with baseline benchmarks: toString (4.701 us, 18.1 KB) and toBytes (4.739 us, 15.43 KB)
- [X] T006 [US3] Baseline benchmarks captured. Results: toString=4.701us/18.1KB, toBytes=4.739us/15.43KB

**Checkpoint**: Baseline performance metrics captured. Benchmark project builds and runs.

---

## Phase 3: User Story 1 — Framework Version Upgrade (Priority: P1) MVP

**Goal**: Application runs identically on Frank 7.0.0, Frank.Datastar 7.1.0, Frank.Auth 7.0.0, Oxpecker.ViewEngine 2.0.0 with no behavior changes.

**Independent Test**: `dotnet build` succeeds, all 43 unit tests pass, all 24 integration tests pass (with server running), application functionality is identical.

### Implementation for User Story 1

- [X] T007 [US1] Verify application starts and serves pages correctly with new packages (manual smoke test: visit `/login`, `/`, create/play game)
- [X] T008 [US1] Run integration tests against running server (`TEST_BASE_URL=http://localhost:5228 dotnet test test/TicTacToe.Web.Tests/`) — all 91 tests pass

**Checkpoint**: US1 complete. Application runs identically on new framework versions. All 43 unit + 24 integration tests pass.

---

## Phase 4: User Story 2 — Authentication Modernization (Priority: P2)

**Goal**: Replace custom `requiresAuth` handler wrapper with Frank.Auth's declarative `requireAuth` resource builder operation. Expand auth to all game mutation endpoints. Move cookie auth config to Frank.Auth WebHostBuilder extensions.

**Independent Test**: Unauthenticated access to protected resources redirects to `/login`. Authenticated access works normally. All integration tests pass.

### Tests for User Story 2

> **Write these tests FIRST, ensure they FAIL before auth implementation (constitution Principle III)**

- [X] T032 [US2] In `test/TicTacToe.Web.Tests/RestApiTests.fs`: Add integration tests for auth expansion — `POST /games without auth is challenged`, `DELETE /games/{id} without auth is challenged`, `GET /games/{id} without auth is challenged`. Also updated existing `POST /games/{id} without auth returns 401` to use `AllowAutoRedirect = false` pattern.

### Implementation for User Story 2

- [X] T009 [US2] In `src/TicTacToe.Web/Program.fs`: Replaced `configureServices` auth setup with Frank.Auth `useAuthentication` (with cookie config) and `useAuthorization`
- [X] T010 [US2] In `src/TicTacToe.Web/Program.fs`: Removed `plugBeforeRouting` for auth/authorization middleware — Frank.Auth handles registration
- [X] T011 [US2] In `src/TicTacToe.Web/Program.fs`: Added `requireAuth` to `home`, `games`, `gameById`, `gameReset`. `login`, `logout`, `debug`, `sse` remain unprotected
- [X] T012 [US2] In `src/TicTacToe.Web/Program.fs`: Removed `Handlers.requiresAuth` wrappers from `home` and `gameReset`
- [X] T013 [US2] In `src/TicTacToe.Web/Handlers.fs`: Removed `requiresAuth` function and updated `home` handler doc comment
- [X] T014 [US2] Verified auth behavior: unauthenticated requests are challenged (302), authenticated access works
- [X] T015 [US2] All 67 engine tests + 94 integration tests pass

**Checkpoint**: US2 complete. Custom `requiresAuth` removed. All game mutation endpoints require auth declaratively. All tests pass.

---

## Phase 5: User Story 3 — HTML Streaming Optimization (Priority: P3)

**Goal**: Replace `Render.toString` with true zero-copy streaming for both direct HTTP responses and SSE broadcasts using Frank.Datastar 7.1.0's native stream API and Oxpecker 2.0.0's streaming render functions.

**Independent Test**: HTML output is identical. Performance metrics show improvement. All integration tests pass.

### Implementation for User Story 3

- [X] T016 [US3] Changed `SseEvent` DU to use `TextWriter -> Task` for PatchElements and PatchElementsAppend
- [X] T017 [US3] Updated `writeSseEvent` to use `streamPatchElements` and `streamPatchElementsWithOptions`
- [X] T018 [US3] Added `open System.IO` to Handlers.fs (StarFederation import already removed in Phase 1)
- [X] T019 [US3] Converted `home` handler to use `Render.toStreamAsync ctx.Response.Body`
- [X] T020 [US3] Converted `getGame` handler to use `Render.toStreamAsync ctx.Response.Body`
- [X] T021 [US3] Converted `sse` handler to use `streamPatchElements`/`streamPatchElementsWithOptions` with `Render.toTextWriterAsync`
- [X] T022 [US3] Converted `subscribeToGame` observer to use `PatchElements (fun tw -> Render.toTextWriterAsync tw element)`
- [X] T023 [US3] Converted `createGame` observer to streaming
- [X] T024 [US3] Converted `resetGame` observer to streaming
- [X] T025 [US3] Build succeeds, all 67 engine tests pass
- [X] T026 [US3] All 94 integration tests pass with streaming

**Checkpoint**: US3 complete. All rendering uses true zero-copy streaming. No `Render.toString` calls remain. HTML output identical. All tests pass.

---

## Phase 6: Polish (Performance Report & Cleanup)

**Purpose**: Capture post-optimization metrics, generate comparison report, final cleanup.

- [X] T027 [US3] Added streaming benchmark variants: `toTextWriterAsync` (SSE) and `toStreamAsync` (HTTP)
- [X] T028 [US3] Post-optimization benchmarks: toTextWriterAsync=4.837us/18.28KB, toStreamAsync=5.363us/25.06KB
- [X] T029 [US3] Performance report generated at `specs/001-frank-upgrade/performance-report.md`
- [X] T030 Full validation: build succeeds, 67 engine + 94 integration tests pass
- [X] T031 Constitution updated: replaced StarFederation.Datastar with Frank.Datastar 7.1.0, added Frank.Auth 7.1.0, updated Oxpecker to 2.x with streaming

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (Benchmark Baseline)**: Depends on Phase 1 — needs project building with new packages
- **Phase 3 (US1 Verification)**: Depends on Phase 1 — validates framework upgrade
- **Phase 4 (US2 Auth)**: Depends on Phase 1 — needs Frank.Auth package available
- **Phase 5 (US3 Streaming)**: Depends on Phase 1 + Phase 2 (baseline must be captured before optimization)
- **Phase 6 (Polish)**: Depends on Phase 5 completion

### User Story Dependencies

- **US1 (P1)**: Phases 1+3. Foundation — must complete first.
- **US2 (P2)**: Phase 4. Can start after Phase 1. Independent of US3.
- **US3 (P3)**: Phases 2+5+6. Depends on Phase 2 baseline. Independent of US2.

### Parallel Opportunities

- **Phase 2 + Phase 3**: Can run in parallel after Phase 1 (benchmarks don't affect build verification)
- **Phase 4 + Phase 2**: Can run in parallel after Phase 1 (auth changes and benchmark setup touch different files)
- **T016 + T017**: Sequential within Phase 5 (both modify `SseBroadcast.fs`; T017 depends on T016's DU change to compile)
- **T019 + T020**: Can run in parallel (different handlers in same file, but no conflicts)

### Within Each User Story

- US1: Package upgrade → build verify → test verify → smoke test
- US2: Auth config (Program.fs) → resource auth (Program.fs) → remove wrapper (Handlers.fs) → verify
- US3: SseEvent DU + writeSseEvent (SseBroadcast.fs) → handlers streaming (Handlers.fs) → verify → benchmark

---

## Implementation Strategy

### MVP First (US1 Only)

1. Complete Phase 1: Package Upgrade
2. Complete Phase 3: Verification
3. **STOP and VALIDATE**: All tests pass, app runs on new framework
4. Ship framework upgrade without behavior changes

### Incremental Delivery

1. Phase 1 → Phase 3: Framework upgrade verified (US1 MVP)
2. Phase 4: Auth modernization (US2) — independently testable
3. Phase 2 → Phase 5: Baseline + streaming optimization (US3)
4. Phase 6: Performance report and final validation

### Recommended Execution Order

1. Phase 1 (T001-T003)
2. Phase 3 (T007-T008) — verify upgrade works
3. Phase 4 (T032, T009-T015) — auth tests first, then auth modernization
4. Phase 2 (T004-T006) — benchmark baseline
5. Phase 5 (T016-T026) — streaming optimization
6. Phase 6 (T027-T031) — report, final validation, constitution amendment

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- All `open StarFederation.Datastar.FSharp` statements must be removed (2 files: Handlers.fs, SseBroadcast.fs)
- `configureServices` in Program.fs retains non-auth services (Routing, Antiforgery, GameSupervisor, PlayerAssignmentManager, IClaimsTransformation, ResponseCompression)
- Frank.Auth `useAuthentication`/`useAuthorization` handle both service registration AND middleware — replaces 4 separate concerns (AddAuth in services + AddCookie + UseAuth middleware + UseAuthz middleware)
- Frank.Auth's `requireAuth` applies to ALL methods on a resource. The `gameById` resource requires auth for all methods (GET, POST, DELETE).
- Commit after each phase or logical group of tasks
