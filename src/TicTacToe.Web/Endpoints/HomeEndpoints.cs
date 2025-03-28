using RazorSlices;
using StarFederation.Datastar.DependencyInjection;
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
            async (IGameRepository gameRepository) =>
            {
                var games = await gameRepository.GetGamesAsync();
                var model = games.Select(g => GameModel.FromGame(g.id, g.game)).ToList();
                return Results.Extensions.RazorSlice<Slices.Index, List<GameModel>>(model);
            }
        );

        // Game list fragment with SSE
        endpoints.MapGet(
            "/page",
            async (IGameRepository repo, IDatastarServerSentEventService sse) =>
            {
                var games = await repo.GetGamesAsync();
                var model = games.Select(g => GameModel.FromGame(g.id, g.game)).ToList();
                var slice = Slices.GameList.Create(model);
                var fragment = await slice.RenderAsync();
                await sse.MergeFragmentsAsync(fragment);
            }
        );
    }
}
