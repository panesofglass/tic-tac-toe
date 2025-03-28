using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TicTacToe.Web.Infrastructure;
using TicTacToe.Web.Models;
using static TicTacToe.Web.Models.AuthModels;

namespace TicTacToe.Tests.Web;

public class PlayerAuthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IPlayerRepository _playerRepository;
    private readonly PasswordHasher _passwordHasher;

    public PlayerAuthTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _playerRepository = factory.Services.GetRequiredService<IPlayerRepository>();
        _passwordHasher = factory.Services.GetRequiredService<PasswordHasher>();
    }

    [Fact]
    public async Task Given_NewRegistration_When_ValidData_Then_CreatesRegisteredPlayer()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = $"test_{Guid.NewGuid()}@example.com";
        var name = "Test User";
        var password = "Test1234!";

        // Get a cookie first
        await client.GetAsync("/");
        var playerId = GetPlayerIdFromCookie(client);
        Assert.NotNull(playerId);

        // Act
        var response = await client.PostAsync("/register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["email"] = email,
            ["name"] = name,
            ["password"] = password
        }));

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.ToString());

        var player = await _playerRepository.GetByIdAsync(playerId.Value);
        Assert.NotNull(player);
        Assert.Equal(email, player.Email);
        Assert.Equal(name, player.Name);
        Assert.True(_passwordHasher.VerifyPassword(player, password, player.PasswordHash));
    }

    [Fact]
    public async Task Given_NewRegistration_When_EmailExists_Then_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = $"test_{Guid.NewGuid()}@example.com";
        
        // Create first user
        var firstPlayer = new Player
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = "First User",
            CreatedAt = DateTimeOffset.UtcNow,
            LastActive = DateTimeOffset.UtcNow
        };
        await _playerRepository.CreateAsync(firstPlayer);

        // Get a cookie for second registration
        await client.GetAsync("/");

        // Act
        var response = await client.PostAsync("/register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["email"] = email,
            ["name"] = "Second User",
            ["password"] = "Test1234!"
        }));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("email is already registered", content.ToLower());
    }

    [Fact]
    public async Task Given_ValidCredentials_When_Login_Then_AuthenticatesPlayer()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = $"test_{Guid.NewGuid()}@example.com";
        var password = "Test1234!";
        
        // Create and register a player
        var player = new Player
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = "Test User",
            CreatedAt = DateTimeOffset.UtcNow,
            LastActive = DateTimeOffset.UtcNow,
            PasswordHash = _passwordHasher.HashPassword(new Player(), password)
        };
        await _playerRepository.CreateAsync(player);

        // Act
        var response = await client.PostAsync("/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["email"] = email,
            ["password"] = password
        }));

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.ToString());
        
        var authenticatedPlayerId = GetPlayerIdFromCookie(client);
        Assert.Equal(player.Id, authenticatedPlayerId);
    }

    [Fact]
    public async Task Given_InvalidCredentials_When_Login_Then_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = $"test_{Guid.NewGuid()}@example.com";
        
        // Act
        var response = await client.PostAsync("/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["email"] = email,
            ["password"] = "wrongpassword"
        }));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("invalid email or password", content.ToLower());
    }

    private static Guid? GetPlayerIdFromCookie(HttpClient client)
    {
        var cookies = client.DefaultRequestHeaders.GetValues("Cookie")
            .SelectMany(c => c.Split(';'))
            .Select(c => c.Trim().Split('='))
            .ToDictionary(c => c[0], c => c[1]);

        if (cookies.TryGetValue("playerId", out var playerIdStr) && 
            Guid.TryParse(playerIdStr, out var playerId))
        {
            return playerId;
        }

        return null;
    }
}
