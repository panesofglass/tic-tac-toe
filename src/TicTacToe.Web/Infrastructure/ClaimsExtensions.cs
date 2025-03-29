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

    public static bool IsRegistered(this ClaimsPrincipal principal) =>
        bool.TryParse(principal.FindFirst("IsRegistered")?.Value, out var isRegistered)
        && isRegistered;

    public static int GetGamesPlayed(this ClaimsPrincipal principal) =>
        int.TryParse(principal.FindFirst("GamesPlayed")?.Value, out var count) ? count : 0;

    public static int GetGamesWon(this ClaimsPrincipal principal) =>
        int.TryParse(principal.FindFirst("GamesWon")?.Value, out var count) ? count : 0;

    public static Player ToPlayer(this ClaimsPrincipal principal)
    {
        var id = principal.GetPlayerId();
        if (!id.HasValue)
            throw new InvalidOperationException("Player ID claim not found");

        return Player.Create(
            id: id.Value,
            name: principal.GetPlayerName(),
            email: principal.GetPlayerEmail()
        );
    }
}
