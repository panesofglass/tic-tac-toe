using System.Collections.Immutable;

namespace TicTacToe.Engine;

/// <summary>
/// Represents a tic-tac-toe game, tracking board state and game status.
/// </summary>
public abstract record Game
{
    private static readonly byte[][] WinningCombinations = new byte[][]
    {
        new byte[] { 0, 1, 2 }, // Top row
        new byte[] { 3, 4, 5 }, // Middle row
        new byte[] { 6, 7, 8 }, // Bottom row
        new byte[] { 0, 3, 6 }, // Left column
        new byte[] { 1, 4, 7 }, // Middle column
        new byte[] { 2, 5, 8 }, // Right column
        new byte[] { 0, 4, 8 }, // Diagonal
        new byte[] { 2, 4, 6 }, // Diagonal
    };

    private Game() { }

    /// <summary>
    /// The game is still in progress.
    /// </summary>
    public sealed record InProgress(GameBoard Board, ImmutableArray<Move> Moves) : Game;

    /// <summary>
    /// The game has been won by a player.
    /// </summary>
    public sealed record Winner(GameBoard Board, Marker WinningPlayer, ImmutableArray<Move> Moves)
        : Game;

    /// <summary>
    /// The game is a draw.
    /// </summary>
    public sealed record Draw(GameBoard Board, ImmutableArray<Move> Moves) : Game;

    /// <summary>
    /// Makes a move at the specified position for the current player.
    /// </summary>
    public Game WithMove(Move move)
    {
        if (this is not InProgress)
        {
            throw new InvalidOperationException("Game is already complete.");
        }

        var inProgressGame = (InProgress)this;

        // Simulate the move to check if it ends the game
        var moves = inProgressGame.Moves.Add(move);
        var nextBoard = inProgressGame.Board.WithMove(move);
        var willEndGame = HasWinner(nextBoard, move.Marker) || IsDraw(nextBoard);
        if (willEndGame)
        {
            // Apply the move with willEndGame flag set appropriately
            nextBoard = inProgressGame.Board.WithMove(move, willEndGame);
        }

        // Check if this move results in a win
        if (HasWinner(nextBoard, move.Marker))
        {
            return new Winner(nextBoard, move.Marker, moves);
        }

        // Check if this move results in a draw
        if (IsDraw(nextBoard))
        {
            return new Draw(nextBoard, moves);
        }

        // Game continues
        return new InProgress(nextBoard, moves);
    }

    /// <summary>
    /// Returns true if the specified marker has won the game.
    /// </summary>
    private static bool HasWinner(GameBoard board, Marker marker)
    {
        return WinningCombinations.Any(combination =>
            combination.All(position =>
                board[new Position((byte)position)] is Square.Taken taken && taken.Marker == marker
            )
        );
    }

    /// <summary>
    /// Returns true if all positions are filled and there is no winner.
    /// </summary>
    private static bool IsDraw(GameBoard board) =>
        board.All(space => space is Square.Taken or Square.Unavailable)
        && !HasWinner(board, Marker.X)
        && !HasWinner(board, Marker.O);

    /// <summary>
    /// Creates a new game.
    /// </summary>
    public static Game Create() => new Game.InProgress(GameBoard.Empty, ImmutableArray<Move>.Empty);

    /// <summary>
    /// Creates a game from a sequence of moves.
    /// </summary>
    public static Game FromMoves(IEnumerable<Move> moves)
    {
        // Build board step by step to validate moves against board state
        var moveArray = moves.ToImmutableArray();

        // Handle empty move array
        if (moveArray.Length == 0)
        {
            return Create();
        }

        var board = GameBoard.Empty;

        // Process all moves except the last one
        if (moveArray.Length > 1)
        {
            foreach (var move in moveArray[..^1])
            {
                board = board.WithMove(move);
            }
        }

        // Process the last move
        var lastMove = moveArray[^1];
        var nextBoard = board.WithMove(lastMove);

        // Check if the game has ended
        var willEndGame =
            HasWinner(nextBoard, Marker.X) || HasWinner(nextBoard, Marker.O) || IsDraw(nextBoard);

        // If we have a last move and the game is ending, replay it with willEndGame = true
        var finalBoard = willEndGame ? board.WithMove(lastMove, true) : nextBoard;

        // Check for winner X
        if (HasWinner(finalBoard, Marker.X))
        {
            return new Winner(finalBoard, Marker.X, moveArray);
        }

        // Check for winner O
        if (HasWinner(finalBoard, Marker.O))
        {
            return new Winner(finalBoard, Marker.O, moveArray);
        }

        // Check for draw
        if (IsDraw(finalBoard))
        {
            return new Draw(finalBoard, moveArray);
        }

        return new Game.InProgress(finalBoard, moveArray);
    }
}
