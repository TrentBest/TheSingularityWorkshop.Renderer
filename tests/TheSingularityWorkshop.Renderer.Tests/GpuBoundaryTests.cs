using System.Numerics;
using TheSingularityWorkshop.Renderer;
using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public class GpuBoundaryTests
{
    [Fact]
    public void RenderResourceId_ZeroIsInvalid()
    {
        Assert.False(new RenderResourceId(0).IsValid);
        Assert.True(new RenderResourceId(1).IsValid);
    }

    [Fact]
    public void RenderDrawCommand_PreservesGpuFacingFields()
    {
        var transform = Matrix4x4.CreateTranslation(1, 2, 3);
        var command = new RenderDrawCommand(
            new RenderResourceId(7),
            new RenderResourceId(11),
            transform,
            36,
            4,
            8,
            RendererRepresentation.Near);

        Assert.Equal(new RenderResourceId(7), command.Geometry);
        Assert.Equal(new RenderResourceId(11), command.Material);
        Assert.Equal(transform, command.WorldTransform);
        Assert.Equal(36, command.IndexCount);
        Assert.Equal(4, command.FirstIndex);
        Assert.Equal(8, command.BaseVertex);
        Assert.Equal(RendererRepresentation.Near, command.Representation);
    }

    [Fact]
    public void RenderFrame_SnapshotsCommandSequence()
    {
        var view = new RenderView(
            Vector3.Zero,
            Matrix4x4.Identity,
            Matrix4x4.Identity);
        var commands = new[]
        {
            new RenderDrawCommand(
                new RenderResourceId(1),
                new RenderResourceId(2),
                Matrix4x4.Identity,
                3,
                0,
                0,
                RendererRepresentation.Distant)
        };

        var frame = new RenderFrame(view, commands);

        Assert.Single(frame.Commands);
        Assert.Equal(RendererRepresentation.Distant, frame.Commands[0].Representation);
    }

    [Fact]
    public void RenderDrawCommand_RejectsInvalidResource()
    {
        Assert.Throws<ArgumentException>(() => new RenderDrawCommand(
            new RenderResourceId(0),
            new RenderResourceId(2),
            Matrix4x4.Identity,
            3,
            0,
            0,
            RendererRepresentation.Near));
    }
}
