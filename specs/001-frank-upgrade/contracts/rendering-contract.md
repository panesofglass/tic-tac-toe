# Rendering Contract

**Date**: 2026-02-07

## Direct HTTP Response Rendering

For handlers that render full HTML pages directly to the response:

| Handler | Current Pattern | New Pattern |
|---------|----------------|-------------|
| `home` | `Render.toString` → `WriteAsync(html)` | `Render.toHtmlDocStreamAsync ctx.Response.Body view` |
| `getGame` | `layout.html \|> Render.toString` → `WriteAsync(html)` | `Render.toHtmlDocStreamAsync ctx.Response.Body (layout.html ctx gameHtml)` |

**Contract**: HTML output MUST be identical. Content-Type header MUST be `text/html; charset=utf-8`.

## SSE Broadcast Rendering

For game state broadcasts via SSE channels:

| Broadcast Type | Current Pattern | New Pattern |
|---------------|----------------|-------------|
| `broadcastPerRole` | `renderGameBoard \|> Render.toString` → `PatchElements html` | `PatchElements (fun tw -> renderGameBoard \|> Render.toTextWriterAsync tw)` |
| `broadcast PatchElementsAppend` | `renderGameBoard \|> Render.toString` → `PatchElementsAppend(sel, html)` | `PatchElementsAppend(sel, fun tw -> renderGameBoard \|> Render.toTextWriterAsync tw)` |

**Contract**: The `TextWriter -> Task` callback is invoked at write-time per subscriber. `Datastar.streamPatchElements` passes the SSE response's internal TextWriter to the callback. No intermediate string allocation occurs — true zero-copy streaming from Oxpecker view engine to HTTP response.

## SSE Event Write Contract

`writeSseEvent` dispatches events to the Frank.Datastar 7.1.0 stream API:

```
PatchElements(render)        → Datastar.streamPatchElements render ctx
PatchElementsAppend(sel, r)  → Datastar.streamPatchElementsWithOptions opts r ctx
RemoveElement(selector)      → Datastar.removeElement selector ctx
PatchSignals(json)           → Datastar.patchSignals json ctx
```
