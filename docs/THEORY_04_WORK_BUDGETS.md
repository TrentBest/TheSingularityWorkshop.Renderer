![The Singularity Workshop animated moniker](assets/moniker.svg)

# Chapter 4 — Budgets, not magic numbers

“Cheaper” is not one scalar. Relevant constraints may include CPU selection time, allocations, GPU time, memory residency, data transfer, synchronization, latency, uncertainty, and interaction precision. These dimensions may trade off against each other and should not be silently collapsed into one score.

A policy may choose among candidate representations by minimizing estimated work subject to explicit constraints:

`minimize estimatedCost(candidate)`

subject to preserving required observer information, meeting freshness and interaction constraints, and staying within the available budget.

This formulation is a design model, not a claim that the current package contains a universal optimizer. An implementation should begin with deterministic rules and measured workload traces. If a weighted score is later introduced, document the units, normalization, tradeoffs, and tie-breaking rules. Never allow a low visual-cost estimate to override authoritative simulation updates or required interaction behavior.

### Degradation order

When resources are scarce, prefer reducing optional presentation fidelity before dropping critical facts, interaction correctness, or authoritative domain updates. For example, reduce particle density before removing a hazard warning. Keep policy decisions explainable: which signal changed, which dimension was reduced, which constraint forced the fallback, and how stale the result may be.

> Part of the [Renderer theory learning path](RENDERING_THEORY.md). The [theory-to-implementation map](THEORY_IMPLEMENTATION_MAP.md) identifies current code, focused tests, and evidence limits.

[Previous chapter](THEORY_03_EVENT_HORIZONS.md) · [Theory index](RENDERING_THEORY.md) · [Next chapter](THEORY_05_VISIBILITY_SEMANTICS_FRESHNESS.md)

---

<p align="center">
  <img src="https://avatars.githubusercontent.com/u/16405167?v=4" width="48" alt="The Singularity Workshop maintainer avatar">
  <br>
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://github.com/TrentBest/TheSingularityWorkshop.Renderer">Renderer</a> · <a href="RENDERING_THEORY.md">Theory index</a>
</p>

## Implementation and evidence

**Source-checked:** [RendererPerformanceModel.cs](RendererPerformanceModel.cs) provides cost-model vocabulary and [RendererPerformanceModelTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/RendererPerformanceModelTests.cs) covers its current contract. [Benchmarking](BENCHMARKING.md) describes measurement discipline and interpretation.

**Limit:** a model estimate, asymptotic analysis, and wall-clock benchmark are different evidence classes. No universal budget or performance improvement follows from the existence of the model.

**Continue:** [Benchmarking](BENCHMARKING.md) and [Chapter 9 — Proving the theory](THEORY_09_PROOF_AND_EVIDENCE.md).
