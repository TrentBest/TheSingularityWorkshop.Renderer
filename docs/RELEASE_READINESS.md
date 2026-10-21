# Renderer Release Readiness

This checklist records evidence required before a Renderer release. It is deliberately conservative: a passing build is necessary, but does not alone establish release readiness. Update each item from a fresh run or source inspection before a release decision.

## Current scope

Renderer supplies observer, representation, visibility, and computational-detail policy foundations. It is graphics-API independent and may be composed by FSM_COS; Renderer must not depend upward on FSM_COS. The living SVG proofs document specific behavior, not a claim that every policy handoff is a fully integrated production pipeline.

## Verification checklist

### Build, tests, and package

- [x] CI workflow defines Release restore/build/test and collects Cobertura coverage.
- [x] CI runs the opt-in FSM_COS rendering composition sample.
- [x] CI verifies the rendering-free FSM_COS consumer has no Renderer, Visualize, or GUI.SVG assembly dependency.
- [x] CI packs the Renderer project and uploads the resulting NuGet package as an artifact.
- [ ] Re-run the complete workflow against the exact release candidate commit; record the run URL and confirm every required job passes.
- [ ] Inspect the generated package contents, package metadata, target frameworks, README/license inclusion, and dependency graph.
- [ ] Verify a clean external/package-only consumer can restore, compile, and exercise the published package version.
- [ ] Confirm version, release notes, repository metadata, and compatibility claims agree across project file, README, and package.

### Executable evidence and documentation

- [x] CI regenerates the living Renderer SVGs and fails if checked-in outputs differ.
- [x] CI generates observer-driven frame, bucket invalidation/reuse, and observer-to-bucket proof artifacts.
- [x] Gallery and living-documentation guide explain the intended responsibility flow and evidence limits.
- [ ] Re-run each documented command from a clean checkout and verify the instructions match current source.
- [ ] Review docs/UML/README.md and document each relevant production C# file under docs/UML; explicitly mark generated, sample, and test sources according to the repository's documentation standard.
- [ ] Ensure diagrams are source-derived and describe current behavior; distinguish implemented behavior from intended policy contracts and exploratory work.

### CI and supply-chain hardening

- [ ] Pin every third-party GitHub Action to a verified full commit SHA; keep the human-readable release tag in a comment.
- [ ] Verify each pinned SHA corresponds to the intended upstream release and inspect action permissions and inputs.
- [ ] Validate workflow syntax and run all affected workflows after pinning.
- [ ] Review GITHUB_TOKEN permissions, third-party action permissions, secret exposure, artifact paths, and untrusted pull-request input handling.
- [ ] Confirm source-integration builds consume dependent repositories from their intended development branches and that release packaging uses the intended published dependencies.

### Release authorization

- [x] NuGet publishing is gated behind manual workflow dispatch, explicit publish=true, and the development ref.
- [ ] Before any release, inspect the exact commit, package artifact, workflow results, version collision risk, and release notes.
- [ ] Obtain explicit release authorization. Readiness is not authorization to publish.

## Known limits to preserve in release claims

- Observer and representation proofs establish only the scenarios and assertions they execute.
- Do not claim universal performance gains, complete production scheduling/cache integration, GPU execution, or end-to-end rendering unless a dedicated reproducible proof demonstrates it.
- Report benchmark results only with reproducible workloads, correctness checks, environment details, and measured resource costs.

## Release decision

**Not yet certified release-ready.** The workflow provides useful build, test, integration, package, and SVG evidence, but immutable action pinning, full UML coverage review, and a fresh release-candidate/package-consumer audit remain open. Reassess from current evidence rather than treating this document as a permanent status assertion.