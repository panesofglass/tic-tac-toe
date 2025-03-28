using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicTacToe.Engine;

namespace TicTacToe.Web.Infrastructure
{
    /// <summary>
    /// Repository interface for managing relationships between players and games.
    /// Handles player assignments and marker tracking.
    /// </summary>
    public interface IGamePlayerRepository
    {
        /// <summary>
        /// Assigns a player to a game with the specified marker.
        /// </summary>
        /// <param name='gameId'>The ID of the game</param>
        /// <param name='playerId'>The ID of the player</param>
        /// <param name='marker'>The marker (X or O) to assign to the player</param>
        /// <returns>True if assignment was successful, false otherwise</returns>
        Task<bool> AssignPlayerToGameAsync(Guid gameId, Guid playerId, Marker marker);
        
        /// <summary>
        /// Gets the player assigned to a specific marker in a game.
        /// </summary>
        /// <param name='gameId'>The ID of the game</param>
        /// <param name='marker'>The marker (X or O) to check</param>
        /// <returns>The player ID if found, null otherwise</returns>
        Task<Guid?> GetPlayerByMarkerAsync(Guid gameId, Marker marker);
        
        /// <summary>
        /// Gets the marker assigned to a player in a game.
        /// </summary>
        /// <param name='gameId'>The ID of the game</param>
        /// <param name='playerId'>The ID of the player</param>
        /// <returns>The assigned marker if found, null otherwise</returns>
        Task<Marker?> GetMarkerByPlayerAsync(Guid gameId, Guid playerId);
        
        /// <summary>
        /// Gets all players assigned to a game.
        /// </summary>
        /// <param name='gameId'>The ID of the game</param>
        /// <returns>Dictionary mapping markers to player IDs</returns>
        Task<Dictionary<Marker, Guid>> GetGamePlayersAsync(Guid gameId);
        
        /// <summary>
        /// Gets all games a player is participating in.
        /// </summary>
        /// <param name='playerId'>The ID of the player</param>
        /// <returns>List of game IDs the player is participating in</returns>
        Task<IEnumerable<Guid>> GetPlayerGamesAsync(Guid playerId);
        
        /// <summary>
        /// Removes all player assignments from a game (e.g., when resetting or deleting a game).
        /// </summary>
        /// <param name='gameId'>The ID of the game</param>
        /// <returns>True if removal was successful, false otherwise</returns>
        Task<bool> RemoveGameAssignmentsAsync(Guid gameId);
    }
}
