# RenderFrame — UML

**Source:** [`RenderFrame.cs`](../../RenderFrame.cs)  
**Companion and use case:** [RenderFrame.cs.md](RenderFrame.cs.md)

```mermaid
classDiagram
    direction TB
    class RenderView
    class RenderDrawCommand
    class RenderFrame {
        +RenderFrame(RenderView view, IEnumerable~RenderDrawCommand~ commands)
        +View RenderView
        +Commands IReadOnlyList~RenderDrawCommand~
    }
    RenderFrame --> RenderView : stores
    RenderFrame "1" o-- "0..*" RenderDrawCommand : ordered snapshot
```

## Structural contract

The constructor materializes the supplied command sequence into an array-backed read-only snapshot. Renderer owns platform-neutral observer decisions and commands; this type does not submit GPU work or create pixels.
