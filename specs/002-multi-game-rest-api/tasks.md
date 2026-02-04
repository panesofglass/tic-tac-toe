# Tasks: Multi-Game REST API

**Input**: Design documents from `/specs/002-multi-game-rest-api/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Included per constitution (Principle III: Test-First Development with Playwright)

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story (US1, US2, US3, US4)
- File paths are absolute from repository root

---

## Phase 1: Setup

**Purpose**: Prepare infrastructure for multi-game support

- [x] T001 Add PatchElementsAppend variant to SseEvent DU in src/TicTacToe.Web/SseBroadcast.fs
- [x] T002 Implement writeSseEvent handler for PatchElementsAppend in src/TicTacToe.Web/SseBroadcast.fs
- [x] T003 Add /games resource route definition in src/TicTacToe.Web/Program.fs
- [x] T004 Add /games/{id} resource route definition in src/TicTacToe.Web/Program.fs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before user stories

**CRITICAL**: No user story work can begin until this phase is complete

- [x] T005 Update MoveSignals type to include gameId field in src/TicTacToe.Web/Handlers.fs
- [x] T006 Create games-container div structure in src/TicTacToe.Web/templates/home.fs
- [x] T007 Update renderGameBoard to accept gameId parameter in src/TicTacToe.Web/templates/game.fs
- [x] T008 Update game board HTML to use id="game-{gameId}" attribute in src/TicTacToe.Web/templates/game.fs
- [x] T009 Update square buttons to include gameId in data-signals in src/TicTacToe.Web/templates/game.fs
- [x] T010 Update square button data-on:click to POST to /games/{gameId} in src/TicTacToe.Web/templates/game.fs
- [x] T011 Remove module-level homeGameId mutable state from src/TicTacToe.Web/Handlers.fs

**Checkpoint**: Foundation ready - user story implementation can begin

---

## Phase 3: User Story 1 - Create and Play a New Game (Priority: P1) MVP

**Goal**: User creates a game via POST /games, game appears on page, can play to completion

**Independent Test**: Click "New Game", game board appears, play X/O to win or draw

### Tests for User Story 1

> **NOTE: Write tests FIRST, ensure they FAIL before implementation**

- [x] T012 [P] [US1] Create RestApiTests.fs test file in test/TicTacToe.Web.Tests/RestApiTests.fs
- [x] T013 [P] [US1] Add test: POST /games returns 201 with Location header in test/TicTacToe.Web.Tests/RestApiTests.fs
- [x] T014 [P] [US1] Add test: POST /games/{id} with valid move returns 202 in test/TicTacToe.Web.Tests/RestApiTests.fs
- [x] T015 [P] [US1] Add Playwright test: New Game button creates visible game board in test/TicTacToe.Web.Tests/GamePlayTests.fs
- [x] T016 [P] [US1] Add Playwright test: Clicking square places X, then O in test/TicTacToe.Web.Tests/GamePlayTests.fs
- [x] T017 [P] [US1] Add Playwright test: Win condition shows winner message in test/TicTacToe.Web.Tests/GamePlayTests.fs

### Implementation for User Story 1

- [x] T018 [US1] Implement createGame handler (POST /games) with 201 + Location in src/TicTacToe.Web/Handlers.fs
- [x] T019 [US1] Broadcast PatchElementsAppend for new game in createGame handler in src/TicTacToe.Web/Handlers.fs
- [x] T020 [US1] Implement makeMove handler (POST /games/{id}) in src/TicTacToe.Web/Handlers.fs
- [x] T021 [US1] Extract gameId from route in makeMove using ctx.GetRouteValue in src/TicTacToe.Web/Handlers.fs
- [x] T022 [US1] Validate game exists, return 404 if not in makeMove handler in src/TicTacToe.Web/Handlers.fs
- [x] T023 [US1] Subscribe to game and broadcast PatchElements on state change in src/TicTacToe.Web/Handlers.fs
- [x] T024 [US1] Update SSE handler to render all existing games on connect in src/TicTacToe.Web/Handlers.fs
- [x] T025 [US1] Add New Game button with data-on:click="@post('/games')" in src/TicTacToe.Web/templates/home.fs
- [x] T026 [US1] Wire up routes to handlers in src/TicTacToe.Web/Program.fs

**Checkpoint**: User Story 1 complete - can create and play single game via REST

---

## Phase 4: User Story 2 - Game Has Unique Resource URL (Priority: P2)

**Goal**: Each game accessible at /games/{id}, direct URL navigation works

**Independent Test**: Create game, navigate to /games/{id} in new tab, see game state

### Tests for User Story 2

- [x] T027 [P] [US2] Add test: GET /games/{id} returns 200 with HTML in test/TicTacToe.Web.Tests/RestApiTests.fs
- [x] T028 [P] [US2] Add test: GET /games/{invalid-id} returns 404 in test/TicTacToe.Web.Tests/RestApiTests.fs
- [x] T029 [P] [US2] Add Playwright test: Direct navigation to /games/{id} shows game in test/TicTacToe.Web.Tests/MultiGameTests.fs

### Implementation for User Story 2

- [x] T030 [US2] Implement getGame handler (GET /games/{id}) in src/TicTacToe.Web/Handlers.fs
- [x] T031 [US2] Return 404 for non-existent game ID in getGame handler in src/TicTacToe.Web/Handlers.fs
- [x] T032 [US2] Render game page layout for direct game URL access in src/TicTacToe.Web/Handlers.fs
- [x] T033 [US2] Wire GET handler to /games/{id} route in src/TicTacToe.Web/Program.fs

**Checkpoint**: User Story 2 complete - games have shareable URLs

---

## Phase 5: User Story 3 - Multiple Games on Single Page (Priority: P3)

**Goal**: Multiple games render and update independently via SSE

**Independent Test**: Create 3 games, make moves in each, verify independent updates

### Tests for User Story 3

- [x] T034 [P] [US3] Create MultiGameTests.fs in test/TicTacToe.Web.Tests/MultiGameTests.fs
- [x] T035 [P] [US3] Add Playwright test: Creating second game shows two boards in test/TicTacToe.Web.Tests/MultiGameTests.fs
- [x] T036 [P] [US3] Add Playwright test: Move in one game doesn't affect others in test/TicTacToe.Web.Tests/MultiGameTests.fs
- [x] T037 [P] [US3] Add Playwright test: 10 concurrent games remain responsive in test/TicTacToe.Web.Tests/MultiGameTests.fs

### Implementation for User Story 3

- [x] T038 [US3] Ensure SSE broadcasts use game-specific element IDs in src/TicTacToe.Web/Handlers.fs
- [x] T039 [US3] Verify PatchElements targets only specific game board in src/TicTacToe.Web/Handlers.fs
- [x] T040 [US3] Test game isolation by running multiple games scenario manually

**Checkpoint**: User Story 3 complete - multiple independent games work

---

## Phase 6: User Story 4 - Delete/Remove a Game (Priority: P4)

**Goal**: Users can delete games, removed from page and server

**Independent Test**: Create game, click delete, verify removed from page and 404 on direct access

### Tests for User Story 4

- [x] T041 [P] [US4] Add test: DELETE /games/{id} returns 204 in test/TicTacToe.Web.Tests/RestApiTests.fs
- [x] T042 [P] [US4] Add test: DELETE /games/{invalid-id} returns 404 in test/TicTacToe.Web.Tests/RestApiTests.fs
- [x] T043 [P] [US4] Add test: GET after DELETE returns 404 in test/TicTacToe.Web.Tests/RestApiTests.fs
- [x] T044 [P] [US4] Add Playwright test: Delete button removes game from page in test/TicTacToe.Web.Tests/MultiGameTests.fs

### Implementation for User Story 4

- [x] T045 [US4] Implement deleteGame handler (DELETE /games/{id}) in src/TicTacToe.Web/Handlers.fs
- [x] T046 [US4] Call supervisor.RemoveGame(gameId) in deleteGame handler in src/TicTacToe.Web/Handlers.fs
- [x] T047 [US4] Broadcast RemoveElement with selector #game-{gameId} in src/TicTacToe.Web/Handlers.fs
- [x] T048 [US4] Return 204 No Content on successful delete in src/TicTacToe.Web/Handlers.fs
- [x] T049 [US4] Return 404 if game doesn't exist in deleteGame handler in src/TicTacToe.Web/Handlers.fs
- [x] T050 [US4] Add Delete Game button to game board template in src/TicTacToe.Web/templates/game.fs
- [x] T051 [US4] Wire DELETE handler to /games/{id} route in src/TicTacToe.Web/Program.fs

**Checkpoint**: User Story 4 complete - games can be deleted

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final cleanup and edge case handling

- [x] T052 [P] Add edge case test: Move on deleted game is ignored in test/TicTacToe.Web.Tests/MultiGameTests.fs
- [x] T053 [P] Add edge case test: Rapid clicking same square in test/TicTacToe.Web.Tests/GamePlayTests.fs
- [ ] T054 [P] Verify SSE reconnection sends current state of all games
- [x] T055 Run full test suite and fix any failures
- [ ] T056 Validate quickstart.md commands work correctly
- [x] T057 Remove deprecated single-game handlers (reset, legacy move) from src/TicTacToe.Web/Handlers.fs

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - start immediately
- **Foundational (Phase 2)**: Depends on Setup - BLOCKS all user stories
- **User Stories (Phase 3-6)**: Depend on Foundational completion
  - US1 must complete first (provides createGame, makeMove infrastructure)
  - US2, US3, US4 can proceed after US1 or in parallel with caution
- **Polish (Phase 7)**: Depends on all user stories complete

### User Story Dependencies

- **User Story 1 (P1)**: Foundation only - MVP
- **User Story 2 (P2)**: Foundation + shares handlers with US1
- **User Story 3 (P3)**: Foundation + US1 (uses multi-game broadcast)
- **User Story 4 (P4)**: Foundation + US1 (extends with DELETE)

### Within Each User Story

- Tests MUST be written and FAIL before implementation (Principle III)
- Implementation tasks proceed sequentially
- Story complete before next priority

### Parallel Opportunities

**Phase 1 Setup**: T001-T004 can run in parallel (different files)

**Phase 2 Foundational**:
- T005-T010 touch different functions, some parallelizable
- T006 (home.fs) and T007-T010 (game.fs) can run in parallel

**Within User Stories**:
- All test tasks marked [P] can run in parallel
- Implementation tasks are mostly sequential per handler

---

## Parallel Example: User Story 1 Tests

```bash
# Launch all US1 tests together:
Task: "Create RestApiTests.fs test file"
Task: "Add test: POST /games returns 201"
Task: "Add test: POST /games/{id} returns 202"
Task: "Add Playwright test: New Game button creates board"
Task: "Add Playwright test: Clicking square places X, O"
Task: "Add Playwright test: Win condition shows message"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T004)
2. Complete Phase 2: Foundational (T005-T011)
3. Complete Phase 3: User Story 1 Tests (T012-T017) - ensure FAIL
4. Complete Phase 3: User Story 1 Implementation (T018-T026)
5. **VALIDATE**: Run tests - all should PASS
6. **DEMO**: Create game, play to completion via REST

### Incremental Delivery

1. Setup + Foundational → Infrastructure ready
2. User Story 1 → MVP: Single game via REST
3. User Story 2 → Games have URLs
4. User Story 3 → Multiple concurrent games
5. User Story 4 → Game deletion
6. Polish → Edge cases and cleanup

---

## Notes

- Engine is PROTECTED - no modifications (constitution)
- All changes in Web layer
- Frank routing uses ASP.NET Core conventions
- Datastar element IDs for SSE targeting
- MailboxProcessor ensures thread-safe game state
- Tests use Playwright + NUnit per constitution
