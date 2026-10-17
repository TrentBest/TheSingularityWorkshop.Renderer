![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# ✳️ 00 TheSingularityWorkshop.Renderer

[![Build](https://img.shields.io/github/actions/workflow/status/TrentBest/TheSingularityWorkshop.Renderer/build.yml?branch=development)](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

> **Choose the computational representation an observer needs—not just the detail a graphics API can draw.**

TheSingularityWorkshop.Renderer is a graphics-API-independent set of observer, representation, visibility, and computational-detail foundations. Its central idea is the **Event Horizon**: a boundary where the work needed to represent something can change with the observer and context.

~~~mermaid
flowchart TD
    W["World / experience state"] --> O["Observer context"]
    O --> H["Event Horizon policy"]
    H --> R["Representation and visibility contracts"]
    R --> C["Selected computational work"]
    C --> A["Host or presentation adapter"]
    A --> P["Pixels, a device view, or another observation"]
~~~

*The diagram describes the intended responsibility flow. It is not a claim that every stage is already wired into one end-to-end renderer.*

## 🟦 01 What

Renderer provides reusable .NET contracts and models for deciding **what representation is appropriate to an observer, and what computational work belongs with it**.

An Event Horizon can affect more than polygon count. Depending on policy, a representation may change geometry or material detail, animation/update cadence, simulation participation, interaction availability, semantic detail, visibility, or representation type.

The horizon count and thresholds are policy—not fixed constants imposed by Renderer.

## 🟣 02 Why

A distant object usually does not need the same continuously updated detail as something the observer is inspecting or manipulating. Conventional visual Level of Detail addresses only part of this problem.

Renderer explores reducing unnecessary work **before presentation** while preserving the information needed to promote an entity to a richer representation later. The intended benefit is more useful computation per available budget, not a blanket promise of a particular speedup.

Renderer is deliberately separate from a graphics engine: a decision about what must be represented should not require the world model to depend on WPF, Unity, WebGPU, DirectX, Vulkan, or another presentation API.

## 🔺 03 How

The working model separates responsibilities:

- **World or experience** owns authoritative meaning and state.
- **Observer context** describes the viewpoint, intent, visibility needs, and other inputs relevant to a representation.
- **Renderer** provides selection, representation, projection/visibility, and computational-policy contracts.
- **FSM_API** can supply state and scheduling primitives for Renderer computation.
- **Host or visualizer** turns selected representation data into a device-specific presentation.

Renderer does not own the composition operating system, a complete world simulation, or a production graphics backend. It must remain independently usable; **FSM_COS may compose Renderer capabilities, but Renderer must not depend upward on FSM_COS**.

The current library contains executable foundations for Event Horizon selection and buckets, an explicit distance-to-representation policy bridge, an FSM-to-bucket refresh coordinator, observer geometry, semantic anchors, obstruction/sensor visibility contracts, and FSM-backed computational representation state. Some architectural handoffs remain policy contracts or exploratory paths rather than a complete integrated rendering pipeline.

## 🟢 04 See it in a minute

Renderer is a library, not a standalone windowed demo. The quickest source-level proof is to run its tests from a checkout with the .NET 8 SDK:

~~~bash
dotnet test tests/TheSingularityWorkshop.Renderer.Tests/TheSingularityWorkshop.Renderer.Tests.csproj
~~~

The tests exercise the current contracts and their edge cases; they do **not** prove pixel output, GPU performance, or a complete host-to-renderer integration.

The first living documentation proof runs the real `RendererComputationMachine` through FSM_API and writes the resulting representation states into an SVG. The pulse is presentation-only; the state choices come from FSM transitions.

A separate cross-repository proof now renders a deterministic three-skin city fixture through Visualize's CPU/SVG path and verifies that distant representations submit fewer face commands: [open the generated SVG/report artifact](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/runs/39003350163/artifacts/12089384831). This is command-level detail reduction—not GPU output, geometry-buffer compaction, or production city rendering.

The broader [living Renderer composition explainer](docs/assets/renderer-composition.svg) shows how observer context, Event Horizons, independent work buckets, and frame assembly fit together. It is an animated architecture guide, not live telemetry; the end-to-end connected path is still a roadmap goal. See the [living Renderer guide](docs/LIVING_RENDERER.md) for the implementation evidence and next proofs.

![Living Event Horizons generated from FSM_API representation states](docs/assets/living-event-horizons.svg)

Regenerate it from the repository root with:

~~~bash
dotnet run --project samples/LivingRenderer/LivingRenderer.csproj --configuration Release -- docs/assets/living-event-horizons.svg
~~~

This is a generated snapshot, not a live process monitor. CI checks that the committed SVG matches the runnable sample.

Want to see a soldier instead of a model test? Open the [Soldier Field WebGPU lab](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/tree/development/samples/SoldierField/WebGpuRunner) in the Visualize repository. It is an exploratory browser-side rendering path with deterministic procedural geometry and an Event Horizon scaling workload. It is **not yet an end-to-end integration of Visualize's .NET contracts with Renderer policy**. Follow its [run instructions](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/blob/development/samples/SoldierField/WebGpuRunner/README.md); a browser with WebGPU support is required.

## 🟪 05 Available documentation and theory

- [Start here](docs/START_HERE.md) — a guided explanation of the model and its place in the Workshop.
- [Living Renderer guide](docs/LIVING_RENDERER.md) — visual teaching path, what is executable today, and the next evidence milestones.
- [What Event Horizons mean](docs/EVENT_HORIZON_THEORY.md) — the conceptual rationale behind observer-relative computational detail.
- [Event Horizon contracts](docs/EVENT_HORIZONS.md) and [bucket model](docs/EVENT_HORIZON_BUCKETS.md) — selection and grouping behavior, with the policy boundaries made explicit.
- [Architecture](docs/ARCHITECTURE.md) — component ownership and dependency direction.
- [Rendering model](docs/RENDERING_MODEL.md) and [representation](docs/REPRESENTATION.md) — how stable identity can outlive a particular materialized form.
- [Observation model](docs/OBSERVATION_MODEL.md) and [obstruction/sensor visibility](docs/OBSTRUCTION_AND_SENSOR_VISIBILITY.md) — observer geometry and channel-specific visibility contracts.\n- [Semantic observation](docs/SEMANTIC_OBSERVATION.md) — Event Horizons as structured observations for AI/FSM agents, including horizon summaries, relations, uncertainty, and freshness.
- [Spatial semantics and affordances](docs/SPATIAL_SEMANTICS_AND_AFFORDANCES.md) — spatially grounded concepts, named geometry anchors, visible widgets, and pose-aware interactions.
- [FSM integration](docs/FSM_INTEGRATION.md) and [scheduling model](docs/FSM_SCHEDULING_MODEL.md) — where FSM_API fits without making it a graphics engine.
- [GPU boundary](docs/GPU_BOUNDARY.md) — the intended division between CPU-side meaning/policy and GPU-facing execution.
- [Rendering mathematics](docs/RENDERING_MATH.md) and [benchmarking](docs/BENCHMARKING.md) — cost hypotheses, measurements, and how to interpret their limits.
- [MicroBundle boundary](docs/MICROBUNDLE.md) — how Renderer capabilities may participate in composition without requiring FSM_COS.
- [Documentation index](docs/START_HERE.md) — use Start Here as the front door to the focused guides.
- [Implementation roadmap](docs/IMPLEMENTATION_ROADMAP.md) — prioritized completeness criteria, integration proofs, and benchmark milestones.

## Current status and evidence

Renderer is **alpha software**. The project has a .NET 8 library and automated tests for its current pure-model contracts. Its benchmark suite provides machine-specific measurements for selected operations; those measurements are calibration evidence, not proof that the complete system outperforms an established production renderer.

Current implementation, design intent, and exploratory demonstrations are not interchangeable. The documentation linked above identifies the relevant contracts and known boundaries. The browser Soldier Field remains a separate Visualize lab until an explicit, tested policy handoff connects it to Renderer.

## Build and contribute

- SDK: .NET 8.
- Build: dotnet build TheSingularityWorkshop.Renderer.csproj
- Test: dotnet test TheSingularityWorkshop.Renderer.slnx
- Benchmark laboratory: [TheSingularityWorkshop.Renderer.Benchmarks](https://github.com/TrentBest/TheSingularityWorkshop.Renderer.Benchmarks).

Read [Architecture](docs/ARCHITECTURE.md) before changing responsibility boundaries. Keep changes measurable, preserve the library's graphics-API independence, and distinguish verified behavior from proposed architecture.

---

<p align="center">
  <img src="https://avatars.githubusercontent.com/u/16405167?v=4" width="48" alt="The Singularity Workshop maintainer avatar">
  <br>
  <a href="https://github.com/TrentBest">GitHub</a> ·
  <a href="https://github.com/TrentBest/TheSingularityWorkshop.Renderer">Renderer</a> ·
  <a href="https://www.nuget.org/packages/TheSingularityWorkshop.Renderer">NuGet</a> ·
  <a href="https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/DOCUMENTATION_STANDARD.md">Workshop documentation standard</a>
</p>
