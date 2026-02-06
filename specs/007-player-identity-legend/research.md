# Research: Player Identity & Legend Display

**Feature**: 007-player-identity-legend
**Date**: 2026-02-05

## R1: User Identity Display in Layout

**Decision**: Display the first 8 characters of the user's GUID in the top-right corner of the page layout by reading `ClaimTypes.UserId` from `HttpContext.User`.

**Rationale**: The user's GUID is already available via `ctx.User.TryGetUserId()` (Auth.fs). The `mainLayout` function in `layout.fs` receives `HttpContext`, so it can extract the user ID without any new dependencies. Truncating to 8 characters provides sufficient uniqueness (4 billion combinations) while keeping the display compact.

**Alternatives considered**:
- Full GUID display: Too long, breaks layout.
- Auto-generated friendly names: Adds scope (name generation logic, potential collisions), deferred to future work.
- Separate user profile endpoint: Over-engineering for current needs.

## R2: Legend Data Availability in SSE Broadcasts

**Decision**: Pass `PlayerAssignment` data to both `renderGameBoardWithContext` and `renderGameBoardForBroadcast` so the legend can show player identifiers in all rendering paths.

**Rationale**: Currently, `renderGameBoardForBroadcast` has no player assignment context. The `PlayerAssignmentManager` is accessible from `Handlers.fs` where broadcasts are triggered. After each move, the assignment data is already retrieved as part of `TryAssignAndValidate`, so it can be passed to the rendering function with minimal additional cost. For the `subscribeToGame` observer callback, we need to query `PlayerAssignmentManager.GetAssignment(gameId)` to get current assignments.

**Alternatives considered**:
- Embed player IDs in Datastar signals: Would require client-side rendering logic, violating the hypermedia architecture principle.
- Cache assignments in the Game object: Mixes concerns; assignments belong in the Web layer, not the Engine.
- Separate SSE event for legend updates: Over-engineering; the legend is part of the game board HTML fragment.

## R3: Bold Turn Indicator in Legend

**Decision**: Use CSS `font-weight: bold` on the active player's legend entry, controlled by adding/removing a CSS class (e.g., `legend-active`) during server-side rendering.

**Rationale**: The `MoveResult` discriminated union already provides turn information (`XTurn` vs `OTurn` vs terminal states). The rendering functions already destructure this to determine `currentPlayer`. Adding a CSS class based on this existing data is trivial and aligns with the server-rendered hypermedia approach.

**Alternatives considered**:
- Datastar-driven client-side toggle: Adds client-side logic unnecessarily; server already knows the turn.
- Color change instead of bold: Spec explicitly requests bold; color can be added later if desired.

## R4: Layout Modification for User Identity Header

**Decision**: Modify `mainLayout` in `layout.fs` to wrap the page content in a header + main structure, with the user identifier in the header's top-right area.

**Rationale**: Currently `mainLayout` simply wraps content in a `<main>` element. Adding a header element with the user's identifier requires minimal structural change. The header will be rendered server-side as part of the layout, so it appears on every authenticated page. For unauthenticated pages (login), the layout can conditionally omit the identifier.

**Alternatives considered**:
- Separate header component file: Unnecessary for a single element; keep it in `layout.fs`.
- Client-side fetch for user info: Violates hypermedia principle; server already has the data.

## R5: Accessing PlayerAssignment from SSE Observer

**Decision**: Capture a reference to `PlayerAssignmentManager` in the `subscribeToGame` function so the observer callback can query assignments when broadcasting.

**Rationale**: The `subscribeToGame` function in `Handlers.fs` creates an `IObserver<MoveResult>` that broadcasts game state. To include the legend, this observer needs access to the `PlayerAssignmentManager` to call `GetAssignment(gameId)`. Since `subscribeToGame` is called from handlers that have access to the DI container, the manager can be passed as a parameter.

**Alternatives considered**:
- Global static reference to the manager: Anti-pattern; breaks testability.
- Include assignment data in `MoveResult`: Would require Engine changes (protected component).
