using TicTacToe.Web.Infrastructure;
using TicTacToe.Web.Models;

namespace TicTacToe.Web.Endpoints;

public static class AuthEndpoints
{
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
                ) =>
                {
                    var model = await RegisterModel.BindAsync(context);
                    if (model is null)
                    {
                        return Results.Extensions.RazorSlice<
                            Slices.Register,
                            (string Title, string Error)
                        >(("Register", "Invalid form data submitted"));
                    }

                    var playerId = context.GetCurrentPlayerId();
                    if (!playerId.HasValue)
                    {
                        return Results.Extensions.RazorSlice<
                            Slices.Register,
                            (string Title, string Error)
                        >(("Register", "No player ID found. Please enable cookies and try again."));
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

                    var player = Player.Create(id: playerId.Value);
                    var passwordHash = passwordHasher.HashPassword(player, model.Password);

                    try
                    {
                        var success = await players.RegisterPlayerAsync(
                            playerId.Value,
                            model.Email,
                            model.Name,
                            passwordHash
                        );

                        if (success)
                        {
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

                    context.SignInPlayer(player.Id);
                    return Results.Redirect(returnUrl);
                }
            )
            .AllowAnonymous();

        endpoints.MapPost(
            "/logout",
            (HttpContext context) =>
            {
                context.SignOutPlayer();
                return Results.Redirect("/");
            }
        );
    }
}
