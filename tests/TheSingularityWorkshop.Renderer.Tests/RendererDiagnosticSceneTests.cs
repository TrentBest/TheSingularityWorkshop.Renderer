using TheSingularityWorkshop.Renderer;
using TheSingularityWorkshop.Renderer.Diagnostics;
using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class RendererDiagnosticSceneTests
{
    [Fact]
    public void ProducesStableEntityVariantsInDeterministicOrder()
    {
        var first = RendererDiagnosticScene.CreateCommands(3);
        var second = RendererDiagnosticScene.CreateCommands(3);

        Assert.Equal(3 * RendererDiagnosticScene.RepresentationVariantCount, first.Count);
        Assert.Equal(first, second);

        for (var entityIndex = 0; entityIndex < 3; entityIndex++)
        {
            var variants = first
                .Skip(entityIndex * RendererDiagnosticScene.RepresentationVariantCount)
                .Take(RendererDiagnosticScene.RepresentationVariantCount)
                .ToArray();

            Assert.All(variants, command => Assert.Equal(new RenderEntityId((ulong)entityIndex + 1UL), command.Entity));
            Assert.Equal(
                new[]
                {
                    RendererRepresentation.Semantic,
                    RendererRepresentation.Aggregate,
                    RendererRepresentation.Distant,
                    RendererRepresentation.Near,
                    RendererRepresentation.Interactive
                },
                variants.Select(command => command.Representation));
            Assert.All(variants, command => Assert.Equal(variants[0].WorldTransform, command.WorldTransform));
        }
    }

    [Theory]
    [InlineData(RendererRepresentation.Semantic)]
    [InlineData(RendererRepresentation.Aggregate)]
    [InlineData(RendererRepresentation.Distant)]
    [InlineData(RendererRepresentation.Near)]
    [InlineData(RendererRepresentation.Interactive)]
    public void EachDetailCeilingSelectsOneVariantPerEntity(RendererRepresentation detail)
    {
        const int entityCount = 25;
        var candidates = RendererDiagnosticScene.CreateCommands(entityCount);

        var result = RenderCommandReducer.Reduce(candidates, detail);

        Assert.Equal(entityCount * RendererDiagnosticScene.RepresentationVariantCount, result.Metrics.InputCommands);
        Assert.Equal(entityCount, result.Metrics.OutputCommands);
        Assert.Equal(entityCount * (RendererDiagnosticScene.RepresentationVariantCount - 1), result.Metrics.OmittedCommands);
        Assert.Equal(entityCount, result.Metrics.EntitiesConsidered);
        Assert.Equal(entityCount, result.Metrics.EntitiesRepresented);
        Assert.All(result.Commands, command => Assert.Equal(detail, command.Representation));
    }

    [Fact]
    public void RejectsAnEmptyDiagnosticScene()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RendererDiagnosticScene.CreateCommands(0));
    }
}
