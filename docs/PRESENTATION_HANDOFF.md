![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# Renderer → presentation handoff

> Renderer decides what an observer needs to perceive. It does not decide how a host arranges its panels or which UI framework manifests the result.

**Status:** Domain scaffold and responsibility contract. This page maps the intended handoff; it does not claim the full observer-to-frame pipeline is already integrated.

## 1. Responsibility map

~~~mermaid
flowchart LR
    E[Experience owns authoritative state] --> O[Observer context]
    O --> R[Renderer policy]
    R --> F[Renderer-neutral frame snapshot]
    F --> V[Optional Visualize adapter]
    V --> G[GUI panel or panel set]
    G --> H[Host presentation]
~~~

| Stage | Responsibility | Owner | Status |
|---|---|---|---|
| Authoritative world/experience | Identity, state, semantics, domain truth | Experience | External input |
| Observer context | Viewpoint, intent, perception needs, visibility inputs | Experience / observer provider | Contracts exist in part |
| Representation policy | Event Horizon and observer-relative detail/work decisions | Renderer | Implemented foundations; integrated path is incomplete |
| Frame snapshot | Ordered, renderer-neutral draw-command snapshot | Renderer | Public contracts exist |
| Backend execution | Convert frame data into a particular presentation form | Visualize or another adapter | Outside Renderer |
| Panel placement | Choose panel identity, size, docking, arrangement, and host lifecycle | GUI / application host | Outside Renderer |

## 2. Why the boundary matters

A representation decision should survive changes in presentation technology. The same observer result may be inspected in a GUI panel, emitted as an SVG artifact, consumed by a simulation-side semantic observer, or eventually submitted to a GPU backend. These are different consumers of domain information, not interchangeable rendering engines.

Renderer must not acquire a dependency on Visualize, GUI.Core, GUI.SVG, WPF, browser APIs, Unity, or FSM_COS. Composition belongs above the package boundary.

**Semantic knowledge is not pixels.** Reducing visual detail must not silently remove facts an agent needs. A soldier may need a structured summary of nearby entities even when the visual representation is an impostor or is not drawn at all.

## 3. The panel handoff contract (scaffold)

Renderer should provide a stable representation/frame snapshot. A consumer or adapter may then map that snapshot to one or more panel targets.

**Every projection has a source rectangle.** Treat the source rectangle as explicit presentation input: it identifies the logical GUI/content region being projected, independently of where that content is ultimately displayed. It must not be inferred from a destination panel's dimensions. A host may map the same source rectangle to a screen panel, a window, a texture, or a world-space surface.

Keep these concepts separate:

- **Source rectangle:** the region of logical GUI content to project.
- **Destination surface:** the output surface or panel receiving that region.
- **Projection transform:** the mapping from source coordinates to destination coordinates. A world-space destination may require position, orientation, scale, and perspective; those are destination/projection concerns, not Renderer policy.

The target identity and layout are not Renderer concepts. Renderer should not carry docking rules, tab state, window handles, GUI node IDs, or platform controls. If viewport dimensions or camera matrices affect representation selection, they must be supplied as explicit observer/view inputs rather than inferred from a GUI widget. The source rectangle remains part of the presentation contract even when a destination changes.

The intended handoff should make these questions answerable without binding Renderer to a UI framework:

- Which observer/view produced this snapshot?
- What ordered commands and resource identities does it contain?
- Which representation was selected, and what evidence or policy produced it?
- What source rectangle is being projected, and what view/projection/viewport inputs apply?
- Which destination surface receives the projection, and is its mapping screen-space or world-space?
- Is the snapshot complete, stale, cancelled, or invalid?
- Can the consumer route it to one panel, several panels, a non-visual observer, or a file artifact?

These are design questions for the contract audit, not assertions that every field or status currently exists in the public API.

### Frame availability is not the same as frame contents

An empty `RenderFrame` is a valid snapshot containing zero draw commands. It is not evidence that upstream observation completed successfully, that every required source was available, or that the result is current. Likewise, a missing frame, a cancelled preparation, an invalid result, and a failed source must not be silently converted into an empty frame: those states communicate different things to a consumer.

The current `RenderFrame` contract snapshots an ordered command sequence and rejects a null sequence. It does **not** carry an explicit completeness, freshness, provenance, cancellation, or failure status. Consumers must not infer those guarantees from `Commands.Count`, and producers must not describe this model as providing them. A future end-to-end contract should preserve at least these distinctions:

| Condition | Meaning for the consumer | Current `RenderFrame` support |
|---|---|---|
| Complete, non-empty snapshot | The reported commands are the produced frame contents | Command snapshot only; completeness is not encoded |
| Complete, empty snapshot | The producer intentionally produced no draw commands | Valid empty frame |
| Missing or cancelled output | No frame was delivered for this request | No explicit status |
| Stale output | A frame exists but refers to an older source or observer revision | Not encoded on `RenderFrame` |
| Partial or failed source data | The frame may omit required inputs or be unsuitable for display | Not encoded on `RenderFrame` |

This is a contract gap, not a request to add speculative fields to the public API. Define ownership, revision/provenance rules, and consumer behavior first; then implement and test the smallest API that expresses those semantics.

## 4. Evidence boundaries

**Implemented foundations:** Event Horizon selection and buckets, observer geometry, representation state, semantic anchors, visibility/sensor contracts, and renderer-neutral frame/command models exist in the repository.

**Not yet a single completed runtime:** the entire observer → policy → selected work → frame assembly → presentation path is not yet connected as one production pipeline.

**Not proven by SVG output:** GPU execution, depth testing, general clipping, production lighting, or a performance improvement on a real graphics device.

Use the [living Renderer guide](LIVING_RENDERER.md), [SVG living documentation guide](SVG_LIVING_DOCUMENTATION.md), [GPU boundary](GPU_BOUNDARY.md), and [implementation roadmap](IMPLEMENTATION_ROADMAP.md) for executable evidence and next steps.

## 5. Consumer rule of thumb

If the question is *what must be represented for this observer?*, ask Renderer.

If the question is *how do I convert this frame into GUI nodes, SVG, native controls, or GPU work?*, ask a presentation adapter.

If the question is *which source rectangle should be projected to which destination, and how should that destination be arranged?*, ask the presentation adapter and GUI/application host.

TheSingularityWorkshop
