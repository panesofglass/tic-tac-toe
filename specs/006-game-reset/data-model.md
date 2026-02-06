# Data Model: Game Reset and Persistent Game Boards

**Feature**: 006-game-reset | **Date**: 2026-02-05

## Entities

### Existing Entities (No Changes)

#### Game (TicTacToe.Engine)
```
Game
├── Id: string (GUID)
├── State: GameState (XTurn | OTurn | Won | Draw)
├── Board: Position -> Player option (9 cells)
└── Lifecycle: IDisposable, IObservable<MoveResult>
```

#### GameSupervisor (TicTacToe.Engine)
```
GameSupervisor
├── CreateGame(): (string * Game)
├── GetGame(gameId): Game option
├── GetActiveGameCount(): int    # Used for delete button logic
└── Dispose(): unit
```

#### PlayerAssignment (TicTacToe.Web)
```
PlayerAssignment
├── GameId: string
├── PlayerXId: string option
└── PlayerOId: string option
```

#### PlayerRole (TicTacToe.Web)
```
PlayerRole
├── PlayerX          # User is assigned as X
├── PlayerO          # User is assigned as O
├── Spectator        # Both slots filled, user is neither
├── UnassignedX      # X slot open (first to move)
└── UnassignedO      # O slot open (waiting for O)
```

### New/Modified Entities

#### GameBoardRenderContext (NEW - TicTacToe.Web)
```
GameBoardRenderContext
├── GameId: string
├── GameState: MoveResult
├── UserRole: PlayerRole
├── TotalGameCount: int
├── HasMoves: bool            # Derived: state is not initial XTurn with empty board
└── CanReset: bool            # Derived: HasMoves AND (UserRole is PlayerX OR PlayerO)
└── CanDelete: bool           # Derived: TotalGameCount > 6 AND (UserRole is PlayerX OR PlayerO)
```

**Rationale**: Consolidates all data needed for button rendering into single context record.

## State Transitions

### Game Reset Flow
```
State: Game A (in progress or complete)
       ↓
Action: Player clicks Reset
       ↓
Validation: User has PlayerRole = PlayerX OR PlayerO
       ↓
Transition:
  1. Create new Game B via supervisor.CreateGame()
  2. Dispose Game A via game.Dispose()
  3. Clear player assignments for Game A
  4. Subscribe to Game B state changes
  5. Broadcast PatchElements replacing game-A div with game-B content
       ↓
Result: Fresh game board in same visual position
        All clients see new game simultaneously
        Player assignments cleared (UnassignedX state)
```

### Game Delete Flow
```
State: N games exist (N > 6)
       ↓
Action: Player clicks Delete
       ↓
Validation:
  - TotalGameCount > 6
  - User has PlayerRole = PlayerX OR PlayerO for this game
       ↓
Transition:
  1. Dispose game via game.Dispose()
  2. Clear player assignments
  3. Broadcast RemoveElement for game div
  4. Broadcast updated game count to all games (for button state refresh)
       ↓
Result: N-1 games remain
        Delete buttons may become disabled if count = 6
```

### Initial Games Creation Flow
```
State: Application starting
       ↓
Event: ApplicationStarted
       ↓
Action: Loop 6 times:
  1. Create game via supervisor.CreateGame()
  2. Subscribe to game state changes
       ↓
Result: 6 games exist before first client connects
```

## Validation Rules

### Reset Button Enable Conditions
| Condition | Enabled |
|-----------|---------|
| Game has no moves AND no assigned players | NO |
| Game has moves, user is Spectator | NO |
| Game has no moves, user is assigned (X or O) | YES |
| Game has moves, user is PlayerX | YES |
| Game has moves, user is PlayerO | YES |

### Delete Button Enable Conditions
| Condition | Enabled |
|-----------|---------|
| Game count = 6 | NO |
| Game count > 6, user is Spectator | NO |
| Game count > 6, user is Unassigned | NO |
| Game count > 6, user is PlayerX | YES |
| Game count > 6, user is PlayerO | YES |

### Reset Action Validation
```
Valid when:
  game exists in supervisor
  AND user has claim "sub" (authenticated)
  AND PlayerAssignmentManager.GetRole(gameId, userId) returns PlayerX OR PlayerO

Invalid responses:
  - Game not found: 404 Not Found
  - User not authenticated: 401 Unauthorized (handled by requiresAuth wrapper)
  - User not assigned player: 403 Forbidden
```

### Delete Action Validation
```
Valid when:
  game exists in supervisor
  AND supervisor.GetActiveGameCount() > 6
  AND PlayerAssignmentManager.GetRole(gameId, userId) returns PlayerX OR PlayerO

Invalid responses:
  - Game not found: 404 Not Found
  - Would reduce count below 6: 409 Conflict
  - User not assigned player: 403 Forbidden
```

## Relationships

```
User (via cookie)
  │
  ├──assigned to──→ PlayerAssignment
  │                    │
  │                    └──tracks──→ Game (via GameId)
  │                                   │
  │                                   └──managed by──→ GameSupervisor
  │
  └──views──→ GameBoard (rendered HTML)
                 │
                 └──buttons state depends on──→ PlayerRole + GameCount
```

## Derived Fields

### HasMoves (computed from MoveResult)
```fsharp
let hasMovesOrPlayers (result: MoveResult) (assignment: PlayerAssignment option) =
    match result with
    | XTurn(state, _) ->
        // Check if board has any moves or if players assigned
        state.Board |> Map.exists (fun _ v -> v.IsSome)
        || assignment.IsSome && (assignment.Value.PlayerXId.IsSome || assignment.Value.PlayerOId.IsSome)
    | OTurn _ | Won _ | Draw _ | Error _ -> true  // Always has activity
```

### CanReset (computed)
```fsharp
let canReset hasMovesOrPlayers role =
    hasMovesOrPlayers &&
    match role with
    | PlayerX | PlayerO -> true
    | Spectator | UnassignedX | UnassignedO -> false
```

### CanDelete (computed)
```fsharp
let canDelete gameCount role =
    gameCount > 6 &&
    match role with
    | PlayerX | PlayerO -> true
    | Spectator | UnassignedX | UnassignedO -> false
```
