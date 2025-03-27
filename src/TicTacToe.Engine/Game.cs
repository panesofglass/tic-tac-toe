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
        var nextBoard = inProgressGame.Board.WithMove(move);
        var moves = inProgressGame.Moves.Add(move);
        return FromBoard(nextBoard, moves);
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
    private static bool IsDraw(GameBoard board, ImmutableArray<Move> moves) =>
        board.All(space => space is Square.Taken)
        && !HasWinner(board, Marker.X)
        && !HasWinner(board, Marker.O);

    /// <summary>
    /// Creates a new game.
    /// </summary>
    public static Game Create() => new Game.InProgress(GameBoard.Empty, ImmutableArray<Move>.Empty);

    /// <summary>
    /// Creates a game from a sequence of moves.
    /// </summary>
    public static Game FromBoard(GameBoard board, ImmutableArray<Move> moves)
    {
        // Build board step by step to validate moves against board state
        var moveArray = moves.ToImmutableArray();

        if (HasWinner(board, Marker.X))
            return new Winner(board, Marker.X, moveArray);

        if (HasWinner(board, Marker.O))
            return new Winner(board, Marker.O, moveArray);

        if (IsDraw(board, moveArray))
            return new Draw(board, moveArray);

        return new Game.InProgress(board, moveArray);
    }

    /// <summary>
    /// Creates a game from a sequence of moves.
    /// </summary>
    public static Game FromMoves(IEnumerable<Move> moves)
    {
        // Validate position values are in range
        foreach (var move in moves)
        {
            if (move.Position < 0 || move.Position > 8)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(move.Position),
                    "Position must be between 0 and 8."
                );
            }
        }

        // Validate no duplicate positions
        var positions = moves.Select(m => (byte)m.Position).ToArray();
        if (positions.Length != positions.Distinct().Count())
        {
            throw new ArgumentException("Position is already occupied.");
        }

        // Build board step by step to validate moves against board state
        var board = moves.Aggregate(GameBoard.Empty, (board, move) => board.WithMove(move));
        return FromBoard(board, moves.ToImmutableArray());
    }
}
