# Blacksmith semantic perception proof

Run from the repository root:

```bash
dotnet run --project samples/BlacksmithSemanticPerceptionProof/BlacksmithSemanticPerceptionProof.csproj --configuration Release
```

The deterministic fixture creates one authoritative blacksmith-shop snapshot and applies two events: a hammer strike and a quench. At each revision it derives:

- A production Renderer `SemanticObservation` for a workshop assistant with operational and maintenance channels.
- A separate `SemanticObservation` for a player with direct line-of-sight permissions, including observer-filtered relations.
- A player-facing `RenderFrame` built from the same snapshot, plus a diagnostic SVG derived from its real `RenderDrawCommand` entries.

The executable asserts revision agreement, command/visible-entity parity, maintenance-channel access, and that neither entity identities nor relation endpoints disclose the locked cabinet to observers without access. It writes `blacksmith-observation-trace.json` and `blacksmith-player-view.svg` under `artifacts/blacksmith-semantic-proof` by default.

**Evidence boundary:** this is an executable deterministic sample using Renderer frame/command and semantic-observation contracts with sample-owned world/observer policies. The contracts validate and carry observer-relative claims; the sample still supplies the world, permissions, and knowledge acquisition. The SVG is a CPU-side diagnostic visualization, not GPU output. There is no live AI, FSM, production perception/sensor integration, or host integration.
