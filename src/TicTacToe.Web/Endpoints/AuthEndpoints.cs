using TicTacToe.Web.Infrastructure;
using TicTacToe.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
namespace TicTacToe.Web.Endpoints;

public static class AuthEndpoints
{
    internal static async Task<IResult> HandleRegistrationAsync(
        HttpContext context,
        IPlayerRepository players,
        PasswordHasher passwordHasher,
        string redirectUrl = "/")
    {
        var model = await RegisterModel.BindAsync(context);
        if (model is null)
        {
            return Results.Extensions.RazorSlice<
                Slices.Register,
                (string Title, string Error)
            >(("Register", "Invalid form data submitted"));
        }

        var validationResult = passwordHasher.ValidatePassword(model.Password);
        if (!validationResult.IsValid)
        {
            return Results.Extensions.RazorSlice<
                Slices.Register,
                (string Title, string? Error)
            >(("Register", validationResult.Error));
        }

        var existingPlayer = await players.GetByEmailAsync(model.Email);
        if (existingPlayer != null)
        {
            return Results.Extensions.RazorSlice<
                Slices.Register,
                (string Title, string? Error)
            >(("Register", "This email is already registered."));
        }

        var player = Player.Create();
        var passwordHash = passwordHasher.HashPassword(player, model.Password);

        try
        {
            var success = await players.RegisterPlayerAsync(
                player.Id,
                model.Email,
                model.Name,
                passwordHash
            );

            if (success)
            {
                player = await players.GetByIdAsync(player.Id);
                if (player != null)
                {
                    // Create claims for the newly registered player
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
                        new Claim(ClaimTypes.Name, player.Name ?? string.Empty),
                        new Claim("IsRegistered", "true"),
                        new Claim("GamesPlayed", player.GamesPlayed.ToString()),
                        new Claim("GamesWon", player.GamesWon.ToString())
                    };

                    // Add email claim if available
                    if (!string.IsNullOrEmpty(player.Email))
                    {
                        claims.Add(new Claim(ClaimTypes.Email, player.Email));
                    }

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await context.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                        });
                }
                
                return Results.Redirect(redirectUrl ?? "/");
            }

            return Results.Extensions.RazorSlice<
                Slices.Register,
                (string Title, string? Error)
            >(("Register", "Registration failed. Please try again."));
        }
        catch (Exception ex)
        {
            return Results.Extensions.RazorSlice<
                Slices.Register,
                (string Title, string? Error)
            >(("Register", $"Registration failed: {ex.Message}"));
        }
    }

    public static void MapAuth(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet(
                "/register",
                (HttpContext context, string? error = null) =>
                    Results.Extensions.RazorSlice<Slices.Register, (string Title, string? Error)>(
                        ("Register", error)
                    )
            )
            .AllowAnonymous();

        endpoints
            .MapPost(
                "/register",
                async (
                    HttpContext context,
                    IPlayerRepository players,
                    PasswordHasher passwordHasher,
                    string redirectUrl = "/"
                ) => await HandleRegistrationAsync(context, players, passwordHasher, redirectUrl)
            )
            .AllowAnonymous();

        endpoints
            .MapGet(
                "/login",
                (HttpContext context, string? error = null) =>
                    Results.Extensions.RazorSlice<Slices.Login, (string Title, string? Error)>(
                        ("Login", error)
                    )
            )
            .AllowAnonymous();

        endpoints
            .MapPost(
                "/login",
                async (
                    HttpContext context,
                    IPlayerRepository players,
                    PasswordHasher passwordHasher,
                    string returnUrl = "/"
                ) =>
                {
                    var model = await LoginModel.BindAsync(context);
                    if (model is null)
                    {
                        return Results.Extensions.RazorSlice<
                            Slices.Login,
                            (string Title, string? Error)
                        >(("Login", "Invalid form data submitted"));
                    }

                    var player = await players.GetByEmailAsync(model.Email);
                    if (
                        player == null
                        || String.IsNullOrEmpty(player.PasswordHash) // This is for the case when the player.PasswordHash == null
                        || !passwordHasher.VerifyPassword(
                            player,
                            model.Password,
                            player.PasswordHash
                        )
                    )
                    {
                        return Results.Extensions.RazorSlice<
                            Slices.Login,
                            (string Title, string? Error)
                        >(("Login", "Invalid email or password."));
                    }

                    // Create claims for the player
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
                        new Claim(ClaimTypes.Name, player.Name ?? string.Empty),
                        new Claim("IsRegistered", player.IsRegistered.ToString()),
                        new Claim("GamesPlayed", player.GamesPlayed.ToString()),
                        new Claim("GamesWon", player.GamesWon.ToString())
                    };

                    // Add email claim if available
                    if (!string.IsNullOrEmpty(player.Email))
                    {
                        claims.Add(new Claim(ClaimTypes.Email, player.Email));
                    }

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await context.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                        });

                    return Results.Redirect(returnUrl);
                }
            )
            .AllowAnonymous();

        endpoints.MapPost(
            "/logout",
            async (HttpContext context) =>
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.Redirect("/");
            }
        );
    }
}
