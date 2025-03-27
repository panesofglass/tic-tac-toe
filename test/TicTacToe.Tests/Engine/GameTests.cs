using TicTacToe.Engine;

namespace TicTacToe.Tests.Engine;

public class GameTests
{
    [Fact]
    public void Given_NewGame_When_Created_Then_BoardHasAllAvailableSpacesForXAsNextMarker()
    {
        // Arrange & Act
        var game = Game.Create();

        // Assert
        Assert.IsType<Game.InProgress>(game);
        var inProgressGame = (Game.InProgress)game;
        Assert.Empty(inProgressGame.Moves);

        // Verify all positions are available with X as the next marker
        for (byte i = 0; i < 9; i++)
        {
            var spaceState = inProgressGame.Board[new Position(i)];
            Assert.IsType<Square.Available>(spaceState);
            Assert.Equal(Marker.X, ((Square.Available)spaceState).NextMarker);
        }
    }

    [Fact]
    public void Given_NewGame_When_MakingValidMove_Then_MoveIsRecordedAndNextMarkerAlternates()
    {
        // Arrange
        var game = Game.Create();

        // Act
        var afterFirstMove = game.WithMove(Move.Create(new Position(0), Marker.X));

        // Assert
        Assert.IsType<Game.InProgress>(afterFirstMove);
        var inProgressGame = (Game.InProgress)afterFirstMove;

        // Check the board has the marker at position 0
        var position0State = inProgressGame.Board[new Position(0)];
        Assert.IsType<Square.Taken>(position0State);
        Assert.Equal(Marker.X, ((Square.Taken)position0State).Marker);

        // Check available spaces now have O as the next marker
        for (byte i = 1; i < 9; i++)
        {
            var spaceState = inProgressGame.Board[new Position(i)];
            Assert.IsType<Square.Available>(spaceState);
            Assert.Equal(Marker.O, ((Square.Available)spaceState).NextMarker);
        }

        // Check moves collection has the move
        Assert.Single(inProgressGame.Moves);
        Assert.Equal(new Position(0), inProgressGame.Moves[0].Position);
        Assert.Equal(Marker.X, inProgressGame.Moves[0].Marker);

        // Make a second move
        var afterSecondMove = afterFirstMove.WithMove(Move.Create(new Position(1), Marker.O));
        Assert.IsType<Game.InProgress>(afterSecondMove);
        var afterSecondMoveGame = (Game.InProgress)afterSecondMove;

        // Check the board has both markers
        var position0NewState = afterSecondMoveGame.Board[new Position(0)];
        Assert.IsType<Square.Taken>(position0NewState);
        Assert.Equal(Marker.X, ((Square.Taken)position0NewState).Marker);

        var position1State = afterSecondMoveGame.Board[new Position(1)];
        Assert.IsType<Square.Taken>(position1State);
        Assert.Equal(Marker.O, ((Square.Taken)position1State).Marker);

        // Check available spaces now have X as the next marker
        for (byte i = 2; i < 9; i++)
        {
            var spaceState = afterSecondMoveGame.Board[new Position(i)];
            Assert.IsType<Square.Available>(spaceState);
            Assert.Equal(Marker.X, ((Square.Available)spaceState).NextMarker);
        }

        // Check moves collection has both moves
        Assert.Equal(2, afterSecondMoveGame.Moves.Length);
        Assert.Equal(Marker.O, afterSecondMoveGame.Moves[1].Marker);
    }

    [Fact]
    public void Given_InProgressGame_When_HorizontalWinningMoveIsMade_Then_GameStateBecomesWinner()
    {
        // Arrange - X in positions 0 and 1, O in positions 3 and 4
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.X),
            Move.Create(new Position(3), Marker.O),
            Move.Create(new Position(1), Marker.X),
            Move.Create(new Position(4), Marker.O),
        };
        var game = Game.FromMoves(moves);

        // Act - X makes winning move in position 2 (completing top row)
        var gameAfterWinningMove = game.WithMove(Move.Create(new Position(2), Marker.X));

        // Assert
        Assert.IsType<Game.Winner>(gameAfterWinningMove);
        var winnerGame = (Game.Winner)gameAfterWinningMove;
        Assert.Equal(Marker.X, winnerGame.WinningPlayer);
        Assert.Equal(5, winnerGame.Moves.Length);
    }

    [Fact]
    public void Given_InProgressGame_When_VerticalWinningMoveIsMade_Then_GameStateBecomesWinner()
    {
        // Arrange - O in positions 1 and 4, X in positions 0, 3, and about to play in 6
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.X),
            Move.Create(new Position(1), Marker.O),
            Move.Create(new Position(3), Marker.X),
            Move.Create(new Position(4), Marker.O),
        };
        var game = Game.FromMoves(moves);

        // Act - X makes winning move in position 6 (completing left column)
        var gameAfterWinningMove = game.WithMove(Move.Create(new Position(6), Marker.X));

        // Assert
        Assert.IsType<Game.Winner>(gameAfterWinningMove);
        var winnerGame = (Game.Winner)gameAfterWinningMove;
        Assert.Equal(Marker.X, winnerGame.WinningPlayer);
        Assert.Equal(5, winnerGame.Moves.Length);
    }

    [Fact]
    public void Given_InProgressGame_When_DiagonalWinningMoveIsMade_Then_GameStateBecomesWinner()
    {
        // Arrange - X in positions 0 and 4, O in positions 1 and 2
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.X),
            Move.Create(new Position(1), Marker.O),
            Move.Create(new Position(4), Marker.X),
            Move.Create(new Position(2), Marker.O),
        };
        var game = Game.FromMoves(moves);

        // Act - X makes winning move in position 8 (completing diagonal from top-left to bottom-right)
        var gameAfterWinningMove = game.WithMove(Move.Create(new Position(8), Marker.X));

        // Assert
        Assert.IsType<Game.Winner>(gameAfterWinningMove);
        var winnerGame = (Game.Winner)gameAfterWinningMove;
        Assert.Equal(Marker.X, winnerGame.WinningPlayer);
        Assert.Equal(5, winnerGame.Moves.Length);
    }

    [Fact]
    public void Given_InProgressGame_When_OtherDiagonalWinningMoveIsMade_Then_GameStateBecomesWinner()
    {
        // Arrange - X in positions 0, 1, 3; O in positions 2 and 4, and about to play in 6
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.X),
            Move.Create(new Position(2), Marker.O),
            Move.Create(new Position(1), Marker.X),
            Move.Create(new Position(4), Marker.O),
            Move.Create(new Position(3), Marker.X),
        };
        var game = Game.FromMoves(moves);

        // Act - O makes winning move in position 6 (completing diagonal from top-right to bottom-left)
        var gameAfterWinningMove = game.WithMove(Move.Create(new Position(6), Marker.O));

        // Assert
        Assert.IsType<Game.Winner>(gameAfterWinningMove);
        var winnerGame = (Game.Winner)gameAfterWinningMove;
        Assert.Equal(Marker.O, winnerGame.WinningPlayer);
        Assert.Equal(6, winnerGame.Moves.Length);
    }

    [Fact]
    public void Given_InProgressGame_When_AllPositionsFilled_Then_GameStateIsDraw()
    {
        // Arrange - Create a game that will result in a draw
        // X | O | X
        // X | O | O
        // O | X | X
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.X), // X top-left
            Move.Create(new Position(1), Marker.O), // O top-middle
            Move.Create(new Position(2), Marker.X), // X top-right
            Move.Create(new Position(4), Marker.O), // O middle-middle
            Move.Create(new Position(3), Marker.X), // X middle-left
            Move.Create(new Position(5), Marker.O), // O middle-right
            Move.Create(new Position(7), Marker.X), // X bottom-middle
            Move.Create(new Position(6), Marker.O), // O bottom-left
        };
        var game = Game.FromMoves(moves);

        // Act - Last move by X in position 8 (bottom-right)
        var gameAfterLastMove = game.WithMove(Move.Create(new Position(8), Marker.X));

        // Assert
        Assert.IsType<Game.Draw>(gameAfterLastMove);
        var drawGame = (Game.Draw)gameAfterLastMove;
        Assert.Equal(9, drawGame.Moves.Length);

        // Verify all positions are taken
        for (byte i = 0; i < 9; i++)
        {
            Assert.IsType<Square.Taken>(drawGame.Board[new Position(i)]);
        }
    }

    [Fact]
    public void Given_GameWithWinner_When_CreatingFromMoves_Then_GameStateIsWinner()
    {
        // Arrange
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.X),
            Move.Create(new Position(3), Marker.O),
            Move.Create(new Position(1), Marker.X),
            Move.Create(new Position(4), Marker.O),
            Move.Create(new Position(2), Marker.X), // X wins with top row
        };

        // Act
        var game = Game.FromMoves(moves);

        // Assert
        Assert.IsType<Game.Winner>(game);
        var winnerGame = (Game.Winner)game;
        Assert.Equal(Marker.X, winnerGame.WinningPlayer);
    }

    [Fact]
    public void Given_GameWithDraw_When_CreatingFromMoves_Then_GameStateIsDraw()
    {
        // Arrange - Create a sequence of moves that results in a draw
        // X | O | X
        // X | O | O
        // O | X | X
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.X), // X top-left
            Move.Create(new Position(1), Marker.O), // O top-middle
            Move.Create(new Position(2), Marker.X), // X top-right
            Move.Create(new Position(4), Marker.O), // O middle-middle
            Move.Create(new Position(3), Marker.X), // X middle-left
            Move.Create(new Position(5), Marker.O), // O middle-right
            Move.Create(new Position(7), Marker.X), // X bottom-middle
            Move.Create(new Position(6), Marker.O), // O bottom-left
            Move.Create(new Position(8), Marker.X), // X bottom-right
        };

        // Act
        var game = Game.FromMoves(moves);

        // Assert
        Assert.IsType<Game.Draw>(game);
        var drawGame = (Game.Draw)game;
        Assert.Equal(9, drawGame.Moves.Length);
    }

    [Fact]
    public void Given_InProgressGame_When_PlayerWins_Then_GameStateTransitionsToWinner()
    {
        // Arrange
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.X),
            Move.Create(new Position(3), Marker.O),
            Move.Create(new Position(1), Marker.X),
            Move.Create(new Position(4), Marker.O),
        };
        var game = Game.FromMoves(moves);

        // Verify game is in progress
        Assert.IsType<Game.InProgress>(game);

        // Act - X makes winning move
        var gameAfterWinningMove = game.WithMove(Move.Create(new Position(2), Marker.X));

        // Assert - Game transitions to Winner state
        Assert.IsType<Game.Winner>(gameAfterWinningMove);
    }

    [Fact]
    public void Given_InProgressGame_When_BoardFills_Then_GameStateTransitionsToDraw()
    {
        // Arrange - Create a game with 8 moves that hasn't resulted in a win
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.X),
            Move.Create(new Position(1), Marker.O),
            Move.Create(new Position(2), Marker.X),
            Move.Create(new Position(4), Marker.O),
            Move.Create(new Position(3), Marker.X),
            Move.Create(new Position(5), Marker.O),
            Move.Create(new Position(7), Marker.X),
            Move.Create(new Position(6), Marker.O),
        };
        var game = Game.FromMoves(moves);

        // Verify game is in progress
        Assert.IsType<Game.InProgress>(game);

        // Act - Last move fills the board without a winner
        var gameAfterLastMove = game.WithMove(Move.Create(new Position(8), Marker.X));

        // Assert - Game transitions to Draw state
        Assert.IsType<Game.Draw>(gameAfterLastMove);
    }

    [Fact]
    public void Given_InProgressGame_When_MovingToOccupiedPosition_Then_ThrowsException()
    {
        // Arrange
        var moves = new[] { Move.Create(new Position(0), Marker.X) };
        var game = Game.FromMoves(moves);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => game.WithMove(Move.Create(new Position(0), Marker.O))
        );

        Assert.Equal("Invalid move. (Parameter 'move')", exception.Message);
    }

    [Fact]
    public void Given_CompletedGame_When_MakingMove_Then_ThrowsException()
    {
        // Arrange - Create a completed game (with winner)
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.X),
            Move.Create(new Position(3), Marker.O),
            Move.Create(new Position(1), Marker.X),
            Move.Create(new Position(4), Marker.O),
            Move.Create(new Position(2), Marker.X), // X wins with top row
        };
        var game = Game.FromMoves(moves);

        // Verify game is completed
        Assert.IsType<Game.Winner>(game);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => game.WithMove(Move.Create(new Position(6), Marker.O))
        );

        Assert.Contains("Game is already complete", exception.Message);
    }

    [Fact]
    public void Given_InProgressGame_When_PositionOutOfRange_Then_ThrowsException()
    {
        // Arrange
        var game = Game.Create();

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => game.WithMove(Move.Create(new Position(9), Marker.X))
        );

        Assert.Equal("Position must be between 0 and 8. (Parameter 'position')", exception.Message);
    }

    [Fact]
    public void Given_InvalidMoveSequence_When_SamePlayerMovesTwice_Then_ThrowsException()
    {
        // Arrange - Two consecutive X moves
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.X),
            Move.Create(new Position(1), Marker.X), // Invalid: X moves twice
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Game.FromMoves(moves));
        Assert.Equal("Invalid move. (Parameter 'move')", exception.Message);
    }

    [Fact]
    public void Given_InvalidPosition_When_MakingMove_Then_ThrowsException()
    {
        // Arrange
        var game = Game.FromMoves(new[] { Move.Create(new Position(0), Marker.X) });

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => game.WithMove(Move.Create(new Position(9), Marker.O))
        );

        Assert.Equal("Position must be between 0 and 8. (Parameter 'position')", exception.Message);
    }

    [Fact]
    public void Given_DuplicateMove_When_CreatingGame_Then_ThrowsException()
    {
        // Arrange - Position 0 is used twice
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.X),
            Move.Create(new Position(1), Marker.O),
            Move.Create(new Position(0), Marker.X), // Invalid: Position 0 already taken
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Game.FromMoves(moves));
        Assert.Equal("Invalid move. (Parameter 'move')", exception.Message);
    }

    [Fact]
    public void Given_EmptyMoves_When_CreatingGame_Then_ReturnsNewGame()
    {
        // Arrange & Act
        var game = Game.FromMoves(Array.Empty<Move>());

        // Assert
        Assert.IsType<Game.InProgress>(game);
        var inProgressGame = (Game.InProgress)game;
        Assert.Empty(inProgressGame.Moves);

        // Verify all positions are available with X as the next marker
        for (byte i = 0; i < 9; i++)
        {
            var spaceState = inProgressGame.Board[new Position(i)];
            Assert.IsType<Square.Available>(spaceState);
            Assert.Equal(Marker.X, ((Square.Available)spaceState).NextMarker);
        }
    }

    [Fact]
    public void Given_MaxMoves_When_CreatingGame_Then_ReturnsDraw()
    {
        // Arrange - Create a sequence of moves that results in a draw
        // X | O | X
        // X | O | O
        // O | X | X
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.X), // X top-left
            Move.Create(new Position(1), Marker.O), // O top-middle
            Move.Create(new Position(2), Marker.X), // X top-right
            Move.Create(new Position(4), Marker.O), // O middle-middle
            Move.Create(new Position(3), Marker.X), // X middle-left
            Move.Create(new Position(5), Marker.O), // O middle-right
            Move.Create(new Position(7), Marker.X), // X bottom-middle
            Move.Create(new Position(6), Marker.O), // O bottom-left
            Move.Create(new Position(8), Marker.X), // X bottom-right
        };

        // Act
        var game = Game.FromMoves(moves);

        // Assert
        Assert.IsType<Game.Draw>(game);
        var drawGame = (Game.Draw)game;
        Assert.Equal(9, drawGame.Moves.Length);

        // Verify all positions are taken
        for (byte i = 0; i < 9; i++)
        {
            Assert.IsType<Square.Taken>(drawGame.Board[new Position(i)]);
        }
    }

    [Fact]
    public void Given_LastMoveCompletesBoard_When_WinningMove_Then_ReturnsWinner()
    {
        // Arrange - Create a sequence where the last move results in a win
        // O | O | X
        // O | O | X
        // X | X | X (X's last move in bottom-right will win)
        var moves = new[]
        {
            Move.Create(new Position(2), Marker.X), // X top-right
            Move.Create(new Position(1), Marker.O), // O top-middle
            Move.Create(new Position(5), Marker.X), // X bottom-left
            Move.Create(new Position(4), Marker.O), // O middle-middle
            Move.Create(new Position(7), Marker.X), // X bottom-middle
            Move.Create(new Position(3), Marker.O), // O middle-left
            Move.Create(new Position(6), Marker.X), // X bottom-left
            Move.Create(new Position(0), Marker.O), // O top-left
        };
        var game = Game.FromMoves(moves);

        // Act - X makes the winning move in bottom-right
        var gameAfterLastMove = game.WithMove(Move.Create(new Position(8), Marker.X));

        // Assert
        Assert.IsType<Game.Winner>(gameAfterLastMove);
        var winnerGame = (Game.Winner)gameAfterLastMove;
        Assert.Equal(Marker.X, winnerGame.WinningPlayer);
        Assert.Equal(9, winnerGame.Moves.Length);
    }
}
