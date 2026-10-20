# Gallery recipe: RenderFrame inspector

## What this exhibit means

The sample constructs a real CPU-side `RenderFrame` and serializes its observer view and ordered draw-command metadata into an SVG: command order, representation, entity ID, geometry/material resource IDs, and index/instance ranges.

![RenderFrame inspector](../assets/render-frame-inspector.svg)

## Reproduce it

From the Renderer repository root:

```bash
dotnet run --project samples/RenderFrameSvgProof/RenderFrameSvgProof.csproj --configuration Release -- docs/assets/render-frame-inspector.svg
```

Source: [RenderFrameSvgProof sample](../../samples/RenderFrameSvgProof/Program.cs).

## What it proves

A downstream consumer can inspect the actual ordered command contract without adding Visualize or GUI.SVG as a Renderer dependency.

## What it does not prove

The inspector does not rasterize source geometry, submit GPU work, or show GPU-produced pixels. It is a state/contract inspector, not a frame capture from a running game.

## Next recipe

Add a repeatable input fixture that varies observer position, then show before/after command counts and representation choices with assertions that the difference is caused by the changed input.
