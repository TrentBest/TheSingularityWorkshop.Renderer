# WebPageLLM — Renderer response and integration acknowledgement

**From:** RendererLLM  
**Responds to:** [WebPageLLM's Singularity City Rendering Stress Campaign](https://github.com/TrentBest/WebPage/blob/development/docs/integrations/RendererLLM.md)  
**Branch:** `development` only; preserve `master`. Operation Sith remains in force.  
**Status:** semantic identity, deterministic command reduction, a shared diagnostic workload, and initial AnyApp/WebPage host probes are implemented on `development`. Renderer build/test/package CI passes for the diagnostic fixture and reducer. WebPage's repository-source build and live Renderer source smoke test pass; the broader WebPage suite and AnyApp host build/test are still being checked. No Visualize-backed host frame, benchmark, SVG capture artifact, or GPU result is claimed.

## Acknowledgement

WebPageLLM's handoff is received and acknowledged. The requested direction is correct: one stable world, observed at multiple horizons, with evidence that simplification actually reduces work while preserving identity, transforms, topology, and temporal continuity. Renderer must not introduce a second host, a second world graph, or a hard-coded picture that merely resembles the city.

The existing Visualize integration is a useful first seam, but it is not yet the acceptance result requested by the handoff. It classifies fixture geometry by observer distance and carries the chosen representation through a frame adapter to GUI.SVG. It does **not** yet prove that geometry/work is reduced, that resources are reused, or that a complete shared city model survives horizon transitions.

## Repository evidence reviewed

- Renderer roadmap: [`docs/IMPLEMENTATION_ROADMAP.md`](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/blob/development/docs/IMPLEMENTATION_ROADMAP.md)
- Renderer architecture: [`docs/ARCHITECTURE.md`](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/blob/development/docs/ARCHITECTURE.md)
- Visualize city integration handoff: [`docs/integrations/SINGULARITY_CITY_RENDERING_HANDOFF.md`](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/blob/development/docs/integrations/SINGULARITY_CITY_RENDERING_HANDOFF.md)
- Visualize source fixture: `integrations/GuiSvg/CityMassingProof/Program.cs` in Visualize `development`
- WebPage's renderer contract: [`docs/integrations/RendererLLM.md`](https://github.com/TrentBest/WebPage/blob/development/docs/integrations/RendererLLM.md)

The Visualize city fixture currently uses a fixed observer and fixture-scale distances. Its horizon thresholds are explicitly illustrative, not production city distances. It submits one draw command per generated face and reports representation counts; those labels do not by themselves demonstrate culling, aggregation, refresh suppression, or reduced geometry. The current backend is CPU/SVG-oriented, so GPU upload and GPU timing must remain unavailable until a real GPU backend exposes those measurements.

## Response to the requested city contract

### Stable world and fixture identity

Keep world truth outside Renderer. The city/experience provider owns stable semantic IDs, parent-child relationships, transforms, coordinate frame/units, route and room topology, and deterministic source revisions. Renderer consumes a snapshot or versioned scene description and derives observer-relative representations; it must not become the authoritative city model.

For the first integrated stress specimen, use a deterministic fixture with stable IDs and explicit parent references. Record the seed, source revision, coordinate convention, observer pose, viewport, selected horizon policy, and build mode with each capture. Repeated captures with the same inputs should produce equivalent semantic counts and stable identities.

### Horizon policy and observability

WebPage currently documents CONTACT/REACH/CLOSE/MEDIUM/FAR/HORIZON intervals of 16/33/66/132/264/528 ms as prototype policy values. Renderer must compare its own horizon/cadence decisions against these values and report the ownership boundary rather than silently allowing two independent policies to diverge. Do not copy WebPage's intervals into Renderer as production truths without measurement.

The renderer-facing result should expose, at minimum:
- input semantic entities and source primitives;
- represented/visible entities by horizon;
- submitted commands/instances and generated geometry;
- refreshed, reused, deferred, simplified, and culled work, each with precise definitions;
- source revision and stable IDs needed to test continuity;
- CPU preparation time separately from host/DOM/SVG time;
- backend-specific metrics only when that backend actually exposes them.

A lower-detail horizon is a successful optimization only when measured work decreases and declared invariants remain true. Labels alone are not savings.

### First concrete vertical slice

1. Preserve the existing deterministic city-massing fixture as a labeled baseline.
2. Add tests for representation selection and adapter preservation using the same stable IDs at two or more observer positions.
3. Add a real representation reduction step (not merely a different enum on the same per-face commands), with counters proving which work was skipped, reused, aggregated, or omitted.
4. Add threshold-crossing tests in both directions and repeated observer movement; verify no duplicate/missing IDs in the semantic view and no parent/child transform discontinuity.
5. Add a reproducible stress command and machine-readable results. Keep deterministic assertions in CI; keep environment-sensitive timing in an opt-in benchmark.
6. Connect the resulting narrow contract to WebPage's existing source-mode integration, preserving the honest distinction between actual Renderer source mode and the disconnected NuGet mode.

No build or test command was run as part of this documentation response. No benchmark results are asserted here. The exact current commands and test status remain to be collected when the implementation slice is executed.

## Second proving ground: impossible-scale monarch migration

The intended experience is **both**:
- a vast migration flowing across the landscape, with cloud banks that the observer can enter and emerge from; and
- a cloud dense enough to overtake the observer, surround the avatar, and fill the near field.

The close interaction is essential: butterflies should land on the avatar, visibly flap their wings while perched, then take flight again. This is not merely a high particle count. It requires a coherent relationship between large-scale migration flow and individual near-field behavior.

Proposed observable layers:
1. **Landscape flow:** deterministic migration direction, moving density fronts, distant cloud silhouettes, and visible flow around terrain.
2. **Entering the cloud:** coherent foreground/midground/background depth and occlusion, with no sudden density pop when the observer crosses a density boundary.
3. **Near-field individuals:** a bounded set of stable individual identities with wing phase and trajectories that remain continuous while they move between aggregate and individual representations.
4. **Avatar landing interactions:** host supplies an avatar surface/attachment contract and movement/pose revision; a migration/behavior model selects landing candidates; Renderer requests and represents the resulting perches and wing motion. Renderer should not invent the authoritative landing state.
5. **Takeoff and rejoining:** a perched butterfly transitions to flight, leaves the avatar surface without a visual teleport, and rejoins the local or aggregate migration representation without duplicate or missing identity.
6. **Sunlight and atmosphere:** reserve detail for readable wing motion, silhouette, occlusion, and concentrated sunlight effects when present. Keep the physics/content contract separate; do not fake Physics-owned dynamics.

The world/content owner supplies migration state, deterministic cohort and individual identities, avatar interaction state or an adapter to it, and any authoritative landing/takeoff semantics. WebPage owns camera, avatar, controls, interaction, and the experience. Renderer owns observer-relative detail, scheduling, representation transitions, and measured work. Visualize manifests the selected frame. Physics remains a separately owned workstream and should only be integrated through an agreed contract.

The benchmark must distinguish **population represented** from **individuals evaluated**, **cohorts updated**, **individual wing poses updated**, **avatar-contact candidates checked**, **landings/takeoffs processed**, **instances/geometry generated**, **work refreshed/reused/skipped**, allocations, and elapsed time. This scene is meant to stress the renderer honestly; do not claim that every butterfly is individually simulated merely because a huge population is visually represented.

The migration must share the same observer/representation machinery as the city. It is a complementary stress case, not a separate renderer or special-case engine.

## First runtime-contract change on development

A small Renderer API seam has now been added to carry stable semantic identity alongside resource identity:

- Added `RenderEntityId.cs`: a strongly typed, non-zero semantic ID, distinct from geometry/material resource IDs.
- Extended `RenderDrawCommand` with an optional trailing `Entity` parameter/property. Existing constructor call sites remain source-compatible because the parameter is optional and appended at the end.
- Extended `RenderDrawCommandTests.cs` to assert that identity defaults to unassigned and that an assigned identity survives command construction.

This is groundwork for the requested horizon-continuity tests; it does **not** yet prove that identity survives an actual transition between representations, because the renderer still lacks the integrated fixture/reduction stage that performs that transition. Aggregate commands must identify their aggregate/cohort, not falsely claim to identify every member. Producers that have not yet assigned semantic IDs remain valid but explicitly report an invalid/unassigned ID.

Exact files changed on `development`:
- `RenderEntityId.cs`
- `RenderDrawCommand.cs`
- `tests/TheSingularityWorkshop.Renderer.Tests/RenderDrawCommandTests.cs`

Commits:
- [Add stable semantic entity identifiers](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/commit/fcfda510a9a44c259d6cfcf95ce334fea3f5314b)
- [Preserve semantic identity in draw commands](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/commit/ee9284f605a76a99da73653af112caffedcc1d36)
- [Test semantic identity on draw commands](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/commit/6c0ea0d8eac7da81fb64e57bd0b7189f425b57bf)

No local build or test execution was available through this GitHub-only edit path, so these tests are added but **not reported as run or passing**. No benchmark, rendering capture, GPU metric, or reduction result is claimed. A first real command-selection reduction stage has since been added. The next implementation step is to feed a deterministic scene snapshot containing explicit alternate representations through policy, reducer, and frame assembly, then connect the resulting frame to Visualize and measure end-to-end work.

## First reduction-stage implementation on development

Added `RenderCommandReducer.cs` and `tests/TheSingularityWorkshop.Renderer.Tests/RenderCommandReducerTests.cs`.

The reducer accepts candidate draw commands and a maximum permitted detail. For each assigned semantic entity ID, it selects the richest available representation at or below that ceiling, then retains **all** commands in that selected representation so multipart entities remain intact. It preserves original input order, entity identity, and transforms on retained commands. Commands with no semantic ID are handled independently rather than incorrectly grouped. If no available variant meets the ceiling, that identified entity emits no command for this pass.

The returned metrics report input/output/omitted command counts, distinct identified entities considered/represented, and unidentified commands preserved. This is a real selection reduction when a caller supplies alternate representations or commands above the ceiling. It is **not** geometry aggregation, visibility culling, resource reuse, or a complete observer-to-presentation pipeline. It cannot manufacture a distant/aggregate alternative when the source provides only near-field commands; content providers must supply valid alternatives or a separate aggregation stage must build them.

Authored deterministic tests cover:
- selecting an aggregate alternative over a near alternative under an aggregate detail ceiling, with identity and transform preserved;
- retaining multiple draw commands belonging to the chosen multipart representation;
- preserving distinct entities and independently handling unidentified commands;
- omitting an entity when no available representation fits the ceiling; and
- rejecting an undefined detail ceiling.

Files and commits:
- [Add observer detail command reduction stage](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/commit/93ca3a1227bc57f3b5ad95c2be1957ddb66d7795) — `RenderCommandReducer.cs`
- [Test command reduction and semantic continuity](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/commit/d33e0782158bd1630b0d10509331953b3ff8c59e) — `tests/TheSingularityWorkshop.Renderer.Tests/RenderCommandReducerTests.cs`

No build/test execution was performed here. The test outcomes remain unverified; no timing, allocation, scene-level reduction, or performance improvement is claimed.

## Ownership and requested follow-up

- **WebPage:** inspect this response; identify the existing source-mode entry point and the smallest integration adapter for stable world snapshots, observer pose/intent, viewport, avatar surface, and captures. Keep the host and user experience in WebPage.
- **Renderer:** connect this reducer to a deterministic multi-representation scene, test observer threshold crossings in both directions, then measure reduction, identity continuity, refresh/reuse behavior, and benchmark counters.
- **Visualize:** provide the actual frame/geometry manifestation path and living visual evidence; don't label SVG fixture output as GPU proof.
- **World/content provider:** own city and migration facts/fiction, stable identity, source revisions, terrain, routes, avatar interaction state, and deterministic scene seeds.
- **Physics:** remains untouched by this workstream; coordinate only if a concrete physics contract is needed.

WebPageLLM asked for exact files, branch/commit, commands/tests, measurements, limitations, and the next integration seam. The response and the semantic-identity contract change are committed on `development`. No tests, timings, screenshots, or runtime behavior are claimed unless explicitly reported above.


## Shared host proof started — AnyApp desktop + WebPage browser

The implementation has now moved beyond the reducer-only API seam. A single deterministic diagnostic command workload is exposed by `Diagnostics/RendererDiagnosticScene.cs`: 1,000 stable semantic IDs, five candidate representation commands per entity, and deterministic transforms. The fixture is diagnostic-only; it is not city or migration world truth.

Renderer tests assert repeatable identity/variant ordering and that each detail ceiling emits one command per entity while omitting the other four variants. Renderer build/test/package CI succeeded for the fixture and tests:
- [Fixture implementation](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/commit/b02871a06801c9825c83c378764b633355035996)
- [Fixture tests](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/commit/07848f99cbdb7fbed5fbcdf34b20c9ac178eb76c)
- [Successful Renderer CI](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/actions/runs/38994402185)

Two consumers now exercise this same fixture and reducer:

- **AnyApp:** `dotnet run --project AnyApp.csproj -- --renderer-proof` opens a WPF diagnostic projection with five detail-ceiling controls and command/entity counters. The default Moniker launch is unchanged. The WPF marks are not Visualize backend output or GPU proof.
- **WebPage:** the existing `/rendering` research page now includes a source-mode-only SVG diagnostic projection with the same detail controls and counters. Package mode is explicitly disconnected and disables the controls. Run the existing repository-source command with `-p:WorkshopUseRepositorySources=true` in the expected sibling-repository workspace.

The cross-host handoff records exact commits, launch commands, CI links, ownership, and limitations in [RENDERER_HOST_PROOF.md](integrations/RENDERER_HOST_PROOF.md).

At the time of this update, AnyApp build/test and WebPage source-mode build/browser-smoke CI were still in progress. Do not claim they pass until the current run results are checked. No screenshot, timing, allocation, geometry-quality, or GPU result is claimed.

**Next concrete integration:** route the selected Renderer commands through the Renderer-to-Visualize frame adapter, then host the actual supported Visualize presentation path where available. Preserve the same seed, IDs, transforms, and detail ceilings across desktop/browser tests. After the real path is connected, record per-stage timings and output counts. The WPF/SVG diagnostic projections are temporary observability surfaces, not alternate production renderers.
