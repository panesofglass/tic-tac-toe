namespace TicTacToe.Web.Tests

open System.Threading.Tasks
open NUnit.Framework

/// Initial games Playwright tests - tests for six-game startup behavior.
/// Note: These tests check initial server state and must run before tests that modify games.
[<TestFixture>]
[<Order(-1)>] // Run before ResetGameTests to ensure fresh state
type InitialGamesTests() =
    inherit TestBase()

    [<SetUp>]
    member this.EnsureCleanState() : Task =
        task {
            // Wait for page to load with New Game button visible
            do! TestHelpers.waitForVisible this.Page ".new-game-btn" this.TimeoutMs
        }

    // ============================================================================
    // User Story 2: Initial Page Load with Six Games
    // ============================================================================

    [<Test>]
    member this.``Home page shows exactly six game boards on load``() : Task =
        task {
            // Wait for games to appear
            do! TestHelpers.waitForCount this.Page ".game-board" 6 this.TimeoutMs

            let! count = this.Page.Locator(".game-board").CountAsync()
            Assert.That(count, Is.EqualTo(6), "Home page should display exactly six game boards")
        }

    [<Test>]
    member this.``All six initial games show X's turn``() : Task =
        task {
            // Wait for games to appear
            do! TestHelpers.waitForCount this.Page ".game-board" 6 this.TimeoutMs

            // Verify each game shows X's turn
            for i in 0..5 do
                let game = this.Page.Locator(".game-board").Nth(i)
                let! status = game.Locator(".status").TextContentAsync()
                Assert.That(status, Does.Contain("X's turn"), $"Game {i + 1} should show X's turn")
        }

    [<Test>]
    member this.``All six initial games have empty boards``() : Task =
        task {
            // Wait for games to appear
            do! TestHelpers.waitForCount this.Page ".game-board" 6 this.TimeoutMs

            // Verify each game has no moves
            for i in 0..5 do
                let game = this.Page.Locator(".game-board").Nth(i)
                let! playerCount = game.Locator(".player").CountAsync()
                Assert.That(playerCount, Is.EqualTo(0), $"Game {i + 1} should have no moves")
        }

    [<Test>]
    member this.``All six initial games have all squares clickable``() : Task =
        task {
            // Wait for games to appear
            do! TestHelpers.waitForCount this.Page ".game-board" 6 this.TimeoutMs

            // Verify each game has 9 clickable squares
            for i in 0..5 do
                let game = this.Page.Locator(".game-board").Nth(i)
                let! clickableCount = game.Locator(".square-clickable").CountAsync()
                Assert.That(clickableCount, Is.EqualTo(9), $"Game {i + 1} should have 9 clickable squares")
        }
