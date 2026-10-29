![The Singularity Workshop animated moniker](assets/moniker.svg)

# Renderer Theory: Observer-Relative Computation

> A modular, textbook-style learning path for understanding why Renderer exists, how observer-relative computation should be reasoned about, and how each proposition maps to code and evidence.

Renderer’s theory is about the domain Renderer owns: **choosing observer-relative representations and computational work while preserving authoritative meaning, identity, freshness, and a safe handoff to presentation**. It does not replace FSM_API’s general state-machine theory, FSM_COS’s composition theory, or Visualize’s manifestation contracts. Follow the chapter sequence, or open any chapter independently.

## 01 — What

Renderer asks a different question from a graphics API: not merely “what can this device draw?” but “what information and work does this observer need now?” The authoritative world remains owned by its source domain; Renderer chooses a representation for an observation.

## 02 — Why

Uniform detail can spend work on information that an observer cannot use, while naive omission can destroy continuity, hide important facts, or make richer detail impossible to recover safely. The theory explores the policy, invalidation, and evidence needed to avoid both mistakes.

## 03 — How: follow the learning path

- [Chapter 01 — The problem: a world is larger than one observer's needs](THEORY_01_PROBLEM_AND_OBSERVER.md)
- [Chapter 02 — The observer is part of the input](THEORY_02_OBSERVER_INPUT.md)
- [Chapter 03 — Event Horizons as computational boundaries](THEORY_03_EVENT_HORIZONS.md)
- [Chapter 04 — Budgets, not magic numbers](THEORY_04_WORK_BUDGETS.md)
- [Chapter 05 — Visibility, semantics, and freshness are different questions](THEORY_05_VISIBILITY_SEMANTICS_FRESHNESS.md)
- [Chapter 06 — Work buckets, dependencies, and temporal coherence](THEORY_06_BUCKETS_AND_DEPENDENCIES.md)
- [Chapter 07 — The frame is a handoff, not a GPU](THEORY_07_FRAME_HANDOFF.md)
- [Chapter 08 — Domain boundaries across the Workshop](THEORY_08_WORKSHOP_BOUNDARIES.md)
- [Chapter 09 — Proving the theory](THEORY_09_PROOF_AND_EVIDENCE.md)
- [Chapter 10 — A worked example: the distant hazard](THEORY_10_WORKED_HAZARD.md)

The chapter order moves from the motivating problem through observer inputs, Event Horizons, budgets, visibility and freshness, dependency-aware work, platform-neutral handoff, domain boundaries, proof methodology, and a worked hazard scenario.

## 04 — See it in a minute

Start with [Observer → buckets → frame](gallery/observer-bucket-frame.md) for a deterministic, executable CPU-side proof that connects observer-driven representation selection to bucket invalidation/reuse and ordered frame commands. Then read [Theory Practice and Traceability](THEORY_PRACTICE_AND_TRACEABILITY.md) to challenge the claims against source and tests.

**Evidence boundary:** the executable sample proves the behavior asserted by its fixture and tests. It does not establish GPU execution, production scheduling, pixel output, or a universal performance improvement.

## 05 — Choose your next step

- [Theory practice, exercises, and traceability](THEORY_PRACTICE_AND_TRACEABILITY.md)
- [Theory-to-implementation map](THEORY_IMPLEMENTATION_MAP.md) — current types, tests, limits, and integration gaps
- [Architecture and ownership](ARCHITECTURE.md)
- [Event Horizon theory](EVENT_HORIZON_THEORY.md)
- [Gallery](GALLERY.md) — two-sentence overview and runnable evidence recipes
- [Benchmarking](BENCHMARKING.md) — measured cost and evidence discipline
- [Workshop documentation standard](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/DOCUMENTATION_STANDARD.md)



---

<p align="center">
  <img src="https://avatars.githubusercontent.com/u/16405167?v=4" width="48" alt="The Singularity Workshop maintainer avatar">
  <br>
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://github.com/TrentBest/TheSingularityWorkshop.Renderer">Renderer</a> · <a href="https://www.nuget.org/packages/TheSingularityWorkshop.Renderer">NuGet</a>
</p>
