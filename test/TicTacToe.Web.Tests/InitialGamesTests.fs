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
    member this.``Home page shows at least six game boards on load``() : Task =
        task {
            // Wait for at least 6 games to appear
            do! TestHelpers.waitForVisible this.Page ".game-board" this.TimeoutMs

            let! count = this.Page.Locator(".game-board").CountAsync()
            Assert.That(count, Is.GreaterThanOrEqualTo(6), "Home page should display at least six game boards")
        }

    [<Test>]
    member this.``Newly created games show X's turn``() : Task =
        task {
            // Create a fresh game to test
            do! this.Page.Locator(".new-game-btn").ClickAsync()
            do! Task.Delay(200)  // Wait for game to be created

            // Check the newly created game (last one)
            let game = this.Page.Locator(".game-board").Last
            let! status = game.Locator(".status").TextContentAsync()
            Assert.That(status, Does.Contain("X's turn"), "New game should show X's turn")
        }

    [<Test>]
    member this.``Newly created games have empty boards``() : Task =
        task {
            // Create a fresh game to test
            do! this.Page.Locator(".new-game-btn").ClickAsync()
            do! Task.Delay(200)  // Wait for game to be created

            // Check the newly created game (last one)
            let game = this.Page.Locator(".game-board").Last
            let! playerCount = game.Locator(".player").CountAsync()
            Assert.That(playerCount, Is.EqualTo(0), "New game should have no moves")
        }

    [<Test>]
    member this.``Newly created games have all squares clickable``() : Task =
        task {
            // Create a fresh game to test
            do! this.Page.Locator(".new-game-btn").ClickAsync()
            do! Task.Delay(200)  // Wait for game to be created

            // Check the newly created game (last one)
            let game = this.Page.Locator(".game-board").Last
            let! clickableCount = game.Locator(".square-clickable").CountAsync()
            Assert.That(clickableCount, Is.EqualTo(9), "New game should have 9 clickable squares")
        }
