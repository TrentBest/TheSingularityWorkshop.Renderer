# Desktop and browser Renderer host proof

**Branch:** `development` only. `master` is untouched. No package publish or deployment was performed.

## Decision

Use AnyApp as the native desktop host and WebPage as the browser host for the same Renderer diagnostic command workload. Keep the hosts responsible for their presentation and input; keep representation choice and command accounting in Renderer.

## Shared Renderer workload

`Diagnostics/RendererDiagnosticScene.cs` creates a deterministic grid of stable diagnostic entity IDs. Each entity supplies one candidate command for each of the five `RendererRepresentation` values. This is explicitly diagnostic content, not authoritative city or migration world data.

`RenderCommandReducer` selects the richest available candidate at or below the chosen detail ceiling. For a 1,000-entity workload, the input contains 5,000 commands and a successful reduction emits 1,000 commands. That is a deterministic count implied by the fixture and reducer contract, not a frame-time or GPU-performance claim.

Renderer CI:
- [Diagnostic fixture build/test/package workflow](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/actions/runs/38994402185) — succeeded for test commit `07848f99cbdb7fbed5fbcdf34b20c9ac178eb76c`.
- [Shared diagnostic fixture implementation](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/commit/b02871a06801c9825c83c378764b633355035996).
- [Fixture and reduction tests](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/commit/07848f99cbdb7fbed5fbcdf34b20c9ac178eb76c).

## AnyApp desktop host

Run on Windows from the AnyApp repository:

```powershell
dotnet run --project AnyApp.csproj -- --renderer-proof
```

The explicit launch flag presents a WPF diagnostic projection of the actual reduced Renderer command list. The detail buttons exercise Semantic, Aggregate, Distant, Near, and Interactive ceilings; counters show candidate commands, selected commands, omitted commands, and represented entities. The ordinary Moniker startup remains the default when the flag is absent.

This is a desktop host integration test, not yet a Visualize backend integration. The WPF markers are deliberately identified as diagnostic projection and do not claim geometry fidelity or GPU rendering.

Relevant commits:
- [Add desktop diagnostic harness](https://github.com/TrentBest/AnyApp/commit/e64b897ceb8844909f918d51a7319abb616e0458).
- [Add explicit launch mode](https://github.com/TrentBest/AnyApp/commit/9b954b98e6bb0d52dfb4451c43e2f0784ff68c76).
- [Document desktop launch mode](https://github.com/TrentBest/AnyApp/commit/4532b5ba496ca65bab4052b78b2e1955c054950f).
- The AnyApp build and baseline workflows had stale dependency branch references. Both workflows now check out FSM_COS, MicroBundleDomain, and MicroBundleRepository from `development`, consistent with the development-workflow branch policy. These corrected workflows are running; their final results are not yet verified.

At the time this handoff was written, AnyApp build/test and baseline workflows were still running. Do not report them as passing until checked:
- [AnyApp .NET workflow](https://github.com/TrentBest/AnyApp/actions/runs/38994424634)
- [AnyApp baseline workflow](https://github.com/TrentBest/AnyApp/actions/runs/38994424671)

## WebPage browser host

The existing `/rendering` research page now contains a shared-workload panel. In repository-source mode it calls `RendererDiagnosticScene` and `RenderCommandReducer` from the actual Renderer project and manifests the selected command positions as an SVG diagnostic projection. The UI exposes the same five detail ceilings and command/entity counters.

Run from a workspace with the sibling repositories required by WebPage's existing source-mode contract:

```powershell
dotnet run --project TheSingularityWorkshop/TheSingularityWorkshop.csproj -p:WorkshopUseRepositorySources=true
```

Then open `/rendering`. Package mode is deliberately marked disconnected and disables the controls; it must not imply that the development Renderer source is loaded.

This is a browser-host proof of the shared decision path and stable diagnostic workload. It does not yet use the Visualize frame adapter/backend in the browser, nor does it prove GPU rendering. The next seam is to pass the selected commands through the documented Renderer-to-Visualize adapter and compare actual Visualize output/metrics in both hosts where each backend supports it.

Relevant commits:
- [Add browser diagnostic harness](https://github.com/TrentBest/WebPage/commit/93d9ba3bc463c8cc1cb73892b097ecc487c5f2b3).
- [Wire browser panel to the renderer reducer](https://github.com/TrentBest/WebPage/commit/91e6075dac53ea4dc39224445bebf741e5f6e64a).
- [Expose stable IDs in the browser SVG](https://github.com/TrentBest/WebPage/commit/2fe41d8df71da1410d54101940f46e83b5c92eda).
- [Assert identity stability and all five detail ceilings in the browser smoke test](https://github.com/TrentBest/WebPage/commit/13b691514138828fd2f41d1b066b906dc677b41c). The latest browser test run is in progress; the earlier source-mode build and live Renderer FSM smoke test succeeded before these additional assertions.

At the time of this handoff, the WebPage .NET workflow was still running, including its dedicated repository-source integration build and browser smoke test:
- [WebPage .NET Tests](https://github.com/TrentBest/WebPage/actions/runs/38994458797)

## Ownership and next steps

- **Renderer:** keep the deterministic fixture/reducer and metrics backend-neutral. Next add an explicit scene-to-frame path and threshold/observer-motion continuity tests.
- **AnyApp:** remain a native host; test the actual shared decision path. Once Visualize exposes a supported desktop backend contract, replace or supplement the WPF diagnostic projection rather than quietly treating it as production rendering.
- **WebPage:** keep camera, interaction, and browser experience ownership. Preserve the honest package/source mode distinction and the existing browser smoke tests.
- **Visualize:** own the presentation-data and backend path. Coordinate before wiring its adapter or experimental GPU proof into a host; the existing WPF/D3D12 clear-only sample is not geometry rendering proof.
- **World providers:** own authoritative scene facts and identities. The shared diagnostic scene is test-only and must not become the city or monarch world model.
- **Physics:** remains separately owned and untouched.

The current acceptance result is deterministic command selection in one shared workload, with two host-specific diagnostic projections. It is not yet the finished Visualize/GPU vertical slice. That distinction is intentional: make the integration real first, then measure the real path.
