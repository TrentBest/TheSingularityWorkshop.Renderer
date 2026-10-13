using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class EventHorizonBucketTests
{
    private static readonly DateTimeOffset Epoch =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void NewBucketRequiresInitialRefresh()
    {
        var bucket = CreateBucket("Environment", 1);

        Assert.False(bucket.HasValidRepresentation);
        Assert.True(bucket.IsRefreshDue(Epoch));
        Assert.Equal(BucketDirtyReason.Initial, bucket.DirtyReasons);

        Assert.True(bucket.TryBeginRefresh(Epoch, out var ticket));
        Assert.True(bucket.TryCompleteRefresh(ticket, Epoch));

        Assert.True(bucket.HasValidRepresentation);
        Assert.Equal(BucketDirtyReason.None, bucket.DirtyReasons);
        Assert.Equal(Epoch, bucket.LastRefreshAt);
    }

    [Fact]
    public void RefreshCadenceIsIndependentFromDisplayFrames()
    {
        var bucket = CreateBucket("FarEnvironment", 1);
        CompleteInitialRefresh(bucket, Epoch);

        Assert.False(bucket.IsRefreshDue(Epoch.AddMilliseconds(999)));
        Assert.True(bucket.IsRefreshDue(Epoch.AddSeconds(1)));
    }

    [Fact]
    public void CadenceExpiryRequestsRefreshWithoutDiscardingReusableRepresentation()
    {
        var bucket = CreateBucket("FarEnvironment", 1);
        CompleteInitialRefresh(bucket, Epoch);

        Assert.True(bucket.HasValidRepresentation);
        Assert.True(bucket.IsRefreshDue(Epoch.AddSeconds(1)));

        // A refresh is due, but the cached representation remains usable until an actual
        // invalidation occurs or replacement work is accepted.
        Assert.True(bucket.HasValidRepresentation);
        Assert.Equal(BucketDirtyReason.None, bucket.DirtyReasons);
    }

    [Fact]
    public void BucketsInvalidateIndependently()
    {
        var environment = CreateBucket("Environment", 1);
        var lighting = CreateBucket("Lighting", 15);
        CompleteInitialRefresh(environment, Epoch);
        CompleteInitialRefresh(lighting, Epoch);

        lighting.MarkDirty(BucketDirtyReason.ResourceChanged);

        Assert.True(lighting.IsRefreshDue(Epoch.AddMilliseconds(1)));
        Assert.False(environment.IsRefreshDue(Epoch.AddMilliseconds(1)));
        Assert.True(environment.HasValidRepresentation);
        Assert.False(lighting.HasValidRepresentation);
    }

    [Fact]
    public void SourceRevisionChangeInvalidatesOnlyWhenRevisionChanges()
    {
        var bucket = CreateBucket("Motion", 30);
        CompleteInitialRefresh(bucket, Epoch);

        bucket.SetSourceRevision(4);
        Assert.Equal(BucketDirtyReason.SourceChanged, bucket.DirtyReasons);
        Assert.True(bucket.IsRefreshDue(Epoch.AddMilliseconds(1)));

        Assert.True(bucket.TryBeginRefresh(Epoch.AddMilliseconds(1), out var ticket));
        Assert.True(bucket.TryCompleteRefresh(ticket, Epoch.AddMilliseconds(2)));
        Assert.Equal(4, bucket.CachedSourceRevision);

        bucket.SetSourceRevision(4);
        Assert.True(bucket.HasValidRepresentation);
        Assert.Equal(BucketDirtyReason.None, bucket.DirtyReasons);
    }

    [Fact]
    public void InvalidatedWorkCannotPublishStaleOutput()
    {
        var bucket = CreateBucket("Shadows", 15);
        CompleteInitialRefresh(bucket, Epoch);
        bucket.SetSourceRevision(10);

        Assert.True(bucket.TryBeginRefresh(Epoch.AddSeconds(1), out var ticket));
        bucket.SetSourceRevision(11);

        Assert.False(bucket.TryCompleteRefresh(ticket, Epoch.AddSeconds(1.1)));
        Assert.False(bucket.HasValidRepresentation);
        Assert.Equal(BucketDirtyReason.SourceChanged, bucket.DirtyReasons);
        Assert.Equal(0, bucket.CachedSourceRevision);
    }

    [Fact]
    public void InvalidatedOutputCanBeRebuiltAndAccepted()
    {
        var bucket = CreateBucket("Shadows", 15);
        bucket.SetSourceRevision(11);

        Assert.True(bucket.TryBeginRefresh(Epoch, out var ticket));
        Assert.True(bucket.TryCompleteRefresh(ticket, Epoch.AddMilliseconds(10)));
        Assert.True(bucket.HasValidRepresentation);
        Assert.Equal(11, bucket.CachedSourceRevision);
    }

    [Fact]
    public void OnlyOneTicketFromAGenerationCanPublish()
    {
        var bucket = CreateBucket("Environment", 1);
        Assert.True(bucket.TryBeginRefresh(Epoch, out var first));
        Assert.True(bucket.TryBeginRefresh(Epoch, out var second));

        Assert.True(bucket.TryCompleteRefresh(first, Epoch.AddMilliseconds(1)));
        Assert.False(bucket.TryCompleteRefresh(second, Epoch.AddMilliseconds(2)));
    }

    [Fact]
    public void CompletionBeforeTicketStartIsRejectedWithoutConsumingTicket()
    {
        var bucket = CreateBucket("Motion", 30);
        Assert.True(bucket.TryBeginRefresh(Epoch, out var ticket));

        Assert.False(bucket.TryCompleteRefresh(ticket, Epoch.AddTicks(-1)));
        Assert.False(bucket.HasValidRepresentation);
        Assert.True(bucket.TryCompleteRefresh(ticket, Epoch.AddTicks(1)));
        Assert.True(bucket.HasValidRepresentation);
    }

    [Fact]
    public void BucketRequiresNameHorizonAndRepresentablePositiveFrequency()
    {
        var horizon = new EventHorizon("Near", 10, 60);

        Assert.Throws<ArgumentException>(() => new EventHorizonBucket(" ", horizon, 1));
        Assert.Throws<ArgumentNullException>(() => new EventHorizonBucket("Motion", null!, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new EventHorizonBucket("Motion", horizon, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new EventHorizonBucket("Motion", horizon, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => new EventHorizonBucket("Motion", horizon, double.MaxValue));
    }

    [Fact]
    public void DirtyReasonMustBeKnownAndNonEmpty()
    {
        var bucket = CreateBucket("Motion", 30);

        Assert.Throws<ArgumentOutOfRangeException>(() => bucket.MarkDirty(BucketDirtyReason.None));
        Assert.Throws<ArgumentOutOfRangeException>(() => bucket.MarkDirty((BucketDirtyReason)(1 << 20)));
    }

    [Fact]
    public void RefreshTicketCannotBeUsedForAnotherBucket()
    {
        var first = CreateBucket("Motion", 30);
        var second = CreateBucket("Lighting", 15);

        Assert.True(first.TryBeginRefresh(Epoch, out var ticket));
        Assert.False(second.TryCompleteRefresh(ticket, Epoch.AddMilliseconds(1)));
        Assert.False(second.HasValidRepresentation);
    }

    [Fact]
    public void RefreshTicketCannotBeUsedForAnotherBucketWithTheSameName()
    {
        var first = CreateBucket("Lighting", 15);
        var second = CreateBucket("Lighting", 15);

        Assert.True(first.TryBeginRefresh(Epoch, out var ticket));
        Assert.False(second.TryCompleteRefresh(ticket, Epoch.AddMilliseconds(1)));
        Assert.False(second.HasValidRepresentation);
    }

    [Fact]
    public void MetricsDistinguishExplicitReuseFromDueRefreshAndRejectedWork()
    {
        var bucket = CreateBucket("FarEnvironment", 1);

        Assert.False(bucket.TryRecordReuse());
        Assert.True(bucket.TryBeginRefresh(Epoch, out var initialTicket));
        Assert.True(bucket.TryCompleteRefresh(initialTicket, Epoch));

        Assert.True(bucket.TryRecordReuse());
        Assert.False(bucket.IsRefreshDue(Epoch.AddMilliseconds(10)));
        bucket.MarkDirty(BucketDirtyReason.ResourceChanged);
        Assert.False(bucket.TryRecordReuse());

        Assert.True(bucket.TryBeginRefresh(Epoch.AddSeconds(2), out var staleTicket));
        bucket.SetSourceRevision(1);
        Assert.False(bucket.TryCompleteRefresh(staleTicket, Epoch.AddSeconds(2).AddMilliseconds(1)));

        var metrics = bucket.Metrics;
        Assert.Equal(3, metrics.RefreshChecks);
        Assert.Equal(2, metrics.RefreshDueChecks);
        Assert.Equal(1, metrics.RefreshNotDueChecks);
        Assert.Equal(2, metrics.RefreshAttempts);
        Assert.Equal(1, metrics.AcceptedRefreshes);
        Assert.Equal(1, metrics.RejectedRefreshes);
        Assert.Equal(2, metrics.Invalidations);
        Assert.Equal(1, metrics.ReuseCount);
        Assert.Equal(
            BucketDirtyReason.ResourceChanged | BucketDirtyReason.SourceChanged,
            metrics.ObservedInvalidationReasons);
    }

    [Fact]
    public void MultipleNamedBucketsCanShareOneHorizonAndKeepIndependentCadence()
    {
        var horizon = new EventHorizon("Near", 100, 30);
        var geometry = new EventHorizonBucket("Geometry", horizon, 30);
        var motion = new EventHorizonBucket("Motion", horizon, 15);
        CompleteInitialRefresh(geometry, Epoch);
        CompleteInitialRefresh(motion, Epoch);

        geometry.MarkDirty(BucketDirtyReason.ResourceChanged);

        Assert.Same(horizon, geometry.Horizon);
        Assert.Same(horizon, motion.Horizon);
        Assert.False(geometry.HasValidRepresentation);
        Assert.True(motion.HasValidRepresentation);
        Assert.True(geometry.IsRefreshDue(Epoch.AddMilliseconds(1)));
        Assert.False(motion.IsRefreshDue(Epoch.AddMilliseconds(1)));
    }

    [Fact]
    public void NearAndFarBucketsRefreshAtDifferentRatesWhileFarRepresentationRemainsReusable()
    {
        var nearHorizon = new EventHorizon("Near", 100, 30);
        var farHorizon = new EventHorizon("Far", 10000, 1);
        var near = new EventHorizonBucket("Zombies", nearHorizon, 30);
        var far = new EventHorizonBucket("Zombies", farHorizon, 1);
        CompleteInitialRefresh(near, Epoch);
        CompleteInitialRefresh(far, Epoch);

        Assert.True(near.IsRefreshDue(Epoch.AddMilliseconds(34)));
        Assert.False(far.IsRefreshDue(Epoch.AddMilliseconds(34)));
        Assert.True(near.HasValidRepresentation);
        Assert.True(far.HasValidRepresentation);
        Assert.True(far.TryRecordReuse());
        Assert.Equal(1, far.Metrics.ReuseCount);
    }

    private static EventHorizonBucket CreateBucket(string name, double updatesPerSecond) =>
        new(name, new EventHorizon("TestHorizon", 100, 1), updatesPerSecond);

    private static void CompleteInitialRefresh(EventHorizonBucket bucket, DateTimeOffset now)
    {
        Assert.True(bucket.TryBeginRefresh(now, out var ticket));
        Assert.True(bucket.TryCompleteRefresh(ticket, now));
    }
}
