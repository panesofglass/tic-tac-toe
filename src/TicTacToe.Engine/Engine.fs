module TicTacToe.Engine

open System.Collections.Generic

[<StructuralEquality; StructuralComparison>]
[<Struct>]
type SquarePosition =
    | TopLeft
    | TopCenter
    | TopRight
    | MiddleLeft
    | MiddleCenter
    | MiddleRight
    | BottomLeft
    | BottomCenter
    | BottomRight

[<StructuralEquality; StructuralComparison>]
[<Struct>]
type Player = X | O

[<StructuralEquality; StructuralComparison>]
[<Struct>]
type SquareState =
    | Taken of Player
    | Empty

[<StructuralEquality; StructuralComparison>]
[<Struct>]
type XPosition = XPos of SquarePosition

[<StructuralEquality; StructuralComparison>]
[<Struct>]
type OPosition = OPos of SquarePosition

type ValidMovesForX = XPosition[]

type ValidMovesForO = OPosition[]

type GameState = IReadOnlyDictionary<SquarePosition, SquareState>

type MoveResult =
    | XTurn of GameState * ValidMovesForX
    | OTurn of GameState * ValidMovesForO
    | Won of GameState * Player
    | Draw of GameState

type StartGame = unit -> MoveResult

[<StructuralEquality; StructuralComparison>]
[<Struct>]
type Move =
    | XMove of SquarePosition
    | OMove of SquarePosition

type XMove = MoveResult * XPosition -> MoveResult

type OMove = MoveResult * OPosition -> MoveResult

type MakeMove = MoveResult * Move -> MoveResult

let startGame: StartGame = fun () ->
    let gameState =
        [| TopLeft, Empty
           TopCenter, Empty
           TopRight, Empty
           MiddleLeft, Empty
           MiddleCenter, Empty
           MiddleRight, Empty
           BottomLeft, Empty
           BottomCenter, Empty
           BottomRight, Empty |]
        |> readOnlyDict
    let validMovesForX: ValidMovesForX =
        [| for KeyValue(pos, state) in gameState do
            if state = Empty then yield XPos pos |]
    XTurn (gameState, validMovesForX)

let winningCombinations =
    [| [| TopLeft; TopCenter; TopRight |]
       [| MiddleLeft; MiddleCenter; MiddleRight |]
       [| BottomLeft; BottomCenter; BottomRight |]
       [| TopLeft; MiddleLeft; BottomLeft |]
       [| TopCenter; MiddleCenter; BottomCenter |]
       [| TopRight; MiddleRight; BottomRight |]
       [| TopLeft; MiddleCenter; BottomRight |]
       [| TopRight; MiddleCenter; BottomLeft |] |]

let (|HasWinner|_|) (gameState: GameState) =
    let getWinningPlayer (combination: SquarePosition[]) =
        let allSquares =
            combination
            |> Array.choose (fun pos -> 
                match gameState.TryGetValue(pos) with
                | true, Taken player -> Some player
                | _ -> None)

        if allSquares.Length = combination.Length && 
           Array.forall (fun p -> p = allSquares[0]) allSquares then
            Some allSquares[0]
        else
            None

    winningCombinations
    |> Array.tryPick getWinningPlayer

let (|IsDraw|_|) (gameState: GameState) =
    // First check if all squares are taken
    let noEmptySquares = gameState.Values |> Seq.forall (fun state -> state <> Empty)
    
    // Then ensure there's no winner
    if noEmptySquares then
        // Check specifically for winners
        match gameState with
        | HasWinner _ -> None  // If there's a winner, it's not a draw
        | _ -> Some()          // Full board with no winner = draw
    else
        None                    // Board not full, not a draw

let moveX: XMove = fun (moveResult, XPos xPosition) ->
    match moveResult with
    | XTurn (gameState, _) ->
        match gameState.TryGetValue(xPosition) with
        | true, Empty ->
            let gameState' =
                [| for KeyValue(pos, state) in gameState ->
                    pos, if pos = xPosition then Taken X else state |]
                |> readOnlyDict

            // First check for a winner
            match gameState' with
            | HasWinner player -> 
                Won(gameState', player)
            // Then check for a draw
            | IsDraw -> 
                Draw(gameState')
            | _ ->
                let validMovesForO: ValidMovesForO =
                    [| for KeyValue(pos, state) in gameState' do
                        if state = Empty then yield OPos pos |]
                OTurn(gameState', validMovesForO)
        | _ -> moveResult
    | _ -> moveResult

let moveO: OMove = fun (moveResult, OPos oPosition) ->
    match moveResult with
    | OTurn (gameState, _) ->
        match gameState.TryGetValue(oPosition) with
        | true, Empty ->
            let gameState' =
                [| for KeyValue(pos, state) in gameState ->
                    pos, if pos = oPosition then Taken O else state |]
                |> readOnlyDict

            // First check for a winner
            match gameState' with
            | HasWinner player -> 
                Won(gameState', player)
            // Then check for a draw
            | IsDraw -> 
                Draw(gameState')
            | _ ->
                let validMovesForX: ValidMovesForX =
                    [| for KeyValue(pos, state) in gameState' do
                        if state = Empty then yield XPos pos |]
                XTurn(gameState', validMovesForX)
        | _ -> moveResult
    | _ -> moveResult

let move: MakeMove = fun (moveResult, move) ->
    match moveResult, move with
    | XTurn _, XMove pos ->
        moveX (moveResult, XPos pos)
    | OTurn _, OMove pos ->
        moveO (moveResult, OPos pos)
    | moveResult, _ -> moveResult
