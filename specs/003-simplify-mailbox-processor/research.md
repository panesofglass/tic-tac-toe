# Research: Simplify MailboxProcessor - Remove System.Reactive

## Research Questions

### Q1: What is the minimal callback interface to replace IObservable<MoveResult>?

**Decision**: Use a simple callback function type `MoveResult -> unit` for state changes and `unit -> unit` for completion.

**Rationale**:
- The current IObservable usage only requires three capabilities:
  1. Notify on state change (`OnNext`)
  2. Notify on completion (`OnCompleted`)
  3. Notify on error (`OnError`)
- Direct function callbacks are simpler and more explicit
- No need for full IObservable interface complexity

**Alternatives Considered**:
- **Keep IObservable interface, remove System.Reactive**: Would require implementing Observable.Subscribe manually, adds complexity without benefit
- **Event-based (IEvent)**: F# events are less flexible for dynamic subscription management
- **Custom Subject type**: Unnecessary abstraction for this use case

### Q2: How should multiple subscribers be managed?

**Decision**: Maintain a thread-safe list of callbacks within the MailboxProcessor message loop.

**Rationale**:
- MailboxProcessor already provides thread-safe message processing
- Subscriber list can be managed as part of the actor's state
- Subscribe/Unsubscribe become messages to the actor
- No external synchronization needed

**Alternatives Considered**:
- **ConcurrentDictionary outside actor**: Splits state across two locations
- **ResizeArray with locks**: More complex, less idiomatic F#
- **Immutable list in actor state**: Chosen approach - simple and safe

### Q3: How to handle completion notification to supervisor?

**Decision**: Add a completion callback parameter when creating a game, or as a Subscribe option.

**Rationale**:
- GameSupervisor currently uses `OnCompleted` to know when to remove a game
- Instead: Game notifies completion via a callback registered at creation or subscription
- The callback is called when game reaches Won/Draw state

**Alternatives Considered**:
- **Polling for completion**: Inefficient, breaks existing real-time semantics
- **Return channel from CreateGame**: Overly complex

### Q4: What pattern for callback registration/disposal?

**Decision**: Subscribe returns an `IDisposable` that removes the callback when disposed.

**Rationale**:
- Maintains API compatibility with existing code patterns
- Familiar pattern for .NET developers
- Handlers.fs already expects `IDisposable` from subscriptions

**Implementation**:
```fsharp
type SubscriptionHandle(unsubscribe: unit -> unit) =
    interface IDisposable with
        member _.Dispose() = unsubscribe()
```

### Q5: How to provide current state to late subscribers?

**Decision**: Not needed - initial state is server-rendered; callbacks only push changes.

**Rationale**:
- Per clarification session: Initial game state is rendered server-side in HTML
- SSE subscriptions are for subsequent real-time updates only
- No replay-on-subscribe needed (unlike BehaviorSubject)
- `GetState` message available for explicit state queries if needed

**Alternatives Considered**:
- **Auto-deliver current state on subscribe**: Adds complexity, not needed per clarification

## Best Practices for MailboxProcessor Callback Pattern

### Pattern 1: Callbacks as Actor State

```fsharp
type GameMessage =
    | MakeMove of Move
    | Subscribe of (MoveResult -> unit) * (unit -> unit) * AsyncReplyChannel<IDisposable>
    | Unsubscribe of int  // subscription ID
    | Stop

type ActorState = {
    GameState: MoveResult
    Subscribers: Map<int, (MoveResult -> unit) * (unit -> unit)>
    NextId: int
}
```

### Pattern 2: Broadcast to Subscribers

```fsharp
let notifyAll (subscribers: Map<int, _>) (result: MoveResult) =
    subscribers |> Map.iter (fun _ (onNext, _) ->
        try onNext result with _ -> ())

let notifyComplete (subscribers: Map<int, _>) =
    subscribers |> Map.iter (fun _ (_, onComplete) ->
        try onComplete () with _ -> ())
```

### Pattern 3: Safe Unsubscribe via IDisposable

```fsharp
| Subscribe(onNext, onComplete, reply) ->
    let id = state.NextId
    let handle = { new IDisposable with
        member _.Dispose() = agent.Post(Unsubscribe id) }
    let newSubs = state.Subscribers |> Map.add id (onNext, onComplete)
    reply.Reply(handle)
    return! messageLoop { state with Subscribers = newSubs; NextId = id + 1 }
```

## Files Requiring Modification

| File | Changes Required |
|------|------------------|
| `TicTacToe.Engine.fsproj` | Remove `<PackageReference Include="System.Reactive" />` |
| `Engine.fs` | Replace BehaviorSubject with callback list; update Game interface |
| `Handlers.fs` | Update `IObserver` usages to callback functions |
| `EngineTests.fs` | Update `collectResults` helper to use new subscribe API |
| `SupervisorTests.fs` | Update game completion tracking |

## Risk Assessment

| Risk | Mitigation |
|------|------------|
| Breaking existing tests | Run full test suite after each change |
| Race conditions in callback management | All subscriber state managed within single MailboxProcessor |
| Performance regression | Callback invocation is simpler than Rx; likely faster |
| Incomplete callback invocation on errors | Wrap callback calls in try/catch (matches current behavior) |
