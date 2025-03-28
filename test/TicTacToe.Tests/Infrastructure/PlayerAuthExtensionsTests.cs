using Microsoft.AspNetCore.Http;
using TicTacToe.Web.Infrastructure;

namespace TicTacToe.Tests.Infrastructure;

public class PlayerAuthExtensionsTests
{
    private readonly DefaultHttpContext _context;
    private readonly IPlayerRepository _playerRepository;

    public PlayerAuthExtensionsTests()
    {
        _context = new DefaultHttpContext();
        _playerRepository = new InMemoryPlayerRepository();
    }

    [Fact]
    public void GetCurrentPlayerId_WhenNoCookie_ReturnsNull()
    {
        // Act
        var result = _context.GetCurrentPlayerId();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SignInPlayer_SetsCookieWithCorrectOptions()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        // Act
        _context.SignInPlayer(playerId);

        // Assert
        var cookie = _context.Response.Headers.SetCookie.ToString();
        Assert.Contains(playerId.ToString(), cookie);
        Assert.Contains("SameSite=Strict", cookie);
        Assert.Contains("HttpOnly", cookie);
        Assert.Contains("Secure", cookie);
    }

    [Fact]
    public void SignOutPlayer_RemovesCookie()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        _context.SignInPlayer(playerId);

        // Act
        _context.SignOutPlayer();

        // Assert
        var cookie = _context.Response.Headers.SetCookie.ToString();
        Assert.Contains("expires=Thu, 01 Jan 1970", cookie.ToLower());
    }

    [Fact]
    public async Task EnsurePlayerAsync_WhenNoCookie_CreatesNewPlayer()
    {
        // Act
        var playerId = await _context.EnsurePlayerAsync(_playerRepository);

        // Assert
        Assert.NotEqual(Guid.Empty, playerId);
        var player = await _playerRepository.GetByIdAsync(playerId);
        Assert.NotNull(player);
        Assert.StartsWith("Player_", player.Name);

        var cookie = _context.Response.Headers.SetCookie.ToString();
        Assert.Contains(playerId.ToString(), cookie);
    }

    [Fact]
    public async Task EnsurePlayerAsync_WhenValidCookie_ReturnsExistingPlayer()
    {
        // Arrange
        var existingPlayer = new Player
        {
            Id = Guid.NewGuid(),
            Name = "Test Player",
            CreatedAt = DateTimeOffset.UtcNow,
            LastActive = DateTimeOffset.UtcNow
        };
        await _playerRepository.CreateAsync(existingPlayer);
        _context.SignInPlayer(existingPlayer.Id);

        // Create a new context with the cookie
        var newContext = new DefaultHttpContext();
        newContext.Request.Headers.Cookie = _context.Response.Headers.SetCookie;

        // Act
        var playerId = await newContext.EnsurePlayerAsync(_playerRepository);

        // Assert
        Assert.Equal(existingPlayer.Id, playerId);
        Assert.Empty(newContext.Response.Headers.SetCookie); // Shouldn't set a new cookie
    }

    [Fact]
    public async Task EnsurePlayerAsync_WhenInvalidCookie_CreatesNewPlayer()
    {
        // Arrange
        _context.Request.Headers.Cookie = $"playerId={Guid.NewGuid()}";

        // Act
        var playerId = await _context.EnsurePlayerAsync(_playerRepository);

        // Assert
        Assert.NotEqual(Guid.Empty, playerId);
        var player = await _playerRepository.GetByIdAsync(playerId);
        Assert.NotNull(player);
        Assert.StartsWith("Player_", player.Name);

        var cookie = _context.Response.Headers.SetCookie.ToString();
        Assert.Contains(playerId.ToString(), cookie);
    }
}
