# Tasks: GitHub Actions CI Workflow

**Input**: Design documents from `/specs/004-github-actions-ci/`
**Prerequisites**: plan.md (required), spec.md (required), research.md

**Tests**: No test tasks included - workflow verification is done by pushing to GitHub and observing the workflow execution.

**Organization**: Tasks are grouped by user story to enable incremental workflow development.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Workflow file**: `.github/workflows/ci.yml`
- All tasks operate on a single YAML file with distinct sections

---

## Phase 1: Setup

**Purpose**: Create the workflow file structure and GitHub Actions directory

- [x] T001 Create `.github/workflows/` directory structure
- [x] T002 Create workflow file skeleton with name and trigger placeholders in `.github/workflows/ci.yml`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core workflow structure that MUST be complete before user story-specific steps

**⚠️ CRITICAL**: No user story steps can be added until this phase is complete

- [x] T003 Add PR trigger configuration (on.pull_request.branches: main) in `.github/workflows/ci.yml`
- [x] T004 Add push trigger configuration (on.push.branches: main) in `.github/workflows/ci.yml`
- [x] T005 Add job definition with ubuntu-latest runner in `.github/workflows/ci.yml`
- [x] T006 Add checkout step using actions/checkout@v4 in `.github/workflows/ci.yml`
- [x] T007 Add .NET 10.0 preview setup using actions/setup-dotnet@v5 with dotnet-quality: preview in `.github/workflows/ci.yml`
- [x] T008 Add dotnet restore step in `.github/workflows/ci.yml`

**Checkpoint**: Foundation ready - workflow can checkout code and setup .NET

---

## Phase 3: User Story 1 - Automatic Build Verification (Priority: P1) 🎯 MVP

**Goal**: CI workflow builds the application and runs unit tests on PRs

**Independent Test**: Push a PR to GitHub and verify the workflow triggers, builds succeed, and unit tests run

### Implementation for User Story 1

- [x] T009 [US1] Add dotnet build step (--no-restore flag) in `.github/workflows/ci.yml`
- [x] T010 [US1] Add unit test step for TicTacToe.Engine.Tests (dotnet test --no-build) in `.github/workflows/ci.yml`

**Checkpoint**: At this point, PRs trigger build + unit tests. US1 is independently verifiable.

---

## Phase 4: User Story 2 - Playwright Browser Tests (Priority: P1)

**Goal**: CI workflow runs Playwright browser tests with proper browser installation and web app startup

**Independent Test**: Push a PR and verify Playwright tests execute successfully with browser properly installed

### Implementation for User Story 2

- [x] T011 [US2] Add Playwright browser installation step (pwsh playwright.ps1 install chromium --with-deps) in `.github/workflows/ci.yml`
- [x] T012 [US2] Add web application startup step with background process and health check in `.github/workflows/ci.yml`
- [x] T013 [US2] Add Playwright test step for TicTacToe.Web.Tests with TEST_BASE_URL env var in `.github/workflows/ci.yml`
- [x] T014 [US2] Add artifact upload step for Playwright traces on failure (actions/upload-artifact@v4 with if: failure()) in `.github/workflows/ci.yml`

**Checkpoint**: At this point, Playwright tests run in CI with browser dependencies. US2 is independently verifiable.

---

## Phase 5: User Story 3 - Main Branch Protection (Priority: P2)

**Goal**: Pushes to main trigger CI, and PR status checks are visible

**Independent Test**: Push directly to main and verify workflow triggers; check PR status display in GitHub UI

### Implementation for User Story 3

- [x] T015 [US3] Verify push trigger covers main branch in `.github/workflows/ci.yml` (already done in T004, verify configuration)
- [x] T016 [US3] Verify workflow reports status back to PRs (native GitHub Actions behavior, verify no continue-on-error: true that would mask failures)

**Checkpoint**: All user stories complete. Full CI workflow operational.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and documentation

- [x] T017 Add workflow timeout-minutes limit (10 min per SC-001) in `.github/workflows/ci.yml`
- [x] T018 Review workflow for clear step names that aid debugging (per SC-003)
- [ ] T019 Run quickstart.md validation - push branch and verify workflow execution on PR

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational
- **User Story 2 (Phase 4)**: Depends on Foundational (can run parallel to US1 but builds on same file)
- **User Story 3 (Phase 5)**: Depends on Foundational (verification of existing config)
- **Polish (Phase 6)**: Depends on all user stories complete

### User Story Dependencies

- **User Story 1 (P1)**: Build + unit tests - can complete independently after Foundational
- **User Story 2 (P1)**: Playwright tests - depends on build step existing (US1), but independently testable
- **User Story 3 (P2)**: Status reporting - verifies configuration from Foundational phase

### Within Each Phase

Since all tasks operate on a single file (`.github/workflows/ci.yml`), tasks within each phase should be executed **sequentially** to avoid merge conflicts. However, the phases themselves can be completed as atomic units.

### Parallel Opportunities

- **Limited**: All tasks modify the same YAML file, so parallelization is minimal
- **Strategy**: Complete each phase as an atomic commit, then proceed to the next phase
- **Alternative**: If multiple developers, one can work on workflow while another prepares test infrastructure improvements (future tracing enhancement)

---

## Parallel Example: Full Workflow Build

Since tasks are sequential within the single file, the parallel opportunity is at the **verification** level:

```bash
# After completing Phase 4, verify both stories simultaneously:
# Terminal 1: Create PR with valid code - verify US1 (build passes)
# Terminal 2: Create PR with Playwright test - verify US2 (browser tests run)
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2)

1. Complete Phase 1: Setup (T001-T002)
2. Complete Phase 2: Foundational (T003-T008)
3. Complete Phase 3: User Story 1 (T009-T010)
4. **STOP and VALIDATE**: Push branch, create PR, verify build + unit tests run
5. Complete Phase 4: User Story 2 (T011-T014)
6. **STOP and VALIDATE**: Verify Playwright tests run successfully

### Incremental Delivery

1. Phases 1-2: Foundation → Can verify checkout + .NET setup works
2. Phase 3: US1 → Can verify build + unit tests (MVP!)
3. Phase 4: US2 → Can verify Playwright tests
4. Phase 5: US3 → Verify status reporting (inherent to GitHub Actions)
5. Phase 6: Polish → Timeout limits and documentation

### Single Developer Strategy

Since all tasks modify one file:
1. Complete all tasks in sequence
2. Commit after each phase as a logical unit
3. Push and verify after Phase 4 (MVP complete)
4. Final commit after Phase 6

---

## Notes

- All tasks operate on `.github/workflows/ci.yml` - sequential execution required
- [Story] labels map tasks to spec.md user stories for traceability
- US1 and US2 are both P1 priority but have implicit dependency (build before Playwright)
- US3 is mostly verification of existing GitHub Actions behavior
- No F# code changes required - this is pure YAML configuration
- Verification requires pushing to GitHub (cannot test locally)
