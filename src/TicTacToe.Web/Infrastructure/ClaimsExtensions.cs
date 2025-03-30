using System.Security.Claims;

namespace TicTacToe.Web.Infrastructure;

public static class ClaimsExtensions
{
    public static Guid? GetPlayerId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }

    public static string? GetPlayerName(this ClaimsPrincipal principal) =>
        principal.FindFirst(ClaimTypes.Name)?.Value;

    public static string? GetPlayerEmail(this ClaimsPrincipal principal) =>
        principal.FindFirst(ClaimTypes.Email)?.Value;
}
