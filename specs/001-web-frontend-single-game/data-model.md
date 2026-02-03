# Data Model: Web Frontend Single Game

**Feature**: 001-web-frontend-single-game
**Date**: 2026-02-02

## Entities

### Game (from Engine - DO NOT MODIFY)

The Engine provides these types which the Web layer consumes:

```fsharp
// Player mark
type Player = X | O

// Board position
type SquarePosition =
    | TopLeft | TopCenter | TopRight
    | MiddleLeft | MiddleCenter | MiddleRight
    | BottomLeft | BottomCenter | BottomRight

// Square state
type SquareState = Taken of Player | Empty

// Game state (board)
type GameState = IReadOnlyDictionary<SquarePosition, SquareState>

// Move result (game state machine)
type MoveResult =
    | XTurn of GameState * ValidMovesForX
    | OTurn of GameState * ValidMovesForO
    | Won of GameState * Player
    | Draw of GameState
    | Error of GameState * string

// Move action
type Move = XMove of SquarePosition | OMove of SquarePosition
```

### User (from Auth.fs - RETAIN)

Cookie-based identity with these claims:

| Claim | Type | Description |
|-------|------|-------------|
| `sub` | string (GUID) | Unique user identifier |
| `created_at` | ISO 8601 | First visit timestamp |
| `last_visit` | ISO 8601 | Most recent visit |
| `ip_address` | string | Client IP (diagnostic) |
| `user_agent` | string | Browser info (diagnostic) |

### SSE Event (new - Web layer)

```fsharp
type SseEvent =
    | PatchElements of html: string
    | RemoveElement of selector: string
    | PatchSignals of json: string
```

### SSE Subscriber (new - Web layer)

```fsharp
// Thread-safe collection of subscriber channels
// Each SSE connection creates one subscriber
type SseSubscriber = Channel<SseEvent>
```

## State Transitions

### Game State Machine

```
┌─────────┐
│ XTurn   │◄──────────────────────────┐
└────┬────┘                           │
     │ X makes valid move             │
     ▼                                │
┌─────────┐                           │
│ OTurn   │───── O makes valid move ──┘
└────┬────┘
     │
     ├── 3-in-a-row ──► Won(winner)
     │
     └── Board full ──► Draw
```

### SSE Connection Lifecycle

```
Page Load
    │
    ▼
Connect to /sse
    │
    ▼
Subscribe (add to subscribers)
    │
    ├──► Receive broadcasts ◄── Game state changes
    │
    ▼
Page Unload / Connection Lost
    │
    ▼
Unsubscribe (remove from subscribers)
```

## Validation Rules

### Move Validation (Engine handles)

- Position must be empty
- Must be correct player's turn (X or O)
- Game must not be finished (Won/Draw)

### User Identity (Auth.fs handles)

- Cookie must be valid (not expired)
- Claims transformation generates ID if missing

## Relationships

```
User (1) ──────── views ──────── (1) Home Page
                                      │
                                      │ embeds
                                      ▼
                                   (1) Game Board
                                      │
                                      │ displays
                                      ▼
                                   (1) Game State (MoveResult)
                                      │
                                      │ updated via
                                      ▼
                                   (1) SSE Connection
```

## Data Volume Assumptions

| Entity | Expected Volume | Notes |
|--------|-----------------|-------|
| Concurrent SSE connections | 1-10 initially | Scales with browser tabs |
| Active games | 1 per page load | Single game scope |
| Game state size | ~100 bytes | 9 squares + metadata |
| SSE events/second | <10 | Only on user clicks |
