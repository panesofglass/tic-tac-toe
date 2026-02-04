# Data Model: Multi-Game REST API

**Feature**: 002-multi-game-rest-api
**Date**: 2026-02-03

## Overview

The multi-game API extends the existing data model with game identification. Core game entities remain unchanged (protected by constitution). New entities support REST resource addressing.

## Entities

### Game (Existing - Engine Layer)

**Location**: `src/TicTacToe.Engine/Model.fs`
**Status**: PROTECTED - No modifications

| Field | Type | Description |
|-------|------|-------------|
| State | `IReadOnlyDictionary<SquarePosition, SquareState>` | 9 squares |
| Current Turn | Implicit in `MoveResult` variant | X or O |
| Status | `MoveResult` DU variant | XTurn, OTurn, Won, Draw, Error |

**State Transitions**:
```
Empty → XTurn → OTurn → XTurn → ... → Won(X) | Won(O) | Draw
```

### GameResource (New - Web Layer)

**Purpose**: REST resource wrapper for games with URL identity

| Field | Type | Description |
|-------|------|-------------|
| id | `string` (GUID) | Unique game identifier (from GameSupervisor) |
| url | `string` | Resource URL: `/games/{id}` |
| state | `MoveResult` | Current game state from Engine |
| createdAt | `DateTimeOffset` | Timestamp (for cleanup) |

**Relationships**:
- GameResource → Game (1:1, wraps engine Game actor)
- GameResource → GameSupervisor (managed by supervisor)

### Move (Existing - Engine Layer)

**Location**: `src/TicTacToe.Engine/Model.fs`
**Status**: PROTECTED - No modifications

| Variant | Fields | Description |
|---------|--------|-------------|
| XMove | SquarePosition | X makes a move |
| OMove | SquarePosition | O makes a move |

### MoveRequest (New - Web Layer)

**Purpose**: Datastar signal payload for move submission

| Field | Type | Validation | Description |
|-------|------|------------|-------------|
| gameId | `string` | Non-empty GUID | Target game |
| player | `string` | "X" or "O" | Which player |
| position | `string` | Valid SquarePosition name | Board position |

**Transformation**:
```fsharp
MoveRequest → Move.TryParse(player, position) → Option<Move>
```

### SSE Event Types (Extended - Web Layer)

**Location**: `src/TicTacToe.Web/SseBroadcast.fs`

| Variant | Fields | Purpose |
|---------|--------|---------|
| PatchElements | html: string | Update existing game board |
| PatchElementsAppend | selector: string, html: string | **NEW**: Add new game to container |
| RemoveElement | selector: string | Remove deleted game |
| PatchSignals | json: string | Update client signals (existing) |

## Validation Rules

### Game ID
- Format: GUID string (existing from `GameSupervisor.CreateGame()`)
- Validation: `Guid.TryParse` for request validation
- Invalid ID: Return 404 Not Found

### Move Validation
1. Game must exist (404 if not)
2. Player must match current turn (handled by Engine)
3. Position must be valid and empty (handled by Engine)
4. Game must be in progress (handled by Engine)

## HTML Structure

### Games Container (Home Page)

```html
<div id="games-container">
    <!-- Games added here via SSE append -->
</div>
```

### Individual Game Board

```html
<div id="game-{gameId}" class="game-board">
    <div class="status">
        <h2>{statusMessage}</h2>
    </div>
    <div class="board">
        <!-- 9 squares as buttons or divs -->
        <button
            data-signals="{gameId: '{gameId}', player: '{currentPlayer}', position: '{pos}'}"
            data-on:click="@post('/games/{gameId}')">
        </button>
    </div>
    <div class="controls">
        <button data-on:click="@delete('/games/{gameId}')">Delete Game</button>
    </div>
</div>
```

## State Management

### Server-Side (MailboxProcessor)

```
GameSupervisor
├── Map<string, GameRef>    // Active games by ID
├── CreateGame() → (id, game)
├── GetGame(id) → Option<Game>
└── RemoveGame(id) → unit   // Existing, exposed for DELETE
```

### Client-Side (Datastar Signals)

Per-game signals scoped by game ID:
```javascript
{
    gameId: "abc123",
    player: "X",      // Set when clicking square
    position: ""      // Set when clicking square
}
```

## Lifecycle

### Game Creation
1. Client clicks "New Game" button
2. POST to `/games` (no body needed)
3. Server creates game via `supervisor.CreateGame()`
4. Server returns 201 with `Location: /games/{id}`
5. Server broadcasts SSE `PatchElementsAppend` to add game to container

### Game Play
1. Client clicks square
2. Datastar sets signals (gameId, player, position)
3. POST to `/games/{gameId}`
4. Server validates and applies move
5. Server returns 202 Accepted
6. Game observable broadcasts state change
7. SSE `PatchElements` updates specific game board

### Game Deletion
1. Client clicks "Delete Game"
2. DELETE to `/games/{gameId}`
3. Server removes game from supervisor
4. Server returns 204 No Content
5. Server broadcasts SSE `RemoveElement #game-{gameId}`
6. Game disappears from all connected clients
