# Implementation Plan: Player Identity & Legend Display

**Branch**: `007-player-identity-legend` | **Date**: 2026-02-05 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/007-player-identity-legend/spec.md`

## Summary

Add three UI elements to the tic-tac-toe application: (1) display the current user's 8-character GUID prefix in the top-right corner of the page layout, (2) add a player legend below each game board showing which player is X and which is O, and (3) bold the active player's legend entry to indicate whose turn it is. All rendering is server-side, with legends included in SSE broadcast updates by passing `PlayerAssignment` data to the broadcast rendering path.

## Technical Context

**Language/Version**: F# targeting .NET 10.0
**Primary Dependencies**: Frank 6.4.0, Frank.Datastar 6.4.0, Oxpecker.ViewEngine 1.1.0
**Storage**: In-memory via MailboxProcessor (existing GameSupervisor/PlayerAssignmentManager pattern)
**Testing**: Expecto (unit), Playwright with NUnit (web/integration)
**Target Platform**: Web application (server-rendered hypermedia)
**Project Type**: Web (single F# project with Engine library)
**Performance Goals**: Legend updates visible within 1 second of a move (via existing SSE infrastructure)
**Constraints**: No Engine modifications (protected component). No client-side JavaScript beyond Datastar runtime.
**Scale/Scope**: Small feature — 4 files modified, ~100 lines of new code

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
| --- | --- | --- |
| I. Functional-First F# | PASS | All new logic is pure functions (legend rendering, ID truncation). No mutable state added. |
| II. Hypermedia Architecture | PASS | All rendering server-side. Legend embedded in HTML fragments sent via SSE. No client-side JS. |
| III. Test-First Development | PASS | Tests will be written before implementation per workflow. |
| IV. Simplicity & Focus | PASS | No new types, no new endpoints, no new dependencies. Legend is derived data rendered inline. |
| Protected: TicTacToe.Engine | PASS | No Engine changes. All modifications in Web layer. |

**Post-Phase 1 Re-check**: All gates still pass. No new abstractions, patterns, or dependencies introduced. Signature changes: adding a parameter to `renderGameBoardForBroadcast` and `subscribeToGame`, and converting `PlayerAssignmentManager` from async to sync API (simplification, not added complexity).

## Project Structure

### Documentation (this feature)

```text
specs/007-player-identity-legend/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── rendering-contracts.md
├── checklists/
│   └── requirements.md
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── TicTacToe.Engine/        # PROTECTED - no changes
│   ├── Engine.fs
│   └── Model.fs
└── TicTacToe.Web/
    ├── Auth.fs              # No changes (TryGetUserId already exists)
    ├── Model.fs             # MODIFY: convert PlayerAssignmentManager methods from async (PostAndAsyncReply) to sync (PostAndReply)
    ├── Handlers.fs          # MODIFY: pass assignmentManager to subscribeToGame; update broadcast calls; remove Async.StartAsTask from assignment manager call sites
    ├── SseBroadcast.fs      # No changes
    └── templates/
        ├── shared/
        │   └── layout.fs    # MODIFY: add user identity display in header
        ├── home.fs          # No changes
        └── game.fs          # MODIFY: add legend rendering; update renderGameBoardForBroadcast signature; add CSS

tests/
└── TicTacToe.Web.Tests/     # ADD: Playwright tests for identity display and legend
```

**Structure Decision**: Existing single-project structure with Engine library. No new files created in `src/`; all changes are modifications to existing files. Test files added in existing test project.

## Complexity Tracking

No violations to justify. Feature requires zero new abstractions, types, or dependencies.
