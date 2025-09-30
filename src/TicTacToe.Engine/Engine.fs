module TicTacToe.Engine

open System
open System.Threading.Channels
open System.Threading.Tasks
open System.Threading
open TicTacToe.Model

type Game =
    inherit IDisposable
    abstract MakeMove: Move -> unit
    abstract GetResultsAsync: CancellationToken -> Collections.Generic.IAsyncEnumerable<MoveResult>

/// Internal message type for the game actor
type GameMessage = MakeMove of Move

[<Literal>]
let MaxMoves = 9

/// Game actor implementation using bounded channels
type GameActor() =
    let inbox =
        Channel.CreateBounded<GameMessage>(BoundedChannelOptions(MaxMoves, SingleWriter = false, SingleReader = true))

    let outbox = Channel.CreateBounded<MoveResult>(MaxMoves + 1) // +1 for initial state

    let mutable disposed = false

    let rec messageLoop state () : Task =
        task {
            try
                let! hasMessage = inbox.Reader.WaitToReadAsync()

                if hasMessage && not disposed then
                    let! message = inbox.Reader.ReadAsync()

                    match message with
                    | MakeMove(move) ->
                        let nextState = makeMove (state, move)
                        do! outbox.Writer.WriteAsync(nextState)

                        match nextState with
                        | Won _
                        | Draw _ ->
                            outbox.Writer.Complete()
                            disposed <- true
                        | _ -> return! messageLoop nextState ()
            with ex ->
                outbox.Writer.Complete(ex)
                disposed <- true
        }

    do
        let initialState = startGame ()
        // Send initial state to channel
        outbox.Writer.TryWrite(initialState) |> ignore
        // Start message processing loop
        Task.Run(messageLoop initialState) |> ignore

    interface Game with
        member _.MakeMove(move: Move) =
            if disposed then
                raise (ObjectDisposedException("Game"))
            else
                let message = MakeMove(move)

                if inbox.Writer.TryWrite(message) then
                    ()
                else
                    raise (
                        InvalidOperationException(
                            "Game message queue is full. Please wait and try again in a few moments."
                        )
                    )

        member _.GetResultsAsync(cancellationToken: CancellationToken) =
            outbox.Reader.ReadAllAsync(cancellationToken)

        member _.Dispose() =
            if not disposed then
                inbox.Writer.Complete()
                outbox.Writer.Complete()
                disposed <- true

let createGame () : Game = new GameActor() :> Game
