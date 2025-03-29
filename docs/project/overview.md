# Project Overview

This project implements a classic Tic-Tac-Toe game using the datastar hypermedia framework. The goal is to demonstrate how interactive web applications can be built with minimal JavaScript by leveraging datastar's declarative data binding approach and server-rendering techniques.

## Core Design Principles

1. Hypermedia-Driven Architecture
   - Application state and interactions managed through HTML attributes
   - Server-rendered HTML with progressive enhancement
   - Real-time updates via server-sent events
   - Player-specific rendering based on user identity and claims

2. Minimal JavaScript
   - Uses datastar.js for declarative data binding
   - No custom JavaScript required
   - Progressive enhancement approach
   - Real-time game state updates

3. Clean Architecture
   - Separation of game logic (Engine) from web presentation
   - Immutable game state management
   - Event-sourced move history
   - Claims-based user identity
   - Player-aware game state

4. Player Management
   - ASP.NET Core Identity for user authentication
   - Secure claims-based authorization
   - Player-to-marker assignments per game
   - Turn-based interaction model
   - Real-time opponent move updates

## Technology Stack

- .NET 8 with MinimalAPI and RazorSlices
- Entity Framework Core for data persistence
- ASP.NET Core Identity for user authentication and management
- datastar for hypermedia-driven interactions
- Server-sent events (SSE) for real-time updates

## Authentication and Player Management

The application uses ASP.NET Core Identity for user authentication, providing secure account management and login capabilities. While users authenticate through Identity, their in-game roles (such as X or O markers) are managed separately by the game logic. This separation allows:

- Secure user authentication and management through proven Identity framework
- Flexible player role assignment within games
- Clear distinction between user accounts and game-specific player states
- Future extensibility for player statistics and achievements
