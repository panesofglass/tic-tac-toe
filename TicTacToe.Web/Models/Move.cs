using System;
using System.Diagnostics;

namespace TicTacToe.Web.Models;

/// <summary>
/// Represents a single move in a tic-tac-toe game.
/// </summary>
public record Move(
    Position Position,
    Marker Marker,
    DateTimeOffset Timestamp)
{
    /// <summary>
    /// Creates a new move with the current timestamp.
    /// </summary>
    public static Move Create(Position position, Marker marker) => 
        new(position, marker, DateTimeOffset.UtcNow);
}

/// <summary>
/// Represents a position on the game board using zero-based coordinates.
/// Each coordinate must be 0, 1, or 2.
/// </summary>
public record Position
{
    public byte Row { get; }
    public byte Column { get; }

    public Position(byte row, byte column)
    {
        Debug.Assert(row <= 2, "Row must be 0, 1, or 2");
        Debug.Assert(column <= 2, "Column must be 0, 1, or 2");
        
        Row = row;
        Column = column;
    }
}

/// <summary>
/// Represents a marker (X or O) in the game.
/// </summary>
public enum Marker
{
    X,
    O
}

