using StarFederation.Datastar.DependencyInjection;
using TicTacToe.Web.Endpoints;
using TicTacToe.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add datastar services
builder.Services.AddWebEncoders();
builder.Services.AddDatastar();

// Configure repositories
builder.Services.AddSingleton<IGameRepository, InMemoryGameRepository>();
builder.Services.AddSingleton<IPlayerRepository, InMemoryPlayerRepository>();
builder.Services.AddSingleton<IGamePlayerRepository, InMemoryGamePlayerRepository>();

var app = builder.Build();

// Configure static file serving
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthorization();

// Map endpoints
app.MapHome();
app.MapGame();

app.Run();

public partial class Program { }
