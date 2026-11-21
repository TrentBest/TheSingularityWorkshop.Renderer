# Blacksmith semantic perception proof

Run from the repository root:

```bash
dotnet run --project samples/BlacksmithSemanticPerceptionProof/BlacksmithSemanticPerceptionProof.csproj --configuration Release
```

The deterministic fixture creates one authoritative blacksmith-shop snapshot and applies two events: a hammer strike and a quench. At each revision it derives:

- A semantic observation for a workshop assistant with operational and maintenance channels.
- A separate semantic observation for a player with direct line-of-sight permissions.
- A player-facing `RenderFrame` built from the same snapshot, plus a diagnostic SVG derived from its real `RenderDrawCommand` entries.

The executable asserts revision agreement, command/visible-entity parity, maintenance-channel access, and that hidden cabinet contents remain unknown. It writes `blacksmith-observation-trace.json` and `blacksmith-player-view.svg` under `artifacts/blacksmith-semantic-proof` by default.

**Evidence boundary:** this is an executable deterministic sample using Renderer frame/command contracts and sample-owned world/observer policies. The SVG is a CPU-side diagnostic visualization, not GPU output. The semantic observation is sample-owned, not a production Renderer semantic-observation API, and there is no live AI, FSM, perception, sensor, or host integration.
