module TicTacToe.Engine

open System
open System.Reactive.Subjects
open System.Threading.Channels
open System.Threading.Tasks
open System.Threading
open TicTacToe.Model

type Game =
    inherit IDisposable
    inherit IObservable<MoveResult>
    abstract MakeMove: Move -> unit

/// Internal message type for the game actor
type GameMessage = MakeMove of Move

[<Literal>]
let MaxMoves = 9

/// Game actor implementation using BehaviorSubject for broadcast semantics
type GameImpl() =
    let inbox =
        Channel.CreateBounded<GameMessage>(BoundedChannelOptions(MaxMoves, SingleWriter = false, SingleReader = true))

    let initialState = startGame ()
    let outbox = new BehaviorSubject<MoveResult>(initialState)

    let mutable disposed = false

    let rec messageLoop state : Task =
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

    do
        // Start message processing loop
        messageLoop (initialState) |> ignore

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
                disposed <- true
                inbox.Writer.Complete()

                if not outbox.IsDisposed then
                    outbox.OnCompleted()
                    outbox.Dispose()

let createGame () : Game = new GameImpl() :> Game

type GameSupervisor =
    inherit IDisposable
    abstract CreateGame: unit -> string * Game
    abstract GetGame: gameId: string -> Game option
    abstract GetActiveGameCount: unit -> int

type GameSupervisorImpl() as this =
    let activeGames = Collections.Concurrent.ConcurrentDictionary<string, Game>()

    let gameCreationTimes =
        Collections.Concurrent.ConcurrentDictionary<string, DateTime>()

    // Track game subscriptions to prevent leaks
    let gameSubscriptions =
        Collections.Concurrent.ConcurrentDictionary<string, IDisposable>()

    let cleanupTimer =
        new Timer((fun _ -> this.CleanupExpiredGames()), null, TimeSpan.FromMinutes(5.0), TimeSpan.FromMinutes(5.0))

    member private this.MonitorGameCompletion(gameId: string, game: Game) =
        let subscription =
            game.Subscribe(
                { new IObserver<MoveResult> with
                    member _.OnNext(_) = () // Don't care about intermediate states
                    member _.OnCompleted() = this.RemoveGame(gameId) // Game completed - clean up
                    member _.OnError(_) = this.RemoveGame(gameId) } // Game errored - clean up
            )

        gameSubscriptions.TryAdd(gameId, subscription) |> ignore

    member private this.RemoveGame(gameId: string) =
        match activeGames.TryRemove(gameId) with
        | true, game ->
            gameCreationTimes.TryRemove(gameId) |> ignore

            // Dispose subscription first to stop callbacks
            match gameSubscriptions.TryRemove(gameId) with
            | true, subscription -> subscription.Dispose()
            | false, _ -> ()

            try
                game.Dispose()
            with _ ->
                ()
        | false, _ -> ()

    member private this.CleanupExpiredGames() =
        let cutoff = DateTime.UtcNow.AddHours(-1.0)

        let expiredGames =
            gameCreationTimes.ToArray()
            |> Array.filter (fun kvp -> kvp.Value < cutoff)
            |> Array.map (_.Key)

        for gameId in expiredGames do
            this.RemoveGame(gameId)

    interface GameSupervisor with
        member this.CreateGame() =
            let gameId = Guid.NewGuid().ToString()
            let game = createGame ()

            activeGames.TryAdd(gameId, game) |> ignore
            gameCreationTimes.TryAdd(gameId, DateTime.UtcNow) |> ignore

            this.MonitorGameCompletion(gameId, game)

            (gameId, game)

        member _.GetGame(gameId: string) =
            activeGames.TryGetValue(gameId)
            |> function
                | true, game -> Some game
                | false, _ -> None

        member _.GetActiveGameCount() = activeGames.Count

        member _.Dispose() =
            cleanupTimer.Dispose()

            // Dispose subscriptions first to stop callbacks
            for kvp in gameSubscriptions.ToArray() do
                try
                    kvp.Value.Dispose()
                with _ ->
                    ()

            gameSubscriptions.Clear()

            // Clear the dictionaries first to prevent completion handlers from interfering
            let gamesToDispose = activeGames.ToArray()
            activeGames.Clear()
            gameCreationTimes.Clear()

            // Now dispose the games
            for kvp in gamesToDispose do
                try
                    kvp.Value.Dispose()
                with _ ->
                    ()

let createGameSupervisor () : GameSupervisor =
    new GameSupervisorImpl() :> GameSupervisor
