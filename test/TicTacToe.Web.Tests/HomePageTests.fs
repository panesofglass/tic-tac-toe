namespace TicTacToe.Web.Tests

open System.Threading.Tasks
open NUnit.Framework

/// Home page tests - basic page structure and functionality.
/// Updated for 6-game minimum: page loads with 6 initial games.
[<TestFixture>]
[<Order(3)>] // Run after game logic tests
type HomePageTests() =
    inherit TestBase()

    /// Creates a new game by clicking the New Game button and waits for it to appear
    member private this.CreateGame() : Task =
        task {
            let! initialCount = this.Page.Locator(".game-board").CountAsync()
            do! this.Page.Locator(".new-game-btn").ClickAsync()
            do! TestHelpers.waitForCount this.Page ".game-board" (initialCount + 1) this.TimeoutMs
        }

    [<SetUp>]
    member this.EnsurePageLoaded() : Task =
        task {
            // Wait for page to load with at least 6 games (may be more from previous tests)
            do! TestHelpers.waitForVisible this.Page ".new-game-btn" this.TimeoutMs
            do! TestHelpers.waitForVisible this.Page ".game-board" this.TimeoutMs
        }

    [<Test>]
    member this.``Home page loads with title``() : Task =
        task {
            let! title = this.Page.TitleAsync()
            Assert.That(title, Does.Contain("Tic Tac Toe"))
        }

    [<Test>]
    member this.``Home page shows New Game button``() : Task =
        task {
            let! isVisible = TestHelpers.isVisible this.Page ".new-game-btn"
            Assert.That(isVisible, Is.True, "New Game button should be visible on page load")
        }

    [<Test>]
    member this.``Home page displays game boards on load``() : Task =
        task {
            // With 6 initial games (may have more from previous tests)
            let! count = this.Page.Locator(".game-board").CountAsync()
            Assert.That(count, Is.GreaterThanOrEqualTo(6), "Should have at least 6 game boards on load")
            let! isVisible = this.Page.Locator(".board").First.IsVisibleAsync()
            Assert.That(isVisible, Is.True, "Board should be visible on page load")
        }

    [<Test>]
    member this.``Game board has exactly 9 squares``() : Task =
        task {
            // Check the first initial game
            let game = this.Page.Locator(".game-board").First
            let! squares = game.Locator(".square").CountAsync()
            Assert.That(squares, Is.EqualTo(9), "Board should have exactly 9 squares")
        }

    [<Test>]
    member this.``Empty board shows X turn status``() : Task =
        task {
            // Check the first initial game
            let game = this.Page.Locator(".game-board").First
            let! statusText = game.Locator(".status").TextContentAsync()
            Assert.That(statusText, Does.Contain("X's turn"), "Initial status should show X's turn")
        }

    [<Test>]
    member this.``Empty board has 9 clickable squares``() : Task =
        task {
            // Create a fresh game to avoid interference from prior tests
            do! this.CreateGame()
            let game = this.Page.Locator(".game-board").Last
            let! clickableSquares = game.Locator(".square-clickable").CountAsync()
            Assert.That(clickableSquares, Is.EqualTo(9), "Empty board should have 9 clickable squares")
        }

    [<Test>]
    member this.``Creating new game increments game count``() : Task =
        task {
            let! initialCount = this.Page.Locator(".game-board").CountAsync()
            do! this.CreateGame()
            let! newCount = this.Page.Locator(".game-board").CountAsync()
            Assert.That(newCount, Is.EqualTo(initialCount + 1), "Should have one more game after creating")
        }
