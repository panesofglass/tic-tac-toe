namespace TicTacToe.Web.Tests

open System.Threading.Tasks
open NUnit.Framework

[<TestFixture>]
type GamePlayTests() =
    inherit TestBase()

    /// Resets the game by sending a DELETE request and reloading
    member private this.ResetGame() : Task =
        task {
            // Send DELETE to reset server-side game state
            do! this.Page.EvaluateAsync("() => fetch('/', { method: 'DELETE' })") |> Async.AwaitTask |> Async.Ignore
            // Reload page to get fresh state with new SSE connection
            let! _ = this.Page.ReloadAsync()
            ()
        }

    /// Additional setup after page navigation - ensure fresh game state
    [<SetUp>]
    member this.EnsureFreshGame() : Task =
        task {
            // Wait for initial board to appear
            do! TestHelpers.waitForVisible this.Page ".board" this.TimeoutMs
            // Check if game is not fresh (has moves or game over)
            let! clickableCount = this.Page.Locator(".square-clickable").CountAsync()
            if clickableCount <> 9 then
                // Reset and reload
                do! this.ResetGame()
                // Wait for fresh board
                do! TestHelpers.waitForCount this.Page ".square-clickable" 9 this.TimeoutMs
        }

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
    member this.``New Game button appears after game ends``() : Task =
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

            do! TestHelpers.waitForVisible this.Page ".new-game-btn" this.TimeoutMs
            let! isVisible = TestHelpers.isVisible this.Page ".new-game-btn"
            Assert.That(isVisible, Is.True, "New Game button should appear after game ends")
        }

    [<Test>]
    member this.``New Game button resets the board``() : Task =
        task {
            do! TestHelpers.waitForVisible this.Page ".square-clickable" this.TimeoutMs

            // Play to game end
            do! this.Page.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "O's turn" this.TimeoutMs
            do! this.Page.Locator(".square-clickable").Nth(2).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs
            do! this.Page.Locator(".square-clickable").Nth(0).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "O's turn" this.TimeoutMs
            do! this.Page.Locator(".square-clickable").Nth(2).ClickAsync()
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs
            do! this.Page.Locator(".square-clickable").Nth(0).ClickAsync()

            do! TestHelpers.waitForVisible this.Page ".new-game-btn" this.TimeoutMs
            do! this.Page.Locator(".new-game-btn").ClickAsync()

            do! TestHelpers.waitForCount this.Page ".square-clickable" 9 this.TimeoutMs
            do! TestHelpers.waitForTextContains this.Page ".status" "X's turn" this.TimeoutMs
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
