using TicTacToe.Engine;

namespace TicTacToe.Web.Infrastructure;

public interface IGameRepository
{
    /// <summary>
    /// Creates a new game and returns its unique identifier and initial state
    /// </summary>
    Task<(Guid id, Game game)> CreateGameAsync();

    /// <summary>
    /// Retrieves all active games with their identifiers
    /// </summary>
    Task<IEnumerable<(Guid id, Game game)>> GetGamesAsync();

    /// <summary>
    /// Retrieves a game by its identifier
    /// </summary>
    /// <exception cref=\"GameNotFoundException\">Thrown when the game doesn't exist</exception>
    Task<Game> GetGameAsync(Guid gameId);

    /// <summary>
    /// Updates a game's state
    /// </summary>
    /// <exception cref=\"GameNotFoundException\">Thrown when the game doesn't exist</exception>
    /// <exception cref=\"ConcurrencyException\">Thrown when the game has been modified by another operation</exception>
    Task<Game> UpdateGameAsync(Guid gameId, Game game);

    /// <summary>
    /// Deletes a game by its identifier
    /// </summary>
    Task DeleteGameAsync(Guid gameId);
}

public class GameNotFoundException : Exception
{
    public GameNotFoundException(Guid gameId)
        : base($"Game with ID {gameId} not found") { }
}

public class ConcurrencyException : Exception
{
    public ConcurrencyException(Guid gameId)
        : base($"Game with ID {gameId} was modified by another operation") { }
}
