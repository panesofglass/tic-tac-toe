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
                var model = GameModel.FromGame(id, game);
                var slice = Slices.Game.Create(model);
                var fragment = await slice.RenderAsync();
                await sse.MergeFragmentsAsync(fragment);
            }
        );
    }
}
