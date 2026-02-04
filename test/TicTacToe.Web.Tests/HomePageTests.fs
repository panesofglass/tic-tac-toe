namespace TicTacToe.Web.Tests

open System.Threading.Tasks
open NUnit.Framework

[<TestFixture>]
type HomePageTests() =
    inherit TestBase()

    /// Creates a new game by clicking the New Game button
    member private this.CreateGame() : Task =
        task {
            do! TestHelpers.waitForVisible this.Page ".new-game-btn" this.TimeoutMs
            do! this.Page.Locator(".new-game-btn").ClickAsync()
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
            do! TestHelpers.waitForVisible this.Page ".new-game-btn" this.TimeoutMs
            let! isVisible = TestHelpers.isVisible this.Page ".new-game-btn"
            Assert.That(isVisible, Is.True, "New Game button should be visible on page load")
        }

    [<Test>]
    member this.``Home page displays game board after clicking New Game``() : Task =
        task {
            do! this.CreateGame()
            let! isVisible = TestHelpers.isVisible this.Page ".board"
            Assert.That(isVisible, Is.True, "Board should be visible after creating game")
        }

    [<Test>]
    member this.``Game board has exactly 9 squares``() : Task =
        task {
            do! this.CreateGame()
            let! squares = this.Page.Locator(".square").CountAsync()
            Assert.That(squares, Is.EqualTo(9), "Board should have exactly 9 squares")
        }

    [<Test>]
    member this.``Empty board shows X turn status``() : Task =
        task {
            do! this.CreateGame()
            do! TestHelpers.waitForVisible this.Page ".status" this.TimeoutMs
            let! statusText = this.Page.Locator(".status").TextContentAsync()
            Assert.That(statusText, Does.Contain("X's turn"), "Initial status should show X's turn")
        }

    [<Test>]
    member this.``Empty board has 9 clickable squares``() : Task =
        task {
            do! this.CreateGame()
            let! clickableSquares = this.Page.Locator(".square-clickable").CountAsync()
            Assert.That(clickableSquares, Is.EqualTo(9), "Empty board should have 9 clickable squares")
        }
