![The Singularity Workshop animated moniker](assets/moniker.svg)

# Renderer Theory: Observer-Relative Computation

> A textbook-style chapter set for understanding why Renderer exists, what its policies mean, and how the theory maps to executable code.

This document is the conceptual companion to the API. It is written to be teachable: instructors can use the chapters independently, add exercises, and connect the general principles to their own domains. Statements about the current code are deliberately separated from design goals.

## Learning objectives

After working through these chapters, a reader should be able to:

1. Distinguish an authoritative entity from one observer's representation of that entity.
2. Explain why conventional visual Level of Detail (LOD) is only one dimension of computational detail.
3. Derive a representation decision from observer needs, policy, and a finite work budget.
4. Explain promotion, demotion, hysteresis, freshness, provenance, and safe aggregation.
5. Keep simulation truth, representation policy, execution, and presentation in their proper domains.
6. Design tests and benchmarks that can falsify a claimed optimization instead of merely illustrating it.

## Chapter 1 — The problem: a world is larger than one observer's needs

A simulated or described world may contain far more entities, effects, relationships, and history than one observer can inspect at once. Treating every entity as if it required maximum detail at every instant spends work without necessarily improving the observer's understanding.

The naive policy is uniform fidelity: update and represent everything at the same rate and resolution. Its cost tends to grow with the whole world, even when the observer is interested in only a small fraction of it.

Renderer investigates a different objective: spend computation according to the information an observer can use, while preserving identity, critical facts, and the ability to recover detail when it becomes relevant.

This is not a claim that invisible or distant entities cease to exist. Their authoritative state remains owned by the world or simulation. Renderer chooses only the observer-relative representation and work that should be materialized for a particular view.

### A useful distinction

- **Existence:** what the authoritative domain says exists.
- **Knowledge:** what a particular observer can currently infer or access.
- **Representation:** the form and fidelity used to communicate relevant aspects of that existence.
- **Execution:** the CPU, GPU, or other capability that performs selected work.
- **Presentation:** the concrete pixels, text, surface, or device output seen by a consumer.

Confusing these layers creates bugs. A distant mesh may be simplified without deleting the entity. A map observer may receive a semantic summary without receiving a rendered image. A GUI may show a control without owning the action's business rules.

## Chapter 2 — The observer is part of the input

A representation is not correct in isolation; it is appropriate relative to an observer and a purpose. A first-person observer, a strategy map, an accessibility view, a remote spectator, and an AI agent can inspect the same authoritative world yet need different information.

An observer context can conceptually include:

- position, orientation, field of view, and visible region;
- purpose or intent, such as exploration, aiming, selecting, inspecting, or planning;
- interaction rights and precision requirements;
- perceptual or semantic salience;
- latency, freshness, uncertainty, and resource constraints;
- a stable observer identity and revision, so cached decisions can be invalidated correctly.

These are candidate policy inputs, not a promise that every listed signal is implemented in one public type today. Add a signal only when its meaning can be documented, tested, and traced to a decision.

### Example

A distant ambulance may occupy only a few pixels. Geometric detail can be reduced, but its emergency identity and direction of travel may remain essential. If the observer selects it, the representation may be promoted immediately. If it becomes occluded, the visual representation may be hidden while a recent semantic fact remains available with an explicit freshness limit.

The correct answer is not simply “far means low detail.” It is “which information does this observer need, and which work is justified to preserve it?”

## Chapter 3 — Event Horizons as computational boundaries

An **Event Horizon** is a spatial or contextual boundary at which Renderer changes the computational representation of an observable entity. It may be based on distance, projected size, visibility, intent, semantic consequence, time, uncertainty, or budget pressure. It need not be a literal sphere.

Conventional LOD usually changes geometric complexity. Renderer generalizes the idea to dimensions such as:

- geometry and material fidelity;
- animation and update cadence;
- residency and streaming;
- interaction precision and affordances;
- semantic detail and observation history;
- particles, volumes, lighting dependencies, and other presentation workloads.

Each dimension may have a different threshold and policy. An entity can therefore be geometrically simple, semantically important, and interaction-critical at the same time.

A conceptual policy can be written as:

`representation = Select(authoritativeState, observerContext, policy, budget, priorState)`

The prior state matters because good decisions are temporal: they should not flicker when inputs move slightly around a boundary. The policy and budget are experience-dependent; Renderer should supply reusable mechanisms rather than universal distance constants.

### Promotion and demotion

**Promotion** increases the fidelity, frequency, or availability of work when relevance rises. **Demotion** reduces work when detail is no longer justified. Neither operation should mutate the authoritative entity merely because its representation changed.

A robust policy may use hysteresis. For example, promote when distance falls below 9 units and demote only after it exceeds 11 units. The gap prevents small movements from repeatedly switching states. Actual thresholds must be calibrated to the scenario; the numbers are illustrative.

### Stable identity and reconstruction

A representation transition must preserve the identity of its source. A consumer should be able to tell that a coarse proxy and a detailed model refer to the same entity and source revision. Where an approximation or aggregation loses information, the policy should record that loss and identify when richer data is required again.

## Chapter 4 — Budgets, not magic numbers

“Cheaper” is not one scalar. Relevant constraints may include CPU selection time, allocations, GPU time, memory residency, data transfer, synchronization, latency, uncertainty, and interaction precision. These dimensions may trade off against each other and should not be silently collapsed into one score.

A policy may choose among candidate representations by minimizing estimated work subject to explicit constraints:

`minimize estimatedCost(candidate)`

subject to preserving required observer information, meeting freshness and interaction constraints, and staying within the available budget.

This formulation is a design model, not a claim that the current package contains a universal optimizer. An implementation should begin with deterministic rules and measured workload traces. If a weighted score is later introduced, document the units, normalization, tradeoffs, and tie-breaking rules. Never allow a low visual-cost estimate to override authoritative simulation updates or required interaction behavior.

### Degradation order

When resources are scarce, prefer reducing optional presentation fidelity before dropping critical facts, interaction correctness, or authoritative domain updates. For example, reduce particle density before removing a hazard warning. Keep policy decisions explainable: which signal changed, which dimension was reduced, which constraint forced the fallback, and how stale the result may be.

## Chapter 5 — Visibility, semantics, and freshness are different questions

“Not visible” does not always mean “not relevant.” Occlusion may hide a target while a recent sound, relationship, map marker, or hazard summary remains important. Conversely, a semantic fact can be too old to support a precise decision.

A safe observation therefore needs more than a value. It may need source identity, source revision, observation time or age, confidence/uncertainty, and the policy under which it was produced. Consumers must be able to distinguish current facts from cached or inferred ones.

Semantic observation is not visual LOD. An AI agent should receive structured facts from authoritative domain state and observation policy rather than being forced to reconstruct them from rendered pixels. Renderer can help select and package observer-relative information; the agent's reasoning and actions remain in the agent/Experience domain.

## Chapter 6 — Work buckets, dependencies, and temporal coherence

Different work has different invalidation rules. Geometry, visibility, lighting history, semantic summaries, and interaction affordances may update at different cadences. A dependency graph can express that a changed source invalidates dependent work without invalidating unrelated reusable results.

A work item should conceptually declare:
- the source identities and revisions it depends on;
- the observer/policy inputs that affect it;
- its validity and freshness conditions;
- the cost and importance of refreshing it;
- what downstream work must be invalidated if it changes.

This allows work to be cached or deferred safely. Reuse is correct only while its dependencies and observer requirements remain valid. An optimization that reuses stale output beyond its contract is a correctness bug, not a performance win.

Renderer should not create a second universal scheduler. It describes renderer-related work and its constraints; foundational state/scheduling capabilities can come from FSM_API, while independent Computation capabilities and providers own execution selection and execution. The host chooses when to request or present output according to its lifecycle.

## Chapter 7 — The frame is a handoff, not a GPU

A platform-neutral frame is a description of ordered rendering work. It is not proof that a GPU executed that work or that pixels were produced. A frame contract can describe views, commands, resource identities, index ranges, and other backend-neutral inputs; a concrete adapter translates them into a target execution path.

The logical content region must be explicit. Every GUI projection needs a **source rectangle** identifying which region of the logical content is projected. Do not infer it from destination dimensions. That same source region can be mapped to screen-space panels, textures, or world-space surfaces; destination-specific projection, camera, and perspective belong at the adapter/host boundary.

This boundary makes Renderer reusable. Visualize may realize scene or image output; GUI owns semantic interface composition and human interaction; hosts own device-specific presentation. None should become authoritative for the underlying Experience state.

## Chapter 8 — Domain boundaries across the Workshop

| Responsibility | Owning domain |
|---|---|
| Authoritative world, simulation, and game rules | Experience / simulation domain |
| Observer-relative representation, horizons, visibility/detail policy | Renderer |
| FSM state primitives used by Renderer computation | FSM_API |
| Workload capability selection and concrete execution providers | Computation and its provider capabilities |
| Image/scene manifestation | Visualize |
| Semantic UI, accessibility, and human input boundary | GUI |
| Runtime composition and capability arbitration | FSM_COS |
| Capability/artifact meaning | MicroBundleDomain |
| Host lifecycle and device integration | Host / adapter |

These are responsibility boundaries, not necessarily mandatory direct package dependencies. Renderer must remain usable without depending upward on FSM_COS, GUI, or Visualize. Composition should connect the contracts; it should not collapse the domains.

## Chapter 9 — Proving the theory

A diagram is an explanation, not evidence. A generated SVG can be valuable when it faithfully depicts a real sample or test, but it must say whether it is conceptual, generated from executable data, or live telemetry.

A minimum deterministic proof should specify:
1. authoritative source state and stable identities;
2. observer input and policy configuration;
3. expected representation choice and reason;
4. promotion/demotion behavior under controlled input changes;
5. freshness, invalidation, and reconstruction rules;
6. measured work and allocations, with hardware/runtime/configuration noted;
7. what the proof does **not** establish.

Benchmarks should vary total population, observer-relevant population, number of observers, and change rate. Measure selection cost, allocations, refresh counts, reused/skipped work, and correctness together. Report raw conditions and compare like with like. A CPU/SVG proof does not establish GPU performance; a command-count reduction does not establish geometry-buffer compaction; an isolated microbenchmark does not establish end-to-end scaling.

The hypothesis worth testing is that the work required to serve an observer should track observer-relevant consequence more closely than total world population—without sacrificing required information, stability, or correctness.

## Chapter 10 — A worked example: the distant hazard

Imagine an authoritative simulation contains a spreading ash cloud, a transient eruption flash, and persistent ash deposited on surfaces.

- The simulation owns the physical evolution and source revisions.
- Renderer receives observer context and source-derived facts.
- The policy selects volume, particle, lighting, and semantic detail independently.
- A distant map observer may receive a regional hazard summary and uncertainty without a detailed volume.
- A first-person observer approaching the cloud may trigger promotion of local density and visibility detail.
- A transient flash may invalidate lighting history, while persistent surface deposition has a longer lifecycle.
- Under budget pressure, optional particles may be reduced, but a required hazard summary remains available.
- Visualize realizes the selected visual workload; the host presents it. No Renderer policy needs to know which graphics API is used.

This scenario forces the architecture to answer hard questions: what is authoritative, what is derived, what is stale, what can be aggregated, which dependencies are invalidated, and what evidence proves the chosen policy is both cheaper and adequate?

## Instructor and contributor exercises

1. **Observer contrast:** Define two observers of the same world. List which facts and fidelity each requires, and explain the difference without changing authoritative state.
2. **Hysteresis:** Design a deterministic input trace that would make a single-threshold policy oscillate. Add a stable transition rule and test it.
3. **Budget fallback:** Given a CPU budget and an interaction-critical target, identify which presentation dimensions may degrade and which may not.
4. **Freshness:** Construct a case where an occluded entity remains semantically relevant. Specify the metadata needed to prevent stale information from appearing current.
5. **Dependency invalidation:** Change a light source while leaving geometry unchanged. Identify which derived work becomes stale and which can be reused.
6. **Benchmark falsification:** Propose a workload where total entity count rises but observer-relevant count stays constant. State the measurements that would support or refute the scaling hypothesis.
7. **Boundary review:** Trace one Experience from authoritative state to GUI interaction and Visualize output. Identify any place where presentation accidentally becomes authoritative.

## Implementation traceability

Use these existing guides as the source of implementation detail and evidence:

- [Start Here](START_HERE.md) — orientation and quick proof.
- [Event Horizon Theory](EVENT_HORIZON_THEORY.md) — focused explanation of horizons and transitions.
- [Architecture](ARCHITECTURE.md) — ownership and dependency direction.
- [Event Horizon buckets](EVENT_HORIZON_BUCKETS.md) — grouping and refresh concepts.
- [Observation model](OBSERVATION_MODEL.md) and [Semantic observation](SEMANTIC_OBSERVATION.md) — observer and structured knowledge.
- [Presentation handoff](PRESENTATION_HANDOFF.md) — neutral handoff and source-rectangle invariant.
- [GPU boundary](GPU_BOUNDARY.md) — frame description versus execution.
- [Benchmarking](BENCHMARKING.md) — measurement conditions and interpretation.
- [Theory-to-implementation map](THEORY_IMPLEMENTATION_MAP.md) — public contracts, focused tests, limitations, and the next vertical proof.
- [Implementation roadmap](IMPLEMENTATION_ROADMAP.md) — remaining integration and proof milestones.

For every public capability, the implementation record should eventually name the owning type/member, input contract, output contract, tests, sample, benchmark (where meaningful), limitations, and consuming repository. If one of these is absent, record it as a gap rather than implying completeness.

---

<p align="center">
  <img src="https://avatars.githubusercontent.com/u/16405167?v=4" width="48" alt="The Singularity Workshop maintainer avatar">
  <br>
  <a href="https://github.com/TrentBest">GitHub</a> ·
  <a href="https://github.com/TrentBest/TheSingularityWorkshop.Renderer">Renderer</a> ·
  <a href="https://www.nuget.org/packages/TheSingularityWorkshop.Renderer">NuGet</a> ·
  <a href="https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/DOCUMENTATION_STANDARD.md">Workshop documentation standard</a>
</p>
