# AGENTS.md - AI Agent Guidelines for Tic-Tac-Toe Project

## Project Overview

**Name**: Tic-Tac-Toe  
**Type**: Hypermedia-driven web application using datastar framework  
**Architecture**: Clean Architecture with F# and ASP.NET Core  
**Primary Goals**:

- Demonstrate hypermedia-driven web application architecture
- Showcase minimal JavaScript approach with server-side rendering
- Implement real-time multiplayer gameplay using Server-Sent Events

## Key Stakeholders

- Development team
- Players of the game

## Technology Stack

### Core Frameworks

- **.NET 9.0** - Primary runtime and SDK
- **F#** - Primary programming language
- **ASP.NET Core** - Web framework
- **Frank.Builder** - Routing and HTTP handling
- **Oxpecker** - View rendering
- **StarFederation.Datastar 1.0.0-beta.4** - .NET SDK for hypermedia applications
- **datastar.js 1.0.0-beta.10** - Client-side declarative data binding

### Testing Frameworks

- **Expecto** - F# testing framework for unit and integration tests
- **ASP.NET Core Integration Testing** - For web layer testing

## Project Structure

```
TicTacToe/
├── src/
│   ├── TicTacToe.Engine/     # Core game logic (F#)
│   │   ├── Engine.fs         # Game state management
│   │   └── Engine.fsi        # Public API interface
│   └── TicTacToe.Web/        # Web application (F#)
│       ├── Auth.fs           # Authentication logic
│       ├── DatastarAttrs.fs  # Datastar attribute helpers
│       ├── DatastarHelpers.fs # Datastar utility functions
│       ├── Extensions.fs     # Extension methods
│       ├── Models.fs         # Data models
│       ├── Program.fs        # Application entry point
│       └── templates/        # View templates
└── test/
    ├── TicTacToe.Engine.Tests/ # Engine unit tests
    └── TicTacToe.Web.Tests/    # Web integration tests
```

## Architecture Components

### TicTacToe.Engine

**Purpose**: Core game logic with immutable state management
**Key Features**:

- Immutable game state using F# types
- Move validation and win detection
- Pattern matching for game states
- Functional programming principles

**Core Types**:

- `SquarePosition` - Board positions (TopLeft, TopCenter, etc.)
- `Player` - X or O players
- `SquareState` - Taken or Empty squares
- `MoveResult` - XTurn, OTurn, Won, Draw, Error states
- `GameState` - Immutable dictionary of board state

### TicTacToe.Web

**Purpose**: Web presentation layer with hypermedia controls
**Key Features**:

- HTTP endpoints using Frank.Builder
- Server-side HTML rendering with Oxpecker
- Real-time updates via Server-Sent Events
- Cookie-based user authentication
- Datastar integration for minimal JavaScript

## Development Guidelines

### F# Code Conventions

- Use **PascalCase** for types, modules, and public functions
- Use **camelCase** for parameters and local bindings
- Prefer **immutable types** and functional programming patterns
- Use **pattern matching** extensively for state handling
- Enable **nullable reference types** where applicable

### Authentication System

The application includes automatic user identification:

- **Cookie-based authentication** with persistent user IDs
- **Claims-based identity** through ASP.NET Core
- **Security measures**: HttpOnly cookies, SameSite policy, antiforgery tokens
- **User tracking**: Unique GUID, timestamps, IP address, user agent

**Claims Used**:

- `sub`: Unique user identifier (GUID)
- `created_at`: When the user was first created
- `last_visit`: When the user last visited
- `ip_address`: User's IP address (diagnostics)
- `user_agent`: Browser information (diagnostics)

### Datastar Integration Patterns

#### Hypermedia Controls

Use data-\* attributes for interactive elements:

```fsharp
// POST request with parameters
div [
    dataOnClick "@@post('/endpoint')"
    dataParams """{ "param": "value" }"""
] [ (* content *) ]

// Navigation
a [
    href "/path"
    dataOnClick "@@navigate"
] [ text "Link Text" ]

// Real-time updates via SSE
div [
    dataConnect "@@sse:/updates"
] [ (* content updated via SSE *) ]
```

#### State Management Principles

- Use **Server-Sent Events (SSE)** for real-time updates
- Render state-dependent UI conditionally on server
- Keep client-side state minimal
- Rely on server-side validation
- Handle state transitions through the Engine module

### Game Engine Implementation

#### State Transitions

Game states are managed through pattern matching:

```fsharp
match moveResult with
| Won (gameState, player) -> (* render winner *)
| Draw gameState -> (* render draw *)
| XTurn (gameState, validMoves) -> (* render X's turn *)
| OTurn (gameState, validMoves) -> (* render O's turn *)
| Error (gameState, message) -> (* handle error *)
```

#### Move Validation

Server-side validation includes:

- **Valid position**: Check if square is empty
- **Player's turn**: Ensure correct player is moving
- **Game state**: Verify game is still in progress
- **Concurrency**: Handle simultaneous moves

#### Win Detection

Implements 8 winning combinations:

- **Rows**: 3 horizontal lines
- **Columns**: 3 vertical lines
- **Diagonals**: 2 diagonal lines

### Testing Strategy

#### Test Organization

- **Mirror source structure** in test projects
- Use **descriptive test names** that explain behavior
- Follow **Arrange-Act-Assert** pattern
- Group related test scenarios together

#### Coverage Areas

- **Game logic**: All state transitions and move validations
- **Web features**: HTTP endpoints and authentication
- **Player interactions**: Multiplayer scenarios and edge cases
- **Error handling**: Invalid moves and concurrent access

#### Test Categories

- **Unit tests**: Engine logic and individual components
- **Integration tests**: Web layer and full request/response cycles
- **Success cases**: Normal gameplay flows
- **Failure cases**: Invalid moves and error conditions
- **Edge cases**: Draw conditions and boundary scenarios

### Code Modification Standards

#### Pull Request Requirements

- [ ] All tests must pass
- [ ] Code follows F# style guidelines
- [ ] Documentation is updated
- [ ] Security considerations addressed
- [ ] Datastar integration follows established patterns

#### Review Checklist

1. **Component Structure**

   - [ ] Proper F# module organization
   - [ ] Correct type definitions and interfaces
   - [ ] Immutable state management
   - [ ] Pattern matching implementation

2. **Web Integration**

   - [ ] Appropriate datastar attributes
   - [ ] Correct endpoint URLs and routing
   - [ ] Parameter formatting and validation
   - [ ] SSE configuration if needed

3. **Security & Authentication**

   - [ ] Cookie authentication implementation
   - [ ] Antiforgery token usage
   - [ ] Input validation and sanitization
   - [ ] Authorization checks where needed

4. **Code Quality**
   - [ ] Follows F# naming conventions
   - [ ] Maintains functional programming principles
   - [ ] Includes comprehensive error handling
   - [ ] Proper documentation and comments

### Version Control

#### Branching Strategy

- **Trunk-based development** with short-lived feature branches
- **Semantic versioning** for releases
- **Descriptive commit messages** in present tense

#### Commit Guidelines

- Use present tense ("Add feature" not "Added feature")
- Be descriptive about what changed and why
- Reference issues when applicable
- Keep commits focused and atomic

## Documentation Requirements

### Code Documentation

- **XML comments** on public APIs and modules
- **Inline comments** for complex F# logic
- **Example usage** in module documentation
- **Type annotations** where helpful for clarity

### Project Documentation

- **README.md**: Project overview, setup, dependencies
- **Architecture decisions**: Document significant design choices
- **API documentation**: Endpoint specifications
- **Security considerations**: Authentication and authorization details

## Common Patterns and Anti-Patterns

### ✅ Recommended Patterns

- Use immutable data structures throughout
- Leverage F# pattern matching for state handling
- Implement server-side validation before state changes
- Use datastar for progressive enhancement
- Keep business logic in the Engine module
- Handle errors explicitly with Result types

### ❌ Anti-Patterns to Avoid

- Mutating game state directly
- Client-side game logic or validation
- Blocking operations in web handlers
- Mixing business logic with web concerns
- Ignoring concurrent access scenarios
- Using JavaScript where server-side rendering suffices

## Getting Started for New Agents

1. **Environment Setup**

   - Ensure .NET 9.0 SDK is installed
   - Understand F# syntax and functional programming concepts
   - Familiarize yourself with datastar.js documentation

2. **Key Files to Review**

   - `src/TicTacToe.Engine/Engine.fs` - Core game logic
   - `src/TicTacToe.Web/Program.fs` - Web application setup
   - `test/TicTacToe.Engine.Tests/EngineTests.fs` - Test examples
   - `README.md` - Project overview and setup instructions

3. **Development Workflow**

   - Run tests: `dotnet test`
   - Start web app: `dotnet run --project src/TicTacToe.Web`
   - Access at: `https://localhost:5001`

4. **Key Concepts to Master**
   - F# discriminated unions and pattern matching
   - Immutable state management
   - Datastar hypermedia controls
   - Server-Sent Events for real-time updates
   - Cookie-based authentication flow

This project demonstrates how hypermedia-driven architecture combined with functional programming can create interactive, real-time web applications with minimal client-side JavaScript complexity.

