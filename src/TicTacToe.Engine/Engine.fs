module TicTacToe.Engine

open System
open System.Reactive.Subjects
open System.Threading
open TicTacToe.Model

type Game =
    inherit IDisposable
    inherit IObservable<MoveResult>
    abstract MakeMove: Move -> unit

/// Internal message type for the game actor
type GameMessage =
    | MakeMove of Move
    | Stop

/// Game actor implementation using MailboxProcessor and BehaviorSubject for broadcast semantics
type GameImpl() =
    let initialState = startGame ()
    let outbox = new BehaviorSubject<MoveResult>(initialState)

    let mutable disposed = false

    let agent =
        MailboxProcessor<GameMessage>.Start(fun inbox ->
            let rec messageLoop state =
                async {
                    try
                        let! message = inbox.Receive()

                        match message with
                        | Stop ->
                            // Stop the message loop - don't process any more messages
                            ()
                        | MakeMove(move) ->
                            let nextState = makeMove (state, move)
                            outbox.OnNext(nextState)

                            match nextState with
                            | Won _
                            | Draw _ ->
                                // Game completed - signal completion
                                outbox.OnCompleted()
                            | Error _ ->
                                // Game had error - signal completion
                                outbox.OnCompleted()
                            | _ -> return! messageLoop nextState
                    with ex ->
                        // Game errored - signal error
                        outbox.OnError(ex)
                }

            messageLoop initialState)

    interface Game with
        member _.MakeMove(move: Move) =
            if disposed then
                raise (ObjectDisposedException("Game"))
            else
                let message = MakeMove(move)
                agent.Post(message)

        member _.Subscribe(observer) = outbox.Subscribe(observer)

        member _.Dispose() =
            if not disposed then
                disposed <- true

                // Send stop message to gracefully stop the message loop
                agent.Post(Stop)

                if not outbox.IsDisposed then
                    outbox.OnCompleted()
                    outbox.Dispose()

let createGame () : Game = new GameImpl() :> Game

type GameSupervisor =
    inherit IDisposable
    abstract CreateGame: unit -> string * Game
    abstract GetGame: gameId: string -> Game option
    abstract GetActiveGameCount: unit -> int

type GameRef =
    { Game: Game
      Subscription: IDisposable
      Timestamp: DateTimeOffset }

type GameSupervisorMessage =
    | CountActive of AsyncReplyChannel<int>
    | CreateGame of AsyncReplyChannel<string * Game>
    | GetGame of string * AsyncReplyChannel<Game option>
    | RemoveGame of string
    | Timeout
    | Dispose

type GameSupervisorImpl() as this =
    let mutable disposed = false

    let cleanupTimer =
        new Timer((fun _ -> this.CleanupExpiredGames()), null, TimeSpan.FromMinutes(5.0), TimeSpan.FromMinutes(5.0))

    let agent =
        MailboxProcessor<GameSupervisorMessage>.Start(fun inbox ->
            let rec messageLoop state =
                async {
                    let! message = inbox.Receive()

                    match message with
                    | CountActive reply ->
                        reply.Reply(Map.count state)
                        return! messageLoop state

                    | CreateGame reply ->
                        let gameId = Guid.NewGuid().ToString()
                        let game = createGame ()
                        let timestamp = DateTimeOffset.UtcNow

                        let subscription =
                            game.Subscribe(
                                { new IObserver<MoveResult> with
                                    member _.OnNext(_) = ()
                                    member _.OnCompleted() = this.RemoveGame(gameId)
                                    member _.OnError(_) = this.RemoveGame(gameId) }
                            )

                        let gameRef =
                            { Game = game
                              Timestamp = timestamp
                              Subscription = subscription }

                        let nextState = state |> Map.add gameId gameRef

                        reply.Reply((gameId, game))

                        return! messageLoop nextState

                    | GetGame(gameId, reply) ->
                        match state |> Map.tryFind gameId with
                        | Some { Game = game } -> reply.Reply(Some game)
                        | None -> reply.Reply(None)

                        return! messageLoop state

                    | RemoveGame gameId ->
                        match Map.tryFind gameId state with
                        | Some gameRef ->
                            let nextState = state |> Map.remove gameId

                            try
                                gameRef.Subscription.Dispose()
                                gameRef.Game.Dispose()
                            with _ ->
                                ()

                            return! messageLoop nextState
                        | None -> return! messageLoop state

                    | Timeout ->
                        let cutoff = DateTimeOffset.UtcNow.AddHours(-1.0)

                        let removeGames, nextState =
                            state |> Map.partition (fun _ gameRef -> gameRef.Timestamp < cutoff)

                        for KeyValue(gameId, gameRef) in removeGames do
                            try
                                gameRef.Subscription.Dispose()
                                gameRef.Game.Dispose()
                            with _ ->
                                ()

                        return! messageLoop nextState

                    | Dispose ->
                        for KeyValue(_, gameRef) in state do
                            try
                                gameRef.Subscription.Dispose()
                                gameRef.Game.Dispose()
                            with _ ->
                                ()
                }

            messageLoop (Map<string, GameRef> Seq.empty))

    member private _.RemoveGame(gameId: string) = agent.Post(RemoveGame(gameId))

    member private _.CleanupExpiredGames() = agent.Post(Timeout)

    interface GameSupervisor with
        member _.CreateGame() = agent.PostAndReply(CreateGame)

        member _.GetGame(gameId: string) =
            agent.PostAndReply(fun reply -> GetGame(gameId, reply))

        member _.GetActiveGameCount() = agent.PostAndReply(CountActive)

        member _.Dispose() =
            if not disposed then
                cleanupTimer.Dispose()
                agent.Post(Dispose)

let createGameSupervisor () : GameSupervisor =
    new GameSupervisorImpl() :> GameSupervisor
