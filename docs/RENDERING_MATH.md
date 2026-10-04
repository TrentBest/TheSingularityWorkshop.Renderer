# Rendering Mathematics and Performance Estimation

## Why the Renderer needs mathematics

The Renderer should not rely on statements such as "this ought to be fast." The Workshop already has empirical FSM_API benchmarks, giving the Renderer a way to construct a first-order performance model before a graphics backend exists.

The goal is not to predict the final machine perfectly. The goal is to make computational cost estimable, falsifiable, and benchmarkable.

## Initial FSM_API calibration

| Processing groups | Update overhead | Measured allocation |
| ---: | ---: | ---: |
| 1 | 305.1 ns | 360 B |
| 50 | 15,736.6 ns | 18,000 B |

The first-order model treats the measured relationship as approximately linear over this range.

```text
T(G) = T1 + (G - 1) * M
T1 = 305.1 ns
M  = (15,736.6 - 305.1) / 49
M  ≈ 314.93 ns/group
T(G) ≈ 305.1 + 314.93 * (G - 1) ns
A(G) ≈ 360 * G bytes
```

At 50 groups this reproduces the observed approximately 15.737 microsecond result.

These are calibration values, not universal constants.

## From scheduler cost to workload cost

If a scheduler update occurs at frequency f, estimated scheduler CPU time per second is:

```text
CPU_ms/s = T(G) * f / 1,000,000
```

Using the 50-group calibration at 60 scheduler updates per second gives approximately 0.944 ms/s of scheduling overhead alone.

This does not include representation evaluation, procedural generation, animation, physics, visibility testing, asset streaming, GPU submission, GPU execution, synchronization, or host presentation.

## A renderer cost equation

The larger Renderer model should eventually be expressible as:

```text
C_total =
    C_selection
  + C_scheduler
  + C_representation
  + C_simulation
  + C_streaming
  + C_submission
  + C_gpu
```

For an observer-relative entity population:

```text
C_total(observer) = SUM entity_i C(entity_i, observer, policy)
```

The cost of an entity is therefore conditional on observation rather than constant.

## Event Horizon as a cost function

A representation can be modeled as geometry, material, animation, simulation, interaction, semantics, streaming, and cadence. Each contributes cost.

```text
R* = argmin C(R)
subject to:
Information(R, observer) >= RequiredInformation(observer)
```

In words: choose the least expensive representation that preserves the information the observer can benefit from.

## Cadence matters

If a representation costs C per evaluation and is evaluated at f evaluations per second:

```text
CPU_r/s = C * f
```

A distant representation can therefore become cheaper in two independent ways: perform less work per evaluation and evaluate it less often.

## Cohort scheduling

If cohort i contains N entities, has frequency f, and average per-entity work C:

```text
C_cohort_i/s ≈ N_i * C_i * f_i
C_entities/s ≈ SUM N_i * C_i * f_i
```

This creates a direct optimization target: move entities toward cheaper valid representations when the observer no longer benefits from their additional detail.

## Deterministic procedural reconstruction

Instead of retaining fully materialized detail, retain identity, seed, placement, and policy, then reconstruct detail deterministically.

```text
Detail = F(identity, seed, placement, representation)
```

This creates a measurable trade between saved memory and reconstruction CPU. Squirrel3 is one candidate primitive for this work, but the repository intentionally does not assume it is the final solution.

## The performance loop

```text
hypothesis -> mathematical model -> implementation -> benchmark
                                      ^                 |
                                      |                 v
                                      +--- refine <--- compare
```

The model is part of the engineering system, not marketing arithmetic.

## The next level

Future benchmarks should calibrate horizon selection, semantic anchor lookup, procedural reconstruction, promotion and demotion, cohort migration, visibility and occlusion decisions, representation construction, streaming, CPU/GPU submission, and complete observer workloads.

Eventually the Renderer should be able to estimate CPU work, memory residency, avoided work, and model confidence for large observer-relative worlds before a graphics backend is selected.

## Parallax and physically meaningful distance

Distance is not a visibility cutoff. Perspective makes distant objects smaller, while parallax makes their apparent motion under observer translation smaller as inverse depth.

For lateral observer movement ΔC:

    |Δu| = f * |ΔC| / Z

For two depths:

    |Δu_relative| = f * |ΔC| * |1/Z_near - 1/Z_far|

This gives the Renderer a way to keep a mountain hundreds of kilometers away visible while reducing the frequency and cost of computation associated with its extremely small projected motion.

See [Parallax and Observer Geometry](docs/PARALLAX_AND_OBSERVER_GEOMETRY.md).
