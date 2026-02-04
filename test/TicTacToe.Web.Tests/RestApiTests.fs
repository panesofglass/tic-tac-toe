namespace TicTacToe.Web.Tests

open System
open System.Net
open System.Net.Http
open System.Threading.Tasks
open NUnit.Framework

/// REST API tests for multi-game support.
/// Tests HTTP semantics without browser interaction.
[<TestFixture>]
type RestApiTests() =
    let mutable client: HttpClient = null

    let baseUrl =
        Environment.GetEnvironmentVariable("TEST_BASE_URL")
        |> Option.ofObj
        |> Option.filter (fun s -> not (String.IsNullOrEmpty(s)))
        |> Option.defaultValue "http://localhost:5000"

    [<OneTimeSetUp>]
    member _.Setup() =
        client <- new HttpClient(BaseAddress = Uri(baseUrl))

    [<OneTimeTearDown>]
    member _.Teardown() =
        if not (isNull client) then
            client.Dispose()
            client <- null

    // ============================================================================
    // User Story 1: Create and Play a New Game
    // ============================================================================

    [<Test>]
    member _.``POST /games returns 201 with Location header``() : Task =
        task {
            let! response = client.PostAsync("/games", null)

            Assert.That(int response.StatusCode, Is.EqualTo(201), "Should return 201 Created")
            Assert.That(response.Headers.Location, Is.Not.Null, "Should include Location header")
            Assert.That(response.Headers.Location.ToString(), Does.StartWith("/games/"), "Location should point to /games/{id}")
        }

    [<Test>]
    member _.``POST /games/{id} with valid move returns 202``() : Task =
        task {
            // First create a game
            let! createResponse = client.PostAsync("/games", null)
            let gameUrl = createResponse.Headers.Location.ToString()
            let gameId = gameUrl.Substring("/games/".Length)

            // Make a valid move (X at TopLeft)
            let signals = sprintf """{"gameId":"%s","player":"X","position":"TopLeft"}""" gameId
            let content = new StringContent(signals, Text.Encoding.UTF8, "application/json")
            content.Headers.ContentType.MediaType <- "application/json"

            use request = new HttpRequestMessage(HttpMethod.Post, gameUrl, Content = content)
            request.Headers.Add("datastar-request", "true")

            let! response = client.SendAsync(request)

            Assert.That(int response.StatusCode, Is.EqualTo(202), "Should return 202 Accepted for valid move")
        }

    [<Test>]
    member _.``POST /games/{id} on non-existent game returns 404``() : Task =
        task {
            let signals = """{"gameId":"nonexistent","player":"X","position":"TopLeft"}"""
            let content = new StringContent(signals, Text.Encoding.UTF8, "application/json")

            use request = new HttpRequestMessage(HttpMethod.Post, "/games/nonexistent-game-id", Content = content)
            request.Headers.Add("datastar-request", "true")

            let! response = client.SendAsync(request)

            Assert.That(int response.StatusCode, Is.EqualTo(404), "Should return 404 for non-existent game")
        }

    // ============================================================================
    // User Story 2: Game Has Unique Resource URL
    // ============================================================================

    [<Test>]
    member _.``GET /games/{id} returns 200 with HTML``() : Task =
        task {
            // First create a game
            let! createResponse = client.PostAsync("/games", null)
            let gameUrl = createResponse.Headers.Location.ToString()

            let! response = client.GetAsync(gameUrl)

            Assert.That(int response.StatusCode, Is.EqualTo(200), "Should return 200 OK")
            let! content = response.Content.ReadAsStringAsync()
            Assert.That(content, Does.Contain("game-board"), "Should contain game board HTML")
        }

    [<Test>]
    member _.``GET /games/{invalid-id} returns 404``() : Task =
        task {
            let! response = client.GetAsync("/games/nonexistent-game-id")

            Assert.That(int response.StatusCode, Is.EqualTo(404), "Should return 404 for non-existent game")
        }

    // ============================================================================
    // User Story 4: Delete/Remove a Game
    // ============================================================================

    [<Test>]
    member _.``DELETE /games/{id} returns 204``() : Task =
        task {
            // First create a game
            let! createResponse = client.PostAsync("/games", null)
            let gameUrl = createResponse.Headers.Location.ToString()

            let! response = client.DeleteAsync(gameUrl)

            Assert.That(int response.StatusCode, Is.EqualTo(204), "Should return 204 No Content")
        }

    [<Test>]
    member _.``DELETE /games/{invalid-id} returns 404``() : Task =
        task {
            let! response = client.DeleteAsync("/games/nonexistent-game-id")

            Assert.That(int response.StatusCode, Is.EqualTo(404), "Should return 404 for non-existent game")
        }

    [<Test>]
    member _.``GET after DELETE returns 404``() : Task =
        task {
            // Create a game
            let! createResponse = client.PostAsync("/games", null)
            let gameUrl = createResponse.Headers.Location.ToString()

            // Delete it
            let! _ = client.DeleteAsync(gameUrl)

            // Try to get it
            let! response = client.GetAsync(gameUrl)

            Assert.That(int response.StatusCode, Is.EqualTo(404), "Should return 404 after deletion")
        }
