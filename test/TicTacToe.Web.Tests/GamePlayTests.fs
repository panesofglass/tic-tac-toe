namespace TicTacToe.Web.Tests

open System.Threading.Tasks
open NUnit.Framework

/// Gameplay tests for single-game interactions.
/// Tests basic game mechanics: moves, turns, win conditions.
[<TestFixture>]
type GamePlayTests() =
    inherit TestBase()

    /// Creates a new game by clicking the New Game button
    member private this.CreateGame() : Task =
        task {
            do! TestHelpers.waitForVisible this.Page ".new-game-btn" this.TimeoutMs
            do! this.Page.Locator(".new-game-btn").ClickAsync()
            do! TestHelpers.waitForVisible this.Page ".game-board" this.TimeoutMs
        }

    /// Deletes all existing games to ensure clean state
    member private this.CleanupGames() : Task =
        task {
            let! count = this.Page.Locator(".delete-game-btn").CountAsync()
            for _ in 1 .. count do
                do! this.Page.Locator(".delete-game-btn").First.ClickAsync()
                do! Task.Delay(100)
        }

    [<SetUp>]
    member this.EnsureFreshGame() : Task =
        task {
            // Wait for page to load with New Game button visible
            do! TestHelpers.waitForVisible this.Page ".new-game-btn" this.TimeoutMs
            // Clean up any existing games
            do! this.CleanupGames()
            // Create a fresh game
            do! this.CreateGame()
        }

    // ============================================================================
    // User Story 1: New Game button creates visible game board
    // ============================================================================

    [<Test>]
    member this.``New Game button creates visible game board``() : Task =
        task {
            // Game was created in setup, verify it's visible
            let! isVisible = TestHelpers.isVisible this.Page ".game-board"
            Assert.That(isVisible, Is.True, "Game board should be visible after clicking New Game")

            let! clickableCount = this.Page.Locator(".square-clickable").CountAsync()
            Assert.That(clickableCount, Is.EqualTo(9), "Should have 9 clickable squares")
        }

    // ============================================================================
    // User Story 1: Clicking square places X, then O
    // ============================================================================

    [<Test>]
    member this.``Clicking square places X on first move``() : Task =
        task {
            do! TestHelpers.waitForVisible this.Page ".square-clickable" this.TimeoutMs

            do! this.Page.Locator(".square-clickable").First.ClickAsync()

            do! TestHelpers.waitForVisible this.Page ".player" this.TimeoutMs
            let! playerText = this.Page.Locator(".player").First.TextContentAsync()
            Assert.That(playerText, Is.EqualTo("X"), "First move should place X")
        }

    [<Test>]
    member this.``Turn alternates between X and O``() : Task =
        task {
            do! TestHelpers.waitForVisible this.Page ".square-clickable" this.TimeoutMs

            // Get game URL for second player
            let gameBoard = this.Page.Locator(".game-board").First
            let! gameId = gameBoard.GetAttributeAsync("id")
            let gameIdValue = gameId.Substring("game-".Length)
            let gameUrl = $"{this.BaseUrl}/games/{gameIdValue}"

            // Create second player (O)
            let! playerO = this.CreateSecondPlayer(gameUrl)
            do! TestHelpers.waitForVisible playerO ".game-board" this.TimeoutMs

            // First move - should be X's turn
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs

            // X clicks first square
            do! this.Page.Locator(".square-clickable").First.ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "O's turn" this.TimeoutMs
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O clicks first available square
            do! playerO.Locator(".square-clickable").First.ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs
        }

    // ============================================================================
    // User Story 1: Win condition shows winner message
    // ============================================================================

    [<Test>]
    member this.``X wins with top row``() : Task =
        task {
            do! TestHelpers.waitForVisible this.Page ".square-clickable" this.TimeoutMs

            // Get game URL for second player
            let gameBoard = this.Page.Locator(".game-board").First
            let! gameId = gameBoard.GetAttributeAsync("id")
            let gameIdValue = gameId.Substring("game-".Length)
            let gameUrl = $"{this.BaseUrl}/games/{gameIdValue}"

            // Create second player (O)
            let! playerO = this.CreateSecondPlayer(gameUrl)
            do! TestHelpers.waitForVisible playerO ".game-board" this.TimeoutMs

            // X: TopLeft (index 0)
            do! this.Page.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: MiddleLeft (now index 2 after X took index 0)
            do! playerO.Locator(".square-clickable").Nth(2).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs

            // X: TopCenter (now index 0 - first available)
            do! this.Page.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: MiddleCenter (now index 2)
            do! playerO.Locator(".square-clickable").Nth(2).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs

            // X: TopRight - wins! (now index 0 - first available)
            do! this.Page.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "wins" this.TimeoutMs

            // Verify both players see the win
            let! statusTextX = this.Page.Locator(".status").TextContentAsync()
            Assert.That(statusTextX, Does.Contain("X wins!"), "X player should see X wins")

            do! TestHelpers.waitForTextContains playerO ".status" "wins" this.TimeoutMs
            let! statusTextO = playerO.Locator(".status").TextContentAsync()
            Assert.That(statusTextO, Does.Contain("X wins!"), "O player should see X wins")
        }

    [<Test>]
    member this.``O wins with left column``() : Task =
        task {
            do! TestHelpers.waitForVisible this.Page ".square-clickable" this.TimeoutMs

            // Get game URL for second player
            let gameBoard = this.Page.Locator(".game-board").First
            let! gameId = gameBoard.GetAttributeAsync("id")
            let gameIdValue = gameId.Substring("game-".Length)
            let gameUrl = $"{this.BaseUrl}/games/{gameIdValue}"

            // Create second player (O)
            let! playerO = this.CreateSecondPlayer(gameUrl)
            do! TestHelpers.waitForVisible playerO ".game-board" this.TimeoutMs

            // Board: [0,1,2,3,4,5,6,7,8] = TL,TC,TR,ML,MC,MR,BL,BC,BR
            // O wins with left column: TL, ML, BL

            // X: TopCenter (index 1)
            do! this.Page.Locator(".square-clickable").Nth(1).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: TopLeft (index 0) - remaining: [0,2,3,4,5,6,7,8]
            do! playerO.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs

            // X: TopRight (index 0) - remaining: [2,3,4,5,6,7,8]
            do! this.Page.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: MiddleLeft (index 0) - remaining: [3,4,5,6,7,8]
            do! playerO.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs

            // X: BottomRight (index 4) - remaining: [4,5,6,7,8]
            do! this.Page.Locator(".square-clickable").Nth(4).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: BottomLeft - wins! (index 2) - remaining: [4,5,6,7]
            do! playerO.Locator(".square-clickable").Nth(2).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "wins" this.TimeoutMs

            // Verify both players see O wins
            let! statusTextO = playerO.Locator(".status").TextContentAsync()
            Assert.That(statusTextO, Does.Contain("O wins!"), "O player should see O wins")

            do! TestHelpers.waitForTextContains this.Page ".status" "wins" this.TimeoutMs
            let! statusTextX = this.Page.Locator(".status").TextContentAsync()
            Assert.That(statusTextX, Does.Contain("O wins!"), "X player should see O wins")
        }

    [<Test>]
    member this.``Game ends in draw when board is full``() : Task =
        task {
            do! TestHelpers.waitForVisible this.Page ".square-clickable" this.TimeoutMs

            // Get game URL for second player
            let gameBoard = this.Page.Locator(".game-board").First
            let! gameId = gameBoard.GetAttributeAsync("id")
            let gameIdValue = gameId.Substring("game-".Length)
            let gameUrl = $"{this.BaseUrl}/games/{gameIdValue}"

            // Create second player (O)
            let! playerO = this.CreateSecondPlayer(gameUrl)
            do! TestHelpers.waitForVisible playerO ".game-board" this.TimeoutMs

            // Play to a draw:
            // X | O | X
            // X | X | O
            // O | X | O

            // X: TopLeft (index 0)
            do! this.Page.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: TopCenter (index 0 after X took one)
            do! playerO.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs

            // X: MiddleLeft (index 1)
            do! this.Page.Locator(".square-clickable").Nth(1).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: MiddleRight (index 2)
            do! playerO.Locator(".square-clickable").Nth(2).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs

            // X: TopRight (index 0)
            do! this.Page.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: BottomLeft (index 1)
            do! playerO.Locator(".square-clickable").Nth(1).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs

            // X: BottomCenter (index 1)
            do! this.Page.Locator(".square-clickable").Nth(1).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: BottomRight (index 1)
            do! playerO.Locator(".square-clickable").Nth(1).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs

            // X: MiddleCenter - final move, draw! (index 0)
            do! this.Page.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "draw" this.TimeoutMs

            // Verify both players see draw
            let! statusTextX = this.Page.Locator(".status").TextContentAsync()
            Assert.That(statusTextX, Does.Contain("draw"), "X player should see draw")

            do! TestHelpers.waitForTextContains playerO ".status" "draw" this.TimeoutMs
            let! statusTextO = playerO.Locator(".status").TextContentAsync()
            Assert.That(statusTextO, Does.Contain("draw"), "O player should see draw")

            // Verify no squares are clickable
            let! clickableCount = this.Page.Locator(".square-clickable").CountAsync()
            Assert.That(clickableCount, Is.EqualTo(0), "No squares should be clickable after draw")
        }

    [<Test>]
    member this.``Board squares are not clickable after game ends``() : Task =
        task {
            do! TestHelpers.waitForVisible this.Page ".square-clickable" this.TimeoutMs

            // Get game URL for second player
            let gameBoard = this.Page.Locator(".game-board").First
            let! gameId = gameBoard.GetAttributeAsync("id")
            let gameIdValue = gameId.Substring("game-".Length)
            let gameUrl = $"{this.BaseUrl}/games/{gameIdValue}"

            // Create second player (O)
            let! playerO = this.CreateSecondPlayer(gameUrl)
            do! TestHelpers.waitForVisible playerO ".game-board" this.TimeoutMs

            // Play to X win with two players
            // X: TopLeft
            do! this.Page.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: MiddleLeft
            do! playerO.Locator(".square-clickable").Nth(2).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs

            // X: TopCenter
            do! this.Page.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: MiddleCenter
            do! playerO.Locator(".square-clickable").Nth(2).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs

            // X: TopRight - wins!
            do! this.Page.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "wins" this.TimeoutMs

            // Verify no squares are clickable for either player
            let! clickableCountX = this.Page.Locator(".square-clickable").CountAsync()
            Assert.That(clickableCountX, Is.EqualTo(0), "No squares should be clickable for X after game ends")

            do! TestHelpers.waitForTextContains playerO ".status" "wins" this.TimeoutMs
            let! clickableCountO = playerO.Locator(".square-clickable").CountAsync()
            Assert.That(clickableCountO, Is.EqualTo(0), "No squares should be clickable for O after game ends")
        }

    // ============================================================================
    // Edge Cases
    // ============================================================================

    [<Test>]
    member this.``Rapid clicking same square only registers once``() : Task =
        task {
            do! TestHelpers.waitForVisible this.Page ".square-clickable" this.TimeoutMs

            // Click the first square
            let square = this.Page.Locator(".square-clickable").First
            do! square.ClickAsync()

            // Wait for board to stabilize
            do! Task.Delay(500)

            // Should only have one X placed
            let! playerCount = this.Page.Locator(".player").CountAsync()
            Assert.That(playerCount, Is.EqualTo(1), "Click should register exactly one move")
        }

    [<Test>]
    member this.``Third player (spectator) cannot make moves``() : Task =
        task {
            do! TestHelpers.waitForVisible this.Page ".square-clickable" this.TimeoutMs

            // Get game URL for other players
            let gameBoard = this.Page.Locator(".game-board").First
            let! gameId = gameBoard.GetAttributeAsync("id")
            let gameIdValue = gameId.Substring("game-".Length)
            let gameUrl = $"{this.BaseUrl}/games/{gameIdValue}"

            // Create second player (O)
            let! playerO = this.CreateSecondPlayer(gameUrl)
            do! TestHelpers.waitForVisible playerO ".game-board" this.TimeoutMs

            // Create third player (spectator)
            let! spectator = this.CreateSecondPlayer(gameUrl)
            do! TestHelpers.waitForVisible spectator ".game-board" this.TimeoutMs

            // X makes first move to establish as player X
            do! this.Page.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs
            do! TestHelpers.waitForTextContains spectator ".status" "O's turn" this.TimeoutMs

            // O makes move to establish as player O
            do! playerO.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs

            // Count moves before spectator attempts
            let! movesBefore = this.Page.Locator(".player").CountAsync()
            Assert.That(movesBefore, Is.EqualTo(2), "Should have 2 moves before spectator attempt")

            // Spectator tries to click - should be rejected (403)
            do! spectator.Locator(".square-clickable").Nth(0).ClickAsync()

            // Wait a moment for any potential update
            do! Task.Delay(500)

            // Verify move count unchanged - spectator's move was rejected
            let! movesAfter = this.Page.Locator(".player").CountAsync()
            Assert.That(movesAfter, Is.EqualTo(2), "Spectator's move should be rejected - count unchanged")

            // Verify it's still X's turn (not O's, which would mean spectator's move went through)
            let! statusText = this.Page.Locator(".status").TextContentAsync()
            Assert.That(statusText, Does.Contain("X's turn"), "Should still be X's turn after spectator rejection")
        }
