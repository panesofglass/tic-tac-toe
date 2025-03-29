using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
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
builder.Services.AddAuthorization();
builder.Services.AddDatastar();
builder.Services.AddWebEncoders();

// Configure repositories and services
builder.Services.AddSingleton<IGameRepository, InMemoryGameRepository>();
builder.Services.AddSingleton<IPlayerRepository, InMemoryPlayerRepository>();
builder.Services.AddSingleton<IGamePlayerRepository, InMemoryGamePlayerRepository>();
builder.Services.AddSingleton<PasswordHasher>();

builder.Services.ConfigureHttpJsonOptions(options =>
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

app.UseAuthorization();

// Map endpoints
app.MapHome();
app.MapGame();
app.MapPlayer();

Console.WriteLine(
    $"RuntimeFeature.IsDynamicCodeSupported = {RuntimeFeature.IsDynamicCodeSupported}"
);
Console.WriteLine($"RuntimeFeature.IsDynamicCodeCompiled = {RuntimeFeature.IsDynamicCodeCompiled}");

app.Run();

public partial class Program { }
