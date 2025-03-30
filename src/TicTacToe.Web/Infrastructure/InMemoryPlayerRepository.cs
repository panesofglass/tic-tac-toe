using System.Collections.Concurrent;

namespace TicTacToe.Web.Infrastructure
{
    public class InMemoryPlayerRepository : IPlayerRepository
    {
        private readonly ConcurrentDictionary<Guid, Player> _players = new();
        private readonly ConcurrentDictionary<string, Guid> _emailIndex = new();

        public Task<Player?> GetByIdAsync(Guid id)
        {
            _players.TryGetValue(id, out var player);
            return Task.FromResult<Player?>(player);
        }

        public Task<Player?> GetByEmailAsync(string email)
        {
            if (_emailIndex.TryGetValue(email.ToLowerInvariant(), out var id))
            {
                return GetByIdAsync(id);
            }
            return Task.FromResult<Player?>(default);
        }

        public Task<IEnumerable<Player>> GetAllAsync()
        {
            return Task.FromResult(_players.Values.AsEnumerable());
        }

        public Task<bool> CreateAsync(Player player)
        {
            // Check for email uniqueness if provided
            var normalizedEmail = player.Email.ToLowerInvariant();
            if (!string.IsNullOrEmpty(player.Email))
            {
                if (_emailIndex.ContainsKey(normalizedEmail))
                {
                    throw new InvalidOperationException(
                        $"Email {player.Email} is already registered"
                    );
                }
            }

            // Treat the received player as a template and ensure it is created properly
            _emailIndex[normalizedEmail] = player.Id;

            if (!_players.TryAdd(player.Id, player))
                throw new InvalidOperationException($"Player with Id {player.Id} already exists");

            return Task.FromResult(true);
        }

        public Task<bool> UpdateAsync(Player player)
        {
            if (player == default)
                throw new ArgumentNullException(nameof(player));

            if (player.Id == Guid.Empty)
                throw new ArgumentException("Player Id cannot be empty", nameof(player));

            if (!_players.TryGetValue(player.Id, out var existingPlayer))
                return Task.FromResult(false);

            // Handle email updates
            if (!string.IsNullOrEmpty(existingPlayer.Email))
            {
                _emailIndex.TryRemove(existingPlayer.Email.ToLowerInvariant(), out _);
            }
            if (!string.IsNullOrEmpty(player.Email))
            {
                var normalizedEmail = player.Email.ToLowerInvariant();
                if (
                    _emailIndex.ContainsKey(normalizedEmail)
                    && _emailIndex[normalizedEmail] != player.Id
                )
                {
                    throw new InvalidOperationException(
                        $"Email {player.Email} is already registered"
                    );
                }
                _emailIndex[normalizedEmail] = player.Id;
            }

            // Preserve creation time and stats
            var updatePlayer = player with
            {
                CreatedAt = existingPlayer.CreatedAt,
            };

            var result = _players.TryUpdate(player.Id, player, existingPlayer);
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

            var matchingPlayers = _players
                .Values.Where(p =>
                    !String.IsNullOrEmpty(p.Name)
                    && p.Name.Contains(name, StringComparison.OrdinalIgnoreCase)
                )
                .ToList();

            return Task.FromResult(matchingPlayers.AsEnumerable());
        }

        public Task<bool> UpdateLastActiveAsync(Guid id)
        {
            if (!_players.TryGetValue(id, out var existing))
                return Task.FromResult(false);

            var nextPlayer = existing with { LastActive = DateTimeOffset.UtcNow };
            var result = _players.TryUpdate(id, nextPlayer, existing);
            return Task.FromResult(result);
        }
    }
}
