# Tasks: User-Specific Affordances

**Input**: Design documents from `/specs/008-user-affordances/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Included per Constitution Principle III (Test-First Development).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Verify baseline — all existing tests pass before making changes

- [ ] T001 Run `dotnet build` to verify the project compiles cleanly on the feature branch
- [ ] T002 Run `dotnet test test/TicTacToe.Engine.Tests` to verify engine tests pass (baseline)
- [ ] T003 Run `TEST_BASE_URL=http://localhost:5228 dotnet test test/TicTacToe.Web.Tests` to verify web tests pass (baseline, requires running server)

**Checkpoint**: Baseline green. All existing functionality works before any changes.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Extend the SSE subscriber model with user identity and add role-aware broadcast infrastructure. These changes MUST be complete before any user story can deliver personalized content.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

### Tests for Foundational Phase

- [ ] T004 [P] Write Playwright test in `test/TicTacToe.Web.Tests/AffordanceTests.fs`: verify that when a user connects to `/sse`, their SSE connection is associated with their authenticated userId. Test by connecting two separate browser contexts and verifying each receives content (this test will initially just verify the connection still works after the subscriber model change). Add the file to the `.fsproj` compilation order before `Main.fs`.

### Implementation for Foundational Phase

- [ ] T005 Extend subscriber model in `src/TicTacToe.Web/SseBroadcast.fs`: change `ResizeArray<Channel<SseEvent>>` to `ResizeArray<string * Channel<SseEvent>>` (userId, channel pairs). Update `subscribe` to accept a `userId: string` parameter. Update `unsubscribe` to match by channel reference (filter the tuple). Update `broadcast` to iterate over tuples, sending to the channel element. All existing broadcast behavior must continue to work identically.
- [ ] T006 Add `sendToUser` function in `src/TicTacToe.Web/SseBroadcast.fs`: given a `userId: string` and an `SseEvent`, send the event only to channels belonging to that userId. This enables targeted rejection signals (D4).
- [ ] T007 Add `broadcastPerRole` function in `src/TicTacToe.Web/SseBroadcast.fs`: accepts `(subscribers: (string * Channel<SseEvent>) list) -> (userId: string -> SseEvent) -> unit` pattern — takes a function that maps userId to the appropriate SseEvent, iterates subscribers, calls the mapping function per subscriber, and sends the result to each channel. Group by role to render once per distinct role (per plan D2).
- [ ] T008 Update `sse` handler in `src/TicTacToe.Web/Handlers.fs`: extract userId via `ctx.User.TryGetUserId()` and pass it to the updated `subscribe(userId)` call. If userId is None (unauthenticated edge case), use a sentinel value or skip subscription.
- [ ] T009 Update SSE initial game population loop in `src/TicTacToe.Web/Handlers.fs` (the `sse` handler, lines ~154-163): replace `renderGameBoardForBroadcast` with `renderGameBoardWithContext` using the connecting user's role from `assignmentManager.GetRole(gameId, userId)` and `supervisor.GetActiveGameCount()` for gameCount. Each game board sent during initial load should now be personalized to the connecting user.
- [ ] T010 Verify build compiles and existing tests still pass after foundational changes. The `broadcast` function signature change may require updating call sites in Handlers.fs (createGame, deleteGame, resetGame broadcast calls) to continue working — these still use generic broadcast for non-game-board events (RemoveElement, PatchElementsAppend for new games).

**Checkpoint**: Foundation ready — SSE subscribers carry userId, role-aware broadcast infrastructure exists, SSE initial load is personalized. Generic broadcasts still work for non-personalized events.

---

## Phase 3: User Story 1 — Active Player Sees Only Their Actionable Controls (Priority: P1) 🎯 MVP

**Goal**: When it's a player's turn, they see clickable move squares + reset/delete buttons. When it's not their turn, they see reset/delete but no move squares. This is the core affordance personalization.

**Independent Test**: Log in as assigned Player X, verify clickable squares appear on X's turn, verify they disappear on O's turn.

### Tests for User Story 1

- [ ] T011 [P] [US1] Write Playwright test in `test/TicTacToe.Web.Tests/AffordanceTests.fs`: Player X on X's turn sees clickable squares (elements with class `square-clickable` or `data-on:click` attribute) on their board. Player X makes a move, then verify that Player X no longer sees clickable squares (it's now O's turn). Use `CreateSecondPlayer` to set up the two-player scenario.
- [ ] T012 [P] [US1] Write Playwright test in `test/TicTacToe.Web.Tests/AffordanceTests.fs`: Player X on X's turn sees enabled reset and delete buttons on their board. After making a move (now O's turn), Player X still sees enabled reset and delete buttons.

### Implementation for User Story 1

- [ ] T013 [US1] Update `subscribeToGame` OnNext handler in `src/TicTacToe.Web/Handlers.fs` (lines ~48-55): replace the single `renderGameBoardForBroadcast` + `broadcast(PatchElements html)` call with `broadcastPerRole` that renders per-subscriber using `renderGameBoardWithContext`. For each subscriber, look up their role via `assignmentManager.GetRole(gameId, userId)`, get `gameCount` from `supervisor.GetActiveGameCount()`, and render with `renderGameBoardWithContext gameId result role assignment gameCount`. The existing `renderGameBoardWithContext` already handles: clickable squares only when it's the user's turn (PlayerX on X's turn, PlayerO on O's turn), and reset/delete enabled for PlayerX/PlayerO.
- [ ] T014 [US1] Update `canReset` and `canDelete` functions in `src/TicTacToe.Web/templates/game.fs` to implement the clarified reset/delete visibility rules: assigned players (PlayerX, PlayerO) always see reset/delete when hasActivity is true; all authenticated users see reset/delete when gameCount > 6 regardless of role. Currently `canReset` and `canDelete` only enable for PlayerX/PlayerO — extend to also enable for any role when gameCount > 6.
- [ ] T015 [US1] Run US1 tests to verify they pass. Active player on their turn sees clickable squares + management buttons; after move, no clickable squares but management buttons persist.

**Checkpoint**: Active players see role-appropriate controls. The core hypermedia affordance personalization works for assigned players.

---

## Phase 4: User Story 4 — Multi-Board Personalized View (Priority: P1)

**Goal**: A user assigned to multiple boards sees each board rendered independently per their role on that specific board. Board 1 may show full controls while Board 2 shows only management buttons.

**Independent Test**: Assign user as X on Board 1 and O on Board 2, set X's turn on both, verify Board 1 has move controls and Board 2 does not.

### Tests for User Story 4

- [ ] T016 [P] [US4] Write Playwright test in `test/TicTacToe.Web.Tests/AffordanceTests.fs`: create multiple games, assign user as X on game 1 (by making first move) and O on game 2 (by having another player move first, then user moves). Set X's turn on both. Verify game 1 board shows clickable squares and game 2 board does not show clickable squares. Both boards should show reset/delete buttons.

### Implementation for User Story 4

- [ ] T017 [US4] Verify that the SSE initial load personalization from T009 correctly renders each board independently. The `sse` handler loops through all games and calls `renderGameBoardWithContext` per game with the user's role for that specific game. No additional implementation should be needed if T009 and T013 are correct — this task validates per-board independence by running the test from T016.
- [ ] T018 [US4] Run US4 tests to verify multi-board personalized view works end-to-end.

**Checkpoint**: Multi-board personalization verified. Each board renders independently per the user's role on that board.

---

## Phase 5: User Story 2 — Assigned Player on Opponent's Turn Sees Limited Controls (Priority: P2)

**Goal**: When it's NOT a player's turn, they see no clickable squares but still see reset/delete buttons. This should already work from US1 implementation — this phase validates and covers edge cases.

**Independent Test**: Log in as Player X, have Player O's turn be active, verify no clickable squares but reset/delete buttons present.

### Tests for User Story 2

- [ ] T019 [P] [US2] Write Playwright test in `test/TicTacToe.Web.Tests/AffordanceTests.fs`: Player X is assigned and it is O's turn. Verify Player X's board shows no elements with `square-clickable` class or `data-on:click` attribute on squares. Verify reset and delete buttons are present and enabled.
- [ ] T020 [P] [US2] Write Playwright test in `test/TicTacToe.Web.Tests/AffordanceTests.fs`: Player O is assigned and it is X's turn. Verify same behavior — no clickable squares, reset/delete buttons present.

### Implementation for User Story 2

- [ ] T021 [US2] Run US2 tests. If they pass without additional changes (expected, since `renderGameBoardWithContext` already handles this via the `currentPlayer` logic in `renderSquare`), mark complete. If any test fails, investigate and fix the rendering logic in `src/TicTacToe.Web/templates/game.fs`.

**Checkpoint**: Waiting player controls verified. Assigned players on opponent's turn see management buttons only.

---

## Phase 6: User Story 5 — Real-Time Updates Maintain Personalized Views (Priority: P2)

**Goal**: When game state changes (move, reset), each connected user receives a board update rendered for their specific role — not a generic broadcast.

**Independent Test**: Two players + observer connected. Player X makes a move. Verify Player X sees no move squares (O's turn now), Player O sees move squares (their turn), observer sees read-only.

### Tests for User Story 5

- [ ] T022 [P] [US5] Write Playwright test in `test/TicTacToe.Web.Tests/AffordanceTests.fs`: Player X and Player O both connected via SSE. Player X makes a move. Verify Player O's board updates to show clickable squares (it's now O's turn). Verify Player X's board updates to show no clickable squares. Use `CreateSecondPlayer` for the two-player setup.
- [ ] T023 [P] [US5] Write Playwright test in `test/TicTacToe.Web.Tests/AffordanceTests.fs`: Player resets a game. Verify both players receive updated board rendered per their role in the new (reset) game. New game should have no assignments, so both see unassigned view.

### Implementation for User Story 5

- [ ] T024 [US5] Update `createGame` handler broadcast in `src/TicTacToe.Web/Handlers.fs` (lines ~193-202): replace `renderGameBoardForBroadcast` + `broadcast(PatchElementsAppend(...))` with per-role rendering. Since new games have no assigned players, all users get the same unassigned/non-player view, but render via `renderGameBoardWithContext` with each subscriber's role (UnassignedX for all, since no one is assigned yet) and current gameCount.
- [ ] T025 [US5] Update `resetGame` handler broadcast in `src/TicTacToe.Web/Handlers.fs` (lines ~364-389): replace `renderGameBoardForBroadcast` + `broadcast(PatchElementsAppend(...))` for the new game with per-role rendering. The removal broadcast (`RemoveElement`) is unchanged (same for all users). The new game addition uses per-role rendering with current gameCount.
- [ ] T026 [US5] Update `makeMove` rejection signal in `src/TicTacToe.Web/Handlers.fs` (line ~293): replace `broadcast(PatchSignals ...)` with `sendToUser userId (PatchSignals ...)` to send rejection animation only to the user who triggered the rejected action, not all subscribers.
- [ ] T027 [US5] Run US5 tests to verify real-time updates deliver personalized views.

**Checkpoint**: Real-time updates are personalized. Each connected user receives role-appropriate board updates on every state change.

---

## Phase 7: User Story 3 — Observer Sees Read-Only Board (Priority: P3)

**Goal**: Observers (spectators, unassigned users on wrong turn) see no move controls. They see reset/delete buttons only when board count > 6. Unassigned users on their claimable turn can still make moves.

**Independent Test**: Log in as third user (not assigned to any game). Verify no clickable squares, no reset/delete buttons when ≤ 6 boards. Create 7th board, verify reset/delete appear.

### Tests for User Story 3

- [ ] T028 [P] [US3] Write Playwright test in `test/TicTacToe.Web.Tests/AffordanceTests.fs`: third user (observer) views a game where both X and O are assigned to other users. With 6 boards, verify observer sees no clickable squares, no reset button, no delete button on that game's board.
- [ ] T029 [P] [US3] Write Playwright test in `test/TicTacToe.Web.Tests/AffordanceTests.fs`: same setup as T028 but create a 7th board (> 6 threshold). Verify observer now sees reset and delete buttons on all boards, but still no clickable squares.
- [ ] T030 [P] [US3] Write Playwright test in `test/TicTacToe.Web.Tests/AffordanceTests.fs`: unassigned user views a game where X slot is open and it's X's turn. Verify the user sees clickable squares (can claim X by making a move). Verify no reset/delete buttons (not assigned yet, board count ≤ 6).

### Implementation for User Story 3

- [ ] T031 [US3] Verify that `renderGameBoardWithContext` in `src/TicTacToe.Web/templates/game.fs` correctly handles observer rendering: for Spectator role, no `currentPlayer` is set (so no clickable squares); `canReset`/`canDelete` return based on role + gameCount per the updated rules from T014. If the existing logic doesn't cover Spectator correctly, update the rendering to handle it.
- [ ] T032 [US3] Verify unassigned user affordances: `renderGameBoardWithContext` for UnassignedX on X's turn should show clickable squares (move squares enabled because currentPlayer is set). For UnassignedX on O's turn, no clickable squares. Reset/delete follow the gameCount > 6 rule for unassigned roles. Adjust rendering in `src/TicTacToe.Web/templates/game.fs` if needed.
- [ ] T033 [US3] Run US3 tests to verify observer and unassigned user views.

**Checkpoint**: Observer and unassigned views work correctly. The full affordance matrix from data-model.md is implemented.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Clean up, edge cases, and retire deprecated code

- [ ] T034 Remove `renderGameBoardForBroadcast` function from `src/TicTacToe.Web/templates/game.fs` if all call sites have been migrated to `renderGameBoardWithContext`. Also remove or update the backward-compatible `renderGameBoard` wrapper (lines ~208-209) if it is no longer needed.
- [ ] T035 Update `getGame` handler in `src/TicTacToe.Web/Handlers.fs` (lines ~210-240) to use role-aware rendering: extract userId from context, look up role, render with `renderGameBoardWithContext` using actual role and gameCount instead of the default `UnassignedX` and gameCount 6.
- [ ] T036 Verify edge case: game completion (win/draw). All users should see no move controls; assigned players see reset/delete; observers see reset/delete only when board count > 6. Add a Playwright test in `test/TicTacToe.Web.Tests/AffordanceTests.fs` if not already covered.
- [ ] T037 Verify edge case: auto-assignment role transition. When an unassigned user makes a first move and gets assigned, the broadcast should update their view (and all other users' views) to reflect the new role assignment. This should work via the existing `subscribeToGame` OnNext flow.
- [ ] T038 Run full test suite (`dotnet test test/TicTacToe.Engine.Tests` and `TEST_BASE_URL=http://localhost:5228 dotnet test test/TicTacToe.Web.Tests`) to verify no regressions.
- [ ] T039 Verify server-side authorization remains intact: attempt unauthorized actions (observer making a move, non-player resetting) and confirm server rejects with appropriate HTTP status codes. Existing `RestApiTests` and `MultiPlayerTests` should cover this.

**Checkpoint**: All edge cases verified, deprecated code removed, full test suite green.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Foundational — core affordance personalization
- **US4 (Phase 4)**: Depends on US1 — validates multi-board independence
- **US2 (Phase 5)**: Depends on Foundational — can run in parallel with US1 (different test scenarios)
- **US5 (Phase 6)**: Depends on US1 — requires broadcast changes from US1
- **US3 (Phase 7)**: Depends on US1 (for canReset/canDelete rule changes in T014)
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (P1)**: Depends on Foundational only. MVP delivery point.
- **US4 (P1)**: Depends on US1 (shares rendering infrastructure). Validates multi-board.
- **US2 (P2)**: Depends on Foundational. Can start in parallel with US1 (tests only), but implementation validation depends on US1 rendering changes.
- **US5 (P2)**: Depends on US1 (needs broadcastPerRole wired in subscribeToGame). Covers create/reset/rejection handlers.
- **US3 (P3)**: Depends on US1 (canReset/canDelete rule changes). Covers observer + threshold.

### Within Each User Story

- Tests MUST be written and FAIL before implementation (Constitution Principle III)
- Implementation tasks are sequential within each story
- Story complete before moving to next priority

### Parallel Opportunities

- T004 can run in parallel with other foundational tests
- T011, T012 (US1 tests) can run in parallel
- T016 (US4 test) can run in parallel with US1 tests
- T019, T020 (US2 tests) can run in parallel
- T022, T023 (US5 tests) can run in parallel
- T028, T029, T030 (US3 tests) can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch US1 tests in parallel (both write to AffordanceTests.fs, so coordinate):
Task T011: "Playwright test: active player sees clickable squares on their turn"
Task T012: "Playwright test: active player sees reset/delete buttons"

# Sequential implementation:
Task T013: "Update subscribeToGame to use broadcastPerRole"
Task T014: "Update canReset/canDelete for clarified rules"
Task T015: "Run US1 tests to verify"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (verify baseline)
2. Complete Phase 2: Foundational (subscriber identity + broadcast infrastructure)
3. Complete Phase 3: User Story 1 (active player affordances)
4. **STOP and VALIDATE**: Active player sees correct controls on their turn
5. This alone delivers the core hypermedia affordance principle

### Incremental Delivery

1. Setup + Foundational → Infrastructure ready
2. US1 → Active player controls → **MVP!**
3. US4 → Multi-board verification → Validates independence
4. US2 → Waiting player controls → Validates opponent-turn rendering
5. US5 → Real-time personalized updates → Full broadcast personalization
6. US3 → Observer read-only + threshold → Complete affordance matrix
7. Polish → Clean up, edge cases, deprecated code removal

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Constitution Principle III requires test-first: write failing tests before implementation
- Engine (TicTacToe.Engine) is PROTECTED — zero changes allowed
- All changes are in TicTacToe.Web (3 source files + 1 new test file)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
