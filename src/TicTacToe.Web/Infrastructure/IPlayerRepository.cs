using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TicTacToe.Web.Infrastructure
{
    /// <summary>
    /// Represents a player in the Tic-Tac-Toe game
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Unique identifier for the player
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Display name of the player
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Email address for login (optional)
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Hashed password for authentication (optional)
        /// </summary>
        public string? PasswordHash { get; set; }

        /// <summary>
        /// Whether this is a registered user or anonymous player
        /// </summary>
        public bool IsRegistered => !string.IsNullOrEmpty(Email);

        /// <summary>
        /// Date and time when the player was created (UTC)
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Date and time when the player was last active (UTC)
        /// </summary>
        public DateTimeOffset LastActive { get; set; }

        /// <summary>
        /// Number of games played by the player
        /// </summary>
        public int GamesPlayed { get; set; }

        /// <summary>
        /// Number of games won by the player
        /// </summary>
        public int GamesWon { get; set; }
    }

    /// <summary>
    /// Repository interface for managing player data
    /// </summary>
    public interface IPlayerRepository
    {
        /// <summary>
        /// Gets a player by their unique identifier
        /// </summary>
        Task<Player?> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets a player by their email address
        /// </summary>
        Task<Player?> GetByEmailAsync(string email);

        /// <summary>
        /// Gets all players in the system
        /// </summary>
        Task<IEnumerable<Player>> GetAllAsync();

        /// <summary>
        /// Creates a new player
        /// </summary>
        Task<Player> CreateAsync(Player player);

        /// <summary>
        /// Updates an existing player
        /// </summary>
        Task<bool> UpdateAsync(Player player);

        /// <summary>
        /// Deletes a player by their unique identifier
        /// </summary>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Finds players by name (partial match)
        /// </summary>
        Task<IEnumerable<Player>> FindByNameAsync(string name);

        /// <summary>
        /// Updates a player's last active timestamp
        /// </summary>
        Task<bool> UpdateLastActiveAsync(Guid id);

        /// <summary>
        /// Increments a player's games played count
        /// </summary>
        Task<bool> IncrementGamesPlayedAsync(Guid id);

        /// <summary>
        /// Increments a player's games won count
        /// </summary>
        Task<bool> IncrementGamesWonAsync(Guid id);

        /// <summary>
        /// Registers an anonymous player with email and password
        /// </summary>
        Task<bool> RegisterPlayerAsync(Guid id, string email, string name, string passwordHash);
    }
}
