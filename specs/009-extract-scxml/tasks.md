# Tasks: Extract SCXML Statecharts Definition

**Input**: Design documents from `/specs/009-extract-scxml/`
**Prerequisites**: plan.md (required), spec.md (required), data-model.md, research.md, quickstart.md

**Tests**: No automated tests — this is a documentation artifact. Validation is via `smcat -I scxml -T svg` conversion and manual scenario tracing.

**Organization**: Tasks are grouped by user story. Each story incrementally adds a parallel region to the same SCXML file, then US4 generates the final SVG and README.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Create the docs/ directory and SCXML document skeleton

- [x] T001 Create docs/ directory and initial SCXML skeleton file at docs/statechart.scxml with XML declaration, `<scxml>` root element (`xmlns="http://www.w3.org/2005/07/scxml"`, `version="1.0"`, `datamodel="ecmascript"`), and an empty `<parallel id="TicTacToe">` element as the initial target

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish the ECMAScript data model that all three parallel regions depend on

**CRITICAL**: The datamodel must be complete before any region can reference data variables in guards or assignments

- [x] T002 Add `<datamodel>` element inside `<scxml>` with all data variables in docs/statechart.scxml: `board` (object with 9 position keys all set to `'Empty'`), `playerXId` (null), `playerOId` (null), `gameCount` (6), `winner` (null) — per data-model.md Data Variables table

**Checkpoint**: SCXML skeleton with datamodel ready — region implementation can begin

---

## Phase 3: User Story 1 - Visualize Game Play State Machine (Priority: P1) MVP

**Goal**: The SCXML models all 5 game play states (XTurn, OTurn, Won, Draw, MoveError) with transitions, guards, and a history pseudo-state for transient error recovery

**Independent Test**: `smcat -I scxml -T svg docs/statechart.scxml` produces SVG showing XTurn, OTurn, Won, Draw, MoveError states with labeled transitions and guard expressions

### Implementation for User Story 1

- [x] T003 [US1] Write the `<state id="GamePlay" initial="XTurn">` compound state inside `<parallel id="TicTacToe">` in docs/statechart.scxml containing: `<history id="GamePlayHistory" type="shallow">` with default transition to XTurn; `<state id="XTurn">` with ordered transitions (win → draw → continue → error) per data-model.md Game Play Transitions table; `<state id="OTurn">` with matching ordered transitions; `<state id="MoveError">` with eventless transition to GamePlayHistory; `<final id="Won"/>`; `<final id="Draw"/>`. Guard expressions use ECMAScript (`board[_event.data.position] === 'Empty'`, `wouldWin()`, `wouldFillBoard()`). Transitions include `<assign>` elements for board updates and winner assignment.
- [x] T004 [US1] Validate Game Play region by running `smcat -I scxml -T svg docs/statechart.scxml -o /tmp/us1-check.svg` and visually confirming: XTurn and OTurn states appear with transitions between them; Won and Draw appear as final states; MoveError appears with history connection; guard labels visible on transitions. Fix any smcat parse errors before proceeding.

**Checkpoint**: Game Play region is complete and renderable. This alone delivers MVP visualization value.

---

## Phase 4: User Story 2 - Capture Player Identity State Machine (Priority: P2)

**Goal**: A parallel region for player identity with 4 states (Unassigned, XOnlyAssigned, OOnlyAssigned, BothAssigned) runs orthogonally alongside Game Play

**Independent Test**: `smcat -I scxml -T svg` produces SVG showing two parallel regions side-by-side with dashed borders — Game Play and Player Identity

### Implementation for User Story 2

- [x] T005 [US2] Add `<state id="PlayerIdentity" initial="Unassigned">` as a sibling of GamePlay inside `<parallel id="TicTacToe">` in docs/statechart.scxml containing: `<state id="Unassigned">` with transitions on `move.x` (assign playerXId → XOnlyAssigned) and `move.o` (assign playerOId → OOnlyAssigned); `<state id="XOnlyAssigned">` with transition on `move.o` [different user] → BothAssigned; `<state id="OOnlyAssigned">` with transition on `move.x` [different user] → BothAssigned; `<state id="BothAssigned"/>` (no outgoing transitions, no final — persists for session lifetime). Guards and assignments per data-model.md Player Identity Transitions table.
- [x] T006 [US2] Validate Player Identity region by running `smcat -I scxml -T svg docs/statechart.scxml -o /tmp/us2-check.svg` and visually confirming: two parallel regions render with dashed borders; PlayerIdentity shows all 4 states with correct transition labels; GamePlay region unchanged from US1 validation.

**Checkpoint**: Two parallel regions render correctly. Player identity assignment logic captured.

---

## Phase 5: User Story 3 - Model Game Session Lifecycle (Priority: P3)

**Goal**: A third parallel region for game session lifecycle with Active and Disposed states, capturing disposal/reset/timeout events with authorization and minimum-count guards

**Independent Test**: `smcat -I scxml -T svg` produces SVG showing all three parallel regions — Game Play, Player Identity, and Game Session Lifecycle

### Implementation for User Story 3

- [x] T007 [US3] Add `<state id="GameSession" initial="Active">` as a sibling of GamePlay and PlayerIdentity inside `<parallel id="TicTacToe">` in docs/statechart.scxml containing: `<state id="Active">` with transitions for `game.dispose` [participant or gameCount > 6] → Disposed, `game.reset` [participant and hasActivity] → Disposed, `game.timeout` (unconditional) → Disposed; `<final id="Disposed"/>`. Guards per data-model.md Game Session Transitions table. Use `isParticipant()` and `hasActivity()` conceptual guard functions in `cond` attributes.
- [x] T008 [US3] Validate Game Session region by running `smcat -I scxml -T svg docs/statechart.scxml -o /tmp/us3-check.svg` and visually confirming: three parallel regions render with dashed borders; GameSession shows Active → Disposed transitions with guard labels; all three regions coexist without rendering issues.

**Checkpoint**: Complete SCXML with all three parallel regions. Full application statechart captured.

---

## Phase 6: User Story 4 - README Statechart Visualization (Priority: P1)

**Goal**: An SVG state diagram embedded in the project README showing all three parallel regions in a single combined visualization

**Independent Test**: Viewing README.md on GitHub shows the statechart diagram rendering inline

### Implementation for User Story 4

- [x] T009 [US4] Generate production SVG by running `smcat -I scxml -T svg docs/statechart.scxml -o docs/statechart.svg`. If the diagram is too dense vertically, try `smcat -I scxml -T svg -d left-right docs/statechart.scxml -o docs/statechart.svg` and keep whichever layout is more readable. Commit the chosen docs/statechart.svg to the repository.
- [x] T010 [US4] Update README.md to embed the statechart SVG. Add a section (e.g., under a "## Statechart" heading or at an appropriate location) with `![Tic-Tac-Toe Statechart](docs/statechart.svg)` markdown syntax. Include a brief description explaining the three parallel regions and a note that the full SCXML source with data model and guard expressions is at `docs/statechart.scxml`.

**Checkpoint**: README displays the statechart diagram. Push branch and verify rendering on GitHub.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Validate the statechart against acceptance scenarios and conceptual purity requirements

- [x] T011 Trace "X wins in 5 moves" scenario through docs/statechart.scxml: verify XTurn → OTurn alternation, guard evaluation order (win checked first), Won final state reached, winner data variable set, PlayerIdentity transitions to BothAssigned, GameSession remains Active
- [x] T012 Trace "spectator tries to dispose game with fewer than 7 active games" scenario through docs/statechart.scxml: verify `game.dispose` guard `isParticipant(userId) || gameCount > 6` blocks the transition when user is not a participant and gameCount <= 6
- [x] T013 Review docs/statechart.scxml for conceptual purity per FR-015 and SC-008: verify no references to implementation boundaries (MailboxProcessor, GameSupervisor, PlayerAssignmentManager, Engine vs Web projects, HTTP handlers, SSE); all state names and event names reflect domain concepts

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 — BLOCKS all user stories (datamodel must exist before guards reference variables)
- **US1 (Phase 3)**: Depends on Phase 2 — first region in the SCXML
- **US2 (Phase 4)**: Depends on US1 — adds to the same `<parallel>` element after GamePlay exists
- **US3 (Phase 5)**: Depends on US2 — adds to the same `<parallel>` element after PlayerIdentity exists
- **US4 (Phase 6)**: Depends on US3 — generates SVG from the complete SCXML with all three regions
- **Polish (Phase 7)**: Depends on US4 — validates the complete deliverable

### User Story Dependencies

- **US1 (P1)**: Can start after Foundational — MVP deliverable
- **US2 (P2)**: Depends on US1 completion — extends same file with second parallel region
- **US3 (P3)**: Depends on US2 completion — extends same file with third parallel region
- **US4 (P1)**: Depends on US1+US2+US3 — generates combined SVG and README (could start after US1 for incremental preview, but final delivery requires all regions)

### Within Each User Story

1. Write the SCXML region (implementation task)
2. Validate with smcat conversion (validation task)
3. Fix any parse errors before proceeding to next story

### Parallel Opportunities

- **Limited parallelism** due to single-file nature: all regions are in docs/statechart.scxml
- T011, T012, T013 (Polish phase) can run in parallel — they are read-only scenario traces
- T009 and T010 (US4) are sequential (generate SVG before updating README)

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001)
2. Complete Phase 2: Foundational (T002)
3. Complete Phase 3: US1 Game Play (T003, T004)
4. **STOP and VALIDATE**: Run `smcat -I scxml -T svg` — game play states render correctly
5. This alone provides a usable statechart visualization of the core game logic

### Incremental Delivery

1. Setup + Foundational → SCXML skeleton with datamodel ready
2. Add US1 Game Play → Validate → First renderable statechart (MVP!)
3. Add US2 Player Identity → Validate → Two parallel regions visible
4. Add US3 Game Session → Validate → Complete three-region statechart
5. US4 README Integration → SVG committed, README updated
6. Polish → Scenario validation, conceptual purity review
7. Each story adds a parallel region without breaking previous regions

---

## Notes

- All implementation tasks target the same file: docs/statechart.scxml
- Validation tasks use `smcat -I scxml -T svg` as the acceptance gate
- SCXML evaluates transitions in document order — win guards MUST come before continue guards
- Guard expressions in `cond` attributes will appear as labels in the SVG (verified in research)
- smcat silently ignores `<datamodel>`, `<data>`, `<assign>` — these are preserved in SCXML but not visible in SVG
- Use `&amp;&amp;` for `&&` and `||` directly in XML attribute values for guard expressions
- Refer to data-model.md for exact state names, transition tables, guard expressions, and event catalog
