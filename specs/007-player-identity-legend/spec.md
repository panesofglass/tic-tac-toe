# Feature Specification: Player Identity & Legend Display

**Feature Branch**: `007-player-identity-legend`
**Created**: 2026-02-05
**Status**: Draft
**Input**: User description: "show the current user's identifier in the top right of the page. Show a legend of which player is X and which is O under each game board. Highlight which player is currently making a move by making their legend item bold."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - See My Identity on the Page (Priority: P1)

As a logged-in user, I want to see my identity displayed in the top-right corner of the page so that I know which account I am playing as and can distinguish myself from other players.

**Why this priority**: Knowing who you are is foundational context for understanding the player legend and turn indicators. Without this, the legend is less meaningful.

**Independent Test**: Can be fully tested by logging in and verifying the user identifier appears in the top-right corner of the page. Delivers immediate clarity about the user's session identity.

**Acceptance Scenarios**:

1. **Given** a user has logged in, **When** they view any page in the application, **Then** their user identifier is displayed in the top-right corner of the page.
2. **Given** a user has not logged in, **When** they visit the login page, **Then** no user identifier is shown.
3. **Given** a user is logged in, **When** the page layout renders, **Then** the user identifier is always visible without scrolling (fixed in the page header area).

---

### User Story 2 - See Player Legend Under Each Game Board (Priority: P1)

As a user viewing a game, I want to see a legend under the game board showing which player is X and which is O so that I know who is playing which side.

**Why this priority**: The legend is the core of this feature - it communicates player-to-symbol mapping, which is essential for understanding ongoing games.

**Independent Test**: Can be fully tested by creating a game, having two players join, and verifying the legend shows each player's identifier alongside their symbol (X or O).

**Acceptance Scenarios**:

1. **Given** a game with both players assigned, **When** the game board is displayed, **Then** a legend appears below the board showing "X: [Player X's identifier]" and "O: [Player O's identifier]".
2. **Given** a game where only Player X has been assigned, **When** the game board is displayed, **Then** the legend shows "X: [Player X's identifier]" and "O: Waiting for player...".
3. **Given** a new game with no players assigned, **When** the game board is displayed, **Then** the legend shows "X: Waiting for player..." and "O: Waiting for player...".
4. **Given** a game is viewed via SSE broadcast update, **When** the board updates, **Then** the legend reflects the current player assignments.

---

### User Story 3 - See Which Player's Turn It Is in the Legend (Priority: P2)

As a user viewing a game, I want the current player's legend entry to be visually bold so that I can quickly see whose turn it is.

**Why this priority**: Turn highlighting enhances usability but depends on the legend (Story 2) existing first. It adds clarity but the game is playable without it.

**Independent Test**: Can be fully tested by starting a game, verifying the active player's legend item is bold, making a move, and verifying the bold switches to the other player.

**Acceptance Scenarios**:

1. **Given** a game where it is X's turn, **When** the game board is displayed, **Then** the X legend entry is displayed in bold and the O legend entry is not bold.
2. **Given** a game where it is O's turn, **When** the game board is displayed, **Then** the O legend entry is displayed in bold and the X legend entry is not bold.
3. **Given** a game that has ended (win or draw), **When** the game board is displayed, **Then** neither legend entry is bold.
4. **Given** a game updates via SSE broadcast, **When** the turn changes, **Then** the bold highlighting updates to reflect the new active player.

---

### Edge Cases

- What happens when a player's identifier is very long? The display should truncate gracefully without breaking the layout.
- How does the legend appear for a spectator? Spectators see the same legend as players - it shows player identifiers and whose turn it is, regardless of the viewer's role.
- What happens when a game is reset? The legend should reset to show "Waiting for player..." for both slots since player assignments are cleared.
- What happens when a game is deleted? The legend is removed along with the game board.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The application MUST display the current user's identifier in the top-right area of the page on all authenticated pages.
- **FR-002**: The user identifier display MUST be part of the persistent page layout (visible on every page, not just the home page).
- **FR-003**: Each game board MUST have a player legend displayed below it.
- **FR-004**: The player legend MUST show the symbol (X or O) paired with the assigned player's identifier for each player slot.
- **FR-005**: When a player slot is not yet assigned, the legend MUST display a placeholder such as "Waiting for player..." for that slot.
- **FR-006**: The legend MUST visually emphasize (bold) the entry of the player whose turn it currently is.
- **FR-007**: When the game is over (win or draw), the legend MUST NOT bold either player's entry.
- **FR-008**: The legend MUST update in real time when game state changes via SSE broadcasts.
- **FR-009**: Long user identifiers MUST be handled gracefully without breaking the page layout (e.g., truncation with ellipsis).

### Key Entities

- **User Identity**: A unique identifier assigned to each user upon login. Currently represented as a GUID string stored in authentication claims.
- **Player Assignment**: The mapping between a user identity and their role (X or O) within a specific game. A game can have zero, one, or two assigned players.
- **Player Legend**: A UI element below each game board that shows the player-to-symbol mapping and indicates the active turn.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can identify themselves on the page within 2 seconds of page load by seeing their identifier in the top-right corner.
- **SC-002**: Users can determine which player is X and which is O for any game by reading the legend below the game board.
- **SC-003**: Users can determine whose turn it is by glancing at the bold legend entry, without needing to read the status text above the board.
- **SC-004**: Legend updates are visible to all connected users within 1 second of a move being made (via real-time updates).
- **SC-005**: The user identifier and legend display correctly on common screen sizes without layout breakage.

## Clarifications

### Session 2026-02-05

- Q: How should the user identifier be displayed? → A: First 8 characters of GUID (e.g., `a3f2b1c9`)
- Q: Should the legend show "You" for the current user's entry? → A: No, always show the 8-char identifier for all players. Personalized "You" labels deferred to a future iteration.

## Assumptions

- The user identifier displayed is the first 8 characters of their GUID (e.g., `a3f2b1c9`). This is compact, unique enough to distinguish players, and requires no additional user-facing identity system.
- The player legend uses the same 8-char identifier format as the top-right user display for consistency. No personalized "You" labels - all players see raw identifiers. This keeps rendering uniform between initial load and SSE broadcast updates.
- The legend is positioned directly below each game board, within the same game card/container.
- Bold styling is sufficient visual emphasis for the active turn indicator (no additional color changes or animations needed).
- The SSE broadcast rendering path will need to include player assignment data to render the legend, since broadcasts currently lack user context.
