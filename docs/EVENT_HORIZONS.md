# Event Horizons

## Definition

An **Event Horizon** is a spatial or contextual boundary at which the Renderer changes the computational representation of an observable entity.

The word is intentional.

A conventional LOD system usually asks:

> Which visual asset should I draw?

The Renderer asks a larger question:

> **How much of this entity needs to computationally exist for this observer, right now?**

That distinction is foundational.

## Detail is multidimensional

An entity does not have one scalar "detail level."

A representation may have independent dimensions:

| Dimension | Near observer | Far from observer |
| --- | --- | --- |
| Geometry | full geometry | simplified geometry / aggregate |
| Materials | detailed materials | simplified materials |
| Animation | continuous | reduced / sampled / absent |
| Update rate | high frequency | low frequency / event driven |
| Simulation | fully participating | reduced / aggregated |
| Interaction | available | unavailable or coarse |
| Semantics | detailed | summarized |
| Streaming | resident | deferred / streamed on demand |

An Event Horizon can change one dimension, several dimensions, or all of them.

## Distance is only one input

Distance from the observer is an important signal, but it is not the definition.

Other signals may include:

- visibility
- occlusion
- observer intent
- interaction state
- camera motion
- semantic importance
- object importance
- predicted future visibility
- available computational budget
- network or streaming state
- experience rules

This means two objects at the same distance may legitimately occupy different computational horizons.

## Horizon transitions

A transition should be treated as a change of representation, not merely a mesh swap.

For example, a distant tree might be represented as:

1. semantic existence
2. environmental mass
3. billboard or impostor
4. simplified geometry
5. full interactive tree

As the observer approaches, the Renderer promotes the entity through these representations.

As the observer leaves, it demotes them.

## Hysteresis

A future implementation should avoid rapid oscillation at a boundary.

If an observer repeatedly crosses the same threshold because of small camera movement, the Renderer should not continuously promote and demote the representation.

Hysteresis, prediction, or temporal stability can provide a stable transition policy.

## Event Horizons and computation

The most important consequence is computational.

Farther entities do not necessarily need to be evaluated every frame.

A possible policy might eventually look like:

| Horizon | Example update policy |
| --- | --- |
| 0 | 60–240+ evaluations/sec |
| 1 | 30–60 evaluations/sec |
| 2 | 5–30 evaluations/sec |
| 3 | ~1 evaluation/sec |
| 4 | event-driven |

These values are examples for investigation, **not Renderer constants**.

The architecture should permit the policy to be selected according to the experience and workload.

## Event Horizon principle

> **Render less by understanding less, not merely by drawing less.**

This is the central hypothesis that implementation and benchmarks should eventually test.
