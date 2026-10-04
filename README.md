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

## FSM_API is part of the rendering substrate

The Renderer is deliberately not building a second state/scheduling system beside FSM_API.

FSM_API already provides:

- named processing groups
- process-rate throttling
- event-driven/manual processing
- state transitions
- runtime definition modification
- POCO-friendly state context

Those capabilities map directly onto Event Horizon computation.

Conceptually:

```
LOD0 / immediate       -> highest-frequency computation
LOD1 / interaction     -> high-frequency computation
LOD2 / local context   -> moderate computation
LOD3 / environment     -> low-frequency computation
...
LOD10 / distant        -> very low-frequency or event-driven computation
```

The LOD number is therefore a policy label. The FSM and its scheduler determine computational responsibility.

See [FSM Scheduling Model](docs/FSM_SCHEDULING_MODEL.md) and [FSM Integration](docs/FSM_INTEGRATION.md) for the deeper model.

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
| **FSM_API** | renderer computation and scheduling substrate |
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

A procedural entity may retain a stable identity and deterministic seed while its materialized detail changes dramatically with observation.

This also makes the model useful for different observers:

- nearby first-person observer
- distant camera
- spectator
- map view
- VR observer
- AI observer
- networked client

---

## Semantic rendering and protocol identity

The Renderer also needs a mathematical boundary between **meaning** and **presentation**.

[ProtocolAi](https://github.com/TrentBest/TheSingularityWorkshop.ProtocolAi) provides deterministic integer-backed vocabulary identity. Renderer uses those protocol-qualified symbols as semantic anchors without owning the vocabulary itself.

A form can therefore expose semantic attachment points for anatomy, clothing and covering, equipment, tools, architecture, interaction surfaces, or other domain concepts. The Renderer only needs the protocol reference and spatial relationship; the consuming experience decides what the symbol means.

```text
ProtocolAi symbol
       |
       v
SemanticAnchor
       |
       +--> representation
       +--> interaction
       +--> procedural detail
       +--> semantic observation
```

This lets semantic detail have its own Event Horizons. Visual, semantic, interaction, simulation, and update-frequency detail do not have to move together.

See [Semantic Rendering](docs/SEMANTIC_RENDERING.md).

## Deterministic procedural detail

A distant tree should not require all of its leaves to remain materialized merely so the tree can later become detailed.

The stronger model is:

```
entity identity
      +
placement identity
      +
deterministic seed
      |
      v
representation appropriate to observer
```

When promoted, additional branches, leaves, material variation, animation parameters, and collision detail can be reconstructed from stable inputs.

The same tree remains the same tree.

The Workshop already has Squirrel3 documentation in SingularityWarehouse, making Squirrel3 a candidate mathematical primitive for investigation. The Renderer will benchmark that and alternatives rather than assuming the answer prematurely.

> **When detail is not justified, retain the information needed to reconstruct it — not the detail itself.**

---

## AI is also an observer

The observer does not necessarily need a rendered image.

An AI observer can receive a semantic observation such as:

```
10 m left: goblin
100 m right: dragon
interaction: goblin reachable
threat: dragon high
```

The LLM or AI system should not have to spend computational effort rediscovering facts already available in the world model.

The same observation system can therefore produce:

```
World
  |
  v
Observer model
  |
  +------> visual representation
  +------> semantic observation
  +------> interaction affordances
  +------> simulation participation
```

Rendering is consequently about constructing the right **observable information**, not merely drawing lines.

---

## Rendering mathematics

The Renderer is being engineered as an **estimable computational system**, not a collection of guesses about performance.

The initial performance model is calibrated from Workshop FSM_API benchmark observations: approximately 305.1 ns for one processing group and 15,736.6 ns for fifty groups, giving a first-order marginal estimate of about 314.93 ns per additional group. The same observations indicate approximately 360 bytes per processing group.

That produces a first-order model of:

```text
T(G) ≈ 305.1 + 314.93 * (G - 1) ns
A(G) ≈ 360 * G bytes
```

This is explicitly a **model to test**, not a promise. Renderer benchmarks will compare predicted and measured cost as the implementation grows.

The longer-term cost model treats rendering as observer-relative work:

```text
C_total = selection + scheduler + representation + simulation
           + streaming + submission + GPU
```

and seeks the least expensive representation that preserves the information the observer can benefit from.

See [Rendering Mathematics](docs/RENDERING_MATH.md).

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

## Occlusion can become computational leverage

A highly detailed foreground object can occupy enough screen space to hide portions of the environment behind it.

That creates a potentially useful relationship:

```
foreground promotion
        |
        v
more foreground detail
        |
        v
greater occlusion
        |
        v
less observable detail behind it
        |
        v
less computation justified behind it
```

This must be measured rather than assumed, but it suggests an important direction: detailed nearby geometry can sometimes reduce the amount of distant geometry an observer can benefit from.

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
- [FSM Scheduling Model](docs/FSM_SCHEDULING_MODEL.md)

### Theory

- [Rendering Theory](docs/RENDERING_THEORY.md)
- [Observation Model](docs/OBSERVATION_MODEL.md)
- [Event Horizon Theory](docs/EVENT_HORIZON_THEORY.md)
- [Semantic Rendering](docs/SEMANTIC_RENDERING.md)
- [Rendering Mathematics](docs/RENDERING_MATH.md)
- [Performance Positioning](docs/PERFORMANCE_POSITIONING.md)
- [Parallax and Observer Geometry](docs/PARALLAX_AND_OBSERVER_GEOMETRY.md)

The documentation is deliberately being established before the graphics implementation so the semantic model remains independent of backend technology.

---

## Development direction

### Phase 1 — Define the model

- Event Horizon
- Observer Context
- Representation
- Representation selection through FSM computation
- FSM-backed scheduling
- Update policy
- Promotion and demotion
- Temporal stability
- deterministic procedural reconstruction
- semantic observation
- semantic anchors and protocol identity
- empirical rendering cost model

### Phase 2 — Prove it without a GPU

Build the smallest testable model that can:

1. create observable entities
2. create an observer
3. determine Event Horizon membership
4. select a representation
5. schedule representation updates
6. promote and demote representations
7. reconstruct procedural detail deterministically
8. measure avoided work
9. produce semantic observations alongside visual representations

### Phase 3 — Add rendering backends

Only after the computational model is proven should the project investigate concrete rendering backends.

### Phase 4 — Integrate with Workshop experiences

Connect the Renderer to Micro Bundles, Ontology, FSM_API, FSM_COS composition, and host environments while preserving dependency direction.

---

## Current status

**Architecture discovery / documentation-first**

The repository is intentionally a clean starting point.

The goal is not to build another graphics wrapper.

The Renderer now has its first real computational integration: **FSM_API drives representation state and provides the foundation for renderer scheduling**, while the Renderer remains independent of FSM_COS and graphics APIs.

The goal is to build a rendering system in which **computational detail follows observation**.

---

## License

MIT
