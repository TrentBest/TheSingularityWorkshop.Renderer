![The Singularity Workshop animated moniker](assets/moniker.svg)

# Chapter 7 — The frame is a handoff, not a GPU

A platform-neutral frame is a description of ordered rendering work. It is not proof that a GPU executed that work or that pixels were produced. A frame contract can describe views, commands, resource identities, index ranges, and other backend-neutral inputs; a concrete adapter translates them into a target execution path.

The logical content region must be explicit. Every GUI projection needs a **source rectangle** identifying which region of the logical content is projected. Do not infer it from destination dimensions. That same source region can be mapped to screen-space panels, textures, or world-space surfaces; destination-specific projection, camera, and perspective belong at the adapter/host boundary.

This boundary makes Renderer reusable. Visualize may realize scene or image output; GUI owns semantic interface composition and human interaction; hosts own device-specific presentation. None should become authoritative for the underlying Experience state.

> Part of the [Renderer theory learning path](RENDERING_THEORY.md). The [theory-to-implementation map](THEORY_IMPLEMENTATION_MAP.md) identifies current code, focused tests, and evidence limits.

[Previous chapter](THEORY_06_BUCKETS_AND_DEPENDENCIES.md) · [Theory index](RENDERING_THEORY.md) · [Next chapter](THEORY_08_WORKSHOP_BOUNDARIES.md)

---

<p align="center">
  <img src="https://avatars.githubusercontent.com/u/16405167?v=4" width="48" alt="The Singularity Workshop maintainer avatar">
  <br>
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://github.com/TrentBest/TheSingularityWorkshop.Renderer">Renderer</a> · <a href="RENDERING_THEORY.md">Theory index</a>
</p>
