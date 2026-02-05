# Implementation Plan: Multi-Player Tic-Tac-Toe

**Branch**: `005-multi-player` | **Date**: 2026-02-04 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/005-multi-player/spec.md`

## Summary

Enable two distinct users to play tic-tac-toe against each other with automatic player assignment. The first user to make a move becomes Player X, the second becomes Player O. Once both players are assigned, only those two users can make moves (others become spectators). Player identification uses the existing Auth module's GUID-based cookies.

**Technical Approach**: Add player assignment tracking at the Web layer (respecting the Engine's protected status). The GameRef in GameSupervisor will be extended to track player-to-role mappings. Move validation in Handlers.fs will enforce that only the assigned player for the current turn can make moves.

## Technical Context

**Language/Version**: F# targeting .NET 10.0
**Primary Dependencies**: Frank 6.4.0, Frank.Datastar 6.4.0, Oxpecker.ViewEngine 1.1.0
**Storage**: In-memory via MailboxProcessor (existing GameSupervisor pattern)
**Testing**: Expecto (unit), Playwright with NUnit (web/integration)
**Target Platform**: Web application (ASP.NET Core)
**Project Type**: Web application with server-side rendering
**Performance Goals**: Game state consistent across viewers within 2 seconds (SC-004)
**Constraints**: No modification to TicTacToe.Engine (protected per constitution)
**Scale/Scope**: Single game instance per URL, multiple concurrent games supported

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Requirement | Status | Notes |
|-----------|-------------|--------|-------|
| I. Functional-First F# | Pure functions, immutable data, MailboxProcessor for state | ✅ PASS | Player assignments stored in MailboxProcessor state |
| II. Hypermedia Architecture | Server-side rendering, SSE for state changes, no custom JS | ✅ PASS | Rejection feedback via CSS animation (no JS) |
| III. Test-First Development | Tests before implementation | ✅ PASS | Will follow TDD workflow |
| IV. Simplicity & Focus | YAGNI, no premature abstractions | ✅ PASS | Minimal changes to existing architecture |
| Protected: TicTacToe.Engine | DO NOT MODIFY | ✅ PASS | All changes at Web layer only |

**Gate Status**: PASSED - Proceed to Phase 0

## Project Structure

### Documentation (this feature)

```text
specs/005-multi-player/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (via /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── TicTacToe.Engine/    # PROTECTED - DO NOT MODIFY
│   ├── Model.fs         # Game domain types
│   └── Engine.fs        # Game actor, supervisor
│
└── TicTacToe.Web/       # MODIFICATION TARGET
    ├── Program.fs       # App config (ADD PlayerAssignmentManager DI registration)
    ├── Auth.fs          # User identification (no changes needed - already complete)
    ├── Handlers.fs      # HTTP handlers (ADD player validation)
    ├── Model.fs         # NEW: PlayerAssignment types
    ├── SseBroadcast.fs  # SSE broadcasting (no changes needed)
    └── templates/
        └── game.fs      # Game board rendering (ADD rejection animation CSS)

test/
├── TicTacToe.Engine.Tests/  # Existing tests (no changes)
└── TicTacToe.Web.Tests/
    ├── MultiPlayerTests.fs  # NEW: Multi-player integration tests
    └── PlayerAssignmentTests.fs  # NEW: Unit tests for assignment logic
```

**Structure Decision**: Single web project with player assignment logic added to the Web layer. No new projects needed. The Engine remains untouched per constitution.

## Complexity Tracking

No violations requiring justification. Design uses existing patterns (MailboxProcessor) and adds minimal new code at Web layer only.

## Post-Design Constitution Re-Check

*Re-evaluated after Phase 1 design completion.*

| Principle | Post-Design Status | Verification |
|-----------|-------------------|--------------|
| I. Functional-First F# | ✅ PASS | PlayerAssignment uses MailboxProcessor; validation functions are pure |
| II. Hypermedia Architecture | ✅ PASS | Rejection uses CSS animation only; role communicated via Datastar signals |
| III. Test-First Development | ✅ READY | Test files identified in structure; TDD workflow to be followed |
| IV. Simplicity & Focus | ✅ PASS | One new file (Model.fs), modifications to two existing files only |
| Protected: TicTacToe.Engine | ✅ PASS | Zero changes to Engine; all logic at Web layer |

**Final Gate Status**: PASSED - Ready for task generation

## Generated Artifacts

| Artifact | Path | Status |
|----------|------|--------|
| Implementation Plan | `specs/005-multi-player/plan.md` | ✅ Complete |
| Research | `specs/005-multi-player/research.md` | ✅ Complete |
| Data Model | `specs/005-multi-player/data-model.md` | ✅ Complete |
| API Contracts | `specs/005-multi-player/contracts/api.md` | ✅ Complete |
| Quickstart Guide | `specs/005-multi-player/quickstart.md` | ✅ Complete |
| Tasks | `specs/005-multi-player/tasks.md` | ⏳ Pending (`/speckit.tasks`) |

## Next Steps

Run `/speckit.tasks` to generate the implementation task list.
