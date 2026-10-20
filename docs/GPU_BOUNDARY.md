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

The current living SVG sample proves the earlier FSM-backed representation-state seam. It does **not** yet visualize actual `RenderFrame`/`RenderDrawCommand` instances. That is the next useful visualization boundary: show the selected representation beside the platform-neutral command it produces, then connect the command to a real backend.

![Current FSM-backed representation-state proof](assets/living-event-horizons.svg)

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


## The Workshop-native GPU stack

The intent is stronger than building another graphics-API wrapper.

We should own the rendering decisions, resource model, command representation, batching rules, material semantics, shader inputs, and GPU scheduling policy. A native graphics API should enter only at the final transport/execution boundary because the operating system and GPU driver necessarily own that hardware interface.

```text
                 THE SINGULARITY WORKSHOP

World / Observer / Event Horizons
              |
              v
      Renderer computation
              |
              v
     Workshop Render Model
              |
       +------+------+
       |             |
       v             v
  resources      command stream
       |             |
       +------+------+
              |
              v
       Workshop GPU Layer
              |
              v
     tiny native transport
              |
              v
             GPU
```

The **GPU Layer is ours**. A platform API is merely the last-mile mechanism required to submit bytes and commands to actual hardware.

That distinction keeps the architecture honest: we are not pretending a DirectX or Vulkan object is our renderer. We are using the platform's hardware doorway to execute a rendering system that we designed.

## What we should own

The Workshop should eventually define and measure its own:

- resource identity and lifetime
- geometry representation
- material representation
- semantic-to-visual mapping
- instance representation
- command stream format
- batching rules
- visibility decisions
- Event Horizon transitions
- update cadence
- procedural reconstruction
- shader input contract
- GPU memory residency policy
- synchronization policy
- render scheduling

A concrete API backend should implement only the unavoidable translation into the operating system's graphics/hardware interface.

## First concrete GPU milestone

The first real GPU experiment should therefore be deliberately small:

1. produce one `RenderFrame`
2. translate it into a Workshop-owned GPU packet
3. upload the packet and its minimum resources
4. execute one draw on real GPU hardware
5. measure CPU preparation, transfer, GPU execution, and presentation independently
6. keep the graphics API-specific code outside the Renderer core

The first image does not need to be impressive. It needs to prove that **our computational model can reach real GPU execution without surrendering ownership of the rendering architecture**.

## What this does not mean

We do not need to write a GPU driver, replace the operating system graphics stack, or reproduce decades of hardware-specific driver work.

We own the renderer. The driver owns the hardware.

That is the useful boundary.

## Hardware discovery and capability specialization

The next layer is **host compatibility and runtime capability discovery**. Identify the execution environment first, initialize a compatible implementation, then query the actual adapter/device/runtime. Do not infer a browser's underlying API from its brand or choose a path from GPU vendor identity.

```text
host compatibility
    ↓
available backend implementations
    ↓
actual runtime/device initialization
    ↓
capabilities + limits + constraints
    ↓
required-feature fit / optional extensions
    ↓
explicit execution path + diagnostics
```

Vendor, architecture, driver, and device identifiers are useful diagnostic and benchmark metadata. They are not the primary backend-selection rule. A generic baseline remains necessary but must not become the ceiling: capability discovery is additive, and explicitly requested extensions may use the features a runtime actually exposes. Missing required features must fail clearly or select another compatible path; optional paths and fallbacks must be visible. This applies equally to native and browser hosts, whose available APIs and lifecycle rules differ.

## C# should reach as close to the metal as practical

The first implementation should remain C# rather than forcing a managed rendering abstraction to hide the hardware.

Where the operating system or driver exposes a native interface, the Workshop can use C# interop directly:

- P/Invoke where the native ABI is the appropriate boundary
- function pointers and unmanaged delegates where appropriate
- `unsafe` code for explicitly controlled native-memory operations
- `NativeMemory` for unmanaged allocations
- blittable structs for direct data exchange
- spans over unmanaged memory where safe and useful
- explicit lifetime ownership for native resources
- direct inspection of adapter/device capabilities
- vendor-specific paths behind capability checks
- CUDA or another compute interface when a device exposes useful compute capability

The goal is not to pretend native APIs are managed objects. The goal is to make the managed boundary extremely thin and keep the Workshop representation on our side of it.

## CUDA is a capability, not the renderer

NVIDIA CUDA should be treated as an important example of specialization rather than as the generic rendering architecture.

Where a runtime exposes CUDA and a measured workload benefits from it, a separately implemented compute extension may use that capability. The decision is based on runtime capability and workload evidence, not on NVIDIA identity alone. Renderer core must not take a CUDA dependency.

```text
Workshop GPU model
       |
       +--> generic GPU execution
       |
       +--> NVIDIA capability path
       |       +--> CUDA compute
       |       +--> future NVIDIA-specific features
       |
       +--> AMD capability path
       |
       +--> Intel capability path
       |
       +--> other hardware path
```

This prevents the common-denominator trap without making NVIDIA a hard dependency.

## Driver versus renderer

We should distinguish three different layers:

1. **Workshop renderer** — the algorithms, representations, command model, resource model, scheduling, and GPU strategy we own.
2. **Native transport layer** — the smallest C# interop layer required to communicate with the operating system/driver interface.
3. **GPU driver/firmware** — the vendor software and hardware control we do not need to replace to obtain direct execution.

Writing a completely new production GPU driver is a different project with operating-system kernel, device-protocol, firmware, certification, and hardware-documentation requirements. It is not required to get the Renderer extremely close to the hardware.

But we should absolutely investigate how thin our native transport can become, what hardware information is available, and how much of the GPU's exposed capability we can use from C#.

That investigation is now part of the Renderer roadmap.

## Visualize integration contract

Renderer and Visualize are coordinated GPU beneficiaries, with deliberately separate ownership. Renderer can benefit first by reducing CPU work and producing fewer, more relevant commands; Visualize benefits by executing the selected work efficiently. Later, eligible data-parallel Renderer workloads—such as bulk visibility, aggregation, or reconstruction—may be offloaded through a neutral optional compute-provider boundary after a concrete workload proves the need. Do not prematurely add GPU API objects or make Renderer depend on Visualize to achieve this.

The current source integration is split deliberately:

- **Renderer core** owns observer-relative representation selection and emits immutable `RenderFrame` command snapshots. It must not reference Visualize, WPF, Vulkan, Direct3D, WebGPU, or native handles.
- **Visualize core** owns backend-neutral frame/resource contracts and validates command ranges against the resource provider.
- **Renderer.VisualizeAdapter** maps frame fields, command order, transforms, index ranges, base vertex, instance metadata, representation, and entity identity. It is an adapter, not a geometry/material upload system.
- **Visualize backend / host adapter** will own concrete GPU resources and presentation after a backend is selected by evidence.

### Important resource distinction

The adapter currently preserves the numeric values of Renderer geometry/material/instance IDs when creating Visualize IDs. That is a naming convention only: it does **not** transfer resource contents, prove that the corresponding Visualize resources exist, or guarantee matching lifetime/revision semantics. A real end-to-end GPU proof must explicitly populate or resolve Visualize-side resource descriptions and must fail clearly for unresolved IDs.

Before production integration, define and test the bridge from Renderer resource identity and revisions to CPU-side geometry/material/instance descriptions and Visualize resource revisions. Avoid putting GPU handles in either core package, and do not make Renderer depend upward on Visualize to solve this. The bridge belongs in the adapter/integration layer, with ownership and disposal documented by the host.

### Current proof boundary

The current AnyApp `--renderer-proof` view is a WPF diagnostic projection, not GPU output. Visualize's SVG and perspective-mesh proofs validate other layers, not native GPU presentation. The native API choice is still open; the Visualize strategy is to compare candidates against an equivalent array-first workload before extending any specific prototype.

The first cross-repository acceptance test is therefore not just “a frame object converted successfully.” It is: a known Renderer frame resolves to known resource data, is submitted to the selected backend, produces a real presented image, and reports backend/device/runtime identity and capabilities—without adding an upward dependency or leaking platform handles into Renderer.

Use one deterministic array-first workload and observer trace across the CPU/SVG reference, browser WebGPU execution where available, and any later native backend. Renderer must supply actual horizon/bucket invalidation and reuse decisions rather than a sample-owned timer. Visualize must resolve logical IDs to real resource data and report upload/reuse accounting. Compare equivalent work and preserve output assertions; never treat a browser API's existence, a successful build, or a software adapter as proof of physical-GPU performance.
