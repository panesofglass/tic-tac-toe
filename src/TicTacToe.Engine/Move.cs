namespace TicTacToe.Engine;

/// <summary>
/// Represents a single move in a tic-tac-toe game.
/// </summary>
public readonly record struct Move
{
    public Position Position { get; }
    public Marker Marker { get; }
    public DateTimeOffset Timestamp { get; }

    private Move(Position position, Marker marker, DateTimeOffset timestamp)
    {
        Position = position;
        Marker = marker;
        Timestamp = timestamp;
    }

    /// <summary>
    /// Creates a new move with the current timestamp.
    /// </summary>
    public static Move Create(Position position, Marker marker)
    {
        if (position < 0 || position > 8)
        {
            throw new ArgumentOutOfRangeException(
                nameof(position),
                "Position must be between 0 and 8."
            );
        }
        return new(position, marker, DateTimeOffset.UtcNow);
    }
}
