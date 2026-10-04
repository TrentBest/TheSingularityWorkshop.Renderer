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

Rendering capabilities can eventually be delivered as independent microbundles.

A renderer capability should be useful independently where practical.

### GPU boundary

The Renderer produces a platform-neutral `RenderFrame` after observer-relative decisions have been made. Its `RenderView` carries observer/view state and its `RenderDrawCommand` values describe resource identity, transforms, and draw ranges.

A host/backend translates that frame into a concrete graphics API. This is the boundary at which DirectX, Vulkan, WebGPU, OpenGL, or another backend may enter the system.

The core Renderer must remain usable and testable without any of those APIs.

See [GPU Boundary](GPU_BOUNDARY.md).

### Hosts

AnyApp, Web, Unity integration, VR environments, or future hosts can adapt Renderer output to their presentation technology.

The host should not redefine the Renderer model.

## Architectural rule

> **The Renderer decides what must be represented. The host decides how that representation is presented.**

That separation is one of the most important constraints of the project.

## Hardware-first execution

The platform-neutral core is not intended to collapse hardware into one generic lowest-common-denominator path. The next layer discovers the physical adapter, records its capabilities, and selects the strongest safe Workshop execution strategy.

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