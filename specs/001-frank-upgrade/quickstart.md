# Quickstart: Frank 7.0 Framework Upgrade

**Date**: 2026-02-07
**Branch**: `001-frank-upgrade`

## Prerequisites

- .NET 10.0 SDK (already installed)
- Frank 7.0.0, Frank.Datastar 7.1.0, Frank.Auth 7.0.0 source at `../frank`
- Oxpecker source at `../Oxpecker`

## Build & Test

```bash
# Build
dotnet build

# Run unit tests (no server needed)
dotnet test test/TicTacToe.Engine.Tests/

# Run web tests (unit tests only - Expecto)
dotnet test test/TicTacToe.Web.Tests/ --filter "FullyQualifiedName~PlayerAssignment|FullyQualifiedName~GameBoard|FullyQualifiedName~Auth"

# Run server for integration tests
dotnet run --project src/TicTacToe.Web/ &
TEST_BASE_URL=http://localhost:5228 dotnet test test/TicTacToe.Web.Tests/
```

## Key Changes Summary

### 1. Package References (`TicTacToe.Web.fsproj`)

```xml
<!-- Before -->
<PackageReference Include="Frank" Version="6.5.0" />
<PackageReference Include="Frank.Datastar" Version="6.5.0" />
<PackageReference Include="Oxpecker.ViewEngine" Version="1.1.0" />

<!-- After -->
<PackageReference Include="Frank" Version="7.0.0" />
<PackageReference Include="Frank.Datastar" Version="7.1.0" />
<PackageReference Include="Frank.Auth" Version="7.0.0" />
<PackageReference Include="Oxpecker.ViewEngine" Version="2.0.0" />
```

### 2. Program.fs Auth Configuration

Replace manual `configureServices` auth setup with Frank.Auth CE operations:

```fsharp
// Before: manual service config + manual middleware plugs
service configureServices
plugBeforeRouting AuthAppBuilderExtensions.UseAuthentication
plugBeforeRouting AuthorizationAppBuilderExtensions.UseAuthorization

// After: Frank.Auth CE operations (handles both services AND middleware)
useAuthentication (fun builder ->
    builder.AddCookie(fun options ->
        options.Cookie.Name <- "TicTacToe.User"
        options.Cookie.HttpOnly <- true
        // ... same cookie config
        options.LoginPath <- "/login")
    builder)
useAuthorization
```

### 3. Resource-Level Auth

```fsharp
// Before: handler wrapper
resource "/" {
    name "Home"
    get (Handlers.requiresAuth Handlers.home)
}

// After: declarative
resource "/" {
    name "Home"
    requireAuth
    get Handlers.home
}
```

### 4. Direct Response Streaming

```fsharp
// Before:
let html = view |> layout.html ctx |> Render.toString
ctx.Response.ContentType <- "text/html; charset=utf-8"
do! ctx.Response.WriteAsync(html)

// After:
ctx.Response.ContentType <- "text/html; charset=utf-8"
do! view |> layout.html ctx |> Render.toHtmlDocStreamAsync ctx.Response.Body
```

### 5. SseEvent DU Refactoring

```fsharp
// Before:
type SseEvent =
    | PatchElements of html: string
    | PatchElementsAppend of selector: string * html: string

// After (using TextWriter -> Task callbacks for zero-copy streaming):
type SseEvent =
    | PatchElements of render: (TextWriter -> Task)
    | PatchElementsAppend of selector: string * render: (TextWriter -> Task)
```

### 6. SSE Event Writing (uses Frank.Datastar 7.1.0 stream API)

```fsharp
// Before:
| PatchElements html -> do! Datastar.patchElements html ctx

// After:
| PatchElements render -> do! Datastar.streamPatchElements render ctx
```

### 7. SSE Broadcast Rendering (zero-copy)

```fsharp
// Before:
let html = renderGameBoard gameId result userId assignment gameCount |> Render.toString
PatchElements html

// After:
PatchElements (fun tw -> renderGameBoard gameId result userId assignment gameCount |> Render.toTextWriterAsync tw)
```

## Performance Benchmarking

```bash
# Run benchmarks (after creating benchmark project)
dotnet run --project test/TicTacToe.Benchmarks/ -c Release
```
