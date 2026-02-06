# Tasks: Player Identity & Legend Display

**Input**: Design documents from `/specs/007-player-identity-legend/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Included per Constitution Principle III (Test-First Development).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/TicTacToe.Web/` (web application)
- **Templates**: `src/TicTacToe.Web/templates/` (server-rendered views)
- **Tests**: `test/TicTacToe.Web.Tests/` (Playwright + NUnit)
- **Engine**: `src/TicTacToe.Engine/` (PROTECTED - no changes)

---

## Phase 1: Setup

**Purpose**: No project initialization needed — existing codebase. This phase is empty.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Convert PlayerAssignmentManager to synchronous API, add shared helper function, and update `subscribeToGame`/`renderGameBoardForBroadcast` signatures that all user stories depend on.

**CRITICAL**: User Story 2 and 3 both depend on the `subscribeToGame` and `renderGameBoardForBroadcast` signature changes. These must be completed first to avoid breaking the build.

- [ ] T001 Add `shortUserId` helper function to truncate GUID to first 8 characters in `src/TicTacToe.Web/templates/game.fs`. This pure function takes a `string option` and returns either the first 8 chars or a placeholder string. Example: `let shortUserId (id: string option) (placeholder: string) = id |> Option.map (fun s -> s.[..7]) |> Option.defaultValue placeholder`
- [ ] T002 Convert all `PlayerAssignmentManager` methods from `PostAndAsyncReply` to `PostAndReply` in `src/TicTacToe.Web/Model.fs`. Change `GetRole`, `TryAssignAndValidate`, and `GetAssignment` to return their values directly (not wrapped in `Async`). Update all call sites in `src/TicTacToe.Web/Handlers.fs` to remove `|> Async.StartAsTask` and change `let!` bindings to `let` bindings where applicable. This eliminates unnecessary async-to-sync overhead for in-memory map lookups behind the mailbox.
- [ ] T003 Update `renderGameBoardForBroadcast` signature in `src/TicTacToe.Web/templates/game.fs` to accept `(assignment: PlayerAssignment option)` as a third parameter. Update the function body to pass the parameter through (legend rendering added in US2). Update the `renderGameBoard` backward-compat wrapper to pass `None` for assignment.
- [ ] T004 Update `subscribeToGame` in `src/TicTacToe.Web/Handlers.fs` to accept `(assignmentManager: PlayerAssignmentManager)` as a third parameter. In the `OnNext` observer callback, call `assignmentManager.GetAssignment(gameId)` (now synchronous after T002) and pass the result to `renderGameBoardForBroadcast`. Update all call sites of `subscribeToGame` in `Handlers.fs` (in `createGame`, `getGame`, `makeMove`, `resetGame`, and `sse`) to pass the `assignmentManager` from DI.

**Checkpoint**: Build compiles with updated signatures. No visual changes yet. All existing tests still pass.

---

## Phase 3: User Story 1 - See My Identity on the Page (Priority: P1) MVP

**Goal**: Display the authenticated user's 8-char GUID prefix in the top-right corner of every page.

**Independent Test**: Log in and verify the user identifier appears in the top-right corner of the page.

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T005 [US1] Write Playwright test in `test/TicTacToe.Web.Tests/PageRenderingTests.fs` (or new `UserIdentityTests.fs`): after login and navigation to home, assert that an element with class `user-identity` is visible in the page header, contains an 8-character string matching the expected GUID prefix format (alphanumeric/hex characters).
- [ ] T006 [US1] Write Playwright test: when visiting the login page directly (unauthenticated), assert that no element with class `user-identity` exists.

### Implementation for User Story 1

- [ ] T007 [US1] Modify `mainLayout` in `src/TicTacToe.Web/templates/shared/layout.fs` to add a `<header class="page-header">` before the `<main>` element. Inside the header, conditionally render `<span class="user-identity">{shortId}</span>` when `ctx.User.TryGetUserId()` returns `Some userId`. Use `userId.[..7]` for the display value. When no user ID is available (unauthenticated), omit the header entirely.
- [ ] T008 [US1] Add CSS styles for `.page-header` and `.user-identity` in `src/TicTacToe.Web/templates/game.fs` (inside `gameStyles`). Header: `display: flex; justify-content: flex-end; padding: 8px 20px;`. User identity: `font-family: monospace; font-size: 0.85em; color: #666;`. Ensure text truncation with `overflow: hidden; text-overflow: ellipsis; max-width: 120px;` for FR-009.

**Checkpoint**: User identity visible in top-right corner on authenticated pages. Tests T005-T006 pass.

---

## Phase 4: User Story 2 - See Player Legend Under Each Game Board (Priority: P1)

**Goal**: Show a legend below each game board displaying "X: {player}" and "O: {player}" with player identifiers or "Waiting for player..." placeholders.

**Independent Test**: Create a game, have two players join, verify the legend shows each player's identifier alongside their symbol.

### Tests for User Story 2

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T009 [US2] Write Playwright test in `test/TicTacToe.Web.Tests/GameBoardTests.fs` (or new `LegendTests.fs`): create a new game, assert that a `div.legend` element exists below the board within the `div.game-board` container, and both legend entries show "Waiting for player...".
- [ ] T010 [US2] Write Playwright test: create a game, Player 1 makes a move (assigns as X), assert legend shows "X: {8-char id}" and "O: Waiting for player...".
- [ ] T011 [US2] Write Playwright test: create a game, Player 1 makes a move, Player 2 (separate browser context via `CreateSecondPlayer`) makes a move, assert legend shows both player identifiers. Verify via SSE that Player 1's page also updates to show both identifiers.

### Implementation for User Story 2

- [ ] T012 [US2] Add `renderLegend` helper function in `src/TicTacToe.Web/templates/game.fs`. Takes `(assignment: PlayerAssignment option)` and `(currentPlayer: Player option)`. Renders `<div class="legend"><span>X: {label}</span><span>O: {label}</span></div>`. Use the `shortUserId` helper from T001 for labels with "Waiting for player..." as the placeholder.
- [ ] T013 [US2] Integrate `renderLegend` into `renderGameBoardWithContext` in `src/TicTacToe.Web/templates/game.fs`. Insert the legend call between the board div and the controls div. The function already receives `assignment: PlayerAssignment option` — pass it along with `currentPlayer` to `renderLegend`.
- [ ] T014 [US2] Integrate `renderLegend` into `renderGameBoardForBroadcast` in `src/TicTacToe.Web/templates/game.fs`. Insert the legend call between the board div and the controls div using the new `assignment` parameter from T003.
- [ ] T015 [US2] Add CSS styles for `.legend` in `src/TicTacToe.Web/templates/game.fs` (inside `gameStyles`). Legend container: `display: flex; justify-content: center; gap: 16px; margin: 8px 0; font-size: 0.9em; color: #555;`. Individual entries: default `font-weight: normal`.

**Checkpoint**: Legend visible under every game board showing player assignments. Tests T009-T011 pass. Legend updates via SSE when players join.

---

## Phase 5: User Story 3 - Bold Active Player's Turn (Priority: P2)

**Goal**: Bold the legend entry of the player whose turn it currently is. No bold when game is over.

**Independent Test**: Start a game, verify X's legend entry is bold. Make a move, verify O's legend entry becomes bold. Complete the game, verify neither is bold.

**Dependency**: Requires US2 (legend must exist to style it).

### Tests for User Story 3

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T016 [US3] Write Playwright test in `test/TicTacToe.Web.Tests/GameBoardTests.fs` (or `LegendTests.fs`): create a game, Player 1 makes a move (it's now O's turn), assert the O legend entry has class `legend-active` (bold) and X legend entry does not.
- [ ] T017 [US3] Write Playwright test: play a game to completion (win or draw), assert neither legend entry has class `legend-active`.

### Implementation for User Story 3

- [ ] T018 [US3] Update `renderLegend` in `src/TicTacToe.Web/templates/game.fs` to use the `currentPlayer: Player option` parameter (already accepted from T012) to conditionally add CSS class `legend-active` to the active player's `<span>`. When `currentPlayer = Some X`, add class to X entry. When `currentPlayer = Some O`, add class to O entry. When `currentPlayer = None` (game over), no class on either.
- [ ] T019 [US3] Add CSS for `.legend-active` in `src/TicTacToe.Web/templates/game.fs` (inside `gameStyles`): `font-weight: bold;`.

**Checkpoint**: Active player's legend entry is bold. Bold switches correctly on each turn. No bold when game ends. Tests T016-T017 pass.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Edge cases and validation across all stories.

- [ ] T020 Verify game reset clears legend: after resetting a game, confirm the new game's legend shows "Waiting for player..." for both slots. Add assertion to existing reset tests in `test/TicTacToe.Web.Tests/ResetGameTests.fs` if not already covered.
- [ ] T021 Verify game delete removes legend: after deleting a game, confirm the entire `game-board` div (including legend) is removed from the DOM. Add assertion to existing delete tests if not already covered.
- [ ] T022 Verify SSE initial-connect includes legend: write a Playwright test (or extend T011) that creates a game, has Player 1 make a move, then opens a new browser tab that connects via SSE. Assert the newly connected client receives game boards with `div.legend` containing the correct player identifier — confirming the `sse` handler's initial game load path renders the legend via `renderGameBoardForBroadcast`.
- [ ] T023 Run full test suite (`dotnet test`) and verify all existing tests still pass with the updated function signatures, sync conversion, and layout changes.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Phase 2)**: No dependencies — can start immediately. BLOCKS all user stories.
- **User Story 1 (Phase 3)**: Depends on Phase 2. Independent of US2 and US3.
- **User Story 2 (Phase 4)**: Depends on Phase 2 (T001, T002, T003, T004). Independent of US1.
- **User Story 3 (Phase 5)**: Depends on Phase 2 AND Phase 4 (US2 must exist to add bold styling).
- **Polish (Phase 6)**: Depends on all user stories being complete.

### User Story Dependencies

- **User Story 1 (P1)**: Independent — only touches `layout.fs` and CSS.
- **User Story 2 (P1)**: Independent of US1 — touches `game.fs` and `Handlers.fs`.
- **User Story 3 (P2)**: Depends on US2 — extends `renderLegend` from US2 with bold logic.
- **US1 and US2 can be implemented in parallel** since they modify different functions/files (US1: `layout.fs`, US2: `game.fs` rendering functions).

### Within Each User Story

- Tests MUST be written and FAIL before implementation (Red-Green-Refactor)
- Implementation tasks are sequenced to build incrementally
- Story complete before moving to next priority

### Parallel Opportunities

- **T005 and T006** (US1 tests) can run in parallel
- **T009, T010, T011** (US2 tests) can run in parallel
- **T016 and T017** (US3 tests) can run in parallel
- **US1 (Phase 3) and US2 (Phase 4)** can be implemented in parallel after Phase 2 completes
- **T007 and T008** (US1 implementation) are sequential (layout first, then CSS)
- **T012, T013, T014** (US2 implementation) are sequential (helper → integrate into both renderers)

---

## Parallel Example: User Stories 1 & 2

```text
# After Phase 2 (Foundational) completes, launch US1 and US2 in parallel:

# Stream 1: User Story 1 (layout.fs)
Task: "T005 [US1] Write identity display test in test/TicTacToe.Web.Tests/"
Task: "T006 [US1] Write unauthenticated identity test"
Task: "T007 [US1] Modify mainLayout in layout.fs"
Task: "T008 [US1] Add header CSS in game.fs"

# Stream 2: User Story 2 (game.fs)
Task: "T009 [US2] Write legend display test"
Task: "T010 [US2] Write single-player legend test"
Task: "T011 [US2] Write two-player legend SSE test"
Task: "T012 [US2] Add renderLegend helper in game.fs"
Task: "T013 [US2] Integrate legend into renderGameBoardWithContext"
Task: "T014 [US2] Integrate legend into renderGameBoardForBroadcast"
Task: "T015 [US2] Add legend CSS in game.fs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 2: Foundational (T001-T004)
2. Complete Phase 3: User Story 1 (T005-T008)
3. **STOP and VALIDATE**: Test User Story 1 independently — user sees their ID in top-right
4. Deploy/demo if ready

### Incremental Delivery

1. Complete Foundational (T001-T004) → Signatures updated, sync conversion done, build green
2. Add User Story 1 (T005-T008) → Identity display working → Deploy/Demo (MVP!)
3. Add User Story 2 (T009-T015) → Legend visible under boards → Deploy/Demo
4. Add User Story 3 (T016-T019) → Bold turn indicator active → Deploy/Demo
5. Polish (T020-T023) → Edge cases verified, full regression green

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable (except US3 depends on US2)
- Tests written first per Constitution Principle III (Red-Green-Refactor)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Total: 23 tasks across 6 phases
