# Obstruction and sensor-specific visibility

**Status:** Direction and first pure-model contract. This is not yet wired to the production render path.

## Why it exists

Event Horizon selection alone does not answer whether an observer can see or interact with an entity. A soldier behind a wall, inside a vehicle, or behind another soldier can exist and remain relevant while ordinary sight, interaction, and thermal presentation disagree about what is available.

The first step is a cheap projected-rectangle test. It answers whether an obstruction is a plausible occluder before the Renderer pays for more detailed visibility work. It must not become the final authority for pixel-perfect silhouettes, collision, or all-obstruction composition.

## Pairwise data contract

- Subject: stable entity/representation identity, projected bounds, depth, selected perception channel.
- Obstruction: stable identity, projected bounds, depth, opacity, and the perception channels it blocks.
- Observer: projection and channel policy; changing observer pose invalidates affected projected bounds.
- Result: no occlusion, partial occlusion, full rectangle occlusion, or invalid input; include the covered fraction and whether the subject may be skipped for that channel.

ProjectedRect uses a caller-defined projection-space coordinate system. Depth is also caller-defined, with the contract that smaller depth values mean nearer. This keeps the core independent of a graphics API and avoids introducing engine-specific vector types.

## Ordering and rejection

1. Reject invalid rectangles, depths, opacity, or an empty channel.
2. If the obstruction is farther away, does not overlap, or does not block the active channel, retain the subject.
3. A bounding rectangle alone never proves full occlusion. Only when a nearer opaque obstruction is explicitly marked as a known filled coverage rectangle and contains the complete subject rectangle may the subject be omitted for that perception channel.
4. If the rectangles overlap only partially, keep the subject eligible and composite the obstruction after its behind-subject contribution, or use an equivalent depth/mask solution in the selected backend.
5. If the sensor channel is not blocked, the subject remains eligible even when visible-light presentation is fully occluded.

The pairwise API does not sum coverage from multiple obstructions. Adding coverage percentages is incorrect when rectangles overlap one another. A later resolver must combine obstruction masks or otherwise account for their union before it declares an entity fully hidden.

## Sensor policy

Blocking is channel-specific, not a single global IsVisible flag. A wall may block ordinary visible light but permit a fictional thermal sensor to present a heat signature. Another material may attenuate or block both. A penetrating sensor is only a policy channel here; it does not claim realistic radar physics.

Sensor output is a different representation of the same source entity. It must not create a second authoritative entity or silently change the entity's underlying world state.

## Relationship to Event Horizons

Obstruction results are inputs to work selection:

- fully occluded for the active channel: skip channel-specific geometry/shading work where no other obligation requires it;
- partially occluded: retain the subject and resolve the obstruction using masks, depth, or compositing;
- visible through a sensor: use that sensor's representation and quality policy;
- newly exposed after observer movement or obstruction change: invalidate affected visibility results and refresh eligible representations.

Visibility does not automatically delete an entity, disable simulation, or remove interaction. Those are separate policies. For example, thermal gear may reveal an entity visually while interaction remains unavailable through a sealed wall.

## Current limits and proof

The initial C# contract provides projected-rectangle overlap, containment, pairwise coverage, depth ordering, opacity handling, per-channel blocking, and an explicit full-coverage guarantee required before a subject may be culled. Unit tests cover full suppression, partial compositing, sensor-channel bypass, far-side obstruction, and invalid bounds.

It does not yet implement camera projection, pixel masks, multi-obstruction union, depth-buffer integration, thermal rendering, interaction reachability, or Event Horizon invalidation wiring. The next vertical slice should feed observer/obstruction revisions into visibility-cache invalidation and make a real sample demonstrate visible-light versus thermal output.
