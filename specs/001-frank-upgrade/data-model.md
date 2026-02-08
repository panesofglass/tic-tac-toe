# Data Model: Frank 7.0 Framework Upgrade

**Date**: 2026-02-07
**Branch**: `001-frank-upgrade`

## Entities Modified

### SseEvent (Modified)

**Current definition** (`SseBroadcast.fs`):
```
SseEvent =
    | PatchElements of html: string
    | PatchElementsAppend of selector: string * html: string
    | RemoveElement of selector: string
    | PatchSignals of json: string
```

**New definition**:
```
SseEvent =
    | PatchElements of render: (TextWriter -> Task)
    | PatchElementsAppend of selector: string * render: (TextWriter -> Task)
    | RemoveElement of selector: string
    | PatchSignals of json: string
```

**Rationale**: True zero-copy streaming via `TextWriter -> Task` callbacks. Frank.Datastar 7.1.0 provides `Datastar.streamPatchElements (writer: TextWriter -> Task) (ctx: HttpContext)` which passes the SSE response's internal TextWriter to the callback. Combined with Oxpecker's `Render.toTextWriterAsync`, no intermediate string allocation occurs at any point in the pipeline.

**State transitions**: None — `SseEvent` is a fire-and-forget message, not stateful.

### Framework Dependencies (Modified)

**Current**:
| Package | Version |
|---------|---------|
| Frank | 6.5.0 |
| Frank.Datastar | 6.5.0 |
| Oxpecker.ViewEngine | 1.1.0 |

**New**:
| Package | Version |
|---------|---------|
| Frank | 7.0.0 |
| Frank.Datastar | 7.1.0 |
| Frank.Auth | 7.0.0 |
| Oxpecker.ViewEngine | 2.0.0 |

### Authentication Configuration (Moved)

**Current location**: `Program.fs` → `configureServices` function (manual service registration)

**New location**: `Program.fs` → `webHost` CE via `useAuthentication` and `useAuthorization` custom operations from Frank.Auth

**Properties preserved**:
- Cookie name: `TicTacToe.User`
- HttpOnly: true
- SameSite: Strict
- SecurePolicy: SameAsRequest
- ExpireTimeSpan: 30 days
- SlidingExpiration: true
- LoginPath: `/login`

### Resource Auth Metadata (New)

**Resources gaining `requireAuth`**:
| Resource | Methods | Currently Protected | Change |
|----------|---------|-------------------|--------|
| `/` | GET | Yes (handler wrapper) | Declarative `requireAuth` |
| `/games` | POST | No | Add `requireAuth` |
| `/games/{id}` | GET, POST, DELETE | No | Add `requireAuth` |
| `/games/{id}/reset` | POST | Yes (handler wrapper) | Declarative `requireAuth` |
| `/login` | GET | No | No change |
| `/logout` | GET | No | No change |
| `/debug` | GET | No | No change |
| `/sse` | GET (datastar) | No | No change |
| `/games/{id}` | GET | No | No change |

## Files Affected

| File | Change Type | Description |
|------|-------------|-------------|
| `TicTacToe.Web.fsproj` | Modify | Update package references |
| `Program.fs` | Modify | Replace configureServices auth config with Frank.Auth CE operations; add `requireAuth` to resources; remove manual auth/authz middleware plugs |
| `Handlers.fs` | Modify | Remove `requiresAuth` function; replace `Render.toString` with streaming for direct responses; refactor SseEvent construction to use callbacks |
| `SseBroadcast.fs` | Modify | Change `SseEvent` DU cases to use callbacks; update `writeSseEvent` |
| `Extensions.fs` | No change | Logging extension stays as-is |
| `Auth.fs` | No change | Claims transformation stays as-is |
| `templates/game.fs` | No change | Render functions unchanged |
| `templates/home.fs` | No change | Template unchanged |
| `templates/shared/layout.fs` | No change | Layout unchanged |
