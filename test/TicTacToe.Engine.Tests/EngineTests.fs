module Tests

open Expecto
open TicTacToe.Engine
open System.Collections.Generic

// Helper functions for testing
let applyMoves (initialState: MoveResult) (moves: Move list) =
    moves |> List.fold (fun state moveAction -> move (state, moveAction)) initialState

let isXTurn (result: MoveResult) =
    match result with
    | XTurn _ -> true
    | _ -> false

let isOTurn (result: MoveResult) =
    match result with
    | OTurn _ -> true
    | _ -> false

let isWon (result: MoveResult) (player: Player) =
    match result with
    | Won (_, p) -> p = player
    | _ -> false

let isDraw (result: MoveResult) =
    match result with
    | Draw _ -> true
    | _ -> false

let getGameState (result: MoveResult) =
    match result with
    | XTurn (gameState, _) -> gameState
    | OTurn (gameState, _) -> gameState
    | Won (gameState, _) -> gameState
    | Draw gameState -> gameState

let getValidXMoves (result: MoveResult) =
    match result with
    | XTurn (_, validMoves) -> validMoves
    | _ -> [||]

let getValidOMoves (result: MoveResult) =
    match result with
    | OTurn (_, validMoves) -> validMoves
    | _ -> [||]

let expectSquareState (gameState: GameState) (position: SquarePosition) (expectedState: SquareState) (message: string) =
    match gameState.TryGetValue(position) with
    | true, state -> Expect.equal state expectedState message
    | false, _ -> failwith $"Position {position} not found in game state"

let expectEmptySquare (gameState: GameState) (position: SquarePosition) =
    expectSquareState gameState position Empty $"Expected position {position} to be empty"

let expectTakenByX (gameState: GameState) (position: SquarePosition) =
    expectSquareState gameState position (Taken X) $"Expected position {position} to be taken by X"

let expectTakenByO (gameState: GameState) (position: SquarePosition) =
    expectSquareState gameState position (Taken O) $"Expected position {position} to be taken by O"

[<Tests>]
let gameInitializationTests =
    testList "Game Initialization Tests" [
        testCase "startGame creates a new game with X's turn" <| fun _ ->
            let result = startGame ()
            Expect.isTrue (isXTurn result) "Initial game state should be X's turn"

        testCase "All squares are empty in the initial state" <| fun _ ->
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

        testCase "All valid moves are available at the start" <| fun _ ->
            let result = startGame ()
            let validMoves = getValidXMoves result

            Expect.equal validMoves.Length 9 "Should have 9 valid moves at start"

            // Check that all positions are available
            let positions = validMoves |> Array.map (fun (XPos pos) -> pos) |> Set.ofArray
            let expectedPositions = 
                [TopLeft; TopCenter; TopRight; 
                 MiddleLeft; MiddleCenter; MiddleRight; 
                 BottomLeft; BottomCenter; BottomRight] |> Set.ofList

            Expect.equal positions expectedPositions "All board positions should be valid moves"
    ]

[<Tests>]
let moveMechanicsTests =
    testList "Move Mechanics Tests" [
        testCase "Making a valid X move results in O's turn" <| fun _ ->
            let initialState = startGame ()
            let result = move (initialState, XMove TopLeft)

            Expect.isTrue (isOTurn result) "After X moves, it should be O's turn"

            let gameState = getGameState result
            expectTakenByX gameState TopLeft

        testCase "Making a valid O move results in X's turn" <| fun _ ->
            let initialState = startGame ()
            let afterXMove = move (initialState, XMove TopLeft)
            let result = move (afterXMove, OMove TopRight)

            Expect.isTrue (isXTurn result) "After O moves, it should be X's turn"

            let gameState = getGameState result
            expectTakenByX gameState TopLeft
            expectTakenByO gameState TopRight

        testCase "Valid moves array is updated after each move" <| fun _ ->
            let initialState = startGame ()
            let afterXMove = move (initialState, XMove TopLeft)

            // Check O's valid moves
            let validOMoves = getValidOMoves afterXMove
            Expect.equal validOMoves.Length 8 "Should have 8 valid moves after X plays"

            // Check that TopLeft is no longer a valid move
            let oPositions = validOMoves |> Array.map (fun (OPos pos) -> pos) |> Set.ofArray
            Expect.isFalse (oPositions.Contains TopLeft) "TopLeft should no longer be a valid move"

            // Make O move
            let afterOMove = move (afterXMove, OMove TopRight)

            // Check X's valid moves
            let validXMoves = getValidXMoves afterOMove
            Expect.equal validXMoves.Length 7 "Should have 7 valid moves after O plays"

            // Check that both played positions are no longer valid moves
            let xPositions = validXMoves |> Array.map (fun (XPos pos) -> pos) |> Set.ofArray
            Expect.isFalse (xPositions.Contains TopLeft) "TopLeft should no longer be a valid move"
            Expect.isFalse (xPositions.Contains TopRight) "TopRight should no longer be a valid move"
    ]

[<Tests>]
let winConditionTests =
    testList "Win Condition Tests" [
        testCase "X wins with top row" <| fun _ ->
            let moves = [
                XMove TopLeft    // X
                OMove MiddleLeft // O
                XMove TopCenter  // X
                OMove MiddleCenter // O
                XMove TopRight   // X wins
            ]

            let result = applyMoves (startGame()) moves

            Expect.isTrue (isWon result X) "X should win with top row"

            let gameState = getGameState result
            expectTakenByX gameState TopLeft
            expectTakenByX gameState TopCenter
            expectTakenByX gameState TopRight

        testCase "O wins with middle row" <| fun _ ->
            let moves = [
                XMove TopLeft      // X
                OMove MiddleLeft   // O
                XMove TopCenter    // X
                OMove MiddleCenter // O
                XMove BottomRight  // X
                OMove MiddleRight  // O wins
            ]

            let result = applyMoves (startGame()) moves

            Expect.isTrue (isWon result O) "O should win with middle row"

            let gameState = getGameState result
            expectTakenByO gameState MiddleLeft
            expectTakenByO gameState MiddleCenter
            expectTakenByO gameState MiddleRight

        testCase "X wins with left column" <| fun _ ->
            let moves = [
                XMove TopLeft     // X
                OMove TopCenter   // O
                XMove MiddleLeft  // X
                OMove MiddleCenter // O
                XMove BottomLeft  // X wins
            ]

            let result = applyMoves (startGame()) moves

            Expect.isTrue (isWon result X) "X should win with left column"

            let gameState = getGameState result
            expectTakenByX gameState TopLeft
            expectTakenByX gameState MiddleLeft
            expectTakenByX gameState BottomLeft

        testCase "O wins with diagonal" <| fun _ ->
            let moves = [
                XMove TopCenter    // X
                OMove TopLeft      // O
                XMove MiddleLeft   // X
                OMove MiddleCenter // O
                XMove BottomLeft   // X
                OMove BottomRight  // O wins
            ]

            let result = applyMoves (startGame()) moves

            Expect.isTrue (isWon result O) "O should win with diagonal"

            let gameState = getGameState result
            expectTakenByO gameState TopLeft
            expectTakenByO gameState MiddleCenter
            expectTakenByO gameState BottomRight
    ]

[<Tests>]
let drawConditionTests =
    testList "Draw Condition Tests" [
        testCase "Game ends in a draw when board is full with no winner" <| fun _ ->
            // This sequence creates a full board with no winner
            let moves = [
                XMove TopLeft      // X | O | X    First row
                OMove TopCenter    // X | O | O    Second row
                XMove MiddleLeft   // O | X | O    Third row - no winning lines
                OMove MiddleRight
                XMove TopRight
                OMove BottomLeft
                XMove BottomCenter
                OMove BottomRight
                XMove MiddleCenter // Final move, fills board with no winner
            ]

            let result = applyMoves (startGame()) moves

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
            expectTakenByO gameState BottomRight
    ]

[<Tests>]
let invalidMoveTests =
    testList "Invalid Move Tests" [
        testCase "Attempting to move in an already taken square is invalid" <| fun _ ->
            let initialState = startGame ()
            let afterXMove = move (initialState, XMove TopLeft)

            // O tries to move in the same square
            let result = move (afterXMove, OMove TopLeft)

            // Game state should not change
            Expect.isTrue (isOTurn result) "Should still be O's turn after invalid move"

            let gameState = getGameState result
            expectTakenByX gameState TopLeft

        testCase "Attempting to make O move during X's turn is invalid" <| fun _ ->
            let initialState = startGame ()

            // O tries to move on X's turn
            let result = move (initialState, OMove TopLeft)

            // Game state should not change
            Expect.isTrue (isXTurn result) "Should still be X's turn after invalid move"

            let gameState = getGameState result
            expectEmptySquare gameState TopLeft

        testCase "Attempting to make X move during O's turn is invalid" <| fun _ ->
            let initialState = startGame ()
            let afterXMove = move (initialState, XMove TopLeft)

            // X tries to move again on O's turn
            let result = move (afterXMove, XMove TopRight)

            // Game state should not change
            Expect.isTrue (isOTurn result) "Should still be O's turn after invalid move"

            let gameState = getGameState result
            expectTakenByX gameState TopLeft
            expectEmptySquare gameState TopRight
    ]

[<Tests>]
let tests =
    testList "TicTacToe Engine Tests" [
        gameInitializationTests
        moveMechanicsTests
        winConditionTests
        drawConditionTests
        invalidMoveTests
    ]
