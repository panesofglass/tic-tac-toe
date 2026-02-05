# Feature Specification: Simplify MailboxProcessor - Remove System.Reactive Dependency

**Feature Branch**: `003-simplify-mailbox-processor`
**Created**: 2026-02-04
**Status**: Draft
**Input**: User description: "Remove dependency on System.Reactive and use simplest version of MailboxProcessor that can work. Document trade-offs so that a reasonable decision can be made before moving forward with implementation."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Maintain Current Game Functionality (Priority: P1)

As a player, I can play tic-tac-toe games with the same user experience as before the refactoring, including real-time updates and multi-game support.

**Why this priority**: This is the core requirement - removing System.Reactive must not break existing functionality. All current game behaviors must continue to work.

**Independent Test**: Can be fully tested by playing complete games through the web interface, verifying moves are reflected immediately and games complete correctly.

**Acceptance Scenarios**:

1. **Given** a new game is created, **When** any player views the game, **Then** they see the current board state immediately
2. **Given** an active game, **When** a player makes a valid move, **Then** all connected clients see the updated board state in real-time
3. **Given** a game in progress, **When** a player wins or the game draws, **Then** the game completion is broadcast to all connected clients
4. **Given** multiple games exist, **When** players interact with different games, **Then** each game operates independently without interference

---

### User Story 2 - Simplified Codebase (Priority: P1)

As a developer, I can understand and maintain the game state management code without needing knowledge of Reactive Extensions (Rx) patterns and operators.

**Why this priority**: The explicit goal is simplification - reducing dependencies and cognitive load for developers working with the codebase.

**Independent Test**: Can be verified by code review confirming no System.Reactive imports exist and that game state management uses standard F# constructs.

**Acceptance Scenarios**:

1. **Given** the refactored codebase, **When** I examine the Engine project, **Then** System.Reactive is not referenced in any project file
2. **Given** the refactored codebase, **When** I read the Game and GameSupervisor implementation, **Then** I can understand the state management using only F# async, MailboxProcessor, and standard library knowledge
3. **Given** the refactored codebase, **When** I need to modify game behavior, **Then** I do not need to understand IObservable, Subject, or Rx subscription semantics

---

### User Story 3 - Documented Trade-off Analysis (Priority: P1)

As a technical decision-maker, I have clear documentation of what capabilities are gained, lost, or changed by removing System.Reactive, enabling an informed go/no-go decision.

**Why this priority**: The user explicitly requested trade-off documentation before implementation proceeds. This is a prerequisite for the decision to implement.

**Independent Test**: Can be verified by reviewing the trade-off documentation and confirming it addresses all current System.Reactive usage patterns.

**Acceptance Scenarios**:

1. **Given** the trade-off analysis, **When** I review it, **Then** I understand what the current System.Reactive code provides
2. **Given** the trade-off analysis, **When** I review it, **Then** I understand what the replacement approach provides
3. **Given** the trade-off analysis, **When** I review it, **Then** I can make an informed decision about whether to proceed with the refactoring

---

### Edge Cases

- What happens when multiple clients subscribe to the same game simultaneously? (Must still work) → **Task T022 adds explicit test coverage**
- How does the system handle a game completing while new subscriptions are being established?
- What happens if a client disconnects and reconnects during a game? (Web-layer concern; SSE handles reconnection)
- How are stale/abandoned games cleaned up without Rx completion events? (Timer-based cleanup unchanged)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST remove the System.Reactive package reference from TicTacToe.Engine.fsproj
- **FR-002**: System MUST replace BehaviorSubject<MoveResult> with pure MailboxProcessor-based subscriber management while keeping the familiar IObservable<MoveResult> interface
- **FR-003**: System MUST maintain the ability for multiple consumers to observe game state changes
- **FR-004**: System MUST maintain the ability to broadcast game completion (Win/Draw) to all observers
- **FR-005**: GameSupervisor MUST continue to automatically clean up completed games
- **FR-006**: GameSupervisor MUST continue to clean up stale games after timeout (currently 1 hour)
- **FR-007**: Web handlers MUST continue to support Server-Sent Events (SSE) for real-time updates using IObserver callbacks
- **FR-008**: System MUST provide a trade-off analysis document comparing current vs. proposed implementation

### Key Entities

- **Game**: Single game instance managing board state, turn tracking, and move validation. Currently exposes state via IObservable<MoveResult>.
- **GameSupervisor**: Actor managing multiple game instances with lifecycle tracking. Uses MailboxProcessor internally, subscribes to game completion via Rx.
- **MoveResult**: Discriminated union representing game state (InProgress, Won, Draw, InvalidMove).
- **GameRef**: Internal tracking record containing Game reference, subscription handle, and timestamp for lifecycle management.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All existing Playwright and API tests pass without modification to test logic (test infrastructure changes acceptable)
- **SC-002**: System.Reactive package is removed from all project files
- **SC-003**: No code imports System.Reactive, System.Reactive.Subjects, or System.Reactive.Linq namespaces
- **SC-004**: Trade-off analysis document is reviewed and approved before implementation begins
- **SC-005**: Real-time game updates continue to work with latency comparable to current implementation (within 100ms)
- **SC-006**: Memory usage for game instances does not increase significantly (within 20% of current)

## Trade-off Analysis *(mandatory for this feature)*

### Current System.Reactive Usage

The codebase uses System.Reactive in two key areas:

1. **Game State Broadcasting (BehaviorSubject)**
   - `BehaviorSubject<MoveResult>` emits current state to new subscribers immediately
   - Broadcasts state changes to all active subscribers
   - Signals completion when game ends (Won/Draw)

2. **GameSupervisor Lifecycle Management**
   - Subscribes to each game's completion event
   - Automatic cleanup when game emits OnCompleted

### Proposed Alternative: Pure MailboxProcessor + Callbacks

Replace reactive streams with explicit state queries and callback registration. This approach uses direct callback registration (no IObservable interface) for maximum simplicity and minimal abstraction overhead.

### Trade-off Matrix

| Aspect                | Current (System.Reactive)                          | Proposed (MailboxProcessor + Callbacks)            |
|-----------------------|----------------------------------------------------|----------------------------------------------------|
| **Dependencies**      | External NuGet (System.Reactive 6.0.2)             | F# standard library only                           |
| **Learning Curve**    | Requires Rx knowledge                              | Standard F# async patterns                         |
| **Code Complexity**   | Higher (Rx operators, subscription semantics)      | Lower (explicit message passing)                   |
| **Push Semantics**    | Built-in (Subject broadcasts automatically)        | Must implement manually                            |
| **State Query**       | BehaviorSubject provides current value             | MailboxProcessor PostAndReply                      |
| **Completion Signal** | OnCompleted built into IObservable                 | Must implement completion callback                 |
| **Backpressure**      | Built-in Rx operators available                    | Manual implementation if needed                    |
| **Composability**     | Rich Rx operators (map, filter, merge, etc.)       | Custom implementation required                     |
| **Testing**           | TestScheduler, marble diagrams available           | Standard async testing                             |
| **Debugging**         | Rx call stacks can be opaque                       | Clear message-passing flow                         |
| **Memory**            | Subject overhead, subscription tracking            | Simpler, potentially lower overhead                |

### What is Lost

1. **Declarative stream composition**: Cannot easily compose game events with Rx operators
2. ~~**Built-in replay semantics**~~: Not needed - initial state is server-rendered; callbacks only push changes
3. **Standardized completion/error semantics**: OnCompleted/OnError are well-understood patterns
4. **Third-party tooling**: Rx debugging tools, marble diagram testing

### What is Gained

1. **Reduced dependencies**: One fewer NuGet package to track and update
2. **Simpler mental model**: Only F# MailboxProcessor patterns required
3. **Easier onboarding**: Developers don't need Rx expertise
4. **Explicit control flow**: Clear message-passing instead of implicit subscriptions
5. **Smaller binary size**: Removing System.Reactive reduces deployment footprint

### Neutral/Unchanged

1. **Concurrency safety**: Both approaches provide thread-safe state management
2. **Multi-subscriber support**: Both can notify multiple observers
3. **Async support**: Both work well with F# async

### Recommendation Considerations

**Favor keeping System.Reactive if**:
- Real-time features will expand (chat, spectating multiple games, event aggregation)
- Team has Rx expertise and finds it productive
- Future features need complex event composition

**Favor removing System.Reactive if**:
- Simplicity is valued over future flexibility
- Team members are not Rx experts
- The current usage is simple enough that custom callbacks suffice
- Minimizing dependencies is a project goal

## Clarifications

### Session 2026-02-04

- Q: What implementation approach for replacing System.Reactive? → A: Pure MailboxProcessor + callbacks (no IObservable interface, direct callback registration)
- Q: Late subscriber state semantics? → A: Explicit poll via GetState if needed, but initial state is server-rendered to page; callbacks only push subsequent changes (no replay-on-subscribe needed).

## Assumptions

- The current test suite (Playwright and API tests) provides sufficient coverage to verify refactoring correctness
- SSE implementation in web handlers can work with callback-based state notification
- Game instance lifecycle (1-hour timeout, completion cleanup) requirements remain unchanged
- No new features requiring complex event composition are planned in the near term

## Future Considerations

None identified. This is a focused simplification with no planned follow-on work.

## Dependencies

- Existing TicTacToe.Engine implementation (to be modified)
- Existing TicTacToe.Web handlers (to be modified)
- Existing test suite (to validate refactoring)

## Out of Scope

- Changing game rules or adding new game features
- Modifying the web UI or Datastar integration patterns
- Performance optimization beyond maintaining current behavior
- Adding new real-time features (e.g., chat, spectator mode)
