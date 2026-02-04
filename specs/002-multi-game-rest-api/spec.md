# Feature Specification: Multi-Game REST API

**Feature Branch**: `002-multi-game-rest-api`
**Created**: 2026-02-03
**Status**: Draft
**Input**: User description: "Support multiple games with RESTful resource-oriented hypermedia approach following Datastar patterns and Roy Fielding's REST dissertation. Games rendered on single page via SSE, with proper resource URLs at /games/{id}. POST to /games creates games with 201 + Location header."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create and Play a New Game (Priority: P1)

A user visits the home page and creates a new tic-tac-toe game. The game appears on the page and the user can play it to completion (win, lose, or draw) with another player taking turns on the same browser.

**Why this priority**: Core functionality - without the ability to create and play games, the feature has no value. This is the fundamental MVP.

**Independent Test**: User clicks "New Game", receives a game board, and can play X and O alternately until the game concludes with a win or draw message.

**Acceptance Scenarios**:

1. **Given** the home page is loaded, **When** user clicks "New Game", **Then** a new game board appears on the page showing an empty 3x3 grid with "X's turn" status
2. **Given** a new game exists, **When** user clicks an empty square, **Then** X is placed and status changes to "O's turn"
3. **Given** it is O's turn, **When** user clicks an empty square, **Then** O is placed and status changes to "X's turn"
4. **Given** a player achieves three in a row, **When** the winning move is made, **Then** the game displays "[X/O] wins!" and squares become non-clickable
5. **Given** all squares are filled with no winner, **When** the last move is made, **Then** the game displays "Draw!" and squares become non-clickable

---

### User Story 2 - Game Has Unique Resource URL (Priority: P2)

Each game has its own URL (`/games/{id}`) that can be bookmarked or shared. Visiting this URL directly shows that specific game's current state.

**Why this priority**: Enables REST compliance and shareability. Users can bookmark games or share links, and the URL serves as the game's unique identifier per REST principles.

**Independent Test**: Create a game, copy its URL, open in new tab, verify same game state appears.

**Acceptance Scenarios**:

1. **Given** a game is created, **When** user inspects the game element or network requests, **Then** the game's unique URL `/games/{id}` is discoverable
2. **Given** a game URL `/games/{id}`, **When** user navigates directly to this URL, **Then** that specific game's current state is displayed
3. **Given** a game with moves made, **When** user shares the URL and another user visits it, **Then** the recipient sees the same game state

---

### User Story 3 - Multiple Games on Single Page (Priority: P3)

Users can create and manage multiple simultaneous games on the home page. All games update in real-time without page refresh.

**Why this priority**: Extends single-game capability to support multiple concurrent games, enabling comparison or parallel play.

**Independent Test**: Create 3 games, make moves in each, verify all update independently and simultaneously via SSE.

**Acceptance Scenarios**:

1. **Given** one game exists on the page, **When** user clicks "New Game", **Then** a second game board appears alongside the first
2. **Given** multiple games exist, **When** user makes a move in one game, **Then** only that game updates; other games remain unchanged
3. **Given** multiple games exist, **When** moves are made rapidly across games, **Then** all games reflect correct state via SSE updates
4. **Given** 10 games exist on the page, **When** user interacts with each, **Then** the page remains responsive with updates appearing within 1 second

---

### User Story 4 - Delete/Remove a Game (Priority: P4)

Users can remove a game from the page. The game resource is deleted from the server.

**Why this priority**: Cleanup functionality - allows users to manage their game list by removing completed or abandoned games.

**Independent Test**: Create a game, delete it, verify it disappears from page and DELETE request returns appropriate response.

**Acceptance Scenarios**:

1. **Given** a game exists on the page, **When** user clicks the delete/remove button for that game, **Then** the game is removed from the page
2. **Given** a game has been deleted, **When** user attempts to access `/games/{id}` directly, **Then** a 404 or appropriate "not found" response is returned
3. **Given** multiple games exist, **When** user deletes one game, **Then** other games remain unaffected

---

### Edge Cases

- **Deleted game move attempt**: Move is silently ignored; SSE removes the deleted game from the page (no error modal needed)
- **Rapid clicking on same square**: First valid click is processed; subsequent clicks on occupied square are ignored per FR-012
- **SSE connection lost/reconnected**: On reconnect, client receives current state of all games (full resync)
- **Non-existent game URL** (`/games/invalid-id`): Returns 404 Not Found response
- **Move after game ended**: Move is rejected per FR-013; game state remains unchanged

## Requirements *(mandatory)*

### Functional Requirements

#### Resource Structure

- **FR-001**: System MUST expose games as resources at `/games/{id}` where `{id}` is a unique identifier
- **FR-002**: System MUST support `POST /games` to create a new game, returning HTTP 201 with a `Location` header containing the new game's URL
- **FR-003**: System MUST support `GET /games/{id}` to retrieve a specific game's current state
- **FR-004**: System MUST support `DELETE /games/{id}` to remove a game
- **FR-005**: System MUST support `POST /games/{id}` (or `PATCH /games/{id}`) to submit a move to a specific game

#### Hypermedia & Real-Time Updates

- **FR-006**: System MUST render all active games on the home page (`/`) as a single-page view
- **FR-007**: System MUST push all game state changes through a single SSE endpoint (`/sse`)
- **FR-008**: Game board elements MUST contain hypermedia controls (clickable squares) that enable state transitions
- **FR-009**: Hypermedia controls MUST appear or disappear based on game state (e.g., "New Game" button, clickable vs. non-clickable squares)
- **FR-010**: System MUST NOT require custom client-side JavaScript beyond the Datastar framework

#### Game Logic

- **FR-011**: System MUST enforce tic-tac-toe rules: alternating turns, valid move positions, win/draw detection
- **FR-012**: System MUST prevent moves on occupied squares
- **FR-013**: System MUST prevent moves after game has ended (win or draw)
- **FR-014**: System MUST correctly identify wins (three in a row: horizontal, vertical, diagonal) and draws (board full, no winner)

#### User Experience

- **FR-015**: Game updates MUST appear without requiring page refresh
- **FR-016**: System MUST provide visual indication of whose turn it is (X or O)
- **FR-017**: System MUST provide clear indication when a game ends (winner announced or draw declared)
- **FR-018**: System MUST provide a way to create new games from the home page

### Key Entities

- **Game**: A single tic-tac-toe game instance with a unique identifier, board state (9 squares), current turn (X or O), and game status (in-progress, X-won, O-won, draw)
- **Move**: An action that places a player's mark (X or O) on a specific square position; associated with a specific game
- **Square**: One of 9 positions on the game board; can be empty, contain X, or contain O
- **Player**: Either X or O; represents whose turn it is or who won

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can create a new game and see it appear on the page within 1 second
- **SC-002**: Game state updates (after moves) appear within 1 second without page refresh
- **SC-003**: System supports at least 10 concurrent games on a single page without performance degradation
- **SC-004**: Direct navigation to `/games/{id}` displays the correct game state
- **SC-005**: POST to `/games` returns 201 status with valid `Location` header containing the new game's URL
- **SC-006**: All game operations (create, move, delete) are reflected across all connected clients via SSE
- **SC-007**: Users can complete a full game (9 moves maximum) with correct win/draw detection 100% of the time

## Clarifications

### Session 2026-02-03

- Q: When a user attempts to make a move in a deleted game, what should happen? → A: Silently ignore the move; the deleted game is removed from page via SSE

## Assumptions

- Users play locally (X and O from the same browser tab) rather than multiplayer across different browsers
- Game state is stored in-memory on the server (persistence not required for MVP)
- The existing cookie-based user identity system (Auth.fs) is retained but not directly tied to game ownership
- The existing game engine (TicTacToe.Engine) provides correct game logic and will not be modified
- Datastar framework handles SSE connection management and DOM patching
- Modern browsers with SSE support are the target platform

## Out of Scope

- Multiplayer across different browsers/devices
- User accounts or game history
- AI opponents
- Game persistence across server restarts
- Tournament or ranking systems
- Chat or communication features
