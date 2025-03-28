# Development Setup

## Prerequisites

- .NET 8.0 SDK
- A modern web browser (Chrome, Firefox, Safari, Edge)
- Git for version control

## Initial Setup

1. Clone the repository:
   `ash
   git clone [repository-url]
   cd tic-tac-toe
   `

2. Restore dependencies:
   `ash
   dotnet restore
   `

3. Build the solution:
   `ash
   dotnet build
   `

## Project Structure

`
TicTacToe/
├── TicTacToe.sln
├── src/
│   ├── TicTacToe.Engine/    # Core game logic
│   │   ├── Game.cs
│   │   ├── GameBoard.cs
│   │   └── Move.cs
│   └── TicTacToe.Web/       # Web application
│       ├── Endpoints/        # HTTP endpoints
│       ├── Infrastructure/   # Services and repos
│       ├── Models/          # View models
│       └── Slices/          # Razor components
└── test/
    └── TicTacToe.Tests/     # Test suite
`

## Development Workflow

1. Running the application:
   `ash
   dotnet run --project src/TicTacToe.Web
   `

2. Running tests:
   `ash
   dotnet test
   `

3. Adding new features:
   - Create tests first
   - Implement the feature
   - Ensure all tests pass
   - Submit pull request
