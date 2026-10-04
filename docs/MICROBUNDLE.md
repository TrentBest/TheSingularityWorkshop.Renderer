# Renderer MicroBundle

The Renderer is a capability, not a composition-kernel responsibility. Concrete execution is deliberately outside the Renderer package: Computation defines the execution boundary, while CPU and GPU execution are independent MicroBundles.

The package therefore contains its own MicroBundle adapter: RendererMicroBundle. This keeps the number of NuGet packages small while still giving FSM_COS the correct composition boundary.

## Voltron principle

```text
Independent NuGet capability
          |
          v
   MicroBundle adapter
          |
          v
       FSM_COS
          |
          v
   RuntimeAssembly
          |
          v
       Host / Experience
```

A package may contain the implementation needed to make its capability useful and the small adapter needed to expose that capability to the composition system. FSM_COS does not need to know the implementation package directly.

The adapter is deliberately inside TheSingularityWorkshop.Renderer rather than becoming a second Renderer.MicroBundle NuGet package. Splitting every adapter into another package would recreate the composition overhead we are trying to eliminate.

## Renderer ontology

The core rendering capability declares the human-level ontology name: Renderer.

Its stable MicroBundle identity is 0x52454E4445520001.

The domain contract intentionally treats the numeric ID as opaque. The Renderer owns the meaning of this identity and can grow the family without changing FSM_COS.

```text
Renderer
├── Renderer core
├── Renderer.Cartoon
├── Renderer.Reality
├── Renderer.LowPoly
├── Renderer.Semantic
├── Renderer.Terrain
└── future rendering capabilities
```

Those are potential capabilities, not commitments to create a package for each one. A capability should become its own MicroBundle when it has an independent composition, configuration, lifecycle, dependency, or arbitration reason to exist.

## What the core bundle does

RendererMicroBundle currently does only three things:

1. identifies the Renderer capability;
2. receives its configuration opaquely from the composition host;
3. participates in arbitration without claiming composition changes.

It does not make FSM_COS responsible for rendering.

It does not define a serialization format.

It does not create a GUI.

It does not choose a GPU backend.

Those remain Renderer or neighboring capability concerns.

## Why this boundary matters

The composition crane should be assembled once and kept small.

```text
             FSM_COS
          /     |     \\
       REST   GUI   Renderer
                    |
             Renderer MicroBundle
                    |
               Computation
                    |
          +---------+---------+
          |                   |
      CPU MicroBundle    GPU MicroBundle
```

The crane does not grow a new arm every time a new capability appears. It receives another MicroBundle and installs it.

That is the intended meaning of the Workshop's modularity: **the pieces become more capable without making the crane more complicated.**

## Dependency direction

```text
MicroBundleDomain
       ^
       |
Renderer ───────► FSM_API
       |
       └── ProtocolAi

FSM_COS ───────► MicroBundleDomain
FSM_COS ───────► Renderer (only when a host/catalog chooses to compose it)
```

Renderer may depend on the neutral MicroBundle contract because it is authoring its own capability adapter. Renderer never depends upward on FSM_COS.

The same rule applies to execution: Renderer does not become a CPU/GPU implementation package. A future Computation package can contain its own MicroBundle adapter, while CPU and GPU provider packages each contain their own adapters and can be composed independently.

FSM_COS remains a consumer of the domain contract and does not acquire Renderer-specific logic.

## Rule for future capabilities

Before creating another NuGet package, ask:

> **Does this capability need independent versioning, distribution, dependency closure, configuration, lifecycle, or arbitration?**

If not, it probably belongs inside an existing package.

If yes, expose it as a MicroBundle so the composition crane can load it without learning what it is.
