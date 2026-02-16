# Implementation Plan: Extract SCXML Statecharts Definition

**Branch**: `009-extract-scxml` | **Date**: 2026-02-15 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/009-extract-scxml/spec.md`

## Summary

Extract the implicit statecharts from the TicTacToe application into a hand-authored W3C SCXML document with three orthogonal parallel regions (Game Play, Player Identity, Game Session Lifecycle). The SCXML serves as source of truth for a future Frank.Statecharts library prototype. An SVG visualization generated via smcat is embedded in the project README.

## Technical Context

**Language/Version**: XML (SCXML W3C Recommendation, version 1.0) with ECMAScript data model
**Primary Dependencies**: smcat 14.0.5 (locally installed) for SCXML → SVG conversion
**Storage**: N/A (documentation artifact, no runtime state)
**Testing**: Manual validation — smcat conversion succeeds, visual inspection of diagram, scenario trace-through
**Target Platform**: GitHub repository (SVG rendered in README.md)
**Project Type**: Documentation/specification (no application code changes)
**Performance Goals**: N/A
**Constraints**: SCXML must be parseable by smcat (`smcat -I scxml -T svg`); SVG must render on GitHub
**Scale/Scope**: Single SCXML file (~200-300 lines), single SVG, README update

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
| --------- | ------ | ----- |
| I. Functional-First F# | N/A | No F# code written. SCXML is a documentation artifact. |
| II. Hypermedia Architecture | N/A | No application changes. Statechart documents existing behavior. |
| III. Test-First Development | PASS | This is a documentation deliverable, not code. Validation via smcat conversion and visual inspection. |
| IV. Simplicity & Focus | PASS | Justified by GitHub issue #10 and Frank.Statecharts library prototype. Direct deliverable, no speculative additions. |
| Protected: TicTacToe.Engine | PASS | Engine is READ ONLY for this feature. No modifications. The SCXML models the domain concepts, referencing but not changing the engine. |

**GATE RESULT**: PASS — No violations.

**Post-Phase 1 re-check**: PASS — Design introduces no code changes, no new dependencies, no engine modifications. The `docs/` directory and README update are minimal additions.

## Project Structure

### Documentation (this feature)

```text
specs/009-extract-scxml/
├── plan.md              # This file
├── research.md          # Phase 0: SCXML best practices, smcat capabilities, GitHub SVG
├── data-model.md        # Phase 1: SCXML state/data model design
├── quickstart.md        # Phase 1: How to edit SCXML, regenerate SVG
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
docs/
├── statechart.scxml     # Hand-authored SCXML source of truth
└── statechart.svg       # Generated: smcat -I scxml -T svg

README.md                # Updated: embed statechart.svg inline
```

**Structure Decision**: A `docs/` directory at the repository root holds both the SCXML source and generated SVG. This keeps documentation artifacts separate from application code while remaining easily discoverable. The README references the SVG via relative path.

## Design Decisions

### 1. Three Orthogonal Parallel Regions

The statechart uses a single `<parallel>` element containing three child `<state>` regions:

1. **Game Play** — Turn-by-turn progression: XTurn ↔ OTurn → Won/Draw, with transient MoveError
2. **Player Identity** — Assignment tracking: Unassigned → XOnly/OOnly → BothAssigned
3. **Game Session Lifecycle** — Session management: Active → Disposed

These represent orthogonal domain concepts confirmed during clarification (FR-015, SC-008):
- Game rules are identity-agnostic (the engine doesn't care who's playing)
- Player identity is an application concern layered on game rules
- Session lifecycle is independent of in-game state

### 2. ECMAScript Data Model

The `datamodel="ecmascript"` attribute enables:
- Guard expressions: `board[pos] === 'Empty'`, `gameCount > 6`
- Data assignments: `board[pos] = 'X'`, `playerXId = userId`
- Cross-region predicates: `In('Won')`, `isParticipant(userId)`

This is critical for the Frank.Statecharts library prototype — it demonstrates what data model capabilities a statecharts library needs to support.

### 3. History State for Transient Error

MoveError uses `<history type="shallow">` to return to the previous turn state. This is the canonical SCXML pattern for "transient interruption that doesn't change state."

smcat renders history states as "H" circles (verified via live test).

### 4. SVG Generation Pipeline

```
docs/statechart.scxml → smcat -I scxml -T svg → docs/statechart.svg → README.md
```

smcat renders: states, transitions, events, guards, parallel regions, history.
smcat ignores: datamodel, data, assign, send, executable content.

The SCXML is the full specification; the SVG is a structural overview.

### 5. GitHub README Embedding

```markdown
![Tic-Tac-Toe Statechart](docs/statechart.svg)
```

GitHub renders SVGs referenced via markdown image syntax. smcat-generated SVGs use standard elements that survive GitHub's sanitizer.

## Implementation Phases

### Phase A: Author Game Play Region (P1)

Write the Game Play parallel region in SCXML:
- States: XTurn, OTurn, Won (final), Draw (final), MoveError
- History pseudo-state: GamePlayHistory (shallow)
- Transitions with ordered guard evaluation (win → draw → continue → error)
- Data model: board (9 positions), winner
- Validate: `smcat -I scxml -T svg` produces valid SVG showing all 5 game play states

### Phase B: Add Player Identity Region (P2)

Add the Player Identity parallel region:
- States: Unassigned, XOnlyAssigned, OOnlyAssigned, BothAssigned
- Transitions triggered by same move events as Game Play (parallel processing)
- Data: playerXId, playerOId
- Guard: userId-based assignment logic
- Validate: SVG shows both regions with correct parallel layout

### Phase C: Add Game Session Lifecycle Region (P3)

Add the Game Session Lifecycle parallel region:
- States: Active, Disposed (final)
- Transitions: game.dispose, game.reset, game.timeout with guards
- Data: gameCount, isParticipant() function
- Validate: SVG shows all three regions

### Phase D: README Integration (P1)

- Generate final SVG via smcat
- Experiment with layout direction (top-down vs left-right) for readability
- Update README.md with embedded SVG
- Verify rendering on GitHub (push branch, check PR preview)

## Risk Mitigation

| Risk | Mitigation |
| ---- | ---------- |
| smcat cannot render complex parallel SCXML | Verified via live test: parallel, history, transitions all render correctly |
| SVG too dense with 3 parallel regions | Use `smcat -d left-right` layout; simplify transition labels if needed |
| GitHub SVG sanitizer strips elements | smcat uses standard SVG elements (rects, text, paths) that survive sanitization |
| Root-level SCXML transitions dropped by smcat | Wrap all content in top-level compound state or parallel element (no root transitions needed for this design) |
| Guard expression syntax not preserved in SVG | Verified: `cond` attributes appear as transition labels in smcat output |
