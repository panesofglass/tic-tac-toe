# Implementation Plan: Game Reset and Persistent Game Boards

**Branch**: `006-game-reset` | **Date**: 2026-02-05 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/006-game-reset/spec.md`

## Summary

Replace the "Delete Game" button with "Reset Game" functionality that creates a new game in the same visual position. Launch the application with six pre-created games and maintain a minimum of six games at all times. Add a conditional Delete button that only activates when more than six games exist.

## Technical Context

**Language/Version**: F# targeting .NET 10.0
**Primary Dependencies**: Frank 6.4.0, Frank.Datastar 6.4.0, Oxpecker.ViewEngine 1.1.0
**Storage**: In-memory via MailboxProcessor (GameSupervisor pattern)
**Testing**: Expecto (unit), Playwright + NUnit (integration)
**Target Platform**: ASP.NET Core web server
**Project Type**: Web application (server-rendered hypermedia)
**Performance Goals**: Page load with 6 games < 2 seconds; Reset action < 1 second
**Constraints**: Maintain minimum 6 games; prevent race conditions on concurrent reset/delete
**Scale/Scope**: Multiple concurrent games, multiple connected users per game

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Functional-First F# | ✅ PASS | Using existing MailboxProcessor patterns for state management |
| II. Hypermedia Architecture | ✅ PASS | SSE broadcasting for real-time updates, server-rendered HTML |
| III. Test-First Development | ✅ PASS | Will write Playwright tests before implementation |
| IV. Simplicity & Focus | ✅ PASS | Extends existing patterns, no new dependencies |
| Protected: TicTacToe.Engine | ✅ PASS | No modifications to Engine required; changes only in Web layer |

**Gate Result**: PASS - No violations. Proceed to Phase 0.

## Project Structure

### Documentation (this feature)

```text
specs/006-game-reset/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── api.md           # REST API changes
└── tasks.md             # Phase 2 output (from /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── TicTacToe.Engine/         # NO CHANGES - Protected component
│   ├── Engine.fs             # GameSupervisor, Game actor
│   └── Model.fs              # Game state, moves
└── TicTacToe.Web/
    ├── Handlers.fs           # MODIFY: Add resetGame, update deleteGame
    ├── Model.fs              # MODIFY: Track game count for delete logic
    ├── Program.fs            # MODIFY: Add reset route, startup game creation
    ├── SseBroadcast.fs       # NO CHANGES - existing patterns sufficient
    └── templates/
        ├── game.fs           # MODIFY: Reset/Delete buttons with enable/disable logic
        └── home.fs           # NO CHANGES

test/
├── TicTacToe.Engine.Tests/   # NO CHANGES
└── TicTacToe.Web.Tests/
    ├── ResetGameTests.fs     # NEW: Reset functionality tests
    ├── InitialGamesTests.fs  # NEW: Six-game startup tests
    └── MultiGameTests.fs     # MODIFY: Update existing delete tests
```

**Structure Decision**: Web-only changes following existing architecture. Engine remains protected and unchanged. All new logic in Web handlers and templates.

## Complexity Tracking

> No constitution violations to justify.

| Item | Justification |
|------|---------------|
| None | All changes align with existing patterns |

## Constitution Re-Check (Post-Design)

| Principle | Status | Verification |
|-----------|--------|--------------|
| I. Functional-First F# | ✅ PASS | All new code uses MailboxProcessor patterns, pure functions, immutable data |
| II. Hypermedia Architecture | ✅ PASS | Reset uses SSE broadcasting; no client-side JS added |
| III. Test-First Development | ✅ PASS | Playwright tests defined before implementation |
| IV. Simplicity & Focus | ✅ PASS | No new dependencies; extends existing patterns |
| Protected: TicTacToe.Engine | ✅ PASS | Engine unchanged; all changes in Web layer |

**Post-Design Gate Result**: PASS - Design complies with all principles.

## Generated Artifacts

| Artifact | Path | Description |
|----------|------|-------------|
| Research | `specs/006-game-reset/research.md` | Technical decisions and patterns |
| Data Model | `specs/006-game-reset/data-model.md` | Entity definitions and state transitions |
| API Contracts | `specs/006-game-reset/contracts/api.md` | REST API changes |
| Quickstart | `specs/006-game-reset/quickstart.md` | Implementation guide |

## Next Steps

Run `/speckit.tasks` to generate the implementation task list.

