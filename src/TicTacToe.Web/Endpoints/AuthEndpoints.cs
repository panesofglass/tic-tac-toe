using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
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
                    UserManager<IdentityUser> userManager,
                    IUserStore<IdentityUser> userStore,
                    SignInManager<IdentityUser> signInManager,
                    IEmailSender _emailSender,
                    ILogger<RegisterModel> logger,
                    string returnUrl = "/"
                ) =>
                {
                    if (model == default)
                    {
                        return Results.Extensions.RazorSlice<
                            Slices.Login,
                            (string Title, string? Error)
                        >(("Register", "Invalid form data submitted"));
                    }

                    var user = new IdentityUser();

                    await userStore.SetUserNameAsync(user, model.Name, CancellationToken.None);
                    if (userManager.SupportsUserEmail)
                    {
                        var emailStore = (IUserEmailStore<IdentityUser>)userStore;
                        await emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);
                    }
                    var result = await userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        logger.LogInformation("User created a new account with password.");

                        var userId = await userManager.GetUserIdAsync(user);

                        // NOTE: ignoring email confirmation for now.

                        await signInManager.SignInAsync(user, isPersistent: false);
                        return Results.Redirect(returnUrl);
                    }
                    else
                    {
                        // TODO: return all field errors alongside their respective fields
                        return Results.Extensions.RazorSlice<
                            Slices.Register,
                            (string Title, string? Error)
                        >(
                            (
                                "Register",
                                string.Join(", ", result.Errors.Select(e => e.Description))
                            )
                        );
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
                    SignInManager<IdentityUser> signInManager,
                    string returnUrl = "/"
                ) =>
                {
                    if (model == default)
                    {
                        return Results.Extensions.RazorSlice<
                            Slices.Login,
                            (string Title, string? Error)
                        >(("Login", "Invalid form data submitted"));
                    }

                    var result = await signInManager.PasswordSignInAsync(
                        userName: model.Email,
                        password: model.Password,
                        isPersistent: false,
                        lockoutOnFailure: false
                    );
                    if (!result.Succeeded)
                    {
                        return Results.Extensions.RazorSlice<
                            Slices.Login,
                            (string Title, string? Error)
                        >(("Login", "Invalid email or password."));
                    }

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
