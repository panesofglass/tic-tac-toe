# Feature Specification: Game Reset and Persistent Game Boards

**Feature Branch**: `006-game-reset`
**Created**: 2026-02-05
**Status**: Draft
**Input**: User description: "Rather than deleting a game, change the button to Reset, which should delete the original game behind the scenes, then generate a new game in its place. Launch with six games on first render, and always maintain a minimum of six game boards at all times. Any game should be able to be reset by the players connected to that game. New games with no players should not be able to be reset."

## Clarifications

### Session 2026-02-05

- Q: What should spectators (non-players) see for the Reset button? → A: Show reset button to spectators but disabled (greyed out)
- Q: How can users remove extra games beyond six? → A: Add Delete button that only appears when more than six games exist
- Q: What is the difference between Delete and Reset? → A: Delete removes the game board entirely (reduces count); Reset removes and replaces with a new board (maintains count)
- Q: How should the Delete button behave for non-players and at minimum count? → A: Grey out (disable) Delete button for non-players and when only six games exist

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Reset a Completed Game (Priority: P1)

After finishing a game, players want to quickly start a new game without navigating away or losing their spot on the page.

**Why this priority**: Core feature that replaces the existing delete functionality with reset behavior. Without this, the primary user interaction is broken.

**Independent Test**: Can be fully tested by playing a game to completion and clicking Reset, verifying a fresh game appears in the same position.

**Acceptance Scenarios**:

1. **Given** a completed game (won or drawn) with two connected players, **When** either player clicks Reset, **Then** the game board clears and a new game begins in the same position with X's turn.
2. **Given** an in-progress game with two connected players, **When** either player clicks Reset, **Then** the game board clears and a new game begins in the same position.
3. **Given** a game with only one player (X assigned, no O), **When** the player clicks Reset, **Then** the game board clears and a new game begins in the same position.

---

### User Story 2 - Initial Page Load with Six Games (Priority: P2)

When visiting the home page, users should immediately see six available game boards ready to play, providing instant access to gameplay without needing to create games manually.

**Why this priority**: Essential for the new user experience. Users expect games to be immediately available.

**Independent Test**: Can be fully tested by loading the home page and counting the visible game boards.

**Acceptance Scenarios**:

1. **Given** a user visits the home page for the first time, **When** the page finishes loading, **Then** exactly six game boards are displayed.
2. **Given** the server has just started with no prior state, **When** any user connects, **Then** six game boards are visible and ready to play.

---

### User Story 3 - Maintain Minimum Six Games (Priority: P2)

The system should always maintain at least six game boards. If games are removed for any reason, new games should be created to maintain the minimum.

**Why this priority**: Ensures consistent user experience and prevents empty or sparse game displays.

**Independent Test**: Can be tested by observing game board count remains at six or above under various conditions.

**Acceptance Scenarios**:

1. **Given** exactly six game boards exist, **When** a game is reset, **Then** six game boards remain (reset replaces, doesn't reduce count).
2. **Given** the system has fewer than six games due to an edge condition, **When** the system detects this, **Then** new games are automatically created to reach six.

---

### User Story 4 - Prevent Reset on Unplayed Games (Priority: P3)

Games that have no players connected should not display the Reset button, preventing unnecessary resets of fresh games.

**Why this priority**: Prevents confusion and unnecessary system activity. Users shouldn't reset games that haven't been played.

**Independent Test**: Can be tested by creating a new game and verifying the Reset button is not shown or is disabled.

**Acceptance Scenarios**:

1. **Given** a game board with no moves made and no players assigned, **When** a user views the game, **Then** the Reset button is not shown or is disabled.
2. **Given** a game where Player X has made at least one move, **When** any connected player views the game, **Then** the Reset button is enabled and visible.

---

### User Story 5 - Delete Extra Games (Priority: P3)

When users have created more than six games, they should be able to delete extra games to clean up the page.

**Why this priority**: Provides cleanup capability but only needed when users exceed the minimum game count.

**Independent Test**: Can be tested by creating seven games and verifying Delete button becomes enabled for assigned players.

**Acceptance Scenarios**:

1. **Given** seven or more game boards exist, **When** an assigned player views the page, **Then** Delete buttons are enabled on game boards.
2. **Given** exactly six game boards exist, **When** a user views the page, **Then** Delete buttons are visible but disabled (greyed out).
3. **Given** seven game boards exist, **When** a spectator views the page, **Then** Delete buttons are visible but disabled (greyed out).
4. **Given** seven game boards exist, **When** an assigned player clicks Delete on one game, **Then** that game is removed and six games remain with Delete buttons now disabled.

---

### Edge Cases

- What happens when a user tries to reset a game while another user is mid-move? The reset should proceed; the pending move is discarded.
- What happens if two players click Reset simultaneously? Only one reset should occur; the second click should act on the already-new game (effectively a no-op or another reset if players are assigned).
- What happens if a game is reset while a spectator (third user) is watching? The spectator should see the new game state via real-time updates.
- What happens on server restart? Six fresh games should be created on startup.
- What happens when multiple users try to delete games simultaneously bringing count below six? System should prevent deletion that would drop below six games.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST replace the single "Delete Game" button with both a "Reset" button and a conditional "Delete" button on each game board (per clarifications: Reset replaces in-place; Delete removes entirely when >6 games exist).
- **FR-002**: System MUST create six game boards when the application starts.
- **FR-003**: System MUST display six game boards when a user first loads the home page.
- **FR-004**: Reset button MUST be disabled when a game has no moves AND no assigned players; enabled otherwise when user is an assigned player.
- **FR-005**: When Reset is clicked, system MUST dispose of the current game and create a new game in the same visual position.
- **FR-006**: System MUST maintain a minimum of six game boards at all times.
- **FR-007**: Reset action MUST clear player assignments and start the new game with X's turn.
- **FR-008**: Reset action MUST be available to any player currently assigned to the game (X or O).
- **FR-009**: Reset button MUST be visible but disabled (greyed out) for spectators (users not assigned as X or O).
- **FR-010**: System MUST broadcast the reset/new game state to all connected clients viewing that game.
- **FR-011**: The "New Game" button MUST remain available for users who want additional games beyond the initial six.
- **FR-012**: Delete button MUST always be visible on game boards.
- **FR-013**: Delete button MUST be disabled (greyed out) when exactly six games exist.
- **FR-014**: Delete button MUST be disabled (greyed out) for spectators (users not assigned as X or O).
- **FR-015**: Delete button MUST remove the game board entirely (not replace it) when clicked by an assigned player.
- **FR-016**: System MUST prevent deletion that would reduce game count below six.

### Key Entities

- **Game Board**: A visual representation of a single tic-tac-toe game, including game state, player assignments, reset capability, and conditional delete capability.
- **Player Assignment**: Tracks which user is playing X and which is playing O for a given game. Reset clears these assignments.
- **Game Position**: The visual slot where a game board appears on the page. Reset replaces the game but maintains the same position; Delete removes the position entirely.
- **Reset Action**: Disposes current game and creates a new game in the same visual position (count unchanged).
- **Delete Action**: Removes the game board entirely from the page (count reduced by one).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users see exactly six game boards within 2 seconds of page load.
- **SC-002**: Reset action completes and displays new game state within 1 second of button click.
- **SC-003**: 100% of reset actions result in a fresh game with no moves and X's turn.
- **SC-004**: Reset button is not clickable on games with zero moves and no assigned players.
- **SC-005**: All connected users viewing a game see the reset state simultaneously via real-time updates.
- **SC-006**: Delete buttons are always visible but disabled when game count is six or user is a spectator.
- **SC-007**: Delete buttons are enabled only when game count exceeds six AND user is an assigned player.
- **SC-008**: Game count never drops below six through user actions.

## Assumptions

- The existing SSE (Server-Sent Events) infrastructure will be used for broadcasting reset state changes.
- The existing player assignment system tracks which users are connected to which games.
- Games will continue to use the existing game supervisor pattern for lifecycle management.
- The minimum of six games applies to the home page view; direct navigation to a specific game URL is unaffected.
- "Connected players" means users who have been assigned as X or O in the game (not spectators).
