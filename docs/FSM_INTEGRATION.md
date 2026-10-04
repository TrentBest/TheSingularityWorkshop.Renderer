# FSM Integration

The Renderer belongs above foundational state mechanics and below application composition.

## Intended relationship

```
             FSM_COS
                |
        composition / assembly
                |
        +-------+-------+
        |               |
   Renderer          other bundles
        |
     FSM_API
```

This diagram describes a possible dependency relationship, not a requirement that every Renderer assembly directly reference FSM_COS.

## Why FSM_API is interesting

FSM_API treats state as an atomic unit of functionality.

That maps naturally to renderer concerns such as:

- horizon membership
- representation activation
- promotion and demotion
- visibility state
- interaction state
- update scheduling
- streaming state

The Renderer now uses FSM_API directly for representation computation. This is not a decorative dependency: an observable entity's representation is an actual FSM state, and promotion/demotion is an actual state transition.

## FSM_API is also the scheduling substrate

The Renderer should not invent a second scheduler merely because its workload is visual.

FSM_API already provides:

- named processing groups;
- process-rate throttling;
- event-driven/manual processing;
- deterministic state transitions;
- runtime definition modification.

These map directly onto Event Horizon computation.

Conceptually:

```
LOD0 / immediate       -> highest-frequency computation
LOD1 / interaction     -> high-frequency computation
LOD2 / local context   -> moderate computation
LOD3 / environment     -> low-frequency computation
...
LOD10 / distant        -> very low-frequency or event-driven computation
```

An LOD number is therefore a policy label, not the scheduler itself.

See [FSM Scheduling Model](FSM_SCHEDULING_MODEL.md) for the deeper model.

## Representation computation is an FSM

The first executable integration is deliberately small:

```
Observer / Policy
       |
       v
Desired Representation
       |
       v
+---------------------------+
| FSM_API                   |
| Renderer.Representation   |
+---------------------------+
       |
       v
Active Representation
       |
       v
Geometry / Material /
Animation / Simulation /
Interaction / Streaming
```

`RendererComputationContext` carries the decision state. `RendererComputationMachine` owns the FSM instance. The FSM does not draw anything; it determines which computational responsibility is active.

This establishes the important direction:

> **Use FSM computation where the renderer has stateful computational behavior—not merely because FSM_API happens to be available.**

That gives the renderer a concrete place to grow richer state machines for promotion, demotion, temporal stability, streaming readiness, interaction, and other Event Horizon dimensions.

## Processing groups and heterogeneous observers

There is an important limitation in the current FSM_API model.

ProcessingGroup and ProcessRate belong to the FSM definition/bucket, so instances of one definition share those scheduling characteristics.

That is excellent for homogeneous cohorts:

```
LOD2 population
    |
    +--> same processing policy
    +--> same cadence
```

But observer-relative rendering naturally creates heterogeneous instances:

```
Tree A -> LOD0
Tree B -> LOD3
Tree C -> LOD7
Tree D -> LOD10
```

We should not hide this limitation by building a parallel Renderer scheduler.

Instead, it is a useful candidate for a future FSM_API capability: allowing an individual FSM instance to select or inherit a processing lane/cadence without mutating the shared definition.

The Renderer can proceed now using FSM states for representation and processing groups/rates for homogeneous scheduling cohorts.

## Deterministic procedural detail

The scheduler alone is not the whole solution.

A distant entity should often retain a compact identity/seed from which additional detail can be reconstructed when promoted.

For a procedural tree:

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

The same inputs should reproduce the same structural decisions.

The Workshop already has Squirrel3 documentation in SingularityWarehouse, so Squirrel3 is a candidate mathematical primitive for investigation. It is not yet assumed to be the final Renderer procedural algorithm.

The requirement is:

> **When detail is not justified, retain the information needed to reconstruct it—not the detail itself.**

## Rendering for AI observers

The observer does not necessarily need a rendered image.

A semantic observer can receive information such as:

```
10 m left: goblin
100 m right: dragon
interaction: goblin reachable
threat: dragon high
```

The AI should not have to spend cognitive/computational effort rediscovering facts already available in the world model.

This means the same Event Horizon system can control multiple observer outputs:

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

That is a direct extension of the Renderer principle: rendering is about constructing the right observable information, not merely drawing lines.

## Occlusion as computational leverage

Promotion of a foreground object can naturally increase its geometric presence and therefore reduce the visibility of objects behind it.

That creates a potentially useful feedback loop:

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

This should be measured rather than assumed, but it is a promising property of the Event Horizon model.

## Important boundary

Renderer must not become coupled to FSM_COS.

FSM_COS may assemble Renderer capabilities.

Renderer should remain independently understandable, testable, and usable.

## First integration experiment

Before integrating a GPU, prove this:

1. Create observable entities.
2. Create an observer context.
3. Assign Event Horizon policies.
4. Select representations.
5. Schedule updates according to those policies.
6. Measure how much work is avoided.
7. Verify that promotion/demotion remains stable.
8. Verify that deterministic procedural representations reproduce correctly.
9. Verify that the same observation model can produce visual and semantic outputs.

If that experiment is successful, the graphics backend becomes an output concern rather than the foundation of the architecture.
