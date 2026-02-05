module TicTacToe.Web.Handlers

open System
open System.Threading.Channels
open System.Threading.Tasks
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Oxpecker.ViewEngine
open Frank.Datastar
open TicTacToe.Web.SseBroadcast
open TicTacToe.Web.templates.shared
open TicTacToe.Web.templates.game
open TicTacToe.Engine
open TicTacToe.Model
open TicTacToe.Web.Model

/// Signals type for move data from Datastar
[<CLIMutable>]
type MoveSignals =
    { gameId: string
      player: string
      position: string }

/// Helper to wrap handlers that require authentication.
/// Checks for valid auth cookie and redirects to /login if not present.
let requiresAuth (handler: HttpContext -> Task<unit>) (ctx: HttpContext) : Task<unit> =
    task {
        // Check if the user has a valid auth cookie (not just claims from transformation)
        let! authResult = ctx.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme)
        if authResult.Succeeded then
            do! handler ctx
        else
            // Issue challenge - will redirect to /login due to LoginPath setting
            do! ctx.ChallengeAsync()
    }

// Active game subscriptions - maps gameId to subscription disposable
let private gameSubscriptions =
    System.Collections.Concurrent.ConcurrentDictionary<string, IDisposable>()

/// Subscribe to a game's state changes and broadcast updates
let private subscribeToGame (gameId: string) (game: Game) =
    if not (gameSubscriptions.ContainsKey(gameId)) then
        let subscription =
            game.Subscribe(
                { new IObserver<MoveResult> with
                    member _.OnNext(result) =
                        let html = renderGameBoard gameId result |> Render.toString
                        broadcast (PatchElements html)

                    member _.OnError(_) = ()

                    member _.OnCompleted() =
                        // Game completed - remove subscription
                        match gameSubscriptions.TryRemove(gameId) with
                        | true, sub -> sub.Dispose()
                        | _ -> () }
            )

        gameSubscriptions.TryAdd(gameId, subscription) |> ignore

/// Login endpoint - signs in user and redirects back
/// This creates a persistent cookie for user identification
let login (ctx: HttpContext) =
    task {
        // Check if already authenticated
        let! authResult = ctx.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme)

        if not authResult.Succeeded then
            // Create a new user identity with a unique ID
            let userId = Guid.NewGuid().ToString()
            let claims = [|
                System.Security.Claims.Claim(ClaimTypes.UserId, userId)
                System.Security.Claims.Claim(ClaimTypes.Created, DateTimeOffset.UtcNow.ToString("o"))
            |]
            let identity = System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)
            let principal = System.Security.Claims.ClaimsPrincipal(identity)

            // Sign in the user
            do! ctx.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                AuthenticationProperties(IsPersistent = true))

        // Redirect back to the return URL or home
        let returnUrl =
            match ctx.Request.Query.TryGetValue("returnUrl") with
            | true, values when values.Count > 0 -> values.[0]
            | _ -> "/"

        ctx.Response.Redirect(returnUrl)
    }

/// Debug endpoint to show claims (temporary)
let debug (ctx: HttpContext) =
    task {
        // Explicitly try to authenticate
        let! authResult = ctx.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme)

        let claims = ctx.User.Claims |> Seq.map (fun c -> $"{c.Type}: {c.Value}") |> String.concat "\n"
        let userId = ctx.User.TryGetUserId()
        let isAuth = ctx.User.Identity.IsAuthenticated

        let authClaims =
            if authResult.Succeeded then
                authResult.Principal.Claims |> Seq.map (fun c -> $"{c.Type}: {c.Value}") |> String.concat "\n"
            else
                $"Auth failed: {authResult.Failure}"

        let response = $"IsAuthenticated: {isAuth}\nUserId: {userId}\nClaims:\n{claims}\n\nExplicit Auth Result:\n{authClaims}"
        ctx.Response.ContentType <- "text/plain"
        do! ctx.Response.WriteAsync(response)
    }

/// Logout endpoint - signs out user and removes cookie
let logout (ctx: HttpContext) =
    task {
        do! ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)

        // Redirect to home or return URL
        let returnUrl =
            match ctx.Request.Query.TryGetValue("returnUrl") with
            | true, values when values.Count > 0 -> values.[0]
            | _ -> "/"

        ctx.Response.Redirect(returnUrl)
    }

/// Home page handler (use with requiresAuth wrapper)
let home (ctx: HttpContext) =
    task {
        let html = templates.home.homePage ctx |> layout.html ctx |> Render.toString
        ctx.Response.ContentType <- "text/html; charset=utf-8"
        do! ctx.Response.WriteAsync(html)
    }

/// SSE endpoint - sends game state updates to all connected clients
let sse (ctx: HttpContext) =
    task {
        let myChannel = subscribe ()

        try
            // Clear loading state when client connects
            do! Datastar.patchElements """<div id="games-container" class="games-container"></div>""" ctx

            // Keep connection open, forwarding all broadcast events
            while not ctx.RequestAborted.IsCancellationRequested do
                let! event = myChannel.Reader.ReadAsync(ctx.RequestAborted).AsTask()
                do! writeSseEvent ctx event
        with
        | :? OperationCanceledException -> ()
        | :? ChannelClosedException -> ()
        | _ -> ()

        unsubscribe myChannel
    }


// ============================================================================
// REST API Handlers for Multi-Game Support
// ============================================================================

/// POST /games - Create a new game
let createGame (ctx: HttpContext) =
    task {
        let supervisor = ctx.RequestServices.GetRequiredService<GameSupervisor>()
        let (gameId, game) = supervisor.CreateGame()

        // Subscribe to game state changes
        subscribeToGame gameId game

        // Get initial state and broadcast to all clients
        use initialSub =
            game.Subscribe(
                { new IObserver<MoveResult> with
                    member _.OnNext(result) =
                        let html = renderGameBoard gameId result |> Render.toString
                        broadcast (PatchElementsAppend("#games-container", html))

                    member _.OnError(_) = ()
                    member _.OnCompleted() = () }
            )

        // Return 201 Created with Location header
        ctx.Response.StatusCode <- 201
        ctx.Response.Headers.Location <- $"/games/{gameId}"
    }

/// GET /games/{id} - Get a specific game
let getGame (ctx: HttpContext) =
    task {
        let supervisor = ctx.RequestServices.GetRequiredService<GameSupervisor>()
        let gameId = ctx.Request.RouteValues.["id"] |> string

        match supervisor.GetGame(gameId) with
        | Some game ->
            // Subscribe to ensure updates are broadcast
            subscribeToGame gameId game

            // Get current state via a temporary subscription
            let mutable currentResult: MoveResult option = None

            use tempSub =
                game.Subscribe(
                    { new IObserver<MoveResult> with
                        member _.OnNext(result) = currentResult <- Some result
                        member _.OnError(_) = ()
                        member _.OnCompleted() = () }
                )

            match currentResult with
            | Some result ->
                let gameHtml = renderGameBoard gameId result
                let html = gameHtml |> layout.html ctx |> Render.toString
                ctx.Response.ContentType <- "text/html; charset=utf-8"
                do! ctx.Response.WriteAsync(html)
            | None -> ctx.Response.StatusCode <- 500
        | None -> ctx.Response.StatusCode <- 404
    }

/// Helper to determine if it's X's turn based on game state
let private isXTurn (moveResult: MoveResult) =
    match moveResult with
    | MoveResult.XTurn _ -> true
    | _ -> false

/// POST /games/{id} - Make a move in a specific game
let makeMove (ctx: HttpContext) =
    task {
        let supervisor = ctx.RequestServices.GetRequiredService<GameSupervisor>()

        let assignmentManager =
            ctx.RequestServices.GetRequiredService<PlayerAssignmentManager>()

        let gameId = ctx.Request.RouteValues.["id"] |> string

        // Get user ID from authenticated user
        let userId = ctx.User.TryGetUserId()

        match supervisor.GetGame(gameId), userId with
        | Some game, Some uid ->
            let! signals = Datastar.tryReadSignals<MoveSignals> ctx

            match signals with
            | ValueSome s ->
                match Move.TryParse(s.player, s.position) with
                | None -> ctx.Response.StatusCode <- 400
                | Some moveAction ->
                    // Get current game state to determine whose turn it is
                    let currentState = game.GetState()
                    let xTurn = isXTurn currentState

                    // Validate and potentially assign player
                    let! (validationResult, _) =
                        assignmentManager.TryAssignAndValidate(gameId, uid, xTurn) |> Async.StartAsTask

                    match validationResult with
                    | Allowed _ ->
                        // Ensure we're subscribed to broadcast updates
                        subscribeToGame gameId game
                        game.MakeMove(moveAction)
                        ctx.Response.StatusCode <- 202
                    | Rejected reason ->
                        // Move was rejected - broadcast rejection animation
                        let rejectionClass =
                            match reason with
                            | NotYourTurn -> "rejection-not-your-turn"
                            | NotAPlayer -> "rejection-not-a-player"
                            | WrongPlayer -> "rejection-wrong-player"
                            | GameOver -> "rejection-game-over"

                        broadcast (PatchSignals $"""{{ "rejectionAnimation": "{rejectionClass}" }}""")
                        ctx.Response.StatusCode <- 403
            | ValueNone -> ctx.Response.StatusCode <- 400
        | None, _ -> ctx.Response.StatusCode <- 404
        | _, None ->
            // No user ID - cannot make moves without authentication
            ctx.Response.StatusCode <- 401
    }

/// DELETE /games/{id} - Delete a game
let deleteGame (ctx: HttpContext) =
    task {
        let supervisor = ctx.RequestServices.GetRequiredService<GameSupervisor>()
        let gameId = ctx.Request.RouteValues.["id"] |> string

        match supervisor.GetGame(gameId) with
        | Some game ->
            // Dispose the game - this triggers OnCompleted which removes subscription
            game.Dispose()

            // Broadcast removal to all clients
            broadcast (RemoveElement $"#game-{gameId}")

            ctx.Response.StatusCode <- 204
        | None -> ctx.Response.StatusCode <- 404
    }
