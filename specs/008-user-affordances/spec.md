# Feature Specification: User-Specific Affordances

**Feature Branch**: `008-user-affordances`
**Created**: 2026-02-06
**Status**: Draft
**Input**: User description: "One of the goals of the hypermedia approach is to deliver to each user the appropriate set of affordances based on their available interactions. At the moment, the same content is sent to all players and observers, and a server check is necessary to know whether the user can take an action. This should be the fallback. Instead, each user should only see links or buttons they are allowed to use. If a game has six boards, two of which are assigned to player abcd as X and O for boards 1 and 2, respectively, and both boards are X's turn, then player abcd should see buttons/links for interacting with board 1, only the reset and delete buttons for board 2, and have observer renders for the remaining boards."

## Clarifications

### Session 2026-02-06

- Q: Should real-time updates be per-user server-rendered HTML, hybrid server+client signals, or broadcast with client-side toggling? → A: Server-side rendering — the server renders HTML fragments per state change, tailored to each user's role (refined to per-role rendering in subsequent clarification).
- Q: Should non-interactive squares be visually distinct from interactive ones, or just lack the clickable element? → A: Same appearance, no affordance — squares look the same but lack clickable elements; the absence of the link/button is sufficient.
- Q: Who should see reset and delete buttons? → A: Assigned players always see reset/delete; all authenticated users (including observers) also see them when board count > 6. Authentication is required for any affordances — unauthenticated users see no interactive controls at all. Unauthenticated users should not normally exist (redirect on first visit), but may occur due to expired cookies or other mishaps.
- Q: Should the spec define an explicit upper bound on concurrent viewers per game for per-user rendering? → A: No explicit bound. Boards grow unbounded. For any given game, there are at most 3 distinct rendered views (Player X, Player O, and non-player), since all non-player users see the same content. The system should leverage this by rendering per-role rather than per-user, using the role as a cache key. Only the two assigned players need unique renders; all other viewers share a single non-player render.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Active Player Sees Only Their Actionable Controls (Priority: P1)

A player viewing a game board where it is their turn sees interactive controls (clickable squares for valid moves, reset button, delete button) for that board. The system renders the board with full interactivity because the player is authorized to act.

**Why this priority**: This is the core value proposition — ensuring the active player's experience is tailored to show only the actions they can take, which is the fundamental hypermedia principle being implemented.

**Independent Test**: Can be fully tested by logging in as a player assigned to a game, verifying that when it is their turn, clickable squares and game management buttons appear on their board.

**Acceptance Scenarios**:

1. **Given** a game where Player X is assigned to the current user and it is X's turn, **When** the user views the board, **Then** the user sees clickable squares for all valid (empty) positions, plus reset and delete buttons.
2. **Given** a game where Player O is assigned to the current user and it is O's turn, **When** the user views the board, **Then** the user sees clickable squares for all valid positions, plus reset and delete buttons.
3. **Given** a game where Player X is assigned to the current user and it is O's turn, **When** the user views the board, **Then** the user does NOT see clickable squares but DOES see reset and delete buttons.

---

### User Story 2 - Assigned Player on Opponent's Turn Sees Limited Controls (Priority: P2)

A player viewing a game board where it is NOT their turn sees the board in a read-only state for moves (no clickable squares) but retains access to game management actions (reset, delete) since they are an assigned player.

**Why this priority**: Players who are assigned but waiting for their opponent still need to manage the game (e.g., reset a stuck game, delete an abandoned game), but should not be presented with move controls they cannot use.

**Independent Test**: Can be fully tested by logging in as the assigned player, having the opponent's turn be active, and verifying that move squares are not interactive but reset and delete buttons remain visible.

**Acceptance Scenarios**:

1. **Given** a game where the current user is assigned as Player X and it is O's turn, **When** the user views the board, **Then** move squares are displayed as non-interactive (no buttons/links) and reset and delete buttons are present.
2. **Given** a game where the current user is assigned as Player O and it is X's turn, **When** the user views the board, **Then** move squares are displayed as non-interactive and reset and delete buttons are present.

---

### User Story 3 - Observer Sees Read-Only Board (Priority: P3)

An authenticated user who is not assigned as either player (an observer/spectator) sees a board with no clickable move squares. When the board count is 6 or fewer, the observer also sees no reset or delete buttons. When the board count exceeds 6, the observer sees reset and delete buttons (per prior requirements allowing any authenticated user to manage games at that threshold).

**Why this priority**: Observers should have a clean viewing experience without move controls they cannot use, while still being able to help manage game pile-up when many boards exist.

**Independent Test**: Can be fully tested by logging in as a user who is not assigned to any role in a game and verifying that move controls are absent, and that reset/delete buttons appear only when the board count threshold is exceeded.

**Acceptance Scenarios**:

1. **Given** a game where both Player X and Player O are assigned to other users and there are 6 or fewer boards, **When** an unassigned user views the board, **Then** no clickable squares, reset buttons, or delete buttons are displayed.
2. **Given** a game where both Player X and Player O are assigned to other users and there are more than 6 boards, **When** an unassigned user views the board, **Then** no clickable squares are displayed, but reset and delete buttons ARE displayed.
3. **Given** a game where one slot is unassigned and the current user has NOT yet claimed it, **When** the user views the board, **Then** the user sees the board in a state appropriate for an unassigned user — move squares may be interactive if the unassigned slot's turn is active (allowing them to claim the slot by making a move), but reset and delete buttons are not shown until the user is assigned or the board count exceeds 6.

---

### User Story 4 - Multi-Board Personalized View (Priority: P1)

A player who is assigned to multiple boards across the game list sees each board rendered according to their specific role and the current turn state for that board. For example, a player assigned as X on Board 1 and O on Board 2, with both boards on X's turn, sees: Board 1 with full move controls (it's their turn as X), Board 2 with only management controls (it's X's turn but they are O), and all other boards as observer views.

**Why this priority**: This is the concrete multi-board scenario described in the feature request and represents the end-to-end value of user-specific affordances across the full game list.

**Independent Test**: Can be fully tested by assigning a single user to multiple boards with different roles, setting specific turn states, and verifying each board renders with the correct set of controls.

**Acceptance Scenarios**:

1. **Given** six boards where the user is X on Board 1 and O on Board 2 and it is X's turn on both, **When** the user views the game list, **Then** Board 1 shows clickable move squares plus reset/delete buttons, Board 2 shows only reset/delete buttons (no move squares), and Boards 3-6 show observer-only views (no move squares, no reset/delete since board count is not > 6).
2. **Given** six boards where the user is X on Board 1 and O on Board 2 and it is O's turn on both, **When** the user views the game list, **Then** Board 1 shows only reset/delete buttons, Board 2 shows clickable move squares plus reset/delete buttons, and Boards 3-6 show observer-only views (no move squares, no reset/delete since board count is not > 6).
3. **Given** seven boards where the user is X on Board 1 and O on Board 2 and it is X's turn on both, **When** the user views the game list, **Then** Board 1 shows clickable move squares plus reset/delete buttons, Board 2 shows only reset/delete buttons, and Boards 3-7 show no move squares but DO show reset/delete buttons (board count > 6 threshold met).

---

### User Story 5 - Real-Time Updates Maintain Personalized Views (Priority: P2)

When a game state changes (a move is made, a game is reset), each connected user receives an updated board rendered specifically for their role and the new turn state — not a generic broadcast.

**Why this priority**: Without personalized real-time updates, the system would revert to the current generic broadcast behavior, undermining the purpose of personalized affordances.

**Independent Test**: Can be fully tested by having two players and an observer connected to the same game, making a move, and verifying each user receives a differently-rendered board update matching their role.

**Acceptance Scenarios**:

1. **Given** Player X and Player O are both viewing a game and it is X's turn, **When** Player X makes a move, **Then** Player X receives an updated board showing it is now O's turn (no move squares for X), Player O receives an updated board showing move squares (it is now their turn), and any observer receives a read-only update.
2. **Given** a player resets a game, **When** the reset broadcast is sent, **Then** each connected user receives a board rendered according to their role in the reset game.

---

### Edge Cases

- What happens when a player disconnects and reconnects mid-game? The system should re-render the board with the correct affordances based on current game state and the user's role at the time of reconnection.
- What happens when a player is assigned to a game during an active session (first move auto-assignment)? The user's view should update from unassigned to assigned affordances in real time.
- What happens when a game ends (win or draw)? All users should see a completed game view with no move controls; assigned players retain reset and delete buttons, observers see read-only.
- What happens when the delete button's visibility condition (more than 6 boards) changes as boards are added or deleted? Each user's view should reflect the current board count and their role for each board.
- How does the system behave if a user has the same board open in multiple browser tabs? Each tab should render affordances consistently based on the user's identity and role.
- What if a server-side authorization check rejects an action that the UI allowed (fallback behavior)? The system should display an appropriate error message and the server-side check remains the authoritative guard.
- What if a user's authentication cookie expires mid-session? The system should render zero affordances for that user. Since all users are redirected to obtain a cookie on first visit, this is an edge case but must be handled gracefully (e.g., redirect to re-authenticate).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST render game boards with interactive controls (clickable move squares) only for the player whose turn it is on that specific board.
- **FR-002**: The system MUST render game management controls (reset and delete buttons) for assigned players (X or O) on that board regardless of board count. When the board count exceeds 6, the system MUST also render reset and delete buttons for all authenticated users (including observers) on every board.
- **FR-003**: The system MUST render a board with no move controls for authenticated observers (not assigned as either player) on a given board. Non-interactive squares retain the same visual appearance as interactive squares but lack clickable elements (no buttons or links). Observers see reset and delete buttons only when the board count exceeds 6 (per FR-008).
- **FR-004**: The system MUST personalize the rendering of each board independently when a user views multiple boards, based on the user's role and the current turn state for each board.
- **FR-005**: The system MUST deliver personalized real-time updates to each connected user when game state changes, tailored to their specific role and the new game state. For any given game, there are at most 3 distinct views (Player X, Player O, and non-player); the system MUST render per distinct role and deliver the appropriate version to each subscriber (per-role server rendering).
- **FR-006**: The system MUST continue to enforce server-side authorization checks on all actions (moves, resets, deletes) as a fallback, regardless of what the UI renders.
- **FR-007**: The system MUST update a user's affordances in real time when their role changes (e.g., from unassigned to assigned player upon making a first move).
- **FR-008**: When the board count exceeds 6, the system MUST show the reset and delete buttons to all authenticated users on every board, regardless of their role on that board.
- **FR-009**: When a game is completed (win or draw), the system MUST remove move controls for all users while preserving reset and delete buttons per the standard access rules (assigned players always; all authenticated users when board count > 6).
- **FR-010**: The system MUST render boards for unassigned users such that if it is the unassigned slot's turn, the user can still make a move to claim that slot, consistent with the existing auto-assignment behavior.
- **FR-011**: The system MUST render zero affordances (no interactive controls of any kind) for unauthenticated users. All users are expected to be redirected to obtain an authentication cookie on first visit; unauthenticated access is an edge case (e.g., expired cookie) and should result in a fully read-only view.

### Key Entities

- **User**: An authenticated visitor identified by a persistent identity. A user may be assigned to zero or more game boards.
- **Game Board**: A single tic-tac-toe game with a state (in progress, won, drawn), assigned players, and a current turn indicator.
- **Player Role**: The relationship between a user and a specific game board — one of: Player X (assigned, plays X), Player O (assigned, plays O), Unassigned X (slot open, X turn claimable), Unassigned O (slot open, O turn claimable), or Spectator (both slots filled, user is neither).
- **Affordance**: An interactive control (button, link, clickable area) presented to a user. Affordances are determined by the intersection of the user's role and the game board's current state.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of game boards render with controls matching the viewing user's role and the board count — active players see move controls only on their turn, assigned players always see management controls, observers see management controls only when board count > 6, and unauthenticated users see no interactive controls.
- **SC-002**: When a user views a list of boards with mixed roles, each board independently shows the correct set of affordances for that user's role on that specific board.
- **SC-003**: Real-time updates deliver personalized board renders to each connected user with render cost proportional to the number of distinct roles per game (at most 3), not the number of connected viewers. Update delivery time remains comparable to the current generic broadcast.
- **SC-004**: Server-side authorization continues to reject 100% of unauthorized actions, ensuring the UI personalization does not create security regressions.
- **SC-005**: Authenticated users who are not assigned to a game see zero move controls on that game's board; they see reset/delete buttons only when board count > 6. Unauthenticated users see zero interactive controls of any kind.
- **SC-006**: Existing auto-assignment behavior (first move claims a slot) continues to work — unassigned users can still interact with boards where the unassigned slot's turn is active.

## Assumptions

- The existing cookie-based authentication system reliably identifies each user and will be used to determine their role per board.
- The current auto-assignment behavior (first user to move on an unassigned turn claims the player slot) is preserved unchanged.
- The delete button threshold (more than 6 boards) remains unchanged from current behavior.
- Real-time updates will use per-role server rendering: on each state change, the server renders at most 3 distinct HTML fragments per game (Player X view, Player O view, non-player view) and delivers the appropriate version to each connected subscriber. This replaces the current single-broadcast approach.
- The server-side authorization checks remain the authoritative security boundary; UI affordance personalization is a usability enhancement, not a security mechanism.
- Observer views have no move controls. Observers gain access to reset and delete buttons when the board count exceeds 6, consistent with prior requirements. Unauthenticated users (e.g., expired cookie) see no affordances at all.
- The number of game boards can grow unbounded. The per-role rendering approach ensures render cost scales with distinct roles (constant at 3 per game), not with viewer count. All non-player viewers share the same rendered content, making role a natural cache key.

## Dependencies

- This feature depends on the existing player assignment system (PlayerAssignmentManager) to determine each user's role per board.
- This feature depends on the existing authentication system to identify the current user.
- This feature depends on the existing real-time update mechanism (SSE), which will need to support per-user rendering.
