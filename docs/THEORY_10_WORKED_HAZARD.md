![The Singularity Workshop animated moniker](assets/moniker.svg)

# Chapter 10 — A worked example: the distant hazard

Imagine an authoritative simulation contains a spreading ash cloud, a transient eruption flash, and persistent ash deposited on surfaces.

- The simulation owns the physical evolution and source revisions.
- Renderer receives observer context and source-derived facts.
- The policy selects volume, particle, lighting, and semantic detail independently.
- A distant map observer may receive a regional hazard summary and uncertainty without a detailed volume.
- A first-person observer approaching the cloud may trigger promotion of local density and visibility detail.
- A transient flash may invalidate lighting history, while persistent surface deposition has a longer lifecycle.
- Under budget pressure, optional particles may be reduced, but a required hazard summary remains available.
- Visualize realizes the selected visual workload; the host presents it. No Renderer policy needs to know which graphics API is used.

This scenario forces the architecture to answer hard questions: what is authoritative, what is derived, what is stale, what can be aggregated, which dependencies are invalidated, and what evidence proves the chosen policy is both cheaper and adequate?

> Part of the [Renderer theory learning path](RENDERING_THEORY.md). The [theory-to-implementation map](THEORY_IMPLEMENTATION_MAP.md) identifies current code, focused tests, and evidence limits.

[Previous chapter](THEORY_09_PROOF_AND_EVIDENCE.md) · [Theory index](RENDERING_THEORY.md)

---

<p align="center">
  <img src="https://avatars.githubusercontent.com/u/16405167?v=4" width="48" alt="The Singularity Workshop maintainer avatar">
  <br>
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://github.com/TrentBest/TheSingularityWorkshop.Renderer">Renderer</a> · <a href="RENDERING_THEORY.md">Theory index</a>
</p>

## Implementation and evidence

**Status: conceptual worked scenario.** The ash-cloud example is a reasoning exercise, not a captured simulation trace or a claim that Renderer currently implements every policy described here. It illustrates how authoritative source revisions, observer context, independent representation choices, invalidation, and a required hazard summary should be traced through the architecture.

To turn it into executable evidence, define a deterministic source fixture, stable entity/revision identifiers, observer inputs, expected representation choices, dependency changes, expected frame commands, and a reproducible run. Then link the resulting tests/sample and state the remaining limitations in the [theory-to-implementation map](THEORY_IMPLEMENTATION_MAP.md).

**Continue:** [Theory Practice and Traceability](THEORY_PRACTICE_AND_TRACEABILITY.md) and the [Renderer Gallery](GALLERY.md).
