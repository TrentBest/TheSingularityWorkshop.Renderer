# Gallery recipe: composition and ownership

![Renderer composition illustration](../assets/renderer-composition.svg)

## What this exhibit means

The picture explains the intended relationship between observer context, Event Horizons, independent work buckets, and frame assembly. It is an architectural map for discussion, not a runtime trace.

## How to use it

Use it to ask which layer owns each decision and which input invalidates each unit of work. Then verify the answer against code and tests before treating the arrow as an implemented dependency.

## Evidence status

- **Illustration:** the ownership and flow described by the picture.
- **Not demonstrated by this picture:** all buckets executing as one live end-to-end pipeline, measured invalidation savings, GPU submission, or hardware performance.

## Next recipe

Replace conceptual arrows with executable probes one at a time: mutate a source input, run the actual dependency graph, and visualize which outputs were recomputed versus reused. Preserve a baseline and timings so the picture explains measured behavior rather than implying it.
