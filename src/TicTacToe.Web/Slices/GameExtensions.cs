using Microsoft.AspNetCore.Html;
using TicTacToe.Engine;

namespace TicTacToe.Web.Slices;

public static class GameExtensions
{
    public static Marker? GetCurrentPlayer(this Game game) =>
        game switch
        {
            Game.InProgress g => g.Board[(Position)0] switch
            {
                Square.Available available => available.NextMarker,
                _ => g.Board.OfType<Square.Available>().FirstOrDefault()?.NextMarker,
            },
            _ => null,
        };

    public static HtmlString[] DrawGame(Guid id, Game game, Marker playerMarker)
    {
        var board = game switch
        {
            Game.InProgress g => g.Board,
            Game.Winner g => g.Board,
            Game.Draw g => g.Board,
            _ => throw new ArgumentException("Invalid game state", nameof(game)),
        };
        return DrawBoard(id, board, playerMarker);
    }

    public static HtmlString[] DrawBoard(Guid gameId, GameBoard board, Marker playerMarker) =>
        board.Select((s, i) => DrawSquare(gameId, s, (Position)i, playerMarker)).ToArray();

    private static HtmlString DrawSquare(
        Guid gameId,
        Square square,
        Position position,
        Marker playerMarker
    ) =>
        square switch
        {
            Square.Taken taken => new HtmlString(playerMarker.ToString()),
            Square.Available available when available.NextMarker == playerMarker => new HtmlString(
                $"""<div class="cell" data-on-click="@@post('/game/{gameId}/{position}')"></div>"""
            ),
            _ => new HtmlString(""),
        };
}
