using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Logging;
using TicTacToe.Web.Endpoints;
using TicTacToe.Web.Infrastructure;
using TicTacToe.Web.Models;
using Xunit;

namespace TicTacToe.Web.Tests;

public class EmailClaimsTests
{
    private IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        
        services.AddOptions();
        services.AddLogging();
        
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        }).AddCookie(options =>
        {
            options.Cookie.Name = "playerId";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });

        return services.BuildServiceProvider();
    }
    
    [Fact]
    public async Task Email_Claim_Is_Preserved_Through_Transformation()
    {
        // Existing test remains unchanged
        var player = Player.Create();
        player = player with { Email = "test@example.com" };
        
        var playerRepo = new InMemoryPlayerRepository();
        await playerRepo.RegisterPlayerAsync(player.Id, player.Email!, "Test User", "hash");
        
        var logger = LoggerFactory.Create(builder => builder.AddConsole())
            .CreateLogger<PlayerClaimsTransformation>();
        
        var transformation = new PlayerClaimsTransformation(playerRepo, logger);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
            new Claim(ClaimTypes.Email, player.Email!)
        };
        
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        var transformedPrincipal = await transformation.TransformAsync(principal);
        
        var emailClaim = transformedPrincipal.FindFirst(ClaimTypes.Email);
        Assert.NotNull(emailClaim);
        Assert.Equal(player.Email, emailClaim.Value);
    }

    [Fact]
    public async Task Registration_Sets_Email_Claim_Correctly()
    {
        // Arrange
        var players = new InMemoryPlayerRepository();
        var hasher = new PasswordHasher();
        
        var sp = CreateServiceProvider();
        
        var context = new DefaultHttpContext
        {
            RequestServices = sp
        };
        
        // Set up form with test data
        var formDict = new Dictionary<string, StringValues>
        {
            ["Email"] = "newuser@example.com",
            ["Name"] = "New User",
            ["Password"] = "Password123!"
        };
        var formCollection = new FormCollection(formDict);
        var formFeature = new FormFeature(formCollection);
        context.Features.Set<IFormFeature>(formFeature);
        
        // Act
        var result = await AuthEndpoints.HandleRegistrationAsync(
            context,
            players,
            hasher,
            "/");
        
        // Assert
        // Verify player was created
        var player = await players.GetByEmailAsync("newuser@example.com");
        Assert.NotNull(player);
        Assert.Equal("New User", player.Name);
        Assert.Equal("newuser@example.com", player.Email);
        
        // Verify claims in the authentication ticket
        var ticket = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        Assert.NotNull(ticket?.Principal);
        Assert.True(ticket.Principal.Identity?.IsAuthenticated);
        
        var emailClaim = ticket.Principal.FindFirst(ClaimTypes.Email);
        Assert.NotNull(emailClaim);
        Assert.Equal("newuser@example.com", emailClaim.Value);

        // Verify other expected claims are present
        Assert.NotNull(ticket.Principal.FindFirst(ClaimTypes.NameIdentifier));
        Assert.NotNull(ticket.Principal.FindFirst(ClaimTypes.Name));
        Assert.NotNull(ticket.Principal.FindFirst("IsRegistered"));
        Assert.Equal("true", ticket.Principal.FindFirst("IsRegistered")?.Value);
    }
}

