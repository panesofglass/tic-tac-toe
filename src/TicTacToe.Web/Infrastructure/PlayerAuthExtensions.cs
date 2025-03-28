namespace TicTacToe.Web.Infrastructure;

public static class PlayerAuthExtensions
{
    private const string PlayerIdCookieName = "playerId";

    /// <summary>
    /// Gets the current player's ID from the auth cookie, if present
    /// </summary>
    public static Guid? GetCurrentPlayerId(this HttpContext context)
    {
        var playerIdStr = context.Request.Cookies[PlayerIdCookieName];
        return Guid.TryParse(playerIdStr, out var playerId) ? playerId : null;
    }

    /// <summary>
    /// Gets the current player from the auth cookie and repository, if present
    /// </summary>
    public static async Task<Player?> GetCurrentPlayerAsync(this HttpContext context)
    {
        var playerId = context.GetCurrentPlayerId();
        if (!playerId.HasValue)
            return null;

        var playerRepo = context.RequestServices.GetRequiredService<IPlayerRepository>();
        return await playerRepo.GetByIdAsync(playerId.Value);
    }

    /// <summary>
    /// Sets the auth cookie for the given player ID
    /// </summary>
    public static void SignInPlayer(this HttpContext context, Guid playerId)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromDays(30),
        };

        context.Response.Cookies.Append(PlayerIdCookieName, playerId.ToString(), cookieOptions);
    }

    /// <summary>
    /// Removes the player's auth cookie
    /// </summary>
    public static void SignOutPlayer(this HttpContext context)
    {
        context.Response.Cookies.Delete(PlayerIdCookieName);
    }

    /// <summary>
    /// Ensures the request has a valid player ID, creates a new player if needed
    /// </summary>
    public static async Task<Guid> EnsurePlayerAsync(
        this HttpContext context,
        IPlayerRepository playerRepository
    )
    {
        var playerId = context.GetCurrentPlayerId();
        if (playerId.HasValue)
        {
            var player = await playerRepository.GetByIdAsync(playerId.Value);
            if (player != null)
            {
                await playerRepository.UpdateLastActiveAsync(playerId.Value);
                return playerId.Value;
            }
        }

        // Create a new player and set the cookie
        var newPlayer = Player.Create();
        await playerRepository.CreateAsync(newPlayer);
        context.SignInPlayer(newPlayer.Id);
        return newPlayer.Id;
    }
}
