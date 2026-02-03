# API Contracts: Web Frontend Single Game

**Feature**: 001-web-frontend-single-game
**Date**: 2026-02-02

## Endpoints

### GET / (Home Page)

Returns HTML page with embedded game board and SSE connection.

**Response**: HTML document
- Contains game board container with `id="game-board"`
- Connects to `/sse` via Datastar `data-on-load="@get('/sse')"`
- Includes Datastar runtime script

**Example Response Structure**:
```html
<!DOCTYPE html>
<html>
<head>
    <script src="https://cdn.jsdelivr.net/npm/@starfederation/datastar"></script>
</head>
<body>
    <div id="game-board" data-on-load="@get('/sse')">
        <!-- Board populated via SSE -->
    </div>
</body>
</html>
```

---

### GET /sse (Server-Sent Events)

Establishes SSE connection for real-time updates.

**Response**: `text/event-stream`
- Sends initial game board state immediately
- Sends updates when game state changes
- Connection persists until client disconnects

**SSE Event Format** (Datastar):
```
event: datastar-merge-fragments
data: fragments <div id="game-board">...</div>

```

---

### POST /move

Submit a move (fire-and-forget pattern).

**Request Body** (form-encoded or Datastar signals):
```
player=X&position=TopLeft
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| player | string | Yes | "X" or "O" |
| position | string | Yes | One of: TopLeft, TopCenter, TopRight, MiddleLeft, MiddleCenter, MiddleRight, BottomLeft, BottomCenter, BottomRight |

**Response**:
- `202 Accepted` - Move accepted, update sent via SSE
- `400 Bad Request` - Invalid player or position
- `409 Conflict` - Invalid move (wrong turn, occupied, game over)

**Note**: No response body. Client receives updated board via SSE.

---

### POST /reset

Reset the game board to start a new game.

**Request Body**: None

**Response**:
- `202 Accepted` - Game reset, empty board sent via SSE

**Note**: No response body. Client receives fresh board via SSE.

---

## SSE Events

### Board Update (patchElements)

Replaces the entire game board container.

**Selector**: `#game-board`

**Content**: HTML fragment containing:
- 3x3 grid of squares
- Current turn indicator (or game outcome)
- Clickable squares for valid moves
- "New Game" button (if game ended)

**Example**:
```html
<div id="game-board">
    <div class="status">X's turn</div>
    <div class="board">
        <button data-on-click="@post('/move')" data-player="X" data-position="TopLeft">
            <span class="empty">·</span>
        </button>
        <!-- ... 8 more squares ... -->
    </div>
</div>
```

---

## Datastar Signals

The client maintains these signals for UI state:

| Signal | Type | Description |
|--------|------|-------------|
| `_fetching` | boolean | True during request, used for loading indicators |

---

## Error Handling

| Scenario | HTTP Status | SSE Behavior |
|----------|-------------|--------------|
| Invalid move format | 400 | No SSE event |
| Wrong player's turn | 409 | No SSE event (board unchanged) |
| Position occupied | 409 | No SSE event (board unchanged) |
| Game already over | 409 | No SSE event (board unchanged) |
| SSE connection lost | - | Client should reconnect automatically |

---

## Wire Format Examples

### Move Request (Datastar signals)

```http
POST /move HTTP/1.1
Content-Type: application/json

{"player":"X","position":"MiddleCenter"}
```

### SSE Game State Update

```
event: datastar-merge-fragments
data: fragments <div id="game-board"><div class="status">O's turn</div><div class="board">...</div></div>

```

### SSE Game Won

```
event: datastar-merge-fragments
data: fragments <div id="game-board"><div class="status">X wins!</div><div class="board">...</div><button data-on-click="@post('/reset')">New Game</button></div>

```
