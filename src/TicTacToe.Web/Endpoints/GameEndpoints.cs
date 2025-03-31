using RazorSlices;
using StarFederation.Datastar.DependencyInjection;
using TicTacToe.Engine;
using TicTacToe.Web.Infrastructure;
using TicTacToe.Web.Models;

namespace TicTacToe.Web.Endpoints;

public static class GameEndpoints
{
    public static void MapGame(this IEndpointRouteBuilder endpoints)
    {
        // Full game page
        endpoints
            .MapGet(
                "/focus/{id}",
                async (Guid id, IGameRepository repo, HttpContext context) =>
                {
                    var game = await repo.GetGameAsync(id);
                    return Results.Extensions.RazorSlice<
                        Slices.FocusGame,
                        (Guid Id, Engine.Game Game)
                    >((id, game));
                }
            )
            .RequireAuthorization();

        // Game state fragment with SSE
        endpoints
            .MapGet(
                "/game/{id}",
                async (Guid id, IGameRepository repo, IDatastarServerSentEventService sse) =>
                {
                    var game = await repo.GetGameAsync(id);
                    var slice = Slices._Game.Create((id, game));
                    var fragment = await slice.RenderAsync();
                    await sse.MergeFragmentsAsync(fragment);
                }
            )
            .RequireAuthorization();

        // Make a move
        endpoints
            .MapPost(
                "/game/{id}",
                async (
                    Guid id,
                    MoveModel move,
                    IGameRepository repo,
                    IDatastarServerSentEventService sse
                ) =>
                {
                    var game = await repo.GetGameAsync(id);
                    var updatedGame = game.WithMove(
                        Move.Create(new Position(move.Position), move.Marker)
                    );
                    await repo.UpdateGameAsync(id, updatedGame);

                    // Send updated game state
                    var slice = Slices._Game.Create((id, updatedGame));
                    var fragment = await slice.RenderAsync();
                    await sse.MergeFragmentsAsync(fragment);

                    // Also update game list if game is complete
                    if (
                        updatedGame switch
                        {
                            Game.InProgress => false,
                            _ => true,
                        }
                    )
                    {
                        var games = await repo.GetGamesAsync();
                        var listSlice = Slices._GameList.Create(games.ToList());
                        var listFragment = await listSlice.RenderAsync();
                        await sse.MergeFragmentsAsync(listFragment);
                    }
                }
            )
            .RequireAuthorization();
    }
}
