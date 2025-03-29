using System.Text.Json.Serialization;
using StarFederation.Datastar.DependencyInjection;
using TicTacToe.Engine;
using TicTacToe.Web.Endpoints;
using TicTacToe.Web.Infrastructure;
using TicTacToe.Web.Models;

var builder = WebApplication.CreateBuilder(args);

// Add datastar services
builder.Services.AddAuthorization();
builder.Services.AddWebEncoders();
builder.Services.AddDatastar();

// Configure repositories and services
builder.Services.AddSingleton<IGameRepository, InMemoryGameRepository>();
builder.Services.AddSingleton<IPlayerRepository, InMemoryPlayerRepository>();
builder.Services.AddSingleton<IGamePlayerRepository, InMemoryGamePlayerRepository>();
builder.Services.AddSingleton<PasswordHasher>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    // Type parameter is required for AOT compilation support
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter<Marker>());
    options.SerializerOptions.TypeInfoResolver = TicTacToeJsonContext.Default;
});

var app = builder.Build();

// Configure static file serving
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthorization();

// Map endpoints
app.MapHome();
app.MapGame();
app.MapPlayer();

app.Run();

public partial class Program { }
