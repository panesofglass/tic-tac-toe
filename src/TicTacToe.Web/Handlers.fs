module TicTacToe.Web.Handlers

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing
open Microsoft.AspNetCore.Routing.Internal
open Oxpecker
open StarFederation.Datastar.FSharp
open TicTacToe.Web.DatastarExtensions
open TicTacToe.Web.templates
open TicTacToe.Web.templates.shared
open TicTacToe.Web.templates.game
open TicTacToe.Engine
open TicTacToe.Model

let writeHtmlView f (ctx: HttpContext) =
    f ctx |> layout.html ctx |> ctx.WriteHtmlView

let home: EndpointHandler = writeHtmlView home.homePage

let graph: EndpointHandler =
    fun ctx ->
        let graphWriter = ctx.GetService<DfaGraphWriter>()
        let endpointDataSource = ctx.GetService<EndpointDataSource>()
        use sw = new IO.StringWriter()
        graphWriter.Write(endpointDataSource, sw)
        ctx.Response.WriteAsync(sw.ToString())

let games: EndpointHandler = writeHtmlView games.gamesPage

let createGame: EndpointHandler =
    fun ctx ->
        let supervisor = ctx.GetService<GameSupervisor>()
        let (gameId, _) = supervisor.CreateGame()
        ctx.Response.StatusCode <- 302
        ctx.Response.Headers.Location <- $"/games/{gameId}"
        Task.CompletedTask

let makeMove gameId : EndpointHandler =
    fun ctx ->
        task {
            let supervisor = ctx.GetService<GameSupervisor>()

            match supervisor.GetGame gameId with
            | None -> ctx.Response.StatusCode <- 404
            | Some game ->
                let! form = ctx.Request.ReadFormAsync()
                let player = form["player"].ToString()
                let position = form["position"].ToString()

                match Move.TryParse(player, position) with
                | None -> ctx.Response.StatusCode <- 400
                | Some move ->
                    try
                        game.MakeMove move
                        ctx.Response.StatusCode <- 202
                    with ex ->
                        ctx.Response.StatusCode <- 409
                        do! ctx.Response.WriteAsync ex.Message
        }

let gamePage gameId : EndpointHandler =
    fun ctx ->
        let supervisor = ctx.GetService<GameSupervisor>()

        match supervisor.GetGame gameId with
        | None ->
            ctx.Response.StatusCode <- 404
            Task.CompletedTask
        | Some _ -> ctx |> writeHtmlView (gamePage gameId)

let gameEvents gameId : EndpointHandler =
    fun ctx ->
        task {
            let supervisor = ctx.GetService<GameSupervisor>()

            match supervisor.GetGame gameId with
            | None -> ctx.Response.StatusCode <- 404
            | Some game ->
                let datastar = ctx.GetService<ServerSentEventGenerator>()
                do datastar.StartServerEventStreamAsync() |> ignore

                let htmlopts =
                    { PatchElementsOptions.Defaults with
                        Selector = ValueSome "#game-board" }

                // Keep connection alive until client disconnects
                let tcs = TaskCompletionSource()

                use subscription =
                    game.Subscribe(
                        { new IObserver<MoveResult> with
                            member _.OnNext(result) =
                                let html = renderGameBoard gameId result
                                datastar.PatchHtmlViewAsync(html, htmlopts) |> ignore

                            member _.OnError(ex) =
                                // TODO: render error notification
                                tcs.TrySetException(ex) |> ignore

                            member _.OnCompleted() =
                                // TODO: render winner notification
                                tcs.TrySetResult() |> ignore }
                    )

                use _reg =
                    ctx.RequestAborted.Register(fun () ->
                        try
                            subscription.Dispose()
                        with _ ->
                            ()

                        tcs.TrySetResult() |> ignore)

                do! tcs.Task
        }
