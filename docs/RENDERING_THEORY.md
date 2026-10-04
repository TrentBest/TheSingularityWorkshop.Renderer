# Rendering Theory

## Why this Renderer exists

A conventional rendering pipeline begins with a representation and asks a graphics system to draw it.

This project starts one level earlier:

> **What representation should exist at all?**

That distinction is the foundation of TheSingularityWorkshop.Renderer.

A GPU is extremely good at processing the work we give it. It does not know whether that work was computationally justified.

If an object is a kilometer away, hidden behind another object, irrelevant to the observer, or only meaningful as a symbol on a map, rendering its full near-field representation may be wasted computation.

The Renderer therefore treats **representation selection as a first-class computational problem**.

---

## Rendering is not the same thing as drawing

It is useful to separate several questions:

1. **Does the entity exist?**
2. **Does this observer need to know about it?**
3. **What does the observer need to know?**
4. **How accurately must that knowledge be maintained?**
5. **What representation expresses that knowledge?**
6. **How often must that representation be evaluated?**
7. **How is the resulting representation presented?**

Traditional graphics pipelines concentrate heavily on the last question.

This Renderer is primarily concerned with questions 2–6.

---

## The observation chain

A useful mental model is:

```
World
  |
  v
Entity
  |
  v
Observable state
  |
  v
Observer context
  |
  v
Policy
  |
  v
Representation
  |
  v
Evaluation schedule
  |
  v
Presentation
```

The important insight is that **presentation is downstream of knowledge**.

The Renderer does not need to fully understand every entity merely because the entity exists.

---

## Representation is a computational contract

A representation is not merely a mesh.

It can define a contract such as:

- which properties are required
- which properties may be approximated
- which properties are omitted
- how frequently the representation is evaluated
- whether interaction is possible
- whether simulation participates
- what assets must be resident
- what events can promote the representation

For example, a distant city may be represented by:

```
City
  ├── semantic identity
  ├── approximate footprint
  ├── population category
  └── landmark visibility
```

A nearby building may instead require:

```
Building
  ├── structure
  ├── materials
  ├── openings
  ├── occupants
  ├── interaction surfaces
  └── dynamic lighting
```

Both representations describe the same entity.

They simply answer different observational questions.

---

## Detail is a budget

Every additional detail has a computational cost.

The cost may appear as:

- CPU evaluation
- memory residency
- GPU submission
- GPU shading
- animation evaluation
- physics
- network traffic
- storage access
- asset streaming
- synchronization

Therefore detail should be treated as a budgeted resource.

A representation that provides ten times the visual detail is not automatically ten times more valuable.

The observer may not be capable of benefiting from that additional information.

This motivates the project's central rule:

> **Do not spend computation on a representation whose observer cannot benefit from the result.**

---

## Why Event Horizons are broader than LOD

LOD generally answers:

> Which geometric level should be drawn?

An Event Horizon asks:

> **Which computational representation of this entity is justified under the current observation conditions?**

That can change geometry, but it can also change:

- simulation
- update rate
- material complexity
- animation
- interaction
- semantic richness
- asset residency
- network synchronization

A horizon transition is therefore a change in computational responsibility, not merely a change in polygon count.

---

## Time is part of rendering

A scene is not only spatial.

It is temporal.

An entity that does not need evaluation every frame should not necessarily be evaluated every frame.

For a slowly changing distant object, repeated evaluation may produce almost no new observable information.

That means the Renderer should eventually reason about:

```
Spatial relevance
+
Temporal relevance
+
Semantic relevance
+
Interaction relevance
=
Required computational detail
```

This is one of the major differences between the Renderer and a conventional draw-list abstraction.

---

## The renderer can reduce work before the GPU

Consider two worlds containing 100,000 entities.

A traditional approach might eventually ask:

```
"What can the GPU draw efficiently?"
```

The Renderer should ask first:

```
"How many of these entities actually require detailed evaluation?"
```

If only 2,000 are relevant to the observer at high fidelity, the other 98,000 should not be allowed to silently become high-detail work merely because the GPU can process them.

The optimization therefore occurs upstream.

---

## Multiple observers

There may be many observers of the same world:

- a first-person player
- another player
- a spectator camera
- a minimap
- a VR eye
- an AI agent
- a remote network client

There is no universal "correct" representation.

The same entity may simultaneously have:

```
Observer A -> Interactive
Observer B -> Near
Observer C -> Distant
Observer D -> Semantic
```

This makes observation an input to rendering rather than an afterthought.

---

## The deeper goal

The goal is not merely to render faster.

The goal is to establish a system in which **computational effort follows observable consequence**.

If changing a hidden object's material cannot affect the observer, that computation has no immediate observational value.

If a nearby interactive object changes state, the representation must respond quickly.

This gives us a principled path toward performance rather than a collection of backend-specific tricks.
