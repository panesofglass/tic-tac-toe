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
        /// Email address for login (optional)
        /// </summary>
        string Email,
        /// <summary>
        /// Display name of the player
        /// </summary>
        string Name,
        /// <summary>
        /// Hashed password for authentication (optional)
        /// </summary>
        string PasswordHash,
        /// <summary>
        /// Date and time when the player was created (UTC)
        /// </summary>
        DateTimeOffset CreatedAt,
        /// <summary>
        /// Date and time when the player was last active (UTC)
        /// </summary>
        DateTimeOffset LastActive
    )
    {
        /// <summary>
        /// Whether this is a registered user or anonymous player
        /// </summary>
        public bool IsRegistered => !string.IsNullOrEmpty(Email);

        /// <summary>
        /// Factory method to create a new Player.
        /// </summary>
        public static Player Create(string email, string name, string passwordHash)
        {
            var now = DateTimeOffset.UtcNow;
            return new Player(
                Id: Guid.NewGuid(),
                Email: email,
                Name: !String.IsNullOrEmpty(name)
                    ? name
                    : $"Player_{Guid.NewGuid().ToString().Substring(0, 8)}",
                PasswordHash: passwordHash,
                CreatedAt: now,
                LastActive: now
            );
        }

        public static Player Default = new Player(
            Id: Guid.Empty,
            Email: string.Empty,
            Name: string.Empty,
            PasswordHash: string.Empty,
            CreatedAt: default,
            LastActive: default
        );
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
        Task<bool> CreateAsync(Player player);

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
    }
}
