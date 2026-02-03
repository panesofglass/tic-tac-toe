# Implementation Plan: Web Frontend Single Game

**Branch**: `001-web-frontend-single-game` | **Date**: 2026-02-02 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-web-frontend-single-game/spec.md`

## Summary

Build a working tic-tac-toe web frontend displaying a single game board directly on the home page. Users play locally (X and O from the same browser) with real-time SSE updates. The architecture uses a single SSE endpoint designed for future scaling to 1000+ boards. Retain existing cookie-based user identification from Auth.fs.

## Technical Context

**Language/Version**: F# targeting .NET 10.0 (per existing project)
**Primary Dependencies**: Frank 6.4.0, Frank.Datastar 6.4.0, Oxpecker.ViewEngine 1.1.0
**Storage**: In-memory via MailboxProcessor (GameSupervisor pattern already exists)
**Testing**: Expecto for unit tests, Playwright with NUnit for web tests (per constitution)
**Target Platform**: Web browser (modern browsers with SSE support)
**Project Type**: Web application (server-rendered hypermedia)
**Performance Goals**: <1s board updates, <2s page load
**Constraints**: Single SSE endpoint for all board updates; no custom client-side JavaScript
**Scale/Scope**: Single game initially; architecture must support 1000+ games on one page

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Functional-First F# | ✅ PASS | Using F#, pure functions in Engine, MailboxProcessor for state |
| II. Hypermedia Architecture | ✅ PASS | Datastar SSE, server-side rendering, no custom JS |
| III. Test-First Development | ✅ PASS | Expecto + Playwright per constitution |
| IV. Simplicity & Focus | ✅ PASS | Single game scope, direct implementation |
| Protected: Engine | ✅ PASS | Engine unchanged; all changes in Web layer |

**Gate Result**: PASS - No violations. Proceed with implementation.

## Project Structure

### Documentation (this feature)

```text
specs/001-web-frontend-single-game/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── TicTacToe.Engine/           # PROTECTED - DO NOT MODIFY
│   ├── Model.fs                # Game types (MoveResult, Player, etc.)
│   └── Engine.fs               # Game/GameSupervisor implementations
│
└── TicTacToe.Web/              # Primary implementation target
    ├── Auth.fs                 # Cookie-based user identification (retain)
    ├── Extensions.fs           # WebHostBuilder extensions
    ├── DatastarHelpers.fs      # Datastar integration helpers
    ├── DatastarAttrs.fs        # Datastar attribute helpers
    ├── Models.fs               # Request/signal models
    ├── Handlers.fs             # HTTP handlers (refactor for single SSE)
    ├── Program.fs              # App configuration and routing
    └── templates/
        ├── shared/layout.fs    # HTML layout wrapper
        ├── home.fs             # Home page (game board here)
        └── game.fs             # Game board rendering (reuse)

test/
├── TicTacToe.Engine.Tests/     # Existing - no changes needed
└── TicTacToe.Web.Tests/        # Add Playwright tests
    ├── GameBoardTests.fs       # Board rendering tests
    ├── MoveInteractionTests.fs # Move/click tests
    └── ...
```

**Structure Decision**: Existing F# web project structure. The Web project already has templates and handlers; refactor to embed game board on home page with single SSE endpoint.

## Complexity Tracking

> No violations requiring justification.

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| SSE Architecture | Single endpoint with broadcast | Matches reference sample; scales to N boards |
| Game State | GameSupervisor (existing) | Already uses MailboxProcessor per constitution |
| View Rendering | Oxpecker.ViewEngine | Per constitution; already in dependencies |
