module TicTacToe.Engine

/// A game actor that manages a single game instance using bounded channels
/// Implements the actor pattern for handling moves asynchronously
type Game =
    inherit System.IDisposable
    inherit System.IObservable<Model.MoveResult>

    /// Make a move in the game
    abstract MakeMove: Model.Move -> unit

/// Create a new game instance
/// Game automatically starts when created
val createGame: unit -> Game
