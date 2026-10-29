![The Singularity Workshop animated moniker](assets/moniker.svg)

# Renderer Documentation Index

This is the map for learning, using, changing, and verifying Renderer. It follows the Workshop's five-part reader journey and points to the authoritative home for each topic.

**Product contract in one sentence:** Renderer provides graphics-API-independent contracts for choosing observer-relative representations and computational work, managing policy buckets and invalidation, and handing ordered frame data to a host or presentation adapter.

## 01 — What is Renderer?

- [Start Here](START_HERE.md) — the initial mental model and Workshop context.
- [README](../README.md) — product orientation, runnable quick proof, and package status.
- [Architecture and ownership](ARCHITECTURE.md) — responsibility boundaries and allowed dependency direction.
- [Rendering model](RENDERING_MODEL.md) — stable identity and materialized representation.
- [Presentation handoff](PRESENTATION_HANDOFF.md) — the platform-neutral frame boundary.

## 02 — Why is it designed this way?

- [Rendering Theory](RENDERING_THEORY.md) — modular textbook path, from the observer problem to proof.
- [Event Horizon Theory](EVENT_HORIZON_THEORY.md) — why computational representation can change at a horizon.
- [Observable phenomena and effect budgets](OBSERVABLE_PHENOMENA_AND_EFFECT_BUDGETS.md) — relevance, effect cost, and degradation.
- [Semantic observation](SEMANTIC_OBSERVATION.md) — observer-relative structured knowledge and freshness.
- [GPU boundary](GPU_BOUNDARY.md) — why a frame description is not proof of device execution.
- [Benchmarking](BENCHMARKING.md) — measurement discipline and interpretation.

## 03 — How do I use or change it?

**Understand the inputs and policies**
- [Observation model](OBSERVATION_MODEL.md)
- [Parallax and observer geometry](PARALLAX_AND_OBSERVER_GEOMETRY.md)
- [Event Horizon contracts](EVENT_HORIZONS.md)
- [Representation](REPRESENTATION.md)
- [Update frequency](UPDATE_FREQUENCY.md)
- [Obstruction and sensor visibility](OBSTRUCTION_AND_SENSOR_VISIBILITY.md)
- [Spatial semantics and affordances](SPATIAL_SEMANTICS_AND_AFFORDANCES.md)

**Understand the computation lifecycle**
- [Event Horizon buckets](EVENT_HORIZON_BUCKETS.md)
- [FSM scheduling model](FSM_SCHEDULING_MODEL.md)
- [FSM integration](FSM_INTEGRATION.md)
- [MicroBundle boundary](MICROBUNDLE.md)
- [Rendering MicroBundle integration](RENDERING_MICROBUNDLE_INTEGRATION.md)

**Inspect exact implementation and evidence**
- [Theory-to-implementation map](THEORY_IMPLEMENTATION_MAP.md)
- [UML source index and current coverage](UML/README.md)
- [Renderer host proof](integrations/RENDERER_HOST_PROOF.md)
- [Implementation roadmap](IMPLEMENTATION_ROADMAP.md)
- [Release readiness](RELEASE_READINESS.md)

## 04 — See it in a minute

Start with the runnable [observer → buckets → frame proof](gallery/observer-bucket-frame.md). It joins observer-relative selection, dependency-aware bucket invalidation, reuse of unchanged work, and ordered RenderFrame commands.

Then browse the [Gallery](GALLERY.md), which links each visual to its recipe, source, and evidence boundary. Current proofs are deterministic CPU-side model/frame proofs and SVG manifestations. They do not establish GPU execution or a universal performance improvement.

## 05 — Choose your next step

- **Learn the theory:** [Rendering Theory](RENDERING_THEORY.md) → [Theory practice and traceability](THEORY_PRACTICE_AND_TRACEABILITY.md).
- **Implement a change:** read [Architecture](ARCHITECTURE.md), then the focused contract and tests listed in the [implementation map](THEORY_IMPLEMENTATION_MAP.md).
- **Verify a claim:** run the linked sample/test and record revision, inputs, observed output, and limitations.
- **Coordinate with another Workshop repository:** use [Coordination](COORDINATION.md) and the relevant repository-owned *LLM.md handoff; do not transfer source ownership silently.

## Coverage and limitations

- **Theory:** ten standalone chapters; the theory index links each chapter and the separate practice/traceability companion.
- **Source-to-test mapping:** a focused map exists; it is not a complete generated API inventory.
- **UML:** [the UML index](UML/README.md) explicitly identifies its audited slice and pending coverage. Full source-file/type coverage and automated freshness enforcement are not claimed.
- **Living visuals:** see [SVG Living Documentation](SVG_LIVING_DOCUMENTATION.md) for provenance, regeneration commands, and what each artifact proves.
- **Avatar footer asset:** the expected Avatar.jpg is not currently present in this branch; existing pages still use the hosted avatar fallback. This is a known standards gap, not a claim of compliance.

---

<p align="center">
  <img src="https://avatars.githubusercontent.com/u/16405167?v=4" width="48" alt="The Singularity Workshop maintainer avatar">
  <br>
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://github.com/TrentBest/TheSingularityWorkshop.Renderer">Renderer</a> · <a href="RENDERING_THEORY.md">Theory</a>
</p>
