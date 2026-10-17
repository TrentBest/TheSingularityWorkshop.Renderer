# Coordination Log — Renderer

This is the persistent coordination log for Renderer and its neighboring projects. Keep this filename stable; update its contents as work progresses.

## Log rules

- Append new dated entries; do not silently rewrite or remove earlier messages.
- Put the intended recipient first: **TO**, then **FROM**, **SUBJECT**, **STATUS**, and **MESSAGE**.
- The receiving LLM should append an **ACKNOWLEDGMENT** and later **FOLLOW-THROUGH** entry when it has acted or discovered a blocker.
- Keep entries factual, actionable, and scoped to repository boundaries.
- Newest entries go first after these rules.

---

## Message 002 — 2026-10-17

**TO:** All Workshop repository agents coordinating with Renderer / Visualize  
**FROM:** ChatGPT — Renderer / Visualize engineering  
**SUBJECT:** Bootstrap cross-repository Kanban and task ownership rules  
**STATUS:** Initial board committed; human-facing coordination only, no automated dispatch

### MESSAGE

A first cross-agent task board now lives at [docs/AGENT_WORKFLOW_KANBAN.md](AGENT_WORKFLOW_KANBAN.md) on Renderer `development`.

Please use it as the shared index, not as a replacement for each repository's issues or owned `*LLM.md` documents:

1. Each task has one owning repository/agent and one canonical issue or handoff.
2. Agents coordinate through their own named handoff docs and append acknowledgment/follow-through to the receiving repository's coordination log.
3. Update board lane and link actual evidence when status changes; do not mark a task done from prose alone.
4. Use the lanes Inbox, Ready, In progress, Waiting, Review / CI, and Done.
5. Record the exact unblock request when waiting on another agent. Do not modify a neighbor's owned source/tests/workflows to unblock it without an explicit ownership handoff.
6. Keep `development` as the working branch; leave `master` untouched. No merge-to-master, publication, deployment, or release workflow changes without explicit approval.

The first backlog snapshot links existing Renderer #7 and #9, Visualize #4, and a new Visualize #14 for the deterministic crowd-instance SVG proof. It also records the known WebPage navigation smoke failure as waiting on WebPage ownership, and distinguishes the completed city-massing specimen from the not-yet-built observer-driven city vertical slice.

This is a bootstrap markdown board. It does not assume a KanbanAi repository or an automated GitHub Project exists. A future KanbanAi should project from GitHub issues and handoff logs instead of creating a second source of truth.

### ACKNOWLEDGMENT

_Pending from recipient agents._

### FOLLOW-THROUGH

_Update when an agent acknowledges the board, starts a card, or reports a blocker._

---

## Message 001 — 2026-10-13

**TO:** LLM responsible for Visualize and its Soldier Field/WebGPU runner  
**FROM:** ChatGPT — Renderer / Visualize engineering  
**SUBJECT:** Event Horizon policy must cross the host boundary explicitly  
**STATUS:** Architecture coordination; no production browser adapter claimed

### MESSAGE

#### Current facts

- Renderer development contains the Event Horizon bucket model: refresh cadence, cache validity, invalidation propagation, membership tracking, and safe handoff between representations.
- Renderer also contains the backend-neutral projected-rectangle visibility contract. Invalid visibility input must preserve the subject conservatively; only explicit full-coverage proof can justify culling.
- Visualize's Soldier Field browser runner currently has its own hard-coded horizon refresh cadence. Renderer C# policy is **not** yet driving those JavaScript updates.
- Renderer is intentionally independent of graphics APIs and host frameworks. WebPage/browser JavaScript should not become a dependency of Renderer core.

#### Requested coordination

1. Inspect the existing Renderer bucket/policy types before creating a parallel horizon scheduler in Visualize.
2. Propose the smallest explicit handoff from Renderer policy to a host/JavaScript adapter. Keep policy decisions separate from browser timers and WebGPU command execution.
3. Define what crosses the boundary (for example, stable bucket identity, due/refresh decision, validity or invalidation information, and the policy/version identity) only after verifying the actual existing types. The examples here are questions to investigate, not a prescribed schema.
4. Add tests for deterministic policy-to-host translation and conservative behavior when input is missing, stale, or invalid.
5. Keep the first milestone narrow: demonstrate the procedural soldier, then prove that at least one independently refreshed workload is controlled by the explicit policy handoff.

#### Guardrails

- Do not claim end-to-end integration until the browser runner demonstrably consumes Renderer-derived policy.
- Do not bake WebGPU, DOM, or browser timer types into Renderer core.
- Do not assume a fixed count of horizons; count and thresholds are policy.
- Do not add speculative types without first checking existing contracts and documenting why an adapter is needed.

#### Requested response

Report the existing types and call path you inspected, the proposed minimal contract, test plan, and any incompatibility that requires a decision from the user.

### ACKNOWLEDGMENT

_Pending from recipient._

### FOLLOW-THROUGH

_Pending; append a dated update when action is taken or a blocker is identified._
