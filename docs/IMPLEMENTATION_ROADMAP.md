# Renderer Implementation Roadmap

> Working plan for taking TheSingularityWorkshop.Renderer from its current executable foundations to a coherent, measurable observer-relative rendering system. This document distinguishes documented implementation from intended work; it is not a claim that the full pipeline already exists.

## North-star contract

**Renderer decides what an observer needs represented and what computational work is justified. A presentation backend manifests that result.**

The world and experience own authoritative meaning/state. Renderer owns observer-relative representation and computational policy. FSM_API supplies stateful computation and scheduling where appropriate. Visualize owns presentation execution. GUI.SVG is one useful presentation/inspection path, not a dependency of Renderer. FSM_COS may compose capabilities but Renderer must not depend upward on it.

The central hypothesis is falsifiable: as world population grows, the work required to serve an observer should track observable consequence and required information more closely than total population. Avoiding work is not a success if the observer receives an incorrect, stale, unstable, or unreconstructable representation.

### Perceptual continuity and the computation budget

The optimization target is not simply fewer draw calls or fewer polygons. It is a convincing, coherent world under a finite computation budget. Renderer should spend work according to what the observer can notice, what could change their interpretation or actions, and what must remain consistent with authoritative state.

- **Spend detail where attention and consequence concentrate.** A tiny, nearby effect can deserve more work than a vast distant district if the observer is looking directly at it or interacting with it.
- **Reduce work without making the world feel dead.** Lower-priority activity may be aggregated, approximated, cached, or updated less often, while retaining enough motion and change to preserve the impression of a living world.
- **Cadence is a perceptual decision, not a universal timer.** Required refresh frequency depends on motion, contrast, visibility, proximity, interaction, causal importance, and the likelihood that stale output will be noticed. Different systems and representations can have different clocks.
- **Never confuse cheap output with correct output.** Reused work must remain valid for its source revision and observer needs. When the budget changes, promote important content and demote less important work without changing world identity or authoritative state.
- **Treat sudden observer changes as invalidation events.** Looking directly at an object, approaching it, selecting it, entering a space, or switching from a city plan to street-level observation can sharply change what work is justified.
- **Measure the illusion and the cost together.** Record work requested, work performed, work reused or skipped, allocations, elapsed time, freshness, and visible/semantic correctness. A faster frame that visibly breaks continuity is not a win.

This is a policy objective, not a claim that these signals or perceptual guarantees are already implemented. Start with deterministic scenarios and explicit observer/workload traces; use measurements to decide whether string-backed FSM dispatch, scheduling, geometry generation, frame assembly, or presentation is the actual bottleneck. Integer-backed FSM state is a promising optimization path if measurements justify it, but it should be evaluated against a representative end-to-end workload rather than a single isolated timing.


## Current baseline from repository documentation

The development README describes executable foundations for Event Horizon selection and buckets, observer geometry, semantic anchors, obstruction/sensor visibility contracts, and FSM-backed computational representation state. It also explicitly says some handoffs remain policy contracts or exploratory paths rather than one complete integrated pipeline.

The repository documents a platform-neutral GPU input boundary (RenderView, RenderDrawCommand, RenderFrame) and a RendererMicroBundle adapter. Its README directs users to tests as the quick executable proof and to Visualize's Soldier Field WebGPU lab as an exploratory path, explicitly not an end-to-end integration of the .NET Visualize contracts with Renderer policy.

Benchmark documentation records a 64-byte allocation for SelectEventHorizon and identifies population scaling, FSM scheduling, promotion/demotion, deterministic reconstruction, residency, visibility/occlusion, and complete observer workloads as future measurements.

These are baseline statements to preserve and verify against implementation and tests. Do not silently promote a conceptual diagram or browser illustration into an integrated feature.

## Priority 0 — Establish an auditable implementation map

- [ ] Inventory every public type and its ownership: observer/context, horizon policy and selection, buckets, representation, visibility/obstruction, semantic anchors, FSM computation, GPU input contracts, and MicroBundle adapter.
- [ ] For each capability, record implemented behavior, entry point, tests, known limitations, benchmark evidence, and consumers.
- [ ] Trace the real path from entity + observer input to horizon decision, representation state, due-work decision, selected output, and presentation handoff.
- [ ] Identify which stages only exchange contracts and which are actually called by the same end-to-end path.
- [ ] Reconcile README, theory pages, XML documentation, tests, and benchmark claims with that inventory.
- [ ] Label capabilities as implemented, experimentally demonstrated, planned, or speculative.

**Exit criteria:** a new contributor can follow a real source-and-test path without mistaking a conceptual diagram or browser illustration for integrated execution.

## Priority 1 — Make Event Horizon decisions complete and stable

- [ ] Define the minimum observer and candidate-entity input contracts, including coordinate frame, units, validity, and missing/unknown data.
- [ ] Specify the policy result explicitly: selected horizon/representation, decision reason, cadence or due-work policy, and relevant invalidation/refresh information.
- [x] Add a distance-only `RepresentationHorizonRule`/`ObserverRepresentationPolicy` bridge and retain the selected horizon identity for downstream bucket lookup. Boundary, ordering, invalid-input, and far-range fallback behavior are covered by tests; non-distance signals and hysteresis remain open.
- [ ] Test boundary values, invalid inputs, ties, policy overrides, and non-distance signals such as visibility, observer intent, semantic importance, interaction state, and budget.
- [ ] Implement and test hysteresis/temporal stability so small observer movements do not cause repeated promotion/demotion.
- [ ] Verify that distance is never treated as a hard visibility cutoff: distant context can remain observable at lower cost.
- [ ] Keep horizon counts and thresholds policy-driven rather than baking a fixed number of levels into core assumptions.
- [ ] Investigate the documented 64-byte allocation in SelectEventHorizon before speculative micro-optimization.

**Exit criteria:** selection is deterministic for the same inputs and policy, boundary behavior is tested, transitions do not oscillate unexpectedly, and measured output explains why the chosen representation is appropriate.

## Priority 2 — Complete Event Horizon buckets, cadence, and invalidation

- [x] Implement stable per-domain bucket membership, member transfer, and invalidation of affected buckets; `docs/EVENT_HORIZON_BUCKETS.md` documents the current model and regression scenario.
- [ ] Specify and verify how processing cohorts map to FSM_API definitions and processing groups without introducing a competing Renderer scheduler.
- [ ] Address the documented heterogeneity constraint: instances sharing one FSM definition share definition/bucket scheduling characteristics. Determine the cleanest FSM_API-supported lane/cadence model before designing a Renderer-side workaround.
- [x] Model selection changes, source revisions, cache invalidation, refresh due/not-due, and valid cached reuse as separate conditions.
- [x] Connect distance-based policy to FSM-backed representation state and the selected horizon bucket's refresh ticket/reuse decision through `ObserverRepresentationCoordinator`. It returns work decisions but leaves actual output generation and ticket completion to the host.
- [x] Implement dependency invalidation, refresh tickets, explicit reuse counters, and safe representation handoff in the bucket model; retain end-to-end integration verification as a separate task.
- [ ] Prove that a new display frame does not automatically force every horizon to recompute in a connected observer-to-presentation workload.
- [ ] Test event-triggered refresh alongside cadence-triggered refresh in the integrated FSM path.
- [ ] Measure scheduler and bucket-transfer costs independently from representation work.

**Exit criteria:** unchanged distant content can be reused safely; relevant changes invalidate the correct work; refresh is bounded by policy; bucket membership and scheduling remain consistent during migration.

## Priority 3 — Turn representation selection into an end-to-end CPU proof

- [ ] Build a minimal deterministic workload with entities, one or more observers, several representation choices, and configurable horizon policy.
- [ ] Run the actual selection → FSM-backed representation state → scheduled evaluation → selected output path.
- [x] Establish a first cross-repository CPU proof from Renderer distance/horizon selection and FSM representation state through `RenderFrame`, the Visualize adapter, `Visualizer`, and GUI.SVG. See [Visualize integration proof PR #12](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/pull/12). This proof does not yet include bucket scheduling and does not satisfy the full Priority 3 exit criteria.
- [ ] Demonstrate promotion and demotion while preserving entity identity and authoritative state.
- [ ] Report work requested, work performed, work reused/avoided, allocation, and time per stage.
- [ ] Include a baseline that performs comparable work without observer-relative reduction.
- [ ] Test multiple observers whose needs differ; one observer's representation must not become the world's authoritative representation.
- [x] Verify independent coordinator instances can hold different active representations without sharing FSM state; a full multi-observer rendering workload remains open.
- [ ] Test visibility/obstruction contracts against the representation decisions that consume them.
- [ ] Ensure semantic observation can be produced without requiring a visual mesh or GPU.

**Exit criteria:** a reproducible test demonstrates correct observer-relative decisions and reports measured reduction against an equivalent baseline.

## Priority 4 — Deterministic reconstruction and semantic identity

- [ ] Specify the stable identity/placement/seed/policy inputs required to reconstruct richer detail.
- [ ] Test repeatability across promotion/demotion cycles and independently created instances.
- [ ] Measure memory saved versus reconstruction time and allocation.
- [ ] Evaluate Squirrel3 and alternatives experimentally; do not hard-code a choice before evidence.
- [ ] Define semantic-anchor lookup and attachment behavior, including unknown protocol symbols and version/identity boundaries.
- [ ] Keep ProtocolAi as a source of protocol identity, not a place where Renderer-owned meaning or presentation behavior is hidden.
- [ ] Verify visual, semantic, interaction, simulation, and update cadence detail can vary independently where policy requires it.

**Exit criteria:** detail can be discarded and reconstructed from stable inputs, semantic identity survives representation changes, and the memory/CPU trade is measured.

## Priority 5 — Make the GPU input contract honest and useful

- [ ] Audit RenderView, RenderDrawCommand, and RenderFrame for ownership, mutability, units, coordinate conventions, resource identity, lifetime, and validation.
- [ ] Define what a frame snapshot guarantees and what it deliberately does not guarantee.
- [ ] Test missing geometry/material/instance resources, invalid ranges, transforms, and cancellation/failure boundaries where applicable.
- [ ] Ensure only work surviving Renderer policy enters the output frame.
- [ ] Benchmark frame construction, command count, allocations, and memory footprint independently.
- [ ] Keep native graphics objects and API-specific synchronization out of Renderer core.
- [ ] Treat hardware discovery and vendor specialization as optional execution capabilities behind the separate Computation/provider boundary, not mandatory Renderer dependencies.
- [ ] Do not describe a neutral frame contract as a completed GPU backend.

**Exit criteria:** the frame is a stable, documented, testable boundary with a clear route to a backend, and its costs can be measured separately.

## Priority 6 — Prove the living SVG documentation path with real Workshop data

SVG is valuable because it can be both a compact artifact and a visible, inspectable scene. It should expose real contracts and behavior, not become a second implementation of Renderer logic.

- [x] Add a runnable .NET living-documentation sample that advances real `RendererComputationMachine` instances through FSM_API and emits the resulting representation states as SVG. The SVG pulse is presentation-only; the state values are runtime outputs.
- [x] Add a visual architecture explainer for observer → Event Horizons → independent buckets → RenderFrame/host, with explicit labeling that it is conceptual animation rather than runtime telemetry.
- [ ] Use the existing Visualize → GUI.SVG proof as the reference pattern: real C# scene/frame data enters a public API, then an adapter emits SVG.
- [ ] Extend the first proof to consume actual Renderer outputs—observer context, horizon/bucket membership, representation decisions, cadence/refresh state, and selected draw commands as integration matures.
- [ ] Keep Renderer independent of GUI.SVG and Visualize. Put SVG conversion in an adapter/consumer.
- [ ] Color-code the data and API handoff: source/model inputs, actual public API consumption, and SVG manifestation must be visually distinguishable.
- [ ] Generate committed SVG artifacts from reproducible C# proof programs and have CI regenerate/compare them. The first FSM-state snapshot now has a generator and a CI freshness check; extend the same pattern to frame commands and richer observer workloads. Label hand-authored or browser-only animation as illustrative.
- [ ] Show Event Horizons as visible regions/bands, an observer, entities changing representation, and counters that change with actual decisions.
- [ ] Add useful SVG interaction: links to source/contracts, titles/descriptions, and explicit browser-lab controls for selecting observers/policies or inspecting entities. Distinguish user-driven lab controls from live Renderer execution.
- [ ] Keep animation presentation-only unless real state updates are explicitly wired through the relevant Workshop API. Never imply SVG animation itself executes FSM_API.
- [ ] Support reduced-motion preferences, keyboard-accessible controls/links, readable contrast, accessible names/descriptions, and a meaningful static view.
- [ ] Test emitted SVG for valid XML, unique IDs, safe generated attributes, and stable output.
- [ ] Include generated visual evidence in README/docs and relevant package artifacts where useful; do not make Renderer core depend on a browser or GUI package.

**Exit criteria:** a reader can inspect a living Renderer diagram, follow displayed decisions back to source and tests, and distinguish real API output from presentation-only animation.

## Priority 7 — Benchmarks that can falsify the design

- [ ] Keep Renderer.Benchmarks separate from the core package.
- [ ] Record hardware, OS, runtime, benchmark version, source/package revision, parameters, allocation, and clean-run status.
- [ ] Measure selection across population and horizon count; FSM scheduling; bucket migration; promotion/demotion; reconstruction; semantic lookup; visibility/occlusion; frame construction; and complete observer workloads.
- [ ] Compare full-detail/every-frame baselines with observer-relative policies under equivalent correctness requirements.
- [ ] Report total and per-stage work, allocation, cache reuse, active representations, emitted commands, and avoided evaluations.
- [ ] Test worst cases: many observers, high churn at boundaries, all entities visible, rapid changes, and expensive promotion bursts.
- [ ] Treat million/hundred-million synthetic runs as scalability experiments, not proof of production performance.
- [ ] Keep benchmark claims proportional to measured evidence; do not infer whole-renderer superiority from microbenchmarks of mathematical primitives.

**Exit criteria:** results show where the model wins, where overhead dominates, and which next engineering change is justified.

## Priority 8 — Workshop integration and package quality

- [ ] Preserve dependency direction: Renderer may consume FSM_API, ProtocolAi, and MicroBundleDomain as designed; it must not depend on FSM_COS, Visualize, GUI.SVG, a host UI, or a specific graphics API to exist.
- [ ] Verify RendererMicroBundle exposes the capability without moving Renderer-specific logic into FSM_COS.
- [ ] Test the normal NuGet dependency path and opt-in sibling-source integration path.
- [x] Configure the package to include README.md, LICENSE, every Markdown guide under docs/, and both the animated moniker and FSM-backed living Event Horizon SVG. The CI workflow regenerates the living SVG and checks for a clean diff before packing; confirm the resulting nupkg contents from a successful CI artifact before release.
- [ ] Check that README image links work both on GitHub and when the README is consumed from the package.
- [ ] Keep build/test/coverage/package CI green and NuGet publishing gated off unless explicitly authorized.
- [ ] Test documented commands from a clean checkout and keep quick-start claims scoped to what those tests prove.

**Exit criteria:** the package is independently consumable, integration boundaries are tested, docs and packaged assets agree, and no release/deployment occurs without explicit authorization.

## Recommended execution order

1. **Audit and map** the real public contracts, tests, and integration points.
2. **Close correctness gaps** in Event Horizon selection, bucket migration, invalidation, and temporal stability.
3. **Prove one end-to-end CPU path** and measure avoided work against a baseline.
4. **Stabilize the neutral frame contract** and benchmark command production.
5. **Extend the FSM-backed SVG snapshot** to show real observer decisions and selected frame commands, then connect the proof to Visualize → GUI.SVG.
6. **Use benchmark results** to prioritize reconstruction, visibility, scheduling, and scaling improvements.
7. **Integrate and package** only after contracts, tests, generated evidence, and docs agree.

## Definition of “proper and full implementation”

Renderer is not complete merely because it has many types, a GPU-shaped contract, a MicroBundle adapter, detailed theory, or a compelling SVG.

A credible full implementation demonstrates that:

- observer-relative policy produces correct, stable decisions;
- representation state and cadence follow those decisions through FSM_API without a duplicate scheduler;
- unchanged work can be reused and relevant changes are invalidated;
- promoted detail can be reconstructed or materialized safely;
- the output frame contains the work that survived the policy;
- at least one consumer can present that output, with SVG as an inspectable proof and a real GPU adapter remaining a separate milestone;
- benchmarks measure savings and overhead against equivalent baselines;
- public contracts, tests, docs, generated artifacts, and NuGet contents describe the same reality.

This roadmap is intentionally staged. The immediate goal is not to implement every possible GPU path; it is to make one observer-relative rendering path correct, observable, measurable, and demonstrably useful, then expand from evidence.

## Singularity City vertical slice

The first concrete large-world consumer is Singularity City, integrated with Visualize and hosted by WebPage. The cross-repository ownership contract and staged evidence gates are maintained in [Visualize's Singularity City rendering handoff](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/blob/development/docs/integrations/SINGULARITY_CITY_RENDERING_HANDOFF.md).

**Current integration step:** Visualize's `integrations/GuiSvg/CityMassingProof` has been changed to calculate face centers, ask `ObserverRepresentationPolicy` for each face's representation, assemble a Renderer `RenderFrame`, and pass it through `RendererFrameAdapter` before GUI.SVG submission. This is the first code-level city consumer of the Renderer policy, beyond the earlier specimen-only proof. The commits are on Visualize `development`; build/CI verification is still pending, and this does not yet prove geometry reduction, refresh scheduling, stable entity identity, or GPU execution.

### Renderer-owned work

- [ ] Use the existing city-massing scene as a real observer-policy consumer; do not replace the current specimen-only Renderer-to-Visualize proof and call it a city integration.
- [ ] Specify the minimum observer input and city-candidate representation inputs, including coordinate frame, units, source revision, stable identity, and invalid/missing-data behavior.
- [ ] Connect selected representation and due-work/refresh decisions to actual city frame assembly using the existing bucket and platform-neutral frame contracts.
- [ ] Preserve city entity identity across representation changes, material skins, observer changes, and view scales.
- [ ] Test observer-dependent selection, bucket refresh/reuse, stale-revision rejection, deterministic frame content, and unaffected cache reuse.
- [ ] Keep world authoring and city rules in the Experience/WebPage domain; Renderer must not become authority for roads, transit schedules, Digiten behavior, or physics.

### Evidence gate

The milestone is complete only when a deterministic city scene passes through actual Renderer observer/representation decisions and into Visualize's public frame/backend path, with tests and a CI-generated inspectable artifact. A CPU-side SVG artifact proves the selected frame reached GUI.SVG; it does not prove GPU execution, full street view, persistence, or a living simulation.

The later sequence is plan-view navigation, spatially coherent district detail, aerial/street representations, live domain-state integration, and measured scaling against observer-relevant work. GPU backend selection and hardware performance claims remain separate evidence gates.



## Natural migration proving ground — impossible-scale monarch phenomenon

The next complementary scene should be a natural environment occupied by a monarch butterfly migration at a deliberately impossible population: far beyond what nature could sustain. The purpose is not to claim ecological realism at that density. It is to create an experiential instrument for understanding a phenomenon that is lost or inaccessible at ordinary human scale: the observer enters a landscape and discovers a living migration that exceeds anything they have seen.

This is a proposed proving ground, not a currently implemented feature. It should stress the same Renderer/Visualize contracts as Singularity City while exercising motion, occlusion, changing density, light, depth, and attention in ways static building massing cannot.

### What the proving ground must test

- **One coherent population, many representations.** Each butterfly or deterministic cohort has stable identity/seed and reproducible state. Near individuals can reveal wing shape and phase; mid-range groups can preserve directional flow and density; distant swarms can become spatial/temporal aggregates without looking like a flat billboard or disappearing.
- **Observer-relative detail.** Looking toward a sunlit swarm, approaching a tree, or tracking one butterfly should promote the relevant details while the migration continues around it. Attention may prioritize a tiny wing or dust/pollen motes in a sunbeam above a much larger low-priority region.
- **Temporal cadence without a frozen world.** Wing phase, flight path, cohort flow, lighting response, vegetation, and ambient motion need not share one update rate. Lower-priority work may be reused or reconstructed, but motion must remain temporally coherent when the observer turns, approaches, or changes intent.
- **Depth and partial occlusion.** Butterflies should pass through foreground, middle distance, and background, overlap vegetation and terrain, and reappear consistently. A representation transition must not expose a seam, duplicate a cohort, or reverse its flow.
- **Illumination as a concentrated budget.** Sunbeams, translucency, wing shimmer, and airborne particles are candidate high-cost details to allocate only where the observer can perceive them. First establish a deterministic CPU reference; then compare alternative implementations using the same scenario and correctness gates.
- **Scale beyond individual simulation.** Compare per-individual work, deterministic cohort/flow representations, and hybrid approaches. Aggregation is only valid when it preserves the cues the current observer can perceive; it must not become a count-only optimization that destroys the phenomenon.
- **Reproducible evidence.** Given a seed, world state, observer trace, and policy, the same decisions and scene output should be reproducible. Record population represented, entities evaluated, individuals simulated, cohorts updated, geometry/instances emitted, refreshes, reuse/skips, allocations, and per-stage elapsed time.

### Staged proof, not a premature production claim

1. **Static density field:** deterministic terrain/vegetation and a spatial distribution of migration cohorts; establish stable IDs, units, bounds, and repeatable output.
2. **Animated CPU reference:** wing phase and trajectories for a manageable sample, with a much larger population represented by deterministic cohorts; measure the full update-to-frame path.
3. **Observer trace:** compare overview, walking through the swarm, looking into a sunbeam, and tracking a single butterfly. The same authoritative migration state must support every view.
4. **Representation transitions:** prove promotion/demotion, source-revision invalidation, valid cached reuse, and no visible discontinuities at the same observer trace.
5. **Scale experiments:** increase population by orders of magnitude and report actual work rather than inferring savings from the requested population size.
6. **Backend comparison:** only after the CPU path and its measurements are credible, compare CPU and GPU manifestations of the same deterministic workload.

A butterfly count is not itself the success metric. The gate is whether observer-relevant work grows more slowly than total represented population while motion, identity, density, depth, and visible transitions remain convincing. If it fails, the measurements should tell us whether the limiting factor is FSM dispatch, policy/scheduling, simulation, geometry generation, frame assembly, or presentation.

Keep ecological truth separate from the artistic premise: this is intentionally impossible-scale spectacle inspired by monarch migration, not a claim that such a population is naturally sustainable.
