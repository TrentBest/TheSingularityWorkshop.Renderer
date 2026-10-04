# Event Horizon Theory

## Definition

An **Event Horizon** is a boundary at which the Renderer changes the computational representation of an observable entity.

The boundary may be spatial, temporal, semantic, interaction-driven, or budget-driven.

It is therefore not necessarily a circle around the observer.

---

## A horizon is a change in responsibility

Imagine a tree.

Close to the observer:

- branches matter
- leaves may animate
- collision may matter
- interaction may be possible

Far away:

- the tree may become a silhouette
- animation may be sampled
- collision may disappear
- only landmark identity may remain

The entity did not disappear.

The Renderer changed what it is responsible for computing.

That is the meaning of crossing a horizon.

---

## Promotion

Promotion occurs when an entity requires more computational detail.

Possible causes:

- approaching the observer
- entering the view
- becoming selected
- becoming interactive
- becoming semantically important
- predicted future visibility
- receiving an important event

Promotion should ideally happen before the observer notices the transition.

That implies **predictive promotion** is a future capability worth investigating.

---

## Demotion

Demotion occurs when detailed computation is no longer justified.

Possible causes:

- increasing distance
- leaving the view
- loss of interaction
- reduced semantic importance
- observer movement
- budget pressure
- streaming pressure

Demotion should not necessarily happen immediately.

---

## Hysteresis

If a boundary is exactly at 10 meters, an object moving around 10 meters can repeatedly cross it:

```
near -> far -> near -> far -> near
```

This causes churn.

A robust policy can use different thresholds:

```
Promote at 9 m
Demote at 11 m
```

The gap is hysteresis.

The principle is:

> **A representation should have stability, not merely correctness at an instant.**

---

## Event Horizons can overlap

Different dimensions may have different horizons.

For one entity:

```
Geometry horizon:      20 m
Animation horizon:     40 m
Interaction horizon:    5 m
Simulation horizon:    15 m
Semantic horizon:     500 m
```

This is much more expressive than a single LOD number.

The entity can therefore exist in a mixed state:

```
low geometry
+
sampled animation
+
active semantics
+
disabled interaction
```

This mixed representation is likely to be important to the eventual architecture.

---

## Horizon crossing is not necessarily a resource swap

A naive implementation might think:

```
representation A
      |
      v
destroy A
      |
      v
create B
```

A more mature renderer may instead use:

```
representation state
      |
      +--> increase detail
      +--> decrease detail
      +--> change update frequency
      +--> acquire/release assets
      +--> change simulation participation
```

This allows gradual transitions and shared resources.

---

## Horizons are policy

The Renderer should not permanently define:

```
10 m = near
50 m = distant
100 m = semantic
```

Those numbers depend on the experience.

A VR experience, strategy map, architectural walkthrough, and space simulator can require radically different policies.

The Renderer should provide mechanisms.

The Experience should provide policy.

---

## The eventual question

The central Event Horizon question is:

> **What is the cheapest representation that preserves the information the observer can actually benefit from?**

That question should guide the API, tests, benchmarks, and eventual GPU backend.
