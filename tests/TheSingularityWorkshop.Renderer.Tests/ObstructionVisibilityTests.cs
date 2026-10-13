using TheSingularityWorkshop.Renderer;
using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class ObstructionVisibilityTests
{
    private static readonly ProjectedRect Subject = new(0.4, 0.4, 0.2, 0.2);
    private static readonly ProjectedRect Wall = new(0.3, 0.3, 0.4, 0.4);

    [Fact]
    public void OpaqueNearWallFullyContainingSubjectSuppressesSubject()
    {
        var result = ObstructionVisibility.Assess(
            Subject, 0.8, Wall, 0.2,
            PerceptionChannel.Visible, PerceptionChannel.Visible, fullCoverageGuaranteed: true);

        Assert.Equal(OcclusionOutcome.FullyOccluded, result.Outcome);
        Assert.False(result.RenderSubject);
        Assert.False(result.CompositeObstructionAfterSubject);
        Assert.Equal(1, result.SubjectCoverage);
    }


    [Fact]
    public void BoundingRectangleAloneDoesNotProveFullOcclusion()
    {
        var result = ObstructionVisibility.Assess(
            Subject, 0.8, Wall, 0.2,
            PerceptionChannel.Visible, PerceptionChannel.Visible);

        Assert.Equal(OcclusionOutcome.OcclusionUncertain, result.Outcome);
        Assert.True(result.RenderSubject);
    }

    [Fact]
    public void PartialOverlapKeepsSubjectAndCompositesObstructionAfterIt()
    {
        var partialWall = new ProjectedRect(0.5, 0.3, 0.2, 0.4);

        var result = ObstructionVisibility.Assess(
            Subject, 0.8, partialWall, 0.2,
            PerceptionChannel.Visible, PerceptionChannel.Visible);

        Assert.Equal(OcclusionOutcome.PartiallyOccluded, result.Outcome);
        Assert.True(result.RenderSubject);
        Assert.True(result.CompositeObstructionAfterSubject);
        Assert.InRange(result.SubjectCoverage, 0, 1);
    }

    [Fact]
    public void ThermalChannelCanPassWhenWallOnlyBlocksVisibleLight()
    {
        var result = ObstructionVisibility.Assess(
            Subject, 0.8, Wall, 0.2,
            PerceptionChannel.Thermal, PerceptionChannel.Visible);

        Assert.Equal(OcclusionOutcome.Unobstructed, result.Outcome);
        Assert.True(result.RenderSubject);
        Assert.False(result.CompositeObstructionAfterSubject);
    }

    [Fact]
    public void ObstructionBehindSubjectDoesNotOccludeIt()
    {
        var result = ObstructionVisibility.Assess(
            Subject, 0.2, Wall, 0.8,
            PerceptionChannel.Visible, PerceptionChannel.Visible);

        Assert.Equal(OcclusionOutcome.Unobstructed, result.Outcome);
        Assert.True(result.RenderSubject);
    }

    [Fact]
    public void InvalidBoundsReturnExplicitInvalidResult()
    {
        var invalid = new ProjectedRect(0, 0, -1, 1);

        var result = ObstructionVisibility.Assess(
            Subject, 0.8, invalid, 0.2,
            PerceptionChannel.Visible, PerceptionChannel.Visible);

        Assert.Equal(OcclusionOutcome.InvalidInput, result.Outcome);
        Assert.False(result.RenderSubject);
    }
}
