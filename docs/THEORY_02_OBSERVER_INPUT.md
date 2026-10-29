![The Singularity Workshop animated moniker](assets/moniker.svg)

# Chapter 2 — The observer is part of the input

A representation is not correct in isolation; it is appropriate relative to an observer and a purpose. A first-person observer, a strategy map, an accessibility view, a remote spectator, and an AI agent can inspect the same authoritative world yet need different information.

An observer context can conceptually include:

- position, orientation, field of view, and visible region;
- purpose or intent, such as exploration, aiming, selecting, inspecting, or planning;
- interaction rights and precision requirements;
- perceptual or semantic salience;
- latency, freshness, uncertainty, and resource constraints;
- a stable observer identity and revision, so cached decisions can be invalidated correctly.

These are candidate policy inputs, not a promise that every listed signal is implemented in one public type today. Add a signal only when its meaning can be documented, tested, and traced to a decision.

### Example

A distant ambulance may occupy only a few pixels. Geometric detail can be reduced, but its emergency identity and direction of travel may remain essential. If the observer selects it, the representation may be promoted immediately. If it becomes occluded, the visual representation may be hidden while a recent semantic fact remains available with an explicit freshness limit.

The correct answer is not simply “far means low detail.” It is “which information does this observer need, and which work is justified to preserve it?”

> Part of the [Renderer theory learning path](RENDERING_THEORY.md). The [theory-to-implementation map](THEORY_IMPLEMENTATION_MAP.md) identifies current code, focused tests, and evidence limits.

[Previous chapter](THEORY_01_PROBLEM_AND_OBSERVER.md) · [Theory index](RENDERING_THEORY.md) · [Next chapter](THEORY_03_EVENT_HORIZONS.md)

---

<p align="center">
  <img src="https://avatars.githubusercontent.com/u/16405167?v=4" width="48" alt="The Singularity Workshop maintainer avatar">
  <br>
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://github.com/TrentBest/TheSingularityWorkshop.Renderer">Renderer</a> · <a href="RENDERING_THEORY.md">Theory index</a>
</p>

## Implementation and evidence

**Source-checked:** [ObserverRepresentationPolicy.cs](../ObserverRepresentationPolicy.cs) and [ObserverDistance.cs](../ObserverDistance.cs) make policy inputs and representation selection explicit. See [ObserverRepresentationPolicyTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/ObserverRepresentationPolicyTests.cs) and [ObserverDistanceDeterminismTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/ObserverDistanceDeterminismTests.cs).

**Limit:** the current bridge is not proof that every signal named in the theory—intent, occlusion, semantic relevance, freshness, or resource pressure—is integrated into one decision function.

**Continue:** [Chapter 3 — Event Horizons](THEORY_03_EVENT_HORIZONS.md) and [Observation Model](OBSERVATION_MODEL.md).
