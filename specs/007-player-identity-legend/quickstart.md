# Quickstart: Player Identity & Legend Display

**Feature**: 007-player-identity-legend
**Date**: 2026-02-05

## Overview

This feature adds three UI elements:
1. **User identity display** in the top-right of the page layout
2. **Player legend** below each game board showing X/O → player mapping
3. **Bold turn indicator** on the active player's legend entry

## Files to Modify

### Template Layer (rendering)
- `src/TicTacToe.Web/templates/shared/layout.fs` — Add user identity display to page header
- `src/TicTacToe.Web/templates/game.fs` — Add legend section to game board rendering; add CSS for legend and header
- `src/TicTacToe.Web/templates/home.fs` — No changes expected (layout handles identity display)

### Handler Layer (data plumbing)
- `src/TicTacToe.Web/Handlers.fs` — Pass `PlayerAssignmentManager` to `subscribeToGame`; update `renderGameBoardForBroadcast` calls to include assignment data

### No Changes Required
- `src/TicTacToe.Engine/*` — Protected component; no changes
- `src/TicTacToe.Web/Model.fs` — Existing `PlayerAssignment` type sufficient
- `src/TicTacToe.Web/Auth.fs` — Existing `TryGetUserId()` sufficient
- `src/TicTacToe.Web/SseBroadcast.fs` — No changes needed

## Key Design Decisions

1. **No new types**: Legend data is derived at render time from existing `PlayerAssignment` and `MoveResult`
2. **8-char GUID prefix**: `id.[..7]` for compact display (e.g., `a3f2b1c9`)
3. **Same identifier everywhere**: No "You" label; raw ID in header and legend for consistency across SSE broadcasts
4. **Legend inside game board div**: Rendered as part of the `game-board` container so SSE `patchElements` updates it atomically with the board
5. **`subscribeToGame` signature change**: Needs `PlayerAssignmentManager` parameter to query assignments in observer callbacks

## Implementation Order

1. Layout header (user identity display) — independent, no game logic
2. Update `renderGameBoardWithContext` signature to accept assignment data for legend
3. Update `renderGameBoardForBroadcast` signature to accept assignment data for legend
4. Add legend rendering helper function
5. Update `subscribeToGame` to pass assignment manager; update all call sites
6. Add CSS styles for header and legend
7. Tests
