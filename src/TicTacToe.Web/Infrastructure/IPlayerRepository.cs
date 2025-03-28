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
        /// <param name='id'>The player's unique identifier</param>
        /// <returns>The player if found, otherwise null</returns>
        Task<Player> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets all players in the system
        /// </summary>
        /// <returns>A collection of all players</returns>
        Task<IEnumerable<Player>> GetAllAsync();

        /// <summary>
        /// Creates a new player
        /// </summary>
        /// <param name='player'>The player to create</param>
        /// <returns>The created player with assigned Id</returns>
        Task<Player> CreateAsync(Player player);

        /// <summary>
        /// Updates an existing player
        /// </summary>
        /// <param name='player'>The player to update</param>
        /// <returns>True if the update was successful, otherwise false</returns>
        Task<bool> UpdateAsync(Player player);

        /// <summary>
        /// Deletes a player by their unique identifier
        /// </summary>
        /// <param name='id'>The player's unique identifier</param>
        /// <returns>True if the deletion was successful, otherwise false</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Finds players by name (partial match)
        /// </summary>
        /// <param name='name'>The name to search for</param>
        /// <returns>A collection of players matching the search criteria</returns>
        Task<IEnumerable<Player>> FindByNameAsync(string name);

        /// <summary>
        /// Updates a player's last active timestamp
        /// </summary>
        /// <param name='id'>The player's unique identifier</param>
        /// <returns>True if the update was successful, otherwise false</returns>
        Task<bool> UpdateLastActiveAsync(Guid id);

        /// <summary>
        /// Increments a player's games played count
        /// </summary>
        /// <param name='id'>The player's unique identifier</param>
        /// <returns>True if the update was successful, otherwise false</returns>
        Task<bool> IncrementGamesPlayedAsync(Guid id);

        /// <summary>
        /// Increments a player's games won count
        /// </summary>
        /// <param name='id'>The player's unique identifier</param>
        /// <returns>True if the update was successful, otherwise false</returns>
        Task<bool> IncrementGamesWonAsync(Guid id);
    }
}
