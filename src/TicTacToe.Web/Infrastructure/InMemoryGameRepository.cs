using System.Collections.Concurrent;
using TicTacToe.Engine;

namespace TicTacToe.Web.Infrastructure;

public class InMemoryGameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<string, Game> _games = new();

    public Task<(string GameId, Game Game)> CreateGameAsync()
    {
        var gameId = Guid.NewGuid().ToString("N");
        var game = Game.Create();

        if (!_games.TryAdd(gameId, game))
        {
            // This should never happen with a GUID, but we handle it anyway
            throw new InvalidOperationException("Failed to create game: ID collision");
        }

        return Task.FromResult((gameId, game));
    }

    public Task<Game> GetGameAsync(string gameId)
    {
        if (!_games.TryGetValue(gameId, out var game))
        {
            throw new GameNotFoundException(gameId);
        }

        return Task.FromResult(game);
    }

    public Task<Game> UpdateGameAsync(string gameId, Game game)
    {
        if (!_games.TryGetValue(gameId, out var currentEntry))
        {
            throw new GameNotFoundException(gameId);
        }

        var oldEntry = currentEntry;

        if (!_games.TryUpdate(gameId, game, oldEntry))
        {
            throw new ConcurrencyException(gameId);
        }

        return Task.FromResult(game);
    }

    public Task DeleteGameAsync(string gameId)
    {
        _games.TryRemove(gameId, out _);
        return Task.CompletedTask;
    }
}
