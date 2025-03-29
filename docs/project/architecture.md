# Architecture

## Technology Decisions

1. Backend Framework: .NET 8
   - MinimalAPI for lightweight HTTP endpoints
   - RazorSlices (0.9.1) for component-based rendering
   - StarFederation.Datastar (1.0.0-beta.4) for .NET SDK
   - ASP.NET Core Identity for authentication
   - Entity Framework Core for data persistence

2. Frontend
   - datastar.js (1.0.0-beta.10) for declarative data binding
   - No custom JavaScript requirement
   - HTML-driven interactions
   - Consistent authentication components

## Component Architecture

1. TicTacToe.Engine
   - Core game logic and models
   - Immutable state management
   - Move validation and win detection

2. TicTacToe.Web
   - HTTP endpoints
   - HTML rendering
   - Game state persistence
   - Real-time updates
   - User authentication and authorization
   - Database integration

3. TicTacToe.Infrastructure
   - Entity Framework Core contexts
   - Identity configuration
   - Data repositories
   - Migration management

4. TicTacToe.Tests
   - Unit tests for game logic
   - Integration tests for web functionality
   - Authentication flow tests
   - Database integration tests

## Data Flow

1. User Authentication
   - Identity-based user management
   - Claims-based authorization
   - Secure credential handling
   - User-to-player mapping

2. User Interaction
   - HTML with data-* attributes triggers actions
   - POST requests for moves
   - SSE for real-time updates
   - Authentication state management

3. State Management
   - Immutable game state
   - Event-sourced move history
   - Server-side validation
   - Persistent user data
   - Game statistics tracking

## API Design

Routes follow a traditional server-rendered pattern:

1. Authentication
   - GET /auth/login - Login page
   - POST /auth/login - Process login
   - GET /auth/register - Registration page
   - POST /auth/register - Process registration
   - POST /auth/logout - Logout

2. Full Pages
   - GET / - Game listing
   - GET /game/{id} - Game view
   - GET /profile - User profile

3. HTML Fragments
   - GET /game/{id}/fragment - Game state
   - POST /game/{id} - Make move

4. Real-time Updates
   - SSE connection for live game state
   - HTML fragment updates

## Data Persistence

1. Identity Data
   - User accounts and profiles
   - Authentication tokens
   - User claims and roles

2. Game Data
   - Active games
   - Move history
   - Player statistics
   - Achievement tracking

# Architecture

## Technology Decisions

1. Backend Framework: .NET 8
   - MinimalAPI for lightweight HTTP endpoints
   - RazorSlices (0.9.1) for component-based rendering
   - StarFederation.Datastar (1.0.0-beta.4) for .NET SDK

2. Frontend
   - datastar.js (1.0.0-beta.10) for declarative data binding
   - No custom JavaScript requirement
   - HTML-driven interactions

## Component Architecture

1. TicTacToe.Engine
   - Core game logic and models
   - Immutable state management
   - Move validation and win detection

2. TicTacToe.Web
   - HTTP endpoints
   - HTML rendering
   - Game state persistence
   - Real-time updates

3. TicTacToe.Tests
   - Unit tests for game logic
   - Integration tests for web functionality

## Data Flow

1. User Interaction
   - HTML with data-* attributes triggers actions
   - POST requests for moves
   - SSE for real-time updates

2. State Management
   - Immutable game state
   - Event-sourced move history
   - Server-side validation

## API Design

Routes follow a traditional server-rendered pattern:

1. Full Pages
   - GET / - Game listing
   - GET /game/{id} - Game view

2. HTML Fragments
   - GET /game/{id}/fragment - Game state
   - POST /game/{id} - Make move

3. Real-time Updates
   - SSE connection for live game state
   - HTML fragment updates
