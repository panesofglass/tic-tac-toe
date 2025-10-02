module HandlerTests

open System.IO
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Expecto
open TicTacToe.Web.Handlers
open TicTacToe.Engine

// Simple helper to create test context
let createBasicContext (gameCount: int) =
    let context = DefaultHttpContext()
    let services = ServiceCollection()
    let supervisor = createGameSupervisor ()

    // Create specified number of games
    for _ in 1..gameCount do
        supervisor.CreateGame() |> ignore

    services.AddSingleton<GameSupervisor>(supervisor) |> ignore

    services.AddSingleton<ILoggerFactory>(new Abstractions.NullLoggerFactory())
    |> ignore

    let serviceProvider = services.BuildServiceProvider()
    context.RequestServices <- serviceProvider

    // Set up response body stream
    context.Response.Body <- new MemoryStream()

    context, supervisor

let runHandler (handler: HttpContext -> Task) (context: HttpContext) =
    let task = handler context
    task.Wait()
    context

[<Tests>]
let tests =
    testList
        "Handler Tests"
        [

          testCase "home handler returns 200 OK"
          <| fun _ ->
              // Arrange
              let context, _ = createBasicContext 0

              // Act
              let result = runHandler home context

              // Assert
              Expect.equal result.Response.StatusCode 200 "Should return 200 OK"

          testCase "home handler sets page title"
          <| fun _ ->
              // Arrange
              let context, _ = createBasicContext 0

              // Act
              let _ = runHandler home context

              // Assert
              let title = context.Items["Title"] :?> string
              Expect.equal title "Tic Tac Toe - Home" "Should set correct page title"

          testCase "games handler returns 200 OK"
          <| fun _ ->
              // Arrange
              let context, _ = createBasicContext 3

              // Act
              let result = runHandler games context

              // Assert
              Expect.equal result.Response.StatusCode 200 "Should return 200 OK"

          testCase "games handler sets page title"
          <| fun _ ->
              // Arrange
              let context, _ = createBasicContext 0

              // Act
              let _ = runHandler games context

              // Assert
              let title = context.Items["Title"] :?> string
              Expect.equal title "All Games" "Should set correct page title"

          testCase "createGame handler creates game and redirects"
          <| fun _ ->
              // Arrange
              let context, supervisor = createBasicContext 0
              let initialCount = supervisor.GetActiveGameCount()

              // Act
              let result = runHandler createGame context

              // Assert
              Expect.equal result.Response.StatusCode 302 "Should return redirect status"
              let location = string result.Response.Headers.Location
              Expect.stringContains location "/games/" "Should redirect to games URL"
              Expect.equal (supervisor.GetActiveGameCount()) (initialCount + 1) "Should create one new game"

          testCase "createGame generates unique game IDs"
          <| fun _ ->
              // Arrange
              let context1, _ = createBasicContext 0
              let context2, _ = createBasicContext 0

              // Act
              let result1 = runHandler createGame context1
              let result2 = runHandler createGame context2

              // Assert
              let location1 = string result1.Response.Headers.Location
              let location2 = string result2.Response.Headers.Location
              Expect.notEqual location1 location2 "Should generate unique game IDs"

          testCase "gamePage returns 404 for nonexistent game"
          <| fun _ ->
              // Arrange
              let context, _ = createBasicContext 0

              // Act
              let handler = gamePage "nonexistent-game"
              let result = runHandler handler context

              // Assert
              Expect.equal result.Response.StatusCode 404 "Should return 404 for nonexistent game"

          testCase "gamePage renders existing game"
          <| fun _ ->
              // Arrange
              let context, supervisor = createBasicContext 0
              let gameId, _ = supervisor.CreateGame()

              // Act
              let handler = gamePage gameId
              let result = runHandler handler context

              // Assert
              Expect.equal result.Response.StatusCode 200 "Should return 200 for existing game"
              let title = context.Items["Title"] :?> string
              Expect.equal title "Tic Tac Toe" "Should set correct page title"

          testCase "gameEvents returns 404 for nonexistent game"
          <| fun _ ->
              // Arrange
              let context, _ = createBasicContext 0

              // Act
              let handler = gameEvents "nonexistent-game"
              let result = runHandler handler context

              // Assert
              Expect.equal result.Response.StatusCode 404 "Should return 404 for nonexistent game"

          testCase "makeMove returns 404 for nonexistent game"
          <| fun _ ->
              // Arrange
              let context, _ = createBasicContext 0

              // Act
              let handler = makeMove "nonexistent-game"
              let result = runHandler handler context

              // Assert
              Expect.equal result.Response.StatusCode 404 "Should return 404 for nonexistent game"

          testCase "handlers preserve existing context items"
          <| fun _ ->
              // Arrange
              let context, _ = createBasicContext 2
              context.Items["ExistingKey"] <- "ExistingValue"

              // Act
              let _ = runHandler games context

              // Assert
              let existingValue = context.Items["ExistingKey"] :?> string
              let titleValue = context.Items["Title"] :?> string
              Expect.equal existingValue "ExistingValue" "Should preserve existing context items"
              Expect.equal titleValue "All Games" "Should set new title while preserving existing items"

          testCase "createGame increments active game count correctly"
          <| fun _ ->
              // Arrange
              let context, supervisor = createBasicContext 0

              // Act & Assert
              let initialCount = supervisor.GetActiveGameCount()
              Expect.equal initialCount 0 "Should start with zero games"

              let _ = runHandler createGame context
              Expect.equal (supervisor.GetActiveGameCount()) 1 "Should have 1 game after first creation"

          testCase "multiple createGame calls increment count correctly"
          <| fun _ ->
              // Arrange
              let context, supervisor = createBasicContext 5 // Start with 5 games
              let initialCount = supervisor.GetActiveGameCount()

              // Act
              let _ = runHandler createGame context

              // Assert
              Expect.equal (supervisor.GetActiveGameCount()) (initialCount + 1) "Should increment by 1"

          testCase "game page handler works with different game IDs"
          <| fun _ ->
              // Arrange
              let context, supervisor = createBasicContext 0
              let gameId1, _ = supervisor.CreateGame()
              let gameId2, _ = supervisor.CreateGame()

              // Act & Assert for first game
              let handler1 = gamePage gameId1
              let result1 = runHandler handler1 context
              Expect.equal result1.Response.StatusCode 200 "Should return 200 for first game"

              // Reset context for second test
              let context2, _ = createBasicContext 0
              context2.RequestServices <- context.RequestServices // Share the supervisor

              // Act & Assert for second game
              let handler2 = gamePage gameId2
              let result2 = runHandler handler2 context2
              Expect.equal result2.Response.StatusCode 200 "Should return 200 for second game" ]
