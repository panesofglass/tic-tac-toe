using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

namespace TicTacToe.Engine;

/// <summary>
/// Represents the current state of a tic-tac-toe game board.
/// The board is immutable; any changes create a new board instance.
/// </summary>
public record GameBoard : IEnumerable<Square>
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
    /// Returns an enumerator that iterates through the board squares.
    /// </summary>
    public IEnumerator<Square> GetEnumerator() => ((IEnumerable<Square>)_squares).GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the board squares.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Square>)_squares).GetEnumerator();

    /// <summary>
    /// Gets a new board with the specified move applied.
    /// </summary>
    public GameBoard WithMove(Move move, bool willEndGame = false)
    {
        if (!IsValidMove(this, move))
        {
            throw new ArgumentException("Invalid move.", nameof(move));
        }

        // Get the next marker for available spaces after this move
        Marker nextMarker = move.Marker == Marker.X ? Marker.O : Marker.X;

        // Verify that the move is for the expected next marker on this space
        var currentSquare = _squares[move.Position];
        if (currentSquare is Square.Available available && available.NextMarker != move.Marker)
        {
            throw new ArgumentException($"It's not {move.Marker}'s turn.", nameof(move));
        }

        var spacesLength = this._squares.Length;
        var newSpaces = this._squares.ToBuilder();

        // Set the moved position to Taken
        newSpaces[move.Position] = new Square.Taken(move.Marker);

        // Update all available spaces to show the next marker
        for (int i = 0; i < spacesLength; i++)
        {
            if (newSpaces[i] is Square.Available)
            {
                newSpaces[i] = willEndGame
                    ? new Square.Unavailable()
                    : new Square.Available(nextMarker);
            }
        }

        return new GameBoard(newSpaces.ToImmutable());
    }

    /// <summary>
    /// Returns true if the specified position is available for a move.
    /// </summary>
    internal static bool IsValidMove(GameBoard board, Move move)
    {
        // Position must be within board bounds (0-8)
        if (move.Position < 0 || move.Position >= board._squares.Length)
        {
            return false;
        }

        // The position must be available
        var square = board._squares[move.Position];
        if (square is not Square.Available availableSquare)
        {
            return false;
        }

        // The marker must match what's expected next
        return availableSquare.NextMarker == move.Marker;
    }

    /// <summary>
    /// Creates a game board from a sequence of moves.
    /// </summary>
    public static GameBoard FromMoves(IEnumerable<Move> moves) =>
        moves.Aggregate(Empty, (board, move) => board.WithMove(move));

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
