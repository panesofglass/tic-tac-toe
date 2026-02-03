# Tasks: Web Frontend Single Game

**Input**: Design documents from `/specs/001-web-frontend-single-game/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests included per constitution (Test-First Development principle). Playwright with NUnit for web tests.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/TicTacToe.Web/` (existing web project)
- **Tests**: `test/TicTacToe.Web.Tests/` (existing test project)
- **Engine**: `src/TicTacToe.Engine/` (PROTECTED - DO NOT MODIFY)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify existing project structure and establish SSE broadcast infrastructure

- [ ] T001 Verify existing project builds and runs with `dotnet build` and `dotnet run --project src/TicTacToe.Web`
- [ ] T002 Add SseEvent type and SseBroadcast module in `src/TicTacToe.Web/SseBroadcast.fs`
- [ ] T003 Update `src/TicTacToe.Web/TicTacToe.Web.fsproj` to include SseBroadcast.fs in compile order

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core SSE infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T004 Implement SSE subscriber management (subscribe/unsubscribe/broadcast) in `src/TicTacToe.Web/SseBroadcast.fs`
- [ ] T005 Implement SSE endpoint handler (GET /sse) in `src/TicTacToe.Web/Handlers.fs`
- [ ] T006 Add `/sse` route to `src/TicTacToe.Web/Program.fs`
- [ ] T007 Create home page game state holder (single game reference) in `src/TicTacToe.Web/GameState.fs`
- [ ] T008 Update `src/TicTacToe.Web/TicTacToe.Web.fsproj` to include GameState.fs in compile order

**Checkpoint**: SSE endpoint responds, subscribers can connect/disconnect

---

## Phase 3: User Story 1 - Play a Complete Game (Priority: P1) 🎯 MVP

**Goal**: Display game board on home page, accept moves, show game progression to win/draw

**Independent Test**: Open home page, click squares to play X and O alternately, observe win/draw message

### Tests for User Story 1

- [ ] T009 [P] [US1] Playwright test: home page loads with empty 3x3 grid in `test/TicTacToe.Web.Tests/HomePageTests.fs`
- [ ] T010 [P] [US1] Playwright test: clicking square places X mark in `test/TicTacToe.Web.Tests/GamePlayTests.fs`
- [ ] T011 [P] [US1] Playwright test: turns alternate X → O → X in `test/TicTacToe.Web.Tests/GamePlayTests.fs`
- [ ] T012 [P] [US1] Playwright test: winning game shows winner message in `test/TicTacToe.Web.Tests/GamePlayTests.fs`
- [ ] T013 [P] [US1] Playwright test: draw game shows draw message in `test/TicTacToe.Web.Tests/GamePlayTests.fs`

### Implementation for User Story 1

- [ ] T014 [US1] Update home page template to embed game board container in `src/TicTacToe.Web/templates/home.fs`
- [ ] T015 [US1] Add SSE connection attribute to game board container in `src/TicTacToe.Web/templates/home.fs`
- [ ] T016 [US1] Update game board renderer to use Datastar click handlers in `src/TicTacToe.Web/templates/game.fs`
- [ ] T017 [US1] Implement POST /move handler (fire-and-forget with broadcast) in `src/TicTacToe.Web/Handlers.fs`
- [ ] T018 [US1] Add `/move` route to `src/TicTacToe.Web/Program.fs`
- [ ] T019 [US1] Update SSE handler to send initial game state on connect in `src/TicTacToe.Web/Handlers.fs`
- [ ] T020 [US1] Wire game state changes to SSE broadcast in `src/TicTacToe.Web/Handlers.fs`
- [ ] T021 [US1] Update home handler to initialize game on first load in `src/TicTacToe.Web/Handlers.fs`
- [ ] T022 [US1] Add game status display (turn indicator, win/draw message) in `src/TicTacToe.Web/templates/game.fs`

**Checkpoint**: Full game playable from home page with SSE updates

---

## Phase 4: User Story 2 - Real-Time Game Updates (Priority: P2)

**Goal**: Board updates instantly without page refresh, smooth responsive UX

**Independent Test**: Make moves and verify board updates within 1 second without visible page reload

### Tests for User Story 2

- [ ] T023 [P] [US2] Playwright test: board updates without page reload in `test/TicTacToe.Web.Tests/RealTimeTests.fs`
- [ ] T024 [P] [US2] Playwright test: status updates immediately on game end in `test/TicTacToe.Web.Tests/RealTimeTests.fs`

### Implementation for User Story 2

- [ ] T025 [US2] Add loading indicator during move submission in `src/TicTacToe.Web/templates/game.fs`
- [ ] T026 [US2] Add disabled state to squares during request in `src/TicTacToe.Web/templates/game.fs`
- [ ] T027 [US2] Verify SSE patchElements targets correct selector (#game-board) in `src/TicTacToe.Web/Handlers.fs`

**Checkpoint**: Moves feel instant, no page flicker

---

## Phase 5: User Story 3 - Start a New Game (Priority: P3)

**Goal**: "New Game" button appears after game ends, resets board to start fresh

**Independent Test**: Complete a game, click "New Game", verify empty board with X's turn

### Tests for User Story 3

- [ ] T028 [P] [US3] Playwright test: "New Game" button appears after win in `test/TicTacToe.Web.Tests/NewGameTests.fs`
- [ ] T029 [P] [US3] Playwright test: "New Game" button appears after draw in `test/TicTacToe.Web.Tests/NewGameTests.fs`
- [ ] T030 [P] [US3] Playwright test: clicking "New Game" resets board in `test/TicTacToe.Web.Tests/NewGameTests.fs`
- [ ] T031 [P] [US3] Playwright test: "New Game" button hidden during game in `test/TicTacToe.Web.Tests/NewGameTests.fs`

### Implementation for User Story 3

- [ ] T032 [US3] Add "New Game" button to game board template (conditional on game ended) in `src/TicTacToe.Web/templates/game.fs`
- [ ] T033 [US3] Implement POST /reset handler to create new game and broadcast in `src/TicTacToe.Web/Handlers.fs`
- [ ] T034 [US3] Add `/reset` route to `src/TicTacToe.Web/Program.fs`

**Checkpoint**: Users can replay games seamlessly

---

## Phase 6: User Story 4 - User Identity Persistence (Priority: P4)

**Goal**: Cookie-based user identity persists across sessions

**Independent Test**: Visit site, check cookie exists, close browser, return, verify same identity

### Tests for User Story 4

- [ ] T035 [P] [US4] Playwright test: first visit creates identity cookie in `test/TicTacToe.Web.Tests/IdentityTests.fs`
- [ ] T036 [P] [US4] Playwright test: returning visit preserves identity in `test/TicTacToe.Web.Tests/IdentityTests.fs`

### Implementation for User Story 4

- [ ] T037 [US4] Verify Auth.fs ClaimsTransformation is active in `src/TicTacToe.Web/Program.fs`
- [ ] T038 [US4] Verify cookie middleware order (auth before antiforgery) in `src/TicTacToe.Web/Program.fs`

**Checkpoint**: User identity works as designed (existing implementation)

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T039 [P] Add CSS styles for game board in `src/TicTacToe.Web/wwwroot/css/game.css` or inline in layout
- [ ] T040 [P] Add edge case handling: ignore clicks on occupied squares in `src/TicTacToe.Web/Handlers.fs`
- [ ] T041 [P] Add edge case handling: ignore moves after game ended in `src/TicTacToe.Web/Handlers.fs`
- [ ] T042 Verify SSE reconnection behavior on connection loss
- [ ] T043 Run quickstart.md validation (manual walkthrough)
- [ ] T044 Clean up unused handlers and routes from old implementation in `src/TicTacToe.Web/`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational phase completion
  - US1 (P1): Foundation complete → Can start
  - US2 (P2): Builds on US1 (uses same SSE infrastructure)
  - US3 (P3): Builds on US1 (needs game end state)
  - US4 (P4): Independent (uses existing Auth.fs)
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after US1 complete - Enhances US1 UX
- **User Story 3 (P3)**: Can start after US1 complete - Requires game end detection from US1
- **User Story 4 (P4)**: Can start after Foundational (Phase 2) - Independent of other stories

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Templates before handlers
- Handlers before routes
- Core implementation before polish

### Parallel Opportunities

- All test tasks within a phase marked [P] can run in parallel
- T039, T040, T041 in Polish phase can run in parallel
- US4 can run in parallel with US2 and US3 (after US1 complete)

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Task: "Playwright test: home page loads with empty 3x3 grid in test/TicTacToe.Web.Tests/HomePageTests.fs"
Task: "Playwright test: clicking square places X mark in test/TicTacToe.Web.Tests/GamePlayTests.fs"
Task: "Playwright test: turns alternate X → O → X in test/TicTacToe.Web.Tests/GamePlayTests.fs"
Task: "Playwright test: winning game shows winner message in test/TicTacToe.Web.Tests/GamePlayTests.fs"
Task: "Playwright test: draw game shows draw message in test/TicTacToe.Web.Tests/GamePlayTests.fs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready - users can play complete games!

### Incremental Delivery

1. Complete Setup + Foundational → SSE infrastructure ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Smooth UX → Deploy/Demo
4. Add User Story 3 → Replay capability → Deploy/Demo
5. Add User Story 4 → Identity ready for future features → Deploy/Demo

### Single Developer Strategy

1. Complete phases sequentially: Setup → Foundational → US1 → US2 → US3 → US4 → Polish
2. Stop after each user story to validate
3. Each story builds on previous work

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing (TDD per constitution)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Engine is PROTECTED - all changes in Web layer only
