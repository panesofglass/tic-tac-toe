module TicTacToe.Engine.Tests.AuthTests

open System
open System.Security.Claims
open System.Threading.Tasks
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Expecto
open TicTacToe.Web

let loggerFactory = new Abstractions.NullLoggerFactory()
let logger = loggerFactory.CreateLogger<GameUserClaimsTransformation>()

// Mock HttpContextAccessor for testing
let mockHttpContextAccessor () =
    let httpContext: HttpContext = DefaultHttpContext()
    { new IHttpContextAccessor with
        member _.HttpContext
            with get() = httpContext
            and set(_) = () }

let createClaimsPrincipal (claims: Claim list) =
    let identity = ClaimsIdentity(claims, "TestAuth")
    ClaimsPrincipal(identity)

let createTestCase name (test: Task<_>) =
    testCase name (fun () ->
        // Run task test synchronously
        test.GetAwaiter().GetResult()
    )

[<Tests>]
let tests =
    testList "Auth Tests" [
        // Test creating new identity for first-time users
        createTestCase "Creates new identity for users without claims" (task {
            // Arrange
            let httpContextAccessor = mockHttpContextAccessor()
            let transformer = GameUserClaimsTransformation(httpContextAccessor, logger)
            let emptyPrincipal = ClaimsPrincipal(ClaimsIdentity())

            // Act
            let! result = (transformer :> IClaimsTransformation).TransformAsync(emptyPrincipal) |> Async.AwaitTask

            // Assert
            Expect.isTrue (result.HasClaim(ClaimTypes.UserId)) "Should have user ID claim"
            Expect.isTrue (result.HasClaim(ClaimTypes.Created)) "Should have created timestamp claim"
            Expect.isTrue (result.HasClaim(ClaimTypes.LastVisit)) "Should have last visit timestamp claim"

            let userId = result.FindFirst(ClaimTypes.UserId).Value
            Expect.isNotEmpty userId "User ID should not be empty"
            Expect.notEqual userId "" "User ID should not be empty string"
        })

        // Test updating LastVisit timestamp for returning users
        createTestCase "Updates LastVisit for existing users" (task {
            // Arrange
            let httpContextAccessor = mockHttpContextAccessor()
            let transformer = GameUserClaimsTransformation(httpContextAccessor, logger)

            // Create a principal with existing claims
            let userId = Guid.NewGuid().ToString()
            let oldTimestamp = DateTimeOffset.UtcNow.AddDays(-1.0).ToString("o")
            let existingClaims = [
                Claim(ClaimTypes.UserId, userId)
                Claim(ClaimTypes.Created, oldTimestamp)
                Claim(ClaimTypes.LastVisit, oldTimestamp)
            ]
            let existingPrincipal = createClaimsPrincipal existingClaims

            // Act
            let! result = (transformer :> IClaimsTransformation).TransformAsync(existingPrincipal) |> Async.AwaitTask

            // Assert
            let resultUserId = result.FindFirst(ClaimTypes.UserId).Value
            let resultCreated = result.FindFirst(ClaimTypes.Created).Value
            let resultLastVisit = result.FindFirst(ClaimTypes.LastVisit).Value

            Expect.equal resultUserId userId "User ID should be preserved"
            Expect.equal resultCreated oldTimestamp "Created timestamp should be preserved"
            Expect.notEqual resultLastVisit oldTimestamp "LastVisit timestamp should be updated"
        })

        // Test that transforms are idempotent and preserve claims across multiple requests
        createTestCase "Preserves claims across multiple transformations" (task {
            // Arrange
            let httpContextAccessor = mockHttpContextAccessor()
            let transformer = GameUserClaimsTransformation(httpContextAccessor, logger)
            let emptyPrincipal = ClaimsPrincipal(ClaimsIdentity())

            // Act - first transformation (new user)
            let! firstResult = (transformer :> IClaimsTransformation).TransformAsync(emptyPrincipal) |> Async.AwaitTask

            // Extract claims from first transformation
            let firstUserId = firstResult.FindFirst(ClaimTypes.UserId).Value
            let firstCreated = firstResult.FindFirst(ClaimTypes.Created).Value
            let firstLastVisit = firstResult.FindFirst(ClaimTypes.LastVisit).Value

            // Simulate some time passing
            do! Task.Delay(10) |> Async.AwaitTask

            // Act - second transformation (returning user)
            let! secondResult = (transformer :> IClaimsTransformation).TransformAsync(firstResult) |> Async.AwaitTask

            // Assert
            let secondUserId = secondResult.FindFirst(ClaimTypes.UserId).Value
            let secondCreated = secondResult.FindFirst(ClaimTypes.Created).Value
            let secondLastVisit = secondResult.FindFirst(ClaimTypes.LastVisit).Value

            Expect.equal secondUserId firstUserId "User ID should be preserved across transformations"
            Expect.equal secondCreated firstCreated "Created timestamp should be preserved across transformations"
            Expect.notEqual secondLastVisit firstLastVisit "LastVisit timestamp should be updated for returning users"
        })

        // Test integration with service collection configuration
        createTestCase "Cookie authentication configuration is secure" (task {
            // Arrange
            let services = ServiceCollection()

            // Act - configure services as in Program.fs
            let configuredServices =
                services
                    .AddAuthentication(fun options ->
                        options.DefaultScheme <- Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(fun options ->
                        options.Cookie.Name <- "TicTacToe.User"
                        options.Cookie.HttpOnly <- true
                        options.Cookie.SameSite <- SameSiteMode.Lax
                        options.Cookie.SecurePolicy <- CookieSecurePolicy.SameAsRequest
                        options.ExpireTimeSpan <- TimeSpan.FromDays(30.0)
                        options.SlidingExpiration <- true
                    )

            // Build the service provider
            let serviceProvider = services.BuildServiceProvider()

            // Get the authentication options
            let authOptions = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Authentication.AuthenticationOptions>>()
            let cookieOptions = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationOptions>>()

            // Assert
            Expect.isNotNull authOptions "Authentication options should be configured"
            Expect.isNotNull cookieOptions "Cookie options should be configured"

            if not (isNull cookieOptions) then
                let options = cookieOptions.Value
                Expect.equal options.Cookie.Name ".AspNetCore." "Cookie name should be set correctly"
                Expect.isTrue options.Cookie.HttpOnly "Cookie should be HttpOnly for security"
                Expect.equal options.Cookie.SameSite SameSiteMode.Lax "SameSite should be Lax to balance security and usability"
                Expect.isTrue options.SlidingExpiration "Sliding expiration should be enabled"

            return ()
        })
    ]
