using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicTacToe.Web.Infrastructure
{
    public class InMemoryPlayerRepository : IPlayerRepository
    {
        private readonly ConcurrentDictionary<Guid, Player> _players = new();
        private readonly ConcurrentDictionary<string, Guid> _emailIndex = new();

        public Task<Player?> GetByIdAsync(Guid id)
        {
            _players.TryGetValue(id, out var player);
            return Task.FromResult(player);
        }

        public Task<Player?> GetByEmailAsync(string email)
        {
            if (_emailIndex.TryGetValue(email.ToLowerInvariant(), out var id))
            {
                return GetByIdAsync(id);
            }
            return Task.FromResult<Player?>(null);
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

            // Check for email uniqueness if provided
            if (!string.IsNullOrEmpty(player.Email))
            {
                var normalizedEmail = player.Email.ToLowerInvariant();
                if (_emailIndex.ContainsKey(normalizedEmail))
                {
                    throw new InvalidOperationException($"Email {player.Email} is already registered");
                }
                _emailIndex[normalizedEmail] = player.Id;
            }

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

            // Handle email updates
            if (!string.IsNullOrEmpty(existingPlayer.Email))
            {
                _emailIndex.TryRemove(existingPlayer.Email.ToLowerInvariant(), out _);
            }
            if (!string.IsNullOrEmpty(player.Email))
            {
                var normalizedEmail = player.Email.ToLowerInvariant();
                if (_emailIndex.ContainsKey(normalizedEmail) && _emailIndex[normalizedEmail] != player.Id)
                {
                    throw new InvalidOperationException($"Email {player.Email} is already registered");
                }
                _emailIndex[normalizedEmail] = player.Id;
            }

            // Preserve creation time and stats
            player.CreatedAt = existingPlayer.CreatedAt;
            player.GamesPlayed = existingPlayer.GamesPlayed;
            player.GamesWon = existingPlayer.GamesWon;

            result = _players.TryUpdate(player.Id, player, existingPlayer);
            return Task.FromResult(result);
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            if (_players.TryRemove(id, out var player) && !string.IsNullOrEmpty(player.Email))
            {
                _emailIndex.TryRemove(player.Email.ToLowerInvariant(), out _);
            }
            return Task.FromResult(true);
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

        public Task<bool> RegisterPlayerAsync(Guid id, string email, string name, string passwordHash)
        {
            var normalizedEmail = email.ToLowerInvariant();
            if (_emailIndex.ContainsKey(normalizedEmail))
            {
                return Task.FromResult(false);
            }

            var success = false;
            _players.AddOrUpdate(
                id,
                (key) => null, // Should not add a new player
                (key, existingPlayer) =>
                {
                    existingPlayer.Email = email;
                    existingPlayer.Name = name;
                    existingPlayer.PasswordHash = passwordHash;
                    success = true;
                    return existingPlayer;
                });

            if (success)
            {
                _emailIndex[normalizedEmail] = id;
            }

            return Task.FromResult(success);
        }
    }
}
