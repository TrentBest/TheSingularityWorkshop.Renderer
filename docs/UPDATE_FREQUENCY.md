# Update Frequency

Event Horizons are partly about **when computation occurs**, not only what gets drawn.

## Principle

> The farther an entity is from the observer, the less frequently it should generally need to be evaluated — unless another condition makes it important.

This allows computational effort to follow perceptual relevance.

## Frequency is policy

There should not be one universal Renderer update rate.

A representation may be:

- frame-driven
- fixed-rate
- sampled
- event-driven
- demand-driven
- dormant until promoted

A horizon supplies policy inputs rather than hard-coding one timing model.

## Example

A hypothetical scene could use:

| Representation | Frequency |
| --- | ---: |
| Immediate interaction | 120 Hz |
| Near-field animation | 60 Hz |
| Contextual scene | 15 Hz |
| Distant environmental state | 1 Hz |
| Semantic existence | event-driven |

Again, these are illustrative targets for experiments, not defaults.

## FSM relationship

FSM_API may provide a useful foundation for grouping and evaluating state transitions.

The Renderer should investigate whether Event Horizon membership can naturally determine which state groups need evaluation during a processing interval.

The important goal is not "use FSM_API everywhere."

The goal is:

> **Do not spend computation on a representation whose observer cannot benefit from the result.**

## Temporal coherence

Reducing update frequency must not produce visible instability.

Future work should investigate:

- interpolation
- extrapolation
- cached representations
- event-triggered promotion
- prediction
- temporal hysteresis

## Measurement

Frequency policy must eventually be benchmarked.

Useful measurements include:

- CPU time per horizon
- allocations per horizon transition
- representation promotion cost
- representation demotion cost
- update count per frame
- cache residency
- streaming latency
- observer-visible quality

The Renderer should prefer measured policies over assumptions.
