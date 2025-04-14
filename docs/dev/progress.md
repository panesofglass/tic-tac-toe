# Development Progress

## Current Status

### Completed [✓]

- Project structure setup
- Core game models implemented
  - Move.cs with Position and Marker
  - GameBoard.cs as immutable state
  - Game.cs with state management
- Core frontend layout implemented
  - \_Layout.cshtml with styles and DataStar
  - LayoutModel with title support

### In Progress [-]

- Player Management Integration
  - [x] Repository-based player management
    - [x] Implement IPlayerRepository interface
    - [x] Implement IGamePlayerRepository interface
    - [x] Handle player data persistence
- Frontend Implementation
  - [ ] Player-aware game board rendering
  - [ ] Interactive squares for current player
  - [ ] Real-time updates with proper context

#### Plan for Implementing Dynamic Marker Assignment and Game Joining

1. **Direct Game Engine Integration (HIGH Priority)**

   - Remove the intermediary GameModel layer so that RazorSlice can interact directly with TicTacToe.Engine.Game objects.
   - Update the frontend code to rely on Game.FromMoves() for reconstructing or initializing game state.
   - Confirm the UI accurately reflects the game state without the intermediary class.

2. **Game Creation Fix (HIGH Priority)**

   - Investigate and resolve any issues blocking successful game creation.
   - Ensure the Game.Create() method is called properly when starting a new game and that returned state is persisted.
   - Include error handling and rollback logic to cleanly handle failing game creations.

3. **Player-Marker Association (MEDIUM Priority)**

   - Introduce a storage mechanism (e.g., MarkerAssignment) linking each GameId with a PlayerId and their assigned marker.
   - Ensure the first player to join a game is always given X, and second player is given O.
   - Enforce constraints so a single player cannot occupy both markers.

4. **Player-Aware Board Rendering (MEDIUM Priority)**

   - Dynamically render the board based on which player is viewing the game.
   - Only show available moves to the player whose marker it currently is.
   - Hide or disable move buttons for users who have not been assigned a marker or whose turn it is not.

5. **Dynamic Game Joining (LOW Priority)**
   - Allow a player to claim a marker when they first move, if unclaimed markers remain.
   - Validate moves to ensure a player can only take actions with their assigned marker.
   - Mark the game as in-progress once two players have joined; handle open and complete states appropriately.

This plan ensures stepwise progress from enabling direct integration with the game engine, through proper creation and association flows, to finalizing the dynamic joining mechanism.

### Planned [•]

- Game persistence
- Error handling
- Logging

## Recent Updates

### 2025-03-26

- Implemented core game models
- Added initial test suite
- Set up project structure

### 2025-03-27

- Refined game state management
- Implemented move validation
- Started frontend development
- Added \_Layout.cshtml with DataStar integration
- Decision: Refactor to remove GameModel in favor of domain types

## Implementation Plan

### 1. Repository-Based Player Management

- [ ] Design player repositories
  - [ ] Define IPlayerRepository interface
    - [ ] GetPlayerById, CreatePlayer, UpdatePlayer methods
    - [ ] Player entity with unique ID and metadata
  - [ ] Define IGamePlayerRepository interface
    - [ ] AssignPlayerToGame, GetPlayersForGame methods
    - [ ] Handle player-game relationships
- [ ] Implement repository pattern
  - [ ] Create concrete implementations of repositories
  - [ ] Configure dependency injection
  - [ ] Integrate with existing game logic

### 2. Game Repository and Domain Model Updates

- [ ] Enhance Game entity
  - [ ] Add player slots (ID + marker)
  - [ ] Implement turn tracking
  - [ ] Remove GameModel abstraction
- [ ] Update IGameRepository
  - [ ] Add player association methods
  - [ ] Handle turn management
  - [ ] Store player associations

### 3. Game Flow Implementation

- [ ] Update game creation
  - [ ] Assign first player as X
  - [ ] Create second player join flow
  - [ ] Handle marker assignments
- [ ] Implement turn validation
  - [ ] Check player identity
  - [ ] Validate marker usage
  - [ ] Enforce turn order

### 4. UI and Real-Time Updates

- [ ] Update Razor views
  - [ ] Use domain types directly
  - [ ] Add player-specific rendering
  - [ ] Show interactive squares for current player
- [ ] Enhance real-time updates
  - [ ] Display opponent moves
  - [ ] Update turn indicators
  - [ ] Show game status

### 5. Testing and Validation

- [ ] Unit tests
  - [ ] Repository implementations
  - [ ] Game repository
  - [ ] Turn validation
- [ ] Integration tests
  - [ ] Player flow
  - [ ] Game interactions
  - [ ] Real-time updates

## Technical Notes

### Repository Pattern Benefits

- Clear separation of concerns for data access
- Improved testability through interface-based design
- Flexible storage options (in-memory, database, etc.)
- Scalable approach that can evolve with application needs
- Reduced coupling between data access and business logic

### Game State

- Rich domain types replacing GameModel
- Player slots with marker assignments
- Turn tracking based on player identity

### UI Considerations

- Player-specific board rendering
- Interactive elements based on turn state
- Real-time updates maintaining player context
