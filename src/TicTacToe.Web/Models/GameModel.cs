using TicTacToe.Engine;

namespace TicTacToe.Web.Models;

public record GameModel(
    Guid Id,
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
            _ => null,
        };

    private static Marker?[] FromBoard(GameBoard board) => board.Select(FromSquare).ToArray();

    private static Marker? GetCurrentPlayer(Game game) =>
        game switch
        {
            Game.InProgress g => g.Board[(Position)0] switch
            {
                Square.Available available => available.NextMarker,
                _ => g.Board.OfType<Square.Available>().FirstOrDefault()?.NextMarker,
            },
            _ => null,
        };

    public static GameModel FromGame(Guid id, Game game) =>
        game switch
        {
            Game.InProgress g => new(
                Id: id,
                Board: FromBoard(g.Board),
                CurrentPlayer: GetCurrentPlayer(game),
                IsComplete: false,
                Winner: null
            ),
            Game.Winner g => new(
                Id: id,
                Board: FromBoard(g.Board),
                CurrentPlayer: null,
                IsComplete: true,
                Winner: g.WinningPlayer
            ),
            Game.Draw g => new(
                Id: id,
                Board: FromBoard(g.Board),
                CurrentPlayer: null,
                IsComplete: true,
                Winner: null
            ),
            _ => throw new ArgumentException("Invalid game state", nameof(game)),
        };
}
