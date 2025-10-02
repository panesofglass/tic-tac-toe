module PageRenderingTests

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Expecto
open Oxpecker.ViewEngine
open TicTacToe.Web.templates
open TicTacToe.Engine

// Helper to create a mock HttpContext with services
let createMockContext (gameCount: int) =
    let context = DefaultHttpContext()
    let services = ServiceCollection()
    let supervisor = createGameSupervisor ()

    // Mock the game count
    for _ in 1..gameCount do
        supervisor.CreateGame() |> ignore

    services.AddSingleton<GameSupervisor>(supervisor) |> ignore

    services.AddSingleton<ILoggerFactory>(new Abstractions.NullLoggerFactory())
    |> ignore

    let serviceProvider = services.BuildServiceProvider()
    context.RequestServices <- serviceProvider
    context

// Helper to render a view function and extract the HTML string
let renderToString (viewFunction: HttpContext -> Fragment) (context: HttpContext) =
    let element = viewFunction context
    Render.toString element

[<Tests>]
let tests =
    testList
        "Page Rendering Tests"
        [ testCase "Home page renders with correct structure"
          <| fun _ ->
              // Arrange
              let context = createMockContext 3

              // Act
              let html = renderToString home.homePage context

              // Assert
              Expect.stringContains html "home-container" "Should contain home container class"
              Expect.stringContains html "Tic Tac Toe" "Should contain main title"
              Expect.stringContains html "home-title" "Should contain title class"
              Expect.stringContains html "Active Games" "Should show active games section"
              Expect.stringContains html "3" "Should display correct game count"
              Expect.stringContains html "game-count" "Should contain game count class"
              Expect.stringContains html "Create New Game" "Should have create game button"
              Expect.stringContains html "create-game-btn" "Should contain create game button class"
              Expect.stringContains html "View All Games" "Should have view games link"
              Expect.stringContains html "view-games-link" "Should contain view games link class"

          testCase "Home page sets correct title in context"
          <| fun _ ->
              // Arrange
              let context = createMockContext 0

              // Act
              let _ = home.homePage context

              // Assert
              let title = context.Items["Title"] :?> string
              Expect.equal title "Tic Tac Toe - Home" "Should set correct page title"

          testCase "Home page renders with zero games"
          <| fun _ ->
              // Arrange
              let context = createMockContext 0

              // Act
              let html = renderToString home.homePage context

              // Assert
              Expect.stringContains html "0" "Should show zero game count"
              Expect.stringContains html "Create New Game" "Should still show create game button"

          testCase "Games page renders with correct structure"
          <| fun _ ->
              // Arrange
              let context = createMockContext 2

              // Act
              let html = renderToString games.gamesPage context

              // Assert
              Expect.stringContains html "games-container" "Should contain games container class"
              Expect.stringContains html "All Games" "Should contain page title"
              Expect.stringContains html "games-title" "Should contain title class"
              Expect.stringContains html "Active Games:" "Should show active games label"
              Expect.stringContains html "2" "Should display correct game count"
              Expect.stringContains html "count-value" "Should contain count value class"
              Expect.stringContains html "Create New Game" "Should have create game button"
              Expect.stringContains html "create-new-btn" "Should contain create new button class"

          testCase "Games page sets correct title in context"
          <| fun _ ->
              // Arrange
              let context = createMockContext 0

              // Act
              let _ = games.gamesPage context

              // Assert
              let title = context.Items["Title"] :?> string
              Expect.equal title "All Games" "Should set correct page title"

          testCase "Games page renders no games state correctly"
          <| fun _ ->
              // Arrange
              let context = createMockContext 0

              // Act
              let html = renderToString games.gamesPage context

              // Assert
              Expect.stringContains html "0" "Should show zero game count"
              Expect.stringContains html "no-games" "Should contain no games class"
              Expect.stringContains html "No active games. Create one to get started!" "Should show no games message"

          testCase "Games page renders games state correctly"
          <| fun _ ->
              // Arrange
              let context = createMockContext 5

              // Act
              let html = renderToString games.gamesPage context

              // Assert
              Expect.stringContains html "5" "Should show correct game count"
              Expect.stringContains html "games-grid" "Should contain games grid class"
              Expect.stringContains html "Game grid view coming soon..." "Should show placeholder content"

          testCase "Home page contains proper CSS classes for styling"
          <| fun _ ->
              // Arrange
              let context = createMockContext 1

              // Act
              let html = renderToString home.homePage context

              // Assert
              let expectedClasses =
                  [ "home-container"
                    "home-title"
                    "home-content"
                    "stats"
                    "game-count"
                    "actions"
                    "create-game-btn"
                    "view-games-link"
                    "info"
                    "tech-note" ]

              for cssClass in expectedClasses do
                  Expect.stringContains html cssClass $"Should contain CSS class: {cssClass}"

          testCase "Games page contains proper CSS classes for styling"
          <| fun _ ->
              // Arrange
              let context = createMockContext 1

              // Act
              let html = renderToString games.gamesPage context

              // Assert
              let expectedClasses =
                  [ "games-container"
                    "games-title"
                    "games-header"
                    "games-count"
                    "count-label"
                    "count-value"
                    "create-new-btn"
                    "games-grid"
                    "game-grid-placeholder"
                    "games-actions"
                    "back-link" ]

              for cssClass in expectedClasses do
                  Expect.stringContains html cssClass $"Should contain CSS class: {cssClass}"

          testCase "Home page contains tech information"
          <| fun _ ->
              // Arrange
              let context = createMockContext 0

              // Act
              let html = renderToString home.homePage context

              // Assert
              Expect.stringContains html "Powered by F#, Datastar, and Server-Sent Events" "Should mention tech stack"

              Expect.stringContains
                  html
                  "Create a new game or view existing games to play."
                  "Should contain instructions" ]
