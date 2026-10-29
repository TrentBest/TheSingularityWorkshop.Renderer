![The Singularity Workshop animated moniker](assets/moniker.svg)

# Chapter 9 — Proving the theory

A diagram is an explanation, not evidence. A generated SVG can be valuable when it faithfully depicts a real sample or test, but it must say whether it is conceptual, generated from executable data, or live telemetry.

A minimum deterministic proof should specify:
1. authoritative source state and stable identities;
2. observer input and policy configuration;
3. expected representation choice and reason;
4. promotion/demotion behavior under controlled input changes;
5. freshness, invalidation, and reconstruction rules;
6. measured work and allocations, with hardware/runtime/configuration noted;
7. what the proof does **not** establish.

Benchmarks should vary total population, observer-relevant population, number of observers, and change rate. Measure selection cost, allocations, refresh counts, reused/skipped work, and correctness together. Report raw conditions and compare like with like. A CPU/SVG proof does not establish GPU performance; a command-count reduction does not establish geometry-buffer compaction; an isolated microbenchmark does not establish end-to-end scaling.

The hypothesis worth testing is that the work required to serve an observer should track observer-relevant consequence more closely than total world population—without sacrificing required information, stability, or correctness.

> Part of the [Renderer theory learning path](RENDERING_THEORY.md). The [theory-to-implementation map](THEORY_IMPLEMENTATION_MAP.md) identifies current code, focused tests, and evidence limits.

[Previous chapter](THEORY_08_WORKSHOP_BOUNDARIES.md) · [Theory index](RENDERING_THEORY.md) · [Next chapter](THEORY_10_WORKED_HAZARD.md)

---

<p align="center">
  <img src="https://avatars.githubusercontent.com/u/16405167?v=4" width="48" alt="The Singularity Workshop maintainer avatar">
  <br>
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://github.com/TrentBest/TheSingularityWorkshop.Renderer">Renderer</a> · <a href="RENDERING_THEORY.md">Theory index</a>
</p>

## Implementation and evidence

**Source-checked:** the [theory-to-implementation map](THEORY_IMPLEMENTATION_MAP.md) links the main claims to public types and focused tests. The runnable [observer → buckets → frame proof](gallery/observer-bucket-frame.md) checks a connected CPU-side slice. [Benchmarking](BENCHMARKING.md) describes the separate requirements for performance claims.

**Limit:** a green build proves only the checks run by that workflow. A passing fixture is not proof of all inputs, a production runtime, GPU output, or a general scaling law. Keep the chain explicit: implementation → observation → proposition → evidence → counterexample/open question.

**Continue:** [Theory Practice and Traceability](THEORY_PRACTICE_AND_TRACEABILITY.md) and [Implementation Roadmap](IMPLEMENTATION_ROADMAP.md).
