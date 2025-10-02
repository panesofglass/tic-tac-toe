module GameBoardTests

open System
open System.Collections.Generic
open Microsoft.AspNetCore.Http
open Expecto
open Oxpecker.ViewEngine
open TicTacToe.Web.templates.game
open TicTacToe.Model

// Helper to create a game state with specific moves
let createGameStateWith (moves: (SquarePosition * Player) list) =
    let mutable gameState = Map.empty<SquarePosition, SquareState>

    for (pos, player) in moves do
        gameState <- gameState.Add(pos, Taken player)

    // Create a ReadOnlyDictionary from the map
    let dict = Dictionary<SquarePosition, SquareState>()

    for kvp in gameState do
        dict.Add(kvp.Key, kvp.Value)

    dict :> IReadOnlyDictionary<SquarePosition, SquareState>

// Helper to create MoveResults for testing
let createXTurnResult gameState validMoves =
    XTurn(gameState, validMoves |> Array.map XPos)

let createOTurnResult gameState validMoves =
    OTurn(gameState, validMoves |> Array.map OPos)

let createWonResult gameState winner = Won(gameState, winner)
let createDrawResult gameState = Draw(gameState)
let createErrorResult gameState message = Error(gameState, message)

let renderGameBoardToString gameId result =
    let element = renderGameBoard gameId result
    Render.toString element

[<Tests>]
let tests =
    testList
        "Game Board Rendering Tests"
        [

          testCase "Empty board renders correctly for X turn"
          <| fun _ ->
              // Arrange
              let gameId = "test-game-1"
              let emptyState = createGameStateWith []

              let allPositions =
                  [| TopLeft
                     TopCenter
                     TopRight
                     MiddleLeft
                     MiddleCenter
                     MiddleRight
                     BottomLeft
                     BottomCenter
                     BottomRight |]

              let result = createXTurnResult emptyState allPositions

              // Act
              let html = renderGameBoardToString gameId result

              // Assert
              Expect.stringContains html "X&#39;s turn" "Should show X's turn message"
              Expect.stringContains html "class=\"status\"" "Should contain status div"
              Expect.stringContains html "class=\"board\"" "Should contain board div"
              Expect.stringContains html "class=\"controls\"" "Should contain controls div"
              Expect.stringContains html "square-clickable" "Should have clickable squares for X"
              Expect.stringContains html "data-player=\"X\"" "Should set player to X for moves"

              // Verify all positions are clickable
              for pos in allPositions do
                  let posStr = pos.ToString()

                  Expect.stringContains
                      html
                      $"data-position=\"{posStr}\""
                      $"Should have clickable square for position {posStr}"

          testCase "Empty board renders correctly for O turn"
          <| fun _ ->
              // Arrange
              let gameId = "test-game-2"
              let emptyState = createGameStateWith []

              let allPositions =
                  [| TopLeft
                     TopCenter
                     TopRight
                     MiddleLeft
                     MiddleCenter
                     MiddleRight
                     BottomLeft
                     BottomCenter
                     BottomRight |]

              let result = createOTurnResult emptyState allPositions

              // Act
              let html = renderGameBoardToString gameId result

              // Assert
              Expect.stringContains html "O&#39;s turn" "Should show O's turn message"
              Expect.stringContains html "data-player=\"O\"" "Should set player to O for moves"
              Expect.stringContains html "square-clickable" "Should have clickable squares for O"

          testCase "Partially filled board renders correctly"
          <| fun _ ->
              // Arrange
              let gameId = "test-game-3"

              let gameState =
                  createGameStateWith [ (TopLeft, X); (TopCenter, O); (MiddleCenter, X) ]

              let remainingMoves =
                  [| TopRight; MiddleLeft; MiddleRight; BottomLeft; BottomCenter; BottomRight |]

              let result = createOTurnResult gameState remainingMoves

              // Act
              let html = renderGameBoardToString gameId result

              // Assert
              Expect.stringContains html "O&#39;s turn" "Should show O's turn"

              // Check that taken squares show the correct player
              Expect.stringContains html "<span class=\"player\">X</span>" "Should show X in taken squares"
              Expect.stringContains html "<span class=\"player\">O</span>" "Should show O in taken squares"

              // Check that remaining moves are clickable
              Expect.stringContains html "square-clickable" "Should have clickable squares for remaining moves"

          testCase "Won game renders correctly with X winner"
          <| fun _ ->
              // Arrange
              let gameId = "test-game-4"

              let gameState =
                  createGameStateWith
                      [ (TopLeft, X)
                        (TopCenter, X)
                        (TopRight, X)
                        (MiddleLeft, O)
                        (MiddleCenter, O) ]

              let result = createWonResult gameState X

              // Act
              let html = renderGameBoardToString gameId result

              // Assert
              Expect.stringContains html "X wins!" "Should show X win message"
              Expect.stringContains html "New Game" "Should show new game button"
              Expect.stringContains html "new-game-btn" "Should have new game button class"
              Expect.stringContains html "@post(&#39;/games&#39;)" "Should have create new game action"

              // Check that no squares are clickable (using inverted assertion)
              Expect.isFalse (html.Contains("square-clickable")) "Should not have any clickable squares"

          testCase "Draw game renders correctly"
          <| fun _ ->
              // Arrange
              let gameId = "test-game-6"

              let gameState =
                  createGameStateWith
                      [ (TopLeft, X)
                        (TopCenter, O)
                        (TopRight, X)
                        (MiddleLeft, O)
                        (MiddleCenter, X)
                        (MiddleRight, O)
                        (BottomLeft, O)
                        (BottomCenter, X)
                        (BottomRight, O) ]

              let result = createDrawResult gameState

              // Act
              let html = renderGameBoardToString gameId result

              // Assert
              Expect.stringContains html "It&#39;s a draw!" "Should show draw message"
              Expect.stringContains html "New Game" "Should show new game button"
              Expect.isFalse (html.Contains("square-clickable")) "Should not have any clickable squares"

          testCase "Error state renders correctly"
          <| fun _ ->
              // Arrange
              let gameId = "test-game-7"
              let gameState = createGameStateWith [ (TopLeft, X) ]
              let errorMessage = "Invalid move attempted"
              let result = createErrorResult gameState errorMessage

              // Act
              let html = renderGameBoardToString gameId result

              // Assert
              Expect.stringContains html $"Error: {errorMessage}" "Should show error message"
              Expect.isFalse (html.Contains("square-clickable")) "Should not have any clickable squares"

          testCase "Game board contains proper CSS classes and structure"
          <| fun _ ->
              // Arrange
              let gameId = "test-game-8"
              let emptyState = createGameStateWith []
              let allPositions = [| TopLeft; TopCenter; TopRight |]
              let result = createXTurnResult emptyState allPositions

              // Act
              let html = renderGameBoardToString gameId result

              // Assert
              let expectedClasses =
                  [ "status"; "board"; "square"; "square-clickable"; "preview"; "controls" ]

              for cssClass in expectedClasses do
                  Expect.stringContains html cssClass $"Should contain CSS class: {cssClass}"

          testCase "Game board renders all 9 squares"
          <| fun _ ->
              // Arrange
              let gameId = "test-game-9"
              let emptyState = createGameStateWith []

              let allPositions =
                  [| TopLeft
                     TopCenter
                     TopRight
                     MiddleLeft
                     MiddleCenter
                     MiddleRight
                     BottomLeft
                     BottomCenter
                     BottomRight |]

              let result = createXTurnResult emptyState allPositions

              // Act
              let html = renderGameBoardToString gameId result

              // Assert
              // Count squares by looking for square class occurrences
              let squareMatches =
                  System.Text.RegularExpressions.Regex.Matches(html, "class=\"[^\"]*square[^\"]*\"")

              Expect.equal squareMatches.Count 9 "Should render exactly 9 squares"

          testCase "Clickable squares have correct data attributes"
          <| fun _ ->
              // Arrange
              let gameId = "test-game-10"
              let gameState = createGameStateWith [ (TopLeft, X) ]
              let remainingMoves = [| TopCenter; TopRight |]
              let result = createOTurnResult gameState remainingMoves

              // Act
              let html = renderGameBoardToString gameId result

              // Assert
              Expect.stringContains html $"@post(&#39;/games/{gameId}/moves&#39;)" "Should have correct POST endpoint"
              Expect.stringContains html "data-player=\"O\"" "Should set player data attribute"
              Expect.stringContains html "data-position=\"TopCenter\"" "Should set position data attribute"
              Expect.stringContains html "data-position=\"TopRight\"" "Should set position data attribute"

          testCase "Game page template renders with SSE connection"
          <| fun _ ->
              // Arrange
              let gameId = "test-game-12"
              let context = DefaultHttpContext()

              // Act
              let element = gamePage gameId context
              let html = Render.toString element

              // Assert
              Expect.stringContains html "game-container" "Should contain game container"
              Expect.stringContains html "Tic Tac Toe" "Should contain title"
              Expect.stringContains html "game-board-container" "Should contain board container"
              Expect.stringContains html $"data-sse-connect=\"/games/{gameId}/events\"" "Should have SSE connection"
              Expect.stringContains html "data-sse-on-patch=\"@patch\"" "Should have patch handler"
              Expect.stringContains html "Loading game..." "Should show loading state"
              Expect.stringContains html $"Game ID: {gameId}" "Should show game ID"

          testCase "Game page sets correct title in context"
          <| fun _ ->
              // Arrange
              let gameId = "test-game-13"
              let context = DefaultHttpContext()

              // Act
              let _ = gamePage gameId context

              // Assert
              let title = context.Items["Title"] :?> string
              Expect.equal title "Tic Tac Toe" "Should set correct page title"

          testCase "Game board handles edge case with no valid moves"
          <| fun _ ->
              // Arrange
              let gameId = "test-game-14"
              let gameState = createGameStateWith [ (TopLeft, X); (TopCenter, O) ]
              let noMoves = [||] // No valid moves available
              let result = createXTurnResult gameState noMoves

              // Act
              let html = renderGameBoardToString gameId result

              // Assert
              Expect.stringContains html "X&#39;s turn" "Should still show turn indicator"
              Expect.isFalse (html.Contains("square-clickable")) "Should not have clickable squares when no valid moves"

          // ENHANCED TESTS
          testCase "Board renders all winning scenarios for X"
          <| fun _ ->
              // Test all possible winning combinations for X
              let winningCombinations =
                  [
                    // Rows
                    [ (TopLeft, X)
                      (TopCenter, X)
                      (TopRight, X)
                      (MiddleLeft, O)
                      (MiddleCenter, O) ]
                    [ (MiddleLeft, X)
                      (MiddleCenter, X)
                      (MiddleRight, X)
                      (TopLeft, O)
                      (TopCenter, O) ]
                    [ (BottomLeft, X)
                      (BottomCenter, X)
                      (BottomRight, X)
                      (TopLeft, O)
                      (TopCenter, O) ]
                    // Columns
                    [ (TopLeft, X)
                      (MiddleLeft, X)
                      (BottomLeft, X)
                      (TopCenter, O)
                      (MiddleCenter, O) ]
                    [ (TopCenter, X)
                      (MiddleCenter, X)
                      (BottomCenter, X)
                      (TopLeft, O)
                      (MiddleLeft, O) ]
                    [ (TopRight, X)
                      (MiddleRight, X)
                      (BottomRight, X)
                      (TopLeft, O)
                      (MiddleLeft, O) ]
                    // Diagonals
                    [ (TopLeft, X)
                      (MiddleCenter, X)
                      (BottomRight, X)
                      (TopCenter, O)
                      (MiddleLeft, O) ]
                    [ (TopRight, X)
                      (MiddleCenter, X)
                      (BottomLeft, X)
                      (TopCenter, O)
                      (MiddleLeft, O) ] ]

              for (i, combination) in winningCombinations |> List.indexed do
                  let gameId = $"win-test-{i}"
                  let gameState = createGameStateWith combination
                  let result = createWonResult gameState X
                  let html = renderGameBoardToString gameId result

                  Expect.stringContains html "X wins!" $"Should show X wins for combination {i}"
                  Expect.stringContains html "New Game" $"Should show new game button for combination {i}"

                  Expect.isFalse
                      (html.Contains("square-clickable"))
                      $"Should have no clickable squares for combination {i}"

          testCase "Board shows correct turn alternation"
          <| fun _ ->
              // Test game progression with turn alternation
              let gameProgression =
                  [ ([], X) // Empty board, X's turn
                    ([ (TopLeft, X) ], O) // After X's move, O's turn
                    ([ (TopLeft, X); (TopCenter, O) ], X) // After O's move, X's turn
                    ([ (TopLeft, X); (TopCenter, O); (MiddleLeft, X) ], O) ] // After X's move, O's turn

              for (i, (moves, expectedPlayer)) in gameProgression |> List.indexed do
                  let gameId = $"turn-test-{i}"
                  let gameState = createGameStateWith moves

                  let validMoves =
                      [| TopRight; MiddleCenter; MiddleRight; BottomLeft; BottomCenter; BottomRight |]
                      |> Array.filter (fun pos -> moves |> List.exists (fun (p, _) -> p = pos) |> not)

                  let result =
                      match expectedPlayer with
                      | X -> createXTurnResult gameState validMoves
                      | O -> createOTurnResult gameState validMoves

                  let html = renderGameBoardToString gameId result
                  let expectedMessage = $"{expectedPlayer}&#39;s turn"

                  Expect.stringContains html expectedMessage $"Should show {expectedPlayer}'s turn for move {i}"

                  Expect.stringContains
                      html
                      $"data-player=\"{expectedPlayer}\""
                      $"Should set data-player to {expectedPlayer}"

          testCase "Board renders complex game states correctly"
          <| fun _ ->
              // Test complex mid-game scenarios
              let complexScenarios =
                  [
                    // Near-win scenario for X
                    ([ (TopLeft, X); (TopCenter, X); (MiddleLeft, O); (MiddleCenter, O) ], "X needs TopRight to win")
                    // Blocking scenario
                    ([ (TopLeft, X); (TopCenter, X); (TopRight, O); (MiddleLeft, O) ], "O blocked X's win")
                    // Fork scenario
                    ([ (MiddleCenter, X); (TopLeft, O); (BottomRight, X); (TopRight, O) ], "X creates fork opportunity") ]

              for (i, (moves, description)) in complexScenarios |> List.indexed do
                  let gameId = $"complex-test-{i}"
                  let gameState = createGameStateWith moves

                  let remainingMoves =
                      [| TopLeft
                         TopCenter
                         TopRight
                         MiddleLeft
                         MiddleCenter
                         MiddleRight
                         BottomLeft
                         BottomCenter
                         BottomRight |]
                      |> Array.filter (fun pos -> moves |> List.exists (fun (p, _) -> p = pos) |> not)

                  // Determine whose turn it is based on move count
                  let nextPlayer = if moves.Length % 2 = 0 then X else O

                  let result =
                      match nextPlayer with
                      | X -> createXTurnResult gameState remainingMoves
                      | O -> createOTurnResult gameState remainingMoves

                  let html = renderGameBoardToString gameId result

                  // Verify all pieces are shown correctly
                  for (pos, player) in moves do
                      let playerHtml = $"<span class=\"player\">{player}</span>"
                      Expect.stringContains html playerHtml $"Should show {player} at {pos} in {description}"

                  // Verify clickable squares for remaining moves
                  for pos in remainingMoves do
                      let positionAttr = $"data-position=\"{pos}\""
                      Expect.stringContains html positionAttr $"Should have clickable square at {pos} in {description}"

          testCase "Board handles all draw scenarios"
          <| fun _ ->
              // Test various draw scenarios
              let drawScenarios =
                  [
                    // Classic draw pattern
                    [ (TopLeft, X)
                      (TopCenter, O)
                      (TopRight, X)
                      (MiddleLeft, O)
                      (MiddleCenter, X)
                      (MiddleRight, O)
                      (BottomLeft, O)
                      (BottomCenter, X)
                      (BottomRight, O) ]
                    // Alternative draw pattern
                    [ (TopLeft, O)
                      (TopCenter, X)
                      (TopRight, O)
                      (MiddleLeft, X)
                      (MiddleCenter, O)
                      (MiddleRight, X)
                      (BottomLeft, X)
                      (BottomCenter, O)
                      (BottomRight, X) ] ]

              for (i, moves) in drawScenarios |> List.indexed do
                  let gameId = $"draw-test-{i}"
                  let gameState = createGameStateWith moves
                  let result = createDrawResult gameState
                  let html = renderGameBoardToString gameId result

                  Expect.stringContains html "It&#39;s a draw!" $"Should show draw message for scenario {i}"
                  Expect.stringContains html "New Game" $"Should show new game button for draw {i}"
                  Expect.isFalse (html.Contains("square-clickable")) $"Should have no clickable squares for draw {i}"

                  // Verify all 9 squares are filled
                  let xCount =
                      html
                      |> fun h ->
                          System.Text.RegularExpressions.Regex.Matches(h, "<span class=\"player\">X</span>").Count

                  let oCount =
                      html
                      |> fun h ->
                          System.Text.RegularExpressions.Regex.Matches(h, "<span class=\"player\">O</span>").Count

                  Expect.equal (xCount + oCount) 9 $"All squares should be filled in draw scenario {i}"

          testCase "Board shows correct hover states and interactions"
          <| fun _ ->
              // Test preview functionality for valid moves
              let gameId = "hover-test"
              let gameState = createGameStateWith [ (TopLeft, X); (MiddleCenter, O) ]

              let validMoves =
                  [| TopCenter
                     TopRight
                     MiddleLeft
                     MiddleRight
                     BottomLeft
                     BottomCenter
                     BottomRight |]

              let result = createXTurnResult gameState validMoves
              let html = renderGameBoardToString gameId result

              // Check that valid moves have preview spans
              Expect.stringContains html "<span class=\"preview\">X</span>" "Should show X preview for valid moves"

              // Check that taken squares don't have preview
              Expect.stringContains html "<span class=\"player\">X</span>" "Should show actual X piece"
              Expect.stringContains html "<span class=\"player\">O</span>" "Should show actual O piece"

              // Check that empty non-clickable squares show dots
              // (This would be the case if no valid moves available)
              let noMovesResult = createXTurnResult gameState [||]
              let noMovesHtml = renderGameBoardToString gameId noMovesResult

              Expect.stringContains
                  noMovesHtml
                  "<span class=\"empty\">&#183;</span>"
                  "Should show dots for empty non-clickable squares"

          testCase "Board handles error states with detailed messages"
          <| fun _ ->
              let errorMessages =
                  [ "Invalid move"
                    "Square already taken"
                    "Not your turn"
                    "Game has ended"
                    "Invalid player" ]

              for (i, message) in errorMessages |> List.indexed do
                  let gameId = $"error-test-{i}"
                  let gameState = createGameStateWith [ (TopLeft, X) ]
                  let result = createErrorResult gameState message
                  let html = renderGameBoardToString gameId result

                  Expect.stringContains html $"Error: {message}" $"Should show error message: {message}"

                  Expect.isFalse
                      (html.Contains("square-clickable"))
                      $"Should have no clickable squares for error: {message}"

          testCase "Board maintains proper CSS grid structure"
          <| fun _ ->
              let gameId = "grid-test"

              let gameState =
                  createGameStateWith [ (TopLeft, X); (MiddleCenter, O); (BottomRight, X) ]

              // Some valid moves (clickable) and some empty non-clickable squares
              let validMoves = [| TopCenter; MiddleLeft |]

              let result = createOTurnResult gameState validMoves
              let html = renderGameBoardToString gameId result

              // Verify grid structure
              Expect.stringContains html "class=\"board\"" "Should have board container"

              // Count total squares (should be 9)
              let squareCount =
                  System.Text.RegularExpressions.Regex.Matches(html, "class=\"[^\"]*square[^\"]*\"").Count

              Expect.equal squareCount 9 "Should have exactly 9 squares in grid"

              // Verify CSS classes for styling
              let expectedClasses =
                  [ "status"
                    "board"
                    "controls"
                    "square"
                    "square-clickable"
                    "preview"
                    "player"
                    "empty" ]

              for cssClass in expectedClasses do
                  Expect.stringContains html cssClass $"Should contain CSS class: {cssClass}" ]
