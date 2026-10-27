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

The target identity and layout are not Renderer concepts. Renderer should not carry docking rules, tab state, window handles, GUI node IDs, or platform controls. If viewport dimensions or camera matrices affect representation selection, they must be supplied as explicit observer/view inputs rather than inferred from a GUI widget.

The intended handoff should make these questions answerable without binding Renderer to a UI framework:

- Which observer/view produced this snapshot?
- What ordered commands and resource identities does it contain?
- Which representation was selected, and what evidence or policy produced it?
- Which view/projection/viewport inputs apply?
- Is the snapshot complete, stale, cancelled, or invalid?
- Can the consumer route it to one panel, several panels, a non-visual observer, or a file artifact?

These are design questions for the contract audit, not assertions that every field or status currently exists in the public API.

## 4. Evidence boundaries

**Implemented foundations:** Event Horizon selection and buckets, observer geometry, representation state, semantic anchors, visibility/sensor contracts, and renderer-neutral frame/command models exist in the repository.

**Not yet a single completed runtime:** the entire observer → policy → selected work → frame assembly → presentation path is not yet connected as one production pipeline.

**Not proven by SVG output:** GPU execution, depth testing, general clipping, production lighting, or a performance improvement on a real graphics device.

Use the [living Renderer guide](LIVING_RENDERER.md), [SVG living documentation guide](SVG_LIVING_DOCUMENTATION.md), [GPU boundary](GPU_BOUNDARY.md), and [implementation roadmap](IMPLEMENTATION_ROADMAP.md) for executable evidence and next steps.

## 5. Consumer rule of thumb

If the question is *what must be represented for this observer?*, ask Renderer.

If the question is *how do I convert this frame into GUI nodes, SVG, native controls, or GPU work?*, ask a presentation adapter.

If the question is *which panel should display it and how should that panel be arranged?*, ask GUI or the application host.

TheSingularityWorkshop
