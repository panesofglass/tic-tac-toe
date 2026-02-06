# tic-tac-toe Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-02-02

## Active Technologies
- In-memory via MailboxProcessor (GameSupervisor pattern) (006-game-reset)
- In-memory via MailboxProcessor (existing GameSupervisor/PlayerAssignmentManager pattern) (007-player-identity-legend)
- F# targeting .NET 10.0 + Frank 6.5.0, Frank.Datastar 6.5.0, Oxpecker.ViewEngine 1.1.0 (008-user-affordances)
- In-memory via MailboxProcessor (GameSupervisor, PlayerAssignmentManager) (008-user-affordances)

- F# targeting .NET 10.0 + Frank 6.4.0, Frank.Datastar 6.4.0, Oxpecker.ViewEngine 1.1.0, System.Reactive 6.0.2 (002-multi-game-rest-api)
- In-memory via MailboxProcessor (existing GameSupervisor pattern) (002-multi-game-rest-api)
- F# targeting .NET 10.0 + GitHub Actions, `actions/checkout`, `actions/setup-dotnet`, `actions/upload-artifact` (004-github-actions-ci)

- F# targeting .NET 10.0 (per existing project) + Frank 6.4.0, Frank.Datastar 6.4.0, Oxpecker.ViewEngine 1.1.0 (001-web-frontend-single-game)

## Project Structure

```text
src/
tests/
```

## Commands

# Add commands for F# targeting .NET 10.0 (per existing project)

## Code Style

F# targeting .NET 10.0 (per existing project): Follow standard conventions

## Recent Changes
- 008-user-affordances: Added F# targeting .NET 10.0 + Frank 6.5.0, Frank.Datastar 6.5.0, Oxpecker.ViewEngine 1.1.0
- 007-player-identity-legend: Added F# targeting .NET 10.0 + Frank 6.4.0, Frank.Datastar 6.4.0, Oxpecker.ViewEngine 1.1.0
- 006-game-reset: Added F# targeting .NET 10.0 + Frank 6.4.0, Frank.Datastar 6.4.0, Oxpecker.ViewEngine 1.1.0


<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
