namespace TicTacToe.Web.Tests

open System
open System.Text.RegularExpressions
open System.Threading.Tasks
open NUnit.Framework
open Microsoft.Playwright

/// User identity display tests - verifies the user's short ID is shown in the page header.
[<TestFixture>]
type UserIdentityTests() =
    inherit TestBase()

    // ============================================================================
    // T005: User identity visible after login
    // ============================================================================

    [<Test>]
    member this.``User identity is visible in page header after login``() : Task =
        task {
            // Wait for page to load
            do! TestHelpers.waitForVisible this.Page ".user-identity" this.TimeoutMs

            let userIdentity = this.Page.Locator(".user-identity")
            let! isVisible = userIdentity.IsVisibleAsync()
            Assert.That(isVisible, Is.True, "User identity should be visible in the page header")
        }

    [<Test>]
    member this.``User identity contains 8-character hex string``() : Task =
        task {
            // Wait for the user identity element to appear
            do! TestHelpers.waitForVisible this.Page ".user-identity" this.TimeoutMs

            let! text = this.Page.Locator(".user-identity").TextContentAsync()
            let trimmed = text.Trim()
            Assert.That(trimmed.Length, Is.EqualTo(8), $"User identity should be 8 characters, got '{trimmed}' ({trimmed.Length} chars)")
            Assert.That(
                Regex.IsMatch(trimmed, "^[0-9a-fA-F]{8}$"),
                Is.True,
                $"User identity should match hex/alphanumeric format, got '{trimmed}'"
            )
        }

    // ============================================================================
    // T006: No user identity on login page (unauthenticated)
    // ============================================================================

    [<Test>]
    member this.``User identity element contains the same ID across page navigations``() : Task =
        task {
            // Verify user identity is consistent - same ID on home page after re-navigation
            do! TestHelpers.waitForVisible this.Page ".user-identity" this.TimeoutMs
            let! id1 = this.Page.Locator(".user-identity").TextContentAsync()

            // Navigate away and back
            let options = PageGotoOptions(Timeout = Nullable(float32 this.TimeoutMs))
            let! _ = this.Page.GotoAsync(this.BaseUrl, options)
            do! TestHelpers.waitForVisible this.Page ".user-identity" this.TimeoutMs
            let! id2 = this.Page.Locator(".user-identity").TextContentAsync()

            Assert.That(id1.Trim(), Is.EqualTo(id2.Trim()), "User identity should remain consistent across navigations")
        }
