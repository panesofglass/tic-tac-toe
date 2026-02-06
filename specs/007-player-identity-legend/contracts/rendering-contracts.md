# Rendering Contracts: Player Identity & Legend Display

**Feature**: 007-player-identity-legend
**Date**: 2026-02-05

## Overview

This feature introduces no new HTTP endpoints. All changes are to existing server-rendered HTML fragments. These contracts document the expected HTML output structures.

## Contract 1: Page Layout Header

**Location**: Every authenticated page (rendered by `layout.fs`)

**HTML Structure**:
```html
<header class="page-header">
  <span class="user-identity">{first 8 chars of user GUID}</span>
</header>
<main>
  {page content}
</main>
```

**Conditions**:
- Header rendered only when `ctx.User.TryGetUserId()` returns `Some userId`
- If no user ID available, render `<main>` only (no header)

## Contract 2: Game Board with Legend

**Location**: Inside each `div.game-board` element (rendered by `renderGameBoardWithContext` and `renderGameBoardForBroadcast`)

**HTML Structure** (additions shown with `<!-- NEW -->`):
```html
<div id="game-{gameId}" class="game-board" data-signals="...">
  <div class="status"><h2>{status text}</h2></div>
  <div class="board">{9 squares}</div>
  <!-- NEW: Legend section -->
  <div class="legend">
    <span class="{legend-active if X's turn}">X: {playerXLabel}</span>
    <span class="{legend-active if O's turn}">O: {playerOLabel}</span>
  </div>
  <div class="controls">{reset, delete buttons}</div>
</div>
```

**Legend Labels**:
| State | playerXLabel | playerOLabel |
| --- | --- | --- |
| No players assigned | "Waiting for player..." | "Waiting for player..." |
| Only X assigned | First 8 chars of X's GUID | "Waiting for player..." |
| Both assigned | First 8 chars of X's GUID | First 8 chars of O's GUID |

**Bold (legend-active class) Rules**:
| Game State | X entry class | O entry class |
| --- | --- | --- |
| XTurn | `legend-active` | (none) |
| OTurn | (none) | `legend-active` |
| Won / Draw / Error | (none) | (none) |

## Contract 3: Updated Function Signatures

### renderGameBoardWithContext
```fsharp
// Current:
renderGameBoardWithContext (gameId: string) (result: MoveResult) (userRole: PlayerRole) (assignment: PlayerAssignment option) (gameCount: int)

// No signature change needed - already receives `assignment: PlayerAssignment option`
```

### renderGameBoardForBroadcast
```fsharp
// Current:
renderGameBoardForBroadcast (gameId: string) (result: MoveResult)

// Updated:
renderGameBoardForBroadcast (gameId: string) (result: MoveResult) (assignment: PlayerAssignment option)
```

### subscribeToGame
```fsharp
// Current:
subscribeToGame (gameId: string) (game: Game)

// Updated:
subscribeToGame (gameId: string) (game: Game) (assignmentManager: PlayerAssignmentManager)
```

### mainLayout
```fsharp
// Current:
mainLayout (ctx: HttpContext) (content: HtmlElement)

// No signature change needed - ctx already available for user identity extraction
```
