using System.Numerics;
using TheSingularityWorkshop.Renderer;
using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class RenderDrawCommandTests
{
    private static RenderDrawCommand Create(
        RenderResourceId instances = default,
        int instanceCount = 1,
        int firstInstance = 0,
        RenderEntityId entity = default) =>
        new(
            new RenderResourceId(1),
            new RenderResourceId(2),
            Matrix4x4.Identity,
            indexCount: 36,
            firstIndex: 0,
            baseVertex: 0,
            RendererRepresentation.Near,
            instances,
            instanceCount,
            firstInstance,
            entity);

    [Fact]
    public void DefaultsToOneNonInstancedDraw()
    {
        var command = Create();

        Assert.False(command.Instances.IsValid);
        Assert.Equal(1, command.InstanceCount);
        Assert.Equal(0, command.FirstInstance);
        Assert.False(command.Entity.IsValid);
    }

    [Fact]
    public void PreservesInstancedResourceAndRange()
    {
        var command = Create(new RenderResourceId(3), instanceCount: 1000, firstInstance: 24);

        Assert.Equal(new RenderResourceId(3), command.Instances);
        Assert.Equal(1000, command.InstanceCount);
        Assert.Equal(24, command.FirstInstance);
    }

    [Fact]
    public void PreservesStableSemanticIdentity()
    {
        var entity = new RenderEntityId(42);

        var command = Create(entity: entity);

        Assert.True(command.Entity.IsValid);
        Assert.Equal(entity, command.Entity);
    }

    [Fact]
    public void RejectsInstancedRangeWithoutInstanceResource()
    {
        Assert.Throws<ArgumentException>(() => Create(instanceCount: 2));
        Assert.Throws<ArgumentException>(() => Create(firstInstance: 1));
    }

    [Fact]
    public void RejectsInvalidInstanceRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(new RenderResourceId(3), instanceCount: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(new RenderResourceId(3), firstInstance: -1));
    }
}
