# Tasks: Simplify MailboxProcessor - Remove System.Reactive

**Input**: Design documents from `/specs/003-simplify-mailbox-processor/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md, contracts/

**Tests**: No new tests required. Existing 67 engine tests validate refactoring correctness.

**Implementation Note**: The approach was modified during implementation to keep the familiar `IObservable<MoveResult>` and `IObserver<MoveResult>` interfaces while removing the System.Reactive package dependency. The observer pattern is now implemented directly in the MailboxProcessor.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Validation)

**Purpose**: Verify starting state before making changes

- [x] T001 Run full test suite to establish baseline in repository root: `dotnet test`
- [x] T002 Document current test count for comparison after refactoring (Engine: 67 passed, Web: 39 total)

**Checkpoint**: Baseline established - all tests pass before refactoring begins ✓

---

## Phase 2: Foundational (Type Definitions)

**Purpose**: Define new types that will replace System.Reactive constructs

- [x] T003 [US1] [US2] Add GameActorState record type in src/TicTacToe.Engine/Engine.fs (private, internal state with subscriber map)
- [x] T004 [US1] [US2] Update GameMessage discriminated union in src/TicTacToe.Engine/Engine.fs: add Subscribe, Unsubscribe, GetState cases

**Checkpoint**: Foundation ready ✓

---

## Phase 3: User Story 1 - Maintain Current Game Functionality (Priority: P1) 🎯 MVP

**Goal**: Remove System.Reactive while maintaining IObservable interface and all existing functionality

**Independent Test**: All 67 engine tests pass

### Engine Core Implementation

- [x] T005 [US1] Add GetState method to Game interface in src/TicTacToe.Engine/Engine.fs and Engine.fsi
- [x] T006 [US1] Rewrite GameImpl actor in src/TicTacToe.Engine/Engine.fs: replace BehaviorSubject with subscriber Map in actor state
- [x] T007 [US1] Implement notifyAll helper in src/TicTacToe.Engine/Engine.fs: broadcast state to all observers with try/catch
- [x] T008 [US1] Implement notifyComplete helper in src/TicTacToe.Engine/Engine.fs: call OnCompleted for all observers
- [x] T009 [US1] Implement IObservable<MoveResult> in GameImpl: subscribe via MailboxProcessor, return IDisposable
- [x] T010 [US1] Add BehaviorSubject semantics: emit current state immediately on subscription
- [x] T011 [US1] Remove System.Reactive import from src/TicTacToe.Engine/Engine.fs: delete `open System.Reactive.Subjects`
- [x] T012 [US1] [US2] Remove System.Reactive package reference from src/TicTacToe.Engine/TicTacToe.Engine.fsproj

**Checkpoint**: Code compiles without System.Reactive. Run `dotnet build` to verify. ✓

### Validation

- [x] T013 [US1] Run full test suite: `dotnet test test/TicTacToe.Engine.Tests` - all 67 tests pass ✓
- [ ] T014 [US1] Manual test: create game via web UI, make moves, verify real-time updates
- [ ] T015 [US1] Manual test: create multiple concurrent games, verify independence

**Checkpoint**: User Story 1 complete - all functionality maintained, System.Reactive removed

---

## Phase 4: User Story 2 - Simplified Codebase (Priority: P1)

**Goal**: Verify codebase is free of System.Reactive dependency

**Note**: IObservable and IObserver are kept (standard .NET BCL types) but System.Reactive package is removed

### Verification Tasks

- [x] T016 [US2] Verify no System.Reactive imports: search codebase for `System.Reactive` - zero results ✓
- [x] T017 [US2] Verify no BehaviorSubject usage: search for `BehaviorSubject` in src/ - zero results ✓
- [N/A] T018 [US2] IObservable kept by design - familiar interface retained
- [N/A] T019 [US2] IObserver kept by design - familiar interface retained

**Checkpoint**: User Story 2 complete - System.Reactive dependency removed ✓

---

## Phase 5: User Story 3 - Documented Trade-off Analysis (Priority: P1)

**Goal**: Ensure trade-off documentation is complete and reviewed

**Independent Test**: spec.md contains comprehensive trade-off analysis

### Documentation Verification

- [x] T020 [US3] Verify spec.md trade-off analysis is complete: Trade-off Matrix, What is Lost, What is Gained sections present ✓
- [x] T021 [US3] Confirm spec.md addresses clarification questions from session 2026-02-04 ✓

**Checkpoint**: User Story 3 complete - trade-off analysis documented ✓

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final cleanup and documentation

- [ ] T022 [P] Review Engine.fs for code clarity and F# idioms
- [x] T023 [P] Verify error handling: try/catch around observer callbacks ✓
- [x] T024 Update CLAUDE.md: note System.Reactive removal ✓

---

## Summary

**Completed**: T001-T013, T016-T017, T020-T021, T023-T024
**Remaining**: T014-T015 (manual testing), T022 (code review)
**N/A**: T018-T019 (kept IObservable/IObserver by design)

**Key Achievement**: System.Reactive 6.0.2 removed while maintaining familiar IObservable/IObserver interface. All 67 engine tests pass.
