# Quickstart: Simplify MailboxProcessor Implementation

## Overview

This guide provides the implementation approach for removing System.Reactive and replacing it with pure MailboxProcessor + callbacks.

## Implementation Order

1. **Engine.fs** - Core changes (highest risk, do first)
2. **TicTacToe.Engine.fsproj** - Remove package reference
3. **Handlers.fs** - Update web handlers
4. **EngineTests.fs** - Update test helpers
5. **SupervisorTests.fs** - Minor updates
6. **Run full test suite** - Verify all tests pass

## Step 1: Update Engine.fs

### 1.1 Remove System.Reactive Import

```fsharp
// REMOVE THIS LINE:
// open System.Reactive.Subjects
```

### 1.2 Add Callback Types

```fsharp
/// Callback type for state change notifications
type StateCallback = MoveResult -> unit

/// Callback type for completion notifications
type CompletionCallback = unit -> unit
```

### 1.3 Update GameMessage Type

```fsharp
type GameMessage =
    | MakeMove of Move
    | Subscribe of StateCallback * CompletionCallback * AsyncReplyChannel<IDisposable>
    | Unsubscribe of int
    | GetState of AsyncReplyChannel<MoveResult>
    | Stop
```

### 1.4 Add Actor State Type

```fsharp
type private GameActorState = {
    GameState: MoveResult
    Subscribers: Map<int, StateCallback * CompletionCallback>
    NextId: int
    Completed: bool
}
```

### 1.5 Update Game Interface

```fsharp
type Game =
    inherit IDisposable
    abstract MakeMove: Move -> unit
    abstract Subscribe: onStateChange: StateCallback * onComplete: CompletionCallback -> IDisposable
    abstract GetState: unit -> MoveResult
```

### 1.6 Implement GameImpl

Key changes:
- Replace `BehaviorSubject` with subscriber map in actor state
- Add broadcast helper function
- Handle Subscribe/Unsubscribe messages
- Call completion callbacks when game ends

```fsharp
type GameImpl() =
    let initialState = startGame ()
    let mutable disposed = false

    let agent =
        MailboxProcessor<GameMessage>.Start(fun inbox ->
            let notifyAll (subs: Map<int, _>) result =
                subs |> Map.iter (fun _ (onNext, _) ->
                    try onNext result with _ -> ())

            let notifyComplete (subs: Map<int, _>) =
                subs |> Map.iter (fun _ (_, onComplete) ->
                    try onComplete () with _ -> ())

            let rec messageLoop (state: GameActorState) =
                async {
                    let! message = inbox.Receive()

                    match message with
                    | Stop ->
                        if not state.Completed then
                            notifyComplete state.Subscribers

                    | GetState reply ->
                        reply.Reply(state.GameState)
                        return! messageLoop state

                    | Subscribe(onNext, onComplete, reply) ->
                        let id = state.NextId
                        let handle =
                            { new IDisposable with
                                member _.Dispose() = inbox.Post(Unsubscribe id) }
                        let newSubs = state.Subscribers |> Map.add id (onNext, onComplete)
                        reply.Reply(handle)
                        return! messageLoop { state with Subscribers = newSubs; NextId = id + 1 }

                    | Unsubscribe id ->
                        let newSubs = state.Subscribers |> Map.remove id
                        return! messageLoop { state with Subscribers = newSubs }

                    | MakeMove move ->
                        let nextResult = makeMove (state.GameState, move)
                        notifyAll state.Subscribers nextResult

                        match nextResult with
                        | Won _ | Draw _ ->
                            notifyComplete state.Subscribers
                            return! messageLoop { state with GameState = nextResult; Completed = true }
                        | Error _ ->
                            notifyAll state.Subscribers state.GameState
                            return! messageLoop state
                        | XTurn _ | OTurn _ ->
                            return! messageLoop { state with GameState = nextResult }
                }

            messageLoop {
                GameState = initialState
                Subscribers = Map.empty
                NextId = 0
                Completed = false
            })

    interface Game with
        member _.MakeMove(move) =
            if disposed then raise (ObjectDisposedException("Game"))
            agent.Post(MakeMove move)

        member _.Subscribe(onStateChange, onComplete) =
            agent.PostAndReply(fun reply -> Subscribe(onStateChange, onComplete, reply))

        member _.GetState() =
            agent.PostAndReply(GetState)

        member _.Dispose() =
            if not disposed then
                disposed <- true
                agent.Post(Stop)
```

### 1.7 Update GameSupervisor

Replace IObserver usage with callback subscription:

```fsharp
| CreateGame reply ->
    let gameId = Guid.NewGuid().ToString()
    let game = createGame ()
    let timestamp = DateTimeOffset.UtcNow

    let subscription =
        game.Subscribe(
            (fun _ -> ()),  // onStateChange: no-op (supervisor doesn't need state)
            (fun () -> this.RemoveGame(gameId))  // onComplete: remove game
        )

    // ... rest unchanged
```

## Step 2: Remove Package Reference

In `TicTacToe.Engine.fsproj`, remove:

```xml
<PackageReference Include="System.Reactive" Version="6.0.2" />
```

## Step 3: Update Handlers.fs

Replace `IObserver<MoveResult>` with callback functions:

### subscribeToGame

```fsharp
let private subscribeToGame (gameId: string) (game: Game) =
    if not (gameSubscriptions.ContainsKey(gameId)) then
        let subscription =
            game.Subscribe(
                (fun result ->
                    let html = renderGameBoard gameId result |> Render.toString
                    broadcast (PatchElements html)),
                (fun () ->
                    match gameSubscriptions.TryRemove(gameId) with
                    | true, sub -> sub.Dispose()
                    | _ -> ()))
        gameSubscriptions.TryAdd(gameId, subscription) |> ignore
```

### createGame handler

```fsharp
// Get initial state and broadcast
let initialState = game.GetState()
let html = renderGameBoard gameId initialState |> Render.toString
broadcast (PatchElementsAppend("#games-container", html))
```

### getGame handler

```fsharp
let currentResult = game.GetState()
let gameHtml = renderGameBoard gameId currentResult
// ... rest unchanged
```

## Step 4: Update Test Helpers

### EngineTests.fs - collectResults

```fsharp
let collectResults (game: Game) =
    let results = ResizeArray<MoveResult>()
    let completed = TaskCompletionSource<unit>()

    let subscription = game.Subscribe(
        (fun result -> results.Add(result)),
        (fun () -> completed.SetResult(())))

    (results, completed.Task, subscription)
```

### collectResultsAndErrors

```fsharp
let collectResultsAndErrors (game: Game) =
    let results = ResizeArray<MoveResult>()
    let errors = ResizeArray<MoveResult>()
    let completed = TaskCompletionSource<unit>()

    let subscription = game.Subscribe(
        (fun result ->
            match result with
            | Error _ -> errors.Add(result)
            | _ -> results.Add(result)),
        (fun () -> completed.SetResult(())))

    (results, errors, completed.Task, subscription)
```

## Step 5: Verify

Run the full test suite:

```bash
dotnet test
```

All 602+ tests should pass without modification to test assertions.

## Rollback Plan

If issues arise:
1. Revert Engine.fs changes
2. Restore System.Reactive package reference
3. Revert Handlers.fs changes
4. Revert test helper changes

Git makes this straightforward: `git checkout -- src/ test/`
