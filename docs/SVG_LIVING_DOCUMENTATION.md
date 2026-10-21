# SVG living documentation

**Purpose:** use generated SVGs as a visible, inspectable proof surface for Renderer behavior—not as a substitute for execution evidence.

## Start here — One observer, two worlds

The same observer change can be examined as **structured computation** and as **visual manifestation**. Renderer decides which representation and draw commands belong in the frame; Visualize consumes that frame and its resources; GUI.SVG optionally serializes the resulting scene to SVG. The two views complement one another: a picture shows the result, while the command data and assertions explain why it changed.

### World 1: inspect the Renderer decision

![RenderFrame inspector generated from actual ordered Renderer frame commands](assets/render-frame-inspector.svg)

The [RenderFrame inspector recipe](gallery/render-frame-inspector.md) constructs a real frame and exposes observer position, ordered commands, representation, entity ID, resource IDs, and draw ranges. This is structured CPU-side evidence—not rendered pixels.

### World 2: inspect the downstream manifestation

The Visualize repository maintains an executable [observer-driven presentation proof](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/blob/development/docs/gallery/observer-driven-presentation.md). It evaluates the actual `ObserverDistance.Select` policy for observer X=0 and X=50, builds two Renderer frames, adapts them through `RendererFrameAdapter`, and generates two SVGs plus a machine-readable report.

- [Proof source](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/blob/development/integrations/GuiSvg/ObserverFrameProof/Program.cs)
- [Proof project](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/blob/development/integrations/GuiSvg/ObserverFrameProof/ObserverFrameProof.csproj)
- Run from the Visualize repository root, with the Renderer and GUI source repositories available:

```bash
dotnet run --project integrations/GuiSvg/ObserverFrameProof/ObserverFrameProof.csproj --configuration Release -- ./artifacts/observer-frame
```

The program asserts that observer movement changes representation decisions, that both frames retain one command per stable subject, and that the adapter preserves command count, entity identity, and representation. It records selected index counts and backend status in its report.

**Evidence boundary:** this is a CPU/SVG integration proof. Its marker geometry is a small fixture; its selected index counts are fixture workload values. It does not submit GPU commands, measure GPU-produced pixels, establish production scheduling/cache behavior, or benchmark timing and allocations.

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

## 4. Bucket invalidation and reuse: execute the dependency graph

The bucket proof executes `EventHorizonBucket` and `EventHorizonBucketDependencyGraph` directly. It refreshes geometry, lighting, distant motion, and shadow output; changes geometry's source revision; verifies that geometry and its declared shadow dependent become stale while lighting and distant motion remain reusable; then accepts replacement refreshes and reports each bucket's actual counters.

Run from the Renderer repository root:

```bash
dotnet run --project samples/BucketInvalidationReuseProof/BucketInvalidationReuseProof.csproj --configuration Release -- artifacts/bucket-invalidation-reuse-proof.svg
```

CI uploads the generated SVG as `renderer-bucket-invalidation-reuse-proof`.

**What it proves:** source-revision invalidation, declared dependency propagation, independent bucket validity, accepted refresh tickets, and explicit consumer-confirmed cache reuse.

**What it does not prove:** GPU submission, pixels, avoided GPU work, timing, allocations, or production workload savings. The dependency edges are fixture policy, not a universal renderer graph.

## 5. Composition picture: understand the intended ownership

![Renderer composition explainer](assets/renderer-composition.svg)

This animated architecture illustration explains how observer context, Event Horizons, independent work buckets, and frame assembly are intended to fit together. It is a conceptual guide—not live telemetry and not proof that every stage is wired into one end-to-end renderer.

## Ownership boundaries

- Renderer owns observer-relative selection, representation policy, and platform-neutral frame/command contracts.
- Visualize owns the optional presentation adapter and its resource/frame consumption.
- GUI.SVG owns SVG scene manifestation and serialization.
- SVG-only animation is not an FSM update loop. A generated image is a snapshot unless an explicit live data connection is demonstrated.

Renderer must remain independently usable: **do not add a Visualize or GUI.SVG dependency to the Renderer library** just to make its documentation visual.

## Next proof progression

1. **Policy-to-frame:** expose deterministic observer/policy inputs and representation counts together, with assertions on command order.
2. **Observer changes:** connect observer changes to bucket invalidation and compare actual selected representations, bucket metrics, and frame counts in one causal trace.
3. **Consumer proof:** keep Visualize's GUI.SVG output as an optional downstream manifestation of real frame/resource contracts.
4. **GPU proof:** label separately and require a real selected backend, submitted work, and verified GPU-produced pixels.

Each proof should be generated by a runnable C# sample or test, checked in or published as a CI artifact, and compared in CI where deterministic. A diagram may explain an intended path, but must be labeled conceptual until executable evidence supports it.
