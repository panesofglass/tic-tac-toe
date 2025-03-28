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

2. Web Application
   - Follow MinimalAPI patterns
   - Use RazorSlices for components
   - Keep endpoints thin, logic in services

## Testing

1. Test Coverage
   - Unit tests for game logic
   - Integration tests for web features
   - Test both success and failure cases

2. Test Organization
   - Mirror source structure in tests
   - Use descriptive test names
   - Follow Arrange-Act-Assert pattern

## Documentation

1. Code Documentation
   - XML comments on public APIs
   - README.md for project overview
   - Keep docs/ up to date

2. Commit Messages
   - Use present tense
   - Be descriptive
   - Reference issues when applicable
