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

type MoveResult<'GameState> =
    | XTurn of 'GameState * ValidMovesForX
    | OTurn of 'GameState * ValidMovesForO
    | Won of 'GameState * Player
    | Draw of 'GameState

type StartGame<'GameState> = unit -> MoveResult<'GameState>

[<StructuralEquality; StructuralComparison>]
[<Struct>]
type Move =
    | XMove of SquarePosition
    | OMove of SquarePosition

type XMove<'GameState> =
    MoveResult<'GameState> * XPosition -> MoveResult<'GameState>

type OMove<'GameState> =
    MoveResult<'GameState> * OPosition -> MoveResult<'GameState>

type MakeMove<'GameState> = MoveResult<'GameState> * Move -> MoveResult<'GameState>

val startGame : StartGame<'GameState>

val makeMove : MakeMove<'GameState>
