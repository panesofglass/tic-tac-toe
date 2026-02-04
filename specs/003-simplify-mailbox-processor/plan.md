# Implementation Plan: Simplify MailboxProcessor - Remove System.Reactive

**Branch**: `003-simplify-mailbox-processor` | **Date**: 2026-02-04 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/003-simplify-mailbox-processor/spec.md`

## Summary

Remove System.Reactive dependency from TicTacToe.Engine by replacing `BehaviorSubject<MoveResult>` and `IObservable<MoveResult>` with pure MailboxProcessor + direct callback registration. This simplifies the codebase and reduces external dependencies.

## Technical Context

**Language/Version**: F# targeting .NET 10.0
**Primary Dependencies**: Frank 6.4.0, Frank.Datastar 6.4.0, Oxpecker.ViewEngine 1.1.0 (System.Reactive 6.0.2 to be removed)
**Storage**: In-memory via MailboxProcessor (existing pattern)
**Testing**: Expecto (unit tests), Playwright with NUnit (web tests)
**Target Platform**: ASP.NET Core server
**Project Type**: Web application (server-rendered with SSE)
**Performance Goals**: Real-time updates within 100ms latency (matching current)
**Constraints**: Must maintain all existing functionality; no breaking changes to tests
**Scale/Scope**: Multiple concurrent games; existing test suite must pass

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Functional-First F# | ✅ PASS | MailboxProcessor is constitution-mandated for stateful concurrent components |
| II. Hypermedia Architecture | ✅ PASS | No changes to SSE/Datastar patterns; refactoring is internal |
| III. Test-First Development | ✅ PASS | Existing tests validate refactoring; no new features requiring new tests |
| IV. Simplicity & Focus | ✅ PASS | Removing dependency aligns with simplicity principle |
| Protected Components | ⚠️ CAUTION | Engine is PROTECTED; changes require extreme caution (see justification below) |

**Engine Modification Justification**:
- Change is internal refactoring, not behavioral modification
- Type-safe game state transitions remain unchanged (Model.fs untouched)
- Invalid-state-prevention guarantees preserved
- All existing tests must pass, verifying correctness
- Simplification aligns with Constitution's Simplicity principle

## Project Structure

### Documentation (this feature)

```text
specs/003-simplify-mailbox-processor/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output (callback types)
├── quickstart.md        # Phase 1 output (implementation guide)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── TicTacToe.Engine/
│   ├── Engine.fs         # Game and GameSupervisor - PRIMARY MODIFICATION
│   ├── Model.fs          # Domain model - NO CHANGES
│   └── TicTacToe.Engine.fsproj  # Remove System.Reactive reference
│
└── TicTacToe.Web/
    ├── Handlers.fs       # Update IObserver usage to callbacks
    ├── SseBroadcast.fs   # NO CHANGES (already callback-based)
    └── Program.fs        # NO CHANGES

test/
├── TicTacToe.Engine.Tests/
│   ├── EngineTests.fs    # Update test subscription helpers
│   └── SupervisorTests.fs # Update game completion tracking
│
└── TicTacToe.Web.Tests/
    └── MultiGameTests.fs  # NO CHANGES (tests behavior, not implementation)
```

**Structure Decision**: Existing structure retained. Changes are internal refactoring within Engine.fs, Handlers.fs, and test files.

## Complexity Tracking

| Aspect | Justification |
|--------|---------------|
| Engine modification | Constitution-protected component, but change is internal simplification, not behavioral |
| Callback pattern | Standard F# pattern; simpler than Rx subscriptions |

No violations requiring justification.

## Constitution Re-Check (Post Phase 1 Design)

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Functional-First F# | ✅ PASS | Callback pattern is pure functional; MailboxProcessor retained |
| II. Hypermedia Architecture | ✅ PASS | SSE/Datastar patterns unchanged |
| III. Test-First Development | ✅ PASS | Existing 602+ tests validate correctness |
| IV. Simplicity & Focus | ✅ PASS | Removes dependency, simplifies mental model |
| Protected Components | ✅ JUSTIFIED | Model.fs untouched; Engine.fs change is internal refactoring |

**Design Compliance Verified**: The callback-based design maintains all constitution principles while achieving the simplification goal.

## Generated Artifacts

| Artifact | Purpose |
|----------|---------|
| [research.md](./research.md) | Research findings and decisions |
| [data-model.md](./data-model.md) | Type changes and state transitions |
| [quickstart.md](./quickstart.md) | Implementation guide with code examples |
| [contracts/game-api.md](./contracts/game-api.md) | API contract for new Game interface |

## Next Steps

Run `/speckit.tasks` to generate implementation tasks from this plan.
