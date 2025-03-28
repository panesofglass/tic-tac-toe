# Development Guidelines

## Code Style

1. C# Conventions
   - Use C# 12 features where appropriate
   - Prefer immutable types (records) for models
   - Use nullable reference types
   - Follow Microsoft's C# coding conventions

2. HTML/CSS
   - Use semantic HTML5 elements
   - Follow progressive enhancement
   - Keep styling minimal and functional

## Architecture Guidelines

1. Game Logic
   - Keep TicTacToe.Engine pure and framework-independent
   - Use immutable state
   - Validate all moves server-side
   - Track player context with moves
   - Enforce turn order in game state

2. Web Application
   - Follow MinimalAPI patterns
   - Use RazorSlices for components
   - Keep endpoints thin, logic in services
   - Use session state for player identity
   - Render game state based on player context

3. Player Management
   - Simple session-based identity
   - Track player-to-marker assignments per game
   - Validate moves against player identity
   - Keep player management separate from game logic
   - Use middleware for session handling

## Testing

1. Test Coverage
   - Unit tests for game logic
   - Integration tests for web features
   - Test both success and failure cases
   - Test player interactions and turn order
   - Validate session management

2. Test Organization
   - Mirror source structure in tests
   - Use descriptive test names
   - Follow Arrange-Act-Assert pattern
   - Group player-specific test scenarios

## Documentation

1. Code Documentation
   - XML comments on public APIs
   - README.md for project overview
   - Keep docs/ up to date
   - Document player management flows
   - Include session handling details

2. Commit Messages
   - Use present tense
   - Be descriptive
   - Reference issues when applicable
