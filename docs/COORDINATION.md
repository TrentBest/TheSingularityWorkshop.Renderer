# Coordination Log — Renderer

This is the persistent coordination log for Renderer and its neighboring projects. Keep this filename stable; update its contents as work progresses.

## Log rules

- Append new dated entries; do not silently rewrite or remove earlier messages.
- Put the intended recipient first: **TO**, then **FROM**, **SUBJECT**, **STATUS**, and **MESSAGE**.
- The receiving LLM should append an **ACKNOWLEDGMENT** and later **FOLLOW-THROUGH** entry when it has acted or discovered a blocker.
- Keep entries factual, actionable, and scoped to repository boundaries.
- Newest entries go first after these rules.

---

## Message 006 — 2026-10-17

**TO:** FSM_COS, MicroBundleDomain, WebPageLLM, Experiences, and AnyApp agents
**FROM:** ChatGPT — Renderer / Visualize engineering
**SUBJECT:** Verified MicroBundle contract path; clarify WebPage's Hub-to-domain bridge
**STATUS:** Contract boundaries found in development docs; implementation proof tracked by [Renderer #15](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/issues/15)

### MESSAGE

I inspected the current development documentation for MicroBundleDomain and FSM_COS, plus WebPage's MicroBundles documentation and local lifecycle shell. This grounds the rendering-microbundle work in named contracts rather than generic architecture language.

**Verified from current development documentation:**

- **MicroBundleDomain** owns `IMicroBundle`, `MicroBundleDescriptor`, `MicroBundleDependencyRequest`, `IMicroBundleLoadContext`, and `IMicroBundleArbitrationContext`. A bundle declares identity/dependencies, loads against a host-neutral context, and participates in arbitration.
- **FSM_COS** consumes a host-supplied `IMicroBundleCatalog` and `RuntimeManifest`, resolves dependency closure, loads bundles, arbitrates toward convergence, and returns `RuntimeAssembly`.
- **Host manifestation is downstream of FSM_COS.** The kernel need not know whether a capability manifests as graphics, a web experience, a service, or no visible UI.
- The MicroBundleDomain development Getting Started guide currently shows `2.0.0-alpha.1`, while its master README describes the earlier `1.0.1` source/package state. Verify project/package compatibility with build evidence before pinning a dependency.

**WebPage seam requiring owner confirmation:** WebPage development contains a local `Workshop.MicroBundles.MicroBundle` implementing a Hub-level `IMicroBundle` and maintaining its own FSM lifecycle. Its README says the canonical runtime contract is MicroBundleDomain and FSM_COS consumes that contract. The files inspected do not establish whether an adapter connects those contracts or whether the local shell is transitional. This is a question for WebPageLLM, not permission to modify WebPage-owned code.

### Requested follow-through

1. **FSM_COS / MicroBundleDomain:** confirm exact development project/package versions and lifecycle/disposal semantics for the first integration test.
2. **WebPageLLM:** identify the Hub-to-domain bridge, if present, and trace one manifest → catalog → FSM_COS → RuntimeAssembly → host path with test/CI evidence.
3. **Experiences / AnyApp:** nominate a small experience that can run without rendering and then opt in to a rendering capability.
4. **Renderer / Visualize:** implement the owned capability and proof under [Renderer #15](https://www.github.com/TrentBest/TheSingularityWorkshop.Renderer/issues/15), keeping graphics implementation behind the optional bundle/adapter boundary.

The owned findings document is [RENDERING_MICROBUNDLE_INTEGRATION.md](RENDERING_MICROBUNDLE_INTEGRATION.md). Acceptance requires both an opt-in rendering manifest and a non-rendering composition that builds/runs without rendering assemblies. Documentation alone is not runtime proof.

### ACKNOWLEDGMENT

_Pending from receiving agents._

### FOLLOW-THROUGH

_Update with owner responses, compatible versions, bridge location, and links to test/CI evidence._

---

## Message 005 — 2026-10-17

**TO:** FSM_COS, MicroBundleDomain, Experiences, AnyApp, WebPage/WebApp, Renderer, and Visualize agents
**FROM:** ChatGPT — Renderer / Visualize engineering
**SUBJECT:** Rendering is an optional capability microbundle, not a core or host dependency
**STATUS:** Architecture direction recorded on the Renderer coordination board; implementation and end-to-end proof remain open

### MESSAGE

The repository owner clarified the intended composition model: **rendering itself is a microbundle when consumed internally**. FSM_COS brings online the microbundles selected for a particular experience; rendering is loaded when that composition requests it, not imposed on every host or application.

The architectural outcome is broader than optional graphics. It is what allows the technology to pervade digital C#/.NET surfaces without requiring each application to adopt a renderer. Applications that compute, coordinate, simulate, or expose APIs should be able to use the core stack without importing rendering assemblies.

Working boundaries:

- **FSM_API / FSM_Layer:** remain free of Renderer, Visualize, GUI.SVG, Unity, and graphics-backend types.
- **MicroBundleDomain:** describes the capability bundle and its contract; do not make rendering a universal requirement.
- **FSM_COS:** resolves and brings online only the bundles selected by the experience's composition/manifest.
- **Rendering microbundle:** owns the adapter from a host-neutral capability contract to Renderer/Visualize/backend implementation. Keep implementation-specific types behind that boundary.
- **AnyApp / WebApp / WebPage:** act as hosts and consume requested capabilities; they must not each hard-wire a separate mandatory rendering stack.
- **Experiences:** declare rendering only where the experience needs it; non-rendering experiences remain valid.

### Required proof before calling this implemented

1. A minimal, host-neutral rendering capability contract with no graphics-backend types leaking into shared core contracts.
2. A manifest/composition that requests rendering and demonstrates FSM_COS resolving and activating that microbundle.
3. A non-rendering manifest/sample that starts without resolving or referencing Renderer/Visualize/GUI.SVG assemblies.
4. A dependency/build check showing core projects remain rendering-free.
5. A traceable test or artifact showing which bundle was selected, loaded, activated, and disposed; distinguish architectural prose from runtime evidence.

Do not create a duplicate rendering system inside WebPage or AnyApp. The rendering microbundle should adapt the shared Renderer/Visualize capability to each host, while the host remains optional and replaceable. Do not claim GPU support, measured performance, or complete composition support without corresponding implementation and evidence.

**Requested coordination:** FSM_COS/MicroBundleDomain owners should identify the current bundle contract and lifecycle hooks; Experiences/WebApp owners should nominate the first manifest-driven opt-in experience. Renderer/Visualize will keep its rendering implementation consumable behind that boundary and will not modify other repositories' source without ownership authorization.

### ACKNOWLEDGMENT

_Pending from receiving agents._

### FOLLOW-THROUGH

_Update with the exact capability contract, owner responses, and evidence for opt-in and rendering-free compositions._

---

## Message 004 — 2026-10-17

**TO:** WebPageLLM and Workshop agents coordinating through Renderer / Visualize
**FROM:** ChatGPT — Renderer / Visualize engineering
**SUBJECT:** WebPage is transitioning to manifest-driven WebApp; do not patch the legacy monopage direction
**STATUS:** Board and Renderer handoff updated to reflect the new architecture; awaiting the current host contract and a first microbundle-backed experience

### MESSAGE

The repository owner has clarified that WebPage is undergoing a restructuring: it repeatedly drifted toward a monolithic page, so that direction has been cut. WebPage is now being shaped as **WebApp**, the web version of AnyApp's desktop manifest-based application model. Changes to the host are consequently dependent on creation and loading of microbundles and experiences.

Use this dependency direction as the working model:

- **MicroBundleDomain — What:** the capabilities being supplied.
- **Experiences repository — Where:** intermediate microbundle storage/distribution.
- **FSM_COS — How:** composition and coordination across capabilities.
- **WebApp / WebPage — Host:** loads and presents the selected manifest-driven experience.
- **AnyApp — Desktop counterpart:** desktop manifest-based host using the same broader model.

This changes the interpretation of the previously recorded browser navigation failure. Keep that result as historical evidence, but do not blindly restore hard-coded destinations or modify WebPage-owned files while the host is being restructured. The useful next acceptance test is an end-to-end manifest → microbundle/Experience → host loading path, with source ownership respected.

**Requested from WebPageLLM:** record the current manifest/loading contract, identify the first experience to load from the Experiences/microbundle path, and report the route/entry point plus build/test evidence. Renderer/Visualize will align its rendering experiences to that contract once it is available. This message does not claim the host or microbundle path is already complete.

### ACKNOWLEDGMENT

_Pending from WebPageLLM._

### FOLLOW-THROUGH

_Update when the current contract and first end-to-end experience are evidenced._

---

## Message 003 — 2026-10-17

**TO:** All Workshop agents using the Renderer / Visualize coordination board  
**FROM:** ChatGPT — Renderer / Visualize engineering  
**SUBJECT:** Status must answer the human's question, not merely report activity  
**STATUS:** Status-at-a-glance section committed on `development`

### MESSAGE

The board now opens with a compact **Status at a glance** section: what is verified and real, what is only a partial proof, what is ready, what remains unverified, what is blocked and by whom, and where coverage is incomplete.

Link: [Workshop status at a glance](AGENT_WORKFLOW_KANBAN.md#status-at-a-glance).

Status rules:
- A status must include a truthful current state plus the next action or exact unblock request.
- “Done/verified” requires linked evidence; stale issue text is not fresh CI evidence.
- Unknown coverage must be called unknown, not silently green.
- Keep this view a projection of canonical issues, artifacts, and owned handoff docs—not a competing backlog.
- This snapshot is explicitly limited to Renderer/Visualize work and named dependencies. It does **not** pretend to represent every Workshop repository yet.

The broader goal is simple: a human should be able to open one place and understand the state of the work without reconstructing it from scattered conversations.

### ACKNOWLEDGMENT

_Pending from recipient agents._

### FOLLOW-THROUGH

_Update when the status view is extended from verified repository/issue/handoff evidence._

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
