namespace TicTacToe.Web.Infrastructure
{
    /// <summary>
    /// Represents a player in the Tic-Tac-Toe game
    /// </summary>
    public record Player(
        /// <summary>
        /// Unique identifier for the player
        /// </summary>
        Guid Id,
        /// <summary>
        /// Display name of the player
        /// </summary>
        string? Name,
        /// <summary>
        /// Email address for login (optional)
        /// </summary>
        string? Email,
        /// <summary>
        /// Hashed password for authentication (optional)
        /// </summary>
        string? PasswordHash,
        /// <summary>
        /// Date and time when the player was created (UTC)
        /// </summary>
        DateTimeOffset CreatedAt,
        /// <summary>
        /// Date and time when the player was last active (UTC)
        /// </summary>
        DateTimeOffset LastActive,
        /// <summary>
        /// Number of games played by the player
        /// </summary>
        int GamesPlayed,
        /// <summary>
        /// Number of games won by the player
        /// </summary>
        int GamesWon
    )
    {
        /// <summary>
        /// Whether this is a registered user or anonymous player
        /// </summary>
        public bool IsRegistered => !string.IsNullOrEmpty(Email);

        /// <summary>
        /// Factory method to create a new Player.
        /// </summary>
        public static Player Create(
            Guid? id = null,
            string? name = null,
            string? email = null,
            string? passwordHash = null
        )
        {
            var now = DateTimeOffset.UtcNow;
            return new Player(
                id ?? Guid.NewGuid(),
                name ?? $"Player_{Guid.NewGuid().ToString().Substring(0, 8)}",
                email,
                passwordHash,
                now,
                now,
                0,
                0
            );
        }
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
