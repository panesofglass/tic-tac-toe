using Microsoft.AspNetCore.Identity;
using RazorSlices;
using TicTacToe.Web.Infrastructure;
using TicTacToe.Web.Models;
using static TicTacToe.Web.Models.AuthModels;

namespace TicTacToe.Web.Endpoints;

public static class PlayerEndpoints
{
    public static void MapPlayer(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/register", async (HttpContext context, string? error = null) =>
            Results.Extensions.RazorSlice<Slices.Register>(("Register", error))
        );

        endpoints.MapPost("/register", async (
            HttpContext context,
            IPlayerRepository players,
            PasswordHasher passwordHasher) =>
        {
            var model = await RegisterModel.BindAsync(context);
            if (model is null)
            {
                return Results.Extensions.RazorSlice<Slices.Register>(
                    ("Register", "Invalid form data submitted"));
            }

            var playerId = context.GetCurrentPlayerId();
            if (!playerId.HasValue)
            {
                return Results.Extensions.RazorSlice<Slices.Register>(
                    ("Register", "No player ID found. Please enable cookies and try again."));
            }

            var validationResult = passwordHasher.ValidatePassword(model.Password);
            if (!validationResult.IsValid)
            {
                return Results.Extensions.RazorSlice<Slices.Register>(("Register", validationResult.Error));
            }

            var existingPlayer = await players.GetByEmailAsync(model.Email);
            if (existingPlayer != null)
            {
                return Results.Extensions.RazorSlice<Slices.Register>(
                    ("Register", "This email is already registered."));
            }

            var player = new Player { Id = playerId.Value };
            var passwordHash = passwordHasher.HashPassword(player, model.Password);

            try
            {
                var success = await players.RegisterPlayerAsync(
                    playerId.Value,
                    model.Email,
                    model.Name,
                    passwordHash);

                if (success)
                {
                    return Results.Redirect("/");
                }
                
                return Results.Extensions.RazorSlice<Slices.Register>(
                    ("Register", "Registration failed. Please try again."));
            }
            catch (Exception ex)
            {
                return Results.Extensions.RazorSlice<Slices.Register>(
                    ("Register", $"Registration failed: {ex.Message}"));
            }
        });

        endpoints.MapGet("/login", async (HttpContext context, string? error = null) =>
            Results.Extensions.RazorSlice<Slices.Login>(("Login", error))
        );

        endpoints.MapPost("/login", async (
            HttpContext context,
            IPlayerRepository players,
            PasswordHasher passwordHasher) =>
        {
            var model = await LoginModel.BindAsync(context);
            if (model is null)
            {
                return Results.Extensions.RazorSlice<Slices.Login>(
                    ("Login", "Invalid form data submitted"));
            }

            var player = await players.GetByEmailAsync(model.Email);
            if (player == null || !passwordHasher.VerifyPassword(player, model.Password, player.PasswordHash))
            {
                return Results.Extensions.RazorSlice<Slices.Login>(
                    ("Login", "Invalid email or password."));
            }

            context.SignInPlayer(player.Id);
            return Results.Redirect("/");
        });

        endpoints.MapPost("/logout", (HttpContext context) =>
        {
            context.SignOutPlayer();
            return Results.Redirect("/");
        });
    }
}
