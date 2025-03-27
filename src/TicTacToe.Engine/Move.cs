namespace TicTacToe.Engine;

/// <summary>
/// Represents a single move in a tic-tac-toe game.
/// </summary>
public record Move(Position Position, Marker Marker, DateTimeOffset Timestamp)
{
    /// <summary>
    /// Creates a new move with the current timestamp.
    /// </summary>
    public static Move Create(Position position, Marker marker) =>
        new(position, marker, DateTimeOffset.UtcNow);
}

/// <summary>
/// Represents a position on the game board using a 0-based index (0-8):
/// 0 1 2
/// 3 4 5
/// 6 7 8
/// </summary>
public readonly struct Position
{
    private readonly byte _value;

    public Position(byte position)
    {
        if (position > 8)
        {
            throw new ArgumentOutOfRangeException(
                nameof(position),
                "Position must be between 0 and 8."
            );
        }
        _value = position;
    }

    public byte Row => (byte)(_value / 3);
    public byte Column => (byte)(_value % 3);

    public static Position FromIndex(byte index) => new(index);

    public static Position At(byte row, byte column)
    {
        if (row > 2)
        {
            throw new ArgumentOutOfRangeException(nameof(row), "Row must be between 0 and 2.");
        }
        if (column > 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(column),
                "Column must be between 0 and 2."
            );
        }
        return new((byte)(row * 3 + column));
    }

    public static implicit operator byte(Position position) => position._value;

    public static explicit operator Position(byte value) => new(value);

    public override string ToString() => _value.ToString();
}

/// <summary>
/// Represents a marker (X or O) in the game.
/// </summary>
public enum Marker
{
    X,
    O,
}
