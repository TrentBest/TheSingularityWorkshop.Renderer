# Renderer LLM — Cross-repository findings

## 2026-10-18: Renderer source integration test failure isolated to FSM_API instance destruction

The latest completed Renderer workflow is [run 39086967875](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/actions/runs/39086967875), commit `4831eb5def6e5b676d857a6c52cce1f737d3971b`.

Observed results:
- Renderer normal Release build/test/package job: **passed**.
- Repository-source integration job: source restore **passed**, source build **passed**, tests **failed** in one test.
- Failing test: `ObserverRepresentationCoordinatorTests.Evaluate_InvalidatesSelectedBucketWhenObserverRevisionChanges`.
- Stack trace ends at `FSM_API.Internal.DestroyInstance(FSMHandle handle)`, specifically `List<T>.RemoveAt(Int32 index)` called from `List<T>.Remove(T item)`, then `FSM_API.Interaction.DestroyInstance`, `RendererComputationMachine.Dispose`, and `ObserverRepresentationCoordinator.Dispose`.

This failure is not evidence of a `RenderFrame` namespace/API mismatch. It is a disposal/instance-registry failure in the FSM_API development source used by Renderer integration tests. Please investigate the source implementation and lifecycle/registry invariants in FSM_API; do not patch over this in Renderer by changing the public rendering contract.

Relevant source context:
- Renderer creates a machine per coordinator and calls `FSMApi.Interaction.DestroyInstance(_handle)` once from idempotent `RendererComputationMachine.Dispose()`.
- Coordinator disposal is also idempotent and serializes disposal through its lock.
- The failing test disposes a coordinator after evaluating the observer revision change; earlier tests also create and dispose several FSM-backed machines, so the instance registry may be inconsistent after prior lifecycle operations or handle removal.

Reproduction target: rerun Renderer workflow's `Test from sibling repository sources` with current FSM_API `development` source checked out. Verify whether the defect reproduces in FSM_API's own tests or is triggered by Renderer lifecycle usage. Fix the owning FSM_API registry/destruction logic if confirmed, then rerun both FSM_API and Renderer source-integration suites.

No Renderer API change is recommended based on this evidence.


## 2026-10-18: Focused disposal-race hypothesis from FSM_API source

A deeper FSM_API source read narrows the failure mechanism. `Create.CreateInstance` reads the shared `bucket.Instances.Count` and immediately calls `bucket.Instances.Add(handle)`; `Internal.DestroyInstance` immediately calls `bucket.Instances.Remove(handle)`. `Internal.MoveInstance` also mutates these lists, while group/definition teardown can clear or remove them. These operations do not share a visible registry-wide synchronization gate.

`RendererComputationMachine.Dispose()` is idempotent per machine, and `ObserverRepresentationCoordinator` serializes its own Evaluate/Dispose calls. But each coordinator owns a different `_gate`; it does not synchronize separate coordinators or any other consumer using FSM_API's static `Renderer` processing group and `Renderer.Representation` definition. Therefore, coordinator-local locking does not eliminate a shared-registry race.

The exception from `List<T>.RemoveAt` inside `List<T>.Remove` is consistent with concurrent mutation between index discovery and removal. This is the leading hypothesis, not yet a reproduced root cause. The next discriminating check is test parallelization: run the source-integration suite serially and then with normal parallelism. If serialization eliminates the failure, identify the parallel tests sharing FSM_API static state and decide whether the intended contract is main-thread-only or registry-wide thread safety. Do not suppress the disposal exception or replace destruction with `IsValid = false`.

FSM_API cross-repository notes: [RendererLLM.md on FSM_API development](https://github.com/TrentBest/FSM_API/blob/development/RendererLLM.md). This finding is documentation/investigation only; no runtime fix or successful reproduction is claimed.


## 2026-10-18: Exact FSM_API revision comparison changes the leading explanation

The workflow logs identify the precise sibling source revisions:

- Original failing source-integration job `117555449357`: FSM_API `da2e05ba41ff33aeb1429433b70593e271b1fd3b`.
- Successful rerun job `117593029298`: FSM_API `f5a89641eaca216f5466af59b2dc4a983fe9511c`.
- The passing revision is 38 commits ahead of the failing revision.

A relevant runtime behavior changed in `FSM_API.Internal.cs`'s `ResetAPI` non-hard-reset path. At the failed revision, an invalid context caused `ResetAPI` to call `DestroyInstance(handle)`; at the successful revision, invalidity is treated as deactivation and the registered handle is retained. The later source also includes a test proving that a handle can resume after its context becomes valid again.

This is a concrete candidate explanation for the failure and supports the user's suggestion that FSM_API work may already have fixed it. It does not conclusively establish causation because 38 commits separate the two revisions and the workflow did not isolate this change in a controlled A/B run. The successful rerun is real evidence that the current combination passes; it is not proof that all concurrency risks are gone.

Next: keep the current Renderer disposal path intact, run the source-integration test against the current FSM_API development revision, and only pursue registry-wide synchronization if the failure reproduces or a focused stress test establishes concurrent mutation as the cause. Do not suppress disposal exceptions or replace explicit destruction with `IsValid = false`.

Evidence: [Renderer workflow and reruns](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/actions/runs/39086967875); [FSM_API comparison](https://github.com/TrentBest/FSM_API/compare/da2e05ba41ff33aeb1429433b70593e271b1fd3b...f5a89641eaca216f5466af59b2dc4a983fe9511c).
