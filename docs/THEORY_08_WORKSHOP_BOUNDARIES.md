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

## Implementation and evidence

**Source-checked:** [Architecture](ARCHITECTURE.md) records the allowed responsibility direction. [RendererMicroBundle.cs](RendererMicroBundle.cs) and [RendererMicroBundleTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/RendererMicroBundleTests.cs) cover the capability boundary, while [FSM_COS](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS) owns runtime composition.

**Limit:** a composable Renderer capability does not mean Renderer depends on FSM_COS, that a host loads the capability, or that a production backend is active. Experience state and rules, GUI intent, Visualize manifestation, and host lifecycle remain separate responsibilities.

**Continue:** [MicroBundle boundary](MICROBUNDLE.md), [Rendering MicroBundle Integration](RENDERING_MICROBUNDLE_INTEGRATION.md), and [Workshop documentation standard](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/DOCUMENTATION_STANDARD.md).
