# Spatial semantics, affordances, and embodied interaction

## The vision: an agent sees meaning in space

A useful AI observation is not only a label such as `"4 Ninja Turtles"`. It is a set of contextual concepts anchored to a spatially meaningful world:

- **What is present?** Four characters, a tank, an ATM, a door, a handrail.
- **Where is it relative to the observer?** In front, to the left, behind an obstruction, above, within reach.
- **What is its shape and structure?** A tank has treads, turret, hatch, maintenance panels and terminals. An ATM has a card slot, screen, keypad and cash dispenser.
- **What can be done with it?** A hatch can be opened, a panel inspected, a card inserted, a handle pulled, a button pressed.
- **What is currently true?** The ATM screen says “Please insert your card”; the card slot is at a particular pose; a door is locked; the tank hatch is occluded.
- **How certain is the observation?** Directly visible, sensed, inferred, stale, occluded, or unknown.

The aim is to give an agent a **spatially grounded semantic scene**, not merely a bag of object labels and not necessarily a pixel stream it must interpret from scratch.

The examples are conceptual design targets. They do not assert that Renderer already builds this full scene graph or recognizes arbitrary objects.

## One construct, multiple descriptions

An entity can have several complementary layers:

1. **Identity and category** — ATM, tank, soldier, character group.
2. **Spatial transform** — world position and orientation, with a defined coordinate convention.
3. **Geometry and subparts** — shell, turret, treads, slot, screen, hatch, ports.
4. **Semantic anchors** — named locations on or inside the construct, such as `card-slot-mouth`, `turret-pivot`, or `maintenance-terminal`.
5. **Affordances** — typed possible interactions, preconditions, target anchors, and success/failure feedback.
6. **Current state** — open/closed, occupied/empty, enabled/disabled, screen message, damage, motion.
7. **Observer-relative knowledge** — visibility, occlusion, sensor channel, confidence, and freshness.

This does not mean every polygon needs a semantic label. Geometry can remain detailed while semantic anchors mark the small number of locations that matter for interaction, explanation, or decision-making.

## Example: the ATM card slot

The ATM construct owns the geometry and spatial anchor for its card slot. The ATM's program or logic may own whether a transaction is available, whether a card is accepted, and what message is displayed. These responsibilities meet through a typed interaction contract.

```text
ATM construct
  geometry
    outer housing
    screen
    card-slot geometry
    keypad
    cash dispenser
  semantic anchors
    screen-surface -> world pose
    card-slot-mouth -> world pose + orientation
    keypad-center -> world pose
  affordance
    InsertCard
      target = card-slot-mouth
      preconditions = slot accepts card
      alignment tolerance = interaction policy
      completion = card crossed insertion threshold
      failure = misaligned / obstructed / rejected
  state
    screen message = "Please insert your card"
```

The `card-slot-mouth` anchor is not just a point. It needs a position and orientation, and likely a usable approach direction, opening dimensions, and tolerance. The remote card also has a pose and geometry. The interaction can validate relative pose, alignment, collision, insertion depth, and state preconditions.

If the player points the remote toward the slot but holds it at the wrong angle, the card should fail to enter in a legible, consistent way. A forgiving tolerance can keep the action cognitively simple while preserving a little embodied difficulty. The goal is satisfying resistance, not arbitrary input frustration.

This is an interaction-design target, not an implemented ATM simulation.

## Visible widgets are spatial objects too

A widget need not be only a flat UI element. Depending on the experience, it can be:

- a world-space screen with a surface pose;
- a button with a hit volume and activation affordance;
- a handle with a grasp pose and permitted motion;
- a card slot with an insertion axis and depth;
- a soldier HUD marker tied to a world entity;
- a contextual indicator that exists only when the observer has the right knowledge;
- a gesture target with a required pose, path, or dwell condition.

A useful widget contract should identify its spatial anchor, interaction volume or surface, allowed input modes, preconditions, state feedback, and accessibility/forgiveness policy. Rendering presents the widget; an interaction system validates the action; the experience owns the rules. Renderer should not grow into a UI framework or game-logic engine merely because it describes where interactions are.

## Why this matters to AI and FSM-driven agents

A soldier agent could receive a compact, structured view such as:

```text
Observation
  entity: tank-17
  category: armored_vehicle
  relation: right / ahead
  visible_parts: turret, left_tread
  occluded_parts: rear_hatch
  semantic_anchors:
    turret_pivot: known
    maintenance_terminal: unknown
  confidence: direct_visual
  freshness: current
```

Or a group-level concept such as `"4 Ninja Turtles"` can carry a group identity/category, count, spatial extent, member locations when known, and uncertainty. The natural-language phrase is a useful explanation or compact semantic token; it should not replace typed data when an FSM needs reliable conditions.

FSM_API then lets the agent's experience choose behavior based on the observation context. For example, knowing that an interactable terminal is reachable, that a hatch is behind the tank, or that two allies occupy the left sector can be sufficient to select a state without reconstructing the entire scene from pixels.

The agent must not receive hidden facts simply because the authoritative simulation has them. Every observation needs an observer and a knowledge policy. Occlusion, sensor capability, confidence, and freshness constrain the information.

## Event Horizons should scale semantic detail, not just geometry

An Event Horizon policy can select different *descriptions* of the same world entity:

- **Near / interaction-critical:** detailed parts, precise anchors, collision and affordance constraints.
- **Mid / tactical:** category, relative pose, important parts, motion, group relations.
- **Far / summary:** category counts, formation, direction, threat or objective relevance when justified by the observation rules.
- **Occluded / unknown:** retain uncertainty and possible sensor evidence without falsely claiming direct knowledge.

These are examples, not a mandatory fixed set of horizons. The policy may depend on task, observer intent, sensor model, interaction state, semantic importance, budget, and visibility—not distance alone.

A visual representation can be cheap while semantic information remains useful. Conversely, an object can be visible but irrelevant to the current agent task. Do not equate visual culling with removing an entity from simulation or erasing its semantic contribution.

## A broad implementation plan

1. Define a small, graphics-API-independent **spatial semantic observation** contract, reusing existing observer, semantic-anchor, and visibility concepts where they genuinely fit.
2. Define a **spatial affordance** concept that references named anchors and declares interaction preconditions and feedback without embedding game-specific behavior in Renderer.
3. Create a deterministic sample scene: ATM with slot/screen/keypad; tank with named parts; a small group of characters; terrain that creates occlusion.
4. Generate two synchronized views: a spatial scene/anchor visualization and an agent-facing semantic observation panel. Make the observer position and orientation visible.
5. Demonstrate an interaction attempt: show remote/card pose, target slot pose, alignment error, insertion axis, collision/precondition result, and a clear success or failure reason.
6. Feed observation facts into an actual small FSM_API agent. Show which semantic inputs caused a state transition.
7. Move the observer, change occlusion, and change interaction pose. Update visual representation, semantic knowledge, and affordance feedback together.
8. Only then connect to a host/renderer backend and measure update cost, observation cost, interaction latency, and scale.

The first demo should be broad enough to communicate the ambition, but every panel must label whether it is conceptual, generated from a deterministic sample, or connected to the production runtime.

## Architectural boundaries

- **Renderer:** observer-relative representation policy, visibility/semantic observation foundations, spatial anchors where appropriate, and platform-neutral output contracts.
- **World/experience:** authoritative entity state, object-specific geometry and meaning, simulation rules, mission and game logic.
- **Interaction system:** pose matching, collision/overlap, gestures, manipulation, and feedback contracts.
- **FSM_API agent:** state transitions and behavior decisions from explicit context.
- **Presentation host:** visualizes geometry, widgets, observations, and interaction feedback.

These layers should collaborate through explicit contracts, not collapse into one monolithic Renderer. In particular, the ATM's geometry can locate the card slot while the ATM experience decides whether it accepts the card.

## Related documentation

- [Semantic observation](SEMANTIC_OBSERVATION.md)
- [Observation model](OBSERVATION_MODEL.md)
- [Obstruction and sensor visibility](OBSTRUCTION_AND_SENSOR_VISIBILITY.md)
- [Event Horizon theory](EVENT_HORIZON_THEORY.md)
- [FSM integration](FSM_INTEGRATION.md)
- [Living Renderer](LIVING_RENDERER.md)
- [Implementation roadmap](IMPLEMENTATION_ROADMAP.md)
