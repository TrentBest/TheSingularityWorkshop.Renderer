using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

/// <summary>
/// Exercises the bucket validity ticket and representation handoff together rather than as isolated primitives.
/// </summary>
public sealed class BucketRepresentationHandoffIntegrationTests
{
    private static readonly DateTimeOffset Epoch =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void StaleRefreshCannotReplaceVisibleOutputAndIndependentBucketRemainsReusable()
    {
        var horizon = new EventHorizon("Near", 100, 30);
        var geometry = new EventHorizonBucket("Geometry", horizon, 30);
        var lighting = new EventHorizonBucket("Lighting", horizon, 15);

        CompleteInitialRefresh(geometry, sourceRevision: 1);
        CompleteInitialRefresh(lighting, sourceRevision: 0);

        var handoff = new HorizonRepresentationHandoff(Representation(
            "geometry-v1", "Near", sourceRevision: 1, observerRevision: 4, ready: true, covers: true));

        // Geometry changes. Lighting is a separate domain and must not be invalidated.
        geometry.SetSourceRevision(2);
        Assert.True(geometry.TryBeginRefresh(Epoch.AddMilliseconds(10), out var staleTicket));
        handoff.Stage(Representation(
            "geometry-v2", "Near", sourceRevision: 2, observerRevision: 4, ready: false, covers: false));

        // The old geometry remains the selected output while replacement work is pending.
        Assert.Equal("geometry-v1", handoff.Snapshot.Current.Id);
        Assert.Equal("geometry-v2", handoff.Snapshot.Pending!.Id);

        // A second source change makes the in-flight ticket and its candidate obsolete.
        geometry.SetSourceRevision(3);
        Assert.False(geometry.TryCompleteRefresh(staleTicket, Epoch.AddMilliseconds(20)));
        Assert.False(handoff.TryCommit(currentSourceRevision: 3, currentObserverRevision: 4));
        Assert.Equal("geometry-v1", handoff.Snapshot.Current.Id);
        Assert.False(geometry.HasValidRepresentation);

        // Rebuild against the current revision. Only accepted output is eligible for promotion.
        Assert.True(geometry.TryBeginRefresh(Epoch.AddMilliseconds(30), out var currentTicket));
        Assert.True(geometry.TryCompleteRefresh(currentTicket, Epoch.AddMilliseconds(40)));
        handoff.Stage(Representation(
            "geometry-v3", "Near", sourceRevision: 3, observerRevision: 4, ready: false, covers: false));
        Assert.True(handoff.UpdatePending(
            "geometry-v3", isReady: true, coversObserverView: true, observerRevision: 4));
        Assert.True(handoff.TryCommit(currentSourceRevision: 3, currentObserverRevision: 4));

        Assert.Equal("geometry-v3", handoff.Snapshot.Current.Id);
        Assert.Null(handoff.Snapshot.Pending);
        Assert.True(geometry.HasValidRepresentation);
        Assert.Equal(3, geometry.CachedSourceRevision);

        // No geometry invalidation or refresh operation touched the lighting bucket.
        Assert.True(lighting.HasValidRepresentation);
        Assert.Equal(0, lighting.CachedSourceRevision);
        Assert.True(lighting.TryRecordReuse());
        Assert.Equal(1, lighting.Metrics.ReuseCount);
        Assert.Equal(0, lighting.Metrics.Invalidations);
        Assert.Equal(1, lighting.Metrics.AcceptedRefreshes);

        // The rejected stale result did not become the cached geometry revision.
        Assert.Equal(1, geometry.Metrics.RejectedRefreshes);
        Assert.Equal(2, geometry.Metrics.AcceptedRefreshes);
    }

    [Fact]
    public void ObserverRevisionMustMatchBeforeAReadyReplacementCanBePromoted()
    {
        var horizon = new EventHorizon("Near", 100, 30);
        var bucket = new EventHorizonBucket("Motion", horizon, 30);
        CompleteInitialRefresh(bucket, sourceRevision: 5);

        var handoff = new HorizonRepresentationHandoff(Representation(
            "motion-old", "Near", sourceRevision: 5, observerRevision: 10, ready: true, covers: true));

        bucket.MarkDirty(BucketDirtyReason.ObserverChanged);
        Assert.True(bucket.TryBeginRefresh(Epoch.AddMilliseconds(1), out var ticket));
        Assert.True(bucket.TryCompleteRefresh(ticket, Epoch.AddMilliseconds(2)));

        handoff.Stage(Representation(
            "motion-new", "Near", sourceRevision: 5, observerRevision: 10, ready: true, covers: true));

        // The candidate is ready, but the observer moved after it was prepared.
        Assert.False(handoff.TryCommit(currentSourceRevision: 5, currentObserverRevision: 11));
        Assert.Equal("motion-old", handoff.Snapshot.Current.Id);

        Assert.True(handoff.UpdatePending(
            "motion-new", isReady: true, coversObserverView: false, observerRevision: 11));
        Assert.False(handoff.TryCommit(currentSourceRevision: 5, currentObserverRevision: 11));
        Assert.Equal("motion-old", handoff.Snapshot.Current.Id);

        Assert.True(handoff.UpdatePending(
            "motion-new", isReady: true, coversObserverView: true, observerRevision: 11));
        Assert.True(handoff.TryCommit(currentSourceRevision: 5, currentObserverRevision: 11));
        Assert.Equal("motion-new", handoff.Snapshot.Current.Id);
    }

    private static void CompleteInitialRefresh(EventHorizonBucket bucket, long sourceRevision)
    {
        bucket.SetSourceRevision(sourceRevision);
        Assert.True(bucket.TryBeginRefresh(Epoch, out var ticket));
        Assert.True(bucket.TryCompleteRefresh(ticket, Epoch.AddTicks(1)));
    }

    private static HorizonRepresentation Representation(
        string id,
        string horizon,
        long sourceRevision,
        long observerRevision,
        bool ready,
        bool covers) =>
        new(id, horizon, sourceRevision, observerRevision, ready, covers)
        {
            SourceId = "world:tree-1"
        };
}
