module TicTacToe.Web.Handlers

open System
open System.Threading.Channels
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Oxpecker.ViewEngine
open Frank.Datastar
open TicTacToe.Web.SseBroadcast
open TicTacToe.Web.templates.shared
open TicTacToe.Web.templates.game
open TicTacToe.Engine
open TicTacToe.Model

/// Signals type for move data from Datastar
[<CLIMutable>]
type MoveSignals =
    { gameId: string
      player: string
      position: string }

// Active game subscriptions - maps gameId to subscription disposable
let private gameSubscriptions = System.Collections.Concurrent.ConcurrentDictionary<string, IDisposable>()

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
                        | _ -> () })
        gameSubscriptions.TryAdd(gameId, subscription) |> ignore

/// Home page handler
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
        use initialSub = game.Subscribe(
            { new IObserver<MoveResult> with
                member _.OnNext(result) =
                    let html = renderGameBoard gameId result |> Render.toString
                    broadcast (PatchElementsAppend("#games-container", html))
                member _.OnError(_) = ()
                member _.OnCompleted() = () })

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
            use tempSub = game.Subscribe(
                { new IObserver<MoveResult> with
                    member _.OnNext(result) = currentResult <- Some result
                    member _.OnError(_) = ()
                    member _.OnCompleted() = () })

            match currentResult with
            | Some result ->
                let gameHtml = renderGameBoard gameId result
                let html = gameHtml |> layout.html ctx |> Render.toString
                ctx.Response.ContentType <- "text/html; charset=utf-8"
                do! ctx.Response.WriteAsync(html)
            | None ->
                ctx.Response.StatusCode <- 500
        | None ->
            ctx.Response.StatusCode <- 404
    }

/// POST /games/{id} - Make a move in a specific game
let makeMove (ctx: HttpContext) =
    task {
        let supervisor = ctx.RequestServices.GetRequiredService<GameSupervisor>()
        let gameId = ctx.Request.RouteValues.["id"] |> string

        match supervisor.GetGame(gameId) with
        | Some game ->
            let! signals = Datastar.tryReadSignals<MoveSignals> ctx

            match signals with
            | ValueSome s ->
                match Move.TryParse(s.player, s.position) with
                | None ->
                    ctx.Response.StatusCode <- 400
                | Some moveAction ->
                    // Ensure we're subscribed to broadcast updates
                    subscribeToGame gameId game
                    game.MakeMove(moveAction)
                    ctx.Response.StatusCode <- 202
            | ValueNone ->
                ctx.Response.StatusCode <- 400
        | None ->
            ctx.Response.StatusCode <- 404
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
        | None ->
            ctx.Response.StatusCode <- 404
    }
