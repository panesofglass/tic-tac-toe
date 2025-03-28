using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicTacToe.Web.Infrastructure
{
    /// <summary>
    /// Thread-safe in-memory implementation of IPlayerRepository
    /// </summary>
    public class InMemoryPlayerRepository : IPlayerRepository
    {
        private readonly ConcurrentDictionary<Guid, Player> _players = new();

        public Task<Player> GetByIdAsync(Guid id)
        {
            _players.TryGetValue(id, out var player);
            return Task.FromResult(player);
        }

        public Task<IEnumerable<Player>> GetAllAsync()
        {
            return Task.FromResult(_players.Values.AsEnumerable());
        }

        public Task<Player> CreateAsync(Player player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (player.Id == Guid.Empty)
                player.Id = Guid.NewGuid();

            if (string.IsNullOrEmpty(player.Name))
                player.Name = $"Player_{player.Id.ToString().Substring(0, 8)}";

            var now = DateTimeOffset.UtcNow;
            player.CreatedAt = now;
            player.LastActive = now;
            player.GamesPlayed = 0;
            player.GamesWon = 0;

            if (!_players.TryAdd(player.Id, player))
                throw new InvalidOperationException($"Player with Id {player.Id} already exists");

            return Task.FromResult(player);
        }

        public Task<bool> UpdateAsync(Player player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (player.Id == Guid.Empty)
                throw new ArgumentException("Player Id cannot be empty", nameof(player));

            var result = _players.TryGetValue(player.Id, out var existingPlayer);
            if (!result)
                return Task.FromResult(false);

            // Preserve creation time and stats
            player.CreatedAt = existingPlayer.CreatedAt;
            player.GamesPlayed = existingPlayer.GamesPlayed;
            player.GamesWon = existingPlayer.GamesWon;

            result = _players.TryUpdate(player.Id, player, existingPlayer);
            return Task.FromResult(result);
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            var result = _players.TryRemove(id, out _);
            return Task.FromResult(result);
        }

        public Task<IEnumerable<Player>> FindByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Task.FromResult(Enumerable.Empty<Player>());

            var matchingPlayers = _players.Values
                .Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Task.FromResult(matchingPlayers.AsEnumerable());
        }

        public Task<bool> UpdateLastActiveAsync(Guid id)
        {
            var result = _players.TryGetValue(id, out var player);
            if (!result)
                return Task.FromResult(false);

            player.LastActive = DateTimeOffset.UtcNow;
            return Task.FromResult(true);
        }

        public Task<bool> IncrementGamesPlayedAsync(Guid id)
        {
            var result = false;
            _players.AddOrUpdate(
                id,
                (key) => null, // Should not add a new player
                (key, existingPlayer) =>
                {
                    existingPlayer.GamesPlayed++;
                    result = true;
                    return existingPlayer;
                });

            return Task.FromResult(result);
        }

        public Task<bool> IncrementGamesWonAsync(Guid id)
        {
            var result = false;
            _players.AddOrUpdate(
                id,
                (key) => null, // Should not add a new player
                (key, existingPlayer) =>
                {
                    existingPlayer.GamesWon++;
                    result = true;
                    return existingPlayer;
                });

            return Task.FromResult(result);
        }
    }
}
