# Tasks: Multi-Player Tic-Tac-Toe

**Input**: Design documents from `/specs/005-multi-player/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Included per Constitution Principle III (Test-First Development)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Web app**: `src/TicTacToe.Web/` (modification target)
- **Engine**: `src/TicTacToe.Engine/` (PROTECTED - DO NOT MODIFY)
- **Tests**: `test/TicTacToe.Web.Tests/`

---

## Phase 1: Setup

**Purpose**: Create new files and test infrastructure for multi-player feature

- [x] T001 Create PlayerAssignment types file at src/TicTacToe.Web/Model.fs
- [x] T002 [P] Create multi-player test file at test/TicTacToe.Web.Tests/MultiPlayerTests.fs
- [x] T003 [P] Create player assignment unit test file at test/TicTacToe.Web.Tests/PlayerAssignmentTests.fs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Tests for Foundation

- [x] T004 [P] Write failing unit tests for PlayerAssignment record type in test/TicTacToe.Web.Tests/PlayerAssignmentTests.fs
- [x] T005 [P] Write failing unit tests for PlayerRole discriminated union in test/TicTacToe.Web.Tests/PlayerAssignmentTests.fs
- [x] T006 [P] Write failing unit tests for MoveValidationResult type in test/TicTacToe.Web.Tests/PlayerAssignmentTests.fs

### Implementation for Foundation

- [x] T007 Implement PlayerAssignment record type in src/TicTacToe.Web/Model.fs (per data-model.md)
- [x] T008 Implement PlayerRole discriminated union in src/TicTacToe.Web/Model.fs (per data-model.md)
- [x] T009 Implement MoveValidationResult and RejectionReason types in src/TicTacToe.Web/Model.fs (per data-model.md)
- [x] T010 Implement PlayerAssignmentMessage type for MailboxProcessor in src/TicTacToe.Web/Model.fs
- [x] T011 Write failing unit tests for PlayerAssignmentManager in test/TicTacToe.Web.Tests/PlayerAssignmentTests.fs
- [x] T011a [P] Write failing unit test: PlayerAssignmentManager retains assignments across multiple queries in test/TicTacToe.Web.Tests/PlayerAssignmentTests.fs
- [x] T012 Implement PlayerAssignmentManager (MailboxProcessor) in src/TicTacToe.Web/Model.fs
- [x] T013 Register PlayerAssignmentManager as singleton service in src/TicTacToe.Web/Program.fs
- [x] T014 Verify all foundational unit tests pass

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - First Player Claims X (Priority: P1) 🎯 MVP

**Goal**: First visitor to make a move becomes Player X

**Independent Test**: Single browser makes move on empty board → recorded as X, browser recognized as Player X on subsequent interactions

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T015 [P] [US1] Write failing unit test: first move assigns user as Player X in test/TicTacToe.Web.Tests/PlayerAssignmentTests.fs
- [x] T016 [P] [US1] Write failing unit test: getUserRole returns UnassignedX for new game in test/TicTacToe.Web.Tests/PlayerAssignmentTests.fs
- [ ] T017 [P] [US1] Write failing integration test: first move stores X assignment in test/TicTacToe.Web.Tests/MultiPlayerTests.fs
- [ ] T017a [P] [US1] Write failing integration test: Player X recognized after page refresh in test/TicTacToe.Web.Tests/MultiPlayerTests.fs

### Implementation for User Story 1

- [x] T018 [US1] Add validateAndAssignPlayer function in src/TicTacToe.Web/Model.fs for X assignment logic
- [x] T019 [US1] Modify makeMove handler in src/TicTacToe.Web/Handlers.fs to extract userId via ctx.User.TryGetUserId()
- [x] T020 [US1] Integrate PlayerAssignmentManager into makeMove handler in src/TicTacToe.Web/Handlers.fs
- [x] T021 [US1] Implement X-turn assignment path in makeMove handler in src/TicTacToe.Web/Handlers.fs
- [ ] T022 [US1] Verify all User Story 1 tests pass

**Checkpoint**: User Story 1 complete - first player assignment works independently

---

## Phase 4: User Story 2 - Second Player Claims O (Priority: P1)

**Goal**: Second distinct visitor to make a move becomes Player O

**Independent Test**: After X's first move, different browser makes move → recorded as O, browser recognized as Player O

### Tests for User Story 2

- [ ] T023 [P] [US2] Write failing unit test: second move by different user assigns as Player O in test/TicTacToe.Web.Tests/PlayerAssignmentTests.fs
- [ ] T024 [P] [US2] Write failing unit test: same user cannot claim both X and O in test/TicTacToe.Web.Tests/PlayerAssignmentTests.fs
- [ ] T025 [P] [US2] Write failing integration test: two-browser game setup in test/TicTacToe.Web.Tests/MultiPlayerTests.fs

### Implementation for User Story 2

- [ ] T026 [US2] Extend validateAndAssignPlayer for O assignment logic in src/TicTacToe.Web/Model.fs
- [ ] T027 [US2] Implement O-turn assignment path in makeMove handler in src/TicTacToe.Web/Handlers.fs
- [ ] T028 [US2] Add invariant check: PlayerXId ≠ PlayerOId in src/TicTacToe.Web/Model.fs
- [ ] T029 [US2] Verify all User Story 2 tests pass

**Checkpoint**: User Story 2 complete - both players can be assigned independently

---

## Phase 5: User Story 3 - Turn Enforcement (Priority: P1)

**Goal**: Only the player whose turn it is can make a move

**Independent Test**: Player X attempts two consecutive moves → second is rejected

### Tests for User Story 3

- [ ] T030 [P] [US3] Write failing unit test: Player X cannot move on O's turn in test/TicTacToe.Web.Tests/PlayerAssignmentTests.fs
- [ ] T031 [P] [US3] Write failing unit test: Player O cannot move on X's turn in test/TicTacToe.Web.Tests/PlayerAssignmentTests.fs
- [ ] T032 [P] [US3] Write failing integration test: wrong-turn move is rejected in test/TicTacToe.Web.Tests/MultiPlayerTests.fs

### Implementation for User Story 3

- [ ] T033 [US3] Add turn validation logic to validateAndAssignPlayer in src/TicTacToe.Web/Model.fs
- [ ] T034 [US3] Query game state via Engine GetState in makeMove handler in src/TicTacToe.Web/Handlers.fs
- [ ] T035 [US3] Return NotYourTurn rejection when assigned player moves out of turn in src/TicTacToe.Web/Handlers.fs
- [ ] T036 [US3] Verify all User Story 3 tests pass

**Checkpoint**: User Story 3 complete - turn enforcement works independently

---

## Phase 6: User Story 4 - Third Party Exclusion (Priority: P2)

**Goal**: Once both players assigned, third visitors cannot make moves

**Independent Test**: Third browser attempts move in locked game → rejected, can still view

### Tests for User Story 4

- [ ] T037 [P] [US4] Write failing unit test: third user rejected as NotAPlayer in test/TicTacToe.Web.Tests/PlayerAssignmentTests.fs
- [ ] T038 [P] [US4] Write failing unit test: getUserRole returns Spectator for third user in test/TicTacToe.Web.Tests/PlayerAssignmentTests.fs
- [ ] T039 [P] [US4] Write failing integration test: spectator sees board but cannot interact in test/TicTacToe.Web.Tests/MultiPlayerTests.fs

### Implementation for User Story 4

- [ ] T040 [US4] Add spectator detection to validateAndAssignPlayer in src/TicTacToe.Web/Model.fs
- [ ] T041 [US4] Return NotAPlayer rejection for unauthorized users in src/TicTacToe.Web/Handlers.fs
- [ ] T042 [US4] Add CSS shake animation for rejection feedback in src/TicTacToe.Web/templates/game.fs
- [ ] T043 [US4] Broadcast rejection animation via SSE PatchSignals in src/TicTacToe.Web/Handlers.fs
- [ ] T044 [US4] Verify all User Story 4 tests pass

**Checkpoint**: User Story 4 complete - third parties are excluded

---

## Phase 7: User Story 5 - Spectator Experience (Priority: P3)

**Goal**: Non-players can watch game updates in real-time

**Independent Test**: Third browser observes game, sees moves as they happen

### Tests for User Story 5

- [ ] T045 [P] [US5] Write failing integration test: spectator receives SSE updates in test/TicTacToe.Web.Tests/MultiPlayerTests.fs
- [ ] T046 [P] [US5] Write failing integration test: spectator sees game completion in test/TicTacToe.Web.Tests/MultiPlayerTests.fs

### Implementation for User Story 5

- [ ] T047 [US5] Add viewerRole signal to game board rendering in src/TicTacToe.Web/templates/game.fs
- [ ] T048 [US5] Modify getGame handler to include viewer role in response in src/TicTacToe.Web/Handlers.fs
- [ ] T049 [US5] Add canMove signal based on role and turn in src/TicTacToe.Web/templates/game.fs
- [ ] T050 [US5] Conditionally render move controls based on viewerRole in src/TicTacToe.Web/templates/game.fs
- [ ] T051 [US5] Verify all User Story 5 tests pass

**Checkpoint**: User Story 5 complete - spectators can watch games

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T052 [P] Add edge case tests: cookie cleared mid-game in test/TicTacToe.Web.Tests/MultiPlayerTests.fs
- [ ] T053 [P] Add edge case tests: same user two browsers in test/TicTacToe.Web.Tests/MultiPlayerTests.fs
- [ ] T054 Add cleanup: remove PlayerAssignment when game deleted in src/TicTacToe.Web/Handlers.fs
- [ ] T055 Run all tests and verify green
- [ ] T056 Run quickstart.md validation scenarios manually

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - US1 and US2 and US3 are all P1 priority - core functionality
  - US4 is P2 - can start after foundation, independent of US1-3
  - US5 is P3 - can start after foundation, independent of US1-4
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Foundation only - No dependencies on other stories
- **User Story 2 (P1)**: Foundation + logically follows US1 (needs X assigned first)
- **User Story 3 (P1)**: Foundation + logically follows US2 (needs both players assigned)
- **User Story 4 (P2)**: Foundation + logically follows US3 (needs turn enforcement)
- **User Story 5 (P3)**: Foundation only - Can be worked in parallel with US4

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Types/models before logic
- Logic before handler integration
- Handler integration before UI updates
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel (T002, T003)
- All Foundational tests marked [P] can run in parallel (T004, T005, T006)
- All US1 tests marked [P] can run in parallel (T015, T016, T017)
- All US2 tests marked [P] can run in parallel (T023, T024, T025)
- All US3 tests marked [P] can run in parallel (T030, T031, T032)
- All US4 tests marked [P] can run in parallel (T037, T038, T039)
- All US5 tests marked [P] can run in parallel (T045, T046)
- All Polish tests marked [P] can run in parallel (T052, T053)

---

## Parallel Example: User Story 3

```bash
# Launch all tests for User Story 3 together:
Task: "Write failing unit test: Player X cannot move on O's turn in test/TicTacToe.Web.Tests/PlayerAssignmentTests.fs"
Task: "Write failing unit test: Player O cannot move on X's turn in test/TicTacToe.Web.Tests/PlayerAssignmentTests.fs"
Task: "Write failing integration test: wrong-turn move is rejected in test/TicTacToe.Web.Tests/MultiPlayerTests.fs"
```

---

## Implementation Strategy

### MVP First (User Stories 1-3 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (First Player Claims X)
4. Complete Phase 4: User Story 2 (Second Player Claims O)
5. Complete Phase 5: User Story 3 (Turn Enforcement)
6. **STOP and VALIDATE**: Test all P1 stories independently
7. Deploy/demo if ready - two players can play a complete game

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → First player works
3. Add User Story 2 → Test independently → Both players work
4. Add User Story 3 → Test independently → Turn enforcement works (MVP!)
5. Add User Story 4 → Test independently → Third parties excluded
6. Add User Story 5 → Test independently → Spectators can watch
7. Each story adds value without breaking previous stories

### Single Developer Strategy

Execute in strict order: Setup → Foundation → US1 → US2 → US3 → US4 → US5 → Polish

Commit after each phase checkpoint.

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing (Red-Green-Refactor per Constitution)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- **CRITICAL**: Do not modify any files in src/TicTacToe.Engine/ (protected per constitution)
