module TicTacToe.Engine

open System
open System.Reactive.Subjects
open System.Threading.Channels
open System.Threading.Tasks
open TicTacToe.Model

type Game =
    inherit IDisposable
    inherit IObservable<MoveResult>
    abstract MakeMove: Move -> unit

/// Internal message type for the game actor
type GameMessage = MakeMove of Move

[<Literal>]
let MaxMoves = 9

/// Game actor implementation using bounded channels
type GameActor() =
    let inbox =
        Channel.CreateBounded<GameMessage>(BoundedChannelOptions(MaxMoves, SingleWriter = false, SingleReader = true))

    let outbox = new ReplaySubject<MoveResult>(1)

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
                        outbox.OnNext(nextState)

                        match nextState with
                        | Won _
                        | Draw _ -> outbox.OnCompleted()
                        | _ -> return! messageLoop nextState ()
            with ex ->
                outbox.OnError(ex)
                disposed <- true
        }

    do
        let initialState = startGame ()
        // Send initial state to channel
        outbox.OnNext(initialState)
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

        member _.Subscribe(observer) = outbox.Subscribe(observer)

        member _.Dispose() =
            if not disposed then
                inbox.Writer.Complete()

                if not outbox.IsDisposed then
                    outbox.OnCompleted()
                    outbox.Dispose()

                disposed <- true

let createGame () : Game = new GameActor() :> Game
