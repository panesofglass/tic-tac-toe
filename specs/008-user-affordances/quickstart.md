# Quickstart: User-Specific Affordances

**Feature Branch**: `008-user-affordances`
**Date**: 2026-02-06

## Prerequisites

- .NET 10.0 SDK
- Node.js (for Playwright browser install)

## Setup

```bash
# Clone and switch to feature branch
git checkout 008-user-affordances

# Restore dependencies
dotnet restore

# Install Playwright browsers (if not already installed)
pwsh test/TicTacToe.Web.Tests/bin/Debug/net10.0/playwright.ps1 install
```

## Build

```bash
dotnet build
```

## Run

```bash
# Start the web server
dotnet run --project src/TicTacToe.Web

# Server runs on http://localhost:5228 (per launchSettings.json)
```

## Test

```bash
# Run engine unit tests (Expecto)
dotnet test test/TicTacToe.Engine.Tests

# Run web tests (NUnit + Playwright) — requires server running on port 5228
TEST_BASE_URL=http://localhost:5228 dotnet test test/TicTacToe.Web.Tests
```

## Manual Testing

1. Open http://localhost:5228 — auto-redirects to /login for cookie, then to home
2. Open a second browser/incognito window — separate user identity
3. Create games, make moves, observe:
   - Active player sees clickable squares + reset/delete buttons
   - Waiting player sees non-interactive squares + reset/delete buttons
   - Observer (third browser) sees read-only board; reset/delete only when > 6 games exist

## Key Files to Modify

| File | Purpose |
|------|---------|
| `src/TicTacToe.Web/SseBroadcast.fs` | Add userId to subscriber model, add role-aware broadcast |
| `src/TicTacToe.Web/templates/game.fs` | Unify rendering to always use role-aware context |
| `src/TicTacToe.Web/Handlers.fs` | Pass userId at SSE subscribe, use role-aware rendering in SSE initial load and game subscriptions |
| `src/TicTacToe.Web/Model.fs` | No structural changes expected |
| `test/TicTacToe.Web.Tests/` | New tests for role-specific affordance rendering |

## Architecture Overview

```
User connects to /sse
  → Handler captures userId from auth cookie
  → subscribe(userId) stores (userId, channel) pair
  → Initial load: for each game, GetRole(gameId, userId) → renderWithContext
  → Live updates: game state change → render per-role → dispatch to matching subscribers

Game state change (move/reset/create/delete)
  → subscribeToGame's OnNext fires
  → For each subscriber: look up role → render appropriate view → send to channel
  → At most 4 distinct renders per game (active player, waiting player, claimable, non-player)
```
