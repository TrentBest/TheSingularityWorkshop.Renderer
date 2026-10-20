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


## 13. Natural phenomena stress the observer model

The Renderer domain must be broad enough for natural phenomena whose parts evolve at different rates and whose importance changes with observer position, intent, and environmental state. These are not simply collections of particles.

| Phenomenon | Potential representations | Horizon and correctness considerations |
| --- | --- | --- |
| Volcanic eruption | source event, near-vent ejecta, lava flow, ash/gas field, regional plume, persistent cooled deposits | preserve eruption/source identity and flow/deposit state while far-field appearance aggregates |
| Fire / wildfire | ignition event, local flame front, ember trajectories, smoke field, regional heat/smoke summary, burned-area state | flame visuals may fade before residual heat/smoke or burned terrain state; suppressing detail must not stop the fire model |
| Lightning / electrical storm | strike event, branching path, brief light contribution, cloud flash, thunder arrival event | immediate latency and temporal stability matter; light response spans far beyond the visible bolt |
| Severe weather | local wind/rain/snow, storm cells, cloud field, flood/visibility summaries | simulation and visual update rates can differ; observer-critical hazards need timely summaries |
| Rivers, waves, floods, waterfalls | local surface/foam/spray, coarse flow field, shoreline/flood extent, underwater volume | use high detail near interaction/camera and regional state for routing or hazard decisions |
| Landslides / avalanches / earthquakes | fracture/impact events, debris trajectories, dust plume, displaced terrain, regional event summaries | persistent geometry/state must outlive transient debris and dust; impact semantics remain authoritative |
| Aurora, eclipse, meteor shower | sky-scale field, high-salience transient, observer-dependent celestial alignment | large projected coverage can justify work despite distance; multi-view/camera consistency matters |
| Geysers, fumaroles, steam vents | pressure-release event, jet, droplets, steam volume, wet/mineral deposits | source event, field transport, and persistent deposit have independent lifecycles |
| Ocean/underwater | surface waves, caustic approximation, bubbles, turbidity, current field, depth-dependent visibility | medium changes lighting and visibility; particle count alone cannot represent water's optical state |
| Biological/environmental clouds | spores, pollen, insects, seed dispersal, fungal gas, dust | retain source identity and hazard/interaction semantics while aggregating visible populations |

These examples define stress cases for representation policy, not scientific models implemented by Renderer. The domain owner supplies state and confidence; Renderer selects consumer-appropriate representations and fidelity.

## 14. Lighting and illumination are cross-cutting phenomena

Renderer should express the need for lighting work and the relevance of its dependencies without owning shader code or light transport implementation. Illumination can be a scene-wide field, a local interaction, a transient event, or an observer-sensitive post-process. It can also affect other representations, not just pixels.

Examples:
- **Volcanic glow:** near-field lava detail, emissive surface, local light contribution, reflection in nearby water, heat shimmer, and a regional glow can have different update budgets.
- **Lightning flash:** one event triggers bolt presentation and a brief illumination change across clouds and terrain; temporal accumulation must not create a stale flash.
- **Fire:** visible flames, emissive embers, shadow-casting behavior, flicker, smoke, and persistent scorched surfaces may use different horizons.
- **Fog or ash:** the medium changes scene visibility and light transmission even if its individual particles are aggregated or not drawn.
- **Sun shafts and cloud shadows:** the effect depends on light direction, media/occluder structure, view, and update cadence; a horizon decision must account for large screen coverage.
- **Underwater scenes:** depth, turbidity, surface waves, bubbles, light attenuation, and caustic-like patterns form a coupled composition.
- **Wetness, snow, ash, soot, and cooled lava:** surface state persists independently of the particles or event that created it and can alter later lighting/material work.
- **Eclipses, sunrise, twilight, aurora:** celestial or atmospheric events may be low-frequency but globally visible, making simple distance-based culling inappropriate.

The policy interface should be able to express dependency and invalidation: a changed light may invalidate shadows or temporal lighting history; a changed density field may invalidate volume integration; a camera cut may invalidate view-dependent history; a surface-state update may invalidate material/lighting caches. The exact invalidation graph belongs to the implementation and must be measured.

## 15. Representation budget is a vector, not a single quality number

A phenomenon's budget should distinguish at least:
- observer-visible representation and projected coverage;
- authoritative domain update requirement (reported by the owner, never inferred from visual LOD);
- CPU selection/preparation and allocation cost;
- GPU execution, bandwidth, and synchronization cost;
- memory residency and reconstruction cost;
- data freshness, latency deadline, and tolerated staleness;
- temporal history and invalidation cost;
- interaction/query precision needed by each consumer;
- optional audio/haptic/semantic observation requests;
- uncertainty, approximation error, and fallback quality.

A single “low/medium/high” quality value hides important contradictions. For example, a distant volcanic ash plume can be visually coarse but still require a current regional visibility estimate for navigation. Conversely, a close decorative ember may need crisp pixels but have no authoritative interaction requirement. Track these axes independently and explain which constraint caused each reduction.

## 16. Required natural-phenomena proof suite

The following suite is proposed; it is not evidence of existing implementation.

1. **Event fan-out:** one source event requests visual, illumination, environmental-field and optional sensory products through independent capability contracts.
2. **Persistent aftermath:** eruption/fire/impact creates a durable surface-state change that remains after transient particles expire.
3. **Field continuity:** plume or water field remains coherent when representation changes between near, medium and far horizons.
4. **Lighting invalidation:** moving light, flash, density change, camera cut and material-state change invalidate only required derived data.
5. **Multi-observer consistency:** nearby player, distant camera and AI/map observer receive different representations derived from the same authoritative revision.
6. **Crowd stress:** dozens to thousands of sources share compatible resources and batches while source identity and field provenance remain inspectable.
7. **Budget conflict:** inject GPU pressure and verify graceful visual degradation without suppressing domain updates or critical observer information.
8. **Reconstruction:** unload/reload a region and rebuild representations from durable state without persisting device objects.
9. **GPU evidence:** prove actual submitted GPU work and report CPU selection, upload, synchronization, GPU execution, memory and presentation separately.
10. **Modular composition:** remove volumetrics, particles, advanced lighting, or post-process independently and verify the experience retains a declared fallback.

The goal is not to maximize every effect at all times. It is to support the full vocabulary of natural phenomena and choose the strongest correct representation that the current observer and hardware can afford.
