# Gallery recipe: Observer-driven policy-to-frame proof

## What this exhibit means

The sample uses Renderer’s actual `ObserverDistance.Select` primitive to select a horizon for each subject, maps that policy result to a `RendererRepresentation`, and builds a real `RenderFrame` with one ordered command per subject. It compares two observer positions against the same world fixture.

The proof asserts that at least one subject changes representation and that the total selected index count changes. The SVG reads the resulting frame commands and policy selections; it does not animate a prerecorded story.

## Reproduce it

From the Renderer repository root:

```bash
dotnet run --project samples/ObserverDrivenFrameProof/ObserverDrivenFrameProof.csproj --configuration Release -- artifacts/observer-driven-frame-proof.svg 0 50
```

Arguments are output path, first observer X, and second observer X. Choose positions that cross one or more configured horizons. The sample intentionally fails if the selected representations or index-work total do not change.

CI generates the default comparison and its reverse, then uploads both SVGs in the `renderer-observer-driven-frame-proof` artifact.

## What it proves

- Observer position affects distance calculations for the same entity fixture.
- The real `ObserverDistance.Select` API chooses horizons from the actual measured distances.
- Selected representation values feed into real `RenderFrame` command snapshots.
- The selected index-count work changes when the observer crosses policy horizons.

## What it does not prove

This is a deterministic CPU-side contract fixture. The index counts are representative workload values selected by this sample, not measurements of actual GPU work. It does not submit commands to a graphics API, produce GPU pixels, or measure frame time or allocations. The mapping from horizon names to representation kinds is explicit fixture policy, not an implicit guarantee of a universal Renderer mapping.

## Next proof

Connect the policy-to-frame result to actual bucket invalidation and reuse metrics, then preserve those metrics beside the command output. Benchmark elapsed time and allocations separately rather than deriving performance claims from index counts.
