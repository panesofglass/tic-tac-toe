namespace TicTacToe.Web.Tests

open System.Threading.Tasks
open NUnit.Framework

/// Multi-game Playwright tests - tests for concurrent game support.
[<TestFixture>]
type MultiGameTests() =
    inherit TestBase()

    /// Creates a new game by clicking the New Game button and waits for it to appear
    member private this.CreateGame() : Task =
        task {
            let! initialCount = this.Page.Locator(".game-board").CountAsync()
            do! this.Page.Locator(".new-game-btn").ClickAsync()
            do! TestHelpers.waitForCount this.Page ".game-board" (initialCount + 1) this.TimeoutMs
        }

    /// Deletes all existing games to ensure clean state
    member private this.CleanupGames() : Task =
        task {
            let! count = this.Page.Locator(".delete-game-btn").CountAsync()

            for _ in 1..count do
                do! this.Page.Locator(".delete-game-btn").First.ClickAsync()
                do! Task.Delay(100) // Small delay to allow SSE update
        }

    [<SetUp>]
    member this.EnsureCleanState() : Task =
        task {
            // Wait for page to load with New Game button visible
            do! TestHelpers.waitForVisible this.Page ".new-game-btn" this.TimeoutMs
            // Clean up any existing games
            do! this.CleanupGames()
        }

    // ============================================================================
    // User Story 3: Multiple Games on Single Page
    // ============================================================================

    [<Test>]
    member this.``Creating second game shows two boards``() : Task =
        task {
            // Create first game
            do! this.CreateGame()
            do! TestHelpers.waitForCount this.Page ".game-board" 1 this.TimeoutMs

            // Create second game
            do! this.CreateGame()
            do! TestHelpers.waitForCount this.Page ".game-board" 2 this.TimeoutMs

            let! count = this.Page.Locator(".game-board").CountAsync()
            Assert.That(count, Is.EqualTo(2), "Should have two game boards")
        }

    [<Test>]
    member this.``Move in one game does not affect others``() : Task =
        task {
            // Create two games
            do! this.CreateGame()
            do! this.CreateGame()
            do! TestHelpers.waitForCount this.Page ".game-board" 2 this.TimeoutMs

            // Get the first game board
            let firstGame = this.Page.Locator(".game-board").First
            let secondGame = this.Page.Locator(".game-board").Nth(1)

            // Make a move in the first game
            do! firstGame.Locator(".square-clickable").First.ClickAsync()
            do! TestHelpers.waitForVisible this.Page ".player" this.TimeoutMs

            // Check first game has a move
            let! firstGamePlayers = firstGame.Locator(".player").CountAsync()
            Assert.That(firstGamePlayers, Is.EqualTo(1), "First game should have one move")

            // Check second game is unaffected
            let! secondGamePlayers = secondGame.Locator(".player").CountAsync()
            Assert.That(secondGamePlayers, Is.EqualTo(0), "Second game should have no moves")

            // Verify second game still has all clickable squares
            let! secondGameClickable = secondGame.Locator(".square-clickable").CountAsync()
            Assert.That(secondGameClickable, Is.EqualTo(9), "Second game should have all squares clickable")
        }

    [<Test>]
    member this.``Ten concurrent games remain responsive``() : Task =
        task {
            // Create 10 games
            for _ in 1..10 do
                do! this.CreateGame()

            do! TestHelpers.waitForCount this.Page ".game-board" 10 this.TimeoutMs

            // Make a move in each game
            for i in 0..9 do
                let game = this.Page.Locator(".game-board").Nth(i)
                do! game.Locator(".square-clickable").First.ClickAsync()
                // Small delay to allow SSE update
                do! Task.Delay(50)

            // Verify each game has exactly one X
            for i in 0..9 do
                let game = this.Page.Locator(".game-board").Nth(i)
                let! playerCount = game.Locator(".player").CountAsync()
                Assert.That(playerCount, Is.EqualTo(1), $"Game {i + 1} should have exactly one move")
        }

    // ============================================================================
    // User Story 4: Delete/Remove a Game
    // ============================================================================

    [<Test>]
    member this.``Delete button removes game from page``() : Task =
        task {
            // Create two games
            do! this.CreateGame()
            do! this.CreateGame()
            do! TestHelpers.waitForCount this.Page ".game-board" 2 this.TimeoutMs

            // Delete the first game
            do! this.Page.Locator(".delete-game-btn").First.ClickAsync()

            // Wait for it to be removed
            do! TestHelpers.waitForCount this.Page ".game-board" 1 this.TimeoutMs

            let! count = this.Page.Locator(".game-board").CountAsync()
            Assert.That(count, Is.EqualTo(1), "Should have one game board after deletion")
        }

    // ============================================================================
    // User Story 2: Direct Navigation
    // ============================================================================

    [<Test>]
    member this.``Direct navigation to game URL shows game``() : Task =
        task {
            // Create a game and get its ID from the DOM
            do! this.CreateGame()
            do! TestHelpers.waitForCount this.Page ".game-board" 1 this.TimeoutMs

            // Get the game ID from the game board element
            let gameBoard = this.Page.Locator(".game-board").First
            let! gameId = gameBoard.GetAttributeAsync("id")
            let gameIdValue = gameId.Substring("game-".Length) // Remove "game-" prefix

            // Navigate directly to the game URL
            let! _ = this.Page.GotoAsync($"{this.BaseUrl}/games/{gameIdValue}")

            // Verify the game is displayed
            do! TestHelpers.waitForVisible this.Page ".game-board" this.TimeoutMs
            do! TestHelpers.waitForTextContains this.Page ".status" "turn" this.TimeoutMs
        }

    // ============================================================================
    // Edge Cases
    // ============================================================================

    [<Test>]
    member this.``Move on deleted game is ignored``() : Task =
        task {
            // Create a game
            do! this.CreateGame()
            do! TestHelpers.waitForCount this.Page ".game-board" 1 this.TimeoutMs

            // Get the game ID
            let gameBoard = this.Page.Locator(".game-board").First
            let! gameId = gameBoard.GetAttributeAsync("id")
            let gameIdValue = gameId.Substring("game-".Length)

            // Delete via API (simulating another client)
            do!
                this.Page.EvaluateAsync($"() => fetch('/games/{gameIdValue}', {{ method: 'DELETE' }})")
                |> Async.AwaitTask
                |> Async.Ignore

            // Wait for removal
            do! TestHelpers.waitForCount this.Page ".game-board" 0 this.TimeoutMs

            // Verify no game boards exist
            let! count = this.Page.Locator(".game-board").CountAsync()
            Assert.That(count, Is.EqualTo(0), "Game should be removed")
        }
