# Data Model: Player Identity & Legend Display

**Feature**: 007-player-identity-legend
**Date**: 2026-02-05

## Existing Entities (No Changes Required)

### PlayerAssignment
Already exists in `Model.fs`. No schema changes needed.

```
PlayerAssignment
├── GameId: string
├── PlayerXId: string option
└── PlayerOId: string option
```

### User Identity (Claims)
Already exists via `ClaimTypes.UserId` ("sub") claim in `Auth.fs`. No changes needed.

## Display Model (Rendering Only)

### Legend Display Data
Derived at render time from existing data. Not persisted.

```
LegendDisplay (computed at render time)
├── PlayerXLabel: string          // First 8 chars of PlayerXId, or "Waiting for player..."
├── PlayerOLabel: string          // First 8 chars of PlayerOId, or "Waiting for player..."
├── ActivePlayer: Player option   // X, O, or None (game over)
```

**Derivation**:
- `PlayerXLabel` = `assignment.PlayerXId |> Option.map (fun id -> id.[..7]) |> Option.defaultValue "Waiting for player..."`
- `PlayerOLabel` = `assignment.PlayerOId |> Option.map (fun id -> id.[..7]) |> Option.defaultValue "Waiting for player..."`
- `ActivePlayer` = extracted from `MoveResult` pattern match (existing logic in `renderGameBoard*` functions)

### User Identity Display
Derived at render time from `HttpContext.User`.

```
UserIdentityDisplay (computed at render time)
├── ShortId: string               // First 8 chars of user's GUID
```

**Derivation**:
- `ShortId` = `ctx.User.TryGetUserId() |> Option.map (fun id -> id.[..7]) |> Option.defaultValue ""`

## State Transitions

No new state transitions introduced. The legend display is a pure function of existing state:
- `PlayerAssignment` changes when players make moves (already handled by `PlayerAssignmentManager`)
- `MoveResult` changes when moves are made (already handled by game engine)
- The legend simply reflects these existing state changes in the UI

## Relationships

```
User (claims) ──1:N──> PlayerAssignment (as PlayerXId or PlayerOId)
Game ──1:1──> PlayerAssignment
Game ──1:1──> MoveResult (current state)
Legend = f(PlayerAssignment, MoveResult)  // Pure derivation, no storage
```
