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

The contract does not define rendering, a GUI, storage, or the composition engine. A rendering capability should implement this contract without making MicroBundleDomain depend on Renderer, Visualize, GUI.SVG, a graphics API, or a host.

References:
- [MicroBundleDomain architecture on development](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain/blob/development/docs/ARCHITECTURE.md)
- [MicroBundleDomain getting started on development](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain/blob/development/docs/GETTING_STARTED.md)

**Version finding (source inspected 2026-10-17):** the MicroBundleDomain `development` project file `TheSingularityWorkshop.MicroBundleDomain.csproj` declares `<TargetFramework>net8.0</TargetFramework>` and `<Version>1.0.1</Version>`. Renderer `development` already references `TheSingularityWorkshop.MicroBundleDomain` version `1.0.1` in its normal package-reference build, and its optional repository-source build points at the same domain project file. However, MicroBundleDomain's development `docs/GETTING_STARTED.md` still instructs consumers to use `2.0.0-alpha.1`. The checked-in project file and Renderer package reference currently agree on `1.0.1`; the guide is inconsistent with those source declarations. This is source inspection, not proof that restore/build/tests pass. Do not pin `2.0.0-alpha.1` based on the guide alone.

### FSM_COS — composition and runtime assembly

FSM_COS consumes domain-owned `IMicroBundle` implementations through a host-supplied `IMicroBundleCatalog`, requested by a `RuntimeManifest`. It resolves dependency closure, loads bundles, arbitrates toward convergence, and returns a `RuntimeAssembly`.

FSM_COS does **not** own host manifestation or rendering. The host receives the assembly and decides how to manifest it.

References:
- [FSM_COS consuming MicroBundles](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/docs/CONSUMING_MICROBUNDLES.md)
- [FSM_COS WebPage integration boundary](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/docs/WEBPAGE_INTEGRATION.md)

## Important WebPage integration observation

The current WebPage development source includes a local `Workshop.MicroBundles.MicroBundle` that implements `TheSingularityWorkshop.SingularityHub.IMicroBundle` (and a Hub alias), with its own lifecycle FSM. The WebPage microbundle README says the canonical runtime contract belongs to MicroBundleDomain and FSM_COS consumes that contract.

This may be an intentional adapter/transition or a remaining contract seam; source inspection alone does not establish which. **Do not edit WebPage-owned code from this repository.** Ask WebPageLLM to identify the current bridge from its Hub-level contract to the domain-owned `TheSingularityWorkshop.MicroBundleDomain.IMicroBundle` contract and whether its first manifest-driven experience actually traverses FSM_COS.

References:
- [WebPage MicroBundles README](https://github.com/TrentBest/WebPage/blob/development/TheSingularityWorkshop/Workshop/MicroBundles/README.md)
- [WebPage local lifecycle shell](https://github.com/TrentBest/WebPage/blob/development/TheSingularityWorkshop/Workshop/MicroBundles/MicroBundle.cs)
- [WebPage Pong experience candidate](https://github.com/TrentBest/WebPage/blob/development/TheSingularityWorkshop/Workshop/MicroBundles/PongMicroBundle.cs)

## Smallest responsible rendering proof

Do not start by making every host render. Start with one opt-in capability and one non-rendering composition.

1. **Capability implementation:** create a rendering microbundle in the Renderer-owned boundary, implementing the domain contract and declaring a stable rendering provider/capability. Keep the provider's public payload host-neutral; do not leak concrete graphics-backend types into MicroBundleDomain or FSM_COS.
2. **Explicit adapter:** keep Renderer/Visualize/GUI.SVG/backend integration inside the rendering bundle or an implementation-owned adapter. Do not make FSM_COS reference those projects.
3. **Opt-in manifest:** a small consumer requests the rendering bundle; a catalog resolves it; FSM_COS loads it and returns an assembly containing the selected bundle.
4. **Rendering-free manifest:** a second consumer requests only a non-rendering capability and proves that Renderer, Visualize, GUI.SVG, and graphics-backend assemblies are neither required nor loaded.
5. **Evidence:** automated tests assert resolved bundle identities, load/dispose lifecycle, and the absence of rendering references for the non-rendering sample. Record build/CI links before upgrading status.

## Current status at a glance

| Area | Verified state | Next action |
|---|---|---|
| Domain contract | Canonical interface and lifecycle documented on MicroBundleDomain development. | Verify exact project reference/version and compile against it. |
| Composition contract | FSM_COS documents manifest → catalog/dependency closure → load/arbitrate → RuntimeAssembly. | Verify current source/test API and add an opt-in consumer proof. |
| WebPage host | Local Hub-level microbundle shell exists; the canonical-domain bridge is not established by the files inspected. | Ask WebPageLLM for bridge location and first experience trace; do not modify its source. |
| Rendering capability | Architecture direction is recorded; no end-to-end bundle proof recorded. | Implement one rendering bundle and a separate rendering-free control sample. |
| Workshop-wide status | This is a scoped integration snapshot, not a complete repository inventory. | Expand status only from authoritative repo/issue/handoff evidence. |

## Coordination request

Please respond through the owning repository's handoff/coordination document with:

- **FSM_COS / MicroBundleDomain:** exact current development contract and compatible project/package versions; any lifecycle limitation relevant to loading and disposal.
- **WebPageLLM:** exact adapter/bridge between the local Hub contract and domain-owned `IMicroBundle`; first manifest → catalog → FSM_COS → RuntimeAssembly → host path and its test/CI evidence.
- **Experiences / AnyApp:** nominate one small manifest-driven experience that can first run with rendering absent, then opt in to rendering.

Until that evidence exists, status remains **contract discovered; integration proof open**.