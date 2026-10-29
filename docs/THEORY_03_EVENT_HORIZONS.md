![The Singularity Workshop animated moniker](assets/moniker.svg)

# Chapter 3 — Event Horizons as computational boundaries

An **Event Horizon** is a spatial or contextual boundary at which Renderer changes the computational representation of an observable entity. It may be based on distance, projected size, visibility, intent, semantic consequence, time, uncertainty, or budget pressure. It need not be a literal sphere.

Conventional LOD usually changes geometric complexity. Renderer generalizes the idea to dimensions such as:

- geometry and material fidelity;
- animation and update cadence;
- residency and streaming;
- interaction precision and affordances;
- semantic detail and observation history;
- particles, volumes, lighting dependencies, and other presentation workloads.

Each dimension may have a different threshold and policy. An entity can therefore be geometrically simple, semantically important, and interaction-critical at the same time.

A conceptual policy can be written as:

`representation = Select(authoritativeState, observerContext, policy, budget, priorState)`

The prior state matters because good decisions are temporal: they should not flicker when inputs move slightly around a boundary. The policy and budget are experience-dependent; Renderer should supply reusable mechanisms rather than universal distance constants.

### Promotion and demotion

**Promotion** increases the fidelity, frequency, or availability of work when relevance rises. **Demotion** reduces work when detail is no longer justified. Neither operation should mutate the authoritative entity merely because its representation changed.

A robust policy may use hysteresis. For example, promote when distance falls below 9 units and demote only after it exceeds 11 units. The gap prevents small movements from repeatedly switching states. Actual thresholds must be calibrated to the scenario; the numbers are illustrative.

### Stable identity and reconstruction

A representation transition must preserve the identity of its source. A consumer should be able to tell that a coarse proxy and a detailed model refer to the same entity and source revision. Where an approximation or aggregation loses information, the policy should record that loss and identify when richer data is required again.

> Part of the [Renderer theory learning path](RENDERING_THEORY.md). The [theory-to-implementation map](THEORY_IMPLEMENTATION_MAP.md) identifies current code, focused tests, and evidence limits.

[Previous chapter](THEORY_02_OBSERVER_INPUT.md) · [Theory index](RENDERING_THEORY.md) · [Next chapter](THEORY_04_WORK_BUDGETS.md)

---

<p align="center">
  <img src="https://avatars.githubusercontent.com/u/16405167?v=4" width="48" alt="The Singularity Workshop maintainer avatar">
  <br>
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://github.com/TrentBest/TheSingularityWorkshop.Renderer">Renderer</a> · <a href="RENDERING_THEORY.md">Theory index</a>
</p>
