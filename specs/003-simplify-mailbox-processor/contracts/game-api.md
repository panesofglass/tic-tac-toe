# Game API Contract: Callback-Based Interface

This document defines the API contract for the refactored Game type using callbacks instead of IObservable.

## Game Interface

```fsharp
/// Game interface - pure MailboxProcessor with callback subscriptions
type Game =
    inherit IDisposable

    /// Make a move in the game
    /// Raises ObjectDisposedException if game is disposed
    abstract MakeMove: Move -> unit

    /// Subscribe to game state changes
    /// Returns IDisposable to unsubscribe
    abstract Subscribe:
        onStateChange: (MoveResult -> unit) *
        onComplete: (unit -> unit)
        -> IDisposable

    /// Get current game state synchronously
    abstract GetState: unit -> MoveResult
```

## Callback Semantics

### onStateChange: MoveResult -> unit

Called when:
- A valid move is made (XTurn, OTurn, Won, Draw states)
- An invalid move is attempted (Error state, then previous state)

Not called:
- On subscription (no replay-on-subscribe)
- After game completion

### onComplete: unit -> unit

Called once when:
- Game reaches Won state
- Game reaches Draw state
- Game is disposed

Guarantee:
- Called exactly once per subscription (unless subscription disposed first)
- Called after final onStateChange for Won/Draw

## GameSupervisor Interface

```fsharp
/// GameSupervisor interface - unchanged from current
type GameSupervisor =
    inherit IDisposable

    /// Create a new game, returns (gameId, game)
    abstract CreateGame: unit -> string * Game

    /// Get a game by ID, None if not found or disposed
    abstract GetGame: gameId: string -> Game option

    /// Get count of active games
    abstract GetActiveGameCount: unit -> int
```

## Usage Examples

### Basic Subscription

```fsharp
let game = createGame()

let subscription = game.Subscribe(
    (fun result -> printfn "State: %A" result),
    (fun () -> printfn "Game complete"))

// Make moves...
game.MakeMove(X, TopLeft)

// Unsubscribe when done
subscription.Dispose()
```

### Web Handler Pattern

```fsharp
let subscription = game.Subscribe(
    (fun result ->
        let html = renderGameBoard gameId result |> Render.toString
        broadcast (PatchElements html)),
    (fun () ->
        // Clean up subscription tracking
        subscriptions.TryRemove(gameId) |> ignore))
```

### Test Collection Pattern

```fsharp
let results = ResizeArray<MoveResult>()
let completed = TaskCompletionSource<unit>()

let sub = game.Subscribe(
    (fun r -> results.Add(r)),
    (fun () -> completed.SetResult(())))

// Play game...
do! completed.Task
// Assert on results
```

## Error Handling

- Callbacks are wrapped in try/catch internally
- A failing callback does not affect other subscribers
- A failing callback does not stop game processing
- Exceptions in callbacks are silently swallowed (matches current Rx behavior)

## Thread Safety

- All operations are thread-safe
- Callbacks may be invoked from any thread
- Callbacks are invoked sequentially (not concurrently)
- Subscribe/Unsubscribe can be called from any thread

## Disposal Semantics

- Disposing a Game calls onComplete for all active subscriptions
- Disposing a subscription removes it from the subscriber list
- Disposing an already-disposed subscription is a no-op
- MakeMove after disposal raises ObjectDisposedException
- GetState after disposal returns last known state
