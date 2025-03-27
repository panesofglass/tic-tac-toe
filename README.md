# Hypermedia Tic-Tac-Toe Game

## Project Overview

This project implements a classic Tic-Tac-Toe game using the datastar hypermedia framework. The goal is to demonstrate how interactive web applications can be built with minimal JavaScript by leveraging datastar's declarative data binding approach and server-rendering techniques.

Tic-Tac-Toe is a simple two-player game where players take turns marking X or O on a 3x3 grid. The player who succeeds in placing three of their marks in a horizontal, vertical, or diagonal row wins the game.

This implementation follows hypermedia-driven architecture principles, where the application state and interactions are primarily managed through HTML attributes rather than imperative JavaScript code.

## Dependencies

The application relies on the following libraries and frameworks:

- **RazorSlices 0.9.1**: Component-based approach to server-side rendering without the full MVC pattern
- **StarFederation.Datastar 1.0.0-beta.4**: .NET SDK for implementing hypermedia applications
- **datastar.js 1.0.0-beta.10**: Client-side JavaScript library for declarative data binding

## Requirements

### Functional Requirements

1. Display a landing page listing all active games
2. Allow users to create new games from the landing page
3. Display a 3x3 game board when accessing a specific game
4. Allow two players to take turns (X and O)
5. Track and display the current player's turn
6. Register player moves when they click on empty cells
7. Check for win conditions after each move:
   - 3 in a row horizontally
   - 3 in a row vertically
   - 3 in a row diagonally
8. Detect and announce game results (win/draw)
9. Allow players to reset the game and start over
10. Allow players to return to the landing page from a game

### Technical Requirements

1. Use datastar.js (version 1.0.0-beta.10) as the primary framework
2. Implement the game using hypermedia principles with data-\* attributes
3. Minimize or eliminate custom JavaScript code
4. Ensure responsive design for different screen sizes
5. Ensure the game works across modern browsers
6. Maintain clean, semantic HTML structure
7. Document the code appropriately
8. Implement a web server to handle game state management
9. Provide server-rendered HTML and server-sent events (SSE) for updates
10. Handle concurrent game sessions
11. Validate moves and enforce game rules server-side
12. Check win conditions after each move
13. Maintain session persistence for ongoing games
14. Implement proper error handling and logging
15. Use fetch-event-source for client-side event stream consumption

## Game Features

- **Game Listing**: Landing page displaying all active games
- **Game Creation**: Ability to create new games from the landing page
- **Interactive Game Board**: Empty cells rendered with datastar click handlers, taken cells rendered as static text
- **Turn Indicator**: Visual display showing which player's turn it is (X or O)
- **Win Detection**: Automatic detection of win conditions
- **Draw Detection**: Identify when the game ends in a draw
- **Game Status Messages**: Display messages about game progress and results
- **Reset Functionality**: Button to restart the game at any point
- **Visual Feedback**: Highlight winning combinations
- **Responsive Design**: Work well on both desktop and mobile devices
- **Navigation**: Return to game listing from an active game

## Technical Implementation Details

### Technology Stack

The application uses the following technology stack:

1. **.NET 8**: Modern, cross-platform framework for building server-side applications

   - **Minimal API**: Lightweight, code-focused approach to building HTTP APIs in .NET
   - **RazorSlices**: Clean, component-based approach to server-side rendering without the full MVC pattern

2. **datastar**: Hypermedia framework for reactive web applications
   - Client-side reactivity through declarative data-\* attributes
   - Server-sent events (SSE) for real-time updates
   - Minimal JavaScript requirement with maximum interactivity

### Architecture

The application follows a hypermedia-driven, server-rendered approach using datastar's declarative binding system and server-sent events for real-time updates:

1. **State Management**:

   - Authoritative game state is stored and managed on the server
     - Board state (9 cells)
     - Current player
     - Game status
   - Temporary client-side state is managed using datastar's reactive data system

2. **Interaction Flow**:

   - Server renders the initial game HTML via RazorSlices
   - data-\* attributes connect the UI to the application state
   - User interactions trigger POST requests to the server via datastar directives
   - Server processes the request, updates game state, and checks game conditions
   - Server sends HTML updates to all connected clients via server-sent events
   - UI automatically updates to reflect state changes using the hypermedia fragments received

3. **Key datastar Features Used**:

   - `data-state`: For maintaining client-side game state
   - `data-bind`: For updating the UI based on state changes
   - `data-on`: For handling player clicks and sending moves to the server
   - `data-if`: For conditional rendering (e.g., showing/hiding elements)
   - `data-each`: For rendering the game board cells
   - `data-connect`: For establishing SSE connections to the server

4. **Progressive Enhancement**:

   - Empty board spaces are rendered with datastar `data-on-click="@post('/game/:id')"` attributes
   - Taken board spaces are rendered as static text (X or O) without click handlers
   - This approach enforces move validation at the HTML structure level
   - No client-side prevention of clicks needed - invalid moves aren't possible by design
   - Ensures the game remains functional even with minimal JavaScript support
   - Simplifies the implementation by using declarative click-to-post behavior

5. **Server-Sent Events (SSE)**:

   - Real-time HTML updates pushed from server to connected clients
   - Updates include new board state, current player, and game status as HTML fragments
   - Clients automatically update UI based on received events using datastar
   - fetch-event-source (included in datastar) facilitates the connection to the event stream
   - Ensures all players see the same game state

6. **Game Logic**:

   - Server-side win condition checking after each move
   - Turn switching between players managed by server
   - Game state management (in-progress, win, draw)
   - Move validation happens naturally through the HTML structure - only valid moves are possible
   - Example of board space HTML:

     ```html
     <!-- An empty space (clickable) -->
     <div
       data-on-click="@post('/game/@Model.Id', { position: 4 })"
       class="cell empty"
     >
       Play here
     </div>

     <!-- A taken space (static) -->
     <div class="cell">X</div>
     ```

   - This approach is simpler than forms while maintaining the same server-side validation security model

### API Design

The application uses a fragment-based HTML-over-the-wire approach with the following routes:

#### Full Page Routes

- `GET /` - Landing page showing list of active games and option to create new game
- `GET /:id` - Full game page for viewing and playing a specific game

#### HTML Fragment Routes

- `GET /page` - Returns HTML fragment for the games list and initiates SSE connection
- `GET /page/:id` - Returns HTML fragment for a specific game and initiates SSE connection
- `POST /page/:id` - Accepts a move for a specific game, triggers updates via SSE

The fragment routes serve two purposes:

1. Initial page rendering - The full page routes internally fetch fragments using datastar's `data-on-load` attribute
2. Dynamic updates - The SSE connections established by GET requests to fragment routes push live updates

#### Server-Sent Events

- No separate endpoint is needed for SSE
- The fragment routes (`/page` and `/page/:id`) establish SSE connections
- Events use `datastar-merge-fragment` to seamlessly update the UI
- POST requests trigger new events on the existing SSE connections

This approach creates a clean separation between full pages and fragments, while leveraging datastar's capabilities to manage connections and updates. The progressive enhancement approach ensures that only valid moves are possible by rendering click handlers with `data-on-click="@post"` only for empty cells.
This simplified approach eliminates the need for separate API endpoints, as the server directly returns HTML fragments that datastar can use to update the page. The progressive enhancement approach ensures that only valid moves are possible by rendering click handlers with `data-on-click="@post"` only for empty cells.

### Project Structure

```
TicTacToe/
├── TicTacToe.sln
├── README.md
├── conversation.md
├── src/
│   └── TicTacToe.Web/
│       ├── Models/
│       │   ├── Game.cs
│       │   ├── GameBoard.cs
│       │   └── Move.cs
│       ├── Infrastructure/
│       │   ├── IGameRepository.cs
│       │   └── InMemoryGameRepository.cs
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── TicTacToe.Web.csproj
└── test/
    └── TicTacToe.Tests/
        ├── Models/
        │   └── GameTests.cs
        ├── Infrastructure/
        │   └── InMemoryGameRepositoryTests.cs
        └── TicTacToe.Tests.csproj
```

The repository follows a clean architecture approach:

- `TicTacToe.Web/`: Contains the web application and core game logic
  - `Models/`: Domain models and game logic
  - `Infrastructure/`: Data access and external service integrations
- `TicTacToe.Tests/`: Contains all tests
  - Mirrors the structure of the main project for easy navigation
  - Each component has its corresponding test file

## Setup Instructions

### Prerequisites

- A modern web browser (Chrome, Firefox, Safari, Edge)
- dotnet 9.0 to build and run the server
- Git for version control

### Installation

1. Clone the repository or download the project files:

   ```
   git clone [repository-url]
   ```

2. Install dependencies:

   ```
   cd tic-tac-toe
   dotnet restore
   ```

3. Configure environment variables:
   - Copy `.env.example` to `.env`
   - Adjust settings as needed (port, logging, etc.)

### Running the Game

1. Start the server:

   ```
   dotnet run --project src/TicTacToe.Web
   ```

   This will start the Node.js server, typically on port 3000 (configurable in .env).

2. Access the game in your browser:

   ```
   http://localhost:3000/
   ```

   This will show the landing page where you can see active games or create a new one.

### Testing

Run the test suite to ensure everything is working correctly:

```
dotnet test
```

This will run both backend unit tests and integration tests for the API endpoints.

---

This project demonstrates how datastar's hypermedia approach combined with server-sent events can be used to create interactive, real-time web applications. The application follows a traditional server-rendered approach where HTML fragments are sent to the client and datastar uses its declarative data bindings to update the UI. By utilizing fetch-event-source for SSE connections, we create a responsive multiplayer experience with minimal client-side JavaScript and no separate API endpoints.
