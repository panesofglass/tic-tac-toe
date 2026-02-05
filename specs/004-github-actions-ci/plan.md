# Implementation Plan: GitHub Actions CI Workflow

**Branch**: `004-github-actions-ci` | **Date**: 2026-02-04 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/004-github-actions-ci/spec.md`

## Summary

Create a GitHub Actions workflow that automatically builds the .NET 10.0 F# solution and runs all tests (Expecto unit tests and Playwright browser tests) on pull requests and pushes to main. Upload Playwright artifacts (screenshots/traces) on test failure.

## Technical Context

**Language/Version**: F# targeting .NET 10.0
**Primary Dependencies**: GitHub Actions, `actions/checkout`, `actions/setup-dotnet`, `actions/upload-artifact`
**Storage**: N/A
**Testing**: Expecto (unit tests via `dotnet test`), Playwright with NUnit (browser tests via `dotnet test`)
**Target Platform**: GitHub-hosted Ubuntu runner (ubuntu-latest)
**Project Type**: .NET solution with web application
**Performance Goals**: Complete workflow in under 10 minutes (per SC-001)
**Constraints**: .NET 10.0 preview required; Playwright browsers must be installed before web tests
**Scale/Scope**: Single workflow file, single job with sequential steps

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Functional-First F# | ✅ PASS | Workflow is YAML configuration, not application code |
| II. Hypermedia Architecture | ✅ N/A | CI workflow does not affect application architecture |
| III. Test-First Development | ✅ PASS | Workflow enforces existing tests run on every PR |
| IV. Simplicity & Focus | ✅ PASS | Single workflow file, minimal configuration |
| Protected Components | ✅ N/A | No changes to TicTacToe.Engine |

**Gate Status**: PASSED - No violations.

### Post-Design Re-Check (Phase 1 Complete)

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Functional-First F# | ✅ PASS | No F# code changes; YAML workflow only |
| II. Hypermedia Architecture | ✅ N/A | CI workflow tests existing architecture |
| III. Test-First Development | ✅ PASS | Workflow enforces TDD discipline on every PR |
| IV. Simplicity & Focus | ✅ PASS | Single workflow file, no unnecessary complexity |
| Protected Components | ✅ N/A | TicTacToe.Engine untouched |

**Post-Design Gate**: PASSED - Design aligns with constitution.

## Project Structure

### Documentation (this feature)

```text
specs/004-github-actions-ci/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (via /speckit.tasks)
```

### Source Code (repository root)

```text
.github/
└── workflows/
    └── ci.yml           # Main CI workflow file (NEW)

src/
├── TicTacToe.Engine/    # Existing - unit tested
└── TicTacToe.Web/       # Existing - Playwright tested

test/
├── TicTacToe.Engine.Tests/  # Expecto unit tests
└── TicTacToe.Web.Tests/     # Playwright browser tests (NUnit)
```

**Structure Decision**: Single workflow file at `.github/workflows/ci.yml`. No additional infrastructure needed.

## Complexity Tracking

No violations requiring justification. Single workflow file follows Principle IV (Simplicity & Focus).
