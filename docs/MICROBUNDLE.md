# Renderer MicroBundle

## Capability and composition boundary

Renderer is intended to be discovered and composed by FSM_COS as a **MicroBundle**. NuGet packages may be used to build Renderer and resolve ordinary build-time dependencies; a NuGet package is not, by itself, proof of the runtime capability-delivery path.

FSM_COS owns discovery, resolution, verification, loading, composition, and runtime lifecycle. Renderer owns its capability implementation and renderer-facing contracts. Renderer must not depend upward on FSM_COS.

## Current executable identity contract

The current executable contract is still numeric:

- `RendererMicroBundle.BundleId` is the non-zero `ulong` passed to `MicroBundleDescriptor`.
- `RendererMicroBundle.Ontology` is currently the provider label `Renderer`; it is not yet the portable artifact identity.
- The current FSM_COS `MicroBundleReference` pins `BundleId`, version, and SHA-256 content hash.
- The current MicroBundleDomain descriptor and FSM_COS catalog resolve capabilities by numeric bundle ID and version.

Consequently, the current executable identity is **(numeric BundleId, version, SHA-256)**. The numeric ID is currently treated as the domain-level MicroBundle identity by the shared MicroBundleDomain/FSM_COS contracts; it must not be described as a repository-local blob handle.

## Intended portable ontology direction

The intended human-readable semantic identity for this capability is:

    TheSingularityWorkshop.Renderer

The desired portable artifact reference is:

    (ontology string, version, SHA-256)

That string is a target semantic identity, not yet the actual lookup key used by the current MicroBundleDomain and FSM_COS APIs. Moving from the current numeric identity to a portable ontology-string identity requires coordinated changes and compatibility planning in those owning repositories. Renderer must not silently pretend that this cross-repository migration has already happened.

A future repository may still use a local numeric handle for efficient blob lookup, but such a handle must remain storage-local and distinct from the portable ontology identity. That is a target architecture, not the current contract.

## Capability family

The core Renderer capability can grow related capabilities under its intended ontology:

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

The Renderer MicroBundle provides observer-relative representation, semantic observation contracts, and computational-detail policy. It does not own FSM_COS composition or runtime lifecycle; authoritative Experience/world meaning or persistent world storage; GUI interaction semantics; Visualize's pixel/scene manifestation; or CPU/GPU provider execution and native graphics APIs.

The intended responsibility flow is:

    Experience: authoritative meaning and state
            |
            v
    Renderer: observer-relative representation
              and semantic observation contracts
            |
            v
    Computation: workload description and provider selection
            |
            +--> CPU execution MicroBundle
            +--> GPU execution MicroBundle
            |
            v
    Visualize / host: manifestation

The same authoritative entity identity and state can support multiple representations. Renderer describes the observer-relative projection; the experience remains authoritative for world state, and Visualize or another host manifests visual output.

## Implementation status

This repository contains a Renderer MicroBundle adapter and public observer/representation contracts. It does **not** claim that the complete durable artifact publisher, portable ontology-string resolver, or end-to-end repository-to-FSM_COS loading path is complete.

The current adapter and downstream identity contract must be migrated together before the intended portable ontology-string reference can be called executable. Ordinary NuGet dependencies remain build-time implementation concerns, not proof of runtime MicroBundle consumption.

## Rule for future work

> Does this belong to Renderer’s implementation, the portable MicroBundle artifact, repository resolution, FSM_COS composition, or a neighboring execution/presentation capability?

Keep each responsibility on its owning side of the boundary, and keep current executable behavior distinct from target architecture.
