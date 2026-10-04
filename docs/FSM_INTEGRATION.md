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

The Renderer should reuse the state model where it provides a real benefit instead of creating a parallel state machine abstraction.

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
