# API Contracts: Game Reset and Persistent Game Boards

**Feature**: 006-game-reset | **Date**: 2026-02-05

## Overview

This document describes the REST API changes for the game reset feature. All endpoints require cookie-based authentication.

## Existing Endpoints (Modified Behavior)

### DELETE /games/{id}

**Description**: Remove a game board entirely.

**Changes**:
- Now validates game count > 6 before allowing deletion
- Returns 409 Conflict if deletion would reduce count below 6
- Validates user is an assigned player (X or O)

**Request**:
```
DELETE /games/{id}
Cookie: .AspNetCore.Cookies=<auth-cookie>
```

**Responses**:

| Status | Condition | Body |
|--------|-----------|------|
| 204 No Content | Game deleted successfully | (none) |
| 401 Unauthorized | No valid auth cookie | (none) |
| 403 Forbidden | User not assigned as X or O | (none) |
| 404 Not Found | Game does not exist | (none) |
| 409 Conflict | Deletion would reduce count below 6 | (none) |

**SSE Broadcast**: On success, broadcasts `RemoveElement` with selector `#game-{id}`

---

## New Endpoints

### POST /games/{id}/reset

**Description**: Reset a game board, creating a fresh game in the same visual position.

**Request**:
```
POST /games/{id}/reset
Cookie: .AspNetCore.Cookies=<auth-cookie>
```

**Responses**:

| Status | Condition | Body |
|--------|-----------|------|
| 200 OK | Game reset successfully | (none) |
| 401 Unauthorized | No valid auth cookie | (none) |
| 403 Forbidden | User not assigned as X or O, or game has no moves/players | (none) |
| 404 Not Found | Game does not exist | (none) |

**Response Headers** (on 200):
```
Location: /games/{new-game-id}
```

**SSE Broadcast**: On success, broadcasts `PatchElements` replacing old game board with new game board HTML.

**Side Effects**:
1. Old game is disposed
2. Player assignments for old game are cleared
3. New game is created and subscribed to SSE
4. New game starts in XTurn state with empty board
5. All connected clients receive updated game board

---

## Existing Endpoints (No Changes)

### GET /
Home page (requires auth). No changes.

### GET /sse
SSE connection for real-time updates. No changes to endpoint, but:
- On connect, now receives 6 initial game boards (instead of empty container)
- Game boards include Reset and Delete buttons with appropriate enabled/disabled state

### POST /games
Create new game. No changes.

### GET /games/{id}
View specific game. No changes.

### POST /games/{id}
Make a move. No changes.

---

## SSE Events

### Existing Events (No Changes)

#### PatchElements
```
event: datastar-fragment
data: fragment <html-content>
data: selector <css-selector>
data: patchMode morph
```

#### PatchElementsAppend
```
event: datastar-fragment
data: fragment <html-content>
data: selector <css-selector>
data: patchMode append
```

#### RemoveElement
```
event: datastar-remove
data: selector <css-selector>
```

### Event Usage for Reset Feature

**Reset Action**:
1. `PatchElements` with selector `#game-{oldId}` and content of new game board (with `id="game-{newId}"`)

**Delete Action**:
1. `RemoveElement` with selector `#game-{id}`
2. `PatchElements` for all remaining game boards (to update delete button state)

**Initial Load**:
1. `PatchElements` for games container with 6 game boards

---

## Route Registration

Current routes in Program.fs:
```fsharp
route "/"                 GET  (requiresAuth home)
route "/sse"              GET  sse
route "/games"            POST createGame
route "/games/{id}"       [ GET; POST; DELETE ] (getGame, makeMove, deleteGame)
```

New routes:
```fsharp
route "/games/{id}/reset" POST (requiresAuth resetGame)
```

---

## Button Actions (Datastar Attributes)

### Reset Button
```html
<button class="reset-game-btn"
        type="button"
        data-on:click="@post('/games/{gameId}/reset')"
        disabled="{disabled-if-not-allowed}">
    Reset Game
</button>
```

### Delete Button
```html
<button class="delete-game-btn"
        type="button"
        data-on:click="@delete('/games/{gameId}')"
        disabled="{disabled-if-not-allowed}">
    Delete Game
</button>
```

---

## Error Handling

All error responses follow existing patterns:
- HTTP status code indicates error type
- No response body (follows existing convention)
- Client handles via Datastar error events or page refresh

---

## Authentication

All game mutation endpoints require authentication:
- `/games/{id}/reset` - wrapped with `requiresAuth`
- `/games/{id}` DELETE - should verify auth (update existing handler)

Authentication verified via:
1. Cookie present and valid
2. Claims transformation adds/updates user ID claim
3. Handler retrieves user ID via `ctx.GetUserId()`
