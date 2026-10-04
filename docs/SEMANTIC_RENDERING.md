# Semantic Rendering

The Renderer should know enough about an entity to decide what information must exist for an observer without owning the vocabulary that gives that information meaning.

```text
ProtocolAi -> integer-backed semantic identity -> Renderer
                                      |
                                      +-- spatial anchor
                                      +-- representation policy
                                      +-- Event Horizon
                                      +-- update frequency
                                      +-- presentation
```

The Renderer therefore does not need a hard-coded list of body parts, garment types, equipment sockets, architectural surfaces, or other domain vocabulary. A consuming experience can define those terms in a ProtocolAi vocabulary.

## Semantic anchors

A SemanticAnchor associates a ProtocolReference with a normalized local position.

A form-specific protocol could define Head, Torso, Hand.Left, Hand.Right, garment attachment points, equipment sockets, or any other domain concepts. Renderer does not need to know what those symbols mean; it only needs their protocol-qualified identity and spatial relationship.

This makes the same mechanism usable for anatomy, clothing and covering attachment, equipment sockets, tools, architectural components, vehicle components, UI forms, interaction surfaces, procedural generation anchors, accessibility descriptions, and AI semantic observations.

## Forms

A SemanticForm is a collection of unique protocol-qualified anchors. A form is therefore not necessarily a mesh. It can be a semantic coordinate system from which multiple representations are derived.

```text
Semantic Form
   |
   +--> near representation
   +--> distant representation
   +--> interaction representation
   +--> semantic observation
   +--> procedural reconstruction
```

This is especially important for procedural rendering. A representation can know where something belongs without requiring the highest-detail geometry to remain materialized.

## Why ProtocolAi belongs here

ProtocolAi supplies deterministic integer-backed vocabulary identity.

The Renderer supplies spatial and computational interpretation.

- ProtocolAi answers what symbol is this?
- Ontology can answer what does this thing mean in the domain?
- Renderer answers where and at what computational detail must it exist for this observer?
- FSM_API answers when should the computation occur?
- FSM_COS may assemble the resulting capabilities.

No part of this requires Renderer to become the owner of domain semantics.

## Observer-aware semantic detail

Semantic information can itself have Event Horizons. A nearby interactive entity may expose identity, component relationships, interaction anchors, and fine semantic detail. A distant entity might expose only identity, category, and coarse location.

An AI observer may require semantic detail without requiring visual detail.

```text
visual detail
semantic detail
interaction detail
simulation detail
update frequency
```

They do not have to move together.

## Design principle

> Use protocol identity to name semantic things; use Renderer policy to decide which of those things must computationally exist for an observer.

This is the Event Horizon principle applied to meaning as well as geometry.