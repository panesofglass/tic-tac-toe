# Data Model: User-Specific Affordances

**Feature Branch**: `008-user-affordances`
**Date**: 2026-02-06

## Entities

### Existing Entities (no changes)

#### User
- **userId**: string (GUID, stored as "sub" claim in authentication cookie)
- **Relationships**: May be assigned to 0..N games via PlayerAssignment

#### Game Board (Game)
- **gameId**: string (GUID)
- **state**: MoveResult (XTurn | OTurn | Won | Draw | Error)
- **gameState**: IReadOnlyDictionary<SquarePosition, SquareState>
- **Relationships**: Has 0..2 assigned players via PlayerAssignment

#### PlayerAssignment
- **GameId**: string
- **PlayerXId**: string option
- **PlayerOId**: string option
- **Relationships**: Links User to Game Board for X and O slots

#### PlayerRole (discriminated union)
- **PlayerX**: User is assigned as X
- **PlayerO**: User is assigned as O
- **Spectator**: Both slots filled, user is neither
- **UnassignedX**: X slot open, user can claim by moving
- **UnassignedO**: O slot open (X assigned), user can claim by moving

### Modified Entities

#### SSE Subscriber (SseBroadcast.fs)

**Current**: `Channel<SseEvent>` (no identity)

**New**:
- **userId**: string (captured from HttpContext at connection time)
- **channel**: Channel<SseEvent>

This is the only structural data change required. The subscriber record must carry the userId so the broadcast mechanism can look up the user's role per game and deliver the appropriate rendered content.

## Affordance Matrix

The following matrix defines which controls are rendered for each combination of PlayerRole and game state:

| PlayerRole | Game Active (their turn) | Game Active (opponent's turn) | Game Completed | Board Count ≤ 6 | Board Count > 6 |
|---|---|---|---|---|---|
| PlayerX (X's turn) | Move squares + Reset + Delete | — | — | — | — |
| PlayerX (O's turn) | — | Reset + Delete | Reset + Delete | (same) | (same) |
| PlayerO (O's turn) | Move squares + Reset + Delete | — | — | — | — |
| PlayerO (X's turn) | — | Reset + Delete | Reset + Delete | (same) | (same) |
| UnassignedX (X's turn) | Move squares only | — | — | No reset/delete | Reset + Delete |
| UnassignedO (O's turn) | Move squares only | — | — | No reset/delete | Reset + Delete |
| Spectator | — | — | — | No controls | Reset + Delete |
| UnassignedX (O's turn) | — | — | — | No controls | Reset + Delete |
| UnassignedO (X's turn) | — | — | — | No controls | Reset + Delete |
| Unauthenticated | No controls | No controls | No controls | No controls | No controls |

**Key**: "Move squares" = clickable empty positions; "Reset + Delete" = game management buttons; "No controls" = fully read-only board.

## State Transitions Affecting Affordances

1. **Move made** → Turn changes → Active player's affordances flip with waiting player's
2. **Auto-assignment** → UnassignedX/O becomes PlayerX/O → Gains persistent reset/delete access
3. **Game completed** → All move controls removed → Assigned players keep reset/delete
4. **Game created** → Board count changes → May cross > 6 threshold for observer management buttons
5. **Game deleted** → Board count changes → May drop to ≤ 6, removing observer management buttons
6. **Game reset** → Old game removed, new game created → Role assignments cleared, board count may change
