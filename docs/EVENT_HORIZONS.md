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

## Horizon cadence is independent of display cadence

The display may present at 60, 120, or more frames per second while some world representations are evaluated far less often. **A new display frame is not a command to recompute every Event Horizon.** The Renderer should identify which horizon representations are due, which remain valid, and which changes require targeted invalidation.

For example, an experience might initially test a policy that refreshes environmental content at and beyond 1 km about once per minute. This is a workload hypothesis, not a default. The actual interval must reflect motion, visibility, lighting, semantic importance, and the experience's acceptable error budget. An event such as a major explosion, a changed skyline, a light-source change, or a newly relevant object may trigger an earlier update.

The Renderer should expose enough information for downstream presentation to distinguish at least:

- **representation selection changed** — a different representation is now appropriate;
- **source content changed** — geometry, instance data, material inputs, or world state changed;
- **presentation cache invalidated** — an existing rendered result can no longer be reused as-is;
- **refresh is due** — the freshness budget elapsed even if no explicit source change was reported;
- **still valid** — no recomputation is required merely because another display frame is being presented.

These are related but different events. A geometry buffer can remain valid while its lighting result becomes stale; a far-field image can remain valid while a near-field representation changes; and a display frame can be composed from both cached and freshly rendered results.

## The visible world extends beyond the focus

The focus is where high-fidelity work is most justified, not a clipping boundary for the world. Renderer should continue selecting suitable representations beyond the immediate focus so the observer sees a coherent environment. Distant terrain, skyline, architecture, vegetation, and other environmental context may use simplified geometry, aggregate representations, impostors, or cached imagery.

When the observer moves, the presentation layer may shift, reproject, or otherwise reuse cached far-field output rather than regenerate it. The reuse method must match the representation:

- a direction-based background can often be reused during translation, but must respond to orientation;
- a distant world-space representation must preserve appropriate parallax;
- a cached image can be reprojected only within the depth and coverage information it actually contains;
- newly exposed regions and disocclusions need new data or a fallback representation;
- moving objects, changed lighting, and resource revisions invalidate only the dependent results where dependency tracking allows it.

A flat image shifted without depth or world-space context is not a universal solution: it can produce incorrect parallax or reveal empty regions. The objective is to **reuse the largest correct portion of the background**, not to force every horizon into one cache format.

## Separate geometry, lighting, and expensive effects where useful

The implementation should not assume every stage has the same refresh cadence. Geometry/visibility, relevant-light selection, direct lighting, shadowing, material shading, and final composition have different dependencies and costs. A possible pipeline first determines visible geometry and the relevant light set, then evaluates the more expensive shadow and shading work only where the inputs or freshness policy require it. This is a design hypothesis to benchmark, not a mandate to adopt one rendering technique.

For example, unchanged distant geometry may remain resident while a shadow result is refreshed because a light moved; conversely, a static environment may reuse both geometry and lighting output until an invalidating event or freshness deadline occurs. Cache keys and invalidation rules must account for actual dependencies rather than treating an entire horizon as one indivisible object.

> **Display cadence, world-evaluation cadence, geometry cadence, and lighting/effect cadence are separate policy decisions.**
