![The Singularity Workshop animated moniker](assets/moniker.svg)

# Chapter 8 — Domain boundaries across the Workshop

| Responsibility | Owning domain |
|---|---|
| Authoritative world, simulation, and game rules | Experience / simulation domain |
| Observer-relative representation, horizons, visibility/detail policy | Renderer |
| FSM state primitives used by Renderer computation | FSM_API |
| Workload capability selection and concrete execution providers | Computation and its provider capabilities |
| Image/scene manifestation | Visualize |
| Semantic UI, accessibility, and human input boundary | GUI |
| Runtime composition and capability arbitration | FSM_COS |
| Capability/artifact meaning | MicroBundleDomain |
| Host lifecycle and device integration | Host / adapter |

These are responsibility boundaries, not necessarily mandatory direct package dependencies. Renderer must remain usable without depending upward on FSM_COS, GUI, or Visualize. Composition should connect the contracts; it should not collapse the domains.

> Part of the [Renderer theory learning path](RENDERING_THEORY.md). The [theory-to-implementation map](THEORY_IMPLEMENTATION_MAP.md) identifies current code, focused tests, and evidence limits.

[Previous chapter](THEORY_07_FRAME_HANDOFF.md) · [Theory index](RENDERING_THEORY.md) · [Next chapter](THEORY_09_PROOF_AND_EVIDENCE.md)

---

<p align="center">
  <img src="https://avatars.githubusercontent.com/u/16405167?v=4" width="48" alt="The Singularity Workshop maintainer avatar">
  <br>
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://github.com/TrentBest/TheSingularityWorkshop.Renderer">Renderer</a> · <a href="RENDERING_THEORY.md">Theory index</a>
</p>
