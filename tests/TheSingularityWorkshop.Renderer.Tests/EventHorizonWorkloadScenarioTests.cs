using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

/// <summary>
/// Exercises the same dependency/cache decisions a rendering consumer needs before
/// it binds those decisions to a concrete CPU or GPU backend.
/// </summary>
public sealed class EventHorizonWorkloadScenarioTests
{
    private static readonly DateTimeOffset Epoch =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void SourceChangeRefreshesOnlyItsDependencyChainWhileUnrelatedFarCacheIsReused()
    {
        var geometry = Bucket("NearGeometry", "Near", 30);
        var lighting = Bucket("NearLighting", "Near", 15);
        var shadows = Bucket("FarShadows", "Far", 1);
        var population = Bucket("FarPopulation", "Far", 1);

        using var dependencies = new EventHorizonBucketDependencyGraph();
        dependencies.AddDependency(geometry, lighting);
        dependencies.AddDependency(lighting, shadows);

        foreach (var bucket in new[] { geometry, lighting, shadows, population })
            Complete(bucket, Epoch);

        // The consumer explicitly confirms the caches it actually reused.
        Assert.True(shadows.TryRecordReuse());
        Assert.True(population.TryRecordReuse());

        // A geometry source revision affects its derived lighting and shadow output,
        // but not the unrelated far-population representation.
        geometry.SetSourceRevision(7);

        Assert.False(geometry.HasValidRepresentation);
        Assert.False(lighting.HasValidRepresentation);
        Assert.False(shadows.HasValidRepresentation);
        Assert.True(population.HasValidRepresentation);
        Assert.True(population.TryRecordReuse());

        Assert.True(geometry.TryBeginRefresh(Epoch.AddMilliseconds(2), out var geometryTicket));
        Assert.True(geometry.TryCompleteRefresh(geometryTicket, Epoch.AddMilliseconds(3)));
        Assert.True(lighting.TryBeginRefresh(Epoch.AddMilliseconds(4), out var lightingTicket));
        Assert.True(lighting.TryCompleteRefresh(lightingTicket, Epoch.AddMilliseconds(5)));
        Assert.True(shadows.TryBeginRefresh(Epoch.AddMilliseconds(6), out var shadowsTicket));
        Assert.True(shadows.TryCompleteRefresh(shadowsTicket, Epoch.AddMilliseconds(7)));

        Assert.True(geometry.HasValidRepresentation);
        Assert.True(lighting.HasValidRepresentation);
        Assert.True(shadows.HasValidRepresentation);
        Assert.True(population.HasValidRepresentation);

        Assert.Equal(2, geometry.Metrics.AcceptedRefreshes);
        Assert.Equal(2, lighting.Metrics.AcceptedRefreshes);
        Assert.Equal(2, shadows.Metrics.AcceptedRefreshes);
        Assert.Equal(1, population.Metrics.RefreshAttempts);
        Assert.Equal(2, population.Metrics.ReuseCount);
        Assert.Equal(2, lighting.Metrics.Invalidations);
        Assert.Equal(2, shadows.Metrics.Invalidations);
    }

    private static EventHorizonBucket Bucket(string name, string horizon, double frequency) =>
        new(name, new EventHorizon(horizon, 100, frequency), frequency);

    private static void Complete(EventHorizonBucket bucket, DateTimeOffset start)
    {
        Assert.True(bucket.TryBeginRefresh(start, out var ticket));
        Assert.True(bucket.TryCompleteRefresh(ticket, start.AddMilliseconds(1)));
    }
}
