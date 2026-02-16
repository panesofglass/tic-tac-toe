# Feature Specification: Extract SCXML Statecharts Definition

**Feature Branch**: `009-extract-scxml`
**Created**: 2026-02-15
**Status**: Draft
**Input**: User description: "Reflect over the implementation of the game across both the TicTacToe.Engine and TicTacToe.Web projects. Extract a Statecharts definition (SCXML) for the application, especially the interactive states. This is a step toward GitHub issue #10. I want to extract this definition so that I can visualize the implicit statecharts and use that as a guide for developing a prototype Frank.Statecharts library through testing on this game."

## Clarifications

### Session 2026-02-15

- Q: Should the primary authoring format be smcat DSL (generating SCXML + SVG) or hand-authored SCXML (using smcat only for visualization)? → A: SCXML as hand-authored source of truth; use smcat to convert SCXML to SVG for README visualization. smcat lacks data model, executable content, conditional expressions, and external communications needed to model the full HTTP application semantics for the Frank.Statecharts library prototype.
- Q: Should the README show one combined diagram (all three parallel regions) or separate diagrams per region? → A: One combined diagram showing all three parallel regions (Game Play, Player Identity, Game Session Lifecycle) in a single SVG. This best represents how the regions interact simultaneously.
- Constraint: The statecharts MUST reflect conceptual divisions (models, events, domain concerns), NOT implementation boundaries (project splits, file organization, class structure). The implementation is a reference for extracting behavior, but the statechart models the domain, not the code.
- Q: Are the three parallel regions (Game Play, Player Identity, Game Session Lifecycle) the right conceptual model, or do they merely mirror the implementation structure? → A: The three regions are confirmed as genuinely orthogonal domain concepts: game rules (play) are identity-agnostic, who's playing (identity) is an application concern layered on top, and game creation/destruction (session) is independent of both.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Visualize Game Play State Machine (Priority: P1)

As a library developer, I want an SCXML definition that captures the complete game play state machine (turns, win, draw, error) so that I can load it into a statecharts visualization tool and see the full game lifecycle at a glance.

**Why this priority**: The core game play state machine is the foundation for all other statecharts. Without an accurate definition of how the game progresses through turns to an outcome, downstream work (player identity, session lifecycle) has no structural anchor. This is the minimum viable deliverable that enables visualization and drives the Frank.Statecharts prototype.

**Independent Test**: Can be fully tested by loading the SCXML file into a standard SCXML-compatible visualizer and confirming that the rendered diagram matches the known game transitions. Additionally, running `smcat -I scxml -T svg` on the SCXML file must produce a valid SVG. Delivers immediate value as a visual reference.

**Acceptance Scenarios**:

1. **Given** the SCXML file for the game play state machine, **When** loaded into a standard SCXML-compatible visualizer, **Then** it renders a valid state diagram showing all game states and transitions
2. **Given** the game play definition, **When** tracing the path "start -> X moves -> O moves -> X wins", **Then** each transition and guard condition is represented in the diagram
3. **Given** the game play definition, **When** reviewing terminal states (Won, Draw), **Then** no outgoing transitions exist from those states
4. **Given** the game play definition, **When** an invalid move occurs, **Then** the MoveError state loops back to the originating turn state with the game state unchanged
5. **Given** the SCXML file, **When** converted to SVG via `smcat -I scxml -T svg`, **Then** a valid SVG image is produced that can be embedded in the README

---

### User Story 2 - Capture Player Identity State Machine (Priority: P2)

As a library developer, I want the SCXML definition to include the player identity lifecycle (unassigned, partially assigned, fully assigned) as a parallel state region so that I can see how player identity interacts with game progression.

**Why this priority**: Player identity is an orthogonal domain concern that determines who can take actions and what each participant sees. Modeling it as a parallel region demonstrates compound statechart capabilities needed for the Frank.Statecharts prototype.

**Independent Test**: Can be tested by verifying that the SCXML defines a parallel region for player identity with correct states and transitions, and that the visualizer renders it alongside the game play state machine.

**Acceptance Scenarios**:

1. **Given** the SCXML file, **When** viewing the player identity region, **Then** it shows states for Unassigned, X-Only-Assigned, O-Only-Assigned, and Both-Assigned
2. **Given** an Unassigned game, **When** a user makes a move on X's turn, **Then** the player identity transitions to X-Only-Assigned
3. **Given** X-Only-Assigned, **When** a different user makes a move on O's turn, **Then** the player identity transitions to Both-Assigned

---

### User Story 3 - Model Game Session Lifecycle (Priority: P3)

As a library developer, I want the SCXML definition to capture the game session lifecycle (creation, active play, reset, disposal) so that the full application lifecycle is documented, not just the turn-by-turn game logic.

**Why this priority**: The session lifecycle adds game management concepts (creation, disposal, reset) that go beyond the turn-by-turn state machine. Including these provides a complete application model for testing the Frank.Statecharts library against real-world complexity.

**Independent Test**: Can be tested by verifying that game creation, disposal, reset events appear as transitions in the SCXML definition, and that guards (minimum game count, participant authorization) are documented.

**Acceptance Scenarios**:

1. **Given** the SCXML file, **When** reviewing the game session region, **Then** it includes states and transitions for game creation, active gameplay, reset, and disposal
2. **Given** an active game with both players assigned, **When** a participant requests disposal, **Then** the transition guard requires the user to be an assigned player
3. **Given** fewer than 7 active games, **When** a spectator attempts to dispose a game, **Then** the transition guard blocks disposal (minimum 6 games maintained)

---

### User Story 4 - README Statechart Visualization (Priority: P1)

As a project contributor, I want an SVG state diagram embedded in the project README so that I can immediately see the application's statechart without needing to open separate tools.

**Why this priority**: The visualization is the primary way the SCXML definition delivers value to the project. Without it, the SCXML file is useful only to those with specialized tools. This is co-priority with User Story 1 because the visual is the user-facing deliverable.

**Independent Test**: Can be tested by viewing the project README on GitHub and confirming the state diagram renders correctly as an inline image.

**Acceptance Scenarios**:

1. **Given** the SCXML file has been converted to SVG via smcat, **When** viewing the project README on GitHub, **Then** the state diagram renders inline as a visible image
2. **Given** the SVG image in the README, **When** a developer reviews it, **Then** all three parallel regions (Game Play, Player Identity, Game Session Lifecycle) are visible in a single combined diagram with labeled states and transitions

---

### Edge Cases

- What happens when a game reset occurs mid-game? The old game session transitions to a terminal "disposed" state, and a new game session begins in its initial state
- How does the statechart represent the Error state? MoveError is modeled as a transient pseudo-state that returns to the originating turn state, since it does not alter the game state
- What happens when a user who is already assigned as X tries to claim O? The move validation rejects with NotYourTurn, and no state transition occurs in the player identity region
- How are spectator interactions represented? Spectators cannot trigger game play transitions; their interactions are limited to observation and conditional game session management (disposal/reset with >6 games guard)
- What happens if smcat cannot fully render all SCXML features in the SVG? The SVG will show the state structure and transitions (which smcat supports) while the full SCXML file retains data model, executable content, and guard expressions that smcat's visualization omits

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The primary deliverable MUST be a hand-authored SCXML document conforming to the W3C State Chart XML specification, serving as the source of truth for the application's statechart
- **FR-002**: The SCXML definition MUST model all five game play states: X's Turn, O's Turn, Won, Draw, and MoveError
- **FR-003**: The SCXML definition MUST model all valid transitions between game play states, including guards (valid move checks, turn enforcement)
- **FR-004**: The SCXML definition MUST model terminal states (Won, Draw) as final states with no outgoing transitions
- **FR-005**: The SCXML definition MUST represent the MoveError state as a transient state that returns to the originating turn state via a history pseudo-state
- **FR-006**: The SCXML definition MUST include a parallel state region for player identity with states: Unassigned, X-Only-Assigned, O-Only-Assigned, Both-Assigned
- **FR-007**: The SCXML definition MUST document transition guards for move validation (turn enforcement, player authorization, square availability)
- **FR-008**: The SCXML definition MUST include game session lifecycle events: game creation, game disposal (with minimum-count guard), and game reset (dispose old, create new)
- **FR-009**: The SCXML definition MUST be loadable by at least one standard SCXML visualization tool without errors
- **FR-010**: The SCXML definition MUST capture the initial state (X's Turn with all squares available) as the default entry point for the game play state machine
- **FR-011**: The SCXML definition MUST document move rejection reasons (NotYourTurn, NotAPlayer) as transition conditions in the game play region via the MoveError transient state
- **FR-012**: The SCXML file MUST be convertible to SVG via `smcat -I scxml -T svg` for visualization
- **FR-013**: The project README MUST include an inline SVG state diagram generated from the SCXML definition
- **FR-014**: The SCXML definition MUST use SCXML data model capabilities (`<datamodel>`, `<data>`) to represent game data (board state, player identities, game count) where these inform the Frank.Statecharts library design
- **FR-015**: The statechart structure MUST reflect conceptual domain divisions (Game Play, Player Identity, Game Session Lifecycle), not implementation boundaries (project structure, file organization, class hierarchy)

### Key Entities

- **Game Play State**: The current board configuration and turn status. Five possible states: XTurn (X to move), OTurn (O to move), Won (game over with winner), Draw (game over, no winner), MoveError (invalid move attempted, state unchanged)
- **Player Identity**: Tracks which participants are playing X and O. Four possible states: Unassigned (no players), X-Only (one player as X), O-Only (one player as O), Both-Assigned (two players). Orthogonal to game play rules.
- **Move**: A participant action targeting a board square. Validated against game play state (square availability, correct turn) and player identity (participant authorization)
- **Game Session Event**: Domain-level actions that create, reset, or dispose game sessions. Subject to guards (participant authorization, minimum game count). Independent of in-game play state.
- **Board Square**: One of nine positions on the game board. Three possible states: Empty, Taken by X, Taken by O

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The SCXML file loads and renders without errors in at least one statecharts visualization tool
- **SC-002**: 100% of game play state transitions identified in the codebase are represented in the SCXML definition (5 states, all valid transitions)
- **SC-003**: 100% of player identity states and transitions are represented as a parallel region in the SCXML definition (4 states)
- **SC-004**: All transition guards identified in the codebase (turn enforcement, participant authorization, square availability, minimum game count) appear as guard conditions in the SCXML definition
- **SC-005**: A developer unfamiliar with the codebase can trace any game scenario (e.g., "X wins in 5 moves", "invalid move rejected", "spectator tries to dispose game") through the SCXML visualization and arrive at the correct final state
- **SC-006**: The project README displays an inline state diagram that renders correctly on GitHub
- **SC-007**: Running `smcat -I scxml -T svg` on the SCXML file produces a valid SVG without errors
- **SC-008**: The statechart's parallel regions correspond to domain concepts, not to project/file/class boundaries in the implementation

## Assumptions

- The SCXML is hand-authored (not generated) to fully leverage SCXML semantics including data model, executable content, and guard expressions that smcat's DSL cannot represent
- smcat is used solely as a visualization tool to convert SCXML to SVG for the README; it is not the authoring format
- The SVG visualization will show state structure and transitions but will necessarily omit SCXML-specific details (data model internals, executable content) that smcat does not render
- The primary consumer of the SCXML definition is the future Frank.Statecharts library prototype; the SVG in the README serves as project documentation
- The statechart models domain concepts, not implementation structure. Concurrency mechanics (MailboxProcessor, GameSupervisor), delivery mechanisms (SSE, HTTP), and code organization (Engine vs Web projects) are implementation details that the statechart abstracts away
- The three parallel regions (Game Play, Player Identity, Game Session Lifecycle) represent orthogonal domain concerns: game rules are identity-agnostic, player identity is an application concern layered on game rules, and session lifecycle is independent of both
- The 8 winning combinations (3 rows, 3 columns, 2 diagonals) are a guard detail within the "check for win" transition and do not need to be individually enumerated as separate transitions
- Game reset is modeled as a compound transition: dispose current game session (terminal) + create new game session (initial state), not as a single state within one session's lifecycle
