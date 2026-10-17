using System.Numerics;
using TheSingularityWorkshop.Renderer;
using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class RenderCommandReducerTests
{
    private static RenderDrawCommand Command(
        ulong entity,
        RendererRepresentation representation,
        ulong geometry = 1) =>
        new(
            new RenderResourceId(geometry),
            new RenderResourceId(2),
            Matrix4x4.CreateTranslation((float)entity, 0, 0),
            indexCount: 6,
            firstIndex: 0,
            baseVertex: 0,
            representation,
            entity: new RenderEntityId(entity));

    [Fact]
    public void SelectsRichestPermittedRepresentationAndPreservesIdentityAndTransform()
    {
        var entity = new RenderEntityId(42);
        var semantic = Command(entity.Value, RendererRepresentation.Semantic);
        var aggregate = Command(entity.Value, RendererRepresentation.Aggregate, geometry: 2);
        var near = Command(entity.Value, RendererRepresentation.Near, geometry: 3);

        var result = RenderCommandReducer.Reduce(
            [semantic, aggregate, near],
            RendererRepresentation.Aggregate);

        var selected = Assert.Single(result.Commands);
        Assert.Equal(aggregate.Entity, selected.Entity);
        Assert.Equal(aggregate.WorldTransform, selected.WorldTransform);
        Assert.Equal(RendererRepresentation.Aggregate, selected.Representation);
        Assert.Equal(new RenderCommandReductionMetrics(3, 1, 2, 1, 1, 0), result.Metrics);
    }

    [Fact]
    public void PreservesEveryCommandInTheSelectedMultipartRepresentation()
    {
        var commands = new[]
        {
            Command(7, RendererRepresentation.Aggregate, geometry: 10),
            Command(7, RendererRepresentation.Aggregate, geometry: 11),
            Command(7, RendererRepresentation.Interactive, geometry: 12)
        };

        var result = RenderCommandReducer.Reduce(commands, RendererRepresentation.Aggregate);

        Assert.Equal(2, result.Commands.Count);
        Assert.Equal(new ulong[] { 10, 11 }, result.Commands.Select(command => command.Geometry.Value));
        Assert.Equal(1, result.Metrics.EntitiesRepresented);
        Assert.Equal(1, result.Metrics.OmittedCommands);
    }

    [Fact]
    public void DistinctEntitiesRemainDistinctAndUnidentifiedCommandsAreNotGrouped()
    {
        var first = Command(1, RendererRepresentation.Distant);
        var second = Command(2, RendererRepresentation.Distant);
        var unidentified = new RenderDrawCommand(
            new RenderResourceId(3),
            new RenderResourceId(2),
            Matrix4x4.Identity,
            6, 0, 0,
            RendererRepresentation.Distant);

        var result = RenderCommandReducer.Reduce(
            [first, second, unidentified],
            RendererRepresentation.Distant);

        Assert.Equal(3, result.Commands.Count);
        Assert.Equal(2, result.Metrics.EntitiesConsidered);
        Assert.Equal(2, result.Metrics.EntitiesRepresented);
        Assert.Equal(1, result.Metrics.UnidentifiedCommandsPreserved);
    }

    [Fact]
    public void OmitsEntityWhenNoAvailableVariantFitsTheDetailCeiling()
    {
        var result = RenderCommandReducer.Reduce(
            [Command(1, RendererRepresentation.Near)],
            RendererRepresentation.Aggregate);

        Assert.Empty(result.Commands);
        Assert.Equal(1, result.Metrics.OmittedCommands);
        Assert.Equal(0, result.Metrics.EntitiesRepresented);
    }

    [Fact]
    public void RejectsUndefinedDetailCeiling()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RenderCommandReducer.Reduce(Array.Empty<RenderDrawCommand>(), (RendererRepresentation)99));
    }
}
