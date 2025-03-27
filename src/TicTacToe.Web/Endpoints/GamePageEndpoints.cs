using TicTacToe.Engine;
using TicTacToe.Web.Infrastructure;
using TicTacToe.Web.Models;

namespace TicTacToe.Web.Endpoints;

public static class GamePageEndpoints
{
    public static void MapGamePage(this IEndpointRouteBuilder endpoints)
    {
        // Game page HTML
        endpoints.MapGet(
            "{id}",
            async (string id, IGameRepository gameRepository) =>
            {
                var game = await gameRepository.GetGameAsync(id);
                var model = GameModel.FromGame(id, game);
                return Results.Extensions.RazorSlice<Slices.GameState, GameModel>(model);
            }
        );
    }
}
