using RazorSlices;
using StarFederation.Datastar.DependencyInjection;
using TicTacToe.Engine;
using TicTacToe.Web.Infrastructure;
using TicTacToe.Web.Models;

namespace TicTacToe.Web.Endpoints;

public static class GameListEndpoints
{
    public static void MapGameList(this IEndpointRouteBuilder endpoints)
    {
        // Games list fragment
        endpoints.MapGet(
            "games",
            async (IGameRepository repo, IDatastarServerSentEventService sse) =>
            {
                // var games = await repo.GetGamesAsync();
                var games = new List<(string, Game)>();
                var model = games
                    .Select((game) => GameModel.FromGame(game.Item1, game.Item2))
                    .ToList();
                var slice = Slices.Home.Create(model);
                var fragment = await slice.RenderAsync();
                await sse.MergeFragmentsAsync(fragment);
            }
        );
    }
}
