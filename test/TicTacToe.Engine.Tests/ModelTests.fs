module TicTacToe.Engine.Tests.ModelTests

open Expecto
open TicTacToe.Model
open TicTacToe.Engine.Tests.TestHelpers

[<Tests>]
let gameInitializationTests =
    testList
        "Game Initialization Tests"
        [ testCase "startGame creates a new game with X's turn"
          <| fun _ ->
              let result = startGame ()
              Expect.isTrue (isXTurn result) "Initial game state should be X's turn"

          testCase "All squares are empty in the initial state"
          <| fun _ ->
              let result = startGame ()
              let gameState = getGameState result

              // Check all positions are empty
              expectEmptySquare gameState TopLeft
              expectEmptySquare gameState TopCenter
              expectEmptySquare gameState TopRight
              expectEmptySquare gameState MiddleLeft
              expectEmptySquare gameState MiddleCenter
              expectEmptySquare gameState MiddleRight
              expectEmptySquare gameState BottomLeft
              expectEmptySquare gameState BottomCenter
              expectEmptySquare gameState BottomRight

          testCase "All valid moves are available at the start"
          <| fun _ ->
              let result = startGame ()
              let validMoves = getValidXMoves result

              Expect.equal validMoves.Length 9 "Should have 9 valid moves at start"

              // Check that all positions are available
              let positions = validMoves |> Array.map (fun (XPos pos) -> pos) |> Set.ofArray

              let expectedPositions =
                  [ TopLeft
                    TopCenter
                    TopRight
                    MiddleLeft
                    MiddleCenter
                    MiddleRight
                    BottomLeft
                    BottomCenter
                    BottomRight ]
                  |> Set.ofList

              Expect.equal positions expectedPositions "All board positions should be valid moves" ]

[<Tests>]
let moveMechanicsTests =
    testList
        "Move Mechanics Tests"
        [ testCase "Valid moves array is updated after each move"
          <| fun _ ->
              let initialState = startGame ()
              let afterXMove = makeMove (initialState, XMove TopLeft)
              expectNoError afterXMove "X's move should succeed"

              // Check O's valid moves
              let validOMoves = getValidOMoves afterXMove
              Expect.equal validOMoves.Length 8 "Should have 8 valid moves after X plays"

              // Check that TopLeft is no longer a valid move
              let oPositions = validOMoves |> Array.map (fun (OPos pos) -> pos) |> Set.ofArray
              Expect.isFalse (oPositions.Contains TopLeft) "TopLeft should no longer be a valid move"

              // Make O move
              let afterOMove = makeMove (afterXMove, OMove TopRight)
              expectNoError afterOMove "O's move should succeed"

              // Check X's valid moves
              let validXMoves = getValidXMoves afterOMove
              Expect.equal validXMoves.Length 7 "Should have 7 valid moves after O plays"

              // Check that both played positions are no longer valid moves
              let xPositions = validXMoves |> Array.map (fun (XPos pos) -> pos) |> Set.ofArray
              Expect.isFalse (xPositions.Contains TopLeft) "TopLeft should no longer be a valid move"
              Expect.isFalse (xPositions.Contains TopRight) "TopRight should no longer be a valid move" ]

[<Tests>]
let winConditionTests =
    testList
        "Win Condition Tests"
        [ testCase "X wins with top row"
          <| fun _ ->
              let moves =
                  [ XMove TopLeft // X
                    OMove MiddleLeft // O
                    XMove TopCenter // X
                    OMove MiddleCenter // O
                    XMove TopRight ] // X wins

              let result = applyMoves (startGame ()) moves
              expectNoError result "Applying moves should succeed"

              Expect.isTrue (isWon result X) "X should win with top row"

              let gameState = getGameState result
              expectTakenByX gameState TopLeft
              expectTakenByX gameState TopCenter
              expectTakenByX gameState TopRight

          testCase "O wins with middle row"
          <| fun _ ->
              let moves =
                  [ XMove TopLeft // X
                    OMove MiddleLeft // O
                    XMove TopCenter // X
                    OMove MiddleCenter // O
                    XMove BottomRight // X
                    OMove MiddleRight ] // O wins

              let result = applyMoves (startGame ()) moves
              expectNoError result "Applying moves should succeed"

              Expect.isTrue (isWon result O) "O should win with middle row"

              let gameState = getGameState result
              expectTakenByO gameState MiddleLeft
              expectTakenByO gameState MiddleCenter
              expectTakenByO gameState MiddleRight

          testCase "X wins with left column"
          <| fun _ ->
              let moves =
                  [ XMove TopLeft // X
                    OMove TopCenter // O
                    XMove MiddleLeft // X
                    OMove MiddleCenter // O
                    XMove BottomLeft ] // X wins

              let result = applyMoves (startGame ()) moves
              expectNoError result "Applying moves should succeed"

              Expect.isTrue (isWon result X) "X should win with left column"

              let gameState = getGameState result
              expectTakenByX gameState TopLeft
              expectTakenByX gameState MiddleLeft
              expectTakenByX gameState BottomLeft

          testCase "O wins with diagonal"
          <| fun _ ->
              let moves =
                  [ XMove TopCenter // X
                    OMove TopLeft // O
                    XMove MiddleLeft // X
                    OMove MiddleCenter // O
                    XMove BottomLeft // X
                    OMove BottomRight ] // O wins

              let result = applyMoves (startGame ()) moves
              expectNoError result "Applying moves should succeed"

              Expect.isTrue (isWon result O) "O should win with diagonal"

              let gameState = getGameState result
              expectTakenByO gameState TopLeft
              expectTakenByO gameState MiddleCenter
              expectTakenByO gameState BottomRight ]

[<Tests>]
let drawConditionTests =
    testList
        "Draw Condition Tests"
        [ testCase "Game ends in a draw when board is full with no winner"
          <| fun _ ->
              // This sequence creates a full board with no winner
              let moves =
                  [ XMove TopLeft // X | O | X    First row
                    OMove TopCenter // X | X | O    Second row
                    XMove MiddleLeft // O | X | O    Third row - no winning lines
                    OMove MiddleRight
                    XMove TopRight
                    OMove BottomLeft
                    XMove BottomCenter
                    OMove BottomRight
                    XMove MiddleCenter ] // Final move, fills board with no winner

              let result = applyMoves (startGame ()) moves
              expectNoError result "Applying moves should succeed"

              Expect.isTrue (isDraw result) "Game should end in a draw"

              // Verify board is full
              let gameState = getGameState result
              expectTakenByX gameState TopLeft
              expectTakenByO gameState TopCenter
              expectTakenByX gameState TopRight
              expectTakenByX gameState MiddleLeft
              expectTakenByX gameState MiddleCenter
              expectTakenByO gameState MiddleRight
              expectTakenByO gameState BottomLeft
              expectTakenByX gameState BottomCenter
              expectTakenByO gameState BottomRight ]

[<Tests>]
let invalidMoveTests =
    testList
        "Invalid Move Tests"
        [ testCase "Attempting to move in an already taken square is invalid"
          <| fun _ ->
              let initialState = startGame ()
              let afterXMove = makeMove (initialState, XMove TopLeft)
              expectNoError afterXMove "X's move should succeed"

              // O tries to move in the same square
              let result = makeMove (afterXMove, OMove TopLeft)
              expectError result "Invalid move" "Invalid move should return Error but not change state"

              let gameState = getGameState afterXMove
              expectTakenByX gameState TopLeft

          testCase "Attempting to make O move during X's turn is invalid"
          <| fun _ ->
              let initialState = startGame ()

              // O tries to move on X's turn
              let result = makeMove (initialState, OMove TopLeft)
              expectError result "Invalid move" "Invalid move should return Error but not change state"

              let gameState = getGameState initialState
              expectEmptySquare gameState TopLeft

          testCase "Attempting to make X move during O's turn is invalid"
          <| fun _ ->
              let initialState = startGame ()
              let afterXMove = makeMove (initialState, XMove TopLeft)
              expectNoError afterXMove "X's move should succeed"

              // X tries to move again on O's turn
              let result = makeMove (afterXMove, XMove TopRight)
              expectError result "Invalid move" "Invalid move should return Error but not change state"

              let gameState = getGameState afterXMove
              expectEmptySquare gameState TopRight

          testCase "Attempting to move after X wins is invalid"
          <| fun _ ->
              let moves =
                  [ XMove TopLeft // X wins with top row
                    OMove MiddleLeft
                    XMove TopCenter
                    OMove MiddleCenter
                    XMove TopRight ]

              let wonState = applyMoves (startGame ()) moves
              expectNoError wonState "X should have won"

              // Try to make moves after game is won
              let resultAfterO = makeMove (wonState, OMove BottomLeft)
              let resultAfterX = makeMove (wonState, XMove BottomRight)

              // Should get "Game already won" error
              expectError resultAfterO "Game already won" "Should return error when moving after win"
              expectError resultAfterX "Game already won" "Should return error when moving after win"

              // Original game state
              let gameState = getGameState wonState
              expectTakenByX gameState TopLeft
              expectTakenByX gameState TopCenter
              expectTakenByX gameState TopRight
              expectTakenByO gameState MiddleLeft
              expectTakenByO gameState MiddleCenter
              expectEmptySquare gameState BottomLeft
              expectEmptySquare gameState BottomRight

          testCase "Attempting to move after draw is invalid"
          <| fun _ ->
              let moves =
                  [ XMove TopLeft // X | O | X
                    OMove TopCenter // X | X | O
                    XMove MiddleLeft // O | X | O
                    OMove MiddleRight
                    XMove TopRight
                    OMove BottomLeft
                    XMove BottomCenter
                    OMove BottomRight
                    XMove MiddleCenter ] // Draw - no winner

              let drawState = applyMoves (startGame ()) moves
              expectNoError drawState "Game should end in draw"

              // Try to make moves after draw
              let resultAfterX = makeMove (drawState, XMove TopLeft) // Try to override existing move
              let resultAfterO = makeMove (drawState, OMove TopRight) // Try to override existing move

              // Should get "Game over" error
              expectError resultAfterX "Game over" "Should return error when moving after draw"
              expectError resultAfterO "Game over" "Should return error when moving after draw"

              // Original game state should remain unchanged
              let gameState = getGameState drawState
              expectTakenByX gameState TopLeft
              expectTakenByO gameState TopCenter
              expectTakenByX gameState TopRight
              expectTakenByX gameState MiddleLeft
              expectTakenByX gameState MiddleCenter
              expectTakenByO gameState MiddleRight
              expectTakenByO gameState BottomLeft
              expectTakenByX gameState BottomCenter
              expectTakenByO gameState BottomRight ]

[<Tests>]
let moveResultErrorTests =
    testList
        "MoveResult Error Tests"
        [ testCase "Moving with wrong player returns Invalid move error"
          <| fun _ ->
              let initialState = startGame ()
              let wrongPlayerMove = makeMove (initialState, OMove TopLeft)
              expectError wrongPlayerMove "Invalid move" "Should get error after wrong player move"

          testCase "Error handling with pattern matching works correctly"
          <| fun _ ->
              // Demo of error handling pattern with MoveResult
              let initialState = startGame ()
              let moves = [ XMove TopLeft; OMove TopCenter; XMove TopRight ]

              // Simulate a sequence of moves with error handling using pattern matching
              let finalResult =
                  moves
                  |> List.fold
                      (fun state move ->
                          match state with
                          | Error _ -> state
                          | _ ->
                              let moveResult = makeMove (state, move)

                              match moveResult with
                              | Error _ -> moveResult
                              | _ -> moveResult)
                      initialState

              // Assert final result is not an error
              expectNoError finalResult "Move sequence should succeed"

              // Check that moves were applied correctly
              let gameState = getGameState finalResult
              expectTakenByX gameState TopLeft
              expectTakenByO gameState TopCenter
              expectTakenByX gameState TopRight

          testCase "Error propagation works correctly"
          <| fun _ ->
              // Test error propagation with a sequence of moves leading to a win
              // followed by an invalid move
              let initialState = startGame ()

              let winningMoves =
                  [ XMove TopLeft
                    OMove MiddleLeft
                    XMove TopCenter
                    OMove MiddleCenter
                    XMove TopRight ] // X wins

              // Apply moves until we reach a win
              let wonGameResult = applyMoves (startGame ()) winningMoves
              expectNoError wonGameResult "Winning sequence should succeed"

              // Try to make another move after game is won
              let invalidMoveResult = makeMove (wonGameResult, OMove BottomLeft)

              // This should be an error
              expectError invalidMoveResult "Game already won" "Should get error when moving after win" ]

[<Tests>]
let tests =
    testList
        "TicTacToe Engine Tests"
        [ gameInitializationTests
          moveMechanicsTests
          winConditionTests
          invalidMoveTests
          moveResultErrorTests ]
