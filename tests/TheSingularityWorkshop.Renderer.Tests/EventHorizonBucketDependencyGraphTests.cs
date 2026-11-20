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

        Assert.True((dependent.DirtyReasons & BucketDirtyReason.DependencyChanged) != BucketDirtyReason.None);
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

        Assert.True((geometry.DirtyReasons & BucketDirtyReason.SourceChanged) != BucketDirtyReason.None);
        Assert.True((lighting.DirtyReasons & BucketDirtyReason.DependencyChanged) != BucketDirtyReason.None);
        Assert.True((distantShadows.DirtyReasons & BucketDirtyReason.DependencyChanged) != BucketDirtyReason.None);
        Assert.False(lighting.HasValidRepresentation);
        Assert.False(distantShadows.HasValidRepresentation);
    }

    [Fact]
    public void AddingDependencyInvalidatesOutputBuiltBeforeTheEdgeExisted()
    {
        var source = Bucket("Geometry", "Near", 30);
        var dependent = Bucket("Lighting", "Near", 10);
        using var graph = new EventHorizonBucketDependencyGraph();
        Complete(source);
        Complete(dependent);

        Assert.True(dependent.HasValidRepresentation);
        Assert.True(graph.AddDependency(source, dependent));

        Assert.False(dependent.HasValidRepresentation);
        Assert.True((dependent.DirtyReasons & BucketDirtyReason.DependencyChanged) != BucketDirtyReason.None);
    }

    [Fact]
    public void AddingDependencyPropagatesInvalidationToAlreadyPreparedTransitiveDependents()
    {
        var source = Bucket("Geometry", "Near", 30);
        var dependent = Bucket("Lighting", "Near", 10);
        var downstream = Bucket("Shadows", "Far", 1);
        using var graph = new EventHorizonBucketDependencyGraph();
        graph.AddDependency(dependent, downstream);
        Complete(source);
        Complete(dependent);
        Complete(downstream);

        Assert.True(dependent.HasValidRepresentation);
        Assert.True(downstream.HasValidRepresentation);

        graph.AddDependency(source, dependent);

        Assert.False(dependent.HasValidRepresentation);
        Assert.False(downstream.HasValidRepresentation);
        Assert.True((downstream.DirtyReasons & BucketDirtyReason.DependencyChanged) != BucketDirtyReason.None);
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


    [Fact]
    public void NaturalPhenomenonDependenciesInvalidateOnlyTheirDerivedOutputs()
    {
        var lightningEvent = Bucket("LightningEvent", "Transient", 60);
        var ashDensityField = Bucket("AshDensityField", "Regional", 5);
        var depositedAsh = Bucket("DepositedAsh", "Persistent", 1);
        var cameraState = Bucket("CameraState", "Observer", 60);

        var shadowHistory = Bucket("ShadowHistory", "Near", 30);
        var temporalLightingHistory = Bucket("TemporalLightingHistory", "Near", 30);
        var volumeIntegration = Bucket("VolumeIntegration", "Regional", 5);
        var viewHistory = Bucket("ViewHistory", "Observer", 60);
        var surfaceMaterials = Bucket("SurfaceMaterials", "Persistent", 1);
        var surfaceLighting = Bucket("SurfaceLighting", "Persistent", 1);

        using var graph = new EventHorizonBucketDependencyGraph();
        graph.AddDependency(lightningEvent, shadowHistory);
        graph.AddDependency(lightningEvent, temporalLightingHistory);
        graph.AddDependency(ashDensityField, volumeIntegration);
        graph.AddDependency(cameraState, viewHistory);
        graph.AddDependency(depositedAsh, surfaceMaterials);
        graph.AddDependency(depositedAsh, surfaceLighting);

        var buckets = new[]
        {
            lightningEvent, ashDensityField, depositedAsh, cameraState,
            shadowHistory, temporalLightingHistory, volumeIntegration,
            viewHistory, surfaceMaterials, surfaceLighting
        };
        foreach (var bucket in buckets)
            Complete(bucket);

        // A transient flash invalidates illumination products, not the evolving ash field,
        // persistent surface state, or camera history.
        lightningEvent.SetSourceRevision(1);
        Assert.False(shadowHistory.HasValidRepresentation);
        Assert.False(temporalLightingHistory.HasValidRepresentation);
        Assert.True(volumeIntegration.HasValidRepresentation);
        Assert.True(viewHistory.HasValidRepresentation);
        Assert.True(surfaceMaterials.HasValidRepresentation);
        Assert.True(surfaceLighting.HasValidRepresentation);

        Complete(shadowHistory);
        Complete(temporalLightingHistory);

        // A new authoritative density-field revision invalidates volume integration only.
        ashDensityField.SetSourceRevision(1);
        Assert.False(volumeIntegration.HasValidRepresentation);
        Assert.True(shadowHistory.HasValidRepresentation);
        Assert.True(temporalLightingHistory.HasValidRepresentation);
        Assert.True(viewHistory.HasValidRepresentation);
        Assert.True(surfaceMaterials.HasValidRepresentation);

        Complete(volumeIntegration);

        // A camera cut invalidates view-dependent history without changing world sources.
        cameraState.MarkDirty(BucketDirtyReason.ObserverChanged);
        Assert.False(viewHistory.HasValidRepresentation);
        Assert.True(volumeIntegration.HasValidRepresentation);
        Assert.True(surfaceMaterials.HasValidRepresentation);

        Complete(viewHistory);

        // Persistent aftermath has its own revision and invalidates later material/lighting
        // products even after the transient event and its visual history have expired.
        depositedAsh.SetSourceRevision(1);
        Assert.False(surfaceMaterials.HasValidRepresentation);
        Assert.False(surfaceLighting.HasValidRepresentation);
        Assert.True(shadowHistory.HasValidRepresentation);
        Assert.True(volumeIntegration.HasValidRepresentation);
        Assert.True(viewHistory.HasValidRepresentation);
    }

    private static EventHorizonBucket Bucket(string name, string horizon, double frequency) =>
        new(name, new EventHorizon(horizon, 100, frequency), frequency);

    private static void Complete(EventHorizonBucket bucket)
    {
        Assert.True(bucket.TryBeginRefresh(Epoch, out var ticket));
        Assert.True(bucket.TryCompleteRefresh(ticket, Epoch.AddMilliseconds(1)));
    }
}
