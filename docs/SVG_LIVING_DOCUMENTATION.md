# SVG living documentation

**Purpose:** use generated SVGs as a visible, inspectable proof surface for Renderer behavior—not as a substitute for execution evidence.

## 1. Living Event Horizons: state transitions made visible

![Living Event Horizons generated from FSM_API representation states](assets/living-event-horizons.svg)

**What you're seeing:** five real `RendererComputationMachine` instances are created and advanced through FSM_API; the sample reads each machine's `CurrentRepresentation` and writes the SVG. The representation state comes from executed FSM transitions. Glow, pulse, layout, and animation are SVG presentation.

**What it proves:** the sample can execute the FSM-backed representation-state path and expose its resulting state in an inspectable snapshot.

**What it does not prove:** this is not live telemetry, a running world, or a frame-time/performance benchmark.

Run from the Renderer repository root:

```bash
dotnet run --project samples/LivingRenderer/LivingRenderer.csproj --configuration Release -- docs/assets/living-event-horizons.svg
```

## 2. RenderFrame inspector: inspect the actual command contract

![RenderFrame inspector generated from actual ordered Renderer frame commands](assets/render-frame-inspector.svg)

**What you're seeing:** a runnable sample constructs a real `RenderFrame`, then reads `RenderFrame.View` and its ordered `RenderFrame.Commands`. The SVG exposes observer position, command order, representation, entity ID, geometry/material resource IDs, and index/instance ranges.

**What it proves:** the CPU-side frame contract contains inspectable, ordered command metadata that a downstream backend can consume.

**What it does not prove:** the inspector does not draw the geometry, submit GPU work, or claim to show rendered pixels.

Run from the Renderer repository root:

```bash
dotnet run --project samples/RenderFrameSvgProof/RenderFrameSvgProof.csproj --configuration Release -- docs/assets/render-frame-inspector.svg
```

CI regenerates both Renderer-owned SVGs and fails if their checked-in outputs drift from the executable samples.

## 3. Observer-driven policy-to-frame proof

The observer-driven sample uses the actual `ObserverDistance.Select` policy primitive to select a horizon for each subject, maps the selected horizon to a representation, and builds a `RenderFrame` containing those representation decisions. It compares two observer positions and asserts that both the selected representations and total selected index counts change.

This proves causal observer input → policy selection → CPU-side frame-command metadata for this deterministic fixture. It does not prove GPU submission, rendered pixels, timing, allocations, or a production scene pipeline.

Run it with custom observer X coordinates:

```bash
dotnet run --project samples/ObserverDrivenFrameProof/ObserverDrivenFrameProof.csproj --configuration Release -- artifacts/observer-driven-frame-proof.svg 0 50
```

The optional final arguments are the first and second observer X positions. The default comparison uses 0 and 50. CI generates both forward and reversed comparisons and uploads them as the `renderer-observer-driven-frame-proof` artifact.

## 4. Composition picture: understand the intended ownership

![Renderer composition explainer](assets/renderer-composition.svg)

This animated architecture illustration explains how observer context, Event Horizons, independent work buckets, and frame assembly are intended to fit together. It is a conceptual guide—not live telemetry and not proof that every stage is wired into one end-to-end renderer.

## 5. Cross-repository proof: follow a real frame downstream

[Open the Visualize living SVG guide](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/blob/work/svg-living-docs/docs/SVG_LIVING_DOCUMENTATION.md).

The Visualize `CityMassingProof` fixture exercises:

`ObserverRepresentationPolicy` → selected face commands → Renderer `RenderFrame` → `RendererFrameAdapter` → Visualizer frame/resources → optional GUI.SVG backend → SVG files and a JSON report.

Open the [Visualize proof workflow](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/runs/39369513107), then its **Artifacts** section and download `visualize-city-massing-proof` to see the three skins and report.

The executable assertion checks that command count/order and each command's geometry/material IDs, world transform, index range, representation, and entity ID survive the adapter. The distant representation omits selected window-detail face commands. This is command-level work reduction—not GPU rendering, geometry-buffer compaction, or a production city renderer.

## Ownership boundaries

- Renderer owns observer-relative selection, representation policy, and platform-neutral frame/command contracts.
- Visualize owns the optional presentation adapter and its resource/frame consumption.
- GUI.SVG owns SVG scene manifestation and serialization.
- SVG-only animation is not an FSM update loop. A generated image is a snapshot unless an explicit live data connection is demonstrated.

Renderer must remain independently usable: **do not add a Visualize or GUI.SVG dependency to the Renderer library** just to make its documentation visual.

## Next proof progression

1. **Policy-to-frame:** expose deterministic observer/policy inputs and representation counts together, with assertions on command order.
2. **Bucket invalidation:** execute the actual dependency graph, change one source, and visualize invalidated versus reusable buckets.
3. **Observer changes:** change the observer and compare actual selected representations and frame counts.
4. **Consumer proof:** keep Visualize's GUI.SVG output as an optional downstream manifestation of real frame/resource contracts.
5. **GPU proof:** label separately and require a real selected backend, submitted work, and verified GPU-produced pixels.

Each proof should be generated by a runnable C# sample or test, checked in or published as a CI artifact, and compared in CI where deterministic. A diagram may explain an intended path, but must be labeled conceptual until executable evidence supports it.
