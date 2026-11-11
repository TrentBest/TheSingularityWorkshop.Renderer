# Renderer MicroBundle

## Capability and composition boundary

Renderer is a capability artifact that FSM_COS discovers and composes as a **MicroBundle**. NuGet is not the runtime capability-distribution or consumption contract for Renderer.

The Renderer repository may use ordinary NuGet dependencies while building its implementation (for example FSM_API and neutral domain contracts). A build/package artifact is not, by itself, the runtime MicroBundle. The MicroBundle artifact is resolved by its semantic identity and version, verified by its content digest, and loaded by FSM_COS through the Workshop repository/resolution boundary.

    Experience requirement
            |
            v
    String ontology identity + version
            |
            v
    Workshop repository / resolver
            |  locate artifact; verify SHA-256
            v
    Renderer MicroBundle artifact
            |
            v
    FSM_COS composition
            |
            v
    Runtime capability
            |
            v
    Host / Experience

FSM_COS owns discovery, resolution, loading, composition, and configuration. Renderer owns its capability implementation and its public semantic contract. Renderer must not depend upward on FSM_COS.

## Canonical identity

The canonical ontology identity for this capability is the stable string:

    TheSingularityWorkshop.Renderer

This string is the semantic identity used in requirements, manifests, and portable artifact references. It is not a repository address, NuGet package coordinate, or storage location.

A portable artifact reference is the tuple:

    (ontology string, version, SHA-256)

The SHA-256 digest identifies the exact artifact bytes. Version expresses the capability's declared release/version lineage. The ontology string identifies what capability the artifact provides.

A repository may map that portable identity to a local non-zero ulong handle for efficient blob lookup. That numeric handle is **repository-local only**: it is not the ontology, must not be exchanged as the portable identity, and may differ between repositories or storage instances.

    Portable identity
      ontology string + version + SHA-256
              |
              v
    Repository-local mapping
      opaque non-zero ulong handle
              |
              v
    Stored artifact blob

Do not publish a numeric constant as Renderer’s stable MicroBundle identity. Do not infer semantic identity from a package name or a blob handle.

## Capability family

The core Renderer capability can grow related capabilities under its ontology:

    TheSingularityWorkshop.Renderer
    ├── Renderer core
    ├── Renderer.Cartoon
    ├── Renderer.Reality
    ├── Renderer.LowPoly
    ├── Renderer.Semantic
    ├── Renderer.Terrain
    └── future rendering capabilities

These names illustrate possible capability boundaries, not a commitment to create a separate artifact for each one. A capability should become an independently resolved MicroBundle when it has a distinct composition, configuration, lifecycle, dependency, versioning, or arbitration reason to exist.

## Ownership

The Renderer MicroBundle provides observer-relative representation and computational-detail policy. It does not own FSM_COS composition or runtime lifecycle; authoritative Experience/world meaning or persistent world storage; GUI interaction semantics; Visualize's pixel/scene manifestation; or CPU/GPU provider execution and native graphics APIs.

The intended responsibility flow is:

    Experience: authoritative meaning and state
            |
            v
    Renderer: observer-relative representation
            |
            v
    Computation: workload description and provider selection
            |
            +--> CPU execution MicroBundle
            +--> GPU execution MicroBundle
            |
            v
    Visualize / host: manifestation

The same authoritative entity identity and state can support multiple representations. Renderer chooses what representation is appropriate; Visualize or another host manifests it. Neither layer becomes the owner of Experience meaning.

## Implementation status

This document defines the required artifact identity and composition boundary. It does **not** claim that this repository already contains a complete durable artifact publisher, repository resolver, FSM_COS loader, or end-to-end runtime composition path. Those require separate implementation and executable integration tests.

The current .NET project and its NuGet dependencies are build-time implementation concerns. They must not be presented as proof that FSM_COS consumes Renderer as a NuGet capability.

## Rule for future work

> Does this belong to Renderer’s implementation, the portable MicroBundle artifact, repository resolution, FSM_COS composition, or a neighboring execution/presentation capability?

Keep each responsibility on its owning side of the boundary.