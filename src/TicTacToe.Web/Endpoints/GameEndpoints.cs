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
        // Game state fragment
        endpoints.MapGet(
            "game/{id}",
            async (string id, IGameRepository repo, IDatastarServerSentEventService sse) =>
            {
                var game = await repo.GetGameAsync(id);
                var model = GameModel.FromGame(id, game);
                var slice = Slices.Game.Create(model);
                var fragment = await slice.RenderAsync();
                await sse.MergeFragmentsAsync(fragment);
            }
        );

        // Make a move
        endpoints.MapPost(
            "game/{id}",
            async (
                string id,
                byte position,
                IGameRepository repo,
                IDatastarServerSentEventService sse
            ) =>
            {
                var game = await repo.GetGameAsync(id);
                var updatedGame = Game.MakeMove(game, new Position(position));
                await repo.UpdateGameAsync(id, updatedGame);
                var model = GameModel.FromGame(id, game);
                var slice = Slices.Game.Create(model);
                var fragment = await slice.RenderAsync();
                await sse.MergeFragmentsAsync(fragment);
            }
        );
    }
}
