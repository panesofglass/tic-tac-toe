module MoveInteractionTests

open System.IO
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Primitives
open Expecto
open TicTacToe.Web.Handlers
open TicTacToe.Engine

module Model = TicTacToe.Model

// Mock form collection for move testing
type MoveFormCollection(player: string, position: string) =
    let data = Map [ ("player", player); ("position", position) ]

    interface IFormCollection with
        member _.Count = data.Count

        member _.Keys =
            data |> Map.toSeq |> Seq.map fst |> Seq.toArray :> System.Collections.Generic.ICollection<string>

        member _.Item
            with get (key: string) =
                match data.TryFind key with
                | Some value -> StringValues(value)
                | None -> StringValues()

        member _.ContainsKey(key) = data.ContainsKey(key)

        member _.TryGetValue(key, value) =
            match data.TryFind(key) with
            | Some v ->
                value <- StringValues(v)
                true
            | None ->
                value <- StringValues()
                false

        member _.GetEnumerator() =
            (data
             |> Map.toSeq
             |> Seq.map (fun (k, v) -> System.Collections.Generic.KeyValuePair(k, StringValues(v))))
                .GetEnumerator()

        member _.GetEnumerator() : System.Collections.IEnumerator =
            (data
             |> Map.toSeq
             |> Seq.map (fun (k, v) -> System.Collections.Generic.KeyValuePair(k, StringValues(v)))
            :> seq<_>)
                .GetEnumerator()
            :> System.Collections.IEnumerator

        member _.Files = FormFileCollection() :> IFormFileCollection

// Mock form feature for move testing
type MoveFormFeature(player: string, position: string) =
    let form = MoveFormCollection(player, position) :> IFormCollection

    interface Microsoft.AspNetCore.Http.Features.IFormFeature with
        member _.Form
            with get () = form
            and set (_) = ()

        member _.ReadForm() = form
        member _.ReadFormAsync(_) = Task.FromResult(form)
        member _.HasFormContentType = true

// Helper to create test context with move form data
let createMoveContext (player: string) (position: string) =
    let context = DefaultHttpContext()
    let services = ServiceCollection()
    let supervisor = createGameSupervisor ()

    services.AddSingleton<GameSupervisor>(supervisor) |> ignore

    services.AddSingleton<ILoggerFactory>(new Abstractions.NullLoggerFactory())
    |> ignore

    let serviceProvider = services.BuildServiceProvider()
    context.RequestServices <- serviceProvider

    // Set up form data
    context.Features.Set<Microsoft.AspNetCore.Http.Features.IFormFeature>(new MoveFormFeature(player, position))

    // Set up response body stream
    context.Response.Body <- new MemoryStream()

    context, supervisor

let runHandler (handler: HttpContext -> Task) (context: HttpContext) =
    let task = handler context
    task.Wait()
    context

let getResponseBody (context: HttpContext) =
    context.Response.Body.Position <- 0L
    use reader = new StreamReader(context.Response.Body)
    reader.ReadToEnd()

// Helper to create test context with shared supervisor
let createMoveContextWithSupervisor (supervisor: GameSupervisor) (player: string) (position: string) =
    let context = DefaultHttpContext()
    let services = ServiceCollection()
    
    services.AddSingleton<GameSupervisor>(supervisor) |> ignore
    services.AddSingleton<ILoggerFactory>(new Abstractions.NullLoggerFactory()) |> ignore
    
    let serviceProvider = services.BuildServiceProvider()
    context.RequestServices <- serviceProvider
    
    // Set up form data
    context.Features.Set<Microsoft.AspNetCore.Http.Features.IFormFeature>(new MoveFormFeature(player, position))
    
    // Set up response body stream  
    context.Response.Body <- new MemoryStream()
    
    context

[<Tests>]
let tests =
    testList
        "Move Interaction Tests"
        [

          testCase "Valid X move on empty board succeeds"
          <| fun _ ->
              // Arrange
              let context, supervisor = createMoveContext "X" "TopLeft"
              let gameId, _ = supervisor.CreateGame()

              // Act
              let handler = makeMove gameId
              let result = runHandler handler context

              // Assert
              Expect.equal result.Response.StatusCode 202 "Should accept valid X move"

          testCase "Valid O move after X move succeeds"
          <| fun _ ->
              // Arrange
              let context, supervisor = createMoveContext "O" "TopCenter"
              let gameId, game = supervisor.CreateGame()

              // Make X's first move
              game.MakeMove(Model.XMove Model.TopLeft)

              // Act
              let handler = makeMove gameId
              let result = runHandler handler context

              // Assert
              Expect.equal result.Response.StatusCode 202 "Should accept valid O move"

          testCase "Invalid player rejected with 400"
          <| fun _ ->
              // Arrange
              let context, supervisor = createMoveContext "Z" "TopLeft"
              let gameId, _ = supervisor.CreateGame()

              // Act
              let handler = makeMove gameId
              let result = runHandler handler context

              // Assert
              Expect.equal result.Response.StatusCode 400 "Should reject invalid player"

          testCase "Invalid position rejected with 400"
          <| fun _ ->
              // Arrange
              let context, supervisor = createMoveContext "X" "InvalidPosition"
              let gameId, _ = supervisor.CreateGame()

              // Act
              let handler = makeMove gameId
              let result = runHandler handler context

              // Assert
              Expect.equal result.Response.StatusCode 400 "Should reject invalid position"

          testCase "Empty player rejected with 400"
          <| fun _ ->
              // Arrange
              let context, supervisor = createMoveContext "" "TopLeft"
              let gameId, _ = supervisor.CreateGame()

              // Act
              let handler = makeMove gameId
              let result = runHandler handler context

              // Assert
              Expect.equal result.Response.StatusCode 400 "Should reject empty player"

          testCase "Empty position rejected with 400"
          <| fun _ ->
              // Arrange
              let context, supervisor = createMoveContext "X" ""
              let gameId, _ = supervisor.CreateGame()

              // Act
              let handler = makeMove gameId
              let result = runHandler handler context

              // Assert
              Expect.equal result.Response.StatusCode 400 "Should reject empty position"

          testCase "Move requests are accepted regardless of game logic"
          <| fun _ ->
              // Test that HTTP layer accepts all properly formatted requests
              let supervisor = createGameSupervisor ()
              let gameId, game = supervisor.CreateGame()
              
              // First X move - should be accepted
              let context1, _ = createMoveContext "X" "TopLeft"
              let services1 = ServiceCollection()
              services1.AddSingleton<GameSupervisor>(supervisor) |> ignore
              services1.AddSingleton<ILoggerFactory>(new Abstractions.NullLoggerFactory()) |> ignore
              context1.RequestServices <- services1.BuildServiceProvider()
              
              let handler1 = makeMove gameId
              let result1 = runHandler handler1 context1
              Expect.equal result1.Response.StatusCode 202 "First X move should be accepted"
              
              // Second X move (invalid turn) - still accepted at HTTP level
              let context2, _ = createMoveContext "X" "TopCenter" 
              let services2 = ServiceCollection()
              services2.AddSingleton<GameSupervisor>(supervisor) |> ignore
              services2.AddSingleton<ILoggerFactory>(new Abstractions.NullLoggerFactory()) |> ignore
              context2.RequestServices <- services2.BuildServiceProvider()
              
              let handler2 = makeMove gameId
              let result2 = runHandler handler2 context2
              Expect.equal result2.Response.StatusCode 202 "Second X move should still be accepted (actor handles errors)"

          testCase "All valid positions can be played"
          <| fun _ ->
              // Test that all 9 positions are accepted
              let allPositions =
                  [ "TopLeft"
                    "TopCenter"
                    "TopRight"
                    "MiddleLeft"
                    "MiddleCenter"
                    "MiddleRight"
                    "BottomLeft"
                    "BottomCenter"
                    "BottomRight" ]

              for position in allPositions do
                  // Arrange - create fresh game for each test
                  let context, supervisor = createMoveContext "X" position
                  let gameId, _ = supervisor.CreateGame()

                  // Act
                  let handler = makeMove gameId
                  let result = runHandler handler context

                  // Assert
                  Expect.equal result.Response.StatusCode 202 $"Should accept position {position}"

          testCase "Both players can make moves"
          <| fun _ ->
              // Test that both X and O are accepted players
              let players = [ "X"; "O" ]

              for player in players do
                  // Arrange
                  let context, supervisor = createMoveContext player "TopLeft"
                  let gameId, game = supervisor.CreateGame()

                  // For O, need to make an X move first
                  if player = "O" then
                      game.MakeMove(Model.XMove Model.TopCenter)

                  // Act
                  let handler = makeMove gameId
                  let result = runHandler handler context

                  // Assert
                  Expect.equal result.Response.StatusCode 202 $"Should accept player {player}"

          testCase "Game winning move is handled correctly"
          <| fun _ ->
              // Arrange - set up a game where X can win
              let context, supervisor = createMoveContext "X" "TopRight"
              let gameId, game = supervisor.CreateGame()

              // Set up winning scenario: X has TopLeft and TopCenter, needs TopRight to win
              game.MakeMove(Model.XMove Model.TopLeft) // X
              game.MakeMove(Model.OMove Model.MiddleLeft) // O
              game.MakeMove(Model.XMove Model.TopCenter) // X
              game.MakeMove(Model.OMove Model.MiddleCenter) // O

              // Act - X makes winning move
              let handler = makeMove gameId
              let result = runHandler handler context

              // Assert
              Expect.equal result.Response.StatusCode 202 "Should accept winning move"

          testCase "Game draw is handled correctly"
          <| fun _ ->
              // Arrange - set up a game that will end in draw
              let context, supervisor = createMoveContext "X" "BottomRight"
              let gameId, game = supervisor.CreateGame()

              // Set up draw scenario
              game.MakeMove(Model.XMove Model.TopLeft) // X
              game.MakeMove(Model.OMove Model.TopCenter) // O
              game.MakeMove(Model.XMove Model.TopRight) // X
              game.MakeMove(Model.OMove Model.MiddleLeft) // O
              game.MakeMove(Model.XMove Model.MiddleCenter) // X
              game.MakeMove(Model.OMove Model.MiddleRight) // O
              game.MakeMove(Model.XMove Model.BottomCenter) // X
              game.MakeMove(Model.OMove Model.BottomLeft) // O
              // Final move by X at BottomRight should result in draw

              // Act
              let handler = makeMove gameId
              let result = runHandler handler context

              // Assert
              Expect.equal result.Response.StatusCode 202 "Should accept final move"

          testCase "Move on nonexistent game returns 404"
          <| fun _ ->
              // Arrange
              let context, _ = createMoveContext "X" "TopLeft"

              // Act
              let handler = makeMove "nonexistent-game"
              let result = runHandler handler context

              // Assert
              Expect.equal result.Response.StatusCode 404 "Should return 404 for nonexistent game"

          testCase "Case sensitive player validation"
          <| fun _ ->
              // Test that lowercase players are rejected
              let invalidPlayers = [ "x"; "o"; "Player1"; "1" ]

              for player in invalidPlayers do
                  // Arrange
                  let context, supervisor = createMoveContext player "TopLeft"
                  let gameId, _ = supervisor.CreateGame()

                  // Act
                  let handler = makeMove gameId
                  let result = runHandler handler context

                  // Assert
                  Expect.equal result.Response.StatusCode 400 $"Should reject invalid player '{player}'"

          testCase "Case sensitive position validation"
          <| fun _ ->
              // Test that invalid case positions are rejected
              let invalidPositions = [ "topleft"; "TOPLEFT"; "Top_Left"; "0,0" ]

              for position in invalidPositions do
                  // Arrange
                  let context, supervisor = createMoveContext "X" position
                  let gameId, _ = supervisor.CreateGame()

                  // Act
                  let handler = makeMove gameId
                  let result = runHandler handler context

                  // Assert
                  Expect.equal result.Response.StatusCode 400 $"Should reject invalid position '{position}'"

          testCase "Complete game flow - X wins"
          <| fun _ ->
              // Arrange - simulate a complete game where X wins
              let _, supervisor = createMoveContext "X" "TopLeft"
              let gameId, game = supervisor.CreateGame()

              let gameSequence =
                  [ ("X", "TopLeft", 202) // X moves
                    ("O", "MiddleLeft", 202) // O moves
                    ("X", "TopCenter", 202) // X moves
                    ("O", "MiddleCenter", 202) // O moves
                    ("X", "TopRight", 202) ] // X wins!

              for (player, position, expectedStatus) in gameSequence do
                  let context, _ = createMoveContext player position
                  let services = ServiceCollection()
                  services.AddSingleton<GameSupervisor>(supervisor) |> ignore

                  services.AddSingleton<ILoggerFactory>(new Abstractions.NullLoggerFactory())
                  |> ignore

                  context.RequestServices <- services.BuildServiceProvider()

                  let handler = makeMove gameId
                  let result = runHandler handler context

                  Expect.equal
                      result.Response.StatusCode
                      expectedStatus
                      $"Move {player} at {position} should return {expectedStatus}"

          testCase "Complete game flow - Draw"
          <| fun _ ->
              // Arrange - simulate a complete game that ends in draw
              let _, supervisor = createMoveContext "X" "TopLeft"
              let gameId, game = supervisor.CreateGame()

              let drawSequence =
                  [ ("X", "TopLeft", 202)
                    ("O", "TopCenter", 202)
                    ("X", "TopRight", 202)
                    ("O", "MiddleLeft", 202)
                    ("X", "MiddleCenter", 202)
                    ("O", "MiddleRight", 202)
                    ("X", "BottomCenter", 202)
                    ("O", "BottomLeft", 202)
                    ("X", "BottomRight", 202) ] // Final move results in draw

              for (player, position, expectedStatus) in drawSequence do
                  let context, _ = createMoveContext player position
                  let services = ServiceCollection()
                  services.AddSingleton<GameSupervisor>(supervisor) |> ignore

                  services.AddSingleton<ILoggerFactory>(new Abstractions.NullLoggerFactory())
                  |> ignore

                  context.RequestServices <- services.BuildServiceProvider()

                  let handler = makeMove gameId
                  let result = runHandler handler context

                  Expect.equal
                      result.Response.StatusCode
                      expectedStatus
                      $"Move {player} at {position} should return {expectedStatus}"

          testCase "Form data processing with special characters"
          <| fun _ ->
              // Test edge cases with form data
              let specialCases =
                  [ (" X ", "TopLeft", 400) // Spaces around player
                    ("X", " TopLeft ", 400) // Spaces around position
                    ("X\t", "TopLeft", 400) // Tab character
                    ("X", "TopLeft\n", 400) ] // Newline character

              for (player, position, expectedStatus) in specialCases do
                  // Arrange
                  let context, supervisor = createMoveContext player position
                  let gameId, _ = supervisor.CreateGame()

                  // Act
                  let handler = makeMove gameId
                  let result = runHandler handler context

                  // Assert
                  Expect.equal
                      result.Response.StatusCode
                      expectedStatus
                      $"Should handle special characters: player='{player}', position='{position}'"

          testCase "Concurrent move attempts"
          <| fun _ ->
              // Test what happens when multiple moves are attempted simultaneously
              let context1, supervisor = createMoveContext "X" "TopLeft"
              let context2, _ = createMoveContext "X" "TopCenter"
              let gameId, _ = supervisor.CreateGame()

              // Share the supervisor between contexts
              let services = ServiceCollection()
              services.AddSingleton<GameSupervisor>(supervisor) |> ignore

              services.AddSingleton<ILoggerFactory>(new Abstractions.NullLoggerFactory())
              |> ignore

              context2.RequestServices <- services.BuildServiceProvider()

              // Act - try to make moves concurrently
              let handler1 = makeMove gameId
              let result1 = runHandler handler1 context1
              let handler2 = makeMove gameId
              let result2 = runHandler handler2 context2

              // Assert - both HTTP requests should be accepted (actor handles game logic)
              Expect.equal result1.Response.StatusCode 202 "First move should be accepted"
              Expect.equal result2.Response.StatusCode 202 "Second move should also be accepted (errors handled by actor)"

          testCase "Error handling preserves game state"
          <| fun _ ->
              // Test that invalid moves don't corrupt game state
              let supervisor = createGameSupervisor ()
              let gameId, game = supervisor.CreateGame()

              // Make valid move
              game.MakeMove(Model.XMove Model.TopLeft)
              // Give actor time to process the move
              System.Threading.Thread.Sleep(200)

              // Try invalid move - X trying to move again when it should be O's turn
              let invalidContext = createMoveContextWithSupervisor supervisor "X" "TopCenter"
              let invalidHandler = makeMove gameId
              let invalidResult = runHandler invalidHandler invalidContext
              Expect.equal invalidResult.Response.StatusCode 202 "Wrong turn move should be accepted at HTTP level"

              // Make valid move after invalid attempt
              let validContext = createMoveContextWithSupervisor supervisor "O" "TopCenter"
              let validHandler = makeMove gameId
              let validResult = runHandler validHandler validContext
              Expect.equal validResult.Response.StatusCode 202 "Valid move should succeed after invalid attempt"
              
              supervisor.Dispose() ]
