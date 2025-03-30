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
        endpoints.MapGet(
            "/focus/{id}",
            async (Guid id, IGameRepository repo, HttpContext context) =>
            {
                var game = await repo.GetGameAsync(id);
                var model = GameModel.FromGame(id, game);
                return Results.Extensions.RazorSlice<Slices.FocusGame, GameModel>(model);
            }
        );

        // Game state fragment with SSE
        endpoints.MapGet(
            "/game/{id}",
            async (Guid id, IGameRepository repo, IDatastarServerSentEventService sse) =>
            {
                var game = await repo.GetGameAsync(id);
                var model = GameModel.FromGame(id, game);
                var slice = Slices._Game.Create(model);
                var fragment = await slice.RenderAsync();
                await sse.MergeFragmentsAsync(fragment);
            }
        );

        // Create new game
        endpoints.MapPost(
            "/game",
            async (
                IGameRepository repo,
                IDatastarServerSentEventService sse,
                HttpContext context
            ) =>
            {
                var (id, game) = await repo.CreateGameAsync();
                var model = GameModel.FromGame(id, game);

                // Also notify the game list that it needs to update
                var listSlice = Slices._GameList.Create(
                    (await repo.GetGamesAsync())
                        .Select(g => GameModel.FromGame(g.id, g.game))
                        .ToList()
                );
                var listFragment = await listSlice.RenderAsync();
                await sse.MergeFragmentsAsync(listFragment);
            }
        );

        // Make a move
        endpoints.MapPost(
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
                var model = GameModel.FromGame(id, updatedGame);
                var slice = Slices._Game.Create(model);
                var fragment = await slice.RenderAsync();
                await sse.MergeFragmentsAsync(fragment);

                // Also update game list if game is complete
                if (model.IsComplete)
                {
                    var games = await repo.GetGamesAsync();
                    var listModel = games.Select(g => GameModel.FromGame(g.id, g.game)).ToList();
                    var listSlice = Slices._GameList.Create(listModel);
                    var listFragment = await listSlice.RenderAsync();
                    await sse.MergeFragmentsAsync(listFragment);
                }
            }
        );
    }
}
