using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class EventHorizonBucketDependencyGraphTests
{
    private static readonly DateTimeOffset Epoch =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void InvalidatingSourceInvalidatesDirectDependentButNotUnrelatedBucket()
    {
        var source = Bucket("Environment", "Near", 10);
        var dependent = Bucket("Shadows", "Near", 10);
        var unrelated = Bucket("Motion", "Near", 10);
        using var graph = new EventHorizonBucketDependencyGraph();
        graph.AddDependency(source, dependent);
        Complete(source);
        Complete(dependent);
        Complete(unrelated);

        source.MarkDirty(BucketDirtyReason.ResourceChanged);

        Assert.NotEqual(BucketDirtyReason.None, dependent.DirtyReasons & BucketDirtyReason.DependencyChanged);
        Assert.False(dependent.HasValidRepresentation);
        Assert.True(unrelated.HasValidRepresentation);
        Assert.False(unrelated.IsRefreshDue(Epoch.AddMilliseconds(1)));
    }

    [Fact]
    public void InvalidationPropagatesTransitivelyAcrossEventHorizons()
    {
        var geometry = Bucket("Geometry", "Near", 30);
        var lighting = Bucket("Lighting", "WithinReach", 10);
        var distantShadows = Bucket("Shadows", "Far", 1);
        using var graph = new EventHorizonBucketDependencyGraph();
        graph.AddDependency(geometry, lighting);
        graph.AddDependency(lighting, distantShadows);
        Complete(geometry);
        Complete(lighting);
        Complete(distantShadows);

        geometry.SetSourceRevision(42);

        Assert.NotEqual(BucketDirtyReason.None, geometry.DirtyReasons & BucketDirtyReason.SourceChanged);
        Assert.NotEqual(BucketDirtyReason.None, lighting.DirtyReasons & BucketDirtyReason.DependencyChanged);
        Assert.NotEqual(BucketDirtyReason.None, distantShadows.DirtyReasons & BucketDirtyReason.DependencyChanged);
        Assert.False(lighting.HasValidRepresentation);
        Assert.False(distantShadows.HasValidRepresentation);
    }

    [Fact]
    public void RepeatedSourceRevisionDoesNotInvalidateDependentsAgain()
    {
        var source = Bucket("Motion", "Near", 30);
        var derived = Bucket("PositionHistory", "Near", 10);
        using var graph = new EventHorizonBucketDependencyGraph();
        graph.AddDependency(source, derived);
        source.SetSourceRevision(5);
        Complete(source);
        Complete(derived);

        source.SetSourceRevision(5);

        Assert.True(source.HasValidRepresentation);
        Assert.True(derived.HasValidRepresentation);
    }

    [Fact]
    public void DuplicateDependencyIsNoOpAndSelfOrIndirectCyclesAreRejected()
    {
        var first = Bucket("Geometry", "Near", 30);
        var second = Bucket("Lighting", "Near", 10);
        var third = Bucket("Shadows", "Far", 1);
        using var graph = new EventHorizonBucketDependencyGraph();

        Assert.True(graph.AddDependency(first, second));
        Assert.False(graph.AddDependency(first, second));
        Assert.True(graph.AddDependency(second, third));
        Assert.Throws<ArgumentException>(() => graph.AddDependency(first, first));
        Assert.Throws<ArgumentException>(() => graph.AddDependency(third, first));
        Assert.Single(graph.GetDependents(first));
    }

    [Fact]
    public void DisposedGraphStopsPropagatingInvalidation()
    {
        var source = Bucket("Geometry", "Near", 30);
        var dependent = Bucket("Lighting", "Near", 10);
        var graph = new EventHorizonBucketDependencyGraph();
        graph.AddDependency(source, dependent);
        Complete(source);
        Complete(dependent);
        graph.Dispose();

        source.MarkDirty(BucketDirtyReason.ResourceChanged);

        Assert.True(dependent.HasValidRepresentation);
        Assert.Throws<ObjectDisposedException>(() => graph.AddDependency(source, dependent));
    }

    private static EventHorizonBucket Bucket(string name, string horizon, double frequency) =>
        new(name, new EventHorizon(horizon, 100, frequency), frequency);

    private static void Complete(EventHorizonBucket bucket)
    {
        Assert.True(bucket.TryBeginRefresh(Epoch, out var ticket));
        Assert.True(bucket.TryCompleteRefresh(ticket, Epoch.AddMilliseconds(1)));
    }
}
