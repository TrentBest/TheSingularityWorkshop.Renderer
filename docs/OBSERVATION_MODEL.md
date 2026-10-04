# Observation Model

## The observer is a first-class input

Rendering is commonly described as:

```
World -> Camera -> Draw
```

The Renderer needs a richer model:

```
World
  |
  +--> Entity state
  |
  +--> Observer context
          |
          +--> visibility
          +--> position
          +--> orientation
          +--> intent
          +--> interaction
          +--> motion
          +--> budget
          +--> experience rules
```

The observer determines what information is useful.

---

## Distance is a signal, not a definition

The first experimental implementation uses distance because it is simple and testable.

It must not become the architecture.

Two objects at the same distance can have completely different computational requirements.

For example:

- a distant landmark may be extremely important;
- a distant blade of grass may be irrelevant;
- a distant warning light may require high temporal fidelity;
- a distant building may only need a silhouette;
- a distant network participant may need semantic state even when visually tiny.

Therefore:

> **Distance can influence an Event Horizon. It should not define the Event Horizon.**

---

## Observer context

An eventual observer context can contain signals such as:

### Spatial

- position
- orientation
- field of view
- view volume
- distance
- angular size

### Temporal

- observer velocity
- predicted view
- time since last evaluation
- expected visibility duration

### Semantic

- importance
- category
- task relevance
- narrative significance
- relationship to current objective

### Interaction

- selected entity
- proximity
- active interaction
- input focus
- manipulation state

### Computational

- CPU budget
- GPU budget
- memory pressure
- streaming pressure
- network budget

The Renderer should be able to combine these signals without making any one of them mandatory in every host.

---

## Visibility is not binary

An entity can be:

- fully visible
- partially visible
- occluded
- outside the current view
- predicted to enter the view
- visible only through a reflection
- visible only through a shadow
- represented indirectly by another object

This matters because visibility can change what information is worth computing.

A completely occluded entity may need no visual update.

A hidden entity that is about to emerge may deserve pre-promotion.

A shadow-casting entity may require a different representation from an entity that is only visible directly.

---

## Angular size matters

Distance alone can be misleading.

A huge mountain and a tiny coin can be at the same distance while occupying radically different portions of the observer's view.

A useful future concept is **screen-space or angular significance**.

Conceptually:

```
importance ~ projected size × semantic importance × observer relevance
```

This is not a final formula. It is a reminder that the observer perceives projected consequence, not raw world-space distance.

---

## Observer intent

The observer may tell the Renderer what matters.

If the user is looking directly at an object, that object may deserve promotion.

If the user is navigating quickly through a large environment, distant scenery may deserve lower update rates.

If the user is editing a component, the selected object may remain highly detailed even when the camera moves away.

The same physical world can therefore produce different computational states depending on intent.

---

## Budget is part of the model

A Renderer without a budget eventually becomes a system that attempts to satisfy every request.

A practical renderer must sometimes choose between desirable computations.

That means policy should be able to express:

```
desired detail
      |
      v
available budget
      |
      v
actual detail
```

This is not a license for arbitrary degradation.

The important principle is that degradation should be **intentional, measurable, and policy-driven**.

---

## Multiple observers

A world should not have one global representation selected for everyone.

Instead:

```
World
 |
 +--> Observer A -> Representation A
 |
 +--> Observer B -> Representation B
 |
 +--> Observer C -> Representation C
```

Some underlying computation may be shared.

Some may not.

The architecture should leave that optimization open rather than forcing it prematurely.

---

## What this means for the API

The API should eventually make it possible to ask:

> Given this entity, this observer, this policy, and this budget, what representation is justified and when should it next be evaluated?

That is a much stronger question than:

> Which mesh should I draw?

The current distance-based primitive is deliberately only the first stepping stone toward this model.
