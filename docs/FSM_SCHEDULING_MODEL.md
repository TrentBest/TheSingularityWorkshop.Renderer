# FSM Scheduling Model for Rendering

The Renderer should treat FSM_API as more than a representation-state helper.

FSM_API already provides three mechanisms that map directly onto observer-relative rendering:

1. Processing groups — named execution lanes.
2. Process rates — controlled evaluation frequency.
3. FSM states and transitions — explicit computational responsibility.

That means the Renderer does not need to invent a second scheduler merely to express Event Horizon cadence.

## The rendering interpretation

| Rendering concept | FSM_API mechanism |
| --- | --- |
| LOD / Event Horizon representation | FSM state |
| Evaluation lane | Processing group |
| Evaluation frequency | Process rate |
| Promotion / demotion | State transition |
| Immediate reaction | Event/manual evaluation |
| Dormant representation | Event-driven or manually stepped FSM |
| Entity-specific context | IStateContext |
| Runtime behavior change | FSM modification |

Conceptually:

    LOD0 / immediate      -> highest-frequency computation
    LOD1 / interaction    -> high-frequency computation
    LOD2 / local context  -> moderate computation
    LOD3 / environment    -> low-frequency computation
    ...
    LOD10 / distant       -> very low-frequency or event-driven computation

The important idea is that an LOD number is not itself the scheduler. It is a policy label that can select a computational regime.

## Why this matters

Consider a tree.

At a close Event Horizon, the Renderer may need individual leaves, secondary and tertiary branches, wind response, collision, interaction, and high-frequency animation.

At a distant Event Horizon, the Renderer may only need the trunk, primary branches, silhouette, semantic identity, and occasional environmental change.

The distant representation should not be an inferior reconstruction of the close representation calculated every frame. It should be a cheaper representation generated from the same underlying identity.

## Deterministic reconstruction

A procedural entity can carry a compact seed rather than a permanently materialized description of every detail.

    Entity identity
          +
    World/placement identity
          +
    Procedural seed
          |
          v
    Deterministic representation

The same seed must produce the same structural decisions when the representation is regenerated.

Therefore a tree can expose its primary branches at a distant horizon without determining every leaf. When promoted, the Renderer can deterministically expand the same tree into additional branches, leaves, material variation, animation parameters, and collision detail.

The distant tree is not a different tree. It is a lower-cost observation of the same tree.

### Squirrel3

The Workshop already has documentation for Squirrel3 in the SingularityWarehouse repository. That work describes Squirrel3 as a stateless, position-based high-speed noise/RNG function.

That is exactly the class of mathematical primitive worth investigating here.

The Renderer should not assume Squirrel3 is the final answer yet. The requirement is more fundamental:

> A representation must be reproducible from stable identity and deterministic inputs without requiring the highest-detail representation to remain fully materialized.

A future Renderer procedural system can then benchmark Squirrel3 and alternatives against that requirement.

## Occlusion becomes computational leverage

Suppose the observer is close to Tree A. Tree A is allowed to become extremely detailed because the observer's visual field and interaction budget justify it.

That detailed tree also occupies screen space. It therefore naturally occludes some of the more distant trees behind it. Those distant trees can remain at lower Event Horizons because their additional detail would not produce an observable benefit.

Conceptually:

    observer proximity
          |
          v
    promotion of foreground representation
          |
          v
    greater foreground geometric presence
          |
          v
    occlusion / reduced observability
          |
          v
    less work justified behind it

This should be measured rather than assumed, but it suggests that detail can sometimes pay for itself by reducing the visibility of other detail.

## Event Horizons are computational, temporal, and observational

The Renderer should think of an Event Horizon as changing at least three things.

### 1. What exists computationally

Geometry, animation, collision, simulation, semantics, interaction, streaming data, and procedural expansion can all be reduced.

### 2. How often it is evaluated

FSM_API process rates are directly useful here. A distant mountain does not necessarily need to be evaluated at the same cadence as a fist occupying the observer's field of view.

Some distant state may be updated periodically. Some may be event-driven. Some may remain dormant until an observation or event requires promotion.

### 3. What information is exposed to an observer

The observer does not necessarily need a rendered image to benefit from an observation.

An AI observer might receive:

    10 m left: goblin
    100 m right: dragon
    interaction: goblin reachable
    threat: dragon high

The LLM or AI system should not have to rediscover these facts from pixels if the world model already knows them.

This gives the Renderer a second output concept:

    World
      |
      v
    Observer model
      |
      +------> visual representation
      +------> semantic observation
      +------> interaction affordances
      +------> simulation participation

Rendering is therefore increasingly about constructing the right observable information, not merely drawing it.

## The important FSM_API limitation

There is one architectural distinction we must preserve.

Today, FSM_API associates ProcessingGroup and ProcessRate with the FSM definition/bucket. Instances of that definition share those scheduling characteristics.

That is excellent for homogeneous workloads:

    LOD2 population
        |
        +--> same processing policy
        +--> same cadence

It is not yet sufficient by itself for a heterogeneous population in which every entity independently crosses horizons:

    Tree A -> LOD0
    Tree B -> LOD3
    Tree C -> LOD7
    Tree D -> LOD10

We should not solve that by duplicating the Renderer scheduler.

Instead, this identifies a clean future FSM_API capability to investigate:

> Can an individual FSM instance select or inherit its processing lane and cadence without mutating the shared FSM definition?

If that capability can be added without compromising FSM_API's performance model, the Renderer can express observer-relative scheduling directly through the foundational FSM system.

Until then, the Renderer can still use FSM states for representation and use processing groups/rates for homogeneous scheduling cohorts.

## The larger architecture

    Observer
       |
       +-------------------+
       |                   |
    visual need       semantic need
       |                   |
       v                   v
    Event Horizon    Observation model
       |                   |
       +---------+---------+
                 |
                 v
          FSM computation
                 |
       +---------+---------+
       |         |         |
     state/LOD cadence   events
       |         |         |
       v         v         v
 representation process  promotion
               group
                 |
                 v
             actual work

The Renderer should therefore consume FSM_API as a computational substrate, not wrap it with another scheduler simply because the domain happens to be graphics.

## Design principle

> Do not calculate detail merely because the world contains detail. Calculate detail when an observer can benefit from knowing it.

And for procedural worlds:

> When detail is not justified, retain the information needed to reconstruct it — not the detail itself.

That combination is where Event Horizons, FSM scheduling, deterministic procedural generation, occlusion, and AI observation begin to become one coherent system.