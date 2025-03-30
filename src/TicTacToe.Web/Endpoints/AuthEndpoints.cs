using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using RazorSlices;
using StarFederation.Datastar.DependencyInjection;
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
                    RegisterModel model,
                    PasswordHasher passwordHasher,
                    IPlayerRepository playerRepository,
                    IDatastarServerSentEventService sse,
                    ILogger<IEndpointRouteBuilder> logger,
                    string returnUrl = "/"
                ) =>
                {
                    if (model == default)
                    {
                        var slice = Slices._RegisterForm.Create(
                            ("Register", "Invalid form data submitted")
                        );
                        var fragment = await slice.RenderAsync();
                        await sse.MergeFragmentsAsync(fragment);
                    }

                    var result = passwordHasher.ValidatePassword(model.Password);
                    if (!result.IsValid)
                    {
                        // TODO: return all field errors alongside their respective fields
                        var slice = Slices._RegisterForm.Create(("Register", result.Error));
                        var fragment = await slice.RenderAsync();
                        await sse.MergeFragmentsAsync(fragment);
                    }

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

                        await context.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            CreateClaimsPrincipal(player)
                        );
                        await sse.ExecuteScriptAsync($"""window.location.href = "{returnUrl}";""");
                    }
                    catch (InvalidOperationException ex)
                    {
                        // TODO: return all field errors alongside their respective fields
                        var slice = Slices._RegisterForm.Create(("Register", ex.Message));
                        var fragment = await slice.RenderAsync();
                        await sse.MergeFragmentsAsync(fragment);
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
                    LoginModel model,
                    PasswordHasher passwordHasher,
                    IPlayerRepository playerRepository,
                    IDatastarServerSentEventService sse,
                    ILogger<IEndpointRouteBuilder> logger,
                    string returnUrl = "/"
                ) =>
                {
                    if (model != default)
                    {
                        var slice = Slices._LoginForm.Create(
                            ("Login", "Invalid form data submitted")
                        );
                        var fragment = await slice.RenderAsync();
                        await sse.MergeFragmentsAsync(fragment);
                    }

                    var player = await playerRepository.GetByEmailAsync(model.Email);
                    if (player == default || !passwordHasher.VerifyPassword(player, model.Password))
                    {
                        var slice = Slices._LoginForm.Create(
                            ("Login", "Invalid email or password.")
                        );
                        var fragment = await slice.RenderAsync();
                        await sse.MergeFragmentsAsync(fragment);
                        return;
                    }

                    await context.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        CreateClaimsPrincipal(player)
                    );

                    await sse.ExecuteScriptAsync($"""window.location.href = "{returnUrl}";""");
                }
            )
            .AllowAnonymous();

        endpoints
            .MapPost(
                "/logout",
                async (HttpContext context, IDatastarServerSentEventService sse) =>
                {
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    await sse.MergeFragmentsAsync("""window.location.href = "/login";""");
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
