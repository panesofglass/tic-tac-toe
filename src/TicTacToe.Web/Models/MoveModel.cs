using TicTacToe.Engine;

namespace TicTacToe.Web.Models;

public record struct MoveModel(byte Position, Marker Marker);
