# Renderer Architecture

## Goal

The Renderer is an independent rendering-computation capability for The Singularity Workshop.

It describes observer-relative representations and their computational policy without depending on a particular graphics platform. Some policy and presentation handoffs remain incomplete; this is a distinction between integration maturity and the independence of the core model.

## Conceptual pipeline

```
Experience / World
       |
       v
     Entity
       |
       v
 Observer Context
       |
       v
 Event Horizon Policy
       |
       v
 Representation Selection
       |
       v
 Update / Simulation Policy
       |
       v
 Renderer Output
       |
       v
 Platform Adapter / Host
       |
       v
 Display / Device
```

The first five stages are the architectural center of this repository.

The final stages are deliberately replaceable.

![FSM-backed representation states manifested as a living SVG](assets/living-event-horizons.svg)

The illustration is produced by the runnable sample under `samples/LivingRenderer`: it advances one `RendererComputationMachine` per target through FSM_API, then writes the resulting states into SVG. Its pulse is a presentation effect, not a second scheduler.

## Boundaries

### Renderer owns

- observer-aware representation selection
- Event Horizon evaluation
- computational detail policy
- representation lifecycle
- rendering-oriented update policy
- renderer-facing semantic contracts
- performance instrumentation and measurement

### Renderer does not own

- application composition
- experience orchestration
- package arbitration
- persistent world storage
- a specific windowing framework
- a specific GPU API
- a specific game engine
- business/application logic

## Execution, rendering, visualization, and interaction

These are neighboring responsibilities, not interchangeable names for one pipeline:

| Responsibility | Owner | Contract |
|---|---|---|
| Execute a workflow and own its run lifecycle | Foundry / Hub workflow pipeline | Starts, coordinates, and reports work; Renderer must not duplicate this lifecycle |
| Decide observer-relative representation and describe renderable output | Renderer | Converts source state and observer/policy inputs into rendering-oriented representations and frame data |
| Manifest pixels, scenes, or world-space visuals | Visualize or a concrete presentation adapter | Turns suitable output into a visual manifestation; it is not the workflow executor |
| Present semantic controls and collect human intent | GUI | Owns interaction-oriented UI semantics and presentation, not Renderer policy |

Foundry outcomes can be rendered as **data**: a host or adapter may map a workflow status, result, or diagnostic into a Renderer-compatible representation. That does not transfer execution authority, cancellation ownership, retry policy, or run lifecycle into Renderer. Renderer consumes the outcome and describes output; it does not schedule the workflow that produced it.

Likewise, a GUI may ask to inspect, filter, or act on an outcome, but those interaction semantics belong to GUI and the owning application. Renderer should not grow windowing, panel-layout, input-event, or GUI-framework dependencies to support that presentation. Visualize may provide an optional visual manifestation without becoming the owner of workflow orchestration.

This table states the intended ownership boundary. It does not claim a complete Foundry-to-Renderer adapter or a production GUI/Visualize integration exists today; those require source-backed implementation and tests.

## Workshop relationships

### FSM_API

FSM_API is a direct dependency and is already used for stateful representation computation: `RendererComputationMachine` creates and advances an FSM whose active state is the current `RepresentationTarget`. This is an executable integration, not merely a possible future dependency. The full observer-policy → bucket/cadence → selected frame → presentation path is not yet one complete end-to-end pipeline. Renderer should extend FSM_API where it fits the workload rather than build a parallel scheduler.

### Ontology

Ontology can describe what an entity is and how it relates to other entities. Renderer can use those semantics when deciding which representation is meaningful.

### FSM_COS

FSM_COS can compose Renderer capabilities into a runtime.

Renderer must not require FSM_COS simply to define or execute its core model.

### Micro Bundles

The core Renderer is itself a MicroBundle capability through the RendererMicroBundle adapter
included in this package.

Additional rendering capabilities can become sibling MicroBundles under the Renderer
ontology. They do not automatically require separate NuGet packages; package boundaries
should follow independent distribution, versioning, dependency, configuration, lifecycle,
or arbitration needs.

FSM_COS consumes these MicroBundles through MicroBundleDomain. It should never need to
learn Renderer-specific types merely to assemble a runtime.

See [Renderer MicroBundle](MICROBUNDLE.md).

### Computation and GPU execution

Renderer defines the visual workload: what must be represented, what representation survives the Event Horizon policy, and what computational work is justified. It does not own the concrete CPU or GPU execution machinery.

The neutral `RenderFrame` is one workload representation. The independent **Computation** capability defines how workloads can be described, selected, scheduled, measured, and matched to available execution capabilities. Concrete CPU and GPU execution are supplied by their own MicroBundles.

```text
Renderer
   |
   | visual workload
   v
Computation
   |
   +--> CPU MicroBundle
   |
   +--> GPU MicroBundle
   |
   +--> future accelerator MicroBundle
```

A GPU MicroBundle may translate Workshop GPU work into DirectX, Vulkan, WebGPU, OpenGL, CUDA, or another native path. Those APIs are execution boundaries, not Renderer architecture.

See [GPU Boundary](GPU_BOUNDARY.md).

### Hosts

AnyApp, Web, Unity integration, VR environments, or future hosts can adapt Renderer output to their presentation technology.

The host should not redefine the Renderer model.

## Architectural rule

> **The Renderer decides what must be represented. The host decides how that representation is presented.**

That separation is one of the most important constraints of the project.

## Hardware-aware computation

The platform-neutral Renderer is not intended to collapse hardware into one generic lowest-common-denominator path. Hardware capability discovery and execution selection belong to the independent Computation capability and its execution-provider MicroBundles.

The Renderer describes the work; Computation discovers what execution capabilities are available and selects an appropriate provider. Hardware presence alone must not activate a provider.

```text
Renderer model
     ↓
GPU capability discovery
     ↓
capability profile
     ↓
generic baseline OR specialized path
     ↓
thin native C# interop
     ↓
driver / hardware
```

Vendor-specific capabilities are therefore opportunities, not dependencies. NVIDIA CUDA is one possible compute specialization; other vendors and APIs can expose their own paths.

See [GPU Boundary](GPU_BOUNDARY.md).