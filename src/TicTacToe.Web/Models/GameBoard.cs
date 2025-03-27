using System.Collections.Immutable;
using System.Diagnostics;

namespace TicTacToe.Web.Models;

/// <summary>
/// Represents the current state of a tic-tac-toe game board.
/// The board is immutable; any changes create a new board instance.
/// </summary>
public record GameBoard
{
    private readonly ImmutableArray<Marker?> _spaces;

    private GameBoard(ImmutableArray<Marker?> spaces)
    {
        Debug.Assert(spaces.Length == 9, "Board must have exactly 9 spaces");
        _spaces = spaces;
    }

    /// <summary>
    /// Gets the marker at the specified position, or null if empty.
    /// </summary>
    public Marker? this[Position position] => _spaces[position];

    /// <summary>
    /// Converts a GameBoard into an array.
    /// </summary>
    public Marker?[] ToArray() => _spaces.ToArray();

    /// <summary>
    /// Returns true if the specified position is available for a move.
    /// </summary>
    public static bool IsAvailable(GameBoard board, Position position) =>
        board._spaces[position] is null;

    /// <summary>
    /// Gets a new board with the specified move applied.
    /// </summary>
    public static GameBoard WithMove(GameBoard board, Move move)
    {
        Debug.Assert(IsAvailable(board, move.Position), "Position must be empty");
        var newSpaces = board._spaces.ToBuilder();
        newSpaces[move.Position] = move.Marker;
        return new GameBoard(newSpaces.ToImmutable());
    }

    /// <summary>
    /// Creates a game board from a sequence of moves.
    /// </summary>
    public static GameBoard FromMoves(IEnumerable<Move> moves) =>
        moves.Aggregate(Empty, (board, move) => WithMove(board, move));

    /// <summary>
    /// Gets an empty game board.
    /// </summary>
    public static GameBoard Empty { get; } =
        new(ImmutableArray.Create<Marker?>(null, null, null, null, null, null, null, null, null));
}
