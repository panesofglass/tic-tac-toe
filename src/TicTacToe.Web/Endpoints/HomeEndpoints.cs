using TicTacToe.Engine;
using TicTacToe.Web.Infrastructure;
using TicTacToe.Web.Models;

namespace TicTacToe.Web.Endpoints;

public static class HomeEndpoints
{
    public static void MapHome(this IEndpointRouteBuilder endpoints)
    {
        // Landing page HTML
        endpoints.MapGet(
            "/",
            (IGameRepository gameRepository) =>
            {
                // var games = await gameRepository.GetGamesAsync();
                var games = new List<(string, Game)>();
                var model = games
                    .Select((game) => GameModel.FromGame(game.Item1, game.Item2))
                    .ToList();
                return Results.Extensions.RazorSlice<Slices.Index, List<GameModel>>(model);
            }
        );
    }
}
