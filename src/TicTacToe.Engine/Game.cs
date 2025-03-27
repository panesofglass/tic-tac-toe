using System.Collections.Immutable;

namespace TicTacToe.Engine;

/// <summary>
/// Represents a tic-tac-toe game, tracking board state and game status.
/// </summary>
public record Game
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
    public static Game MakeMove(Game game, Position position)
    {
        if (game is not InProgress)
        {
            throw new InvalidOperationException("Game is already complete.");
        }

        if (position < 0 || position > 8)
        {
            throw new ArgumentOutOfRangeException(
                nameof(position),
                "Position must be between 0 and 8."
            );
        }
        // Check if position is available
        var inProgressGame = (InProgress)game;
        if (!GameBoard.IsAvailable(inProgressGame.Board, position))
        {
            throw new ArgumentOutOfRangeException(
                paramName: "position",
                message: "The position is not available."
            );
        }

        // Get the current player from the available space
        var currentSquare = inProgressGame.Board[position];
        if (currentSquare is not Square.Available availableSquare)
        {
            throw new InvalidOperationException("The position is not available.");
        }
        return FromMoves(
            inProgressGame.Moves.Add(Move.Create(position, availableSquare.NextMarker))
        );
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
        board.AsEnumerable().All(space => space is Square.Taken)
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
        var moveArray = moves.ToImmutableArray();

        // Validate position values are in range
        foreach (var move in moveArray)
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
        var positions = moveArray.Select(m => (byte)m.Position).ToArray();
        if (positions.Length != positions.Distinct().Count())
        {
            throw new ArgumentException("Position is already occupied.");
        }

        // Build board step by step to validate moves against board state
        var board = GameBoard.Empty;

        foreach (var move in moveArray)
        {
            // Verify this position is available
            if (!GameBoard.IsAvailable(board, move.Position))
            {
                throw new ArgumentException("Position is already occupied.");
            }

            // Verify correct player is making the move
            var space = board[move.Position];
            if (space is Square.Available available && available.NextMarker != move.Marker)
            {
                throw new ArgumentException(
                    $"Players must alternate turns. Expected {available.NextMarker} but got {move.Marker}."
                );
            }

            // Apply the move
            board = GameBoard.WithMove(board, move);
        }

        if (HasWinner(board, Marker.X))
            return new Winner(board, Marker.X, moveArray);

        if (HasWinner(board, Marker.O))
            return new Winner(board, Marker.O, moveArray);

        if (IsDraw(board, moveArray))
            return new Draw(board, moveArray);

        return new Game.InProgress(board, moveArray);
    }
}
