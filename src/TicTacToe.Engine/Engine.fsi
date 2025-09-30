module TicTacToe.Engine

/// A game actor that manages a single game instance using bounded channels
/// Implements the actor pattern for handling moves asynchronously
type Game =
    inherit System.IDisposable

    /// Make a move in the game
    abstract MakeMove: Model.Move -> unit

    /// Stream all game state changes as an async enumerable
    abstract GetResultsAsync:
        System.Threading.CancellationToken -> System.Collections.Generic.IAsyncEnumerable<Model.MoveResult>

/// Create a new game instance
/// Game automatically starts when created
val createGame: unit -> Game
