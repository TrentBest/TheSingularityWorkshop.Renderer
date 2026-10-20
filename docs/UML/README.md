# UML source documentation

This folder contains source-backed UML for individual C# files. Every page must describe the implementation at the same repository revision, not an intended future architecture.

## Coverage

| Source file | UML page | Coverage |
|---|---|---|
| `RenderFrame.cs` | [RenderFrame.cs.md](RenderFrame.cs.md) | Documented |
| `RenderDrawCommand.cs` | [RenderDrawCommand.cs.md](RenderDrawCommand.cs.md) | Documented |
| `RenderView.cs` | [RenderView.cs.md](RenderView.cs.md) | Documented |
| `RendererComputationMachine.cs` | [RendererComputationMachine.cs.md](RendererComputationMachine.cs.md) | Documented |
| Other production, sample, and test `.cs` files | Not yet documented | Pending |

## Rules

- Create one page per `.cs` file and preserve the source path in the page title.
- Record type relationships, public contracts, validation, state/lifecycle behavior, and relevant tests.
- Separate decision-making from rendering/presentation; diagrams must not imply that Renderer itself produces pixels.
- Link source and tests where they support a claim. Mark missing evidence as a gap.
- This is the first audited slice, not complete UML coverage.
