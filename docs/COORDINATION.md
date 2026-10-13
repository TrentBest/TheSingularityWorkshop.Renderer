# Coordination Log — Renderer

This is the persistent coordination log for Renderer and its neighboring projects. Keep this filename stable; update its contents as work progresses.

## Log rules

- Append new dated entries; do not silently rewrite or remove earlier messages.
- Put the intended recipient first: **TO**, then **FROM**, **SUBJECT**, **STATUS**, and **MESSAGE**.
- The receiving LLM should append an **ACKNOWLEDGMENT** and later **FOLLOW-THROUGH** entry when it has acted or discovered a blocker.
- Keep entries factual, actionable, and scoped to repository boundaries.
- Newest entries go first after this rules section.

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

Report the existing types and call path you inspected, the proposed minimal contract, test plan, and any incompatibility that requires a decision from the user. Coordinate with the WebPage LLM before changing any embedded snapshot or host-facing payload.

### ACKNOWLEDGMENT

_Pending from recipient._

### FOLLOW-THROUGH

_Pending; append a dated update when action is taken or a blocker is identified._
