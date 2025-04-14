# AI Agent Context

## Project State

- Type: Web-based Tic-Tac-Toe game
- Architecture: Hypermedia-driven, server-rendered
- Current Phase: Initial development
- Status: Core models implemented, frontend in progress

## Technology Stack

1. Framework Versions:
   - RazorSlices 0.9.1
   - StarFederation.Datastar 1.0.0-beta.4
   - datastar.js 1.0.0-beta.10

2. Core Dependencies:
   - .NET 8.0
   - ASP.NET Core MinimalAPI
   - fetch-event-source (via datastar)

## Key Architectural Decisions

1. Game State:
   - Immutable records for state
   - Event-sourced move history
   - Server-side validation

2. Frontend:
   - Progressive enhancement
   - data-* attributes for interaction
   - No custom JavaScript

3. Real-time Updates:
   - Server-sent events
   - HTML fragment updates
   - Automatic UI updates via datastar

## Model Structure

1. Move.cs
   - Position (row/column as byte 0-2)
   - Marker (X/O)
   - Timestamp

2. GameBoard.cs
   - Immutable state record
   - Derived from move history

3. Game.cs
   - Aggregate root
   - Implements IGameStatus
   - Manages game state

## Current Development Context

1. Implemented:
   - Core game models
   - Initial test suite
   - Project structure

2. In Progress:
   - Frontend implementation
   - Game state management
   - Move handling

## Important Notes

- All game logic must be server-side
- Focus on progressive enhancement
- Maintain immutable state pattern
- Follow hypermedia-driven approach
