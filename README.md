# Hypermedia Tic-Tac-Toe Game

A tic-tac-toe game implementation using the datastar hypermedia framework, demonstrating how to build interactive web applications with minimal JavaScript through declarative data binding and server-side rendering.

## Core Dependencies

- **RazorSlices 0.9.1**: Component-based server-side rendering
- **StarFederation.Datastar 1.0.0-beta.4**: .NET SDK for hypermedia apps
- **datastar.js 1.0.0-beta.10**: Declarative data binding

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- A modern web browser
- Git

### Setup and Run
`ash
git clone [repository-url]
cd tic-tac-toe
dotnet restore
dotnet run --project src/TicTacToe.Web
`

Access the game at http://localhost:5000

## Documentation

This project's documentation is organized into three main sections:

### 1. Project Documentation
- [Project Overview](docs/project/overview.md)
- [Requirements](docs/project/requirements.md)
- [Architecture](docs/project/architecture.md)

### 2. Development Guide
- [Setup Guide](docs/dev/setup.md)
- [Development Guidelines](docs/dev/guidelines.md)
- [Progress Tracker](docs/dev/progress.md)

### 3. AI Agent Documentation
- [Project Context](docs/agent/context.md)
- [Task Status](docs/agent/tasks.md)
- [Decision Log](docs/agent/decisions.md)

## Project Structure

`
TicTacToe/
├── docs/                    # Project documentation
├── src/
│   ├── TicTacToe.Engine/   # Core game logic
│   └── TicTacToe.Web/      # Web application
└── test/
    └── TicTacToe.Tests/    # Test suite
`

## Development Status

Current development status and next steps can be found in the [Progress Tracker](docs/dev/progress.md).

This project demonstrates how datastar's hypermedia approach combined with server-sent events can create interactive, real-time web applications with minimal client-side JavaScript.
