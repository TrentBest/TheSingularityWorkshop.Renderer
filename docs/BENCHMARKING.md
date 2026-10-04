# Renderer Benchmarking

The Renderer benchmark program is intentionally kept in a separate repository:

**TheSingularityWorkshop.Renderer.Benchmarks**

This keeps the package itself small and dependency-light while giving performance work a dedicated laboratory for large workloads and repeated BenchmarkDotNet experiments.

## What is measured

The benchmark laboratory is organized around the Renderer hypothesis:

> **The amount of stuff in the world should not determine the amount of computation required to show the world. Observable consequence should.**

The current benchmark matrix includes:

1. **Observer-relative mathematics** — perspective projection, lateral parallax, view angle, and Event Horizon selection.
2. **Population scaling** — 1,000 through 10,000,000 entities.
3. **Event Horizon selection** — 4, 16, 64, and 256 horizons.
4. **Renderer FSM scheduling** — one Renderer computation machine versus complete configured populations.
5. **Autonomous-world workloads** — million-scale through hundred-million-scale synthetic populations, with future safety/order propagation and cohort migration experiments.

The large experiments are deliberately allowed to falsify the architecture. If traversal, selection, scheduling, allocation, or global event propagation dominates the workload, the benchmark should show it.

## First measured result

| Operation | 10 m | 1,000 m | 300,000 m | Allocation |
| --- | ---: | ---: | ---: | ---: |
| ProjectHorizontal | 6.244 ns | 6.225 ns | 6.387 ns | 0 B |
| LateralParallax | 2.810 ns | 2.828 ns | 2.821 ns | 0 B |
| ViewAngle | 20.395 ns | 19.409 ns | 14.388 ns | 0 B |
| SelectEventHorizon | 33.148 ns | 35.584 ns | 38.379 ns | 64 B |

Environment:
- Windows 10 22H2
- Intel Core i5-10400F
- .NET 8.0.31 host
- BenchmarkDotNet 0.16.0-preview.2
- TheSingularityWorkshop.Renderer 0.1.0-alpha.1

A later debugger-attached run reproduced the same order of magnitude. It is recorded as validation only because an attached debugger is not a clean performance environment.

### What this establishes

The first result establishes that the core observer-relative mathematical primitives are extremely small operations on the test machine and, importantly, their cost does not materially increase as depth changes from meters to hundreds of kilometers.

It does **not** establish that the complete Renderer is faster than Nanite, Unreal, Unity, or any other production renderer.

It establishes a calibration point for the next experiments.

### Known optimization target

`SelectEventHorizon` currently reports a 64-byte allocation in the benchmark. That allocation is now a concrete optimization target rather than an assumption.

The expanded benchmark suite will determine whether selection, traversal, scheduler overhead, or representation work becomes the dominant cost as population grows.

## Mathematical interpretation

For lateral observer movement:

    |Δu| = f * |ΔC| / Z

and for relative parallax between two depths:

    |Δu_relative| = f * |ΔC| * |1/Z_near - 1/Z_far|

This means a distant mountain can remain observable while its projected motion becomes extremely small. The renderer therefore has a mathematically grounded reason to reduce update frequency without treating distance as a visibility cutoff.

## Scaling hypothesis

A naive renderer that fully evaluates every entity at the same frequency behaves approximately like:

    Work ≈ N_world * C_full * f_full

The observer-relative model instead seeks:

    Work ≈ Σ(N_i * C_i * f_i)

where each cohort has a representation and cadence appropriate to its observer-relative consequence.

The benchmark is successful only if this reduces measured work while preserving the information required by the observer.

## Benchmark discipline

Results are machine-specific.

Every meaningful benchmark record should identify CPU, operating system, .NET runtime, BenchmarkDotNet version, Renderer package/source version, benchmark parameters, debugger status, allocation measurements, and mean/error/deviation where available.

No benchmark result should be presented as a universal hardware claim.

## Next decisive measurements

The most important next measurements are:

1. complete population scaling
2. Event Horizon selection cost versus horizon count
3. Renderer FSM scheduling cost versus population
4. promotion/demotion and cohort migration
5. deterministic procedural reconstruction
6. memory residency versus materialized detail
7. visibility/occlusion work
8. large autonomous-world safety/event propagation
9. complete observer workloads

The goal is not to manufacture a favorable benchmark. The goal is to discover where the observer-relative model actually wins, where it does not, and what the architecture must change as a result.