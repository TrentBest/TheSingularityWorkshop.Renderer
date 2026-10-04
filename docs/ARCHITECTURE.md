# Renderer Architecture

## Goal

The Renderer should become an independent rendering computation layer for The Singularity Workshop.

It should describe and schedule representations without making the core dependent on a particular graphics platform.

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

## Workshop relationships

### FSM_API

FSM_API is a possible foundational dependency for stateful renderer processing. Renderer should consume it only where that improves the architecture; it should not reproduce an unrelated state abstraction.

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