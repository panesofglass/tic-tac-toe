# Data Model: Multi-Player Tic-Tac-Toe

**Date**: 2026-02-04
**Feature**: 005-multi-player

## Entities

### PlayerAssignment

Tracks which authenticated user is assigned to which role (X or O) in a specific game.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| GameId | string | Unique identifier for the game | Required, matches existing game ID format |
| PlayerXId | string option | User ID of player assigned to X | None if not yet assigned |
| PlayerOId | string option | User ID of player assigned to O | None if not yet assigned |

**Invariants**:
- `PlayerXId` and `PlayerOId` must be different when both are assigned
- `PlayerOId` can only be assigned after `PlayerXId` is assigned (X always moves first)
- Once assigned, player IDs cannot be changed for the duration of the game

### PlayerRole (Discriminated Union)

Represents the possible roles a user can have in relation to a game.

```fsharp
type PlayerRole =
    | PlayerX           // Assigned as X
    | PlayerO           // Assigned as O
    | Spectator         // Both slots filled, user is neither
    | UnassignedX       // X slot open (first to move gets it)
    | UnassignedO       // O slot open (next to move gets it)
```

### MoveValidationResult (Discriminated Union)

Result of validating whether a user can make a move.

```fsharp
type MoveValidationResult =
    | Allowed of PlayerRole           // User may proceed with move
    | Rejected of RejectionReason     // User cannot make this move

type RejectionReason =
    | NotYourTurn                     // Correct player, wrong turn
    | NotAPlayer                      // User is spectator
    | WrongPlayer                     // User is X but O's turn (or vice versa)
    | GameOver                        // Game already finished
```

## State Transitions

### Player Assignment State Machine

```
┌──────────────────┐
│   NoPlayers      │  (game created, no moves yet)
│   X: None        │
│   O: None        │
└────────┬─────────┘
         │ User A makes first move
         ▼
┌──────────────────┐
│   OnePlayer      │  (X assigned, waiting for O)
│   X: Some(A)     │
│   O: None        │
└────────┬─────────┘
         │ User B makes second move (B ≠ A)
         ▼
┌──────────────────┐
│   TwoPlayers     │  (both assigned, game locked)
│   X: Some(A)     │
│   O: Some(B)     │
└──────────────────┘
         │
         │ Game continues until Won/Draw
         ▼
     [Game Over]
```

### Move Validation Logic

```
Input: (userId, gameId, attemptedMove)

1. Get PlayerAssignment for gameId
2. Get current game state (XTurn/OTurn/Won/Draw)
3. Determine user's role:
   - If Won/Draw → GameOver
   - If XTurn and X unassigned → AssignAndAllow (user becomes X)
   - If OTurn and O unassigned and userId ≠ X → AssignAndAllow (user becomes O)
   - If XTurn and userId = X → Allow
   - If OTurn and userId = O → Allow
   - If userId = X but OTurn → NotYourTurn
   - If userId = O but XTurn → NotYourTurn
   - Otherwise → NotAPlayer (spectator)
```

## Relationships

```
┌─────────────────┐       ┌──────────────────┐
│      Game       │ 1───1 │ PlayerAssignment │
│  (Engine.fs)    │       │   (Web layer)    │
│                 │       │                  │
│  - GameId       │◄──────│  - GameId (FK)   │
│  - MoveResult   │       │  - PlayerXId     │
│  - Subscribers  │       │  - PlayerOId     │
└─────────────────┘       └──────────────────┘
                                   │
                                   │ references
                                   ▼
                          ┌──────────────────┐
                          │   User (Auth)    │
                          │                  │
                          │  - UserId (GUID) │
                          │  - Cookie claim  │
                          └──────────────────┘
```

## Validation Rules

### From Functional Requirements

| Requirement | Validation Rule |
|-------------|-----------------|
| FR-001 | First move maker becomes X: `if XTurn && PlayerXId = None then assign userId` |
| FR-002 | Second move maker becomes O: `if OTurn && PlayerOId = None && userId ≠ PlayerXId then assign userId` |
| FR-004 | Reject non-players: `if BothAssigned && userId ∉ {PlayerXId, PlayerOId} then reject` |
| FR-005 | Enforce turn order: `if XTurn && userId ≠ PlayerXId then reject` |
| FR-009 | Reject wrong turn: `if OTurn && userId = PlayerXId then reject NotYourTurn` |

### Edge Case Handling

| Scenario | Behavior |
|----------|----------|
| User clears cookies mid-game | User gets new ID, becomes spectator |
| Same user in two browsers | Each browser is separate user (different cookie) |
| Player X tries to also be O | Rejected - `userId = PlayerXId` prevents O assignment |
| Move on completed game | Rejected - GameOver reason |

## Storage Approach

**Pattern**: MailboxProcessor (consistent with constitution Principle I)

```fsharp
type PlayerAssignmentMessage =
    | GetRole of gameId: string * userId: string * AsyncReplyChannel<PlayerRole>
    | AssignPlayer of gameId: string * userId: string * Player * AsyncReplyChannel<MoveValidationResult>
    | RemoveGame of gameId: string
```

**State Structure**:
```fsharp
type AssignmentState = Map<string, PlayerAssignment>
// Key: gameId, Value: PlayerAssignment record
```

**Lifecycle**:
- Created when first move attempted on a game
- Removed when game is deleted from supervisor
- No persistence beyond process lifetime (matches game lifecycle)
