# Performance Positioning

## The question

The Renderer is intended to make computational effort follow observable consequence rather than raw world population.

The relevant comparison is therefore not simply frames per second. The meaningful question is:

> How much computation, memory, geometry, submission, and update work is required to preserve the information an observer can actually perceive or act upon?

This document records the current comparison between the Workshop model and established high-performance rendering approaches.

## What the strongest existing systems demonstrate

Modern high-performance renderers already validate a major part of this direction.

Unreal Engine's Nanite documentation describes fine-grained visibility and detail selection whose effective geometric work is intended to scale with pixels rather than source-scene complexity. Nanite performs hierarchical cluster selection, occlusion culling, and on-demand streaming so that detail which cannot contribute meaningfully to the image need not be processed at full fidelity.

That is very close to the Renderer hypothesis:

```text
source world complexity != observable computational complexity
```

Our proposed Event Horizon model generalizes the same principle beyond geometry:

```text
observable consequence
    -> representation
    -> evaluation cadence
    -> simulation responsibility
    -> semantic responsibility
    -> streaming responsibility
    -> presentation
```

## Comparison of principles

| Concern | Conventional high-performance rendering | Nanite-style virtualized geometry | Renderer hypothesis |
| --- | --- | --- | --- |
| Distance | Often a signal for LOD/culling | Contributes to projected detail selection | Signal, never the definition |
| Visibility | Culls work that cannot contribute | Fine-grained visibility/occlusion | Observable consequence |
| Geometry detail | LOD / cluster simplification | Hierarchical clusters | Event Horizon representation |
| Screen consequence | Important | Central scaling signal | Central policy input |
| Temporal frequency | Usually handled elsewhere | Primarily rendering-frame driven | First-class Event Horizon dimension |
| Simulation | Separate systems | Outside core geometry mechanism | Can change with representation |
| Semantics | Usually outside renderer | Outside core geometry mechanism | Protocol-qualified semantic detail |
| Interaction | Usually separate | Mostly outside geometry selection | Can change by horizon |
| Streaming | Asset/geometry streaming | Fine-grained geometry streaming | Representation-specific streaming |
| Scheduler | Engine/frame scheduler | GPU-driven rendering path | FSM cohort/process-rate model |
| Distant visible landmarks | Can remain visible at low geometric cost | Possible through coarse representation | Explicitly preserved; parallax determines required temporal work |
| World population | Reduced through culling/LOD | Geometry work tends toward visible screen complexity | Hypothesis: work follows observer consequence across multiple computation types |

## What our current mathematics actually proves

It does **not** yet prove that the Workshop Renderer is faster than Nanite, Unity, Unreal, or another production renderer.

It proves something narrower and useful:

1. The observer-relative geometry is mathematically well-defined.
2. Parallax decreases as inverse depth.
3. Therefore a distant object can remain visible while requiring dramatically less positional update work.
4. FSM_API provides an empirical scheduling-cost calibration.
5. Event Horizons provide a mechanism for reducing both work-per-evaluation and evaluations-per-second.
6. The resulting prediction is measurable rather than purely intuitive.

The current FSM_API calibration is:

```text
1 processing group  = 305.1 ns / 360 B
50 processing groups = 15,736.6 ns / 18,000 B
```

The first-order interpolation is:

```text
T(G) ≈ 305.1 + 314.93(G - 1) ns
A(G) ≈ 360G bytes
```

At 60 scheduler updates per second, the measured 50-group update corresponds to approximately:

```text
15,736.6 ns * 60 = 944,196 ns/s
                          = 0.944 ms/s
```

If that scheduler update occurs once per rendered frame at 60 FPS, the same overhead is approximately 0.0157 ms per frame. This is only the measured FSM scheduling component; it is not a claim about complete rendering cost.

## The mountain case

For a 1920-pixel image with a 90° horizontal field of view:

```text
f = 960 px
```

For 10 m of lateral observer movement:

```text
10 m depth:       |Δu| ≈ 960 px
300,000 m depth:  |Δu| ≈ 0.032 px
```

The mountain does not need to disappear.

Its projected motion is simply approximately 30,000 times smaller than the 10 m object's motion:

```text
300,000 / 10 = 30,000
```

That gives the Renderer a physical basis for reducing update frequency while preserving visibility.

This is fundamentally different from saying:

```text
distance > threshold -> hide object
```

Instead:

```text
distance -> projected consequence -> required representation -> required cadence
```

## The critical next experiment

The next benchmark must stop using the scheduler as a proxy for the complete Renderer.

We need controlled synthetic worlds with increasing populations:

```text
1,000
10,000
100,000
1,000,000
10,000,000
...```

For each population, measure at least:

- total world entities
- currently visible entities
- predicted-to-be-visible entities
- semantic-important entities
- entities by Event Horizon
- horizon-selection CPU time
- parallax/projection CPU time
- FSM scheduling CPU time
- representation evaluation CPU time
- procedural reconstruction CPU time
- promotion/demotion work
- memory residency
- streaming bytes and churn
- command/draw submission
- GPU frame time
- GPU geometry work
- GPU pixel/fragment work
- frame-time variance
- observer-visible information retained
- work avoided relative to naive full-rate evaluation

The decisive comparison is:

```text
Naive:
    Work ≈ N_world * C_full * f_full

Observer-relative:
    Work ≈ Σ(N_i * C_i * f_i)
```

The theory succeeds if the second curve grows substantially more slowly as world population increases while the observer's visible result remains equivalent within a defined perceptual/error budget.

## Why this could exceed conventional LOD

The interesting possibility is not merely another LOD system.

A conventional geometry system asks something like:

> Which geometric detail should be drawn?

The Renderer asks the larger question:

> What computational representation must exist, at what fidelity and cadence, for this observer to receive the required information?

That permits independent Event Horizons for:

- geometry
- materials
- animation
- simulation
- interaction
- semantics
- procedural reconstruction
- streaming
- scheduling

A distant mountain may therefore remain visually present, have near-zero meaningful parallax velocity, require infrequent geometric evaluation, retain semantic identity, and remain available for promotion when observer context changes.

## Current conclusion

The mathematics supports the **direction** of the theory.

It does not yet establish a performance victory.

The strongest evidence available today is that the theory independently arrives at a principle already demonstrated by leading virtualized rendering technology: computational effort should track observable consequence rather than raw source complexity.

The potentially novel part is extending that principle across the full computational representation rather than geometry alone.

The next step is empirical scaling.

That benchmark—not an intuition, FPS screenshot, or theoretical extrapolation—will tell us whether the Workshop approach is merely philosophically aligned with the best systems or measurably more efficient for the workloads it targets.
