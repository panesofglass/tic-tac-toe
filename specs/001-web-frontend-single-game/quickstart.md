# Quickstart: Web Frontend Single Game

**Feature**: 001-web-frontend-single-game
**Date**: 2026-02-02

## Prerequisites

- .NET 10.0 SDK
- Modern web browser with SSE support (Chrome, Firefox, Safari, Edge)

## Run the Application

```bash
cd /Users/ryanr/Code/tic-tac-toe
dotnet run --project src/TicTacToe.Web
```

The application starts at `http://localhost:5000` (or `https://localhost:5001`).

## Play the Game

1. **Open the home page** - Navigate to `http://localhost:5000`
2. **See the board** - A 3x3 grid appears with all empty squares
3. **Make a move** - Click any empty square to place X
4. **Alternate turns** - X and O take turns (played from same browser)
5. **Win or draw** - Game detects three-in-a-row or full board
6. **Start over** - Click "New Game" after game ends

## Verify SSE Connection

Open browser DevTools → Network tab → Filter by "EventStream":
- You should see a persistent connection to `/sse`
- Updates appear as SSE events when you click squares

## Run Tests

```bash
# Unit tests (Expecto)
dotnet test test/TicTacToe.Engine.Tests

# Web tests (Playwright + NUnit)
dotnet test test/TicTacToe.Web.Tests
```

## Troubleshooting

### Board doesn't load
- Check browser console for JavaScript errors
- Verify SSE connection in Network tab
- Ensure Datastar script is loaded

### Clicks don't register
- Check Network tab for POST to `/move`
- Verify response is 202 Accepted
- Check SSE stream for update events

### "New Game" doesn't appear
- Only shows after game ends (win or draw)
- Play a complete game to see it

## Architecture Overview

```
Browser                          Server
   │                                │
   │──── GET / ─────────────────────►│ Home page with board
   │◄─────────────────────────────── │
   │                                │
   │──── GET /sse ──────────────────►│ SSE connection
   │◄──── Initial board state ────── │
   │                                │
   │──── POST /move ────────────────►│ Submit move
   │◄──── 202 Accepted ──────────── │
   │◄──── SSE: Updated board ─────── │
   │                                │
   │──── POST /reset ───────────────►│ New game
   │◄──── 202 Accepted ──────────── │
   │◄──── SSE: Empty board ───────── │
```

## Files Modified

| File | Purpose |
|------|---------|
| `src/TicTacToe.Web/Program.fs` | Route configuration |
| `src/TicTacToe.Web/Handlers.fs` | HTTP handlers with SSE broadcast |
| `src/TicTacToe.Web/templates/home.fs` | Home page with embedded board |
| `src/TicTacToe.Web/templates/game.fs` | Game board rendering |
| `test/TicTacToe.Web.Tests/*.fs` | Playwright integration tests |
