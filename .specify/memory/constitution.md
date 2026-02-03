<!--
SYNC IMPACT REPORT
==================
Version change: 1.0.0 → 1.1.0
Ratification: 2025-04-14 (project inception date from first commit)
Last Amended: 2026-02-02

Changes in 1.1.0:
- Added: MailboxProcessor state management requirement to Principle I
- Added: "Make Invalid States Unrepresentable" section for Engine
- Added: Engine stability warning (PROTECTED component)
- Updated: Testing section with Expecto (unit) and Playwright/NUnit (web)

Templates Status:
- .specify/templates/plan-template.md ✅ (no updates needed - generic constitution check section)
- .specify/templates/spec-template.md ✅ (no updates needed - generic format)
- .specify/templates/tasks-template.md ✅ (no updates needed - generic format)

Follow-up TODOs: None
-->

# Tic-Tac-Toe Constitution

## Core Principles

### I. Functional-First F#

All application logic MUST be written in idiomatic F# using functional programming patterns.

- Pure functions preferred; side effects isolated to boundaries (HTTP handlers, database)
- Immutable data structures by default; mutable state only when performance-critical
- Pattern matching over conditional branching where applicable
- Railway-oriented programming for error handling (Result types, Option types)
- Composition over inheritance; functions composed to build complex behavior
- MailboxProcessor MUST be used for stateful components requiring concurrency safety

**Rationale**: F# enables concise, correct code with strong type safety. Functional patterns reduce bugs and improve testability. MailboxProcessor provides thread-safe state management with message-passing semantics.

### II. Hypermedia Architecture

The application MUST use hypermedia-driven interactions through Datastar, minimizing client-side JavaScript.

- Server-side rendering is the primary UI mechanism
- State changes communicated via server-sent events (SSE)
- Declarative data binding through Datastar attributes
- Client JavaScript limited to Datastar runtime; no custom JS unless strictly necessary
- HTML responses carry both data and UI affordances

**Rationale**: Hypermedia reduces complexity, improves accessibility, and keeps application logic on the server where it can be tested and debugged effectively.

### III. Test-First Development

Tests MUST be written before implementation code for non-trivial features.

- Unit tests for game logic and pure functions using **Expecto**
- Web/integration tests using **Playwright** with NUnit as the test runner
- Tests must fail before implementation (Red-Green-Refactor)
- Test coverage focused on behavior, not implementation details

**Rationale**: Test-first development catches defects early and ensures code meets requirements from the start. Expecto provides idiomatic F# testing; Playwright enables reliable browser automation.

### IV. Simplicity & Focus

Every addition MUST be justified by a clear user need. Reject speculative features and premature abstractions.

- YAGNI (You Aren't Gonna Need It) strictly enforced
- No frameworks, patterns, or dependencies without demonstrated necessity
- Direct solutions preferred over "flexible" or "extensible" designs
- If in doubt, leave it out

**Rationale**: This is a focused game application. Complexity has carrying costs; simplicity enables maintainability.

## Protected Components

### TicTacToe.Engine (STABLE - DO NOT MODIFY)

The Engine library (`src/TicTacToe.Engine/`) is considered **stable and complete**. It follows the "Make Invalid States Unrepresentable" pattern where the type system enforces game rules at compile time.

**Modification Policy**:
- Changes to the Engine require **extreme caution** and explicit justification
- The Engine's type design prevents invalid game states by construction
- Before proposing Engine changes, verify the change cannot be handled at the Web layer
- Any Engine modification MUST preserve the invalid-state-prevention guarantees

**Design Principles Enforced**:
- Game state transitions are type-safe; invalid moves cannot compile
- The domain model makes illegal states unrepresentable
- Pure functions with no side effects

**Rationale**: A stable, well-typed game engine is the foundation of correctness. Unnecessary modifications risk introducing bugs in proven code.

## Technology Stack

This section documents the approved technology choices for the project.

**Language**: F# targeting .NET 10.0
**Web Framework**: ASP.NET Core with Frank.Builder routing
**View Rendering**: Oxpecker for HTML generation
**Hypermedia**: StarFederation.Datastar for server-sent events and data binding
**State Management**: MailboxProcessor for concurrent state
**Unit Testing**: Expecto
**Web Testing**: Playwright with NUnit
**Build**: dotnet CLI

Changes to the technology stack require constitution amendment.

## Development Workflow

This section establishes the standard development process.

**Feature Development**:
1. Create feature specification before implementation
2. Write failing tests per Principle III
3. Implement until tests pass
4. Refactor while keeping tests green
5. Review for compliance with all principles

**Code Review Requirements**:
- All changes reviewed before merge to main
- Reviewer verifies principle compliance
- Tests must pass in CI before merge

**Branch Strategy**:
- `main` branch is always deployable
- Feature work on topic branches
- Squash merge preferred for clean history

## Governance

This constitution is the authoritative source for project standards. All development decisions MUST comply with its principles.

**Amendment Process**:
1. Propose amendment with rationale
2. Document impact on existing code
3. Update version per semantic versioning:
   - MAJOR: Principle removal or incompatible redefinition
   - MINOR: New principle or significant guidance expansion
   - PATCH: Clarifications and wording improvements
4. Update LAST_AMENDED_DATE upon change

**Compliance Review**:
- PRs must verify alignment with principles
- Constitution violations block merge
- Exceptions require explicit justification documented in PR

**Version**: 1.1.0 | **Ratified**: 2025-04-14 | **Last Amended**: 2026-02-02
