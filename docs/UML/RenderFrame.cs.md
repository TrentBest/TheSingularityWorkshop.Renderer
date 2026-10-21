# RenderFrame — source companion

**Purpose:** Carry the observer-relative view and ordered rendering commands for one frame across the platform-neutral Renderer boundary.

**Implementation source:** [`RenderFrame.cs`](../../RenderFrame.cs)  
**Class diagram:** [`RenderFrame.cs.svg`](RenderFrame.cs.svg)  
**Tests and executable evidence:** [Renderer test suite](../../tests) · [observer-driven frame proof](../gallery/observer-driven-frame.md)

## Class card

```text
RenderFrame
  + RenderFrame(view: RenderView, commands: IEnumerable<RenderDrawCommand>)
  + View : RenderView
  + Commands : IReadOnlyList<RenderDrawCommand>
```

## Contract

- Construction rejects a null command sequence.
- Construction materializes the source sequence into an array and exposes it as a read-only collection, so later reuse or mutation of the producer's sequence does not change the frame snapshot.
- Command ordering is preserved.
- The frame carries platform-neutral data; it does not submit GPU work or produce pixels.
- A downstream backend should consume the snapshot directly unless its own lifetime or threading contract requires another copy.

## Use case — preserve an observer decision through presentation

1. Renderer selects the commands for a view.
2. Construct a `RenderFrame` with that `RenderView` and the ordered commands.
3. Pass the immutable snapshot to an optional downstream adapter such as Visualize.
4. Verify the downstream output preserves command identity and representation; do not mistake the frame model itself for a GPU backend.

## Evidence boundary

This page describes the current source contract. The observer-driven proof exercises policy-to-frame behavior for deterministic fixtures; it does not establish universal LOD correctness or GPU performance.
