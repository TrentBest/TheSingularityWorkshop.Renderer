using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

/// <summary>
/// FSM_API uses a shared static registry and documents main-thread operations.
/// Tests that create, update, or destroy its instances must not run concurrently
/// on unrelated xUnit worker threads.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class FsmApiSharedRegistryCollection
{
    public const string Name = "FSM_API shared registry";
}
