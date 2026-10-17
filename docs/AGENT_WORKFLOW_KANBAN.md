# Agent Workflow Kanban

> **Purpose:** one small, durable coordination surface for directing parallel agents across the Workshop repositories. This is the initial board, not a claim that a GitHub Projects board or automated dispatcher already exists.

**Board owner:** Renderer/Visualize coordination  
**Canonical branch:** `development`  
**Last reconciled:** 2026-10-17  
**Out of scope for this board owner:** Physics implementation; WebPage-owned source, tests, and workflows.

## Operating model

The board is an index, not a second source of technical truth:

- **Each task's owning repository and GitHub issue are its source of truth.** Keep acceptance criteria, discussion, and closure there.
- **Each agent communicates across repository boundaries through its owned `*LLM.md` handoff or the recipient repository's coordination log.** Include a direct task/issue link and exact request; the receiving agent records acknowledgment and follow-through.
- **This file is the cross-repository view.** Update the card when a task changes lane, add dependencies/blockers, and link evidence. Do not copy whole issue bodies here.
- **No silent scope transfer.** The listed owner can implement only in their repository. Consumers request a contract or fixture from the source owner; they do not edit the source owner's repository.
- **No status by optimism.** “Done” requires acceptance criteria and evidence (tests/CI/artifact/link). A document or a compile is not runtime proof.
- **No release side effects by default.** Use `development`; leave `master` alone. No merges to master, publishing, deployment, or enabling release workflows without explicit approval.
- **Start small.** Each agent should have at most one primary implementation card in progress, plus a small review/coordination task. If blocked, record exactly what is needed and move it to **Waiting** rather than silently starting duplicate work.

## Status at a glance

> **The answer you should get in one glance:** what is real, what is moving, what is blocked, and what needs a decision. This is a verified snapshot of the work tracked by this board—not a claim that every Workshop repository has been audited.

**Overall:** useful proof work is landing; the main risk is integration visibility across repository/agent boundaries. WebPage is undergoing a deliberate host-architecture transition; its next status must be measured against the manifest → microbundle/Experience loading path, not legacy monolithic-page assumptions.

| Area | Status | What is true now | Next move / unblock |
| --- | --- | --- | --- |
| Visualize crowd proof | **Verified** | CI passed; deterministic 24-instance SVG and JSON manifest artifact exists. Input is a hand-authored fixture, not Gestures output. | Get a pinned, provenance-bearing golden bake from the Gestures owner. |
| Rendering capability boundary | **Architecture direction established; integration not yet evidenced** | Rendering should be consumed as an optional microbundle loaded by FSM_COS when the selected experience requests it. Renderer/Visualize implementation details must stay behind the bundle contract; core and non-rendering .NET apps must not inherit rendering dependencies. | Define the minimal rendering capability contract and prove one opt-in load path plus one host/experience that runs without it. |\n| City rendering | **Partial proof** | CPU/SVG specimen demonstrates distant window draw-command omission only. No GPU or measured performance claim. | Build the observer-dependent vertical slice with WebPageLLM; keep ownership boundaries intact. |
| Visualize lighting | **Ready** | Issue #4 is scoped to deterministic light data, revisions/invalidation, and reference-scene tests. | Start the narrow testable slice. |
| Renderer bucket arbitration | **Ready** | Issue #7 still needs implementation evidence for forge-time preparation versus runtime execution. | Add concrete tests and a configuration-free manifest path. |
| Event Horizon continuity | **Needs verification** | Issue #9 references draft PR #8; the board does not yet record verified passing CI. | Check current PR and CI before changing status. |
| WebPage / WebApp restructuring | **Transition in progress (external owner)** | WebPage is being restructured from a monolithic-page direction into the web counterpart of manifest-based AnyApp. Capabilities and experiences are expected to arrive through microbundles/Experiences, rather than accumulating in the host. The earlier navigation smoke failure is historical context, not an instruction to patch the restructuring branch blindly. | Await WebPageLLM's manifest/experience loading contract and the first microbundle-backed experience; then verify host discovery/loading and CI against the new architecture. |
| Workshop-wide status | **Incomplete coverage** | This board contains the Renderer/Visualize work and named cross-agent dependencies, not a live inventory of every repository. | Expand from authoritative repo/issue/handoff evidence, one owner at a time; label unknowns instead of guessing. |

**Decision needed from you:** none to continue this status-first approach. I will not turn unknown or stale information into a green status.

## Lanes

| Lane | Meaning |
| --- | --- |
| **Inbox** | Newly surfaced; not yet scoped or assigned. |
| **Ready** | Owner, acceptance criteria, and next action are clear; safe to start. |
| **In progress** | An agent is actively changing its owned repo. |
| **Waiting** | Blocked on another agent, user decision, external capability, or CI. State the exact unblock request. |
| **Review / CI** | Implementation exists; needs review, automated validation, or evidence. |
| **Done** | Acceptance criteria met and evidence linked. |

## Current board

### In progress

### Ready

#### RENDERING-MICROBUNDLE-01 — Keep rendering optional at the FSM_COS composition boundary
- **Owner:** FSM_COS for composition/loading contract; Renderer and Visualize for rendering implementation; consuming host for experience wiring
- **Status:** Architecture direction established; no end-to-end optional-load proof recorded yet.
- **Principle:** rendering is an optional capability microbundle, loaded only when selected by the experience/FSM_COS composition. Core state, coordination, simulation, and non-rendering .NET applications must not take a mandatory dependency on Renderer, Visualize, GUI.SVG, or a graphics backend.
- **Next action:** define the smallest host-neutral capability contract; demonstrate both (a) a manifest that loads rendering and (b) a non-rendering manifest that starts without loading or referencing rendering assemblies.
- **Acceptance evidence:** dependency graph/build test proves core is rendering-free; integration test proves opt-in bundle resolution and activation; a non-rendering sample passes without rendering packages. No claim of implementation until these tests exist.

#### VIS-04 — First testable lighting slice
- **Owner:** Visualize agent
- **Repo / issue:** [Visualize #4](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/issues/4)
- **Next action:** keep it narrow: deterministic light source data, revisions/invalidation, and reference-scene contract tests before choosing a graphics backend.
- **Dependency:** existing public frame/resource contracts; no speculative backend commitment.

#### REN-07 — Forge-time bucket arbitration and prepared FSM pipelines
- **Owner:** Renderer agent
- **Repo / issue:** [Renderer #7](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/issues/7)
- **Next action:** align the implementation with forge-time preparation versus runtime execution; preserve bundle-owned defaults and a valid configuration-free manifest.
- **Acceptance:** concrete tests and an explicit source path, not only architectural prose.

### Review / CI

#### VIS-14 — Deterministic crowd-instance SVG proof
- **Owner:** Visualize agent
- **Repo / issue:** [Visualize #14](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/issues/14)
- **Implementation commits:** [fixture project](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/commit/9190629bc54cf447cff47d9a03b2a834c89043ea), [fixture program](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/commit/fce826ee750698a063ab65447af01205c0a7776f), [CI generation and validation](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/commit/b227c1b1cb800bd8720716614f423c4a3c7b558d), [compile-glob correction](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/commit/506da051b83c080a33ca33ce11d979497ec8da1b).
- **Evidence:** [CI run 39060642124 passed](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/runs/39060642124); both build/test and GUI.SVG package-proof jobs succeeded. The run uploaded [visualize-extremus-crowd-proof artifact](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/runs/39060642124#artifacts).
- **Result:** deterministic hand-authored 24-instance crowd SVG + JSON manifest, repeatability check, missing-resource assertion, CI artifact. This is a CPU/SVG fixture—not a Gestures bake, GPU instancing proof, pose-animation proof, or performance benchmark.
- **Next action:** ask the Gestures owner for a pinned golden bake and agree on its payload/provenance contract before upgrading this to a cross-repository integration.
- **Acceptance:** fixture proof itself is CI-green; keep the next integration milestone separate until real Gestures input and a traceable output are present.


#### REN-09 — Event Horizon handoff continuity
- **Owner:** Renderer agent
- **Repo / issue:** [Renderer #9](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/issues/9)
- **Current evidence:** issue reports the handoff primitive and deterministic acceptance tests in draft PR #8; issue text says CI validation is still pending.
- **Next action:** inspect the latest PR/CI result; verify readiness, stale revision, observer movement, near/far transitions, and no-gap/no-implicit-duplicate cases before calling it done.

#### VIS-CITY-01 — City massing specimen
- **Owner:** Visualize agent (specimen); Renderer agent (policy)
- **Evidence:** [Visualize city-massing handoff](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/blob/development/docs/integrations/SINGULARITY_CITY_RENDERING_HANDOFF.md) and [successful CI run / artifact](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/runs/39003350163)
- **Status:** narrow CPU/SVG policy-consumption proof completed. It shows command-level omission of distant window-slot draws; it does **not** establish geometry compaction/reuse, complete city navigation, GPU execution, or a measured performance win.
- **Next action:** do not expand the claim. The next city milestone is an actual observer-dependent vertical slice with stable identity and traceable frame output, coordinated with WebPageLLM.

### Waiting

#### WEBAPP-EXPERIENCES-01 — Integrate the manifest-driven WebApp host with microbundle-backed experiences
- **Owner:** WebPage agent for host; each microbundle/experience remains with its source owner
- **Reference:** [Renderer WebPageLLM handoff](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/blob/development/docs/WebPageLLM.md)
- **Architecture update (2026-10-17):** WebPage is being restructured away from the monolithic-page direction into the web version of AnyApp. The host should load experiences through the manifest/microbundle model; the Experiences repository is the intermediate microbundle storage/distribution location.
- **Status:** Waiting for WebPageLLM's current host contract and a concrete first experience path. The previously observed navigation smoke failure is historical evidence only; do not treat it as a mandate to restore hard-coded destinations or interfere with the restructuring.
- **Unblock request:** publish the current manifest schema/loading contract and identify one microbundle-backed experience end-to-end. Renderer/Visualize will not edit WebPage source, tests, or workflows.

### Done

#### COORD-01 — Record the cross-agent work contract
- **Evidence:** this file plus the Renderer coordination log entry linking it.
- **Completion rule:** agents use task links and their owned handoff logs; future KanbanAi may consume this board format, but is not assumed to exist.

#### VIS-CROWD-HANDOFF-01 — Document the current crowd data boundary
- **Evidence:** [Visualize crowd presentation contract](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/blob/development/docs/integrations/SINGULARITY_CITY_RENDERING_HANDOFF.md#2026-10-17-extremus-crowd-presentation-contract-visualize-owner-response)
- **Result:** documents the CPU layout-bake boundary, instance payload limitations, and honest SVG/backend claims.

## Cross-agent task protocol

Every new card should state these fields, either here or in the owning issue:

1. **Outcome:** what visible or testable thing will exist when finished?
2. **Owner:** one agent/repository accountable for implementation.
3. **Source of truth:** issue or owned handoff file.
4. **Next action:** one concrete step, not a broad aspiration.
5. **Acceptance evidence:** test, CI run, generated artifact, screenshot, or measured report.
6. **Dependencies / unblock request:** name the upstream owner and exact contract/output needed.
7. **Boundary:** repositories/files this agent must not change.

When an agent starts, move the card to **In progress** and append a dated entry to its owned coordination log if cross-repo coordination is involved. When it finishes, link the commit and CI/artifact evidence, then move to **Review / CI** until acceptance passes. A task is **Done** only after evidence is checked. When waiting, include the request and who can unblock it.

## Future KanbanAi direction (not implemented)

Treat this markdown board as the bootstrap interface, not as a permanent bespoke task database. A future KanbanAi should ideally **read existing GitHub issues and `*LLM.md` handoff documents**, then project them into one board; it should not create a competing backlog. Useful later capabilities are:
- cross-repository cards with one authoritative owner;
- explicit dependencies and blocked-by links;
- agent identity, heartbeat, and last meaningful update;
- review/approval lane distinct from agent-reported completion;
- stale-card detection and a daily/turn-level summary;
- human approval gates for merges, publishing, deployment, or scope changes.

Do not automate dispatch, claim completion, or mutate task status based on model prose alone. Start with visibility and reliable source links; add orchestration only after the read model is trustworthy.
