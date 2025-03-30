using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace TicTacToe.Web.Infrastructure;

public class PlayerClaimsTransformation : IClaimsTransformation
{
    private readonly IPlayerRepository _players;
    private readonly ILogger<PlayerClaimsTransformation> _logger;

    public PlayerClaimsTransformation(
        IPlayerRepository players,
        ILogger<PlayerClaimsTransformation> logger
    )
    {
        _players = players;
        _logger = logger;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Don't transform unauthenticated principals
        if (!principal.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogDebug("Skipping claims transformation for unauthenticated principal");
            return principal;
        }

        var playerId = principal.GetPlayerId();
        if (!playerId.HasValue)
        {
            _logger.LogWarning("No player ID claim found in authenticated principal");
            return principal;
        }

        _logger.LogDebug("Transforming claims for player {PlayerId}", playerId.Value);

        var player = await _players.GetByIdAsync(playerId.Value);
        if (player == null)
        {
            _logger.LogWarning("No player found for ID {PlayerId}", playerId.Value);
            return principal;
        }

        // Get existing claims that we want to preserve
        var existingClaims = principal.Claims.Where(c =>
            c.Type != ClaimTypes.NameIdentifier
            && c.Type != ClaimTypes.Name
            && c.Type != ClaimTypes.Email
        );

        var claims = new List<Claim>(existingClaims)
        {
            new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
            new Claim(ClaimTypes.Name, player.Name),
            new Claim(ClaimTypes.Email, player.Email),
        };

        var identity = new ClaimsIdentity(claims, principal.Identity?.AuthenticationType);
        var transformedPrincipal = new ClaimsPrincipal(identity);

        _logger.LogDebug("Claims transformation complete for player {PlayerId}", playerId.Value);
        return transformedPrincipal;
    }
}
