# Tic-Tac-Toe Web Application

A multi-player web-based Tic-Tac-Toe game built with F# and ASP.NET Core, featuring real-time updates and concurrent game support.

## Features

- **Multi-player gameplay** - Two players (X and O) play from separate browsers
- **Multiple concurrent games** - Create and manage multiple games on a single page
- **Real-time updates** - Server-sent events (SSE) broadcast game state to all connected players
- **Automatic authentication** - Cookie-based user identification without explicit sign-in
- **Direct game URLs** - Share game links for others to join

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- A modern web browser

### Running the Application

1. Clone the repository
2. Navigate to the project directory
3. Run the application:

```bash
dotnet run --project src/TicTacToe.Web/
```

4. Open your browser and navigate to `http://localhost:5228`

### Playing a Game

1. Visit the home page - you'll be automatically authenticated
2. Click **New Game** to create a game board
3. Share the game URL with another player (or open in a different browser)
4. Take turns - X plays first, then O
5. The first player to get three in a row wins!

## Architecture

### Technologies

- **F#** - Primary language
- **ASP.NET Core** - Web framework
- **Frank 6.5.0** - Routing and HTTP handling
- **Frank.Datastar** - Server-sent events integration
- **Oxpecker.ViewEngine** - HTML view rendering
- **Playwright** - End-to-end testing

### Key Components

- **GameSupervisor** - Manages multiple concurrent game instances
- **PlayerAssignmentManager** - Tracks which user plays X or O in each game
- **SSE Broadcast** - Real-time game state updates to all connected clients

## Authentication

The application uses automatic cookie-based authentication:

### Flow

1. Unauthenticated users visiting the home page are redirected to `/login`
2. The login endpoint creates a persistent cookie with a unique user ID
3. Users are redirected back to their original destination
4. Subsequent requests include the cookie for identification

### Multi-Player Validation

- First player to move in a game is assigned as X
- Second player (different user) is assigned as O
- Players can only move on their turn
- Same user cannot play both sides

### Claims

- `sub` - Unique user identifier (GUID)
- `created_at` - Account creation timestamp
- `last_visit` - Last activity timestamp

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/` | Home page (requires auth) |
| GET | `/login` | Authenticate and get cookie |
| GET | `/logout` | Clear authentication |
| GET | `/sse` | SSE stream for real-time updates |
| POST | `/games` | Create a new game |
| GET | `/games/{id}` | View a specific game |
| POST | `/games/{id}` | Make a move |
| DELETE | `/games/{id}` | Delete a game |

## Testing

### Run All Tests

```bash
dotnet test
```

### Run with Server

The Playwright tests require the server to be running:

```bash
# Terminal 1: Start the server
dotnet run --project src/TicTacToe.Web/

# Terminal 2: Run tests
TEST_BASE_URL="http://localhost:5228" dotnet test
```

### Test Categories

- **HomePageTests** - Basic page loading and navigation
- **GamePlayTests** - Game mechanics, turns, win conditions (uses two browser contexts for multi-player)
- **MultiGameTests** - Concurrent game management
- **RestApiTests** - API endpoint validation

## CI/CD

GitHub Actions runs on every push and pull request:
- Build validation
- All tests (unit + Playwright)

## License

This project is licensed under the MIT License.
