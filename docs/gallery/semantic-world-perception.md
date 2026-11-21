# Semantic world perception: what an AI knows and what a player sees

## The exhibit

Imagine an AI-controlled character entering a blacksmith's shop.

Before any pixels are drawn, the experience already has structured world meaning: this place is a blacksmith shop; a forge is burning; a blacksmith is working a heated iron bar on an anvil; tongs and a hammer are tools; a quench barrel is nearby; the forge and hot workpiece are hazards.

Renderer's semantic-observation direction is to make that meaning available to an AI as an observer-relative observation—not require the AI to rediscover the world's ontology by examining a screenshot.

The player receives a different manifestation of the same world: warm firelight, smoke, sparks, glowing metal, tools on the bench, the blacksmith's movements, spatial audio, and atmosphere.

**One authoritative world. Different observer-specific representations.**

## Blacksmith shop: side by side

| AI semantic observation | Player-facing manifestation |
|---|---|
| Place category: blacksmith workshop | A recognizable workshop interior |
| Forge: active; heat source; hazardous radius | Fire, glow, heat shimmer, smoke, crackling |
| Workpiece: iron; heated; being shaped | Orange-hot metal on the anvil |
| Blacksmith: working; hammering action | Character animation and hammer impacts |
| Hammer: tool; currently in use | Hammer visible in the blacksmith's hand |
| Tongs: tool; available on nearby bench | Tongs among the workshop tools |
| Quench barrel: water container; usable interaction | Water barrel with surface reflections and steam effects |
| Work area: restricted / dangerous | Environmental cues and optional warning feedback |
| Current activity: forging | Repeated hammer strikes, sparks, changing workpiece state |

The semantic column is not a transcript of pixels. It is structured knowledge derived from the experience's authoritative world state and exposed according to what this AI observer is entitled and equipped to know. The visual column is a presentation of that same state.

An AI may know the forge is active even when the camera does not show the flame. Conversely, a player may see a decorative object that has no gameplay meaning. The two channels should agree where they describe the same facts, but neither must be a copy of the other.

## A gallery of worlds

These are scenario concepts for a repeatable showcase. The examples describe the kind of semantic record an agent could receive and the parallel experience a player could see; they are not claims that Renderer currently generates these records automatically.

| World | AI can receive | Player can see |
|---|---|---|
| **Blacksmith shop** | Forge state, workpiece temperature/state, tools, crafting activity, hazards, available interactions | Firelight, sparks, hammering, glowing metal, smoke, workshop layout |
| **Kitchen** | Ingredients, appliances, cooking states, recipe steps, contamination risks, available utensils | Food, steam, boiling pots, chopping, flame, kitchen layout |
| **Hospital room** | Patient and equipment roles, device alarms, room state, authorized care tasks, urgency | Beds, monitors, staff, equipment, lighting, activity |
| **Forest** | Species or entity classes, terrain, weather, water sources, tracks, hazards, confidence of sensed contacts | Trees, undergrowth, streams, wildlife, fog, changing weather |
| **Battlefield** | Friendly/hostile affiliation where known, squad membership, cover, line of sight, threats, objectives, uncertainty | Terrain, units, smoke, vehicles, muzzle flashes, movement |
| **Construction site** | Structural elements, machine states, load zones, workers, exclusion areas, task dependencies | Cranes, scaffolds, materials, workers, machinery, site geometry |
| **Vehicle cockpit** | Controls, instrument values, warnings, system status, reachable controls, failure state | Dashboard, gauges, warning lamps, outside view, control animations |
| **Classroom** | People/roles, lesson topic, learning objects, activity state, available resources | Students, teacher, board, books, demonstrations |
| **Warehouse** | Inventory identities/counts, locations, routes, shelf capacity, robot/task state, restricted zones | Racks, boxes, labels, forklifts, robots, aisle layout |
| **Unfamiliar room** | Recognized categories, spatial relations, unknown objects, confidence, unresolved affordances | The complete visual scene, including details not yet semantically classified |

The showcase should deliberately include ordinary environments, dynamic environments, safety-critical environments, and unfamiliar environments. That makes the distinction more convincing than a gallery of only combat scenes.

## What the AI should see

A semantic observation should be compact, typed, and explicit about the quality of its knowledge. For example, the blacksmith-shop observation might conceptually look like this:

```text
Observer: WorkshopAssistant
Place:
  category: BlacksmithShop
  confidence: 0.99
Entities:
  - id: forge-01
    category: Forge
    state: Active
    hazard: HighTemperature
    relation: BehindCounter
  - id: smith-01
    category: Person.Blacksmith
    activity: Forging
    relation: At(anvil-01)
  - id: workpiece-01
    category: MetalWorkpiece
    state: Heated
    activity: BeingShaped
    relation: On(anvil-01)
  - id: quench-01
    category: WaterBarrel
    affordances: [Quench]
    relation: Near(anvil-01)
Knowledge:
  freshness: Current
  source: AuthoritativeExperienceState
  unknowns: [ContentsOfClosedCabinet]
```

This is illustrative notation, not a prescribed ontology. Renderer now supplies provider-neutral contracts for stable entity identity, semantic category, relations, state, affordances, provenance, freshness, confidence, and knowledge quality. The experience still owns the vocabulary, authoritative world state, and observer permissions.

## What the player should see

The player-facing view can be rich without being the AI's data contract:

- A camera and lighting setup that communicates the room's shape.
- Animated forge flames and light spill.
- Sparks and smoke when the hammer strikes.
- A heated workpiece whose appearance changes as it is worked.
- Spatially arranged tools and work surfaces.
- Sound and interaction feedback.
- Optional UI cues when the player approaches a hazard or usable tool.

The visualizer is free to choose meshes, textures, particles, shaders, animation, sound, and UI. Those are manifestation choices. They should not be required to recover the underlying fact that the forge is active or that the workpiece is hot.

## The same event, different updates

A useful demonstration is not just a static side-by-side screenshot. Change the world and show both observations updating from the same authoritative event.

1. The blacksmith strikes the workpiece.
2. The workpiece state and forging activity revision change.
3. The semantic observation reports a new activity/state revision.
4. The visual representation changes the spark burst, workpiece appearance, and animation.
5. The quench action changes the workpiece state again; the semantic and visual views both reflect it.

The trace should show the source revision, observer identity, semantic output, selected visual representation, and what was invalidated or reused. This connects semantic meaning to Renderer’s existing interest in observer-relative representation, provenance, freshness, and dependency-aware work.

## Observer knowledge is not omniscience

The AI must not automatically receive every fact that exists in the world model. A useful system distinguishes:

- **Observed:** directly available through the observer's permitted visual or sensor channel.
- **Sensed:** available through a declared non-visual sensor.
- **Inferred:** derived from evidence, with confidence and supporting provenance.
- **Occluded:** present in the world but not currently observable through this channel.
- **Unknown:** not classified or not available.
- **Stale:** previously observed, but not refreshed within the relevant freshness window.

For example, a shop assistant may know that a forge is active from the shop's operational state, while a player character entering through a closed door may not yet know that. The observation contract must encode the observer and channel rather than leaking global world truth.

This is also where multiple agents become interesting: a player, a shopkeeper, a security observer, and a robot helper can occupy the same room and receive different legitimate knowledge.

## Why this belongs in Renderer

Renderer already treats the observer as an input to representation and computational policy. Semantic observation extends that idea beyond the visual channel:

- A player-facing observer may need detailed geometry, lighting, and interaction precision.
- An AI agent may need object identities, relations, activities, counts, affordances, and confidence.
- A remote supervisor may need summaries and exceptions.
- A sensor agent may need only the facts supported by its sensor model.

These consumers share authoritative world meaning but do not need identical representations. Renderer can help determine which representation and level of work is useful for each observer, while the experience remains authoritative for world state and the AI/FSM remains authoritative for its own decisions.

## Executable proof and evidence boundary

The first executable blacksmith proof now lives in [BlacksmithSemanticPerceptionProof](../../samples/BlacksmithSemanticPerceptionProof/README.md). Run it from the repository root:

```bash
dotnet run --project samples/BlacksmithSemanticPerceptionProof/BlacksmithSemanticPerceptionProof.csproj --configuration Release
```

The deterministic fixture applies an initial snapshot, a hammer-strike event, and a quench event. At each source revision it derives two distinct semantic observations and a player-facing `RenderFrame` from the same authoritative world snapshot. The assistant has operational and maintenance channels; the player has direct line of sight. The executable uses Renderer’s production `SemanticObservation`, `SemanticEntityObservation`, `SemanticRelationObservation`, `SemanticAffordance`, and `SemanticProvenance` contracts. It asserts that the assistant can know the fuel-valve state while the player cannot, that the locked cabinet's identity is not disclosed to either observer, that relations never point to unavailable entities, and that the player frame command count matches the player-visible semantic entity count. CI archives a machine-readable observation trace and a companion SVG.

**Evidence boundary:** this is an executable deterministic sample using Renderer’s actual frame, draw-command, and semantic-observation contracts plus sample-owned world state and observer permissions. The knowledge policy and sensor authorization remain sample-owned; the public contracts validate and transport claims but do not acquire them. The SVG is a CPU-side diagnostic visualization, not GPU output or a full pixel renderer. There is no live AI, FSM, perception, sensor, or host integration, and no calibrated timing or allocation claim.

Remaining proof work includes testing richer sensor/occlusion models, connecting observer policy to production Event Horizon selection, and adding further environments by reusing the same fixture contract.

## Related work

- [Semantic observation model](../SEMANTIC_OBSERVATION.md)
- [Spatial semantics and affordances](../SPATIAL_SEMANTICS_AND_AFFORDANCES.md)
- [Event Horizon theory](../EVENT_HORIZON_THEORY.md)
- [Living Renderer](../LIVING_RENDERER.md)
- [Contribution aggregation and provenance](../CONTRIBUTION_AGGREGATION.md)
