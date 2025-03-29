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
   - Maintain consistent authentication UI components
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
   - Use ASP.NET Core Identity for authentication
   - Render game state based on user identity and claims

3. User Authentication
   - Implement ASP.NET Core Identity for all user management
   - Use claims-based authorization for game actions
   - Follow secure password handling practices
   - Implement proper token-based authentication flows
   - Keep authentication logic in dedicated services

4. Entity Framework Core
   - Follow Code-First approach for all database models
   - Use migrations for schema changes
   - Keep DbContext classes focused and separated by domain
   - Implement repository pattern for data access
   - Use async/await for all database operations
   - Follow proper connection management practices

5. Player Management
   - Link players to ASP.NET Core Identity users
   - Use claims for player-specific permissions
   - Track player-to-marker assignments per game
   - Validate moves against player identity
   - Keep player management separate from game logic
   - Store extended player data in related tables
## Testing

1. Test Coverage
   - Unit tests for game logic
   - Integration tests for web features
   - Authentication flow tests
   - Database integration tests
   - Test both success and failure cases
   - Test player interactions and turn order

2. Identity Testing
   - Mock UserManager and SignInManager in tests
   - Test registration and login flows
   - Verify claim transformations
   - Test authorization requirements
   - Use in-memory database for Identity tests

3. Database Testing
   - Use in-memory provider for unit tests
   - Implement test database for integration tests
   - Test migrations in isolation
   - Verify data access patterns
   - Test concurrent operations

4. Test Organization
   - Mirror source structure in tests
   - Use descriptive test names
   - Follow Arrange-Act-Assert pattern
   - Group authentication-specific test scenarios
   - Separate integration and unit tests
## Documentation

1. Code Documentation
   - XML comments on public APIs
   - README.md for project overview
   - Keep docs/ up to date
   - Document authentication flows
   - Include database schema details
   - Document migration procedures

2. Security Documentation
   - Document authentication configuration
   - Include authorization policies
   - Describe claim requirements
   - Document database access patterns
   - Include security best practices

3. Commit Messages
   - Use present tense
   - Be descriptive
   - Reference issues when applicable
   - Tag authentication-related changes
   - Note database schema changes
