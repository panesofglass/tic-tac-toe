using Microsoft.AspNetCore.Http;
using System;

namespace TicTacToe.Web.Infrastructure
{
    /// <summary>
    /// Interface for managing player session identifiers
    /// </summary>
    public interface IPlayerSession
    {
        /// <summary>
        /// Gets the current player ID from the session or creates a new one if it doesn't exist
        /// </summary>
        /// <returns>The player's unique identifier</returns>
        string GetOrCreatePlayerId();

        /// <summary>
        /// Gets the current player ID from the session
        /// </summary>
        /// <returns>The player's unique identifier, or null if not set</returns>
        string? GetPlayerId();
    }

    /// <summary>
    /// Implementation of IPlayerSession that uses ASP.NET Core session state
    /// </summary>
    public class PlayerSession : IPlayerSession
    {
        private const string PlayerIdKey = "PlayerId";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PlayerSession(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public string GetOrCreatePlayerId()
        {
            var session = _httpContextAccessor.HttpContext?.Session 
                ?? throw new InvalidOperationException("HttpContext or Session is not available");

            var playerId = session.GetString(PlayerIdKey);
            
            if (string.IsNullOrEmpty(playerId))
            {
                playerId = Guid.NewGuid().ToString();
                session.SetString(PlayerIdKey, playerId);
            }

            return playerId;
        }

        public string? GetPlayerId()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.GetString(PlayerIdKey);
        }
    }

    /// <summary>
    /// Extension methods for registering PlayerSession services
    /// </summary>
    public static class PlayerSessionExtensions
    {
        /// <summary>
        /// Adds the PlayerSession service to the service collection
        /// </summary>
        public static IServiceCollection AddPlayerSession(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IPlayerSession, PlayerSession>();
            return services;
        }
    }
}

