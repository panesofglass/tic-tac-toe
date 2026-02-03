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
    { player: string
      position: string }

// Module-level state for the home page game ID
let mutable private homeGameId: string option = None

/// Get or create the home page game
let private getOrCreateHomeGame (supervisor: GameSupervisor) =
    match homeGameId with
    | Some id ->
        match supervisor.GetGame(id) with
        | Some game -> (id, game)
        | None ->
            // Game was cleaned up, create new one
            let (newId, game) = supervisor.CreateGame()
            homeGameId <- Some newId
            (newId, game)
    | None ->
        let (newId, game) = supervisor.CreateGame()
        homeGameId <- Some newId
        (newId, game)

/// Reset the home page game
let private resetHomeGame (supervisor: GameSupervisor) =
    let (newId, game) = supervisor.CreateGame()
    homeGameId <- Some newId
    (newId, game)

/// Home page handler
let home (ctx: HttpContext) =
    task {
        let html = templates.home.homePage ctx |> layout.html ctx |> Render.toString
        ctx.Response.ContentType <- "text/html; charset=utf-8"
        do! ctx.Response.WriteAsync(html)
    }

/// SSE endpoint - sends game state updates
let sse (ctx: HttpContext) =
    task {
        let supervisor = ctx.RequestServices.GetRequiredService<GameSupervisor>()
        let (_, game) = getOrCreateHomeGame supervisor

        let myChannel = subscribe ()

        try
            // Subscribe to game state changes and forward to SSE channel
            use _ =
                game.Subscribe(
                    { new IObserver<MoveResult> with
                        member _.OnNext(result) =
                            let html = renderGameBoard result |> Render.toString
                            broadcast (PatchElements html)
                        member _.OnError(_) = ()
                        member _.OnCompleted() = () })

            // Keep connection open, forwarding events from our channel
            while not ctx.RequestAborted.IsCancellationRequested do
                let! event = myChannel.Reader.ReadAsync(ctx.RequestAborted).AsTask()
                do! writeSseEvent ctx event
        with
        | :? OperationCanceledException -> ()
        | :? ChannelClosedException -> ()
        | _ -> ()

        unsubscribe myChannel
    }

/// POST handler - make a move
let move (ctx: HttpContext) =
    task {
        let supervisor = ctx.RequestServices.GetRequiredService<GameSupervisor>()

        let! signals = Datastar.tryReadSignals<MoveSignals> ctx

        match signals with
        | ValueSome s ->
            match Move.TryParse(s.player, s.position) with
            | None ->
                ctx.Response.StatusCode <- 400
            | Some moveAction ->
                let (_, game) = getOrCreateHomeGame supervisor
                game.MakeMove(moveAction)
                ctx.Response.StatusCode <- 202
        | ValueNone ->
            ctx.Response.StatusCode <- 400
    }

/// DELETE handler - reset/clear the game
let reset (ctx: HttpContext) =
    task {
        let supervisor = ctx.RequestServices.GetRequiredService<GameSupervisor>()
        let (_, game) = resetHomeGame supervisor

        // Subscribe briefly to trigger initial state broadcast
        use _ = game.Subscribe(
            { new IObserver<MoveResult> with
                member _.OnNext(result) =
                    let html = renderGameBoard result |> Render.toString
                    broadcast (PatchElements html)
                member _.OnError(_) = ()
                member _.OnCompleted() = () })

        ctx.Response.StatusCode <- 202
    }
