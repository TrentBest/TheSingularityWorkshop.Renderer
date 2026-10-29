![The Singularity Workshop animated moniker](assets/moniker.svg)

# Theory-to-Implementation Map

This is the source-backed companion to [Rendering Theory](RENDERING_THEORY.md). It connects the ideas taught in the theory chapters to the current public implementation and its focused tests. It is intentionally conservative: a passing unit test proves the behavior exercised by that test, not a complete production rendering pipeline.

## Reading the evidence

- **Executable contract** means a public type or method implements a bounded behavior.
- **Focused test** means the named test file exercises a slice of that behavior.
- **Integration proof** means a runnable sample connects multiple stages.
- **Unproven** means the repository does not yet establish that capability end to end.

## Current traceability matrix

| Theory concept | Current implementation | Focused evidence | Boundary / limitation |
|---|---|---|---|
| Event Horizon as a policy boundary | [EventHorizon.cs](../EventHorizon.cs) | [EventHorizonTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/EventHorizonTests.cs) | A horizon carries a maximum distance and preferred updates-per-second; this alone is not a multidimensional policy optimizer. |
| Distance → explicit representation decision | [ObserverRepresentationPolicy.cs](../ObserverRepresentationPolicy.cs), [ObserverDistance.cs](../ObserverDistance.cs) | [ObserverRepresentationPolicyTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/ObserverRepresentationPolicyTests.cs), [ObserverDistanceDeterminismTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/ObserverDistanceDeterminismTests.cs) | The explicit bridge selects a representation from ordered distance/horizon rules. Do not infer support for every observer signal discussed in theory. |
| FSM-backed representation state | [RendererComputationMachine.cs](../RendererComputationMachine.cs) | [RendererComputationMachineTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/RendererComputationMachineTests.cs) | The FSM represents computational target state; it is not itself a graphics backend or a universal scheduler. |
| Cadence, validity, refresh tickets, and stale-result rejection | [EventHorizonBucket.cs](../EventHorizonBucket.cs) | [EventHorizonBucketTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/EventHorizonBucketTests.cs), [EventHorizonWorkloadScenarioTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/EventHorizonWorkloadScenarioTests.cs) | The bucket is a policy/validity primitive. A host or scheduling capability must actually request and perform work. |
| Targeted dependency invalidation | [EventHorizonBucketDependencyGraph.cs](../EventHorizonBucketDependencyGraph.cs) | [EventHorizonBucketDependencyGraphTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/EventHorizonBucketDependencyGraphTests.cs) | Declared dependencies control invalidation; the graph does not automatically discover arbitrary data dependencies. |
| Bucket membership and cohort lookup | [EventHorizonBucketMembershipIndex.cs](../EventHorizonBucketMembershipIndex.cs) | [EventHorizonBucketMembershipIndexTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/EventHorizonBucketMembershipIndexTests.cs) | Membership/indexing is not proof of an optimized scene database or GPU culling. |
| Observer change and representation coordination | [ObserverRepresentationCoordinator.cs](../ObserverRepresentationCoordinator.cs) | [ObserverRepresentationCoordinatorTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/ObserverRepresentationCoordinatorTests.cs) | Coordinator behavior is tested at its contract boundary; the entire production path from world source through backend output is not one integrated runtime yet. |
| Gap-free promotion/demotion handoff | [HorizonRepresentationHandoff.cs](../HorizonRepresentationHandoff.cs) | [HorizonRepresentationHandoffTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/HorizonRepresentationHandoffTests.cs) | This coordinates selection of prepared representations; it does not create resources, schedule preparation, or perform fades. |
| Neutral frame and ordered commands | [RenderView.cs](../RenderView.cs), [RenderDrawCommand.cs](../RenderDrawCommand.cs), [RenderFrame.cs](../RenderFrame.cs) | [RenderViewTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/RenderViewTests.cs), [RenderDrawCommandTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/RenderDrawCommandTests.cs), [GpuBoundaryTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/GpuBoundaryTests.cs) | A frame is a stable command snapshot. These contracts do not prove GPU execution, rasterization, clipping, shader correctness, or pixel output. |
| Reducing compatible commands | [RenderCommandReducer.cs](../RenderCommandReducer.cs) | [RenderCommandReducerTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/RenderCommandReducerTests.cs) | Command reduction is not equivalent to geometry-buffer compaction or a guaranteed performance gain. |
| Obstruction and sensor-specific visibility | [ObstructionVisibility.cs](../ObstructionVisibility.cs) | [ObstructionVisibilityTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/ObstructionVisibilityTests.cs) | Visibility results are model/policy outputs; a host must provide the actual geometry, sensor evidence, and freshness policy required by its scenario. |
| Semantic anchors/forms | [SemanticAnchor.cs](../SemanticAnchor.cs), [SemanticForm.cs](../SemanticForm.cs) | [SemanticFormTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/SemanticFormTests.cs) | Semantic structure is not a complete AI observation pipeline. Authoritative facts remain owned by the source domain. |
| Backend-neutral MicroBundle capability | [RendererMicroBundle.cs](../RendererMicroBundle.cs) | [RendererMicroBundleTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/RendererMicroBundleTests.cs) | This makes the capability composable; it does not make Renderer depend on FSM_COS or prove an experience has loaded and executed a production rendering backend. |
| Cost model and measurement vocabulary | [RendererPerformanceModel.cs](../RendererPerformanceModel.cs) | [RendererPerformanceModelTests.cs](../tests/TheSingularityWorkshop.Renderer.Tests/RendererPerformanceModelTests.cs) | Estimates and model outputs must be labeled as estimates. Hardware cost requires actual measurements on a specified backend. |
| Living documentation artifacts | [LivingRenderer sample](../samples/LivingRenderer/Program.cs), [RenderFrame SVG proof](../samples/RenderFrameSvgProof/Program.cs), [bucket invalidation proof](../samples/BucketInvalidationReuseProof/Program.cs) | Build workflow checks samples and generated artifacts | Generated SVGs are snapshots from executable samples, not live telemetry and not proof of GPU rendering. |

## The currently honest end-to-end picture

The repository has meaningful executable foundations, including selection, buckets, invalidation, FSM-backed state, handoff, visibility models, semantic anchors, and a neutral frame/command contract. The remaining gap is not “no implementation”; it is **integration completeness and evidence depth**.

The following distinctions must remain visible in documentation and issue closure:

1. A tested policy decision is not the same as a scheduled refresh.
2. A valid bucket refresh is not the same as a selected frame.
3. A RenderFrame is not a backend execution.
4. An SVG generated from a frame is not a GPU-rendered scene.
5. A Renderer MicroBundle is not proof that an application host composes and presents it.
6. A reduction in command count is not, by itself, proof of lower end-to-end cost or preserved visual quality.

## The next vertical proof

The most useful next implementation increment is a deterministic vertical proof that starts with a small authoritative fixture and an explicit observer revision, then records:

1. the selected horizon and representation, including the reason;
2. which bucket work is due, reused, or invalidated;
3. the source and observer revisions used by the selected representation;
4. the resulting ordered RenderFrame commands;
5. a presentation artifact or adapter result, explicitly labeled by its actual output path;
6. a reproducible trace of counts, allocations, elapsed time, and stale/rejected work.

It should include an observer change and a source change, and verify that unrelated cached work remains reusable. This proof can be CPU-only initially. Do not expand public API until the existing contracts have been composed and the missing seam has been demonstrated by a failing test or a clearly documented integration requirement.

## Issue ownership and cross-repository boundaries

- [Renderer issue #29 — observer-to-presentation handoff](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/issues/29) tracks the source-backed contract audit.
- [Renderer issue #17 — multidimensional Event Horizons](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/issues/17) tracks the policy proof for phenomena, dependencies, and budgets.
- [Renderer issue #30 — interactive 2D Experience contract](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/issues/30) tracks the small consumer-facing proof.
- [GUI issue #52](https://github.com/TrentBest/TheSingularityWorkshop.GUI/issues/52) owns semantic UI and human interaction.
- [Visualize issue #31](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/issues/31) owns scene/image manifestation.
- [Experiences issue #22](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/issues/22) owns Capsule Clinic rules and authoritative state.

These are active work items, not claims that the acceptance criteria are already complete.

---

<p align="center">
  <img src="https://avatars.githubusercontent.com/u/16405167?v=4" width="48" alt="The Singularity Workshop maintainer avatar">
  <br>
  <a href="https://github.com/TrentBest">GitHub</a> ·
  <a href="https://github.com/TrentBest/TheSingularityWorkshop.Renderer">Renderer</a> ·
  <a href="https://www.nuget.org/packages/TheSingularityWorkshop.Renderer">NuGet</a> ·
  <a href="https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/DOCUMENTATION_STANDARD.md">Workshop documentation standard</a>
</p>
