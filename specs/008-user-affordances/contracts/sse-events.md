# SSE Event Contracts: User-Specific Affordances

**Feature Branch**: `008-user-affordances`
**Date**: 2026-02-06

## Overview

This application uses Server-Sent Events (SSE) with Datastar protocol for real-time updates. There are no traditional REST API contracts to change — the existing HTTP endpoints (POST /games/{id}, DELETE /games/{id}, POST /games/{id}/reset) remain unchanged. The contract change is in what HTML content is delivered via SSE to each subscriber.

## SSE Event Types (unchanged)

```
PatchElements(html)              — Replace matched element with new HTML
PatchElementsAppend(selector, html) — Append HTML to selector
RemoveElement(selector)          — Remove element from DOM
PatchSignals(json)               — Update Datastar signals
```

## Changed Contracts

### Game Board HTML Fragment (PatchElements / PatchElementsAppend)

**Current**: Single HTML fragment broadcast to all subscribers, rendered via `renderGameBoardForBroadcast` (no user context).

**New**: Per-role HTML fragments delivered to each subscriber based on their PlayerRole for the affected game. Rendered via `renderGameBoardWithContext` (or equivalent) with appropriate role, assignment, and gameCount.

#### Fragment Variants Per Game State Change

For each game state change, the server renders up to 4 distinct HTML fragments:

1. **Active Player View** (role = PlayerX on X's turn, or PlayerO on O's turn)
   - Clickable squares for valid moves
   - Reset button (enabled)
   - Delete button (enabled)

2. **Waiting Player View** (role = PlayerX on O's turn, or PlayerO on X's turn)
   - Non-interactive squares (same appearance, no buttons/links)
   - Reset button (enabled)
   - Delete button (enabled)

3. **Claimable Unassigned View** (role = UnassignedX on X's turn, or UnassignedO on O's turn)
   - Clickable squares for valid moves (allows auto-assignment)
   - Reset button: enabled only if board count > 6
   - Delete button: enabled only if board count > 6

4. **Non-Player View** (role = Spectator, or unassigned on wrong turn)
   - Non-interactive squares
   - Reset button: enabled only if board count > 6
   - Delete button: enabled only if board count > 6

#### Subscriber-to-Fragment Routing

```
For each subscriber (userId, channel):
  role = PlayerAssignmentManager.GetRole(gameId, userId)
  fragment = match role, currentTurn, boardCount with appropriate variant
  channel.TryWrite(PatchElements(fragment))
```

### Game Creation (PatchElementsAppend)

**Current**: Broadcasts single new-game HTML to all via `renderGameBoardForBroadcast`.

**New**: Broadcasts per-role new-game HTML. Since a new game has no assigned players, all users receive the same non-player or unassigned view (variant 3 or 4 depending on board count).

### Game Deletion (RemoveElement)

**Unchanged**: `RemoveElement("#game-{gameId}")` is the same for all users — no role-specific content.

### Game Reset (RemoveElement + PatchElementsAppend)

**Current**: Broadcasts removal of old game + addition of new game (same HTML to all).

**New**: Removal is unchanged (same for all). Addition of new game uses per-role rendering (same as Game Creation — new game has no assigned players).

### Rejection Signals (PatchSignals)

**Current**: Broadcasts rejection animation to ALL subscribers.

**New consideration**: Rejection signals should ideally only be sent to the user who triggered the rejection, not all subscribers. This requires targeted send to a specific userId's channel(s).

## HTTP Endpoints (unchanged)

No changes to HTTP request/response contracts:

| Method | Path | Request | Response | Auth |
|--------|------|---------|----------|------|
| POST | /games | — | 201 + Location | Required |
| GET | /games/{id} | — | 200 + HTML | — |
| POST | /games/{id} | signals: gameId, player, position | 200/4xx | Required |
| DELETE | /games/{id} | — | 204/403/409 | Required |
| POST | /games/{id}/reset | — | 200/403 | Required |
| GET | /sse | — | SSE stream | Required |

Server-side authorization on all mutating endpoints remains unchanged as the authoritative security boundary.
