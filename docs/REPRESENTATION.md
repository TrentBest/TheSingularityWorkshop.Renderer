# Representation

## Why representation is a first-class concept

A traditional rendering pipeline often begins with a drawable object.

This Renderer begins one level earlier:

> **What representation of this entity is justified for this observer?**

That lets the Renderer reduce computation before it reaches drawing.

## A representation may contain

- geometry
- material information
- animation state
- lighting participation
- collision or interaction information
- semantic information
- simulation participation
- update policy
- resource requirements

Not every representation needs every component.

## Representation capability

A future representation may be modeled as a capability with requirements and transitions.

Conceptually:

```
Representation
  ├── Identity
  ├── Detail
  ├── Resource Requirements
  ├── Update Policy
  ├── Interaction Policy
  └── Promotion / Demotion Rules
```

The exact interfaces should wait until the model has been exercised.

## Examples

### Near tree

May include:

- complete geometry
- detailed materials
- animation
- interaction
- collision
- frequent state updates

### Distant tree

May include:

- silhouette
- coarse environmental contribution
- infrequent updates
- no interaction

### Very distant forest

May be represented as:

- a single environmental aggregate
- a terrain/biome semantic
- an impostor
- or simply existence in the world model

The renderer is free to choose the representation that satisfies the observer's needs.

## Representation is not destruction

Demoting an entity does not mean deleting the entity from the world.

It means releasing computational detail that the observer does not currently require.

Promotion can recreate or stream the required representation later.

## Design objective

A good Renderer should make computational detail elastic.

The world can remain rich while the active representation remains proportional to what can actually be observed or interacted with.
