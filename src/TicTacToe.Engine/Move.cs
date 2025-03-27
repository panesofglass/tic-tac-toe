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
