# Data Model: Simplify MailboxProcessor - Remove System.Reactive

## Type Changes

### Current Types (to be removed/modified)

```fsharp
// Engine.fs - CURRENT (using System.Reactive)
open System.Reactive.Subjects

type Game =
    inherit IDisposable
    inherit IObservable<MoveResult>  // ← REMOVE THIS
    abstract MakeMove: Move -> unit

type GameImpl() =
    let outbox = new BehaviorSubject<MoveResult>(initialState)  // ← REMOVE THIS
```

### New Types (pure MailboxProcessor)

```fsharp
// Engine.fs - NEW (no System.Reactive)

/// Callback type for state change notifications
type StateCallback = MoveResult -> unit

/// Callback type for completion notifications
type CompletionCallback = unit -> unit

/// Subscription handle returned from Subscribe
type SubscriptionHandle(unsubscribe: unit -> unit) =
    interface IDisposable with
        member _.Dispose() = unsubscribe()

/// Game interface - no longer inherits IObservable
type Game =
    inherit IDisposable
    abstract MakeMove: Move -> unit
    abstract Subscribe: onStateChange: StateCallback * onComplete: CompletionCallback -> IDisposable
    abstract GetState: unit -> MoveResult

/// Internal message type for the game actor
type GameMessage =
    | MakeMove of Move
    | Subscribe of StateCallback * CompletionCallback * AsyncReplyChannel<IDisposable>
    | Unsubscribe of int
    | GetState of AsyncReplyChannel<MoveResult>
    | Stop

/// Internal actor state
type GameActorState = {
    GameState: MoveResult
    Subscribers: Map<int, StateCallback * CompletionCallback>
    NextSubscriberId: int
}
```

## Entity Relationships

```
┌─────────────────────────────────────────────────────────────┐
│                      GameSupervisor                          │
│  MailboxProcessor<GameSupervisorMessage>                    │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ State: Map<string, GameRef>                          │   │
│  │   gameId → { Game, CompletionCallback, Timestamp }   │   │
│  └─────────────────────────────────────────────────────┘   │
└───────────────────────┬─────────────────────────────────────┘
                        │ manages
                        ▼
┌─────────────────────────────────────────────────────────────┐
│                         Game                                 │
│  MailboxProcessor<GameMessage>                              │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ State: GameActorState                                │   │
│  │   - GameState: MoveResult                            │   │
│  │   - Subscribers: Map<int, Callbacks>                 │   │
│  │   - NextSubscriberId: int                            │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                        │ notifies
                        ▼
┌─────────────────────────────────────────────────────────────┐
│                    Subscribers                               │
│  - Handlers.fs: Web subscription for SSE broadcast          │
│  - GameSupervisor: Completion tracking for cleanup          │
│  - Tests: Result collection for assertions                  │
└─────────────────────────────────────────────────────────────┘
```

## State Transitions

### Game Actor State Machine

```
                    ┌─────────────┐
                    │   Initial   │
                    │  (XTurn)    │
                    └──────┬──────┘
                           │ MakeMove
                           ▼
              ┌────────────────────────┐
              │   Processing Move      │
              │   - Validate           │
              │   - Update state       │
              │   - Notify subscribers │
              └────────────┬───────────┘
                           │
          ┌────────────────┼────────────────┐
          ▼                ▼                ▼
    ┌──────────┐    ┌──────────┐    ┌──────────┐
    │ Continue │    │   Won    │    │   Draw   │
    │ (X/OTurn)│    │          │    │          │
    └────┬─────┘    └────┬─────┘    └────┬─────┘
         │               │               │
         │ MakeMove      │ Notify        │ Notify
         └───────────────┴───────────────┘
                         │
                         ▼
                  ┌──────────────┐
                  │  Completed   │
                  │ (Disposed)   │
                  └──────────────┘
```

### Subscription Lifecycle

```
┌──────────────────┐
│  Not Subscribed  │
└────────┬─────────┘
         │ Subscribe(onState, onComplete)
         ▼
┌──────────────────┐
│   Subscribed     │ ◄─── Receives onState callbacks
│   (Active)       │
└────────┬─────────┘
         │
    ┌────┴────┐
    ▼         ▼
┌────────┐  ┌────────────┐
│Dispose │  │Game Complete│
│ called │  │ (Won/Draw) │
└───┬────┘  └─────┬──────┘
    │             │
    │             │ onComplete callback
    │             │
    └──────┬──────┘
           ▼
┌──────────────────┐
│  Unsubscribed    │
└──────────────────┘
```

## Validation Rules

### Subscribe

- `onStateChange` callback MUST NOT be null
- `onComplete` callback MUST NOT be null
- Returns IDisposable that removes subscription when disposed
- Calling Dispose multiple times is safe (no-op after first)

### MakeMove

- Game MUST NOT be disposed
- Move MUST be valid per game rules (enforced by Model.fs)
- Invalid moves emit Error state, then restore previous state
- State change notifies all active subscribers

### GetState

- Returns current game state synchronously
- Safe to call from any thread
- Returns last known state even after completion

## GameRef Changes

```fsharp
// CURRENT
type GameRef =
    { Game: Game
      Subscription: IDisposable  // ← Rx subscription
      Timestamp: DateTimeOffset }

// NEW (unchanged structure, different semantics)
type GameRef =
    { Game: Game
      Subscription: IDisposable  // ← Callback subscription
      Timestamp: DateTimeOffset }
```

The structure remains the same; only the subscription mechanism changes from IObservable to direct callbacks.
