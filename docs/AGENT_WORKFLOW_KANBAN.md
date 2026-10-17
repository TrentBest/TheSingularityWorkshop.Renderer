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

#### VIS-14 — Deterministic crowd-instance SVG proof
- **Owner:** Visualize agent
- **Repo / issue:** [Visualize #14](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/issues/14)
- **Implementation commits:** [fixture project](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/commit/9190629bc54cf447cff47d9a03b2a834c89043ea), [fixture program](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/commit/fce826ee750698a063ab65447af01205c0a7776f), [CI generation and validation](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/commit/b227c1b1cb800bd8720716614f423c4a3c7b558d).
- **Current evidence:** the first fixture run exposed a project-glob collision: the executable's top-level `Program.cs` was also compiled into the GUI.SVG library. Fixed by excluding `ExtremusCrowdProof/**/*.cs` from that library project. The corrected [CI run 39060642124](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/runs/39060642124) was queued at last check; the earlier failure is recorded, not hidden.
- **Next action:** inspect CI, fix any failures, and only then move to Review / CI or Done.
- **Acceptance:** repeatable SVG + JSON manifest; explicit resource-resolution assertions; CI artifact; honest CPU/SVG capability label.
- **Dependency:** after this is green, request a pinned golden bake from the Gestures owner. Do not pretend the hand-authored fixture is already a Gestures integration.

### Ready

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

#### WEBPAGE-NAV-01 — Restore expected hub destinations
- **Owner:** WebPage agent
- **Reference:** [Renderer WebPageLLM handoff](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/blob/development/docs/WebPageLLM.md)
- **Known failure:** browser smoke reported only “Welcome, About Us” where core destinations plus manifest-declared gallery entries were expected; preserve `/about-us` and do not weaken the test.
- **Unblock request:** WebPage owner restores the navigation destinations in WebPage-owned source and reports the new CI result. Renderer/Visualize will not edit WebPage source, tests, or workflows.

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
