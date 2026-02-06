# Quickstart: Game Reset and Persistent Game Boards

**Feature**: 006-game-reset | **Date**: 2026-02-05

## Overview

This feature replaces the "Delete Game" button with "Reset Game" functionality and ensures the application always has at least six game boards available.

## Key Changes Summary

| Component | Change |
|-----------|--------|
| Templates | Replace Delete button with Reset + conditional Delete |
| Handlers | Add `resetGame` handler; modify `deleteGame` validation |
| Program | Create 6 games on startup |
| Model | No structural changes; use existing patterns |

## Implementation Order

### Phase 1: Tests First (TDD)
1. Write Playwright tests for reset functionality
2. Write Playwright tests for initial 6 games
3. Write Playwright tests for delete button visibility
4. All tests should fail initially

### Phase 2: Initial Games on Startup
1. Modify `Program.fs` to create 6 games on `ApplicationStarted`
2. Ensure games are subscribed to SSE

### Phase 3: Reset Handler
1. Add `resetGame` handler in `Handlers.fs`
2. Add route `POST /games/{id}/reset` in `Program.fs`
3. Implement authorization check (must be assigned player)
4. Implement reset logic (dispose old, create new, broadcast)

### Phase 4: Template Updates
1. Modify `game.fs` to render Reset and Delete buttons
2. Add enable/disable logic based on player role and game count
3. Add CSS for disabled button state

### Phase 5: Delete Handler Updates
1. Modify `deleteGame` to check game count > 6
2. Return 409 Conflict if deletion would go below 6
3. Add authorization check (must be assigned player)

## File-by-File Changes

### `src/TicTacToe.Web/Program.fs`

```fsharp
// Add after app.Build()
let lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>()
let supervisor = app.Services.GetRequiredService<GameSupervisor>()

lifetime.ApplicationStarted.Register(fun () ->
    // Create 6 initial games
    for _ in 1..6 do
        let (gameId, game) = supervisor.CreateGame()
        subscribeToGame gameId game
) |> ignore

// Add route
route "/games/{id}/reset" POST (requiresAuth resetGame)
```

### `src/TicTacToe.Web/Handlers.fs`

```fsharp
/// POST /games/{id}/reset - Reset a game
let resetGame (ctx: HttpContext) =
    task {
        let supervisor = ctx.RequestServices.GetRequiredService<GameSupervisor>()
        let assignmentManager = ctx.RequestServices.GetRequiredService<PlayerAssignmentManager>()
        let gameId = ctx.Request.RouteValues.["id"] |> string
        let userId = ctx.GetUserId()

        match supervisor.GetGame(gameId) with
        | None -> ctx.Response.StatusCode <- 404
        | Some oldGame ->
            // Check authorization
            let! role = assignmentManager.GetRole(gameId, userId)
            match role with
            | PlayerX | PlayerO ->
                // Create new game
                let (newGameId, newGame) = supervisor.CreateGame()
                subscribeToGame newGameId newGame

                // Dispose old game and clear assignments
                assignmentManager.RemoveGame(gameId)
                oldGame.Dispose()

                // Broadcast replacement
                let gameCount = supervisor.GetActiveGameCount()
                let html = renderGameBoard newGameId (newGame.GetState()) role gameCount
                broadcast (PatchElements (Render.toString html))

                ctx.Response.StatusCode <- 200
                ctx.Response.Headers.Location <- $"/games/{newGameId}"
            | _ ->
                ctx.Response.StatusCode <- 403
    }
```

### `src/TicTacToe.Web/templates/game.fs`

```fsharp
// Modify renderGameBoard signature
let renderGameBoard (gameId: string) (result: MoveResult) (userRole: PlayerRole) (gameCount: int) =
    // ... existing rendering ...

    // Button logic
    let hasActivity =
        match result with
        | XTurn(state, _) -> state.Board |> Map.exists (fun _ v -> v.IsSome)
        | _ -> true

    let isAssignedPlayer =
        match userRole with
        | PlayerX | PlayerO -> true
        | _ -> false

    let canReset = hasActivity && isAssignedPlayer
    let canDelete = gameCount > 6 && isAssignedPlayer

    // Render buttons
    div(class' = "controls") {
        button(class' = "reset-game-btn", type' = "button")
            .attr("data-on:click", sprintf "@post('/games/%s/reset')" gameId)
            .attr(if not canReset then "disabled" else "", "disabled") {
            "Reset Game"
        }
        button(class' = "delete-game-btn", type' = "button")
            .attr("data-on:click", sprintf "@delete('/games/%s')" gameId)
            .attr(if not canDelete then "disabled" else "", "disabled") {
            "Delete Game"
        }
    }
```

## Testing Checklist

### Reset Tests
- [ ] Reset creates new game in same position
- [ ] Reset clears player assignments
- [ ] Reset broadcasts to all clients
- [ ] Reset disabled for spectators
- [ ] Reset disabled for unplayed games
- [ ] Reset returns 403 for unauthorized user

### Initial Games Tests
- [ ] 6 games visible on page load
- [ ] All 6 games have empty boards
- [ ] All 6 games show "X's turn"

### Delete Tests
- [ ] Delete button disabled when 6 games
- [ ] Delete button enabled when >6 games
- [ ] Delete returns 409 when would go below 6
- [ ] Delete removes game from page
- [ ] Delete updates all clients' button states

## Common Pitfalls

1. **Forgetting to subscribe new game**: After `supervisor.CreateGame()`, must call `subscribeToGame` or SSE updates won't work.

2. **Wrong selector for PatchElements**: Reset should replace by old game ID, not new game ID (element doesn't exist yet).

3. **Race condition on count check**: Use MailboxProcessor serialization; don't cache game count.

4. **Button attribute syntax**: In Oxpecker, conditional attributes need proper syntax for disabled state.

## Verification Steps

1. Start server: `dotnet run --project src/TicTacToe.Web/`
2. Open browser to http://localhost:5228
3. Verify 6 game boards appear
4. Make a move in one game
5. Click Reset - should clear board
6. Click New Game - should add 7th game
7. Delete buttons should now be enabled
8. Delete one game - should go back to 6
9. Delete buttons should be disabled again
