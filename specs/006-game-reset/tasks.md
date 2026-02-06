# Tasks: Game Reset and Persistent Game Boards

**Input**: Design documents from `/specs/006-game-reset/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api.md

**Tests**: Constitution mandates Test-First Development (Principle III). Playwright tests are written before implementation.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

```text
src/TicTacToe.Web/           # Web application code
test/TicTacToe.Web.Tests/    # Playwright integration tests
```

---

## Phase 1: Setup

**Purpose**: Prepare test infrastructure for new feature

- [ ] T001 Create test file `test/TicTacToe.Web.Tests/ResetGameTests.fs` with NUnit test fixture structure
- [ ] T002 [P] Create test file `test/TicTacToe.Web.Tests/InitialGamesTests.fs` with NUnit test fixture structure

**Checkpoint**: Test files ready for test implementation

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Template changes that enable all user stories

**⚠️ CRITICAL**: Button rendering changes must be complete before testing user stories

- [ ] T003 Add `renderGameBoardWithContext` function signature to `src/TicTacToe.Web/templates/game.fs` that accepts `userRole: PlayerRole` and `gameCount: int` parameters
- [ ] T004 Add helper functions `hasMovesOrPlayers`, `canReset`, `canDelete` to `src/TicTacToe.Web/templates/game.fs` per data-model.md
- [ ] T005 Replace Delete button with Reset and Delete buttons in `src/TicTacToe.Web/templates/game.fs` controls div
- [ ] T006 Add CSS styles for `.reset-game-btn` and disabled button states in `src/TicTacToe.Web/templates/game.fs` gameStyles
- [ ] T007 Add route `POST /games/{id}/reset` to `src/TicTacToe.Web/Program.fs` routing configuration

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Reset a Completed Game (Priority: P1) 🎯 MVP

**Goal**: Players can reset a game to start fresh in the same position

**Independent Test**: Play a game to completion, click Reset, verify fresh game appears with X's turn

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T008 [US1] Write Playwright test `Reset button creates new game in same position` in `test/TicTacToe.Web.Tests/ResetGameTests.fs`
- [ ] T009 [P] [US1] Write Playwright test `Reset clears player assignments and shows X's turn` in `test/TicTacToe.Web.Tests/ResetGameTests.fs`
- [ ] T010 [P] [US1] Write Playwright test `Reset broadcasts to all connected clients` in `test/TicTacToe.Web.Tests/ResetGameTests.fs`

### Implementation for User Story 1

- [ ] T011 [US1] Implement `resetGame` handler in `src/TicTacToe.Web/Handlers.fs` with game disposal, new game creation, and SSE broadcast
- [ ] T012 [US1] Add authorization check in `resetGame` handler to verify user is PlayerX or PlayerO via `PlayerAssignmentManager.GetRole`
- [ ] T013 [US1] Update `subscribeToGame` function in `src/TicTacToe.Web/Handlers.fs` to support reset flow (new game subscription)
- [ ] T014 [US1] Wire `resetGame` handler to route in `src/TicTacToe.Web/Program.fs` with `requiresAuth` wrapper

**Checkpoint**: Reset functionality works - users can reset games they're playing

---

## Phase 4: User Story 2 - Initial Page Load with Six Games (Priority: P2)

**Goal**: Users see six game boards immediately on page load

**Independent Test**: Load home page, count exactly six game boards visible

### Tests for User Story 2

- [ ] T015 [US2] Write Playwright test `Home page shows exactly six game boards on load` in `test/TicTacToe.Web.Tests/InitialGamesTests.fs`
- [ ] T016 [P] [US2] Write Playwright test `All six initial games show X's turn` in `test/TicTacToe.Web.Tests/InitialGamesTests.fs`

### Implementation for User Story 2

- [ ] T017 [US2] Add `createInitialGames` function to `src/TicTacToe.Web/Handlers.fs` that creates 6 games via supervisor
- [ ] T018 [US2] Register `IHostApplicationLifetime.ApplicationStarted` handler in `src/TicTacToe.Web/Program.fs` to call `createInitialGames`
- [ ] T019 [US2] Update `sse` handler in `src/TicTacToe.Web/Handlers.fs` to send all existing games on connect (not just empty container)

**Checkpoint**: Six games appear on startup - users have immediate gameplay options

---

## Phase 5: User Story 3 - Maintain Minimum Six Games (Priority: P2)

**Goal**: System always maintains at least six games

**Independent Test**: Reset games repeatedly, count never drops below six

### Tests for User Story 3

- [ ] T020 [US3] Write Playwright test `Reset maintains six game count` in `test/TicTacToe.Web.Tests/ResetGameTests.fs`

### Implementation for User Story 3

- [ ] T021 [US3] Verify reset flow in `resetGame` handler creates new game before disposing old (count never drops) in `src/TicTacToe.Web/Handlers.fs`

**Checkpoint**: Game count stability verified

---

## Phase 6: User Story 4 - Prevent Reset on Unplayed Games (Priority: P3)

**Goal**: Reset button disabled when game has no moves and no assigned players

**Independent Test**: View fresh game, verify Reset button is disabled

### Tests for User Story 4

- [ ] T022 [US4] Write Playwright test `Reset button disabled on fresh game with no players` in `test/TicTacToe.Web.Tests/ResetGameTests.fs`
- [ ] T023 [P] [US4] Write Playwright test `Reset button enabled after first move` in `test/TicTacToe.Web.Tests/ResetGameTests.fs`
- [ ] T024 [P] [US4] Write Playwright test `Reset button disabled for spectators` in `test/TicTacToe.Web.Tests/ResetGameTests.fs`

### Implementation for User Story 4

- [ ] T025 [US4] Update `renderGameBoardWithContext` in `src/TicTacToe.Web/templates/game.fs` to set Reset button disabled attribute based on `canReset` logic
- [ ] T026 [US4] Update all handlers that render game boards to pass correct `userRole` and `gameCount` to template in `src/TicTacToe.Web/Handlers.fs`
- [ ] T027 [US4] Add validation in `resetGame` handler to return 403 if game has no moves and no assigned players

**Checkpoint**: Reset button state correctly reflects authorization

---

## Phase 7: User Story 5 - Delete Extra Games (Priority: P3)

**Goal**: Users can delete games when count exceeds six

**Independent Test**: Create 7 games, delete one, verify 6 remain with Delete disabled

### Tests for User Story 5

- [ ] T028 [US5] Write Playwright test `Delete button disabled when exactly six games` in `test/TicTacToe.Web.Tests/MultiGameTests.fs`
- [ ] T029 [P] [US5] Write Playwright test `Delete button enabled when more than six games` in `test/TicTacToe.Web.Tests/MultiGameTests.fs`
- [ ] T030 [P] [US5] Write Playwright test `Delete removes game and updates button state` in `test/TicTacToe.Web.Tests/MultiGameTests.fs`
- [ ] T031 [P] [US5] Write Playwright test `Delete returns 409 when would drop below six` in `test/TicTacToe.Web.Tests/RestApiTests.fs`

### Implementation for User Story 5

- [ ] T032 [US5] Update `deleteGame` handler in `src/TicTacToe.Web/Handlers.fs` to check `supervisor.GetActiveGameCount() > 6` before deletion
- [ ] T033 [US5] Update `deleteGame` handler to return 409 Conflict if deletion would reduce count below 6
- [ ] T034 [US5] Update `deleteGame` handler to verify user is PlayerX or PlayerO before allowing deletion
- [ ] T035 [US5] Update `renderGameBoardWithContext` in `src/TicTacToe.Web/templates/game.fs` to set Delete button disabled based on `canDelete` logic

**Checkpoint**: Delete functionality works with proper constraints

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and cleanup

- [X] T036 [P] Run all Playwright tests to verify all user stories work together
- [X] T037 [P] Verify SSE broadcasts work correctly for multi-client scenarios
- [ ] T038 Run quickstart.md verification steps manually
- [X] T039 Update existing tests in `test/TicTacToe.Web.Tests/MultiGameTests.fs` if any break due to new behavior; verify FR-011 (New Game button remains functional for creating games beyond initial six)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phases 3-7)**: All depend on Foundational phase completion
  - US1 (Reset) can proceed independently
  - US2 (Initial Games) can proceed independently
  - US3 (Maintain Six) depends on US1 being testable
  - US4 (Prevent Reset) depends on US1 implementation
  - US5 (Delete Extra) can proceed independently
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational - No dependencies on other stories
- **User Story 3 (P2)**: Depends on US1 reset flow being implemented
- **User Story 4 (P3)**: Depends on US1 reset handler existing
- **User Story 5 (P3)**: Can start after Foundational - No dependencies on other stories

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Handler implementation before template integration
- Story complete before moving to next priority

### Parallel Opportunities

- T001, T002 can run in parallel (different test files)
- T008, T009, T010 can run in parallel (different test cases)
- T015, T016 can run in parallel (different test cases)
- T022, T023, T024 can run in parallel (different test cases)
- T028, T029, T030, T031 can run in parallel (different test cases)
- T036, T037 can run in parallel (different verification areas)

---

## Parallel Example: User Story 1 Tests

```bash
# Launch all US1 tests together:
Task: "Write Playwright test Reset button creates new game in same position"
Task: "Write Playwright test Reset clears player assignments and shows X's turn"
Task: "Write Playwright test Reset broadcasts to all connected clients"
```

## Parallel Example: User Story 5 Tests

```bash
# Launch all US5 tests together:
Task: "Write Playwright test Delete button disabled when exactly six games"
Task: "Write Playwright test Delete button enabled when more than six games"
Task: "Write Playwright test Delete removes game and updates button state"
Task: "Write Playwright test Delete returns 409 when would drop below six"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (Reset functionality)
4. **STOP and VALIDATE**: Test Reset independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test → Deploy (MVP - Reset works!)
3. Add User Story 2 → Test → Deploy (Initial 6 games!)
4. Add User Stories 3-5 → Test → Deploy (Full feature!)

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Reset)
   - Developer B: User Story 2 (Initial Games)
   - Developer C: User Story 5 (Delete)
3. After US1: Developer A picks up US3 and US4
4. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Engine (TicTacToe.Engine) is PROTECTED - no changes allowed
