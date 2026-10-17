# Rendering MicroBundle — Integration Findings

**Updated:** 2026-10-17  
**Owner:** Renderer / Visualize integration  
**Branch:** `development`  
**Status:** Contract discovered; rendering bundle and end-to-end optional-load proof not implemented.

## Verified contract boundaries

### MicroBundleDomain — capability contract

The canonical domain contract is owned by [TheSingularityWorkshop.MicroBundleDomain](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain). Its development documentation describes:

- `IMicroBundle` and `MicroBundleDescriptor` for executable identity and metadata;
- `MicroBundleDependencyRequest` for declared dependencies and optional configuration;
- `IMicroBundleLoadContext` for host-supplied runtime configuration;
- `IMicroBundleArbitrationContext` for inspecting the assembled composition;
- `Load(...)` and `Arbitrate(...)` as capability lifecycle participation.

The inspected `IMicroBundle` source declares `Descriptor`, `Id`, `Dependencies`, `Load(IMicroBundleLoadContext)`, and `Arbitrate(IMicroBundleArbitrationContext, int)`. It does **not** declare `IDisposable`, `Dispose`, `Unload`, or another teardown callback. Do not describe disposal as a MicroBundleDomain guarantee. The integration proof must inspect the actual FSM_COS `RuntimeAssembly` ownership/cleanup behavior; if it provides no cleanup hook, any rendering resources created by the adapter need an explicitly owned cleanup path outside the domain contract.

The contract does not define rendering, a GUI, storage, or the composition engine. A rendering capability should implement this contract without making MicroBundleDomain depend on Renderer, Visualize, GUI.SVG, a graphics API, or a host.

References:
- [MicroBundleDomain architecture on development](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain/blob/development/docs/ARCHITECTURE.md)
- [MicroBundleDomain getting started on development](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain/blob/development/docs/GETTING_STARTED.md)

**Version finding (source inspected 2026-10-17):** the MicroBundleDomain `development` project file `TheSingularityWorkshop.MicroBundleDomain.csproj` declares `<TargetFramework>net8.0</TargetFramework>` and `<Version>1.0.1</Version>`. Renderer `development` already references `TheSingularityWorkshop.MicroBundleDomain` version `1.0.1` in its normal package-reference build, and its optional repository-source build points at the same domain project file. However, MicroBundleDomain's development `docs/GETTING_STARTED.md` still instructs consumers to use `2.0.0-alpha.1`. The checked-in project file and Renderer package reference currently agree on `1.0.1`; the guide is inconsistent with those source declarations. This is source inspection, not proof that restore/build/tests pass. Do not pin `2.0.0-alpha.1` based on the guide alone.

### FSM_COS — composition and runtime assembly

FSM_COS consumes domain-owned `IMicroBundle` implementations through a host-supplied `IMicroBundleCatalog`, requested by a `RuntimeManifest`. It resolves dependency closure, loads bundles, arbitrates toward convergence, and returns a `RuntimeAssembly`.

FSM_COS does **not** own host manifestation or rendering. The host receives the assembly and decides how to manifest it.

**RuntimeAssembly evidence boundary (2026-10-17):** the FSM_COS consuming guide documents runtime identity, loaded MicroBundles, convergence-round index, and lookup by bundle ID/type. The Minimal Consumer sample demonstrates dependency-first loading and stable arbitration. The inspected guide and sample do not establish a `RuntimeAssembly.Dispose`, a bundle unload callback, or an automatic resource cleanup owner. This is not proof that no cleanup API exists in uninspected source. Until source/tests confirm otherwise, do not claim automatic disposal; any rendering resources must have an explicitly owned host/adapter cleanup path.

References:
- [FSM_COS consuming MicroBundles](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/docs/CONSUMING_MICROBUNDLES.md)
- [FSM_COS WebPage integration boundary](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/docs/WEBPAGE_INTEGRATION.md)

## WebPage manifest/composition path — owner response (source inspection, 2026-10-17)

WebPageLLM reported the current development call path in its response on [Renderer issue #15](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/issues/15#issuecomment-6239963342):

- `WorkshopExperienceService.SwitchManifestAsync` converts manifest root entries to `RuntimeManifest`, calls `_compositionSystem.Execute(...)`, verifies selected bundles in the returned `RuntimeAssembly`, and activates only after candidate composition succeeds.
- `WorkshopCompositionCatalog` is typed against canonical `TheSingularityWorkshop.MicroBundleDomain.IMicroBundle` and registers a `RenderingMicroBundle` factory for ID `4100`.
- That `TheSingularityWorkshop.Workshop.Rendering.RenderingMicroBundle` is WebPage-owned, version `0.1.0`, and declares ProtocolAi + GrammarAi dependencies.
- The Hub-level `TheSingularityWorkshop.Workshop.MicroBundles.MicroBundle` is a separate local lifecycle shell implementing `TheSingularityWorkshop.SingularityHub.IMicroBundle`; it is not the type consumed by the FSM_COS catalog. `MonikerMicroBundle` explicitly implements both contracts, so there is no general automatic bridge: dual-interface behavior is implemented per bundle where needed.
- The reported WebPage rendering bundle is a semantic/AI-facing placeholder, **not** proof that Renderer/Visualize is loaded or that rendering/GPU work occurs.

This clarifies the host composition path by owner report/source inspection, but it does not complete Renderer #15. The Renderer-owned capability, rendering-free control, and explicit cleanup ownership still need executable proof. **Do not edit WebPage-owned code from this repository.**

References:
- [WebPage MicroBundles README](https://github.com/TrentBest/WebPage/blob/development/TheSingularityWorkshop/Workshop/MicroBundles/README.md)
- [WebPage local lifecycle shell](https://github.com/TrentBest/WebPage/blob/development/TheSingularityWorkshop/Workshop/MicroBundles/MicroBundle.cs)
- [WebPage issue #15 owner response](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/issues/15#issuecomment-6239963342)

## Smallest responsible rendering proof

Do not start by making every host render. Start with one opt-in capability and one non-rendering composition.

1. **Capability implementation:** create a rendering microbundle in the Renderer-owned boundary, implementing the domain contract and declaring a stable rendering provider/capability. Keep the provider's public payload host-neutral; do not leak concrete graphics-backend types into MicroBundleDomain or FSM_COS.
2. **Explicit adapter:** keep Renderer/Visualize/GUI.SVG/backend integration inside the rendering bundle or an implementation-owned adapter. Do not make FSM_COS reference those projects.
3. **Opt-in manifest:** a small consumer requests the rendering bundle; a catalog resolves it; FSM_COS loads it and returns an assembly containing the selected bundle.
4. **Rendering-free manifest:** a second consumer requests only a non-rendering capability and proves that Renderer, Visualize, GUI.SVG, and graphics-backend assemblies are neither required nor loaded.
5. **Evidence:** automated tests assert resolved bundle identities, load/arbitration lifecycle, loaded-bundle visibility, an explicitly owned cleanup path (without assuming a domain teardown hook), and the absence of rendering references for the non-rendering sample. Record build/CI links before upgrading status.

## Current status at a glance

| Area | Verified state | Next action |
|---|---|---|
| Domain contract | Canonical interface and lifecycle documented on MicroBundleDomain development. | Verify exact project reference/version and compile against it. |
| Composition contract | FSM_COS documents manifest → catalog/dependency closure → load/arbitrate → RuntimeAssembly; the guide documents loaded-bundle visibility and lookup. Cleanup ownership is not established by the inspected guide/sample. | Verify current source/test API and add an opt-in consumer proof with an explicit host/adapter cleanup owner. |
| WebPage host | Owner reported `WorkshopExperienceService.SwitchManifestAsync` → FSM_COS `Execute` → `RuntimeAssembly`; catalog registers WebPage-owned `RenderingMicroBundle` ID 4100. The Hub shell is separate; `MonikerMicroBundle` explicitly implements both contracts. The current rendering bundle is semantic/AI-facing placeholder, not actual Renderer loading. | Keep WebPage source untouched. Renderer must prove its own optional capability, rendering-free control, and cleanup ownership. |
| Rendering capability | Architecture direction is recorded; no end-to-end bundle proof recorded. | Implement one rendering bundle and a separate rendering-free control sample. |
| Workshop-wide status | This is a scoped integration snapshot, not a complete repository inventory. | Expand status only from authoritative repo/issue/handoff evidence. |

## Coordination request

Please respond through the owning repository's handoff/coordination document with:

- **FSM_COS / MicroBundleDomain:** exact current development contract and compatible project/package versions; any lifecycle limitation relevant to loading and disposal.
- **WebPageLLM:** exact adapter/bridge between the local Hub contract and domain-owned `IMicroBundle`; first manifest → catalog → FSM_COS → RuntimeAssembly → host path and its test/CI evidence.
- **Experiences / AnyApp:** nominate one small manifest-driven experience that can first run with rendering absent, then opt in to rendering.

Until that evidence exists, status remains **contract discovered; integration proof open**.

## Visualize frame contract findings — source inspection (2026-10-17)

VisualizeLLM's source-level inspection is recorded in the [Visualize-owned WebPage handoff](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/blob/development/docs/integrations/WebPageLLM.md#2026-10-17-visualizellm-response--frameresource-and-projection-boundary). The current API/proof confirms:

- `VisualizerFrame` contains a `VisualizerView` and an ordered snapshot of `VisualizerDrawCommand` values.
- Draw commands reference geometry, material, and optional instance resources through logical IDs, plus transform, index range/base vertex, and representation.
- The GUI.SVG backend resolves resources through `IVisualizerResourceProvider` and fails explicitly for unresolved required geometry/material/instance data.
- The frame/draw-command contract does not carry WebPage city entity IDs, source-world revisions, or a canonical world coordinate convention. Those must remain explicit alongside the frame in the host/Renderer adapter; do not infer them from command order, SVG node IDs, or resource IDs.
- The GUI.SVG backend can optionally apply the frame's view/projection matrices on the CPU, but consumes ordered commands and does not establish depth-buffer visibility, general clipping, camera-ray picking, or production 3D rendering.

This supports the narrow fixture → Renderer frame → Visualize/GUI.SVG source path, but does not prove that the interactive WebPage CityLab is driven by the same path, nor that observer changes currently alter Renderer policy output while preserving city identity. The next integration proof should carry stable entity IDs, scene revision, world bounds/centre, semantic role, and explicit coordinate frame/units through the adapter, then assert observer-dependent selection plus stable source identity. Keep camera/input/movement, surface picking, and destination UI host-owned.

No API change is proposed by this source inspection. Do not close the observer-driven integration work or describe the SVG output as depth-correct 3D/GPU rendering based on this evidence.


### Additional city source-mode finding — synthetic entity identity

A direct read of WebPage's current `samples/SingularityStudiosPlan/Program.cs` shows that fixture entities have source string IDs, but the `AddRect`/`AddTriangle` helpers assign `RenderEntityId(nextEntityId++)`. Therefore the checked-in source-mode sample does not yet prove that fixture source identity is propagated into Renderer commands; its Renderer entity IDs are synthetic and depend on command-construction order. Deterministic output for one fixed ordering is not equivalent to stable identity across observer changes or fixture reorderings.

The next city integration test should map source IDs to stable Renderer identity (or carry a documented sidecar mapping) and vary observer input plus fixture enumeration order while asserting identity continuity. This is independent of Renderer #15's optional MicroBundle acceptance criteria. See the [Visualize handoff follow-up](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/blob/development/docs/integrations/WebPageLLM.md#2026-10-17-follow-up--source-entity-identity-is-not-yet-propagated-by-the-plan-sample).
