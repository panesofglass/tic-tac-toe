using TicTacToe.Web.Models;

namespace TicTacToe.Web.Infrastructure;

public interface IGameRepository
{
    /// <summary>
    /// Creates a new game and returns its unique identifier
    /// </summary>
    Task<(string GameId, Game Game)> CreateGameAsync();

    /// <summary>
    /// Retrieves a game by its identifier
    /// </summary>
    /// <exception cref="GameNotFoundException">Thrown when the game doesn't exist</exception>
    Task<Game> GetGameAsync(string gameId);

    /// <summary>
    /// Updates a game's state
    /// </summary>
    /// <exception cref="GameNotFoundException">Thrown when the game doesn't exist</exception>
    /// <exception cref="ConcurrencyException">Thrown when the game has been modified by another operation</exception>
    Task<Game> UpdateGameAsync(string gameId, Game game, int expectedVersion);

    /// <summary>
    /// Deletes a game by its identifier
    /// </summary>
    Task DeleteGameAsync(string gameId);
}

public class GameNotFoundException : Exception
{
    public GameNotFoundException(string gameId)
        : base($"Game with ID {gameId} not found") { }
}

public class ConcurrencyException : Exception
{
    public ConcurrencyException(string gameId)
        : base($"Game with ID {gameId} was modified by another operation") { }
}

