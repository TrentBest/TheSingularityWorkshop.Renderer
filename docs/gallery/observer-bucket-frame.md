# Observer-to-bucket-to-frame causal proof

This exhibit joins the two existing CPU-side proofs into one causal trace:

1. The observer position changes.
2. The real `ObserverDistance.Select` policy evaluates the same subjects at the new distances.
3. Only subjects whose selected representation changes receive `BucketDirtyReason.ObserverChanged`.
4. The real `EventHorizonBucketDependencyGraph` propagates invalidation from each changed representation bucket to its declared shadow dependent.
5. Changed buckets accept refresh tickets; unchanged representation and shadow buckets explicitly record cache reuse.
6. The selected representations are reflected in actual `RenderFrame` command snapshots.

## Reproduce

```bash
dotnet run --project samples/ObserverBucketFrameProof/ObserverBucketFrameProof.csproj --configuration Release -- artifacts/observer-bucket-frame-proof.svg 0 50
```

Arguments are output path, initial observer X, and moved observer X. The sample fails if no representation changes, if invalidated output is not restored to a valid state, or if unchanged buckets fail to record reuse. The fixture includes a relay mast that stays in the same representation across the default movement so the selective-reuse path is exercised.

CI generates this proof and uploads it as `renderer-observer-bucket-frame-proof`.

## Evidence boundary

- **Actual APIs exercised:** `ObserverDistance.Select`, `EventHorizonBucket`, `EventHorizonBucketDependencyGraph`, `RenderFrame`, and ordered `RenderDrawCommand` snapshots.
- **Asserted causal behavior:** observer-dependent representation change, selective invalidation, dependency propagation to shadows, accepted refreshes, explicit reuse of unchanged buckets, and a changed resulting frame.
- **Fixture values:** the index counts are representative workload values, not measured GPU work.
- **Not demonstrated:** a production renderer scheduler, GPU submission, rendered pixels, actual GPU work avoided, frame timing, or allocation savings.

This is a connected deterministic CPU-side contract proof—not yet an end-to-end graphics pipeline or performance benchmark.
