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

    [Fact]
    public void Given_NullMoves_When_CreatingGame_Then_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Game.FromMoves(null!));
    }

    [Fact]
    public void Given_OStartsFirst_When_CreatingGame_Then_ThrowsException()
    {
        // Arrange
        var moves = new[] { Move.Create(new Position(0), Marker.O) };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Game.FromMoves(moves));
        Assert.Equal("Invalid move. (Parameter 'move')", exception.Message);
    }

    [Fact]
    public void Given_Position_When_CreatingFromRowAndColumn_Then_CalculatesCorrectly()
    {
        // Test all positions
        for (byte row = 0; row < 3; row++)
        {
            for (byte col = 0; col < 3; col++)
            {
                var position = Position.At(row, col);
                byte expectedIndex = (byte)(row * 3 + col);

                Assert.Equal(row, position.Row);
                Assert.Equal(col, position.Column);
                Assert.Equal(expectedIndex, (byte)position);
            }
        }
    }

    [Fact]
    public void Given_Position_When_Converting_Then_MaintainsValue()
    {
        for (byte i = 0; i < 9; i++)
        {
            var position = new Position(i);
            byte value = position;
            var converted = (Position)value;

            Assert.Equal(position.Row, converted.Row);
            Assert.Equal(position.Column, converted.Column);
            Assert.Equal((byte)position, (byte)converted);
        }
    }

    [Fact]
    public void Given_InvalidRowOrColumn_When_CreatingPosition_Then_ThrowsException()
    {
        // Row too high
        Assert.Throws<ArgumentOutOfRangeException>(() => Position.At(3, 0));

        // Column too high
        Assert.Throws<ArgumentOutOfRangeException>(() => Position.At(0, 3));

        // Both too high
        Assert.Throws<ArgumentOutOfRangeException>(() => Position.At(3, 3));
    }

    [Fact]
    public void Given_WrongPlayerTurn_When_MakingMove_Then_ThrowsException()
    {
        // Arrange - Game where X just moved
        var game = Game.FromMoves(new[] { Move.Create(new Position(0), Marker.X) });

        // Act & Assert - X tries to move again
        var exception = Assert.Throws<ArgumentException>(
            () => game.WithMove(Move.Create(new Position(1), Marker.X))
        );
        Assert.Equal("Invalid move. (Parameter 'move')", exception.Message);
    }

    [Fact]
    public void Given_GameInDraw_When_MakingMove_Then_ThrowsException()
    {
        // Arrange - Create a draw game
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
            Move.Create(new Position(8), Marker.X),
        };
        var game = Game.FromMoves(moves);

        // Verify game is in draw
        Assert.IsType<Game.Draw>(game);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => game.WithMove(Move.Create(new Position(0), Marker.O))
        );
        Assert.Contains("Game is already complete", exception.Message);
    }

    [Fact]
    public void Given_MinimalMoves_When_WinningMove_Then_GameStateBecomesWinner()
    {
        // Arrange - Minimum moves needed for X to win (5 total moves)
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.X), // X top-left
            Move.Create(new Position(3), Marker.O), // O middle-left
            Move.Create(new Position(1), Marker.X), // X top-middle
            Move.Create(new Position(4), Marker.O), // O middle-middle
        };
        var game = Game.FromMoves(moves);

        // Act - X makes winning move to complete top row
        var gameAfterWin = game.WithMove(Move.Create(new Position(2), Marker.X));

        // Assert
        Assert.IsType<Game.Winner>(gameAfterWin);
        var winner = (Game.Winner)gameAfterWin;
        Assert.Equal(Marker.X, winner.WinningPlayer);
        Assert.Equal(5, winner.Moves.Length); // Verify minimum moves
    }

    [Fact]
    public void Given_TooManyMoves_When_CreatingGame_Then_ThrowsException()
    {
        // Arrange - 10 moves (more than 9 allowed positions)
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.X),
            Move.Create(new Position(1), Marker.O),
            Move.Create(new Position(2), Marker.X),
            Move.Create(new Position(3), Marker.O),
            Move.Create(new Position(4), Marker.X),
            Move.Create(new Position(5), Marker.O),
            Move.Create(new Position(6), Marker.X),
            Move.Create(new Position(7), Marker.O),
            Move.Create(new Position(8), Marker.X),
            Move.Create(new Position(0), Marker.O), // Extra move
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Game.FromMoves(moves));
        Assert.Equal("Invalid move. (Parameter 'move')", exception.Message);
    }

    [Fact]
    public void Given_MirrorDiagonalWin_When_WinningMove_Then_GameStateBecomesWinner()
    {
        // Arrange - Setting up a diagonal win from top-right to bottom-left
        var moves = new[]
        {
            Move.Create(new Position(2), Marker.X), // X top-right
            Move.Create(new Position(0), Marker.O), // O top-left
            Move.Create(new Position(4), Marker.X), // X middle-middle
            Move.Create(new Position(1), Marker.O), // O top-middle
        };
        var game = Game.FromMoves(moves);

        // Act - X completes diagonal
        var gameAfterWin = game.WithMove(Move.Create(new Position(6), Marker.X));

        // Assert
        Assert.IsType<Game.Winner>(gameAfterWin);
        var winner = (Game.Winner)gameAfterWin;
        Assert.Equal(Marker.X, winner.WinningPlayer);
    }

    [Fact]
    public void Given_GameBoard_When_AllSpacesAvailable_Then_TracksNextMarkerCorrectly()
    {
        // Arrange
        var game = Game.Create();
        var inProgressGame = (Game.InProgress)game;

        // Assert - All spaces should be available with X as next marker
        for (byte i = 0; i < 9; i++)
        {
            var space = inProgressGame.Board[new Position(i)];
            Assert.IsType<Square.Available>(space);
            Assert.Equal(Marker.X, ((Square.Available)space).NextMarker);
        }

        // Act - Make a move
        var afterMove = game.WithMove(Move.Create(new Position(0), Marker.X));
        var afterMoveGame = (Game.InProgress)afterMove;

        // Assert - All remaining spaces should show O as next marker
        for (byte i = 1; i < 9; i++)
        {
            var space = afterMoveGame.Board[new Position(i)];
            Assert.IsType<Square.Available>(space);
            Assert.Equal(Marker.O, ((Square.Available)space).NextMarker);
        }
    }

    [Fact]
    public void Given_BoardWithWinner_When_CreatingGame_Then_TransitionsToWinnerState()
    {
        // Arrange - Create a winning board directly
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.X),
            Move.Create(new Position(3), Marker.O),
            Move.Create(new Position(1), Marker.X),
            Move.Create(new Position(4), Marker.O),
            Move.Create(new Position(2), Marker.X),
        };

        // Act
        var game = Game.FromMoves(moves);

        // Assert
        Assert.IsType<Game.Winner>(game);
        var winner = (Game.Winner)game;
        Assert.Equal(Marker.X, winner.WinningPlayer);
        Assert.Equal(moves.Length, winner.Moves.Length);
    }

    [Fact]
    public void Given_BoardInProgress_When_MakingWinningMove_Then_TransitionsToWinner()
    {
        // Arrange - Set up a board one move away from winning
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

        // Act - Make winning move
        var afterMove = game.WithMove(Move.Create(new Position(2), Marker.X));

        // Assert
        Assert.IsType<Game.Winner>(afterMove);
        var winner = (Game.Winner)afterMove;
        Assert.Equal(Marker.X, winner.WinningPlayer);
        Assert.Equal(moves.Length + 1, winner.Moves.Length);
    }

    [Fact]
    public void Given_BoardInProgress_When_MakingDrawMove_Then_TransitionsToDraw()
    {
        // Arrange - Set up a board one move away from draw
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
        var board = moves.Aggregate(GameBoard.Empty, (b, m) => b.WithMove(m));
        var game = Game.FromMoves(moves);

        // Verify game is in progress
        Assert.IsType<Game.InProgress>(game);

        // Act - Make final move leading to draw
        var afterMove = game.WithMove(Move.Create(new Position(8), Marker.X));

        // Assert
        Assert.IsType<Game.Draw>(afterMove);
        var draw = (Game.Draw)afterMove;
        Assert.Equal(9, draw.Moves.Length);

        // Verify all positions are taken
        for (byte i = 0; i < 9; i++)
        {
            Assert.IsType<Square.Taken>(draw.Board[new Position(i)]);
        }
    }

    [Fact]
    public void Given_Move_When_Creating_Then_SetsTimestamp()
    {
        // Arrange & Act
        var move = Move.Create(new Position(0), Marker.X);

        // Assert - Verify timestamp is recent
        var timeDiff = DateTimeOffset.UtcNow - move.Timestamp;
        Assert.True(timeDiff.TotalSeconds < 1);
        Assert.True(move.Timestamp.Offset == TimeSpan.Zero); // Ensure UTC
    }

    [Fact]
    public void Given_ValidPosition_When_CreatingMove_Then_SetsPosition()
    {
        // Arrange & Act
        var position = new Position(4); // Center position
        var move = Move.Create(position, Marker.X);

        // Assert
        Assert.Equal(position, move.Position);
    }

    [Fact]
    public void Given_ValidMoves_When_CreatingFromMoves_Then_CreatesBoard()
    {
        // Arrange
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.X),
            Move.Create(new Position(4), Marker.O),
            Move.Create(new Position(8), Marker.X),
        };

        // Act
        var board = GameBoard.FromMoves(moves);

        // Assert - Check specific positions
        Assert.IsType<Square.Taken>(board[new Position(0)]);
        Assert.Equal(Marker.X, ((Square.Taken)board[new Position(0)]).Marker);
        Assert.IsType<Square.Taken>(board[new Position(4)]);
        Assert.Equal(Marker.O, ((Square.Taken)board[new Position(4)]).Marker);
        Assert.IsType<Square.Taken>(board[new Position(8)]);
        Assert.Equal(Marker.X, ((Square.Taken)board[new Position(8)]).Marker);

        // Check that remaining positions are available with O as next marker
        for (byte i = 1; i < 8; i++)
        {
            if (i != 4)
            {
                Assert.IsType<Square.Available>(board[new Position(i)]);
                Assert.Equal(Marker.O, ((Square.Available)board[new Position(i)]).NextMarker);
            }
        }
    }

    [Fact]
    public void Given_InvalidMoveSequence_When_FromMoves_Then_ThrowsException()
    {
        // Arrange - Moves with an invalid sequence (O tries to start)
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.O),
            Move.Create(new Position(4), Marker.X),
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => GameBoard.FromMoves(moves));
        Assert.Equal("Invalid move. (Parameter 'move')", exception.Message);
    }

    [Fact]
    public void Given_CompletedGameWithWinner_Then_RemainingSquaresAreUnavailable()
    {
        // Arrange - X wins with top row (positions 0, 1, 2)
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

        // Positions 0, 1, 2, 3, 4 should be Taken
        Assert.IsType<Square.Taken>(winnerGame.Board[new Position(0)]);
        Assert.IsType<Square.Taken>(winnerGame.Board[new Position(1)]);
        Assert.IsType<Square.Taken>(winnerGame.Board[new Position(2)]);
        Assert.IsType<Square.Taken>(winnerGame.Board[new Position(3)]);
        Assert.IsType<Square.Taken>(winnerGame.Board[new Position(4)]);

        // Positions 5, 6, 7, 8 should be Unavailable
        Assert.IsType<Square.Unavailable>(winnerGame.Board[new Position(5)]);
        Assert.IsType<Square.Unavailable>(winnerGame.Board[new Position(6)]);
        Assert.IsType<Square.Unavailable>(winnerGame.Board[new Position(7)]);
        Assert.IsType<Square.Unavailable>(winnerGame.Board[new Position(8)]);
    }

    [Fact]
    public void Given_CompletedGameWithDraw_Then_AllSquaresAreTaken()
    {
        // Arrange - Create a sequence of moves that results in a draw
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
            Move.Create(new Position(8), Marker.X), // X bottom-right (board is full)
        };

        // Act
        var game = Game.FromMoves(moves);

        // Assert
        Assert.IsType<Game.Draw>(game);
        var drawGame = (Game.Draw)game;

        // In a draw, all squares should be Taken (not Unavailable)
        for (byte i = 0; i < 9; i++)
        {
            Assert.IsType<Square.Taken>(drawGame.Board[new Position(i)]);
        }
    }

    [Fact]
    public void Given_GameTransitionsToWinner_Then_RemainingSquaresBecomeUnavailable()
    {
        // Arrange - Setup game one move away from X winning
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.X),
            Move.Create(new Position(3), Marker.O),
            Move.Create(new Position(1), Marker.X),
            Move.Create(new Position(4), Marker.O),
        };
        var inProgressGame = Game.FromMoves(moves);
        Assert.IsType<Game.InProgress>(inProgressGame);

        // Verify squares 5-8 are Available before winning move
        for (byte i = 5; i < 9; i++)
        {
            Assert.IsType<Square.Available>(
                ((Game.InProgress)inProgressGame).Board[new Position(i)]
            );
        }

        // Act - Make winning move
        var afterWinningMove = inProgressGame.WithMove(Move.Create(new Position(2), Marker.X));

        // Assert
        Assert.IsType<Game.Winner>(afterWinningMove);
        var winnerGame = (Game.Winner)afterWinningMove;

        // Positions 0, 1, 2, 3, 4 should be Taken
        for (byte i = 0; i <= 4; i++)
        {
            Assert.IsType<Square.Taken>(winnerGame.Board[new Position(i)]);
        }

        // Positions 5, 6, 7, 8 should be Unavailable
        for (byte i = 5; i < 9; i++)
        {
            Assert.IsType<Square.Unavailable>(winnerGame.Board[new Position(i)]);
        }
    }

    [Fact]
    public void Given_InProgressGame_When_TransitioningToDraw_Then_NoSquaresAreUnavailable()
    {
        // Arrange - Setup a game one move away from draw
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
        var inProgressGame = Game.FromMoves(moves);
        Assert.IsType<Game.InProgress>(inProgressGame);

        // Act - Make final move leading to draw
        var afterFinalMove = inProgressGame.WithMove(Move.Create(new Position(8), Marker.X));

        // Assert - Should be a draw with all positions Taken (not Unavailable)
        Assert.IsType<Game.Draw>(afterFinalMove);
        var drawGame = (Game.Draw)afterFinalMove;

        // All squares should be Taken
        for (byte i = 0; i < 9; i++)
        {
            Assert.IsType<Square.Taken>(drawGame.Board[new Position(i)]);
        }
    }

    [Fact]
    public void Given_Winner_When_LoadingFromExactMoves_Then_RemainingSquaresAreUnavailable()
    {
        // Arrange - X wins with left column (positions 0, 3, 6)
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.X), // X top-left
            Move.Create(new Position(1), Marker.O), // O top-middle
            Move.Create(new Position(3), Marker.X), // X middle-left
            Move.Create(new Position(2), Marker.O), // O top-right
            Move.Create(new Position(6), Marker.X), // X bottom-left (winning move)
        };

        // Act
        var game = Game.FromMoves(moves);

        // Assert
        Assert.IsType<Game.Winner>(game);
        var winnerGame = (Game.Winner)game;

        // Positions 0, 1, 2, 3, 6 should be Taken
        Assert.IsType<Square.Taken>(winnerGame.Board[new Position(0)]); // X
        Assert.IsType<Square.Taken>(winnerGame.Board[new Position(1)]); // O
        Assert.IsType<Square.Taken>(winnerGame.Board[new Position(2)]); // O
        Assert.IsType<Square.Taken>(winnerGame.Board[new Position(3)]); // X
        Assert.IsType<Square.Taken>(winnerGame.Board[new Position(6)]); // X

        // Positions 4, 5, 7, 8 should be Unavailable (not Taken or Available)
        Assert.IsType<Square.Unavailable>(winnerGame.Board[new Position(4)]);
        Assert.IsType<Square.Unavailable>(winnerGame.Board[new Position(5)]);
        Assert.IsType<Square.Unavailable>(winnerGame.Board[new Position(7)]);
        Assert.IsType<Square.Unavailable>(winnerGame.Board[new Position(8)]);
    }

    [Fact]
    public void Given_WinnerGame_When_CreatingFromBoard_Then_RemainingSquaresAreUnavailable()
    {
        // Arrange - Create a board with X winning the diagonal (0, 4, 8)
        var moves = new[]
        {
            Move.Create(new Position(0), Marker.X), // X top-left
            Move.Create(new Position(1), Marker.O), // O top-middle
            Move.Create(new Position(4), Marker.X), // X middle-middle
            Move.Create(new Position(2), Marker.O), // O top-right
            Move.Create(new Position(8), Marker.X), // X bottom-right (winning move)
        };

        // Act
        var game = Game.FromMoves(moves);

        // Assert
        Assert.IsType<Game.Winner>(game);
        var winnerGame = (Game.Winner)game;

        // Positions 0, 1, 2, 4, 8 should be Taken
        Assert.IsType<Square.Taken>(winnerGame.Board[new Position(0)]); // X
        Assert.IsType<Square.Taken>(winnerGame.Board[new Position(1)]); // O
        Assert.IsType<Square.Taken>(winnerGame.Board[new Position(2)]); // O
        Assert.IsType<Square.Taken>(winnerGame.Board[new Position(4)]); // X
        Assert.IsType<Square.Taken>(winnerGame.Board[new Position(8)]); // X

        // Positions 3, 5, 6, 7 should be Unavailable
        Assert.IsType<Square.Unavailable>(winnerGame.Board[new Position(3)]);
        Assert.IsType<Square.Unavailable>(winnerGame.Board[new Position(5)]);
        Assert.IsType<Square.Unavailable>(winnerGame.Board[new Position(6)]);
        Assert.IsType<Square.Unavailable>(winnerGame.Board[new Position(7)]);
    }
}
