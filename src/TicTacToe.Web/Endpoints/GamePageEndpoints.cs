using TicTacToe.Web.Infrastructure;

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
                var model = Slices.GameModel.FromGame(id, game);
                return Results.Extensions.RazorSlice<Slices.GameState, Slices.GameModel>(model);
            }
        );
    }
}
