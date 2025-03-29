using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TicTacToe.Web.Models;

namespace TicTacToe.Tests.Web;

public class PlayerAuthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public PlayerAuthTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpContextAccessor = factory.Services.GetRequiredService<IHttpContextAccessor>();
        _signInManager = factory.Services.GetRequiredService<SignInManager<IdentityUser>>();
        _userManager = factory.Services.GetRequiredService<UserManager<IdentityUser>>();
    }

    [Fact]
    public async Task Given_NewRegistration_When_ValidData_Then_CreatesRegisteredPlayer()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = $"test_{Guid.NewGuid()}@example.com";
        var name = "Test User";
        var password = "Test1234!";

        // Act

        // Verify new user can access /register
        await client.GetAsync("/register");

        // Submit register form
        var response = await client.PostAsJsonAsync<RegisterModel>(
            "/register",
            new RegisterModel(email, name, password)
        );

        var player = await _userManager.FindByEmailAsync(email);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.ToString());
        Assert.NotNull(player);
        Assert.Equal(email, player.Email);
        Assert.Equal(name, player.UserName);

        Assert.True(await _userManager.CheckPasswordAsync(player!, password));
    }

    [Fact]
    public async Task Given_NewRegistration_When_EmailExists_Then_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = $"test_{Guid.NewGuid()}@example.com";
        var password = "Test1234!";

        // Create first user
        await _userManager.CreateAsync(
            new IdentityUser { UserName = "First Name", Email = email },
            password
        );

        // Act

        // Verify new user can access /register
        await client.GetAsync("/register");

        // Submit register form
        var response = await client.PostAsJsonAsync<RegisterModel>(
            "/register",
            new RegisterModel(email, "Second Name", password)
        );

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
        var name = "Test User";
        var password = "Test1234!";

        // Verify new user can access /register
        await client.GetAsync("/register");

        // Submit register form
        await client.PostAsJsonAsync<RegisterModel>(
            "/register",
            new RegisterModel(email, name, password)
        );

        // Act
        var response = await client.PostAsJsonAsync<LoginModel>(
            "/login",
            new LoginModel(email, password)
        );

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.ToString());

        var httpContext = _httpContextAccessor.HttpContext;
        Assert.NotNull(httpContext);

        var authenticatedPlayerId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var player = await _userManager.FindByEmailAsync(email);
        Assert.Equal(player?.UserName, authenticatedPlayerId);
    }

    [Fact]
    public async Task Given_InvalidCredentials_When_Login_Then_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = $"test_{Guid.NewGuid()}@example.com";

        // Act
        var response = await client.PostAsJsonAsync(
            "/login",
            new LoginModel(email, "wrongpassword")
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("invalid email or password", content.ToLower());
    }
}
