using System.Collections.Concurrent;
using TicTacToe.Engine;

namespace TicTacToe.Web.Infrastructure;

public class InMemoryGameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<Guid, Game> _games = new();

    public Task<(Guid id, Game game)> CreateGameAsync()
    {
        var gameId = Guid.NewGuid();
        var game = Game.Create();

        if (!_games.TryAdd(gameId, game))
        {
            // This should never happen with a GUID, but we handle it anyway
            throw new InvalidOperationException("Failed to create game: ID collision");
        }

        return Task.FromResult((gameId, game));
    }

    public Task<IEnumerable<(Guid id, Game game)>> GetGamesAsync()
    {
        var games = _games
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToList();
        return Task.FromResult<IEnumerable<(Guid id, Game game)>>(games);
    }

    public Task<Game> GetGameAsync(Guid gameId)
    {
        if (!_games.TryGetValue(gameId, out var game))
        {
            throw new GameNotFoundException(gameId);
        }

        return Task.FromResult(game);
    }

    public Task<Game> UpdateGameAsync(Guid gameId, Game game)
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

    public Task DeleteGameAsync(Guid gameId)
    {
        _games.TryRemove(gameId, out _);
        return Task.CompletedTask;
    }
}
