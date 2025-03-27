using TicTacToe.Engine;

namespace TicTacToe.Web.Models;

public record GameModel(
    string Id,
    Marker? CurrentPlayer,
    Marker?[] Board,
    bool IsComplete,
    Marker? Winner
)
{
    public static GameModel FromGame(string id, Game game) =>
        game switch
        {
            Game.InProgress g => new GameModel(
                Id: id,
                CurrentPlayer: g.CurrentPlayer,
                Board: g.Board.ToArray(),
                IsComplete: false,
                Winner: null
            ),
            Game.Winner g => new GameModel(
                Id: id,
                CurrentPlayer: null,
                Board: g.Board.ToArray(),
                IsComplete: true,
                Winner: g.WinningPlayer
            ),
            Game.Draw g => new GameModel(
                Id: id,
                CurrentPlayer: null,
                Board: g.Board.ToArray(),
                IsComplete: true,
                Winner: null
            ),
            _ => throw new ArgumentException("Unexpected game state", nameof(game)),
        };
}
