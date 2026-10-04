# Rendering Model

The Renderer is being designed around **observable representations** rather than around permanent drawable objects.

## Entity versus representation

An entity is what exists in the modeled world.

A representation is how much of that entity needs to be computationally present for a particular observer.

One entity may therefore have several representations:

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

## Observer-relative rendering

Rendering is observer-relative.

The same world can produce different representations for:

- a nearby first-person observer
- a distant observer
- a spectator camera
- a map view
- a VR observer
- an AI observer
- a networked client

This makes the Observer Context a first-class part of the rendering model.

## Representation selection

A future renderer may evaluate something conceptually similar to:

```
Representation =
    Policy(
        Entity,
        Observer,
        SpatialContext,
        SemanticImportance,
        Visibility,
        Budget,
        ExperienceRules)
```

This is a conceptual model, not an API commitment.

## Representation lifecycle

A representation should have an explicit lifecycle:

```
Absent
  |
  v
Requested
  |
  v
Loading
  |
  v
Available
  |
  v
Active
  |
  v
Demoting
  |
  v
Released
```

This becomes important when representations are streamed, cached, generated, or GPU-backed.

## The renderer should be allowed to say "not now"

An entity can exist without being fully represented.

That is not a failure.

It is an intentional computational decision based on the observer, current horizon, available resources, and experience requirements.

## Future implementation question

The first useful implementation should prove representation selection independently of a GPU.

If the model cannot select the correct representation without a graphics API, the architecture is too tightly coupled to presentation.
