# Feature Specification: Multi-Player Tic-Tac-Toe

**Feature Branch**: `005-multi-player`
**Created**: 2026-02-04
**Status**: Draft
**Input**: User description: "Implement multi-player play. First player to make a move becomes the first marker (X), and the second player to make a move becomes the second player (O). Once the players begin play, no other player may replace them. Use the browser cookie as the player identifier."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - First Player Claims X (Priority: P1)

A visitor arrives at a game and makes the first move. By doing so, they become Player X and are assigned to that role for the duration of the game.

**Why this priority**: This establishes the core mechanic of player assignment. Without this, no game can begin.

**Independent Test**: Can be fully tested by having a single browser make a move on an empty board and verifying that move is recorded as X, and the browser is recognized as Player X on subsequent interactions.

**Acceptance Scenarios**:

1. **Given** an active game with no moves made, **When** a visitor makes the first move, **Then** that move is recorded as X and the visitor is assigned as Player X
2. **Given** an active game with no moves made, **When** a visitor makes the first move, **Then** a player identifier is stored in the browser cookie
3. **Given** a visitor who made the first move, **When** the page is refreshed, **Then** the system recognizes them as Player X

---

### User Story 2 - Second Player Claims O (Priority: P1)

A different visitor arrives at the same game after the first move has been made. When they make a move, they become Player O.

**Why this priority**: This completes the player pairing mechanism required for any two-player game.

**Independent Test**: Can be tested by having a second browser (different cookie) make a move after X's first move, verifying the move is recorded as O and that browser is recognized as Player O.

**Acceptance Scenarios**:

1. **Given** a game where Player X has made a move, **When** a different visitor makes a move, **Then** that move is recorded as O and the visitor is assigned as Player O
2. **Given** a game where Player X has made a move, **When** a different visitor makes a move, **Then** a player identifier is stored in their browser cookie
3. **Given** both players assigned, **When** either player refreshes the page, **Then** the system recognizes their respective roles

---

### User Story 3 - Turn Enforcement (Priority: P1)

Once both players are assigned, only the player whose turn it is can make a move.

**Why this priority**: Without turn enforcement, the game rules cannot be maintained and the game becomes meaningless.

**Independent Test**: Can be tested by having Player X attempt to make two consecutive moves and verifying the second move is rejected.

**Acceptance Scenarios**:

1. **Given** Player X has just moved, **When** Player X attempts another move, **Then** the move is rejected
2. **Given** Player O has just moved, **When** Player O attempts another move, **Then** the move is rejected
3. **Given** it is Player X's turn, **When** Player X makes a valid move, **Then** the move is accepted

---

### User Story 4 - Third Party Exclusion (Priority: P2)

Once both Player X and Player O are assigned, any other visitor is excluded from making moves in that game.

**Why this priority**: This protects game integrity but is secondary to the core player assignment and turn mechanics.

**Independent Test**: Can be tested by having a third browser attempt to make a move in a game where both players are already assigned and verifying the move is rejected.

**Acceptance Scenarios**:

1. **Given** a game with both Player X and Player O assigned, **When** a third visitor attempts to make a move, **Then** the move is rejected
2. **Given** a game with both players assigned, **When** a third visitor views the game, **Then** they can see the current game state but cannot interact
3. **Given** a third visitor is viewing a game, **When** either player makes a move, **Then** the third visitor sees the updated board state

---

### User Story 5 - Spectator Experience (Priority: P3)

Visitors who cannot play (third parties or late arrivals) can still watch the game in progress.

**Why this priority**: Enhances the experience but is not core to gameplay functionality.

**Independent Test**: Can be tested by having a third browser observe a game in progress and verifying board updates appear without the ability to interact.

**Acceptance Scenarios**:

1. **Given** a spectator viewing an in-progress game, **When** a player makes a move, **Then** the spectator sees the board update
2. **Given** a spectator viewing a completed game, **When** the game ends, **Then** the spectator sees the final result

---

### Edge Cases

- What happens when Player X starts a game but never returns? (Assumption: Game remains waiting for O indefinitely; no timeout mechanism in initial scope)
- How does the system handle a player clearing their browser cookies mid-game? (The player loses their identity and becomes a spectator; they cannot reclaim their role)
- What happens if the same person opens the game in two different browsers? (Each browser is treated as a separate player based on its cookie)
- What happens if a player's cookie expires during gameplay? (Standard browser session cookie duration applies; player loses identity if cookie expires)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST assign the first move maker as Player X
- **FR-002**: System MUST assign the second move maker (with a different player identifier) as Player O
- **FR-003**: System MUST store a unique player identifier in a browser cookie when a player makes their first move
- **FR-004**: System MUST reject moves from any visitor who is not Player X or Player O once both players are assigned
- **FR-005**: System MUST enforce turn order (X plays on odd turns, O plays on even turns)
- **FR-006**: System MUST identify returning players by their browser cookie
- **FR-007**: System MUST allow spectators to view game state without making moves
- **FR-008**: System MUST persist player assignments for the duration of the game
- **FR-009**: System MUST reject moves from a player when it is not their turn
- **FR-010**: System MUST provide visual feedback (e.g., flash/shake animation) on the board when a move is rejected, without requiring user acknowledgment

### Key Entities

- **Player**: A participant in a game, identified by a unique browser cookie value. Has a role (X or O) within a specific game.
- **Game**: A tic-tac-toe match with board state, optional Player X assignment, optional Player O assignment, and game status (in-progress, won, drawn).
- **Player Identifier (Cookie)**: A server-generated GUID (via existing Auth module) stored in the `TicTacToe.User` cookie with 30-day expiration. The existing `UserId` claim identifies a specific player across requests.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Two users on different devices can complete a full game of tic-tac-toe against each other
- **SC-002**: 100% of attempted moves by unauthorized users (third parties or wrong turn) are rejected
- **SC-003**: Players can refresh their browser and resume play without losing their assigned role
- **SC-004**: Game state remains consistent across all viewers (players and spectators) within 2 seconds of any move

## Clarifications

### Session 2026-02-04

- Q: When a player attempts an invalid move (wrong turn, or a third party trying to play), what feedback should they receive? → A: Visual indication on the board (flash/shake) with no modal or alert
- Q: How should the unique player identifier be generated? → A: Use existing Auth module (`Auth.fs`) which generates server-side GUIDs via `Guid.NewGuid()` stored in `TicTacToe.User` cookie

## Assumptions

- Browser cookies are available and not blocked by the user
- Existing Auth module (`Auth.fs`) and cookie configuration (`TicTacToe.User`, 30-day expiration) will be reused for player identification
- Games do not need a timeout mechanism for inactive players in initial scope
- A single game instance is accessed by all players (the game URL identifies the game)
- Real-time updates to spectators rely on existing game state synchronization mechanisms
