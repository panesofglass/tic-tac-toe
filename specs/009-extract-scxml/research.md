# Research: Extract SCXML Statecharts

**Feature**: 009-extract-scxml
**Date**: 2026-02-15

## 1. SCXML Data Model Language

**Decision**: Use `datamodel="ecmascript"`

**Rationale**: ECMAScript provides natural expression syntax for the guard conditions needed (e.g., `board[pos] === 'Empty'`, `gameCount > 6`, `playerXId === null`). The `null` data model has no expression support. XPath requires verbose XML path expressions for simple comparisons.

**Alternatives considered**:
- `"null"` — No variables or guard expressions. Eliminated: cannot express game logic guards.
- `"xpath"` — XPath 1.0 expressions against XML data. Eliminated: verbose for simple comparisons, awkward data updates.

**Key capability**: The `In(stateId)` predicate allows cross-region guards in parallel states (e.g., `In('Won')` to check if GamePlay has finished from the GameSession region).

## 2. Transient Error State via History

**Decision**: Use `<history type="shallow">` pseudo-state within the GamePlay region. The MoveError state transitions to the history state, which restores the previous turn state.

**Rationale**: SCXML's `<history>` pseudo-state records the active configuration when the parent state is exited. Targeting it from MoveError restores whichever turn state was active before the error. This is the canonical SCXML pattern for "return to previous state after transient interruption."

**Alternatives considered**:
- Self-transitions on turn states (e.g., `XTurn → XTurn` on invalid move) — Would work but the spec requires an explicit Error state (FR-005).
- Separate Error states per turn (XTurnError, OTurnError) — Duplicates structure unnecessarily.

**Syntax**:
```xml
<state id="GamePlay" initial="XTurn">
  <history id="GamePlayHistory" type="shallow">
    <transition target="XTurn"/>  <!-- default if never entered -->
  </history>
  <!-- ... turn states ... -->
  <state id="MoveError">
    <transition target="GamePlayHistory"/>
  </state>
</state>
```

**smcat rendering**: History states render as "H" circles (correct UML notation). Verified via live test.

## 3. Parallel + Final State Interaction

**Decision**: When one child region of a `<parallel>` reaches `<final>` (e.g., GamePlay reaches Won), sibling regions continue running independently. `done.state.parallel_id` fires only when ALL children reach final.

**Rationale**: This is specified in the W3C SCXML recommendation. It means:
- GamePlay reaching Won/Draw does NOT stop PlayerIdentity or GameSession
- GameSession can still process disposal events after the game ends
- PlayerIdentity remains in BothAssigned (or wherever it was) after the game concludes
- Regions without `<final>` states stay active indefinitely

**Implication for design**: PlayerIdentity does NOT need a `<final>` state — it persists as long as the game session exists. GameSession has `<final id="Disposed"/>` which represents the end of the session.

## 4. smcat SCXML Rendering Capabilities

**Decision**: smcat renders the structural elements needed for visualization. Non-visual SCXML elements are silently ignored.

**Verified via live test** (`smcat -I scxml -T svg` and `smcat -I scxml -T smcat`):

| SCXML Element        | smcat Behavior                                      |
| -------------------- | --------------------------------------------------- |
| `<parallel>`         | Rendered as parallel regions with dashed borders     |
| `<history>`          | Rendered as "H" circle (UML notation)                |
| `<state>`, `<final>` | Rendered correctly                                  |
| `<transition>` event | Rendered as edge labels                              |
| `<transition>` cond  | Rendered as guard labels on edges                    |
| `<onentry>/<onexit>` | Rendered as `entry/`/`exit/` labels                  |
| `<datamodel>/<data>` | Silently ignored                                     |
| `<assign>`           | Silently ignored                                     |
| `<send>/<invoke>`    | Ignored (invoke partially handled)                   |
| `<if>/<log>/<script>`| Silently ignored                                     |
| Root-level transitions | Silently dropped (no concept of root state)        |

**Caveat**: Root-level `<transition>` elements on the `<scxml>` element are dropped. Workaround: wrap everything in a top-level compound state if root transitions are needed.

## 5. GitHub SVG Embedding

**Decision**: Use markdown image syntax `![alt](docs/statechart.svg)` with the SVG file committed to the repository.

**Rationale**: GitHub renders SVGs referenced via markdown image syntax or `<img>` tags. Inline `<svg>` is stripped entirely. Relative paths to committed files are the most reliable.

**Alternatives considered**:
- Inline `<svg>` in README — Stripped by GitHub sanitizer. Eliminated.
- Base64 data URIs — Stripped by GitHub. Eliminated.
- `<img>` tag — Works, allows width/height control. Use if sizing needed.

**Dark mode consideration**: smcat generates SVGs with white backgrounds. For dark mode support, could use `<picture>` element with light/dark variants, but this is a future enhancement. Default light-mode SVG is sufficient for initial delivery.

**Sanitization**: smcat-generated SVGs use standard elements (rects, text, paths) that survive GitHub's sanitizer without issues. Guard expressions in transition labels render as normal text.
