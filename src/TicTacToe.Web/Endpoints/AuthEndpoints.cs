using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
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
                (HttpContext context, IAntiforgery antiforgery, string? error = null) =>
                    Results.Extensions.RazorSlice<
                        Slices.Register,
                        (string Title, AntiforgeryTokenSet Token, string? Error)
                    >(("Register", antiforgery.GetAndStoreTokens(context), error))
            )
            .AllowAnonymous();

        endpoints
            .MapPost(
                "/register",
                async (
                    HttpContext context,
                    [FromForm] RegisterModel model,
                    IAntiforgery antiforgery,
                    PasswordHasher passwordHasher,
                    IPlayerRepository playerRepository,
                    ILogger<Slices.Register> logger,
                    string returnUrl = "/"
                ) =>
                {
                    logger.LogInformation("RYANTEST: Registering new user");
                    if (model == default)
                    {
                        return Results.Extensions.RazorSlice<
                            Slices.Register,
                            (string Title, AntiforgeryTokenSet Token, string? Error)
                        >(
                            (
                                "Register",
                                antiforgery.GetAndStoreTokens(context),
                                "Invalid form data submitted"
                            )
                        );
                    }

                    logger.LogInformation("RYANTEST: Validating password");
                    var result = passwordHasher.ValidatePassword(model.Password);
                    if (!result.IsValid)
                    {
                        return Results.Extensions.RazorSlice<
                            Slices.Register,
                            (string Title, AntiforgeryTokenSet Token, string? Error)
                        >(("Register", antiforgery.GetAndStoreTokens(context), result.Error));
                    }

                    logger.LogInformation("RYANTEST: Creating user");
                    var tempPlayer = Player.Create(
                        email: model.Email,
                        name: model.Name,
                        passwordHash: ""
                    );
                    var player = tempPlayer with
                    {
                        PasswordHash = passwordHasher.HashPassword(Player.Default, model.Password),
                    };
                    try
                    {
                        await playerRepository.CreateAsync(player);
                        logger.LogInformation("RYANTEST: Created user and signing in ...");

                        await context.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            CreateClaimsPrincipal(player)
                        );
                        return Results.Redirect(returnUrl);
                    }
                    catch (Exception ex)
                    {
                        return Results.Extensions.RazorSlice<
                            Slices.Register,
                            (string Title, AntiforgeryTokenSet Token, string? Error)
                        >(("Register", antiforgery.GetAndStoreTokens(context), ex.Message));
                    }
                }
            )
            .AllowAnonymous();

        endpoints
            .MapGet(
                "/login",
                (HttpContext context, IAntiforgery antiforgery, string? error = null) =>
                {
                    var token = antiforgery.GetAndStoreTokens(context);
                    return Results.Extensions.RazorSlice<
                        Slices.Login,
                        (string Title, AntiforgeryTokenSet Token, string? Error)
                    >(("Login", token, error));
                }
            )
            .AllowAnonymous();

        endpoints
            .MapPost(
                "/login",
                async (
                    HttpContext context,
                    [FromForm] LoginModel model,
                    IAntiforgery antiforgery,
                    PasswordHasher passwordHasher,
                    IPlayerRepository playerRepository,
                    ILogger<Slices.Login> logger,
                    string returnUrl = "/"
                ) =>
                {
                    if (model != default)
                    {
                        return Results.Extensions.RazorSlice<
                            Slices.Login,
                            (string Title, AntiforgeryTokenSet Token, string? Error)
                        >(
                            (
                                "Login",
                                antiforgery.GetAndStoreTokens(context),
                                "Invalid form data submitted"
                            )
                        );
                    }

                    var player = await playerRepository.GetByEmailAsync(model.Email);
                    if (player == default || !passwordHasher.VerifyPassword(player, model.Password))
                    {
                        return Results.Extensions.RazorSlice<
                            Slices.Login,
                            (string Title, AntiforgeryTokenSet Token, string? Error)
                        >(
                            (
                                "Login",
                                antiforgery.GetAndStoreTokens(context),
                                "Invalid email or password."
                            )
                        );
                    }

                    await context.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        CreateClaimsPrincipal(player)
                    );

                    return Results.Redirect(returnUrl);
                }
            )
            .AllowAnonymous();

        endpoints
            .MapPost(
                "/logout",
                async (HttpContext context) =>
                {
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return Results.Redirect("/login");
                }
            )
            .RequireAuthorization();
    }

    private static ClaimsPrincipal CreateClaimsPrincipal(Player player)
    {
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
            new Claim(ClaimTypes.Name, player.Name),
            new Claim(ClaimTypes.Email, player.Email),
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );
        return new ClaimsPrincipal(identity);
    }
}
