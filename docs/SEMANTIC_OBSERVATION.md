# Event Horizons as semantic sensors

## The larger idea

Renderer is not only a way to reduce visual detail or avoid culling work. An Event Horizon can be a **semantic observation boundary**: it determines what an observer or agent can usefully know, at what fidelity, and at what computational cost.

That creates a powerful path for AI and FSM-driven agents. Instead of asking an agent to infer the world from pixels, the experience can provide structured observations derived from authoritative world state and the same observer-relative policy that guides representation.

For example, a soldier agent might receive:

- **Near:** two friendly soldiers to the left; one damaged vehicle directly ahead.
- **Mid:** a tank on the right; a squad moving behind cover.
- **Far:** a large hostile formation approaching from the north; individual identities omitted.
- **Unknown / occluded:** a likely contact behind terrain, with confidence and freshness represented explicitly.

These are illustrative observation records, not a claim that Renderer currently emits this exact schema. The important idea is that the agent gets actionable, typed meaning—not a screenshot it must visually recognize.

## One world, several consumers

```text
                 Authoritative world / experience
                    /                    \
                   v                      v
          Observer + policy        Agent / task context
                   \                      /
                    v                    v
                  Event Horizon evaluation
                            |
            +---------------+----------------+
            |               |                |
            v               v                v
      Visual output   Semantic observations  Work policy
      geometry, etc.  entities, counts,      refresh, cadence,
                      relations, confidence  detail / budget
            |               |                |
            v               v                v
       RenderFrame      Agent's FSM       FSM_API scheduling
       / host adapter   transitions       and computation
```

The shared source is authoritative world state. Visual and semantic products are different projections for different consumers. A renderer representation must not become the world's authoritative state, and an agent should not be forced to reverse-engineer semantics from a rendered frame.

The same policy can help both consumers decide where detail matters, but their output requirements are not identical. A distant entity may need only a silhouette visually while still needing a semantic category or count for a tactical decision. Conversely, a visually prominent effect may have little tactical importance.

## What a semantic observation needs

A useful observation contract should be explicit about:

- **Identity and category:** stable entity identity when permitted, plus semantic class (soldier, tank, obstacle, objective, and so on).
- **Relation to the observer:** bearing/sector, relative position, range band, and optionally a named relation such as left, right, ahead, above, or behind.
- **Aggregation:** exact entities nearby; groups or counts at larger horizons; summary statistics when individual identity is not useful.
- **Knowledge quality:** visible, sensed, inferred, occluded, stale, or unknown. Do not report an unseen entity as certain merely because it exists in authoritative world state.
- **Time and freshness:** observation timestamp or revision, last refresh, and the policy that controls when it should be refreshed.
- **Provenance:** which sensor/channel or world-query contract supports the observation.
- **Budget and importance:** enough information to explain why the observation is retained or summarized.

This is a design target, not a finalized public API. The first implementation should stay small and typed, avoid embedding behavior in data, and remain independent of a particular AI provider.

## Event Horizons are not simply distance rings

Distance is one signal, not the meaning of a horizon. A policy can combine distance with visibility, occlusion, sensor capability, observer intent, semantic importance, interaction state, uncertainty, and computational budget.

A useful result is therefore richer than “LOD 2.” Conceptually it could answer:

1. What is the observer justified in knowing about this entity or group?
2. At what semantic and visual resolution?
3. Which work is due now, and what can be reused?
4. What changed since the previous observation?
5. What must remain unknown because the observer has not acquired it?

Do not hard-code a fixed number of horizons into the semantic model. Names such as Near, Mid, and Far are helpful examples; the policy and the consuming experience should determine the actual bands and summaries.

## Feed decisions into an FSM, not a script of special cases

An FSM-driven soldier should make behavior choices from a structured observation context. For example:

```text
Observation context
  friendly_left_count = 2
  hostile_vehicle_right = true
  forward_path_obstructed = true
  contact_confidence = known / uncertain
                |
                v
          Soldier FSM_API
                |
        +-------+--------+
        |       |        |
        v       v        v
      Cover   Coordinate  Reposition
```

The values above are illustrative. They are not a prescribed tactical policy. The point is architectural: the agent's FSM consumes stable semantic inputs and selects behavior; Renderer does not own soldier behavior, combat doctrine, or world truth.

This also makes testing stronger. A test can assert that a known observation context leads to the expected FSM transition without rendering a pixel or loading a GPU.

## Avoiding a semantic-culling trap

Reducing visual work must not accidentally erase information that another consumer still needs. Visual representation, semantic observation, simulation participation, interaction, and update cadence may share source data while using separate policies.

In particular:

- A far entity may be visually aggregated but still contribute to a count or threat summary.
- An occluded entity should not automatically become known to an observer; visibility and sensor channels matter.
- A representation demotion must not destroy authoritative identity, state, or the ability to reconstruct richer detail.
- A shared cached observation must declare its observer, sensor assumptions, revision, and freshness; it cannot silently stand in for every observer's knowledge.
- Aggregates should be updated incrementally where possible, with dependency invalidation and measurable refresh cost.

The goal is not to abolish every form of culling. It is to avoid confusing **“not drawn individually”** with **“does not exist,” “does not matter,” or “must be unknown to every system.”**

## Implementation path

1. **Define a minimal semantic observation contract** beside the existing observer and semantic-anchor concepts. Keep it graphics-API- and AI-provider-independent.
2. **Create a deterministic sample world** containing friendly soldiers, hostile soldiers, a tank, terrain obstruction, and multiple observer positions.
3. **Generate horizon summaries from known world data**, including entity counts and relative relations. Label this as a semantic policy sample until connected to the production selection path.
4. **Feed the observation into a small FSM_API agent** and show the transition and its reason.
5. **Connect the same world to visual representation selection**, so the documentation can show a coordinated pair: what the observer sees visually and what the agent knows semantically.
6. **Move the observer and change sensor/context inputs**. Show counts, summaries, confidence/freshness, representation, and FSM decisions changing together.
7. **Measure scaling** as total population grows by 10× and 100×: observation cost, refreshes, per-horizon work, allocations, and decision latency. Compare equivalent correctness requirements, not just raw operation counts.

## Evidence labels

Every demo must distinguish these levels:

- **Conceptual:** explains the proposed architecture.
- **Generated sample:** output from a deterministic executable sample, with inputs and command documented.
- **Tested integration:** actual Renderer and FSM_API components are connected in the test path.
- **Live host/backend:** the host consumes the real output and displays or acts on it.

An animated diagram is useful teaching material, but animation alone is not evidence of a running policy or agent.

## Spatially grounded concepts and interaction

Semantic observations can extend beyond labels and counts to spatially grounded parts and possible interactions: for example, an ATM's card-slot pose, a tank's hatch or maintenance terminal, or a group of characters with a count and spatial extent. See [Spatial Semantics and Affordances](SPATIAL_SEMANTICS_AND_AFFORDANCES.md) for the broader concept and a staged implementation plan. These are design targets, not claims of current runtime support.

## Related documentation

- [Observation model](OBSERVATION_MODEL.md)
- [Event Horizon theory](EVENT_HORIZON_THEORY.md)
- [Event Horizon buckets](EVENT_HORIZON_BUCKETS.md)
- [FSM integration](FSM_INTEGRATION.md)
- [Living Renderer](LIVING_RENDERER.md)
- [Implementation roadmap](IMPLEMENTATION_ROADMAP.md)
