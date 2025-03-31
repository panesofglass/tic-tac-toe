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

4. Authentication
   - Cookie-based authentication using ASP.NET Core Identity
   - Authentication flow:
     * Forms-based login/register with password hashing
     * Claims-based identity storing NameIdentifier, Name, and Email
     * Automatic redirects to login for protected endpoints
     * Sign-out invalidates the authentication cookie
   
   - Cookie configuration:
     * Secure-only (HTTPS required)
     * HTTP-only (no JavaScript access)
     * 20-minute sliding expiration
     * Strict same-site policy
     * Anti-forgery token protection

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
