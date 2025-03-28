using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Engine;

namespace TicTacToe.Web.Infrastructure
{
    /// <summary>
    /// In-memory implementation of IGamePlayerRepository using ConcurrentDictionary for thread safety.
    /// </summary>
    public class InMemoryGamePlayerRepository : IGamePlayerRepository
    {
        // Inner class to represent assignments for a single game
        private class GamePlayerAssignment
        {
            // Dictionary mapping markers to player IDs
            public ConcurrentDictionary<Marker, Guid> PlayerAssignments { get; } = new();
        }

        // Dictionary mapping game IDs to their player assignments
        private readonly ConcurrentDictionary<Guid, GamePlayerAssignment> _gameAssignments = new();

        /// <inheritdoc />
        public Task<bool> AssignPlayerToGameAsync(Guid gameId, Guid playerId, Marker marker)
        {
            // Get or create game assignment
            var gameAssignment = _gameAssignments.GetOrAdd(gameId, _ => new GamePlayerAssignment());

            // Check if the marker is already assigned to another player
            foreach (var kvp in gameAssignment.PlayerAssignments)
            {
                if (kvp.Value == playerId && kvp.Key != marker)
                {
                    // Player already assigned with a different marker
                    return Task.FromResult(false);
                }
                
                if (kvp.Key == marker && kvp.Value != playerId)
                {
                    // Marker already assigned to another player
                    return Task.FromResult(false);
                }
            }
            
            // Add or update the assignment
            bool result = gameAssignment.PlayerAssignments.AddOrUpdate(
                marker, 
                playerId, 
                (_, _) => playerId) == playerId;

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<Guid?> GetPlayerByMarkerAsync(Guid gameId, Marker marker)
        {
            if (_gameAssignments.TryGetValue(gameId, out var gameAssignment))
            {
                if (gameAssignment.PlayerAssignments.TryGetValue(marker, out var playerId))
                {
                    return Task.FromResult<Guid?>(playerId);
                }
            }

            return Task.FromResult<Guid?>(null);
        }

        /// <inheritdoc />
        public Task<Marker?> GetMarkerByPlayerAsync(Guid gameId, Guid playerId)
        {
            if (_gameAssignments.TryGetValue(gameId, out var gameAssignment))
            {
                foreach (var kvp in gameAssignment.PlayerAssignments)
                {
                    if (kvp.Value == playerId)
                    {
                        return Task.FromResult<Marker?>(kvp.Key);
                    }
                }
            }

            return Task.FromResult<Marker?>(null);
        }

        /// <inheritdoc />
        public Task<Dictionary<Marker, Guid>> GetGamePlayersAsync(Guid gameId)
        {
            if (_gameAssignments.TryGetValue(gameId, out var gameAssignment))
            {
                // Create a new dictionary from the concurrent dictionary to avoid thread safety issues
                return Task.FromResult(gameAssignment.PlayerAssignments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            }

            return Task.FromResult(new Dictionary<Marker, Guid>());
        }

        /// <inheritdoc />
        public Task<IEnumerable<Guid>> GetPlayerGamesAsync(Guid playerId)
        {
            var games = new List<Guid>();

            foreach (var gameEntry in _gameAssignments)
            {
                if (gameEntry.Value.PlayerAssignments.Any(assignment => assignment.Value == playerId))
                {
                    games.Add(gameEntry.Key);
                }
            }

            return Task.FromResult<IEnumerable<Guid>>(games);
        }

        /// <inheritdoc />
        public Task<bool> RemoveGameAssignmentsAsync(Guid gameId)
        {
            return Task.FromResult(_gameAssignments.TryRemove(gameId, out _));
        }
    }
}
