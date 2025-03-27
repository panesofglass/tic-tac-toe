using System.Collections.Immutable;
using System.Diagnostics;

namespace TicTacToe.Engine;

/// <summary>
/// Represents the current state of a tic-tac-toe game board.
/// The board is immutable; any changes create a new board instance.
/// </summary>
public record GameBoard
{
    private readonly ImmutableArray<Square> _squares;

    private GameBoard(ImmutableArray<Square> spaces)
    {
        Debug.Assert(spaces.Length == 9, "Board must have exactly 9 spaces");
        _squares = spaces;
    }

    /// <summary>
    /// Gets the state of the space at the specified position.
    /// </summary>
    public Square this[Position position] => _squares[position];

    /// <summary>
    /// Converts a GameBoard into an enumerable.
    /// </summary>
    public IEnumerable<Square> AsEnumerable() => _squares.AsEnumerable();

    /// <summary>
    /// Returns true if the specified position is available for a move.
    /// </summary>
    public static bool IsAvailable(GameBoard board, Position position) =>
        board._squares[position] is Square.Available;

    /// <summary>
    /// Gets a new board with the specified move applied.
    /// </summary>
    public static GameBoard WithMove(GameBoard board, Move move)
    {
        Debug.Assert(IsAvailable(board, move.Position), "Position must be empty");

        // Get the next marker for available spaces after this move
        Marker nextMarker = move.Marker == Marker.X ? Marker.O : Marker.X;

        var spacesLength = board._squares.Length;
        var newSpaces = board._squares.ToBuilder();

        // Set the moved position to Taken
        newSpaces[move.Position] = new Square.Taken(move.Marker);

        // Update all available spaces to show the next marker
        for (int i = 0; i < spacesLength; i++)
        {
            if (newSpaces[i] is Square.Available)
            {
                newSpaces[i] = new Square.Available(nextMarker);
            }
        }

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
    public static GameBoard Empty { get; } = CreateEmptyBoard();

    private static GameBoard CreateEmptyBoard()
    {
        var builder = ImmutableArray.CreateBuilder<Square>(9);
        for (int i = 0; i < 9; i++)
        {
            builder.Add(new Square.Available(Marker.X));
        }
        return new GameBoard(builder.ToImmutable());
    }
}
