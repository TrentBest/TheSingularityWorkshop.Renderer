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

### Hosts

AnyApp, Web, Unity integration, VR environments, or future hosts can adapt Renderer output to their presentation technology.

The host should not redefine the Renderer model.

## Architectural rule

> **The Renderer decides what must be represented. The host decides how that representation is presented.**

That separation is one of the most important constraints of the project.
