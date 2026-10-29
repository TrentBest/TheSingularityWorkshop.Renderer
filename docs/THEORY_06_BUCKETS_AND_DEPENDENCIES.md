![The Singularity Workshop animated moniker](assets/moniker.svg)

# Chapter 6 — Work buckets, dependencies, and temporal coherence

Different work has different invalidation rules. Geometry, visibility, lighting history, semantic summaries, and interaction affordances may update at different cadences. A dependency graph can express that a changed source invalidates dependent work without invalidating unrelated reusable results.

A work item should conceptually declare:
- the source identities and revisions it depends on;
- the observer/policy inputs that affect it;
- its validity and freshness conditions;
- the cost and importance of refreshing it;
- what downstream work must be invalidated if it changes.

This allows work to be cached or deferred safely. Reuse is correct only while its dependencies and observer requirements remain valid. An optimization that reuses stale output beyond its contract is a correctness bug, not a performance win.

Renderer should not create a second universal scheduler. It describes renderer-related work and its constraints; foundational state/scheduling capabilities can come from FSM_API, while independent Computation capabilities and providers own execution selection and execution. The host chooses when to request or present output according to its lifecycle.

> Part of the [Renderer theory learning path](RENDERING_THEORY.md). The [theory-to-implementation map](THEORY_IMPLEMENTATION_MAP.md) identifies current code, focused tests, and evidence limits.

[Previous chapter](THEORY_05_VISIBILITY_SEMANTICS_FRESHNESS.md) · [Theory index](RENDERING_THEORY.md) · [Next chapter](THEORY_07_FRAME_HANDOFF.md)

---

<p align="center">
  <img src="https://avatars.githubusercontent.com/u/16405167?v=4" width="48" alt="The Singularity Workshop maintainer avatar">
  <br>
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://github.com/TrentBest/TheSingularityWorkshop.Renderer">Renderer</a> · <a href="RENDERING_THEORY.md">Theory index</a>
</p>

## Implementation and evidence

**Source-checked:** [EventHorizonBucket.cs](../EventHorizonBucket.cs), [EventHorizonBucketDependencyGraph.cs](../EventHorizonBucketDependencyGraph.cs), and [EventHorizonBucketMembershipIndex.cs](../EventHorizonBucketMembershipIndex.cs) provide explicit bucket validity, declared invalidation dependencies, and membership lookup. Focused tests include [EventHorizonBucketTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/EventHorizonBucketTests.cs), [EventHorizonBucketDependencyGraphTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/EventHorizonBucketDependencyGraphTests.cs), and [EventHorizonBucketMembershipIndexTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/EventHorizonBucketMembershipIndexTests.cs). The [bucket invalidation/reuse proof](gallery/bucket-invalidation-and-reuse.md) connects those APIs in a runnable fixture.

**Limit:** dependencies must be declared; the graph does not discover arbitrary data dependencies or prove that a host scheduled the work. Fixture-level reuse is not a measured production speedup.

**Continue:** [Event Horizon buckets](EVENT_HORIZON_BUCKETS.md) and [Chapter 9 — Proving the theory](THEORY_09_PROOF_AND_EVIDENCE.md).
