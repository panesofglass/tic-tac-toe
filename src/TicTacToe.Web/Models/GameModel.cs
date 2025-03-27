using TicTacToe.Engine;

namespace TicTacToe.Web.Models;

public record GameModel(
    string Id,
    Marker?[] Board,
    Marker? CurrentPlayer,
    bool IsComplete,
    Marker? Winner
)
{
    private static Marker? FromSquare(Square space) =>
        space switch
        {
            Square.Taken taken => taken.Marker,
            _ => (Marker?)null,
        };

    private static Marker?[] FromGameBoard(GameBoard board) =>
        board.Select(FromSquare).ToArray();

    private static Marker? FindCurrentPlayer(GameBoard board)
    {
        var available = board.FirstOrDefault((space) => space is Square.Available);
        return available == null ? null : ((Square.Available)available).NextMarker;
    }

    public static GameModel FromGame(string id, Game game) =>
        game switch
        {
            Game.InProgress g => new GameModel(
                Id: id,
                CurrentPlayer: FindCurrentPlayer(g.Board),
                Board: FromGameBoard(g.Board),
                IsComplete: false,
                Winner: null
            ),
            Game.Winner g => new GameModel(
                Id: id,
                CurrentPlayer: null,
                Board: FromGameBoard(g.Board),
                IsComplete: true,
                Winner: g.WinningPlayer
            ),
            Game.Draw g => new GameModel(
                Id: id,
                CurrentPlayer: null,
                Board: FromGameBoard(g.Board),
                IsComplete: true,
                Winner: null
            ),
            _ => throw new ArgumentException("Unexpected game state", nameof(game)),
        };
}
