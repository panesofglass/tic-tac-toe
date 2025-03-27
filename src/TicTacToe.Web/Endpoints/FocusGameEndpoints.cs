using TicTacToe.Web.Infrastructure;
using TicTacToe.Web.Models;

namespace TicTacToe.Web.Endpoints;

public static class FocusGameEndpoints
{
    public static void MapFocusGame(this IEndpointRouteBuilder endpoints)
    {
        // Game page HTML
        endpoints.MapGet(
            "{id}",
            async (string id, IGameRepository gameRepository) =>
            {
                var game = await gameRepository.GetGameAsync(id);
                var model = GameModel.FromGame(id, game);
                return Results.Extensions.RazorSlice<Slices.FocusGame, GameModel>(model);
            }
        );
    }
}
