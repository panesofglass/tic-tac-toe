# API Contracts: Multi-Player Tic-Tac-Toe

**Date**: 2026-02-04
**Feature**: 005-multi-player

## Overview

This feature modifies the existing `POST /games/{id}` endpoint behavior and adds role-awareness to `GET /games/{id}`. No new endpoints are created.

## Modified Endpoints

### POST /games/{id} - Make Move

**Change**: Add player validation before processing move.

**Request** (unchanged):
```
POST /games/{gameId}
Content-Type: application/x-www-form-urlencoded
Cookie: TicTacToe.User=<auth-cookie>

datastar: {"gameId": "abc123", "player": "X", "position": "TopLeft"}
```

**Response Scenarios**:

| Scenario | Status | Response |
|----------|--------|----------|
| Valid move (assigned player, correct turn) | 202 Accepted | SSE broadcasts updated board |
| First move (X slot open) | 202 Accepted | User assigned as X, SSE broadcasts |
| Second move (O slot open, different user) | 202 Accepted | User assigned as O, SSE broadcasts |
| Wrong turn (assigned player) | 202 Accepted | SSE broadcasts rejection animation |
| Not a player (spectator) | 202 Accepted | SSE broadcasts rejection animation |
| Game over | 202 Accepted | SSE broadcasts rejection animation |
| Invalid position/move | 202 Accepted | SSE broadcasts error (existing behavior) |

**Note**: All responses return 202 to maintain Datastar compatibility. Rejections are communicated via SSE with visual feedback, not HTTP status codes.

**SSE Rejection Broadcast**:
```
event: datastar-patch-signals
data: signals {"rejected": true, "rejectionReason": "NotYourTurn"}

event: datastar-patch-elements
data: selector #game-board
data: settle 300
data: fragment <div id="game-board" class="rejected">...</div>
```

### GET /games/{id} - View Game

**Change**: Response includes viewer's role for UI customization.

**Request** (unchanged):
```
GET /games/{gameId}
Cookie: TicTacToe.User=<auth-cookie>
```

**Response**: HTML page with Datastar signals indicating viewer role.

**New Datastar Signals**:
```html
<div data-signals='{"viewerRole": "PlayerX", "canMove": true}'>
  <!-- game board -->
</div>
```

| viewerRole | canMove | UI Behavior |
|------------|---------|-------------|
| "PlayerX" | true (when X's turn) | Show move buttons |
| "PlayerO" | true (when O's turn) | Show move buttons |
| "Spectator" | false | Hide/disable move buttons |
| "UnassignedX" | true | Show move buttons (first to click becomes X) |
| "UnassignedO" | true (if not X) | Show move buttons for non-X users |

## Datastar Signal Schema

### Existing Signals (unchanged)
```typescript
{
  gameId: string,      // Current game ID
  player: "X" | "O",   // Which marker to place
  position: string     // Board position (TopLeft, etc.)
}
```

### New Signals
```typescript
{
  viewerRole: "PlayerX" | "PlayerO" | "Spectator" | "UnassignedX" | "UnassignedO",
  canMove: boolean,
  rejected: boolean,           // Triggers rejection animation
  rejectionReason: string | null  // "NotYourTurn" | "NotAPlayer" | "GameOver" | null
}
```

## SSE Event Types

### Existing Events (unchanged)
- `datastar-patch-elements` - Update board HTML
- `datastar-remove-element` - Remove game (on delete)

### Modified Events

**Board Update with Role Info**:
```
event: datastar-patch-elements
data: selector #game-board
data: fragment <div id="game-board" data-signals='{"viewerRole":"PlayerX","canMove":true}'>...</div>
```

**Rejection Animation Trigger**:
```
event: datastar-patch-signals
data: signals {"rejected":true,"rejectionReason":"NotYourTurn"}
```

**Rejection Animation Clear** (after 300ms via CSS):
```
event: datastar-patch-signals
data: signals {"rejected":false,"rejectionReason":null}
```

## Cookie Contract

**Name**: `TicTacToe.User`
**Content**: Encrypted claims including `sub` (user ID GUID)
**Lifetime**: 30 days (sliding expiration)
**Flags**: HttpOnly, SameSite=Strict

**Server extracts user ID via**:
```fsharp
ctx.User.TryGetUserId() // Returns Some(guid-string) or None
```

## Error Handling

| Error Condition | Handling |
|-----------------|----------|
| Missing cookie | Auth middleware creates new user ID automatically |
| Game not found | 404 Not Found (existing behavior) |
| Malformed request | 400 Bad Request (existing behavior) |

## Backward Compatibility

- Existing clients continue to work; `viewerRole` signal is additive
- Single-browser testing works (user plays both sides if only one cookie)
- Rejection animation is CSS-only, degrades gracefully if CSS not loaded
