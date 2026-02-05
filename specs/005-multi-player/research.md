# Research: Multi-Player Tic-Tac-Toe

**Date**: 2026-02-04
**Feature**: 005-multi-player

## Research Questions

### 1. Where to Store Player Assignments Without Modifying Engine?

**Decision**: Create a `PlayerAssignmentManager` at the Web layer using MailboxProcessor pattern.

**Rationale**:
- The Engine is protected per constitution and cannot be modified
- The Web layer already has access to user identity via Auth module
- MailboxProcessor is the established pattern for concurrent state (per constitution Principle I)
- Player assignments are a Web concern (HTTP session/cookie based), not game logic

**Alternatives Considered**:
- Modify GameRef in Engine's GameSupervisor: Rejected - violates Engine protection
- Store in separate database: Rejected - YAGNI, in-memory is sufficient for game duration
- Use ASP.NET Core session: Rejected - adds complexity, MailboxProcessor is idiomatic F#

### 2. How to Integrate with Existing Move Flow?

**Decision**: Add validation in `Handlers.makeMove` before calling `game.MakeMove()`.

**Rationale**:
- Handler already extracts user ID via `ctx.User.TryGetUserId()`
- Validation at handler level keeps Engine untouched
- Rejection returns before Engine sees invalid moves (clean separation)

**Current Flow** (Handlers.fs lines 131-154):
```
POST /games/{id} → parseSignals → Move.TryParse → game.MakeMove → broadcast
```

**New Flow**:
```
POST /games/{id} → parseSignals → getUserId → validatePlayerAssignment → Move.TryParse → game.MakeMove → broadcast
                                      ↓ (if rejected)
                                 broadcast rejection animation
```

**Alternatives Considered**:
- Middleware validation: Rejected - too generic, this is game-specific
- Engine-level validation: Rejected - violates Engine protection

### 3. How to Handle First-Move Player Assignment?

**Decision**: Assign player role on first successful move attempt, not on game view.

**Rationale**:
- Spec states "first player to make a move becomes X"
- Viewing a game doesn't commit to playing
- Assignment happens atomically with move validation

**Assignment Logic**:
```fsharp
match (currentTurn, playerXId, playerOId, userId) with
| (XTurn, None, _, _) -> Assign userId as X, allow move
| (OTurn, Some _, None, userId) when userId <> playerXId -> Assign userId as O, allow move
| (XTurn, Some x, _, userId) when userId = x -> Allow move (X's turn, X is moving)
| (OTurn, _, Some o, userId) when userId = o -> Allow move (O's turn, O is moving)
| _ -> Reject (wrong player or unauthorized)
```

**Alternatives Considered**:
- Assign on game creation: Rejected - contradicts spec "first to move"
- Assign on page load: Rejected - viewing ≠ commitment to play

### 4. How to Implement Rejection Feedback Without JavaScript?

**Decision**: CSS animation triggered by Datastar attribute, broadcast via SSE.

**Rationale**:
- Constitution Principle II prohibits custom JavaScript
- CSS `@keyframes` with shake/flash animation is pure CSS
- SSE can broadcast a temporary class to trigger animation
- Datastar's `data-*` attributes can toggle animation state

**Implementation Approach**:
```css
@keyframes shake {
  0%, 100% { transform: translateX(0); }
  25% { transform: translateX(-5px); }
  75% { transform: translateX(5px); }
}
.rejected { animation: shake 0.3s ease-in-out; }
```

**Broadcast**: Send `PatchSignals` with `rejected: true`, CSS handles the rest.

**Alternatives Considered**:
- JavaScript shake: Rejected - violates Principle II
- Alert/modal: Rejected - spec says "no modal or alert"
- No feedback: Rejected - spec requires visual indication

### 5. How to Determine Current Turn from Web Layer?

**Decision**: Query game state from Engine before processing move.

**Rationale**:
- Engine exposes `GetState` message via MailboxProcessor
- Current turn is derivable from `MoveResult` (XTurn vs OTurn)
- No Engine modification needed - just read current state

**Implementation**:
```fsharp
let! currentState = game.GetState() // Returns MoveResult
match currentState with
| XTurn _ -> // X's turn
| OTurn _ -> // O's turn
| Won _ | Draw _ -> // Game over, reject move
```

**Alternatives Considered**:
- Track turn separately: Rejected - duplicates Engine state, risks inconsistency
- Count moves: Rejected - fragile, Engine is source of truth

### 6. How to Handle Spectator View?

**Decision**: No changes needed - existing SSE broadcast already supports multiple subscribers.

**Rationale**:
- Game's IObservable pattern broadcasts to all subscribers
- SseBroadcast.fs already handles multiple clients
- Spectators simply don't make moves - UI can hide move buttons for non-players

**Implementation**:
- On page load, check if user is assigned player X, O, or neither
- If neither and both slots filled → spectator mode (view only)
- Datastar can conditionally show/hide move controls based on role

**Alternatives Considered**:
- Separate spectator endpoint: Rejected - YAGNI, same broadcast works
- Different SSE channel: Rejected - unnecessary complexity

## Existing Code Assets

| Asset | Location | Reuse Strategy |
|-------|----------|----------------|
| User ID extraction | Auth.fs `TryGetUserId()` | Call in Handlers.makeMove |
| Cookie config | Program.fs (30-day, HttpOnly) | No changes needed |
| MailboxProcessor pattern | Engine.fs | Copy pattern for PlayerAssignmentManager |
| SSE broadcast | SseBroadcast.fs | Use for rejection animation |
| Game state query | Engine.fs `GetState` message | Use to determine current turn |

## Key Design Decisions Summary

1. **Web layer only** - Engine remains untouched
2. **MailboxProcessor for assignments** - Follows constitution, thread-safe
3. **Validate in handler** - Before Engine sees the move
4. **CSS animation for rejection** - No custom JS per Principle II
5. **Assign on first move** - Not on view, per spec
6. **Spectators via existing SSE** - No new infrastructure

## Open Questions Resolved

All NEEDS CLARIFICATION items from spec have been addressed:
- ✅ Player ID generation: Existing Auth module (GUID via `Guid.NewGuid()`)
- ✅ Rejection feedback: CSS animation (shake/flash)

## Next Phase

Proceed to Phase 1: Generate data-model.md, contracts/, quickstart.md
