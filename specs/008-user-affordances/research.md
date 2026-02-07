# Research: User-Specific Affordances

**Feature Branch**: `008-user-affordances`
**Date**: 2026-02-06

## Research Findings

### R1: SSE Subscriber Identity Model

**Decision**: Extend the subscriber model to associate each SSE channel with a userId.

**Rationale**: The current `ResizeArray<Channel<SseEvent>>` in SseBroadcast.fs stores raw channels with no identity. The `sse` handler in Handlers.fs has access to `ctx.User.TryGetUserId()` at connection time. By capturing the userId when `subscribe()` is called, we can deliver role-appropriate content per subscriber.

**Alternatives considered**:
- Client-side Datastar signal toggling (rejected: violates hypermedia principle II — server should control affordances)
- Separate SSE endpoints per role (rejected: overly complex, breaks single-connection model)
- Middleware-based content rewriting (rejected: fragile, adds unnecessary layer)

### R2: Per-Role Rendering vs Per-User Rendering

**Decision**: Render at most 3 HTML fragments per game per state change (Player X view, Player O view, non-player view) and deliver the matching version to each subscriber based on their role.

**Rationale**: For any game, there are exactly 3 distinct views determined by the user's PlayerRole. All non-player users (Spectator, UnassignedX, UnassignedO when not their turn) share the same rendered output. Using role as a cache key avoids O(N) renders for N subscribers.

**Alternatives considered**:
- Render per-subscriber (rejected: unnecessary duplication when most observers share identical views)
- Pre-render all 3 views on every state change regardless of who's connected (acceptable optimization, simpler than lazy rendering)

### R3: Broadcast Function Redesign

**Decision**: Replace global `broadcast(event)` with a role-aware function that accepts a mapping from role-category to SseEvent, then dispatches the correct event to each subscriber based on their userId and role lookup.

**Rationale**: The current `broadcast()` sends identical events to all channels. A role-aware dispatch function keeps the broadcast abstraction but adds per-subscriber routing. The PlayerAssignmentManager already provides `GetRole(gameId, userId)` which returns the exact role needed for dispatch.

**Alternatives considered**:
- Multiple broadcast channels per role (rejected: adds complexity, role can change mid-session)
- Store role in subscriber record (rejected: role is per-game, not per-connection; a user has different roles across different games)

### R4: Unassigned User Affordances

**Decision**: Unassigned users (UnassignedX when it's X's turn, UnassignedO when it's O's turn) see move controls to allow auto-assignment via first move. All other unassigned states see the non-player view.

**Rationale**: The existing auto-assignment behavior (FR-010) requires that unassigned users on the active turn can still click squares. This means the role-to-view mapping is:
- PlayerX on X's turn → full controls (moves + reset + delete)
- PlayerO on O's turn → full controls (moves + reset + delete)
- PlayerX on O's turn → limited controls (reset + delete only)
- PlayerO on X's turn → limited controls (reset + delete only)
- UnassignedX on X's turn → move controls only (no reset/delete until assigned)
- UnassignedO on O's turn → move controls only (no reset/delete until assigned)
- All other roles → non-player view (no moves; reset/delete only when board count > 6)

**Correction**: This means there are up to **5 distinct views** per game, not 3:
1. Active player with management controls (assigned + their turn)
2. Waiting player with management controls only (assigned + opponent's turn)
3. Unassigned user with move controls (unassigned + their slot's turn)
4. Non-player with management controls (observer/unassigned when board count > 6)
5. Non-player read-only (observer/unassigned when board count ≤ 6)

However, views 4 and 5 differ only by the presence of reset/delete buttons, which depends on the global board count — a value shared across all games. So the practical render set per game is:
- At most 3 role-specific renders (active player, waiting player, unassigned-claimable)
- Plus 1 non-player render (with or without management buttons based on board count)
- = **at most 4 distinct renders per game**

### R5: Reset/Delete Button Visibility Rules

**Decision**: The reset and delete button visibility follows these rules:
1. Assigned players (PlayerX, PlayerO) → always see reset and delete buttons
2. All authenticated users → see reset and delete when board count > 6
3. Unauthenticated users → see no affordances at all

**Rationale**: This preserves the prior requirement that any authenticated user can help manage game pile-up at the > 6 threshold, while giving assigned players persistent management access.

### R6: Handler-Level Changes for Role-Aware Initial Load

**Decision**: Modify the SSE endpoint's initial game population loop to use `renderGameBoardWithContext` (or equivalent) with the connecting user's role, instead of `renderGameBoardForBroadcast`.

**Rationale**: The SSE handler at Handlers.fs line 143 has full access to `ctx.User.TryGetUserId()`. During the initial game iteration (lines 154-163), it can look up the user's role per game and render personalized content. This addresses both the initial page load and the real-time update paths.
