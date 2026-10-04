# GPU Boundary

The Renderer now approaches the GPU from the correct side of the boundary.

The CPU/FSM side decides **what the observer needs to see, at what computational detail, and therefore what work is justified**. The GPU receives the resulting presentation work.

## Boundary

```text
World / Experience
       |
       v
Entity + Observer
       |
       v
Event Horizon Policy
       |
       v
Representation Selection
       |
       v
CPU / FSM Evaluation
       |
       v
+-----------------------------+
| Platform-neutral GPU input  |
|                             |
| RenderView                  |
| RenderDrawCommand[]         |
| resource identifiers        |
| transforms                  |
| draw ranges                 |
+-----------------------------+
       |
       v
Graphics / Platform Adapter
       |
       +--> DirectX
       +--> Vulkan
       +--> WebGPU
       +--> OpenGL
       +--> future backend
       |
       v
GPU
```

The Renderer does not create DirectX, Vulkan, WebGPU, OpenGL, WPF, or Unity objects at this boundary. It produces a compact description of the work that survived observer-relative computation.

## What the CPU decides

Before a command reaches a graphics adapter, the Renderer should decide:

- whether the entity needs a visual representation at all
- which Event Horizon applies
- which representation is appropriate
- which geometry resource is required
- which material resource is required
- which transform is required
- which portion of that geometry is required
- whether the representation is resident
- how frequently the representation must be updated
- whether promotion or demotion is required

This is where the Workshop's computational advantage is supposed to occur.

## What the GPU adapter decides

The platform adapter translates the neutral command into the native graphics system:

- resource handles and descriptor/binding models
- vertex/index buffer bindings
- shader or pipeline selection
- command-buffer construction
- synchronization
- presentation
- GPU-specific batching
- API-specific barriers and state

The adapter may optimize submission, but it must not silently reintroduce high-detail world evaluation that the Renderer intentionally avoided.

## First GPU-facing contract

`RenderView` carries observer view and projection state.

`RenderDrawCommand` carries the minimum information needed to describe a draw without naming a graphics API:

- geometry resource identity
- material resource identity
- world transform
- index range
- base vertex
- selected computational representation

`RenderFrame` is the resulting command snapshot.

This is intentionally not a GPU API abstraction. It is a **GPU input contract**.

## Why this matters

A renderer can be GPU-bound and still be architecturally CPU-wrong. If the CPU generates a full-fidelity command for every entity before the GPU sees the workload, the GPU becomes an expensive place to discover that most of the work was unnecessary.

Our intended pipeline is:

```text
world population
    |
    v
observable consequence
    |
    v
required computational detail
    |
    v
render representation
    |
    v
GPU work
```

## Next experiments

The benchmark lab should now measure:

1. CPU cost of constructing commands.
2. Memory cost of command snapshots.
3. command count versus world population.
4. command count versus observer-visible population.
5. command generation across Event Horizons.
6. batching/grouping cost.
7. promotion and demotion cost.
8. conversion of neutral commands into one concrete backend.
9. CPU submission cost versus GPU execution time.

The first backend should be selected to **prove that the Renderer can feed a real GPU**, not to define what the Renderer is.
