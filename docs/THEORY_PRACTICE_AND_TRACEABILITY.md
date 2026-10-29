![The Singularity Workshop animated moniker](assets/moniker.svg)

# Theory Practice and Traceability

This companion holds the reusable exercises and the high-level traceability obligations for Renderer’s textbook chapters. It is deliberately separate so instructors can adapt the exercises without editing the chapter sequence.

## 01 — What

Use this companion to challenge the theory and connect learning to implementation. The chapters themselves are independently linkable from the [Renderer Theory index](RENDERING_THEORY.md).

## 02 — Why

A theory that cannot be questioned becomes marketing. Practice should expose assumptions, counterexamples, and gaps between a policy primitive and a connected runtime.

## 03 — How

### Exercises

1. **Observer contrast:** Define two observers of the same world. List which facts and fidelity each requires, and explain the difference without changing authoritative state.
2. **Hysteresis:** Design a deterministic input trace that would make a single-threshold policy oscillate. Add a stable transition rule and test it.
3. **Budget fallback:** Given a CPU budget and an interaction-critical target, identify which presentation dimensions may degrade and which may not.
4. **Freshness:** Construct a case where an occluded entity remains semantically relevant. Specify the metadata needed to prevent stale information from appearing current.
5. **Dependency invalidation:** Change a light source while leaving geometry unchanged. Identify which derived work becomes stale and which can be reused.
6. **Benchmark falsification:** Propose a workload where total entity count rises but observer-relevant count stays constant. State the measurements that would support or refute the scaling hypothesis.
7. **Boundary review:** Trace one Experience from authoritative state to GUI interaction and Visualize output. Identify any place where presentation accidentally becomes authoritative.

### Implementation traceability

Use these existing guides as the source of implementation detail and evidence:

- [Start Here](START_HERE.md) — orientation and quick proof.
- [Event Horizon Theory](EVENT_HORIZON_THEORY.md) — focused explanation of horizons and transitions.
- [Architecture](ARCHITECTURE.md) — ownership and dependency direction.
- [Event Horizon buckets](EVENT_HORIZON_BUCKETS.md) — grouping and refresh concepts.
- [Observation model](OBSERVATION_MODEL.md) and [Semantic observation](SEMANTIC_OBSERVATION.md) — observer and structured knowledge.
- [Presentation handoff](PRESENTATION_HANDOFF.md) — neutral handoff and source-rectangle invariant.
- [GPU boundary](GPU_BOUNDARY.md) — frame description versus execution.
- [Benchmarking](BENCHMARKING.md) — measurement conditions and interpretation.
- [Theory-to-implementation map](THEORY_IMPLEMENTATION_MAP.md) — public contracts, focused tests, limitations, and the next vertical proof.
- [Implementation roadmap](IMPLEMENTATION_ROADMAP.md) — remaining integration and proof milestones.

For every public capability, the implementation record should eventually name the owning type/member, input contract, output contract, tests, sample, benchmark (where meaningful), limitations, and consuming repository. If one of these is absent, record it as a gap rather than implying completeness.

---

<p align="center">
  <img src="https://avatars.githubusercontent.com/u/16405167?v=4" width="48" alt="The Singularity Workshop maintainer avatar">
  <br>
  <a href="https://github.com/TrentBest">GitHub</a> ·
  <a href="https://github.com/TrentBest/TheSingularityWorkshop.Renderer">Renderer</a> ·
  <a href="https://www.nuget.org/packages/TheSingularityWorkshop.Renderer">NuGet</a> ·
  <a href="https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/DOCUMENTATION_STANDARD.md">Workshop documentation standard</a>
</p>

## 04 — See it in a minute

Choose a concept from the [theory-to-implementation map](THEORY_IMPLEMENTATION_MAP.md), find its named source and focused test, then identify one guarantee the test does not establish.

## 05 — Choose your next step

- [Theory index](RENDERING_THEORY.md)
- [Theory-to-implementation map](THEORY_IMPLEMENTATION_MAP.md)
- [Gallery](GALLERY.md)
- [Benchmarking](BENCHMARKING.md)

---

<p align="center">
  <img src="https://avatars.githubusercontent.com/u/16405167?v=4" width="48" alt="The Singularity Workshop maintainer avatar">
  <br>
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://github.com/TrentBest/TheSingularityWorkshop.Renderer">Renderer</a> · <a href="RENDERING_THEORY.md">Theory index</a>
</p>
