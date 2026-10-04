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

## Horizon-driven processing

A useful future experiment is to divide renderer work according to Event Horizon.

For example:

```
Horizon 0 -> evaluate every processing interval
Horizon 1 -> evaluate frequently
Horizon 2 -> evaluate periodically
Horizon 3 -> evaluate rarely
Horizon 4 -> evaluate on events
```

The scheduler would then process only the groups whose policies require evaluation.

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

If that experiment is successful, the graphics backend becomes an output concern rather than the foundation of the architecture.
