# tic-tac-toe Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-02-02

## Active Technologies
- F# targeting .NET 10.0 + Frank 6.4.0, Frank.Datastar 6.4.0, Oxpecker.ViewEngine 1.1.0
- In-memory via MailboxProcessor (GameSupervisor pattern with IObservable interface)

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
- 003-simplify-mailbox-processor: Removed System.Reactive 6.0.2 dependency, implemented IObservable directly in MailboxProcessor

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
