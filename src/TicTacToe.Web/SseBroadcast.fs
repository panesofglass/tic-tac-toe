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

/// Thread-safe collection of subscriber channels
let private subscribersLock = obj ()
let private subscribers = ResizeArray<Channel<SseEvent>>()

/// Create a new subscriber channel for an SSE connection
let subscribe () : Channel<SseEvent> =
    let channel = Channel.CreateUnbounded<SseEvent>()
    lock subscribersLock (fun () -> subscribers.Add(channel))
    channel

/// Remove a subscriber channel when SSE connection closes
let unsubscribe (channel: Channel<SseEvent>) =
    lock subscribersLock (fun () -> subscribers.Remove(channel) |> ignore)
    channel.Writer.Complete()

/// Broadcast an event to ALL active SSE connections
let broadcast (event: SseEvent) =
    lock subscribersLock (fun () ->
        for ch in subscribers do
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
