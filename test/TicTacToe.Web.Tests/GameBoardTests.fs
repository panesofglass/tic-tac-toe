module GameBoardTests

open System
open System.Collections.Generic
open Expecto
open Oxpecker.ViewEngine
open TicTacToe.Web.templates.game
open TicTacToe.Model

// Helper to create a game state with specific moves
let createGameStateWith (moves: (SquarePosition * Player) list) =
    let mutable gameState = Map.empty<SquarePosition, SquareState>

    for (pos, player) in moves do
        gameState <- gameState.Add(pos, Taken player)

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

let testGameId = "test-game-123"

let renderGameBoardToString result =
    let element = renderGameBoard testGameId result
    Render.toString element

[<Tests>]
let tests =
    testList
        "Game Board Rendering Tests"
        [
          testCase "Empty board renders correctly for X turn"
          <| fun _ ->
              let emptyState = createGameStateWith []

              let allPositions =
                  [| TopLeft; TopCenter; TopRight
                     MiddleLeft; MiddleCenter; MiddleRight
                     BottomLeft; BottomCenter; BottomRight |]

              let result = createXTurnResult emptyState allPositions
              let html = renderGameBoardToString result

              Expect.stringContains html "X&#39;s turn" "Should show X's turn message"
              Expect.stringContains html "class=\"status\"" "Should contain status div"
              Expect.stringContains html "class=\"board\"" "Should contain board div"
              Expect.stringContains html "class=\"controls\"" "Should contain controls div"
              Expect.stringContains html "square-clickable" "Should have clickable squares for X"

          testCase "Empty board renders correctly for O turn"
          <| fun _ ->
              let emptyState = createGameStateWith []

              let allPositions =
                  [| TopLeft; TopCenter; TopRight
                     MiddleLeft; MiddleCenter; MiddleRight
                     BottomLeft; BottomCenter; BottomRight |]

              let result = createOTurnResult emptyState allPositions
              let html = renderGameBoardToString result

              Expect.stringContains html "O&#39;s turn" "Should show O's turn message"
              Expect.stringContains html "square-clickable" "Should have clickable squares for O"

          testCase "Partially filled board renders correctly"
          <| fun _ ->
              let gameState =
                  createGameStateWith [ (TopLeft, X); (TopCenter, O); (MiddleCenter, X) ]

              let remainingMoves =
                  [| TopRight; MiddleLeft; MiddleRight; BottomLeft; BottomCenter; BottomRight |]

              let result = createOTurnResult gameState remainingMoves
              let html = renderGameBoardToString result

              Expect.stringContains html "O&#39;s turn" "Should show O's turn"
              Expect.stringContains html "<span class=\"player\">X</span>" "Should show X in taken squares"
              Expect.stringContains html "<span class=\"player\">O</span>" "Should show O in taken squares"
              Expect.stringContains html "square-clickable" "Should have clickable squares for remaining moves"

          testCase "Won game renders correctly with X winner"
          <| fun _ ->
              let gameState =
                  createGameStateWith
                      [ (TopLeft, X); (TopCenter, X); (TopRight, X)
                        (MiddleLeft, O); (MiddleCenter, O) ]

              let result = createWonResult gameState X
              let html = renderGameBoardToString result

              Expect.stringContains html "X wins!" "Should show X win message"
              Expect.stringContains html "Delete Game" "Should show delete game button"
              Expect.stringContains html "delete-game-btn" "Should have delete game button class"
              Expect.stringContains html $"@delete(&#39;/games/{testGameId}&#39;)" "Should have DELETE action for this game"
              Expect.isFalse (html.Contains("square-clickable")) "Should not have any clickable squares"

          testCase "Draw game renders correctly"
          <| fun _ ->
              let gameState =
                  createGameStateWith
                      [ (TopLeft, X); (TopCenter, O); (TopRight, X)
                        (MiddleLeft, O); (MiddleCenter, X); (MiddleRight, O)
                        (BottomLeft, O); (BottomCenter, X); (BottomRight, O) ]

              let result = createDrawResult gameState
              let html = renderGameBoardToString result

              Expect.stringContains html "It&#39;s a draw!" "Should show draw message"
              Expect.stringContains html "Delete Game" "Should show delete game button"
              Expect.isFalse (html.Contains("square-clickable")) "Should not have any clickable squares"

          testCase "Error state renders correctly"
          <| fun _ ->
              let gameState = createGameStateWith [ (TopLeft, X) ]
              let errorMessage = "Invalid move attempted"
              let result = createErrorResult gameState errorMessage
              let html = renderGameBoardToString result

              Expect.stringContains html $"Error: {errorMessage}" "Should show error message"
              Expect.isFalse (html.Contains("square-clickable")) "Should not have any clickable squares"

          testCase "Game board renders all 9 squares"
          <| fun _ ->
              let emptyState = createGameStateWith []

              let allPositions =
                  [| TopLeft; TopCenter; TopRight
                     MiddleLeft; MiddleCenter; MiddleRight
                     BottomLeft; BottomCenter; BottomRight |]

              let result = createXTurnResult emptyState allPositions
              let html = renderGameBoardToString result

              let squareMatches =
                  System.Text.RegularExpressions.Regex.Matches(html, "class=\"[^\"]*square[^\"]*\"")

              Expect.equal squareMatches.Count 9 "Should render exactly 9 squares"

          testCase "Clickable squares have correct data attributes for move endpoint"
          <| fun _ ->
              let gameState = createGameStateWith [ (TopLeft, X) ]
              let remainingMoves = [| TopCenter; TopRight |]
              let result = createOTurnResult gameState remainingMoves
              let html = renderGameBoardToString result

              Expect.stringContains html $"@post(&#39;/games/{testGameId}&#39;)" "Should have POST to game resource"
              Expect.stringContains html "$player = &#39;O&#39;" "Should set player signal in click handler"
              Expect.stringContains html "$position = &#39;TopCenter&#39;" "Should set position signal in click handler"

          testCase "Board shows correct turn alternation"
          <| fun _ ->
              let gameProgression =
                  [ ([], X)
                    ([ (TopLeft, X) ], O)
                    ([ (TopLeft, X); (TopCenter, O) ], X)
                    ([ (TopLeft, X); (TopCenter, O); (MiddleLeft, X) ], O) ]

              for (i, (moves, expectedPlayer)) in gameProgression |> List.indexed do
                  let gameState = createGameStateWith moves

                  let validMoves =
                      [| TopRight; MiddleCenter; MiddleRight; BottomLeft; BottomCenter; BottomRight |]
                      |> Array.filter (fun pos -> moves |> List.exists (fun (p, _) -> p = pos) |> not)

                  let result =
                      match expectedPlayer with
                      | X -> createXTurnResult gameState validMoves
                      | O -> createOTurnResult gameState validMoves

                  let html = renderGameBoardToString result
                  let expectedMessage = $"{expectedPlayer}&#39;s turn"

                  Expect.stringContains html expectedMessage $"Should show {expectedPlayer}'s turn for move {i}" ]
