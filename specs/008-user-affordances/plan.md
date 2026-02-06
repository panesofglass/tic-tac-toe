# Implementation Plan: User-Specific Affordances

**Branch**: `008-user-affordances` | **Date**: 2026-02-06 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/008-user-affordances/spec.md`

## Summary

Replace the current generic SSE broadcast (same HTML to all subscribers) with per-role server rendering. Each connected user receives game board HTML tailored to their PlayerRole and the game's current state. The SSE subscriber model gains user identity, enabling the broadcast mechanism to look up each subscriber's role per game and deliver the appropriate view. The existing `renderGameBoardWithContext` function already supports role-aware rendering; the key change is wiring it into the broadcast path and SSE initial load.

## Technical Context

**Language/Version**: F# targeting .NET 10.0
**Primary Dependencies**: Frank 6.5.0, Frank.Datastar 6.5.0, Oxpecker.ViewEngine 1.1.0
**Storage**: In-memory via MailboxProcessor (GameSupervisor, PlayerAssignmentManager)
**Testing**: Expecto (unit tests), Playwright + NUnit (web integration tests)
**Target Platform**: ASP.NET Core web server (Kestrel)
**Project Type**: Web application (server-rendered hypermedia)
**Performance Goals**: Per-role rendering — at most 4 distinct renders per game per state change, independent of subscriber count
**Constraints**: No custom client-side JavaScript (Datastar runtime only); server is single source of truth for UI affordances
**Scale/Scope**: Unbounded game boards, small concurrent viewer count per game

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Research Check

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Functional-First F# | PASS | All changes are in F# using functional patterns. MailboxProcessor used for PlayerAssignmentManager (existing). No new mutable state beyond extending subscriber record. |
| II. Hypermedia Architecture | PASS | Core purpose of this feature — server controls which affordances each user sees. No client-side JS for affordance toggling. SSE delivers role-appropriate HTML. |
| III. Test-First Development | PASS | New Playwright tests for role-specific rendering. Existing Expecto tests for engine unchanged. |
| IV. Simplicity & Focus | PASS | Minimal structural changes: extend subscriber with userId, replace broadcast calls with role-aware dispatch, reuse existing `renderGameBoardWithContext`. No new frameworks or abstractions. |
| Protected: TicTacToe.Engine | PASS | No Engine changes required. All changes in TicTacToe.Web. PlayerRole, PlayerAssignment, and game state types remain unchanged. |

### Post-Design Check

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Functional-First F# | PASS | Subscriber model change is a simple record/tuple extension. Role-aware broadcast uses pattern matching and function composition. No OOP patterns introduced. |
| II. Hypermedia Architecture | PASS | This feature is the quintessential hypermedia principle — HTML responses carry affordances appropriate to the user's authorization. Datastar SSE protocol unchanged. |
| III. Test-First Development | PASS | Plan includes new test scenarios for each role variant (active player, waiting player, observer, unassigned claimable, threshold management buttons). |
| IV. Simplicity & Focus | PASS | Reuses existing `renderGameBoardWithContext` which already takes PlayerRole and gameCount. The broadcast function grows by ~20 lines. No new projects, no new dependencies. |
| Protected: TicTacToe.Engine | PASS | Zero changes to Engine. |

## Project Structure

### Documentation (this feature)

```text
specs/008-user-affordances/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0: research findings
├── data-model.md        # Phase 1: entity/affordance matrix
├── quickstart.md        # Phase 1: dev setup guide
├── contracts/
│   └── sse-events.md    # Phase 1: SSE event contract changes
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── TicTacToe.Engine/          # NO CHANGES (protected)
│   ├── Model.fs               # Game types, MoveResult
│   └── Engine.fs              # Game actor, GameSupervisor
│
└── TicTacToe.Web/             # ALL CHANGES HERE
    ├── Model.fs               # PlayerRole, PlayerAssignmentManager (no structural changes)
    ├── Auth.fs                # Authentication, claims (no changes)
    ├── SseBroadcast.fs        # MODIFY: subscriber identity, role-aware broadcast
    ├── templates/
    │   ├── game.fs            # MODIFY: unify rendering, retire renderGameBoardForBroadcast
    │   ├── home.fs            # No changes expected
    │   └── shared/layout.fs   # No changes expected
    ├── Handlers.fs            # MODIFY: SSE initial load, game subscription broadcasts
    └── Program.fs             # No changes expected

test/
├── TicTacToe.Engine.Tests/    # No changes
└── TicTacToe.Web.Tests/       # ADD: role-specific affordance tests
    ├── TestBase.fs             # May need helpers for multi-user role testing
    └── AffordanceTests.fs     # NEW: tests for per-role rendering
```

**Structure Decision**: Existing single web project structure. No new projects, files, or dependencies. Changes concentrated in 3 source files (SseBroadcast.fs, game.fs, Handlers.fs) plus new test file.

## Design Decisions

### D1: Subscriber Model Extension

**Current** (SseBroadcast.fs):
```fsharp
let private subscribers = ResizeArray<Channel<SseEvent>>()
```

**New**:
```fsharp
let private subscribers = ResizeArray<string * Channel<SseEvent>>()
// (userId, channel) pairs
```

The `subscribe` function gains a `userId: string` parameter. The `unsubscribe` function matches by channel reference (unchanged). This is the minimal change to enable per-subscriber routing.

### D2: Role-Aware Broadcast Function

New function alongside existing `broadcast`:

```fsharp
let broadcastPerRole
    (gameId: string)
    (assignmentManager: PlayerAssignmentManager)
    (gameCount: int)
    (renderForRole: string -> PlayerRole -> PlayerAssignment option -> int -> string)
    (assignment: PlayerAssignment option) : unit
```

Logic:
1. Group subscribers by their role for this game (via `assignmentManager.GetRole(gameId, userId)`)
2. Render once per distinct role group
3. Send the matching rendered HTML to each subscriber's channel

This avoids rendering N times for N subscribers — only renders per distinct role.

### D3: Rendering Unification

Retire `renderGameBoardForBroadcast` and use `renderGameBoardWithContext` for all paths:
- SSE initial game population (Handlers.fs lines 154-163)
- Game state change broadcasts (subscribeToGame OnNext, Handlers.fs lines 48-55)
- Game creation broadcasts (Handlers.fs lines 193-202)
- Game reset broadcasts (Handlers.fs lines 364-389)

The `renderGameBoardWithContext` already accepts `PlayerRole`, `PlayerAssignment option`, and `gameCount` — all the parameters needed for role-specific rendering.

### D4: Targeted Rejection Signals

Currently, rejection animations (PatchSignals) broadcast to ALL subscribers. With user-aware subscribers, send rejection signals only to the specific user who triggered the rejected action. New function:

```fsharp
let sendToUser (userId: string) (event: SseEvent) : unit
```

### D5: SSE Initial Load Personalization

The `sse` handler (Handlers.fs line 143) has `ctx.User.TryGetUserId()`. During the initial game iteration:
1. Extract userId from context
2. For each game: `assignmentManager.GetRole(gameId, userId)` → role
3. Render with `renderGameBoardWithContext gameId result role assignment gameCount`
4. Send personalized HTML to this specific connection

This is simpler than the broadcast path because the SSE handler already has the HttpContext.

### D6: Game Count Propagation

`renderGameBoardWithContext` needs `gameCount` to determine delete button visibility. The `GameSupervisor.GetActiveGameCount()` call is cheap (Map.count on MailboxProcessor state). Pass it through the broadcast function so each render has the current count.

## Complexity Tracking

No constitution violations to justify. All changes follow existing patterns:
- Functional F# with pattern matching
- MailboxProcessor for concurrent state (existing PlayerAssignmentManager)
- Server-side rendering via Oxpecker view engine (existing templates)
- SSE via Datastar protocol (existing SseBroadcast)
