# Quickstart: Multi-Game REST API

**Feature**: 002-multi-game-rest-api
**Date**: 2026-02-03

## Prerequisites

- .NET 10.0 SDK
- Modern browser with SSE support

## Build & Run

```bash
# Build
dotnet build

# Run web server
dotnet run --project src/TicTacToe.Web

# Open browser
open http://localhost:5000
```

## Test

```bash
# Unit tests (Engine)
dotnet test test/TicTacToe.Engine.Tests

# Web/integration tests (requires server running)
dotnet run --project src/TicTacToe.Web &
dotnet test test/TicTacToe.Web.Tests
```

## API Usage

### Create a New Game

```bash
curl -X POST http://localhost:5000/games -i
# Response:
# HTTP/1.1 201 Created
# Location: /games/550e8400-e29b-41d4-a716-446655440000
```

### View a Game

```bash
curl http://localhost:5000/games/550e8400-e29b-41d4-a716-446655440000
# Response: HTML game board
```

### Make a Move

```bash
curl -X POST http://localhost:5000/games/550e8400-e29b-41d4-a716-446655440000 \
  -H "Content-Type: application/json" \
  -d '{"gameId": "550e8400-e29b-41d4-a716-446655440000", "player": "X", "position": "MiddleCenter"}'
# Response: 202 Accepted
```

### Delete a Game

```bash
curl -X DELETE http://localhost:5000/games/550e8400-e29b-41d4-a716-446655440000 -i
# Response: 204 No Content
```

### SSE Stream (for debugging)

```bash
curl http://localhost:5000/sse
# Streams Datastar events as games change
```

## Key Files

| File | Purpose |
|------|---------|
| `src/TicTacToe.Web/Program.fs` | Route definitions |
| `src/TicTacToe.Web/Handlers.fs` | Request handlers |
| `src/TicTacToe.Web/SseBroadcast.fs` | SSE event broadcasting |
| `src/TicTacToe.Web/templates/game.fs` | Game board rendering |
| `src/TicTacToe.Engine/Engine.fs` | Game supervisor (multi-game) |

## Architecture

```
Browser                          Server
   │                                │
   │◄──── GET / ────────────────────│  Home page with Datastar
   │                                │
   │──── GET /sse ─────────────────►│  SSE connection
   │◄─── (stream) ──────────────────│
   │                                │
   │──── POST /games ──────────────►│  Create game
   │◄─── 201 + Location ────────────│
   │◄─── SSE: append game ──────────│
   │                                │
   │──── POST /games/{id} ─────────►│  Make move
   │◄─── 202 ───────────────────────│
   │◄─── SSE: patch game ───────────│
   │                                │
   │──── DELETE /games/{id} ───────►│  Delete game
   │◄─── 204 ───────────────────────│
   │◄─── SSE: remove game ──────────│
```

## Debugging

### Check Active Games

The GameSupervisor tracks all games. Active game count available in logs.

### SSE Events

Open browser DevTools → Network → Filter by "sse" to see real-time events.

### Datastar Signals

Open browser DevTools → Console → Type `window.datastarSignals` to inspect current state.
