# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

This is a **hypermedia-driven Tic-Tac-Toe web application** built with F# and .NET 9.0, demonstrating how to create interactive, real-time web applications with minimal client-side JavaScript using the datastar framework.

**Key Architecture Principles:**
- **Clean Architecture** with functional programming in F#
- **Server-side rendering** with Oxpecker view engine
- **Hypermedia controls** via datastar.js for client interactions
- **Immutable state management** in the game engine
- **Real-time updates** through Server-Sent Events (SSE)

## Common Commands

### Development Commands
```bash
# Run the web application
dotnet run --project src/TicTacToe.Web

# Access the application
open https://localhost:5001
```

### Testing Commands
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test test/TicTacToe.Engine.Tests
dotnet test test/TicTacToe.Web.Tests

# Run with verbose output
dotnet test --verbosity normal

# Run a single test method (using filter)
dotnet test --filter "testName"
```

### Build Commands
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build src/TicTacToe.Engine
dotnet build src/TicTacToe.Web

# Clean and rebuild
dotnet clean && dotnet build
```

### Package Management
```bash
# Restore packages
dotnet restore

# Add package to a project
dotnet add src/TicTacToe.Web package PackageName

# Update packages
dotnet list package --outdated
```

## Architecture Overview

### Two-Layer Architecture

**TicTacToe.Engine** (Pure F# Business Logic)
- Contains all game rules and state management
- **Immutable types**: `GameState`, `MoveResult`, `Player`, `SquarePosition`
- **Pattern matching** for state transitions
- **Functional API**: `startGame : unit -> MoveResult` and `makeMove : MoveResult * Move -> MoveResult`
- Zero dependencies on web frameworks

**TicTacToe.Web** (ASP.NET Core Web Layer)
- HTTP endpoints using **Frank.Builder** routing
- **Oxpecker** for server-side HTML rendering
- **Datastar integration** for hypermedia controls
- **Cookie-based authentication** with claims transformation
- **Server-Sent Events** for real-time game updates

### Key Types and State Flow

The game engine uses discriminated unions for type-safe state management:

```fsharp
type MoveResult =
    | XTurn of GameState * ValidMovesForX
    | OTurn of GameState * ValidMovesForO  
    | Won of GameState * Player
    | Draw of GameState
    | Error of GameState * string
```

State transitions are handled through pattern matching, ensuring all possible game states are explicitly handled.

### Authentication System

The application uses **automatic user identification** without explicit sign-up:
- Persistent cookie-based identity with 30-day expiration
- Claims include: `sub` (user ID), `created_at`, `last_visit`, `ip_address`, `user_agent`
- Security: HttpOnly cookies, Strict SameSite policy, antiforgery protection

### Datastar Integration Patterns

Hypermedia controls use data-* attributes for interactions:

```fsharp
// POST requests with parameters
div [
    dataOnClick "@@post('/endpoint')"
    dataParams """{ "param": "value" }"""
] [ (* content *) ]

// Real-time updates via SSE
div [
    dataConnect "@@sse:/updates"
] [ (* content updated via SSE *) ]
```

## Testing Strategy

### Test Organization
- **Expecto** framework for F# testing
- **Mirror source structure** in test projects
- Helper functions for common test patterns
- Separate test categories: initialization, move mechanics, win conditions, error handling

### Key Test Patterns
```fsharp
// Apply sequence of moves for testing scenarios
let applyMoves (initialState: MoveResult) (moves: Move list) =
    moves |> List.fold (fun state move -> makeMove (state, move)) initialState

// Test win conditions
let expectWin player moves =
    let result = applyMoves (startGame()) moves
    Expect.isTrue (isWon result player) "Expected win condition"
```

## F# Development Guidelines

### Code Style
- **PascalCase** for types, modules, and public functions
- **camelCase** for parameters and local bindings  
- **Pattern matching** preferred over if/else chains
- **Immutable data structures** throughout
- **Explicit error handling** with Result types where appropriate

### Module Organization
- `Engine.fsi` defines public API surface
- `Engine.fs` contains implementation
- Web layer modules: `Auth.fs`, `Models.fs`, `DatastarHelpers.fs`, `Extensions.fs`
- Template modules under `templates/` directory

### Key Dependencies
- **Frank**: HTTP routing and handlers
- **Oxpecker**: View engine for server-side rendering
- **StarFederation.Datastar**: .NET SDK for datastar hypermedia
- **FSharp.SystemTextJson**: JSON serialization with F# support
- **Expecto**: F# testing framework

## Development Workflow

### File Compilation Order
F# requires specific compilation order (defined in .fsproj files):
1. Type definitions and interfaces first
2. Implementation modules
3. Web-specific modules (Auth, Models, Helpers)
4. Templates and views
5. Program.fs entry point last

### Local Development
1. Start with `dotnet run --project src/TicTacToe.Web`
2. Access at `https://localhost:5001`
3. Tests run automatically on build
4. Use browser dev tools to inspect datastar SSE connections

### Project Structure Navigation
```
├── src/
│   ├── TicTacToe.Engine/     # Pure F# game logic
│   │   ├── Engine.fsi        # Public API interface  
│   │   └── Engine.fs         # Implementation
│   └── TicTacToe.Web/        # ASP.NET Core web app
│       ├── Auth.fs           # Claims transformation
│       ├── Models.fs         # Web models/DTOs
│       ├── DatastarHelpers.fs # Datastar utilities
│       ├── templates/        # View templates
│       └── Program.fs        # Web app entry point
└── test/
    ├── TicTacToe.Engine.Tests/ # Engine unit tests
    └── TicTacToe.Web.Tests/    # Web integration tests
```

## Key Development Concepts

### Immutable State Management
All game state is immutable. State changes return new `MoveResult` instances rather than mutating existing state.

### Hypermedia-Driven Architecture  
The web interface uses datastar's declarative data binding to minimize client-side JavaScript. Server-side events drive UI updates.

### Type Safety
F#'s type system prevents invalid game states at compile time through discriminated unions and pattern matching exhaustiveness checking.

### Real-time Multiplayer
Server-Sent Events enable real-time game updates without WebSocket complexity, suitable for turn-based gameplay.