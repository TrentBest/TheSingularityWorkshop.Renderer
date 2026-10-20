# Bucket invalidation and reuse

This exhibit executes the actual `EventHorizonBucket` and `EventHorizonBucketDependencyGraph` APIs. It is stronger than the composition illustration: the proof changes an authoritative source revision, observes dependency propagation, checks unrelated buckets remain valid, records consumer-confirmed reuse, refreshes invalidated outputs, and emits an SVG from the resulting metrics.

## Reproduce

```bash
dotnet run --project samples/BucketInvalidationReuseProof/BucketInvalidationReuseProof.csproj --configuration Release -- artifacts/bucket-invalidation-reuse-proof.svg
```

The scenario creates four independently scheduled buckets. Environment geometry and lighting are declared inputs to shadow output. Changing geometry from source revision 1 to 2 invalidates geometry and the dependent shadow bucket; lighting and distant motion remain reusable. The sample then accepts refreshed geometry and shadow output and reports the actual counters returned by each bucket.

## Evidence boundary

- **Implemented behavior exercised:** source-revision invalidation, dependency propagation, independent bucket validity, refresh ticket acceptance, and explicit consumer reuse.
- **Metrics:** values come from `EventHorizonBucket.Metrics`; reuse increments only after `TryRecordReuse` confirms a valid cached representation.
- **Not demonstrated:** GPU submission, pixels, GPU work avoided, timing, allocations, or production workload savings. This is a deterministic CPU-side contract proof, not a performance benchmark.
- **Policy scope:** the sample's geometry → shadow and lighting → shadow edges are fixture dependencies, not a universal renderer dependency graph.

The proof is generated in CI and uploaded as `renderer-bucket-invalidation-reuse-proof`.
