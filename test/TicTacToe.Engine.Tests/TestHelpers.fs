module TicTacToe.Engine.Tests.TestHelpers

open Expecto
open TicTacToe.Model

// Common assertion helpers for MoveResult pattern matching
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
    | Won(_, p) -> p = player
    | _ -> false

let isDraw (result: MoveResult) =
    match result with
    | Draw _ -> true
    | _ -> false

let isError (result: MoveResult) =
    match result with
    | Error _ -> true
    | _ -> false

let getErrorMessage (result: MoveResult) =
    match result with
    | Error(_, message) -> message
    | _ -> failwith "Expected Error but got a different MoveResult variant"

// Expectation helpers
let expectNoError (result: MoveResult) message =
    match result with
    | Error(_, err) -> failwith $"{message}. Got Error: {err}"
    | _ -> Expect.isTrue true message

let expectError (result: MoveResult) expectedError message =
    match result with
    | Error(_, err) -> Expect.equal err expectedError message
    | _ -> failwith $"{message}. Expected Error but got a different MoveResult variant"

// Valid moves extraction
let getValidXMoves (result: MoveResult) =
    match result with
    | XTurn(_, validMoves) -> validMoves
    | _ -> [||]

let getValidOMoves (result: MoveResult) =
    match result with
    | OTurn(_, validMoves) -> validMoves
    | _ -> [||]

// GameState extraction
let getGameState (result: MoveResult) =
    match result with
    | XTurn(gameState, _) -> gameState
    | OTurn(gameState, _) -> gameState
    | Won(gameState, _) -> gameState
    | Draw gameState -> gameState
    | Error(gameState, _) -> gameState

// Board state assertions
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

// Move sequence application (for ModelTests)
let applyMoves (initialState: MoveResult) (moves: Move list) =
    moves
    |> List.fold
        (fun state moveAction ->
            match state with
            | Error _ -> state // If we already have an error, propagate it
            | _ -> makeMove (state, moveAction))
        initialState

