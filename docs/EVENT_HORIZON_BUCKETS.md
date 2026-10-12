# Event Horizon Buckets

## What a bucket is

An **Event Horizon** chooses the computational detail and freshness policy appropriate to an observer. A **bucket** partitions the work inside that horizon by a domain or population that can be refreshed independently.

A horizon is not one indivisible image, and a bucket is not necessarily one texture.

```text
Event Horizon: Near
├── Environment     geometry + static resources
├── Constructs      authored structures
├── Motion          transforms and animation samples
├── Lighting        light lists and derived lighting
├── Shadows         visibility/shadow representation
└── Zombies         individual or cohort representation

Event Horizon: Far
├── Environment     reusable distant representation
├── Motion          sampled or aggregate movement
├── Lighting        simplified/aggregated light contribution
├── Shadows         lower-cost shadow representation
└── Zombies         instanced or population aggregate
```

The names are examples, not a fixed enum. An experience can define its own domains. A zombie-survival game may have a Zombies cohort; an architectural viewer need not.

## Why buckets are independent

Geometry, motion, lighting, shadows, and population do not necessarily change at the same time or at the same rate.

- A transform can change while geometry resources remain valid.
- A light can change while object geometry remains valid.
- A far population can retain an aggregate representation while near entities are animated individually.
- A freshness deadline can request work without making a still-valid cached result disappear.
- A membership change should invalidate the buckets whose contents changed, not every bucket in the scene.

The governing rule is **invalidate by dependency, not by proximity or habit**. A far representation remains visible and reusable until a correctness-relevant change makes it stale or a replacement is safely ready.

## How membership works

`EventHorizonBucketMembershipIndex` tracks stable member IDs for one visual domain. Within that index, one member belongs to at most one bucket at a time. `Move` transfers a member between buckets and marks both affected buckets dirty. This supports near/far cohort transitions without leaving the same membership in both buckets.

Use a separate index for each domain. The same soldier may have independent memberships in Geometry, Motion, Lighting, and Semantics because those are separate computational responsibilities.

```csharp
var near = new EventHorizonBucket(
    "Zombies",
    new EventHorizon("Near", maximumDistance: 100, updatesPerSecond: 30),
    updatesPerSecond: 30);

var far = new EventHorizonBucket(
    "Zombies",
    new EventHorizon("Far", maximumDistance: 10_000, updatesPerSecond: 1),
    updatesPerSecond: 1);

var population = new EventHorizonBucketMembershipIndex();
population.Add("cohort:42", near);

// When policy decides this cohort belongs to the far horizon:
population.Move("cohort:42", far);
```

The membership index is not a spatial partitioner, a horizon-selection algorithm, or a population simulator. The caller decides when a member should move. Simulation, collision, damage, and authoritative gameplay must not silently inherit a bucket's visual refresh rate.

## Validity, cadence, and safe refresh

Each bucket owns its cadence, dirty reasons, source revision, cache validity, and refresh history. A due refresh does not itself invalidate a valid representation.

```text
valid cached representation
          |
          +-- cadence not due --> reuse
          |
          +-- cadence due ------> keep old output visible
                                   |
                                   v
                              build candidate
                                   |
                      +------------+-----------+
                      |                        |
                 ticket current            ticket stale
                      |                        |
                 accept output            reject output
```

A refresh ticket captures the bucket identity, invalidation generation, and source revision. If the source or dependencies change during processing, the result is rejected rather than published as current. The previous cache is not overwritten by stale work.

`EventHorizonBucketDependencyGraph` declares input-to-dependent relationships. Invalidation propagates transitively across horizons, and cycles are rejected. A newly added dependency also invalidates any dependent result that may have been built before that dependency was declared.

`HorizonRepresentationHandoff` applies the corresponding presentation rule: the current representation remains selected while a candidate is prepared. A handoff can commit only when the candidate is ready, covers the observer view, and matches the current source and observer revisions. The code represents coverage as an input; it does not yet calculate reprojection or disocclusion coverage.

## Measuring the work

The bucket exposes a diagnostic snapshot with counts for cadence checks, due/not-due results, refresh attempts, accepted/rejected refreshes, invalidations, observed invalidation reasons, and explicit cache reuse.

A consumer calls `TryRecordReuse` only when it actually reused a valid cached representation. The counter is not inferred from a low refresh rate. These counts make it possible to distinguish “the cache could have been reused” from “the pipeline did reuse it.”

They are **work-accounting signals, not proof of a frame-time or energy improvement**. End-to-end performance still needs repeatable workloads measuring CPU preparation/scheduling, allocations, transferred bytes, GPU stage timings, cache memory, and frame-time percentiles on recorded hardware.

## A complete selective-refresh scenario

The regression scenario in `EventHorizonWorkloadScenarioTests` exercises the policy as one chain rather than testing each primitive in isolation:

1. Prepare valid representations for near geometry, near lighting, far shadows, and an unrelated far population.
2. Declare geometry → lighting → shadows dependencies.
3. Change the geometry source revision.
4. Verify lighting and shadows become invalid, while the unrelated far population remains valid and records explicit cache reuse.
5. Refresh the invalidated chain and verify the resulting representations are accepted.

This demonstrates the intended correctness boundary: **a source change propagates to declared dependents, not every bucket in the scene**. The scenario uses the real Renderer bucket APIs and deterministic timestamps. It is a policy regression test—not a live renderer, a GPU execution test, or a performance benchmark.

## Current boundaries

Implemented in the backend-free Renderer model:

- independently named buckets with separate cadence and validity;
- source-revision and dirty-reason tracking;
- stale refresh rejection;
- per-domain member-to-bucket tracking and membership invalidation;
- transitive dependency invalidation and cycle prevention;
- explicit cache-reuse and refresh outcome counters;
- safe candidate handoff checks for readiness, coverage, and revisions.

Not implemented by these primitives:

- spatial indexing or automatic horizon assignment;
- actual GPU resources, drawing, or persistent rendered textures;
- observer reprojection, parallax reconstruction, or filling newly exposed regions;
- forge-time MicroBundle arbitration or prepared bucket-local FSM pipelines;
- a measured end-to-end performance result.

Those boundaries are deliberate: the validity model can be tested without selecting a graphics API, while the eventual backend must prove that its cache representation remains correct as the observer moves.
