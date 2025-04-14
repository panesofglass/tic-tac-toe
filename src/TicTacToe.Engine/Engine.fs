module TicTacToe.Engine

[<StructuralEquality; StructuralComparison>]
[<Struct>]
type ColumnPosition =
    | Left
    | Center
    | Right

[<StructuralEquality; StructuralComparison>]
[<Struct>]
type RowPosition =
    | Top
    | Middle
    | Bottom

type SquarePosition = (struct(ColumnPosition * RowPosition))

[<StructuralEquality; StructuralComparison>]
[<Struct>]
type Player = PlayerX | PlayerO

[<StructuralEquality; StructuralComparison>]
[<Struct>]
type SquareState =
    | Taken of Player
    | Empty

[<Struct>]
type Square = { Position: SquarePosition; State: SquareState }

[<Struct>]
type GameState = { Squares: Square list }

type StartGame<'GameState> = unit -> 'GameState

[<StructuralEquality; StructuralComparison>]
[<Struct>]
type PlayerXPos = PlayerXPos of Square

[<StructuralEquality; StructuralComparison>]
[<Struct>]
type PlayerOPos = PlayerOPos of Square

type ValidMovesForPlayerX = PlayerXPos list

type ValidMovesForPlayerO = PlayerOPos list

type MoveResult<'GameState> =
    | PlayerXTurn of 'GameState * ValidMovesForPlayerX
    | PlayerOTurn of 'GameState * ValidMovesForPlayerO
    | Won of 'GameState * Player
    | Draw of 'GameState

type PlayerXMove<'GameState> =
    'GameState * PlayerXPos -> MoveResult<'GameState>

type PlayerOMove<'GameState> =
    'GameState * PlayerOPos -> MoveResult<'GameState>
