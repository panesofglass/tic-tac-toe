# Research: Web Frontend Single Game

**Feature**: 001-web-frontend-single-game
**Date**: 2026-02-02

## Research Summary

All technical context is resolved. The existing codebase provides established patterns; no external research required.

## Decisions

### 1. SSE Broadcast Architecture

**Decision**: Use channel-based broadcast pattern from Frank.Datastar.Oxpecker sample

**Rationale**: The reference sample demonstrates exactly the pattern needed:
- Single `/sse` endpoint for all page updates
- Thread-safe subscriber collection using `Channel<SseEvent>`
- Fire-and-forget handlers that broadcast to all connected clients
- Clean subscription lifecycle (subscribe on connect, unsubscribe on disconnect)

**Alternatives Considered**:
- Per-game SSE endpoints (current implementation): More complex routing, doesn't scale to 1000 boards
- WebSockets: Requires custom JS, violates Hypermedia Architecture principle

### 2. Game State Management

**Decision**: Adapt existing GameSupervisor to work with broadcast SSE

**Rationale**: The GameSupervisor already:
- Uses MailboxProcessor per constitution requirement
- Manages game lifecycle (create, get, cleanup)
- Provides IObservable<MoveResult> for state changes

The adaptation needed:
- Instead of per-game SSE subscriptions, subscribe to game state and broadcast via shared channel
- Home page creates/retrieves a single game on load
- All state changes broadcast to all connected clients (trivial for single game)

**Alternatives Considered**:
- New game actor design: Unnecessary; existing design is sound
- Global mutable state: Violates Functional-First principle

### 3. Home Page Integration

**Decision**: Embed game board directly in home.fs template with SSE connection

**Rationale**:
- Spec requires "single board on home page, no redirection"
- Existing game.fs has `renderGameBoard` function that can be reused
- SSE connection via Datastar `data-on-load` attribute

**Alternatives Considered**:
- Separate game component loaded via AJAX: More complex, no benefit
- iframe embedding: Poor UX, complicates SSE

### 4. Move Submission Pattern

**Decision**: Use Datastar `data-on-click` with `@post` to fire-and-forget endpoint

**Rationale**:
- Matches reference sample patterns (contacts, items)
- Handler validates move, updates game state via GameSupervisor
- State change triggers broadcast to all SSE subscribers
- No response body needed; client sees update via SSE

**Alternatives Considered**:
- Form submission: More complex, requires redirect handling
- Custom fetch: Violates no-custom-JS constraint

### 5. New Game Reset

**Decision**: POST to `/games/reset` broadcasts empty board state

**Rationale**:
- Spec clarifies "reset existing board" (not add new board)
- Handler creates new game, replaces current game reference
- Broadcasts initial state (empty board, X's turn)

**Alternatives Considered**:
- Client-side state reset: Violates server-side rendering principle
- Full page reload: Poor UX, unnecessary

## Technology Patterns

### Datastar Attributes Used

| Attribute | Purpose |
|-----------|---------|
| `data-on-load` | Connect to SSE endpoint on page load |
| `data-on-click` | Trigger move submission |
| `data-indicator` | Show loading state during requests |
| `data-attr:disabled` | Disable squares during request |

### SSE Event Types

| Event | Datastar Method | Purpose |
|-------|-----------------|---------|
| Board update | `patchElements` | Replace game board HTML |
| Status change | `patchElements` | Update turn/outcome display |

## Open Questions

None. All technical decisions resolved.
