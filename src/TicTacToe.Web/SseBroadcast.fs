module TicTacToe.Web.SseBroadcast

open System.Threading.Channels
open Microsoft.AspNetCore.Http
open Frank.Datastar
open StarFederation.Datastar.FSharp

/// SSE event types for broadcasting to connected clients
type SseEvent =
    | PatchElements of html: string
    | PatchElementsAppend of selector: string * html: string
    | RemoveElement of selector: string
    | PatchSignals of json: string

/// Thread-safe collection of subscriber channels with user identity
let private subscribersLock = obj ()
let private subscribers = ResizeArray<string * Channel<SseEvent>>()

/// Create a new subscriber channel for an SSE connection with user identity
let subscribe (userId: string) : Channel<SseEvent> =
    let channel = Channel.CreateUnbounded<SseEvent>()
    lock subscribersLock (fun () -> subscribers.Add((userId, channel)))
    channel

/// Remove a subscriber channel when SSE connection closes
let unsubscribe (channel: Channel<SseEvent>) =
    lock subscribersLock (fun () ->
        subscribers.RemoveAll(fun (_, ch) -> ch = channel) |> ignore)
    channel.Writer.Complete()

/// Broadcast an event to ALL active SSE connections
let broadcast (event: SseEvent) =
    lock subscribersLock (fun () ->
        for (_, ch) in subscribers do
            ch.Writer.TryWrite(event) |> ignore)

/// Send an event to a specific user's SSE connections
let sendToUser (userId: string) (event: SseEvent) =
    lock subscribersLock (fun () ->
        for (uid, ch) in subscribers do
            if uid = userId then
                ch.Writer.TryWrite(event) |> ignore)

/// Broadcast an event per role: maps each subscriber's userId to the appropriate event
let broadcastPerRole (renderForRole: string -> SseEvent) =
    lock subscribersLock (fun () ->
        for (userId, ch) in subscribers do
            let event = renderForRole userId
            ch.Writer.TryWrite(event) |> ignore)

/// Helper to write SSE events to response
let writeSseEvent (ctx: HttpContext) (event: SseEvent) =
    task {
        match event with
        | PatchElements html -> do! Datastar.patchElements html ctx
        | PatchElementsAppend(selector, html) ->
            let opts = { PatchElementsOptions.Defaults with Selector = ValueSome (Selector selector); PatchMode = ElementPatchMode.Append }
            do! Datastar.patchElementsWithOptions opts html ctx
        | RemoveElement selector -> do! Datastar.removeElement selector ctx
        | PatchSignals json -> do! Datastar.patchSignals json ctx
    }
