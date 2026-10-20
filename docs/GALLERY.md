# Renderer Gallery

**Choose the right work for the observer before handing it to presentation.**

This is the brief index. Each exhibit links to the document that explains its recipe, source, and evidence. The current SVGs are generated snapshots; they are not live telemetry. The [SVG living-documentation guide](SVG_LIVING_DOCUMENTATION.md) contains the full technical explanation.

## 01 — Event Horizons: representation states

![Event Horizon representation-state snapshot](assets/living-event-horizons.svg)

**See:** five FSM-backed computation machines advanced through actual state transitions; the generated SVG reads their resulting representation values. The glow, pulse, and motion are SVG presentation, not a live world.

**Recipe:** [Event Horizon state proof recipe](gallery/event-horizons.md).

## 02 — RenderFrame inspector: ordered work

![Renderer frame command inspector](assets/render-frame-inspector.svg)

**See:** observer position and the ordered CPU-side commands that downstream presentation can consume, including representation, entity, resource IDs, and draw ranges.

**Recipe:** [RenderFrame inspector recipe and run command](gallery/render-frame-inspector.md).

## 03 — Composition: where responsibilities live

![Renderer composition illustration](assets/renderer-composition.svg)

**See:** the intended relationship between observer context, Event Horizons, work buckets, and frame assembly. This is an architecture illustration, not proof that all stages currently run end to end.

**Recipe:** [Composition and ownership recipe](gallery/composition-ownership.md).

## 04 — Renderer → Visualize → GUI.SVG

The city-massing fixture in Visualize consumes a real Renderer frame, preserves ordered command identity through the adapter, and emits three material skins.

**See:** [Cross-repository proof path](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/blob/work/svg-living-docs/docs/SVG_LIVING_DOCUMENTATION.md#more-advanced-proof-outputs-generated-by-ci) · [Visualize Gallery](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/blob/work/svg-living-docs/docs/GALLERY.md).

## The next living proof

The current gallery makes Renderer state and command contracts inspectable, but it does not yet make the renderer itself live. The next exhibit should let the user change observer position or policy input and watch **real** selected representations, command counts, and work buckets change. The SVG should be driven by those outputs; timing and allocation data should be recorded alongside them. No pre-scripted animation should be described as live behavior.

Run the current generated samples and inspect the evidence rules in the [full guide](SVG_LIVING_DOCUMENTATION.md).

---

[Architecture and ownership](../README.md) · [Development workflow](DEVELOPMENT.md)
