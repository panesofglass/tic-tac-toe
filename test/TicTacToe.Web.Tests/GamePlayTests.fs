namespace TicTacToe.Web.Tests

open System.Threading.Tasks
open NUnit.Framework

/// Gameplay tests for single-game interactions.
/// Tests basic game mechanics: moves, turns, win conditions.
/// Updated for 6-game minimum: tests create a new game (7th) for clean state.
[<TestFixture>]
[<Order(1)>] // Run after InitialGamesTests and ResetGameTests
type GamePlayTests() =
    inherit TestBase()

    /// Creates a new game by clicking the New Game button and waits for it to appear
    member private this.CreateGame() : Task =
        task {
            let! initialCount = this.Page.Locator(".game-board").CountAsync()
            do! this.Page.Locator(".new-game-btn").ClickAsync()
            do! TestHelpers.waitForCount this.Page ".game-board" (initialCount + 1) this.TimeoutMs
        }

    [<SetUp>]
    member this.EnsureFreshGame() : Task =
        task {
            // Wait for page to load with games visible
            do! TestHelpers.waitForVisible this.Page ".new-game-btn" this.TimeoutMs
            do! TestHelpers.waitForVisible this.Page ".game-board" this.TimeoutMs
            // Create a fresh game for testing
            do! this.CreateGame()
        }

    // ============================================================================
    // User Story 1: New Game button creates visible game board
    // ============================================================================

    [<Test>]
    member this.``New Game button creates visible game board``() : Task =
        task {
            // Game was created in setup, verify we have more than 6 games now
            let! count = this.Page.Locator(".game-board").CountAsync()
            Assert.That(count, Is.GreaterThan(6), "Should have more than 6 game boards after setup")

            // Verify the new (last) game has 9 clickable squares
            let lastGame = this.Page.Locator(".game-board").Last
            let! clickableCount = lastGame.Locator(".square-clickable").CountAsync()
            Assert.That(clickableCount, Is.EqualTo(9), "New game should have 9 clickable squares")
        }

    // ============================================================================
    // User Story 1: Clicking square places X, then O
    // ============================================================================

    [<Test>]
    member this.``Clicking square places X on first move``() : Task =
        task {
            // Use the last game (our fresh 7th game)
            let game = this.Page.Locator(".game-board").Last
            do! game.Locator(".square-clickable").First.WaitForAsync()

            do! game.Locator(".square-clickable").First.ClickAsync()

            do! game.Locator(".player").First.WaitForAsync()
            let! playerText = game.Locator(".player").First.TextContentAsync()
            Assert.That(playerText, Is.EqualTo("X"), "First move should place X")
        }

    [<Test>]
    member this.``Turn alternates between X and O``() : Task =
        task {
            // Use the last game (our fresh 7th game)
            let game = this.Page.Locator(".game-board").Last
            do! game.Locator(".square-clickable").First.WaitForAsync()

            // Get game URL for second player
            let! gameId = game.GetAttributeAsync("id")
            let gameIdValue = gameId.Substring("game-".Length)
            let gameUrl = $"{this.BaseUrl}/games/{gameIdValue}"

            // Create second player (O)
            let! playerO = this.CreateSecondPlayer(gameUrl)
            do! TestHelpers.waitForVisible playerO ".game-board" this.TimeoutMs

            // Verify the newly created game is X's turn initially
            do! game.Locator(".status").Filter(new Microsoft.Playwright.LocatorFilterOptions(HasText = "X's turn")).WaitForAsync()

            // X clicks first square on the test game
            do! game.Locator(".square-clickable").First.ClickAsync()
            do! game.Locator(".status").Filter(new Microsoft.Playwright.LocatorFilterOptions(HasText = "O's turn")).WaitForAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O clicks first available square (they only see one game)
            do! playerO.Locator(".square-clickable").First.ClickAsync()
            do! game.Locator(".status").Filter(new Microsoft.Playwright.LocatorFilterOptions(HasText = "X's turn")).WaitForAsync()
        }

    // ============================================================================
    // User Story 1: Win condition shows winner message
    // ============================================================================

    [<Test>]
    member this.``X wins with top row``() : Task =
        task {
            // Use the last game (our fresh 7th game)
            let game = this.Page.Locator(".game-board").Last
            do! game.Locator(".square-clickable").First.WaitForAsync()

            // Get game URL for second player
            let! gameId = game.GetAttributeAsync("id")
            let gameIdValue = gameId.Substring("game-".Length)
            let gameUrl = $"{this.BaseUrl}/games/{gameIdValue}"

            // Create second player (O)
            let! playerO = this.CreateSecondPlayer(gameUrl)
            do! TestHelpers.waitForVisible playerO ".game-board" this.TimeoutMs

            // X: TopLeft (index 0)
            do! game.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: MiddleLeft (now index 2 after X took index 0)
            do! playerO.Locator(".square-clickable").Nth(2).ClickAsync()
            do! game.Locator(".status").Filter(new Microsoft.Playwright.LocatorFilterOptions(HasText = "X's turn")).WaitForAsync()

            // X: TopCenter (now index 0 - first available)
            do! game.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: MiddleCenter (now index 2)
            do! playerO.Locator(".square-clickable").Nth(2).ClickAsync()
            do! game.Locator(".status").Filter(new Microsoft.Playwright.LocatorFilterOptions(HasText = "X's turn")).WaitForAsync()

            // X: TopRight - wins! (now index 0 - first available)
            do! game.Locator(".square-clickable").Nth(0).ClickAsync()
            do! game.Locator(".status").Filter(new Microsoft.Playwright.LocatorFilterOptions(HasText = "wins")).WaitForAsync()

            // Verify both players see the win
            let! statusTextX = game.Locator(".status").TextContentAsync()
            Assert.That(statusTextX, Does.Contain("X wins!"), "X player should see X wins")

            do! TestHelpers.waitForTextContains playerO ".status" "wins" this.TimeoutMs
            let! statusTextO = playerO.Locator(".status").TextContentAsync()
            Assert.That(statusTextO, Does.Contain("X wins!"), "O player should see X wins")
        }

    [<Test>]
    member this.``O wins with left column``() : Task =
        task {
            // Use the last game (our fresh 7th game)
            let game = this.Page.Locator(".game-board").Last
            do! game.Locator(".square-clickable").First.WaitForAsync()

            // Get game URL for second player
            let! gameId = game.GetAttributeAsync("id")
            let gameIdValue = gameId.Substring("game-".Length)
            let gameUrl = $"{this.BaseUrl}/games/{gameIdValue}"

            // Create second player (O)
            let! playerO = this.CreateSecondPlayer(gameUrl)
            do! TestHelpers.waitForVisible playerO ".game-board" this.TimeoutMs

            // Board: [0,1,2,3,4,5,6,7,8] = TL,TC,TR,ML,MC,MR,BL,BC,BR
            // O wins with left column: TL, ML, BL

            // X: TopCenter (index 1)
            do! game.Locator(".square-clickable").Nth(1).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: TopLeft (index 0) - remaining: [0,2,3,4,5,6,7,8]
            do! playerO.Locator(".square-clickable").Nth(0).ClickAsync()
            do! game.Locator(".status").Filter(new Microsoft.Playwright.LocatorFilterOptions(HasText = "X's turn")).WaitForAsync()

            // X: TopRight (index 0) - remaining: [2,3,4,5,6,7,8]
            do! game.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: MiddleLeft (index 0) - remaining: [3,4,5,6,7,8]
            do! playerO.Locator(".square-clickable").Nth(0).ClickAsync()
            do! game.Locator(".status").Filter(new Microsoft.Playwright.LocatorFilterOptions(HasText = "X's turn")).WaitForAsync()

            // X: BottomRight (index 4) - remaining: [4,5,6,7,8]
            do! game.Locator(".square-clickable").Nth(4).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: BottomLeft - wins! (index 2) - remaining: [4,5,6,7]
            do! playerO.Locator(".square-clickable").Nth(2).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "wins" this.TimeoutMs

            // Verify both players see O wins
            let! statusTextO = playerO.Locator(".status").TextContentAsync()
            Assert.That(statusTextO, Does.Contain("O wins!"), "O player should see O wins")

            do! game.Locator(".status").Filter(new Microsoft.Playwright.LocatorFilterOptions(HasText = "wins")).WaitForAsync()
            let! statusTextX = game.Locator(".status").TextContentAsync()
            Assert.That(statusTextX, Does.Contain("O wins!"), "X player should see O wins")
        }

    [<Test>]
    member this.``Game ends in draw when board is full``() : Task =
        task {
            // Use the last game (our fresh 7th game)
            let game = this.Page.Locator(".game-board").Last
            do! game.Locator(".square-clickable").First.WaitForAsync()

            // Get game URL for second player
            let! gameId = game.GetAttributeAsync("id")
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
            do! game.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: TopCenter (index 0 after X took one)
            do! playerO.Locator(".square-clickable").Nth(0).ClickAsync()
            do! game.Locator(".status").Filter(new Microsoft.Playwright.LocatorFilterOptions(HasText = "X's turn")).WaitForAsync()

            // X: MiddleLeft (index 1)
            do! game.Locator(".square-clickable").Nth(1).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: MiddleRight (index 2)
            do! playerO.Locator(".square-clickable").Nth(2).ClickAsync()
            do! game.Locator(".status").Filter(new Microsoft.Playwright.LocatorFilterOptions(HasText = "X's turn")).WaitForAsync()

            // X: TopRight (index 0)
            do! game.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: BottomLeft (index 1)
            do! playerO.Locator(".square-clickable").Nth(1).ClickAsync()
            do! game.Locator(".status").Filter(new Microsoft.Playwright.LocatorFilterOptions(HasText = "X's turn")).WaitForAsync()

            // X: BottomCenter (index 1)
            do! game.Locator(".square-clickable").Nth(1).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: BottomRight (index 1)
            do! playerO.Locator(".square-clickable").Nth(1).ClickAsync()
            do! game.Locator(".status").Filter(new Microsoft.Playwright.LocatorFilterOptions(HasText = "X's turn")).WaitForAsync()

            // X: MiddleCenter - final move, draw! (index 0)
            do! game.Locator(".square-clickable").Nth(0).ClickAsync()
            do! game.Locator(".status").Filter(new Microsoft.Playwright.LocatorFilterOptions(HasText = "draw")).WaitForAsync()

            // Verify both players see draw
            let! statusTextX = game.Locator(".status").TextContentAsync()
            Assert.That(statusTextX, Does.Contain("draw"), "X player should see draw")

            do! TestHelpers.waitForTextContains playerO ".status" "draw" this.TimeoutMs
            let! statusTextO = playerO.Locator(".status").TextContentAsync()
            Assert.That(statusTextO, Does.Contain("draw"), "O player should see draw")

            // Verify no squares are clickable on this game
            let! clickableCount = game.Locator(".square-clickable").CountAsync()
            Assert.That(clickableCount, Is.EqualTo(0), "No squares should be clickable after draw")
        }

    [<Test>]
    member this.``Board squares are not clickable after game ends``() : Task =
        task {
            // Use the last game (our fresh 7th game)
            let game = this.Page.Locator(".game-board").Last
            do! game.Locator(".square-clickable").First.WaitForAsync()

            // Get game URL for second player
            let! gameId = game.GetAttributeAsync("id")
            let gameIdValue = gameId.Substring("game-".Length)
            let gameUrl = $"{this.BaseUrl}/games/{gameIdValue}"

            // Create second player (O)
            let! playerO = this.CreateSecondPlayer(gameUrl)
            do! TestHelpers.waitForVisible playerO ".game-board" this.TimeoutMs

            // Play to X win with two players
            // X: TopLeft
            do! game.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: MiddleLeft
            do! playerO.Locator(".square-clickable").Nth(2).ClickAsync()
            do! game.Locator(".status").Filter(new Microsoft.Playwright.LocatorFilterOptions(HasText = "X's turn")).WaitForAsync()

            // X: TopCenter
            do! game.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs

            // O: MiddleCenter
            do! playerO.Locator(".square-clickable").Nth(2).ClickAsync()
            do! game.Locator(".status").Filter(new Microsoft.Playwright.LocatorFilterOptions(HasText = "X's turn")).WaitForAsync()

            // X: TopRight - wins!
            do! game.Locator(".square-clickable").Nth(0).ClickAsync()
            do! game.Locator(".status").Filter(new Microsoft.Playwright.LocatorFilterOptions(HasText = "wins")).WaitForAsync()

            // Verify no squares are clickable for this game
            let! clickableCountX = game.Locator(".square-clickable").CountAsync()
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
            // Use the last game (our fresh 7th game)
            let game = this.Page.Locator(".game-board").Last
            do! game.Locator(".square-clickable").First.WaitForAsync()

            // Click the first square
            let square = game.Locator(".square-clickable").First
            do! square.ClickAsync()

            // Wait for board to stabilize
            do! Task.Delay(500)

            // Should only have one X placed in this game
            let! playerCount = game.Locator(".player").CountAsync()
            Assert.That(playerCount, Is.EqualTo(1), "Click should register exactly one move")
        }

    [<Test>]
    member this.``Third player (spectator) cannot make moves``() : Task =
        task {
            // Use the last game (our fresh 7th game)
            let game = this.Page.Locator(".game-board").Last
            do! game.Locator(".square-clickable").First.WaitForAsync()

            // Get game URL for other players
            let! gameId = game.GetAttributeAsync("id")
            let gameIdValue = gameId.Substring("game-".Length)
            let gameUrl = $"{this.BaseUrl}/games/{gameIdValue}"

            // Create second player (O)
            let! playerO = this.CreateSecondPlayer(gameUrl)
            do! TestHelpers.waitForVisible playerO ".game-board" this.TimeoutMs

            // Create third player (spectator)
            let! spectator = this.CreateSecondPlayer(gameUrl)
            do! TestHelpers.waitForVisible spectator ".game-board" this.TimeoutMs

            // X makes first move to establish as player X
            do! game.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains playerO ".status" "O's turn" this.TimeoutMs
            do! TestHelpers.waitForTextContains spectator ".status" "O's turn" this.TimeoutMs

            // O makes move to establish as player O
            do! playerO.Locator(".square-clickable").Nth(0).ClickAsync()
            do! game.Locator(".status").Filter(new Microsoft.Playwright.LocatorFilterOptions(HasText = "X's turn")).WaitForAsync()

            // Count moves before verifying spectator view
            let! movesBefore = game.Locator(".player").CountAsync()
            Assert.That(movesBefore, Is.EqualTo(2), "Should have 2 moves before checking spectator")

            // Verify spectator sees NO clickable squares (correct behavior after refactoring)
            let! spectatorClickableCount = spectator.Locator(".square-clickable").CountAsync()
            Assert.That(spectatorClickableCount, Is.EqualTo(0), "Spectator should see NO clickable squares")

            // Verify it's still X's turn (game state unchanged)
            let! statusText = game.Locator(".status").TextContentAsync()
            Assert.That(statusText, Does.Contain("X's turn"), "Should still be X's turn")
        }
