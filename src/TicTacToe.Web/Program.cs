using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using StarFederation.Datastar.DependencyInjection;
using TicTacToe.Engine;
using TicTacToe.Web.Endpoints;
using TicTacToe.Web.Infrastructure;

#if DEBUG
// Use the default builder during inner-loop so Hot Reload works
var builder = WebApplication.CreateBuilder(args);
#else
// Use the slim builder for Release builds
var builder = WebApplication.CreateSlimBuilder(args);
builder.WebHost.UseKestrelHttpsConfiguration();
#endif

// Add datastar services
builder.Services.AddWebEncoders().AddDatastar();

// Configure auth
builder
    .Services.AddAuthorization()
    .AddAntiforgery()
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "playerId";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ReturnUrlParameter = "returnUrl";
        options.SlidingExpiration = true;
    });

// Configure repositories and services
builder
    .Services.AddSingleton<IGameRepository, InMemoryGameRepository>()
    .AddSingleton<IPlayerRepository, InMemoryPlayerRepository>()
    .AddSingleton<IGamePlayerRepository, InMemoryGamePlayerRepository>()
    .AddSingleton<IClaimsTransformation, PlayerClaimsTransformation>()
    .AddSingleton<PasswordHasher>()
    .ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.TypeInfoResolverChain.Insert(0, TicTacToeJsonContext.Default);
        // Type parameter is required for AOT compilation support
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter<Marker>());
    });

var app = builder.Build();

// Configure static file serving
app.UseHttpsRedirection();
app.UseStatusCodePages();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Map endpoints
app.MapAuth();
app.MapHome();
app.MapGame();

Console.WriteLine(
    $"RuntimeFeature.IsDynamicCodeSupported = {RuntimeFeature.IsDynamicCodeSupported}"
);
Console.WriteLine($"RuntimeFeature.IsDynamicCodeCompiled = {RuntimeFeature.IsDynamicCodeCompiled}");

app.Run();

public partial class Program { }
