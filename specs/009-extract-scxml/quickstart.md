# Quickstart: Working with the SCXML Statechart

**Feature**: 009-extract-scxml

## File Locations

| File                     | Purpose                              |
| ------------------------ | ------------------------------------ |
| `docs/statechart.scxml`  | Hand-authored SCXML source of truth  |
| `docs/statechart.smcat`  | smcat DSL mirror for SVG generation  |
| `docs/statechart.svg`    | Generated SVG for README embedding   |
| `README.md`              | Embeds the SVG diagram               |

## Regenerating the SVG

After editing `docs/statechart.scxml`, update `docs/statechart.smcat` to match the SCXML structure, then regenerate:

```bash
smcat -T svg -d left-right docs/statechart.smcat -o docs/statechart.svg
```

**Why smcat DSL instead of SCXML input?** smcat's SCXML parser (`-I scxml`) has a bug where transitions in the 2nd+ child of a `<parallel>` element are silently dropped from the SVG. The smcat DSL format renders all transitions correctly. The SCXML remains the source of truth; the `.smcat` file is a visualization-only mirror with simplified guard labels.

Verify the output:
```bash
open docs/statechart.svg  # macOS
# or
xdg-open docs/statechart.svg  # Linux
```

## Viewing the smcat Internal Representation

To see how smcat interprets the SCXML (useful for debugging rendering issues):

```bash
smcat -I scxml -T smcat docs/statechart.scxml
```

## Embedding in README

The README references the SVG with standard markdown:

```markdown
![Tic-Tac-Toe Statechart](docs/statechart.svg)
```

For size control, use an HTML img tag instead:

```html
<img src="docs/statechart.svg" alt="Tic-Tac-Toe Statechart" width="800">
```

## What smcat Renders vs. What It Ignores

**Rendered in SVG**: States, transitions, event labels, guard conditions (`cond`), parallel regions (dashed borders), history pseudo-states ("H" circles), entry/exit actions, final states.

**Silently ignored**: `<datamodel>`, `<data>`, `<assign>`, `<send>`, `<if>`, `<log>`, `<script>`, root-level transitions.

The SCXML file is the complete specification. The SVG is a structural overview.

## Layout Options

smcat supports different layout directions if the default is hard to read:

```bash
# Top-down (default)
smcat -I scxml -T svg docs/statechart.scxml -o docs/statechart.svg

# Left-to-right (often better for wide parallel regions)
smcat -I scxml -T svg -d left-right docs/statechart.scxml -o docs/statechart.svg
```

## Validating the SCXML

No local SCXML validator is required for this feature. Validation is done by:

1. **smcat conversion**: `smcat -I scxml -T svg` succeeds without errors
2. **Visual inspection**: The SVG diagram correctly represents all states and transitions
3. **Manual trace**: Walk through game scenarios and verify the diagram matches expected behavior
