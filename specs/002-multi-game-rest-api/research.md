# Research: Multi-Game REST API

**Feature**: 002-multi-game-rest-api
**Date**: 2026-02-03

## Summary

All technical context items resolved. No NEEDS CLARIFICATION markers. Research focused on extending existing patterns to multi-game support.

## Decisions

### 1. Datastar Element Targeting for Multi-Game Updates

**Decision**: Use HTML `id` attributes with game-specific identifiers (e.g., `id="game-{gameId}"`) for SSE targeting.

**Rationale**: Datastar's `datastar-patch-elements` event morphs elements by matching top-level element IDs. Each game board rendered with a unique ID (`game-abc123`) allows SSE updates to target only that specific game without affecting others on the page.

**Alternatives Considered**:
- Custom `data-star-id` attribute: Not supported by Datastar; standard `id` is the mechanism
- Signal-based targeting: More complex; element ID matching is simpler and aligns with REST resource identity

**Implementation Pattern**:
```html
<!-- Each game gets unique ID -->
<div id="game-abc123">
    <!-- Game board content -->
</div>

<!-- SSE targets specific game -->
event: datastar-patch-elements
data: elements <div id="game-abc123">Updated content</div>
```

### 2. New Game Creation via SSE

**Decision**: Use `mode: append` with `selector: #games-container` to add new games to the page.

**Rationale**: Datastar supports append/prepend modes for adding elements without replacing existing content. New games appear at the end of the container.

**Implementation Pattern**:
```text
event: datastar-patch-elements
data: selector #games-container
data: mode append
data: elements <div id="game-newid">New game board</div>
```

### 3. Game Deletion via SSE

**Decision**: Use `RemoveElement` SSE event with CSS selector `#game-{id}`.

**Rationale**: Existing `SseBroadcast.fs` already supports `RemoveElement of selector: string`. The selector `#game-abc123` removes that specific game from all connected clients.

**Implementation Pattern**:
```fsharp
broadcast (RemoveElement $"#game-{gameId}")
```

### 4. Frank Routing with Path Parameters

**Decision**: Use ASP.NET Core route template syntax (`/games/{id}`) with `ctx.GetRouteValue("id")` for parameter extraction.

**Rationale**: Frank 6.4.0 is built on ASP.NET Core routing. Route parameters follow standard ASP.NET Core conventions.

**Implementation Pattern**:
```fsharp
let games =
    resource "/games" {
        name "Games"
        post createGame      // POST /games -> 201 + Location header
    }

let gameById =
    resource "/games/{id}" {
        name "GameById"
        get getGame          // GET /games/{id} -> game state
        post makeMove        // POST /games/{id} -> make move
        delete deleteGame    // DELETE /games/{id} -> remove game
    }

// In handler:
let getGame (ctx: HttpContext) =
    task {
        let gameId = ctx.GetRouteValue("id") |> string
        // ...
    }
```

### 5. SSE Broadcast Scope

**Decision**: Keep global broadcast; Datastar handles filtering via element ID matching.

**Rationale**:
- All clients receive all game updates
- Datastar only morphs elements with matching IDs
- Clients without a game element on page ignore updates for that game
- Simpler than maintaining per-game SSE channels

**Alternatives Considered**:
- Per-game SSE channels (`/sse/{gameId}`): More complex, requires managing multiple connections per client for multi-game page
- Filtered broadcasts: Requires tracking which games each client is viewing

### 6. Game Container Structure

**Decision**: Use a `#games-container` div on home page with individual game boards as children.

**Rationale**: Enables append/prepend for new games and clean removal for deleted games.

**Implementation Pattern**:
```html
<div id="games-container">
    <div id="game-abc123"><!-- Game 1 --></div>
    <div id="game-xyz789"><!-- Game 2 --></div>
</div>
```

### 7. Signal Namespacing per Game

**Decision**: Use game-scoped Datastar signals: `data-signals="{gameId: '{id}', player: '', position: ''}"`.

**Rationale**: Each game board carries its own game ID in signals. When a square is clicked, the POST includes the game ID, player, and position. No global signal conflicts.

**Implementation Pattern**:
```html
<button
    data-signals="{gameId: 'abc123', player: 'X', position: 'TopLeft'}"
    data-on:click="@post('/games/abc123')">
</button>
```

### 8. REST Response Semantics

**Decision**: Follow RFC 7231 HTTP semantics strictly.

| Action | Method | Path | Response |
|--------|--------|------|----------|
| Create game | POST | /games | 201 Created + Location: /games/{id} |
| Get game | GET | /games/{id} | 200 OK + HTML |
| Make move | POST | /games/{id} | 202 Accepted |
| Delete game | DELETE | /games/{id} | 204 No Content |

**Rationale**: Aligns with spec requirements (FR-002) and REST best practices.

## Dependencies

### Existing (No Changes Needed)

- **TicTacToe.Engine**: `GameSupervisor` already supports multi-game via `CreateGame()` returning `(id, game)` tuple and `GetGame(id)` for retrieval
- **System.Reactive**: `BehaviorSubject` for game state broadcasting
- **Frank.Datastar**: SSE primitives (`patchElements`, `removeElement`)

### Updated (Web Layer Only)

- **SseBroadcast.fs**: Add helper for append mode
- **Handlers.fs**: New handlers for REST endpoints
- **templates/game.fs**: Update for game-scoped IDs and signals
- **Program.fs**: Add `/games` and `/games/{id}` resources

## Testing Strategy

### Unit Tests (Expecto)
- Existing Engine tests cover game logic (no changes)

### Integration Tests (Playwright + NUnit)
- Test REST endpoint responses (201, 204, 404)
- Test Location header format
- Test multi-game interactions
- Test SSE updates for specific games
- Test game deletion removes from page

## Risks & Mitigations

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| SSE broadcast volume with many games | Low | Medium | Datastar filters by ID; only relevant updates processed |
| Route parameter parsing errors | Low | Low | Validate game ID format, return 404 for invalid |
| Race conditions on game creation | Low | Low | MailboxProcessor ensures thread-safe state |
