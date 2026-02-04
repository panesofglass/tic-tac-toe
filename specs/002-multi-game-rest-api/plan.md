# Implementation Plan: Multi-Game REST API

**Branch**: `002-multi-game-rest-api` | **Date**: 2026-02-03 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-multi-game-rest-api/spec.md`

## Summary

Extend the existing single-game tic-tac-toe implementation to support multiple concurrent games with RESTful resource URLs. Games are exposed at `/games/{id}` with proper HTTP semantics (POST creates with 201 + Location header, DELETE removes). All games render on the home page (`/`) and updates push through the existing SSE infrastructure. The existing `GameSupervisor` already supports multi-game management; primary changes are in routing, handlers, and SSE filtering.

## Technical Context

**Language/Version**: F# targeting .NET 10.0
**Primary Dependencies**: Frank 6.4.0, Frank.Datastar 6.4.0, Oxpecker.ViewEngine 1.1.0, System.Reactive 6.0.2
**Storage**: In-memory via MailboxProcessor (existing GameSupervisor pattern)
**Testing**: Expecto for unit tests, Playwright + NUnit for web/integration tests
**Target Platform**: ASP.NET Core web server, modern browsers with SSE support
**Project Type**: Web application (single project - Engine lib + Web app)
**Performance Goals**: Game updates within 1 second, 10+ concurrent games without degradation
**Constraints**: No custom client-side JavaScript beyond Datastar framework
**Scale/Scope**: Local play (X and O same browser), in-memory storage, no persistence

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Functional-First F# | ✅ PASS | Pure functions, immutable data, MailboxProcessor for state |
| II. Hypermedia Architecture | ✅ PASS | Server-side rendering, SSE updates, Datastar attributes, no custom JS |
| III. Test-First Development | ✅ PASS | Expecto for unit tests, Playwright for web tests |
| IV. Simplicity & Focus | ✅ PASS | Direct extension of existing patterns, no new frameworks |
| Protected: TicTacToe.Engine | ✅ PASS | No Engine modifications required; all changes in Web layer |

**Gate Result**: PASSED - No violations. Proceed with implementation.

### Post-Design Re-check (Phase 1 Complete)

| Principle | Status | Design Decision |
|-----------|--------|-----------------|
| I. Functional-First F# | ✅ PASS | Handlers use pure functions, existing MailboxProcessor patterns |
| II. Hypermedia Architecture | ✅ PASS | Datastar SSE events, element ID targeting, no custom JS |
| III. Test-First Development | ✅ PASS | Playwright tests for REST endpoints planned |
| IV. Simplicity & Focus | ✅ PASS | Extends existing patterns, no new abstractions |
| Protected: TicTacToe.Engine | ✅ PASS | Zero Engine changes; all REST logic in Web layer |

**Post-Design Gate Result**: PASSED - Design aligns with constitution.

## Project Structure

### Documentation (this feature)

```text
specs/002-multi-game-rest-api/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (OpenAPI spec)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── TicTacToe.Engine/        # PROTECTED - No changes needed
│   ├── Engine.fs            # GameSupervisor already supports multi-game
│   ├── Engine.fsi
│   ├── Model.fs
│   └── Model.fsi
└── TicTacToe.Web/
    ├── Program.fs           # Add /games resource routes
    ├── Handlers.fs          # New handlers for REST endpoints
    ├── SseBroadcast.fs      # Add game-scoped event filtering
    ├── Auth.fs              # No changes needed
    └── templates/
        ├── layout.fs        # No changes needed
        ├── game.fs          # Update for multi-game rendering
        └── home.fs          # Update for games list view

test/
├── TicTacToe.Engine.Tests/  # No changes needed
└── TicTacToe.Web.Tests/
    ├── GamePlayTests.fs     # Update for multi-game scenarios
    ├── RestApiTests.fs      # NEW: REST endpoint tests
    └── MultiGameTests.fs    # NEW: Multiple concurrent games tests
```

**Structure Decision**: Existing single-project structure maintained. Web layer extended with new routes and handlers. Engine remains unchanged per constitution.

## Complexity Tracking

> No violations requiring justification. Design uses existing patterns.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | N/A | N/A |
