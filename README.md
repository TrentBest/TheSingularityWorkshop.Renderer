# TheSingularityWorkshop.Renderer

> **A rendering stack designed around computational detail, observation, and Event Horizons.**

TheSingularityWorkshop.Renderer is the beginning of an independent rendering stack for **The Singularity Workshop**.

This project is intentionally starting with the rendering model before committing to a graphics API.

The core question is not:

> Which graphics engine should we wrap?

It is:

> **What must computationally exist for an observer to perceive and interact with the world we intend to present?**

That leads to the central Renderer concept: **Event Horizons**.

---

## Event Horizons

An **Event Horizon** is a spatial or contextual boundary at which the Renderer changes the computational representation of an observable entity.

This goes beyond conventional Level of Detail (LOD).

Crossing an Event Horizon may change:

- geometry detail
- material detail
- animation fidelity
- update frequency
- simulation participation
- interaction availability
- semantic evaluation
- streaming requirements
- representation type

An Event Horizon is therefore a boundary of **computational detail**, not merely visual resolution.

### The basic model

```
Observer
   |
   v
+--------------------------+
| Event Horizon 0          |
| immediate / highest      |
| computational detail     |
+--------------------------+
   |
+--------------------------+
| Event Horizon 1          |
| near field               |
+--------------------------+
   |
+--------------------------+
| Event Horizon 2          |
| contextual field         |
+--------------------------+
   |
+--------------------------+
| Event Horizon 3          |
| distant / aggregate      |
+--------------------------+
   |
+--------------------------+
| Event Horizon 4          |
| semantic existence       |
+--------------------------+
```

The number of horizons and their thresholds are **policy**, not fixed Renderer constants.

---

## Render less by understanding less

The fundamental hypothesis of this project is:

> **Do not spend computation on a representation whose observer cannot benefit from the result.**

A distant entity may not need:

- continuous animation
- detailed geometry
- detailed materials
- collision
- interaction
- high-frequency simulation
- per-frame evaluation

It still exists.

The Renderer simply chooses a representation appropriate to the observer and current conditions.

This means performance can improve **before** work reaches the graphics API.

---

## Detail is multidimensional

Traditional LOD tends to reduce detail along one visual axis.

The Renderer treats detail as multidimensional:

| Dimension | Near observer | Far from observer |
| --- | --- | --- |
| Geometry | detailed | simplified / aggregate |
| Materials | detailed | simplified |
| Animation | continuous | reduced / sampled |
| Update rate | high | low / event-driven |
| Simulation | active | reduced / aggregated |
| Interaction | available | coarse / unavailable |
| Semantics | detailed | summarized |
| Streaming | resident | deferred |

An Event Horizon can change one dimension, several dimensions, or all of them.

Distance is important, but it is only one possible signal. Visibility, observer intent, semantic importance, camera motion, interaction state, computational budget, and experience rules can also affect representation.

---

## Architecture

The conceptual pipeline is:

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

The Renderer owns the observer-aware computational model.

The platform adapter owns presentation-specific details.

### The Renderer should not be a graphics API wrapper

The core is intended to remain independent of:

- Unity
- WPF
- DirectX
- Vulkan
- OpenGL
- WebGPU
- any other particular graphics API or engine

Those technologies may become hosts or adapters.

> **The Renderer decides what must be represented. The host decides how that representation is presented.**

---

## Relationship to The Singularity Workshop

The Renderer is intended to fit into the Workshop without becoming the composition operating system.

| System | Relationship |
| --- | --- |
| **FSM_API** | foundational state abstraction where useful |
| **Ontology** | semantic structure and relationships |
| **Micro Bundles** | independent rendering capabilities and representations |
| **FSM_COS** | composition and runtime assembly |
| **Experiences** | world/experience configuration |
| **AnyApp / Web / MyVR** | host and presentation environments |

The dependency direction matters.

**Renderer must not depend on FSM_COS merely to exist.**

FSM_COS may assemble Renderer capabilities into a runtime. Renderer should remain independently understandable, testable, and usable.

---

## Observable representations

The Renderer is being designed around **representations**, not permanent drawable objects.

One entity can have several representations:

```
Entity: Tree
 |
 +-- Identity / semantic representation
 +-- Environmental representation
 +-- Distant representation
 +-- Near representation
 +-- Interactive representation
```

The entity does not become a different entity when its representation changes.

The observer determines which representation is computationally justified.

This also makes the model useful for different observers:

- nearby first-person observer
- distant camera
- spectator
- map view
- VR observer
- AI observer
- networked client

---

## Event Horizon update frequency

Event Horizons also control **when computation needs to occur**.

A representation might be:

- frame-driven
- fixed-rate
- sampled
- event-driven
- demand-driven
- dormant until promoted

Illustrative policies might eventually look like:

| Horizon | Example |
| --- | --- |
| 0 | 60–240+ evaluations/sec |
| 1 | 30–60 evaluations/sec |
| 2 | 5–30 evaluations/sec |
| 3 | ~1 evaluation/sec |
| 4 | event-driven |

These are experimental values, **not Renderer defaults**.

The architecture should allow an experience to choose the policy appropriate to its workload.

---

## Documentation

Start here:

- [Start Here](docs/START_HERE.md)
- [Architecture](docs/ARCHITECTURE.md)
- [Event Horizons](docs/EVENT_HORIZONS.md)
- [Rendering Model](docs/RENDERING_MODEL.md)
- [Representation](docs/REPRESENTATION.md)
- [Update Frequency](docs/UPDATE_FREQUENCY.md)
- [FSM Integration](docs/FSM_INTEGRATION.md)

The documentation is deliberately being established before the graphics implementation so the semantic model remains independent of backend technology.

---

## Development direction

### Phase 1 — Define the model

- Event Horizon
- Observer Context
- Representation
- Representation selection
- Update policy
- Promotion and demotion
- Temporal stability

### Phase 2 — Prove it without a GPU

Build the smallest testable model that can:

1. create observable entities
2. create an observer
3. determine Event Horizon membership
4. select a representation
5. schedule representation updates
6. promote and demote representations
7. measure avoided work

### Phase 3 — Add rendering backends

Only after the computational model is proven should the project investigate concrete rendering backends.

### Phase 4 — Integrate with Workshop experiences

Connect the Renderer to Micro Bundles, Ontology, FSM_API, FSM_COS composition, and host environments while preserving dependency direction.

---

## Current status

**Architecture discovery / documentation-first**

The repository is intentionally a clean starting point.

The goal is not to build another graphics wrapper.

The goal is to build a rendering system in which **computational detail follows observation**.

---

## License

MIT
