using System.Numerics;
using TheSingularityWorkshop.Renderer;
using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class RenderFrameTests
{
    private static RenderDrawCommand CreateCommand(ulong entityId) =>
        new(
            new RenderResourceId(1),
            new RenderResourceId(2),
            Matrix4x4.Identity,
            indexCount: 3,
            firstIndex: 0,
            baseVertex: 0,
            RendererRepresentation.Near,
            entity: new RenderEntityId(entityId));

    [Fact]
    public void CapturesStableOrderedSnapshotWhenSourceChanges()
    {
        var source = new List<RenderDrawCommand>
        {
            CreateCommand(10),
            CreateCommand(20)
        };
        var frame = new RenderFrame(
            new RenderView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
            source);

        source.Clear();
        source.Add(CreateCommand(30));

        Assert.Equal(2, frame.Commands.Count);
        Assert.Equal(new RenderEntityId(10), frame.Commands[0].Entity);
        Assert.Equal(new RenderEntityId(20), frame.Commands[1].Entity);
    }

    [Fact]
    public void ExposesSnapshotAsReadOnlyCollection()
    {
        var frame = new RenderFrame(
            new RenderView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
            new[] { CreateCommand(10) });

        var commands = Assert.IsAssignableFrom<IList<RenderDrawCommand>>(frame.Commands);

        Assert.Throws<NotSupportedException>(() => commands.Add(CreateCommand(20)));
        Assert.Throws<NotSupportedException>(() => commands[0] = CreateCommand(30));
    }

    [Fact]
    public void AllowsAnEmptyFrame()
    {
        var frame = new RenderFrame(
            new RenderView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
            Array.Empty<RenderDrawCommand>());

        Assert.Empty(frame.Commands);
    }

    [Fact]
    public void RejectsNullCommandSequence()
    {
        Assert.Throws<ArgumentNullException>(() => new RenderFrame(
            new RenderView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
            null!));
    }
}
