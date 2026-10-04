# Renderer — Start Here

TheSingularityWorkshop.Renderer is the beginning of a rendering stack designed around The Singularity Workshop's computational model rather than around a particular graphics API.

The first question is not **which GPU API should we wrap?**

The first question is:

> **What must exist, and at what computational detail, for an observer to perceive the world we intend to render?**

That leads to the central Renderer concept: **Event Horizons**.

## Event Horizons

An Event Horizon is a spatial or contextual boundary at which the renderer changes the computational representation of an observable entity.

Crossing an Event Horizon may change:

- geometric detail
- material detail
- animation fidelity
- update frequency
- simulation participation
- interaction availability
- semantic evaluation
- streaming requirements
- representation type

An Event Horizon is therefore more than conventional visual LOD. It is a boundary of **computational detail**.

## The initial mental model

```
Observer
   |
   v
+--------------------+
| Event Horizon 0    |  immediate / highest detail
+--------------------+
   |
+--------------------+
| Event Horizon 1    |  near field
+--------------------+
   |
+--------------------+
| Event Horizon 2    |  contextual field
+--------------------+
   |
+--------------------+
| Event Horizon 3    |  distant / aggregate
+--------------------+
   |
+--------------------+
| Event Horizon 4    |  existence / semantic presence
+--------------------+
```

The numbers are illustrative, not a fixed implementation contract.

## Renderer is not an engine wrapper

The core Renderer should remain independent of Unity, WPF, DirectX, Vulkan, WebGPU, OpenGL, or any other presentation technology.

Those technologies may become **hosts or adapters**.

The Renderer owns the model that decides what should be represented. A platform adapter owns how that representation is ultimately manifested.

## Relationship to the Workshop

The Renderer is intended to fit into the larger Workshop without becoming the composition operating system.

- **FSM_API** provides the foundational state abstraction.
- **Ontology** provides semantic structure and relationships.
- **Micro Bundles** can provide rendering capabilities and representations.
- **FSM_COS** can compose Renderer capabilities into an experience.
- **Experiences** describe what is present and how it is configured.
- **AnyApp / Web / MyVR** can provide hosts or presentation environments.

The dependency direction must remain downward and compositional. Renderer must not depend on FSM_COS merely to exist.

## Documentation

- [Architecture](ARCHITECTURE.md)
- [Event Horizons](EVENT_HORIZONS.md)
- [Rendering Model](RENDERING_MODEL.md)
- [Representation](REPRESENTATION.md)
- [Update Frequency](UPDATE_FREQUENCY.md)
- [FSM Integration](FSM_INTEGRATION.md)

## Current status

**Alpha / computational model established**

The Renderer now has executable foundations for Event Horizon selection, observer-relative projection/parallax, semantic anchors, and FSM-backed computational representation state. The published package is **0.1.0-alpha.2**.

The benchmark laboratory has also produced the first measured calibration data and identified Event Horizon selection allocation as a concrete optimization target.

The next implementation milestones should continue to prove the model with small, measurable abstractions before selecting or building a graphics backend.
