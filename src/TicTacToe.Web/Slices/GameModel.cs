namespace TicTacToe.Web.Slices;

public record GameModel(
    string Id,
    Models.Marker? CurrentPlayer,
    Models.Marker?[] Board,
    bool IsComplete,
    Models.Marker? Winner
)
{
    public static GameModel FromGame(string id, Models.Game game) =>
        game switch
        {
            Models.Game.InProgress g => new GameModel(
                Id: id,
                CurrentPlayer: g.CurrentPlayer,
                Board: g.Board.ToArray(),
                IsComplete: false,
                Winner: null
            ),
            Models.Game.Winner g => new GameModel(
                Id: id,
                CurrentPlayer: null,
                Board: g.Board.ToArray(),
                IsComplete: true,
                Winner: g.WinningPlayer
            ),
            Models.Game.Draw g => new GameModel(
                Id: id,
                CurrentPlayer: null,
                Board: g.Board.ToArray(),
                IsComplete: true,
                Winner: null
            ),
            _ => throw new ArgumentException("Unexpected game state", nameof(game)),
        };
}
