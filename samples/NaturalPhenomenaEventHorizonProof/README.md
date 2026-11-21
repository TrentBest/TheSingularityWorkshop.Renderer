# Natural phenomena Event Horizon proof

Run from the repository root:

```bash
dotnet run --project samples/NaturalPhenomenaEventHorizonProof/NaturalPhenomenaEventHorizonProof.csproj --configuration Release
```

This deterministic CPU-side proof uses one volcanic-ash phenomenon with three distinct source lifecycles:

- A transient lightning event.
- An evolving regional ash-density field.
- Persistent deposited ash aftermath.

It runs the production `DeterministicRepresentationPolicy` for three observer roles: first-person, distant camera, and AI/map. Each decision retains the same authoritative ash source ID and revision while selecting a different multidimensional profile under independent CPU, GPU-estimate, memory, transfer, freshness, and uncertainty budgets. The first-person observer carries `Interact` intent, the camera uses passive `Observe`, and the AI/map observer carries `RespondToHazard` intent. The expected choices are FullVisual, RegionalVolume, and HazardMarker respectively. Costs are host-supplied estimates, not measured execution.

The proof also runs `RenderContributionAggregator` over compatible ash contributions. Ordinary ash cells may aggregate within the declared error budget; the hazard plume and interaction-critical rescue corridor remain individual and preserve their source revisions.

Finally, it exercises the real `EventHorizonBucketDependencyGraph`:

- Lightning invalidates shadow and temporal-lighting histories.
- Ash-density changes invalidate volume integration.
- Camera changes invalidate view history.
- Persistent deposited ash invalidates surface materials and surface lighting.

Unrelated outputs remain reusable. The sample writes a machine-readable JSON trace and a diagnostic SVG under `artifacts/natural-phenomena-event-horizon-proof`.

**Evidence boundary:** this is deterministic Renderer policy, provenance, aggregation, and invalidation evidence. The phenomenon states and costs are fixture-supplied; there is no volcanic/atmospheric simulation, shader, particle system, GPU execution, or measured performance claim. Visualize remains the owner of concrete visual manifestation.
