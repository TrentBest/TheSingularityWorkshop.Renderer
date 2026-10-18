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

## 2026-10-18: Current-source disposal verification passed

The latest rerun of Renderer workflow run [39086967875](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/actions/runs/39086967875) completed successfully. Source-integration job `117594490093` checked out FSM_API `development` at `69949dc499c39f28d5b85d554d3f6006c7e9286d`; restore and build succeeded, and the Renderer test assembly reported **115 tests passed, 0 failed**. The independent `build-and-test` job also passed, including composition checks and living-SVG reproducibility. NuGet publishing was skipped.

This is positive evidence that the current cross-repository source combination no longer reproduces the previously observed disposal failure. It does not isolate which FSM_API commit/change explains the earlier failure, and it does not prove registry-wide concurrent mutation is safe. Keep the current disposal path; do not add speculative locking or suppress destruction exceptions without a reproducible failure or a contract-backed concurrency test.


## 2026-10-18: Apply FSM_API's documented main-thread mutation contract

FSM_API's README states its supported model as **main-thread operations with deferred mutation handling**. Source inspection distinguishes the APIs: Interaction.MoveInstance queues a structural change, while Interaction.DestroyInstance synchronously reaches Internal.DestroyInstance and mutates the shared instance list. Do not assume destruction is deferred just because the API supports deferred mutation elsewhere.

The current Renderer source-integration run passes **115/115 tests** against FSM_API development commit 69949dc499c39f28d5b85d554d3f6006c7e9286d. No Renderer runtime change is justified from the old failure alone. Before introducing locks or changing disposal semantics, verify that calls to RendererComputationMachine.Dispose() and FSM_API destruction occur on the intended owning/main thread and are not overlapping ticking or other structural operations. If that check is inconclusive, compare serial and normal-parallel test runs; report the exact outcomes and do not infer root cause from a single pass.


## 2026-10-18: Isolate FSM_API-backed tests from xUnit parallel execution

The source-integration workflow exposed a test-harness risk worth addressing without changing runtime behavior. Renderer tests create and destroy FSM_API instances through a shared static registry, while FSM_API documents a main-thread operations model. xUnit may execute different test classes concurrently on separate worker threads; coordinator-local locks do not serialize independent tests.

Added `FsmApiSharedRegistryCollection` with `DisableParallelization = true` and assigned both `RendererComputationMachineTests` and `ObserverRepresentationCoordinatorTests` to that collection. This serializes the tests that exercise the shared FSM_API registry while leaving unrelated Renderer tests free to run in parallel.

This is a test-isolation correction aligned with the documented API contract, not proof that the original exception was caused by test parallelism and not a runtime fix. No production disposal code was changed. Verification workflow: [run 39101008220](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/actions/runs/39101008220) (triggered by commit `17a37cae7fe503c71341cf4ef45e0f2992beaeb3`); outcome must be checked before claiming the change passes.
