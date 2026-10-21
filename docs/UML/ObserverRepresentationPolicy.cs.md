# ObserverRepresentationPolicy.cs

**User moniker:** Observer-to-representation decision policy

## Purpose

Map observer distance to an explicit representation target using an ordered set of Event Horizon rules. This provides a stable, inspectable decision for downstream computation without requiring callers to parse descriptive horizon names.

## Source and executable evidence

- Implementation: [ObserverRepresentationPolicy.cs](../../ObserverRepresentationPolicy.cs)
- Distance selection primitive: [ObserverDistance.cs](../../ObserverDistance.cs)
- Horizon contract: [EventHorizon.cs](../../EventHorizon.cs)
- Representation vocabulary: [RendererRepresentation.cs](../../RendererRepresentation.cs)
- Tests: [ObserverRepresentationPolicyTests.cs](../../tests/TheSingularityWorkshop.Renderer.Tests/ObserverRepresentationPolicyTests.cs)

## Class card

| Type | Responsibility |
|---|---|
| `RepresentationHorizonRule` | Immutable mapping from one Event Horizon to one valid `RepresentationTarget` |
| `ObserverRepresentationDecision` | Carries the requested distance, selected horizon, representation, horizon name, and preferred cadence |
| `ObserverRepresentationPolicy` | Validates ordered, unique rules and selects the mapped representation for a distance |

## Observed constraints

- At least one rule is required; null rules and duplicate horizon objects are rejected.
- Rules must be ordered from nearest to farthest by non-decreasing maximum distance.
- `Select` delegates distance-to-horizon selection to `ObserverDistance.Select`, then maps the selected horizon through the explicit rule dictionary.
- The decision retains the horizon object so later stages can reuse its identity for bucket/cadence selection.
- This policy does not advance FSM_API, schedule bucket refreshes, determine occlusion, create a RenderFrame, or render pixels.

## Use case

A host asks the policy which representation is appropriate for a given observer distance, inspects the decision, and then separately chooses which computation or presentation stage to run. Horizon names remain descriptive metadata, not an encoded protocol.

## UML

See [ObserverRepresentationPolicyUML.md](ObserverRepresentationPolicyUML.md).

## Evidence boundary

The policy establishes a representation decision, not successful downstream execution. GPU, performance, and end-to-end rendering claims require separate reproducible evidence.
