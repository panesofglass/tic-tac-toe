using TicTacToe.Web.Infrastructure;

namespace TicTacToe.Web.Models;

public class LayoutModel
{
    public string Title { get; init; } = "Tic-Tac-Toe";
    public Player? CurrentPlayer { get; init; }
}
