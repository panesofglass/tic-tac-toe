using TicTacToe.Engine;
using TicTacToe.Web.Infrastructure;

namespace TicTacToe.Tests.Infrastructure;

public class InMemoryGameRepositoryTests
{
    private readonly InMemoryGameRepository _repository;

    public InMemoryGameRepositoryTests()
    {
        _repository = new InMemoryGameRepository();
    }

    [Fact]
    public async Task CreateGame_Returns_UniqueIds()
    {
        var gameId1 = await _repository.CreateGameAsync();
        var gameId2 = await _repository.CreateGameAsync();

        Assert.NotEqual(gameId1, gameId2);
    }

    [Fact]
    public async Task GetGame_Returns_CreatedGame()
    {
        // Arrange
        var (gameId, originalGame) = await _repository.CreateGameAsync();

        // Act
        var retrievedGame = await _repository.GetGameAsync(gameId);

        // Assert
        Assert.Equal(originalGame, retrievedGame);
    }

    [Fact]
    public async Task GetGame_ThrowsException_WhenGameNotFound()
    {
        await Assert.ThrowsAsync<GameNotFoundException>(
            () => _repository.GetGameAsync("nonexistent-id")
        );
    }

    [Fact]
    public async Task UpdateGame_Succeeds_WithCorrectVersion()
    {
        // Arrange
        var (gameId, game) = await _repository.CreateGameAsync();
        var updatedGame = Game.MakeMove(game, new Position(0));

        // Act
        var result = await _repository.UpdateGameAsync(gameId, updatedGame);

        // Assert
        Assert.Equal(updatedGame, result);
        var stored = await _repository.GetGameAsync(gameId);
        Assert.Equal(updatedGame, stored);
    }

    [Fact]
    public async Task DeleteGame_RemovesGame()
    {
        // Arrange
        var (gameId, _) = await _repository.CreateGameAsync();

        // Act
        await _repository.DeleteGameAsync(gameId);

        // Assert
        await Assert.ThrowsAsync<GameNotFoundException>(() => _repository.GetGameAsync(gameId));
    }
}
