using StarFederation.Datastar.DependencyInjection;
using TicTacToe.Web.Endpoints;
using TicTacToe.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add datastar services
builder.Services.AddWebEncoders();
builder.Services.AddDatastar();

// Add game repository
builder.Services.AddSingleton<IGameRepository, InMemoryGameRepository>();

var app = builder.Build();

// Configure static file serving
app.UseStatusCodePages();
app.UseStaticFiles();

// Map endpoints
app.MapHome();
app.MapGame();

app.Run();

public partial class Program { }
