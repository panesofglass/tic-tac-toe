# Data Model: SCXML Statechart for Tic-Tac-Toe

**Feature**: 009-extract-scxml
**Date**: 2026-02-15

## Overview

The statechart uses three orthogonal parallel regions within a single SCXML document. The `datamodel="ecmascript"` attribute enables expressive guard conditions and data assignments.

## Data Variables

| Variable       | Type                      | Initial Value                | Purpose                                    |
| -------------- | ------------------------- | ---------------------------- | ------------------------------------------ |
| `board`        | Object (9 keys → string)  | All keys set to `'Empty'`    | Board state: 9 positions, each Empty/X/O   |
| `playerXId`    | string or null            | `null`                       | Authenticated user playing X               |
| `playerOId`    | string or null            | `null`                       | Authenticated user playing O               |
| `gameCount`    | number                    | `6`                          | Active games in supervisor (for min guard)  |
| `winner`       | string or null            | `null`                       | Winning player ('X' or 'O') when Won       |

### Board Position Keys

`TopLeft`, `TopCenter`, `TopRight`, `MiddleLeft`, `MiddleCenter`, `MiddleRight`, `BottomLeft`, `BottomCenter`, `BottomRight`

### Board Square Values

`'Empty'`, `'X'`, `'O'`

## Parallel Region 1: Game Play

**Initial state**: XTurn

```
                        ┌──────────────────────────────────────────┐
                        │              GamePlay                     │
                        │                                          │
                        │  ┌─[H]─ GamePlayHistory                  │
                        │  │                                       │
  ┌─────────┐   move.x  │  ▼                                       │
  │         │ [valid &  │  ┌──────┐   move.o   ┌──────┐           │
  │  Won ◉  │◄─wins]────┤  │XTurn │──[valid]──►│OTurn │           │
  │         │           │  └──┬───┘◄──[valid]──┘──┬───┘           │
  └─────────┘           │     │                    │               │
                        │     │ [invalid]          │ [invalid]     │
  ┌─────────┐           │     ▼                    ▼               │
  │ Draw ◉  │◄─[full]──┤  ┌─────────────────────────┐            │
  │         │           │  │       MoveError          │            │
  └─────────┘           │  │  (transitions to History)│            │
                        │  └─────────────────────────┘            │
                        └──────────────────────────────────────────┘
```

### States

| State            | Type     | Description                                          |
| ---------------- | -------- | ---------------------------------------------------- |
| `XTurn`          | state    | X to move. All valid moves for X are available.      |
| `OTurn`          | state    | O to move. All valid moves for O are available.      |
| `Won`            | final    | Game over with a winner. `winner` data variable set. |
| `Draw`           | final    | Game over, board full, no winner.                    |
| `MoveError`      | state    | Transient: invalid move. Returns via history.        |
| `GamePlayHistory`| history  | Shallow history restoring previous turn state.       |

### Transitions

| From       | Event    | Guard                                               | Target     | Actions                                           |
| ---------- | -------- | --------------------------------------------------- | ---------- | ------------------------------------------------- |
| `XTurn`    | `move.x` | `board[pos] === 'Empty' && wouldWin('X', pos)`     | `Won`      | `board[pos] = 'X'`, `winner = 'X'`               |
| `XTurn`    | `move.x` | `board[pos] === 'Empty' && wouldFillBoard(pos)`    | `Draw`     | `board[pos] = 'X'`                                |
| `XTurn`    | `move.x` | `board[pos] === 'Empty'`                            | `OTurn`    | `board[pos] = 'X'`                                |
| `XTurn`    | `move`   | *(else: wrong player or taken square)*              | `MoveError`| *(none)*                                          |
| `OTurn`    | `move.o` | `board[pos] === 'Empty' && wouldWin('O', pos)`     | `Won`      | `board[pos] = 'O'`, `winner = 'O'`               |
| `OTurn`    | `move.o` | `board[pos] === 'Empty' && wouldFillBoard(pos)`    | `Draw`     | `board[pos] = 'O'`                                |
| `OTurn`    | `move.o` | `board[pos] === 'Empty'`                            | `XTurn`    | `board[pos] = 'O'`                                |
| `OTurn`    | `move`   | *(else: wrong player or taken square)*              | `MoveError`| *(none)*                                          |
| `MoveError`| *(auto)* | *(eventless — fires immediately)*                  | `GamePlayHistory` | *(none)*                                   |

**Note**: SCXML evaluates transitions in document order. The win check comes first, then draw, then normal move, then error fallback.

### Guard Functions (conceptual, expressed in ECMAScript)

- `wouldWin(player, pos)` — Returns true if placing `player` at `pos` completes any of the 8 winning lines
- `wouldFillBoard(pos)` — Returns true if `pos` is the last empty square (9th move)
- `isValidMove(pos)` — Alias for `board[pos] === 'Empty'`

## Parallel Region 2: Player Identity

**Initial state**: Unassigned

```
  ┌───────────┐  move.x [no X]  ┌───────────────┐
  │ Unassigned │────────────────►│ XOnlyAssigned  │
  └───────────┘                  └───────┬────────┘
                                         │ move.o [diff user]
                                         ▼
                                 ┌───────────────┐
                                 │ BothAssigned   │
                                 └───────────────┘
                                         ▲
  ┌───────────────┐  move.x [diff user]  │
  │ OOnlyAssigned  │─────────────────────┘
  └───────────────┘
```

### States

| State             | Type  | Description                                     |
| ----------------- | ----- | ----------------------------------------------- |
| `Unassigned`      | state | No participants have claimed a role.             |
| `XOnlyAssigned`   | state | One participant claimed X; O slot open.          |
| `OOnlyAssigned`   | state | One participant claimed O; X slot open.          |
| `BothAssigned`    | state | Both roles claimed by distinct participants.     |

### Transitions

| From              | Event    | Guard                                               | Target           | Actions                         |
| ----------------- | -------- | --------------------------------------------------- | ---------------- | ------------------------------- |
| `Unassigned`      | `move.x` | `playerXId === null`                                | `XOnlyAssigned`  | `playerXId = userId`            |
| `Unassigned`      | `move.o` | `playerOId === null`                                | `OOnlyAssigned`  | `playerOId = userId`            |
| `XOnlyAssigned`   | `move.o` | `userId !== playerXId && playerOId === null`         | `BothAssigned`   | `playerOId = userId`            |
| `OOnlyAssigned`   | `move.x` | `userId !== playerOId && playerXId === null`         | `BothAssigned`   | `playerXId = userId`            |

**Note**: No `<final>` state in this region. Player identity persists for the life of the game session, even after the game play concludes.

## Parallel Region 3: Game Session Lifecycle

**Initial state**: Active

```
  ┌────────┐  game.dispose [participant OR count > 6]  ┌──────────┐
  │ Active  │──────────────────────────────────────────►│ Disposed ◉│
  │        │──game.reset [participant & hasActivity]───►│          │
  │        │──game.timeout──────────────────────────────►│          │
  └────────┘                                            └──────────┘
```

### States

| State      | Type  | Description                                             |
| ---------- | ----- | ------------------------------------------------------- |
| `Active`   | state | Game session is live and accepting events.               |
| `Disposed` | final | Game session terminated. No further events processed.    |

### Transitions

| From     | Event          | Guard                                                          | Target     | Actions  |
| -------- | -------------- | -------------------------------------------------------------- | ---------- | -------- |
| `Active` | `game.dispose` | `isParticipant(userId) \|\| gameCount > 6`                      | `Disposed` | *(none)* |
| `Active` | `game.reset`   | `isParticipant(userId) && hasActivity()`                       | `Disposed` | *(none)* |
| `Active` | `game.timeout` | *(unconditional — cleanup timer)*                              | `Disposed` | *(none)* |

### Guard Functions

- `isParticipant(userId)` — Returns true if `userId === playerXId || userId === playerOId`
- `hasActivity()` — Returns true if any board square is taken OR any player is assigned

**Note**: Game reset disposes the current session. A new session is created separately — the reset is modeled as disposal from the current session's perspective.

## Event Catalog

| Event           | Data Payload                  | Source (conceptual)       |
| --------------- | ----------------------------- | ------------------------- |
| `move.x`        | `{ position, userId }`        | Participant action        |
| `move.o`        | `{ position, userId }`        | Participant action        |
| `move`          | `{ position, userId }`        | Participant action (generic, catches invalid) |
| `game.dispose`  | `{ userId }`                  | Participant/spectator action |
| `game.reset`    | `{ userId }`                  | Participant action        |
| `game.timeout`  | *(none)*                      | Supervisor cleanup timer  |

## Cross-Region Interactions

The three regions communicate via shared data variables and the `In()` predicate:

1. **Move events affect both GamePlay and PlayerIdentity**: A `move.x` event triggers transitions in both regions simultaneously (game state advances AND player identity may update).
2. **GameSession can observe GamePlay**: Using `In('Won')` or `In('Draw')` guards, the GameSession region could react to game completion (though in the current design, session disposal is explicitly triggered, not automatic).
3. **PlayerIdentity informs GameSession guards**: `isParticipant()` reads `playerXId`/`playerOId` which are set by PlayerIdentity transitions.
