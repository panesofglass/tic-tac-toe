using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TicTacToe.Engine;
using TicTacToe.Web.Models;

namespace TicTacToe.Tests.Web;

public class WebIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WebIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact(Skip = "Frontend not yet implemented")]
    public async Task Given_NewGame_When_CreatingGame_Then_ReturnsGameWithEmptyBoard()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("/game", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var game = await response.Content.ReadFromJsonAsync<GameModel>();
        Assert.NotNull(game);
        Assert.NotNull(game.Id);
        Assert.Equal(9, game.Board.Length);
        Assert.All(game.Board, square => Assert.Null(square));
        Assert.Equal(Marker.X, game.CurrentPlayer);
        Assert.False(game.IsComplete);
        Assert.Null(game.Winner);
    }

    [Fact(Skip = "Frontend not yet implemented")]
    public async Task Given_ValidGame_When_MakingMove_Then_UpdatesGameState()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createResponse = await client.PostAsync("/game", null);
        var game = await createResponse.Content.ReadFromJsonAsync<GameModel>();
        Assert.NotNull(game);

        var move = new MoveModel(4, Marker.X); // Center position

        // Act
        var response = await client.PostAsync($"/game/{game.Id}", JsonContent.Create(move));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedGame = await response.Content.ReadFromJsonAsync<GameModel>();
        Assert.NotNull(updatedGame);
        Assert.Equal(game.Id, updatedGame.Id);
        Assert.Equal(Marker.X, updatedGame.Board[4]); // Center position marked
        Assert.Equal(Marker.O, updatedGame.CurrentPlayer); // Next player's turn
        Assert.False(updatedGame.IsComplete);
        Assert.Null(updatedGame.Winner);
    }

    [Fact(Skip = "Frontend not yet implemented")]
    public async Task Given_InvalidMove_When_MakingMove_Then_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createResponse = await client.PostAsync("/game", null);
        var game = await createResponse.Content.ReadFromJsonAsync<GameModel>();
        Assert.NotNull(game);

        var move = new MoveModel(9, Marker.X); // Invalid position

        // Act
        var response = await client.PostAsync($"/game/{game.Id}", JsonContent.Create(move));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(Skip = "Frontend not yet implemented")]
    public async Task Given_NonexistentGame_When_MakingMove_Then_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var move = new MoveModel(4, Marker.X);

        // Act
        var response = await client.PostAsync("/game/nonexistent", JsonContent.Create(move));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(Skip = "Frontend not yet implemented")]
    public async Task Given_CompletedGame_When_MakingMove_Then_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createResponse = await client.PostAsync("/game", null);
        var game = await createResponse.Content.ReadFromJsonAsync<GameModel>();
        Assert.NotNull(game);

        // Make winning moves for X
        var winningMoves = new MoveModel[]
        {
            new MoveModel(0, Marker.X),
            new MoveModel(3, Marker.O),
            new MoveModel(1, Marker.X),
            new MoveModel(4, Marker.O),
            new MoveModel(2, Marker.X),
        };
        // Top row for X
        foreach (var move in winningMoves)
        {
            var moveResponse = await client.PostAsync($"/game/{game.Id}", JsonContent.Create(move));
            Assert.Equal(HttpStatusCode.OK, moveResponse.StatusCode);
        }

        // Try to make another move after game is complete
        var invalidMove = new MoveModel(5, Marker.O);

        // Act
        var response = await client.PostAsync($"/game/{game.Id}", JsonContent.Create(invalidMove));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
