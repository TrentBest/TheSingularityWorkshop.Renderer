# Renderer → Visualize array-first workload trace

This sample exports a deterministic JSON trace from the real Renderer policy path. It is intended to replace the WebGPU demo's sample-owned near/far cadence with decisions made by `ObserverRepresentationCoordinator` and `EventHorizonBucket`.

## Generate

From the Renderer repository root:

```powershell
dotnet run --project samples/RendererArrayFirstWorkloadTrace -- artifacts/renderer-workload-trace.json
```

The trace uses a fixed 60 Hz timeline, deterministic near/far observer movement, a source revision change, and an observer revision change. It records the selected horizon and representation, refresh tickets, accepted refreshes, cache reuse, invalidation reasons, and bucket counters.

## Contract and limits

- `SchemaVersion` versions the trace format; consumers should reject unsupported versions rather than silently misread them.
- `RefreshRequired` records a Renderer decision. `RefreshAccepted` records whether the bucket accepted the simulated completion.
- `ReusedCachedRepresentation` comes from the coordinator's valid-cache path; it is not inferred from elapsed time.
- `DirtyReasons` and counters are diagnostics from the actual selected bucket.
- The sample completes refreshes immediately to make the policy trace deterministic. It does not create geometry, invoke Visualize, or render pixels.
- This is a starting bridge, not the final integrated GPU proof. Visualize's WebGPU runner must consume this trace and map refresh/reuse decisions to cohort updates and resource reuse before the end-to-end milestone is considered complete.

No Visualize dependency, graphics API, package publication, or hardware-performance claim is introduced in Renderer.
