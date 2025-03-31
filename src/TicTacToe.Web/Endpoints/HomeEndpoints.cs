using RazorSlices;
using StarFederation.Datastar.DependencyInjection;
using TicTacToe.Web.Infrastructure;

namespace TicTacToe.Web.Endpoints;

public static class HomeEndpoints
{
    public static void MapHome(this IEndpointRouteBuilder endpoints)
    {
        // Landing page HTML
        endpoints
            .MapGet(
                "/",
                async (IGameRepository gameRepository, HttpContext context) =>
                {
                    var games = await gameRepository.GetGamesAsync();
                    return Results.Extensions.RazorSlice<
                        Slices.Index,
                        List<(Guid Id, Engine.Game Game)>
                    >(games.ToList());
                }
            )
            .RequireAuthorization();

        // Game list fragment with SSE
        endpoints
            .MapGet(
                "/games",
                async (IGameRepository repo, IDatastarServerSentEventService sse) =>
                {
                    var games = await repo.GetGamesAsync();
                    var slice = Slices._GameList.Create(games.ToList());
                    var fragment = await slice.RenderAsync();
                    await sse.MergeFragmentsAsync(fragment);
                }
            )
            .RequireAuthorization();

        // Create new game
        endpoints
            .MapPost(
                "/games",
                async (
                    IGameRepository repo,
                    IDatastarServerSentEventService sse,
                    HttpContext context
                ) =>
                {
                    var (id, game) = await repo.CreateGameAsync();
                    var slice = Slices._Game.Create((id, game));
                    var fragment = await slice.RenderAsync();
                    await sse.MergeFragmentsAsync(fragment);

                    // Also notify the game list that it needs to update
                    var games = await repo.GetGamesAsync();
                    var listSlice = Slices._GameList.Create(games.ToList());
                    var listFragment = await listSlice.RenderAsync();
                    await sse.MergeFragmentsAsync(listFragment);
                }
            )
            .RequireAuthorization();
    }
}
