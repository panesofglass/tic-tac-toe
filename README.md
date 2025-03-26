# Hypermedia Tic-Tac-Toe Game

## Project Overview

This project implements a classic Tic-Tac-Toe game using the data-star hypermedia framework. The goal is to demonstrate how interactive web applications can be built with minimal JavaScript by leveraging data-star's declarative data binding approach and server-rendering techniques.

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

1. Use data-star.js (version 1.0.0-beta.10) as the primary framework
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
- **Interactive Game Board**: Empty cells rendered with data-star click handlers, taken cells rendered as static text
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

The application follows a hypermedia-driven, server-rendered approach using data-star's declarative binding system and server-sent events for real-time updates:

1. **State Management**:

   - Authoritative game state is stored and managed on the server
     - Board state (9 cells)
     - Current player
     - Game status
   - Temporary client-side state is managed using data-star's reactive data system

2. **Interaction Flow**:

   - Server renders the initial game HTML via RazorSlices
   - data-\* attributes connect the UI to the application state
   - User interactions trigger POST requests to the server via data-star directives
   - Server processes the request, updates game state, and checks game conditions
   - Server sends HTML updates to all connected clients via server-sent events
   - UI automatically updates to reflect state changes using the hypermedia fragments received

3. **Key data-star Features Used**:

   - `data-state`: For maintaining client-side game state
   - `data-bind`: For updating the UI based on state changes
   - `data-on`: For handling player clicks and sending moves to the server
   - `data-if`: For conditional rendering (e.g., showing/hiding elements)
   - `data-each`: For rendering the game board cells
   - `data-connect`: For establishing SSE connections to the server

4. **Progressive Enhancement**:

   - Empty board spaces are rendered with data-star `data-on-click="@post('/game/:id')"` attributes
   - Taken board spaces are rendered as static text (X or O) without click handlers
   - This approach enforces move validation at the HTML structure level
   - No client-side prevention of clicks needed - invalid moves aren't possible by design
   - Ensures the game remains functional even with minimal JavaScript support
   - Simplifies the implementation by using declarative click-to-post behavior

5. **Server-Sent Events (SSE)**:

   - Real-time HTML updates pushed from server to connected clients
   - Updates include new board state, current player, and game status as HTML fragments
   - Clients automatically update UI based on received events using data-star
   - fetch-event-source (included in data-star) facilitates the connection to the event stream
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

### Endpoints

The game uses a simpler, more traditional server-rendered approach with these main endpoints:

| Endpoint    | Method | Description                                                                           |
| ----------- | ------ | ------------------------------------------------------------------------------------- |
| `/`         | GET    | Renders the landing page with a list of active games and options to create a new game |
| `/game/:id` | GET    | Renders the game page and establishes SSE connection for updates                      |
| `/game/:id` | POST   | Submit a move or game action, returns updated HTML fragments                          |

This simplified approach eliminates the need for separate API endpoints, as the server directly returns HTML fragments that data-star can use to update the page. The progressive enhancement approach ensures that only valid moves are possible by rendering click handlers with `data-on-click="@post"` only for empty cells.

### File Structure

```
tic-tac-toe/
│
├── public/             # Static assets
│   ├── styles.css      # Game styling
│   └── assets/         # Images, fonts, etc.
│
├── views/              # Server-rendered templates
│   ├── layout.html     # Main page layout
│   ├── index.html      # Landing page template listing all games
│   ├── game.html       # Game board template
│   └── components/     # Reusable HTML components
│       ├── board.html  # Game board component
│       ├── status.html # Game status component
│       └── gameList.html # List of active games component
│
├── server.js           # Main server entry point
├── game.js             # Game logic and state management
├── routes.js           # Route handlers
├── utils.js            # Utility functions
│
├── package.json        # Node.js dependencies and scripts
├── .env                # Environment variables (gitignored)
└── README.md           # Project documentation
```

## Setup Instructions

### Prerequisites

- A modern web browser (Chrome, Firefox, Safari, Edge)
- Node.js (v16 or higher) and npm for the backend
- Git for version control

### Installation

1. Clone the repository or download the project files:

   ```
   git clone [repository-url]
   ```

2. Install dependencies:

   ```
   cd tic-tac-toe
   npm install
   ```

3. Configure environment variables:
   - Copy `.env.example` to `.env`
   - Adjust settings as needed (port, logging, etc.)

### Running the Game

1. Start the server:

   ```
   npm start
   ```

   This will start the Node.js server, typically on port 3000 (configurable in .env).

2. Access the game in your browser:

   ```
   http://localhost:3000/
   ```

   This will show the landing page where you can see active games or create a new one.

#### Development Mode

1. For development with auto-reload:

   ```
   npm run dev
   ```

   This uses nodemon to automatically restart the server when changes are detected.

### Development

1. Edit template files in the `views/` directory to change the structure or data-star attributes
2. Modify the `public/styles.css` file to adjust the visual appearance
3. For game logic changes, update `game.js`
4. For route handling changes, update `routes.js`
5. The server will automatically restart if you're using development mode

### Testing

Run the test suite to ensure everything is working correctly:

```
npm test
```

This will run both backend unit tests and integration tests for the API endpoints.

---

This project demonstrates how data-star's hypermedia approach combined with server-sent events can be used to create interactive, real-time web applications. The application follows a traditional server-rendered approach where HTML fragments are sent to the client and data-star uses its declarative data bindings to update the UI. By utilizing fetch-event-source for SSE connections, we create a responsive multiplayer experience with minimal client-side JavaScript and no separate API endpoints.
