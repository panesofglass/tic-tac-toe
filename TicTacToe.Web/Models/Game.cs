using System.Collections.Immutable;
using System.Diagnostics;

namespace TicTacToe.Web.Models;

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
    public sealed record InProgress(
        GameBoard Board,
        Marker CurrentPlayer,
        ImmutableArray<Move> Moves
    ) : Game;

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
        Debug.Assert(game is InProgress, "Game is already complete");
        Debug.Assert(
            GameBoard.IsAvailable(((InProgress)game).Board, position),
            "The position is not available"
        );
        return game switch
        {
            Game.InProgress g when GameBoard.IsAvailable(g.Board, position) => FromMoves(
                g.Moves.Add(Move.Create(position, g.CurrentPlayer))
            ),
            _ => throw new ArgumentOutOfRangeException(
                paramName: "position",
                message: "The position is not available."
            ),
        };
    }

    /// <summary>
    /// Returns true if the specified marker has won the game.
    /// </summary>
    private static bool HasWinner(GameBoard board, Marker marker)
    {
        return WinningCombinations.Any(combination =>
            combination.All(position => board[new Position((byte)position)] == marker)
        );
    }

    /// <summary>
    /// Returns true if all positions are filled and there is no winner.
    /// </summary>
    private static bool IsDraw(GameBoard board, ImmutableArray<Move> moves) =>
        moves.Length == 9 && !HasWinner(board, Marker.X) && !HasWinner(board, Marker.O);

    /// <summary>
    /// Creates a new game.
    /// </summary>
    public static Game Create() =>
        new Game.InProgress(GameBoard.Empty, Marker.X, ImmutableArray<Move>.Empty);

    /// <summary>
    /// Creates a game from a sequence of moves.
    /// </summary>
    public static Game FromMoves(IEnumerable<Move> moves)
    {
        var moveArray = moves.ToImmutableArray();
        var board = GameBoard.FromMoves(moveArray);
        var currentPlayer = moveArray.Length % 2 == 0 ? Marker.X : Marker.O;
        if (HasWinner(board, Marker.X))
            return new Winner(board, Marker.X, moveArray);

        if (HasWinner(board, Marker.O))
            return new Winner(board, Marker.O, moveArray);

        if (IsDraw(board, moveArray))
            return new Draw(board, moveArray);

        return new Game.InProgress(board, currentPlayer, moveArray);
    }
}
