using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using RazorSlices;
using StarFederation.Datastar.DependencyInjection;
using TicTacToe.Web.Models;

namespace TicTacToe.Web.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuth(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/register",
            (HttpContext context, string? error = null) =>
                Results.Extensions.RazorSlice<Slices.Register, (string Title, string? Error)>(
                    ("Register", error)
                )
        );

        endpoints.MapPost(
            "/register",
            async (
                HttpContext context,
                RegisterModel model,
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

                var user = new IdentityUser();

                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    logger.LogInformation("RYANTEST: User created a new account with password.");
                    var userId = await userManager.GetUserIdAsync(user);
                    // NOTE: ignoring email confirmation for now.
                    logger.LogInformation("RYANTEST: Signing in.");
                    await signInManager.SignInAsync(user, isPersistent: false);
                    logger.LogInformation("RYANTEST: User signed in.");
                    await sse.ExecuteScriptAsync($"""window.location.href = "{returnUrl}";""");
                }
                else
                {
                    logger.LogError($"RYANTEST: Errors: ${result.Errors}");
                    // TODO: return all field errors alongside their respective fields
                    var slice = Slices._RegisterForm.Create(
                        ("Register", string.Join(", ", result.Errors.Select(e => e.Description)))
                    );
                    var fragment = await slice.RenderAsync();
                    await sse.MergeFragmentsAsync(fragment);
                }
            }
        );

        endpoints.MapGet(
            "/login",
            (HttpContext context, string? error = null) =>
                Results.Extensions.RazorSlice<Slices.Login, (string Title, string? Error)>(
                    ("Login", error)
                )
        );

        endpoints.MapPost(
            "/login",
            async (
                HttpContext context,
                LoginModel model,
                IDatastarServerSentEventService sse,
                ILogger<IEndpointRouteBuilder> logger,
                string returnUrl = "/"
            ) =>
            {
                if (model != default)
                {
                    var slice = Slices._LoginForm.Create(("Login", "Invalid form data submitted"));
                    var fragment = await slice.RenderAsync();
                    await sse.MergeFragmentsAsync(fragment);
                }

                var result = await signInManager.PasswordSignInAsync(
                    userName: model.Email,
                    password: model.Password,
                    isPersistent: false,
                    lockoutOnFailure: false
                );

                if (!result.Succeeded)
                {
                    var slice = Slices._LoginForm.Create(("Login", "Invalid email or password."));
                    var fragment = await slice.RenderAsync();
                    await sse.MergeFragmentsAsync(fragment);
                }

                await sse.ExecuteScriptAsync($"""window.location.href = "{returnUrl}";""");
            }
        );

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
}
