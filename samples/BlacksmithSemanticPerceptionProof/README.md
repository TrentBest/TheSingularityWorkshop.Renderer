# Blacksmith semantic perception proof

Run from the repository root:

```bash
dotnet run --project samples/BlacksmithSemanticPerceptionProof/BlacksmithSemanticPerceptionProof.csproj --configuration Release
```

The deterministic fixture creates one authoritative blacksmith-shop snapshot and applies two events: a hammer strike and a quench. At each revision it derives:

- A production Renderer `SemanticObservation` for a workshop assistant with operational and maintenance channels.
- A separate `SemanticObservation` for a player with direct line-of-sight permissions, including observer-filtered relations.
- Independent `ObserverRepresentationCoordinator` instances for the assistant and player, each using the production observer policy, FSM_API representation machine, and horizon-specific refresh buckets.
- A player-facing `RenderFrame` built from the same snapshot and the player's FSM-selected representation, plus diagnostic SVGs derived from its real `RenderDrawCommand` entries.

The executable asserts revision agreement, command/visible-entity parity, separate Near/Interactive versus Mid/Near observer decisions, refresh completion, maintenance-channel access, and that neither the locked-cabinet identity nor relation endpoints disclose it to observers without access. Unknown subjects are omitted rather than represented by an identity-bearing record. The trace records each observer's selected horizon, active representation, refresh decision, and distance. It writes `blacksmith-observation-trace.json`, one SVG for each world revision, and the final `blacksmith-player-view.svg` under `artifacts/blacksmith-semantic-proof` by default.

**Evidence boundary:** this is an executable deterministic sample using Renderer frame/command and semantic-observation contracts with sample-owned world/observer policies. The contracts validate and carry observer-relative claims; the sample still supplies the world, permissions, and knowledge acquisition. The Event Horizon/FSM_API path is an actual tested Renderer integration, while the observer permissions and semantic fact acquisition remain fixture-owned. The SVG is a CPU-side diagnostic visualization, not GPU output. There is no live AI, FSM, production perception/sensor integration, or host integration.
