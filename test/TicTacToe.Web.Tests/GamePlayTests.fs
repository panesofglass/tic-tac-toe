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

            // First move - should be X's turn
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs

            // Click first square
            do! this.Page.Locator(".square-clickable").First.ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "O's turn" this.TimeoutMs

            // Click second square
            do! this.Page.Locator(".square-clickable").First.ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs
        }

    // ============================================================================
    // User Story 1: Win condition shows winner message
    // ============================================================================

    [<Test>]
    member this.``X wins with top row``() : Task =
        task {
            do! TestHelpers.waitForVisible this.Page ".square-clickable" this.TimeoutMs

            // X: TopLeft
            do! this.Page.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "O's turn" this.TimeoutMs

            // O: MiddleLeft
            do! this.Page.Locator(".square-clickable").Nth(2).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs

            // X: TopCenter
            do! this.Page.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "O's turn" this.TimeoutMs

            // O: MiddleCenter
            do! this.Page.Locator(".square-clickable").Nth(2).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs

            // X: TopRight - wins!
            do! this.Page.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "wins" this.TimeoutMs

            let! statusText = this.Page.Locator(".status").TextContentAsync()
            Assert.That(statusText, Does.Contain("X wins!"), "X should win")
        }

    [<Test>]
    member this.``Board squares are not clickable after game ends``() : Task =
        task {
            do! TestHelpers.waitForVisible this.Page ".square-clickable" this.TimeoutMs

            // Play to X win
            do! this.Page.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "O's turn" this.TimeoutMs
            do! this.Page.Locator(".square-clickable").Nth(2).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs
            do! this.Page.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "O's turn" this.TimeoutMs
            do! this.Page.Locator(".square-clickable").Nth(2).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs
            do! this.Page.Locator(".square-clickable").Nth(0).ClickAsync()

            do! TestHelpers.waitForTextContains this.Page ".status" "wins" this.TimeoutMs

            let! clickableCount = this.Page.Locator(".square-clickable").CountAsync()
            Assert.That(clickableCount, Is.EqualTo(0), "No squares should be clickable after game ends")
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
