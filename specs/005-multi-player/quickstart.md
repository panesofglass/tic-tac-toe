# Quickstart: Multi-Player Tic-Tac-Toe

**Date**: 2026-02-04
**Feature**: 005-multi-player

## Prerequisites

- .NET 10.0 SDK
- Two browsers or browser profiles (for testing multi-player)

## Build & Run

```bash
# From repository root
dotnet build
dotnet run --project src/TicTacToe.Web
```

Application starts at `http://localhost:5000` (or configured port).

## Test Multi-Player Locally

### Setup Two Players

1. **Browser A** (Player X):
   - Open `http://localhost:5000`
   - Click "New Game"
   - Note the game URL (e.g., `/games/abc123`)

2. **Browser B** (Player O):
   - Open the same game URL in a different browser or incognito window
   - Each browser has a unique cookie, so they're treated as different users

### Play a Game

1. **Browser A**: Click any cell → You become Player X, X is placed
2. **Browser B**: Click any cell → You become Player O, O is placed
3. Continue alternating moves
4. If wrong player tries to move → Board shakes briefly (rejection feedback)

### Test Spectator Mode

1. Start a game between Browser A and Browser B (both make at least one move)
2. Open **Browser C** (third incognito window)
3. Navigate to the same game URL
4. Browser C can see the board but clicking cells triggers rejection animation

## Run Tests

```bash
# Unit tests (Expecto)
dotnet test test/TicTacToe.Engine.Tests

# Web tests (Playwright + NUnit)
dotnet test test/TicTacToe.Web.Tests
```

### Multi-Player Specific Tests

```bash
# Run only multi-player tests
dotnet test test/TicTacToe.Web.Tests --filter "FullyQualifiedName~MultiPlayer"
```

## Key Files

| File | Purpose |
|------|---------|
| `src/TicTacToe.Web/Model.fs` | PlayerAssignment types |
| `src/TicTacToe.Web/Handlers.fs` | Move validation with player check |
| `src/TicTacToe.Web/templates/game.fs` | UI with role-based rendering |
| `test/TicTacToe.Web.Tests/MultiPlayerTests.fs` | Integration tests |

## Debugging Tips

### Check Your User ID

The user ID is stored in the `TicTacToe.User` cookie. In browser dev tools:
- Application → Cookies → `TicTacToe.User`
- This is encrypted; the actual GUID is in the `sub` claim server-side

### Verify Player Assignment

Add logging in `Handlers.fs`:
```fsharp
log.LogInformation("User {UserId} attempting move on game {GameId}", userId, gameId)
```

### Reset Player State

- Clear cookies in browser to get a new user ID
- Restart the application to clear all in-memory game and assignment state

## Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| Can't make moves | Playing in same browser tab | Use different browsers/incognito |
| Always Player X | Same cookie across windows | Use truly separate browser profiles |
| No rejection animation | CSS not loading | Check browser console for errors |
| "Game not found" | Game expired (1 hour timeout) | Create a new game |

## Architecture Overview

```
Browser A (Cookie: user-A)    Browser B (Cookie: user-B)
         │                              │
         └──────────┬───────────────────┘
                    │ HTTP
                    ▼
            ┌───────────────┐
            │  Handlers.fs  │ ← Validates user can make move
            └───────┬───────┘
                    │
         ┌──────────┼──────────┐
         ▼                     ▼
┌─────────────────┐   ┌──────────────────┐
│ PlayerAssignment│   │   Game Engine    │
│   (Web layer)   │   │   (protected)    │
│                 │   │                  │
│ X: user-A       │   │ Current: OTurn   │
│ O: user-B       │   │ Board: [X,O,...] │
└─────────────────┘   └──────────────────┘
```
