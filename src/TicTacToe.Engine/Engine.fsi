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
type Player =
    | X
    | O

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
    | Error of GameState * string

[<StructuralEquality; StructuralComparison>]
[<Struct>]
type Move =
    | XMove of SquarePosition
    | OMove of SquarePosition

type StartGame = unit -> MoveResult

type MakeMove = MoveResult * Move -> MoveResult

val startGame: StartGame

val makeMove: MakeMove

/// A game actor that manages a single game instance using bounded channels
/// Implements the actor pattern for handling moves asynchronously
type Game =
    inherit System.IDisposable

    /// Make a move in the game asynchronously
    abstract MakeMoveAsync: Move -> System.Threading.Tasks.Task

    /// Read all game state changes as an async enumerable
    abstract ReadAllAsync: unit -> System.Collections.Generic.IAsyncEnumerable<MoveResult>

/// Create a new game instance
/// Game automatically starts when created
val createGame: unit -> Game
