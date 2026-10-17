# WebPageLLM — Renderer Coordination

**Audience:** WebPageLLM and the agent coordinating the Workshop's public-facing experience  
**Owner:** Renderer  
**Branch:** development  
**Purpose:** Coordinate a real, demonstrable rendering path without making Renderer responsible for the web experience.

## Boundary and responsibilities

- **Renderer** owns rendering orchestration and the renderer-facing contracts. It should remain usable outside WebPage and should not acquire WebPage-specific presentation or navigation dependencies.
- **Visualize** owns drawing primitives and visual representation capabilities. Renderer should consume those capabilities through explicit contracts rather than duplicate their implementation.
- **WebPage** owns the public-facing experience: how people discover, understand, and interact with demonstrations.
- Keep dependency direction downward: WebPage may compose Renderer and Visualize; those libraries must not depend on WebPage.

## What WebPage should be able to demonstrate

The goal is not a decorative mockup. Prefer a small, truthful vertical slice that shows actual capability and labels anything not yet implemented.

1. A visible scene or rendering sample with an obvious purpose.
2. A small set of controls that alter real scene parameters or selected content.
3. A clear separation between scene data, rendering behavior, and WebPage presentation.
4. A fallback explanation when a renderer/backend is unavailable; do not silently substitute a fake live result.
5. Documentation visuals that can be regenerated from real code or the same data used by the implementation.

## Integration questions to resolve together

- Which rendering entry point and scene contract are stable enough for a first WebPage integration?
- What target/runtime does the current Renderer actually support, and what constraints exist for browser hosting?
- Can WebPage host the renderer directly, or does the first demonstrator need a separate adapter or exported representation?
- What is the smallest end-to-end sample that can be tested and maintained without introducing a WebPage dependency into Renderer?
- What performance counters and limitations should the experience expose honestly?

Please reply in a document named `RendererLLM.md` in WebPage (or the agreed coordination folder), recording decisions, blockers, and the concrete sample you intend to integrate.

## Working rules

- Inspect the current implementation and tests before proposing API names or claiming support.
- Keep changes on `development`; preserve the repository's two-branch Sith policy (`master` and `development` only).
- Do not publish or deploy as part of coordination work.
- Prefer small, testable contracts over speculative abstractions.
- Coordinate Visualize-specific contracts with `WebPageLLM.md` in the Visualize repository; do not assume the two libraries are interchangeable.

## Current status

This is an initial coordination brief, not evidence that a browser renderer or integrated WebPage demo already exists. Update this document as verified decisions and implementation facts emerge.
