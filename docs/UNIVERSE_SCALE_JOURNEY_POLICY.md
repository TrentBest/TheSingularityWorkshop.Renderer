# Universe Scale Journey — Renderer Policy Handoff

**Companion to:** [Visualize Universe Scale Journey](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/blob/work/gpu-foundation/docs/UNIVERSE_SCALE_JOURNEY.md)

**Status:** policy design for a proposed experience; not an implemented end-to-end pipeline.

## Renderer’s role in the journey

The Experience owns the narrative, camera waypoint sequence, pause timing, seed values, and authoritative universe model. Renderer answers a different question at each point in that journey:

> Given this observer, current intent, projected scale, available budget, and scene state, what is the cheapest representation that preserves what the viewer needs to perceive now?

Visualize then executes that selected work. Renderer must not hardcode a camera animation, galaxy storyline, Moniker asset, window API, or GPU backend into its core library.

## Semantic horizons for the experience

Treat these as authored policy classes, not mandatory fixed-distance rings.

| Horizon / phase | Useful representation | Work that can be reduced |
|---|---|---|
| Cosmic overview | Cluster/filament field, cell lattice, highlighted target cell | Per-galaxy identity and individual motion |
| Cell 42 approach | Cell boundaries, local cluster structure, target marker | Detail in irrelevant distant cells |
| Intrinsic transition | Nearby cell context, clear observer origin and direction | Exterior-only framing once the boundary is crossed |
| Galaxy | Spiral silhouette, arm density, major luminous regions | Individual stars beyond meaningful projected size |
| Stellar system | Sun and selected planets/orbits | Distant galaxy structure beyond contextual light |
| Earth | Globe, atmosphere, terrain, regional surface | Individual terrain/building detail until it projects meaningfully |
| Local / Moniker | Real local geometry and animated Moniker | Cosmic structure reduced to a contextual background |
| Optional material dive | Sign/wall layers and a declared lattice model | Full city/world detail not relevant to material inspection |

A phase change is not necessarily a physical distance. The camera route can use named semantic anchors and desired projected scale to request a transition, while Renderer resolves which representations and work buckets satisfy that request.

## Policy inputs and stable transitions

A future policy evaluation should be able to consider:

- observer pose and target intent;
- projected size / visual importance, not distance alone;
- camera speed and time-to-arrival, so detail can be prepared before a fast approach;
- semantic importance of the current authored beat;
- available frame and compute budget;
- representation readiness and transition state;
- hysteresis to avoid rapid switching near a threshold.

Representations need stable identity across promotion. A galaxy proxy and its later richer representation should refer to the same logical subject even though geometry, update cadence, or aggregation changes. Where possible, expose overlap/transition states so Visualize can cross-fade or morph instead of popping.

Do not put authored dramatic holds inside Event Horizon policy. The Experience owns the hold; Renderer may preserve extra detail during it because the observer's intent has changed from travelling to inspecting.

## Millions and billions: representation before execution

Renderer should distinguish these workload meanings in data and telemetry:

1. **Individually simulated entities** — each has independent state and update requirements.
2. **Individually represented instances** — many entities share geometry/material but can be placed separately.
3. **Procedural samples** — repeatable positions generated from a seed and region.
4. **Aggregated field** — a density, volume, or statistical representation communicates a population without materializing every member.
5. **Contextual summary** — the whole distant population may reduce to a silhouette, light field, or semantic cue.

Promotion should refine the same stable region/subject, not randomly regenerate it on each threshold crossing. Demotion can discard derived detail, but must not destroy authoritative state owned by the Experience.

“Billion-scale” is a scene requirement only after the contract states which of the meanings above is being counted. Renderer should expose candidate, selected, visible, and promoted counts separately. Visualize can then report encoded bytes, upload volume, submitted instances, and execution timings without conflating these values.

## Independent work buckets

The journey is a useful test for independently scheduled work:

- cosmic structure / cell context;
- galaxy and star population;
- stellar-system motion;
- planetary atmosphere and terrain;
- local constructs;
- Moniker animation;
- optional material inspection.

Bucket cadence should follow observer relevance, expected change, and correctness requirements—not simply one global frame tick. Close animation may need high cadence while distant cosmic context updates slowly. Camera-critical transition inputs must be ready before the camera reaches their target.

## Coordinate precision and identity

A single global floating-point coordinate frame is not an adequate assumption for a path from cosmic scales to a sign's material lattice. Use hierarchical region-local coordinates, camera-relative rendering coordinates, or an equivalent origin-rebasing strategy. Keep logical identity and parent/region transforms independent of a particular visual proxy.

The 10×10×10 cell layout has 1,000 cells. The Experience must author the exact linear-index rule for cell 42—index base, axis order, origin, and whether the label means a cell ID or an array offset. Renderer must consume the resulting stable cell identity, not infer it from an undocumented integer.

## First contract-sized proof

Before the full scale journey, test the Renderer policy with a deterministic fixture containing:

- 1,000 cell identities and a documented cell-42 mapping;
- a broad cosmic representation;
- a richer representation for the selected cell;
- a galaxy/stellar-system/planet/local hierarchy with stable subject IDs;
- multiple observer contexts along the same waypoint route;
- explicit budgets and expected representation changes.

Tests should verify stable identity during promotion/demotion, hysteresis, observer-dependent selection, reduced distant work, and readiness before arrival. They should not assert a specific graphics API, pixel output, or performance number.

The Visualize proof should then consume the selected frame through its actual supported path. A policy unit test, SVG storyboard, browser prototype, successful build, and hardware-verified GPU capture are distinct evidence levels.

## Ownership rule

- **Experience:** universe semantics, seeds, cell indexing, camera choreography, narrative holds, Moniker identity.
- **Renderer:** observer context, Event Horizon policy, stable representation identity, visibility and computational-detail selection, work-budget telemetry.
- **Visualize:** resource resolution, batching/instancing, GPU resource lifetime, submission, synchronization, backend diagnostics, pixels.
- **Host:** window/surface lifecycle, user controls, accessibility, capture/export.

This is the scale journey's core architectural test: change the observer's scale by many orders of magnitude without changing which layer owns the world.
