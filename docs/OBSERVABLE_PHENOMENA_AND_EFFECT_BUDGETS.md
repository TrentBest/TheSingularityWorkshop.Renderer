# Observable Phenomena, Representation Budgets, and Event Horizons

**Status:** Renderer domain specification. This document extends the existing observer-relative model; it does not claim that every policy or GPU-assisted path is implemented.

## 1. Renderer is responsible for what computation an observer needs

Renderer sits between world/experience meaning and a concrete visual, semantic, or interaction representation. Its job is not merely to cull objects or select mesh LOD. It determines which computational representation is justified for an observer, under the experience's correctness rules and resource budgets.

Renderer owns:
- observer context and relevant view/intent metadata;
- Event Horizon policy and representation selection;
- representation promotion/demotion and temporal stability;
- observer-relevant visibility and detail requests;
- representation update policy and scheduling intent;
- aggregation and disaggregation policy where semantics permit;
- resource residency/streaming requests and dependency priorities;
- cost accounting and diagnostics for selected work;
- coherent visual and nonvisual observation products where supported.

Renderer does **not** own:
- authoritative physics, chemistry, biology, weather, or gameplay outcomes;
- authored scene persistence or story truth;
- visual material/shader implementation (Visualize);
- native graphics API or GPU-driver internals;
- audio playback or haptic device execution;
- the global composition OS or a mandatory frame loop.

## 2. One phenomenon, multiple computational representations

A phenomenon can be represented differently without changing its identity or meaning. A gas cloud might be:

- exact source and field state for a simulation;
- a coarse regional density/velocity field;
- a sparse set of occupied volume bricks;
- particles near the observer;
- a low-detail volume or impostor at medium range;
- a regional hazard summary for gameplay/AI observation;
- a semantic statement that a path is currently hazardous for a nonvisual observer.

These are not interchangeable truths. They are consumer-specific representations derived from an authoritative owner. Renderer selects the appropriate representation for a consumer; it does not invent authoritative fields to fill gaps.

Representation transitions should preserve stable identity, revision/tick provenance, and a documented relationship to the source state. Promotion can reconstruct detail from stable inputs; demotion can discard derived detail while preserving enough state to reconstruct it.

## 3. Observer relevance is not just distance

Selection may consider:
- distance and projected screen size;
- frustum, occlusion, and expected visibility;
- observer type (first-person, spectator, map, VR, remote camera, AI);
- current intent (inspect, navigate, target, interact, cinematic focus);
- semantic importance, threat, novelty, and event salience supplied by the experience;
- temporal change and motion prediction;
- interaction reachability and required response latency;
- uncertainty and confidence in the available representation;
- CPU/GPU/memory/network budgets and current contention;
- shared resources and opportunities for batching;
- the cost and risk of changing representation.

The policy must be explainable and measurable. Distance is a signal, not a universal truth. A tiny dangerous gas boundary may deserve more fidelity than a huge distant mountain because the observer is about to cross it.

## 4. Event Horizon dimensions

An Event Horizon may change independent dimensions rather than forcing one monolithic LOD level:

| Dimension | Example degradation/promotion |
| --- | --- |
| Geometry | full mesh → simplified mesh → impostor |
| Material | full material → reduced parameter set → flat color |
| Volume | dense near-field field → sparse bricks → regional envelope |
| Particles | detailed trajectories → pooled particles → statistical visual density |
| Animation | continuous → sampled → state transition only |
| Simulation request | fine local query → coarse aggregate → event-driven update |
| Interaction | precise affordances → coarse reachability → semantic-only |
| Semantics | full attributes → summary → existence/threat category |
| Update cadence | per-frame → fixed-rate → event-driven |
| Residency | resident → compressed/streamed → reconstruct-on-demand |
| Audio/haptics request | detailed cue → summarized event → omitted where optional |
| History | detailed recent trail → sampled history → aggregate statistics |

The exact horizons and thresholds are policy/configuration. They must not be hard-coded as universal distances or frequencies.

## 5. Environmental phenomena and effects

Renderer must be able to select work for both discrete events and continuously evolving phenomena.

**Discrete events** include release, impact, ignition, breakage, splash, explosion, landing, death, or a door opening. They often need immediate low-latency representation and then decay into cheaper residual state.

**Evolving fields** include gas density, velocity, fog, wind, water, temperature, humidity, pressure, illumination, and other spatial values supplied by an owning domain. Their representation can evolve independently of the event that created them.

**Persistent environmental changes** include wet ground, deposited ash, broken geometry, rot, stains, or altered terrain. These require stable domain state or authored references, not just a transient effect particle.

A phenomenon can combine all three. Renderer should avoid coupling the source lifecycle to the visual plume lifecycle: the gas release may stop while the cloud continues drifting; the cloud may visually dissipate while the authoritative hazard remains above threshold, or vice versa, according to the simulation and presentation contract.

## 6. Worked example: many gas-releasing fungi

The fungal hazard illustrates the desired boundary:

1. The organism/experience emits a release event and owns the sensing trigger.
2. Physics or a gas-domain capability owns source release, transport, mixing, and any defined field evolution.
3. Gameplay owns target exposure and consequences, or consumes an authoritative exposure result from the simulation.
4. Renderer evaluates observers and selects source/field detail: high-detail local plume near the player, coarser regional volume farther away, and semantic hazard summaries for AI/map observers.
5. Visualize renders the selected visual forms using shared resources and GPU-friendly batches.
6. Renderer may combine compatible visual contributions for presentation, but it must preserve source identities and never use the merged image as the hazard calculation.
7. When the observer moves away, high-detail visual resources may be demoted while the environmental model continues at its own required cadence.
8. When the observer returns, detailed appearance can be reconstructed from current field state and stable seeds; it need not replay every discarded puff.

This separates **simulation fidelity**, **visual fidelity**, and **observer information**. They influence each other through contracts but are not the same quality slider.

## 7. Aggregation and disaggregation rules

Aggregation can reduce work by representing many entities or cells as a shared region, volume, cluster, or statistical summary. It is valid only if it preserves the information required by each consumer.

Required safeguards:
- preserve source/entity identity in authoritative state;
- preserve spatial bounds, time/tick, revision, units, and provenance;
- specify the error or loss introduced by the aggregate;
- never use a presentation-only aggregate for exact collision, exposure, or gameplay outcomes;
- support promotion from aggregate to finer detail when demand rises;
- define how split/merge operations preserve totals and continuity where applicable;
- avoid temporal popping by blending representations or maintaining overlap during transitions where useful;
- expose diagnostics for aggregate coverage, stale data, and reconstruction failures.

For example, visual density from ten nearby gas sources may be composited into one field for shading, while the simulation still tracks separate sources and the game evaluates each target's exposure against its authoritative hazard model.

## 8. Budgets and scheduling

Renderer should produce explicit work requests or estimates that can be scheduled by the existing FSM/runtime substrate and the composed execution capabilities. It should not create a second universal scheduler.

Budgets may include:
- CPU time and allocations for selection/representation preparation;
- GPU execution time and command/dispatch count;
- memory residency, transient allocation, and upload bandwidth;
- latency deadlines for interaction-critical work;
- volume/particle resolution and error tolerance;
- streaming bandwidth and reconstruction cost;
- power/thermal constraints where exposed;
- synchronization and readback cost;
- stale-data tolerance and maximum acceptable representation age.

A work item should identify its priority/reason, estimated cost, deadline or cadence, dependencies, quality range, and fallback. Actual costs should feed measurement and policy refinement. Initial estimates are hypotheses until benchmarked on target hardware.

## 9. GPU boundary and division of labor

Renderer should reject unnecessary work before it becomes a GPU command, while allowing the GPU to perform highly parallel tasks where appropriate.

**Renderer / CPU-side policy:** observer context, semantic relevance, representation choice, promotion/demotion, dependency/residency requests, scheduling intent, budget and diagnostic metadata.

**Visualize / GPU-facing visual work:** geometry/material/volume/particle execution, shader and pipeline inputs, visual batching, light/shadow passes, field sampling for presentation, compositing, and image-space operations.

**Optional GPU-assisted Renderer paths:** occlusion queries, visibility compaction, indirect command generation, instance culling, or coarse screen-space estimation may be useful when proven. These are optimization mechanisms, not prerequisites and not permission to move authoritative simulation into Renderer.

A neutral RenderFrame or command packet proves only that CPU-side work was described. Real GPU execution requires an adapter, device, submitted commands, and verified output. Benchmarks must separate selection, CPU preparation, transfer, synchronization, GPU work, and presentation.

## 10. Stability, correctness, and observability

Representation policy must account for more than throughput:
- avoid rapid horizon oscillation using hysteresis, temporal smoothing, or transition overlap;
- ensure promotion does not change entity identity or authoritative outcomes;
- preserve required interaction latency even when a visual representation is cheap;
- communicate uncertainty or stale field data to consumers that need it;
- keep camera/observer changes from invalidating unrelated persistent state;
- expose why a representation was chosen, which dimensions were reduced, and which budget forced fallback;
- record selected representation, revision/tick, estimated cost, measured cost, and quality/fallback outcome.

## 11. Proposed proof suite

1. **Policy proof:** deterministic observer + entity inputs select the expected representation and explain why.
2. **Promotion proof:** demote then promote a procedural entity and verify stable identity and deterministic reconstruction.
3. **Field proof:** select near/medium/far representations for one evolving volume while keeping field provenance intact.
4. **Multi-observer proof:** first-person and AI observers receive appropriate but consistent representations of the same phenomenon.
5. **Aggregation proof:** many source fields produce a cheaper visual aggregate without changing authoritative per-source state.
6. **Temporal proof:** representation changes do not create unacceptable popping or stale-data contradictions.
7. **Budget proof:** measured CPU, memory, GPU, and latency costs are compared against estimates under increasing population.
8. **GPU proof:** a concrete backend demonstrates a real draw/dispatch and measures each stage independently.

These are proposed tests until code and measurements exist. Existing Event Horizon mathematics and benchmarks do not by themselves prove a complete visual renderer or GPU backend.

## 12. Acceptance principles

- Renderer chooses observable computation and representation; it does not own domain truth.
- Event Horizons are multidimensional computational boundaries, not just distance-based mesh LOD.
- Events, fields, and persistent environmental changes have distinct lifecycles.
- Aggregation is consumer-specific, provenance-preserving, and reversible where higher detail may be needed.
- Simulation fidelity, visual fidelity, and observer information remain separate concepts.
- Budgets are explicit and measurable; update cadence follows domain need and observer value.
- GPU acceleration is optional, measured, and behind the neutral model.
- Any implementation status claim is tied to code or a reproducible proof.

---

*The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.*
