# Repository-Source Development and NuGet Release Mode

## Purpose

Develop several Workshop repositories together without waiting for every intermediate NuGet package to be published, while keeping NuGet as the normal distribution contract for released packages.

**Rule of thumb**

- **Development/integration:** check out the dependency repositories' `development` branches and compile against their source with conditional `ProjectReference` entries.
- **Release/consumer verification:** use the consumer repository's normal `PackageReference` entries and explicit package versions from NuGet.
- **Default must remain safe:** ordinary builds and package workflows use NuGet unless a developer explicitly opts into source mode. Never make a package publish happen merely because source mode was used.

A `ProjectReference` does not fetch a Git branch by itself. The dependency repository must first be checked out at the intended branch/commit; MSBuild then builds the local project.

## 1. Establish a workspace

Use sibling checkouts so relative paths are predictable. For example:

```text
Workshop/
  TheSingularityWorkshop.Renderer/       # repository being changed
  FSM_API/                               # dependency source
  TheSingularityWorkshop.ProtocolAi/     # dependency source
  TheSingularityWorkshop.MicroBundleDomain/
```

Clone each repository if it is not already present. In each dependency checkout, fetch and check out its current `development` branch:

```bash
git fetch origin
git switch development
git pull --ff-only origin development
```

Repeat only for dependencies the project actually needs. Do not assume every dependency repository has a `development` branch: verify its branch policy first. If a dependency has no `development` branch, stop and choose the intended integration ref with the repository owner rather than silently using `master`.

Record the dependency commits used for a test when results need to be reproducible. A branch name moves; a commit SHA identifies the exact source tested.

## 2. Add an opt-in source mode to the consuming project

Inspect the consumer's `.csproj` and each dependency's actual project path and target frameworks first. Do not copy placeholder paths blindly. For each direct package dependency that should be built from source, make the package and project references mutually exclusive.

Illustrative MSBuild pattern:

```xml
<PropertyGroup>
  <!-- Safe default: normal builds consume published packages. -->
  <WorkshopUseRepositorySources Condition="'$(WorkshopUseRepositorySources)' == ''">false</WorkshopUseRepositorySources>
  <!-- Override this for a different sibling-workspace location. -->
  <WorkshopSourceRoot Condition="'$(WorkshopSourceRoot)' == ''">$(MSBuildThisFileDirectory)..</WorkshopSourceRoot>
</PropertyGroup>

<ItemGroup Condition="'$(WorkshopUseRepositorySources)' != 'true'">
  <PackageReference Include="The.Dependency.Package" Version="1.2.3" />
</ItemGroup>

<ItemGroup Condition="'$(WorkshopUseRepositorySources)' == 'true'">
  <ProjectReference Include="$(WorkshopSourceRoot)/The.Dependency.Repository/src/The.Dependency.Project.csproj" />
</ItemGroup>
```

Replace the package ID, version, repository directory, and project path with values verified from the real repositories. Adapt the root calculation to the consumer's location. Repeat the pattern for each dependency selected for source mode, including the relevant transitive source dependencies when necessary.

**Important safeguards**

- Never include a `PackageReference` and a `ProjectReference` for the same assembly in the same build.
- Keep source mode opt-in and off by default.
- Keep dependency direction intact: a lower-level package must not gain a reference to a higher-level application or composition layer.
- Check target-framework compatibility before adding a project reference.
- Do not add every repository to every project. Replace only the package references needed for the current development task.
- Avoid pinning a moving `development` branch inside the project file. Branch selection belongs to workspace setup/CI; the project file points at local source.

## 3. Build and test in source mode

From the consumer repository, explicitly opt in:

```bash
dotnet restore -p:WorkshopUseRepositorySources=true
dotnet build -c Debug -p:WorkshopUseRepositorySources=true
dotnet test -c Debug -p:WorkshopUseRepositorySources=true
```

If the solution contains several projects, confirm that the property reaches each relevant project. Check the build output/project graph to ensure the intended local projects were selected and no stale NuGet copy of the same assembly is also being used.

Before committing, inspect `git status` in every checkout. A source-mode integration test can span multiple repositories, but each repository still owns its own changes and pull request. Do not commit dependency edits into the consumer repository accidentally.

## 4. Verify the normal NuGet path separately

Source-mode success does **not** prove the published package graph works. In a clean checkout or separate CI job, use the default mode with the explicit `PackageReference` versions:

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
dotnet pack -c Release
```

Do not pass `WorkshopUseRepositorySources=true` to the NuGet verification/pack job. If the package is intended for external consumers, verify restore/build/test against the public NuGet feed rather than relying on locally built project outputs or cached packages.

A useful CI arrangement is two independent lanes:

1. **Repository-source integration:** check out the intended dependency refs, enable source mode, then restore/build/test.
2. **Published-package compatibility:** use the default package mode and explicit package versions, then restore/build/test/pack.

If a dependency change is required, land and publish dependencies in dependency order before releasing the consumer package. Source mode helps teams develop together; it does not make a missing public package version available to consumers.

## 5. Release checklist

Before packaging or publishing:

- [ ] Source-mode property is unset or explicitly `false`.
- [ ] The consumer project contains the intended explicit `PackageReference` versions.
- [ ] Restore succeeds from the public NuGet feed in a clean environment.
- [ ] Build, tests, and pack succeed in package mode.
- [ ] The generated package's dependency metadata points to package IDs/versions, not local source paths.
- [ ] The repository's publish workflow remains gated; no publishing gate is enabled without explicit release authorization.

Do not use a package publish as the way to test a source-mode change. Keep source integration, package validation, and publishing as separate steps.

## 6. Instructions for a future coding assistant

When asked to apply this pattern to another Workshop repository:

1. Inspect the consumer's `.csproj`, solution, target frameworks, current package IDs/versions, and dependency direction.
2. Inspect each dependency repository's default branch, available development branch, actual project path, target frameworks, and current working-tree status.
3. Ask if the intended branch or dependency boundary is ambiguous. Do not guess.
4. Propose the smallest opt-in MSBuild change. Preserve NuGet package mode as the default.
5. Test source mode using checked-out dependency source.
6. Test package mode independently using the public NuGet feed.
7. Report exactly which refs/commits and commands were tested, what passed, and what remains unverified.
8. Do not publish packages or enable a disabled publish gate unless the owner explicitly authorizes it.

## Scope note

This document defines the repeatable approach; it does not claim that every Workshop repository already implements source mode. Each consuming project needs a small, reviewed adaptation and verification. Start with the repository that benefits immediately, then reuse the pattern where it removes real dependency-version bottlenecks.
