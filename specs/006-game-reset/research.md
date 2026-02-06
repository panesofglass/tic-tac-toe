# Research: Game Reset and Persistent Game Boards

**Feature**: 006-game-reset | **Date**: 2026-02-05

## Technical Decisions

### 1. Reset Implementation Strategy

**Decision**: Replace game in-place using existing PatchElements SSE broadcast

**Rationale**:
- The existing `PatchElements` SSE event can replace DOM elements by ID
- Game boards are rendered with `id="game-{gameId}"`
- Reset creates new game, renders it with SAME ID as old game, broadcasts replacement
- All connected clients receive the update simultaneously

**Alternatives Considered**:
- `RemoveElement` + `PatchElementsAppend`: Would cause visual flicker as element disappears and reappears
- Custom SSE event type: Unnecessary complexity; existing events sufficient

### 2. Game Count Tracking for Delete Logic

**Decision**: Use `GameSupervisor.GetActiveGameCount()` method (already exists)

**Rationale**:
- The GameSupervisor already exposes `GetActiveGameCount(): int`
- This provides the authoritative count of active games
- No new state tracking needed

**Alternatives Considered**:
- Track count separately in Web layer: Would create duplicate state and sync issues
- Count DOM elements client-side: Would be inconsistent across clients

### 3. Initial Six Games Creation

**Decision**: Create games during application startup in Program.fs using `IHostApplicationLifetime`

**Rationale**:
- ASP.NET Core provides `IHostApplicationLifetime.ApplicationStarted` event
- Games can be created after DI container is fully built
- Ensures games exist before any client connects

**Alternatives Considered**:
- Create on first SSE connection: Would cause race conditions with multiple first clients
- Lazy initialization: Would delay first page load and create inconsistent experience

### 4. Button Enable/Disable Logic

**Decision**: Pass player role and game count to template; render disabled attribute conditionally

**Rationale**:
- Template already receives `gameId` and `result` (game state)
- Add parameters for `userRole: PlayerRole` and `gameCount: int`
- Render `disabled` attribute based on conditions
- Keep logic in F# templates, not JavaScript

**Alternatives Considered**:
- Client-side JavaScript logic: Violates hypermedia architecture principle
- Separate enabled/disabled button templates: Unnecessary duplication

### 5. Reset Authorization

**Decision**: Validate user is assigned player (X or O) in handler before allowing reset

**Rationale**:
- Handler already has access to `PlayerAssignmentManager`
- Can call `GetRole(gameId, userId)` to check authorization
- Return 403 Forbidden if user is Spectator or Unassigned

**Alternatives Considered**:
- Rely only on button disabling: Client could still send POST request directly
- No authorization: Would allow any user to reset any game

### 6. Race Condition Handling

**Decision**: Use MailboxProcessor serialization to handle concurrent reset/delete requests

**Rationale**:
- GameSupervisor uses MailboxProcessor which serializes all messages
- If game is already disposed when second request arrives, return 404
- No explicit locking needed; actor model handles concurrency

**Alternatives Considered**:
- Explicit mutex/lock: Would add complexity and potential deadlocks
- Optimistic concurrency tokens: Overkill for this use case

### 7. Game Position Preservation During Reset

**Decision**: New game inherits visual position by using same container slot, NOT same ID

**Rationale**:
- The spec requires "same visual position" but games are identified by unique GUIDs
- New game gets new ID (from `supervisor.CreateGame()`)
- Position preserved by replacing the entire `<div id="game-{oldId}">` with `<div id="game-{newId}">`
- Use `PatchElements` targeting the old game's container

**Alternatives Considered**:
- Reuse same game ID: Would require modifying Engine (protected component)
- Track "slot positions": Adds unnecessary complexity; DOM order is sufficient

### 8. Delete vs Reset Endpoint Design

**Decision**: Keep DELETE /games/{id} for delete; add POST /games/{id}/reset for reset

**Rationale**:
- DELETE already exists and follows REST conventions
- POST for reset is appropriate (creates new resource as side effect)
- Separate endpoints make authorization logic clearer
- Datastar can use `@post('/games/{id}/reset')` and `@delete('/games/{id}')`

**Alternatives Considered**:
- PATCH /games/{id} with action body: Less clear semantics
- Single endpoint with query param: Harder to reason about

## Patterns from Existing Code

### SSE Broadcast Pattern
```fsharp
// Current delete pattern
broadcast (RemoveElement $"#game-{gameId}")

// Reset pattern (new)
let newHtml = renderGameBoard newGameId newGame.GetState() userRole gameCount
broadcast (PatchElements newHtml)

// Must also update other clients' views of ALL games (for delete button state)
// When game count changes, all game boards need re-render
```

### Handler Authorization Pattern
```fsharp
// Current auth check in makeMove handler
let userId = ctx.GetUserId()
let! result = assignmentManager.TryAssignAndValidate(gameId, userId, isXTurn)
match result with
| Ok _ -> // proceed
| Rejected reason -> // return error

// Apply same pattern to reset handler
```

### Template Conditional Rendering
```fsharp
// Current move button pattern
let isClickable = validMoves |> Array.contains position && currentPlayer.IsSome
if isClickable then
    button(class' = "square square-clickable", ...) { ... }
else
    button(class' = "square", disabled = "disabled", ...) { ... }

// Apply same pattern to reset/delete buttons
```

## Open Questions Resolved

| Question | Resolution |
|----------|------------|
| How to maintain visual position? | Replace DOM element by selector, new game takes old game's visual slot |
| How to track game count? | Use existing `GameSupervisor.GetActiveGameCount()` |
| Where to create initial games? | Application startup via `IHostApplicationLifetime.ApplicationStarted` |
| How to handle concurrent resets? | MailboxProcessor serialization handles it |
| Which SSE event for reset? | `PatchElements` to replace entire game board |

## Dependencies Verified

- `GameSupervisor.GetActiveGameCount()` - Already exists in Engine.fs
- `PlayerAssignmentManager.GetRole()` - Already exists in Web/Model.fs
- `PatchElements` SSE event - Already exists in SseBroadcast.fs
- `IHostApplicationLifetime` - Available via ASP.NET Core DI

## Risk Assessment

| Risk | Mitigation |
|------|------------|
| Race condition on reset | MailboxProcessor serializes requests; check game exists before reset |
| User confusion about Reset vs Delete | Clear visual distinction; Delete only shown when >6 games |
| Performance with 6+ games on load | Existing SSE pattern handles well; games render incrementally |
| Button state out of sync | Re-render all game boards when count changes via SSE |
