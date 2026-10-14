using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class ObserverRepresentationCoordinatorTests
{
    [Fact]
    public void Evaluate_SelectsHorizon_AdvancesFsm_AndIssuesInitialRefreshTicket()
    {
        var (coordinator, _) = CreateCoordinator();

        using (coordinator)
        {
            var now = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
            var evaluation = coordinator.Evaluate(0.5, sourceRevision: 1, observerRevision: 1, now);

            Assert.Equal(RepresentationTarget.Interactive, evaluation.Decision.Representation);
            Assert.Equal("Interactive", evaluation.Decision.HorizonName);
            Assert.Equal(RepresentationTarget.Interactive, evaluation.ActiveRepresentation);
            Assert.Equal(RepresentationTarget.Interactive, coordinator.ActiveRepresentation);
            Assert.True(evaluation.RefreshRequired);
            Assert.NotNull(evaluation.RefreshTicket);
            Assert.False(evaluation.CanReuseCachedRepresentation);
        }
    }

    [Fact]
    public void Evaluate_ReusesValidCacheUntilCadenceExpires()
    {
        var (coordinator, buckets) = CreateCoordinator();
        using (coordinator)
        {
            var now = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
            var initial = coordinator.Evaluate(0.5, 1, 1, now);
            Assert.True(initial.RefreshTicket.HasValue);
            Assert.True(initial.Bucket.TryCompleteRefresh(initial.RefreshTicket!.Value, now));

            var reused = coordinator.Evaluate(0.5, 1, 1, now.AddMilliseconds(20));
            Assert.False(reused.RefreshRequired);
            Assert.True(reused.CanReuseCachedRepresentation);
            Assert.Equal(1, reused.Bucket.Metrics.ReuseCount);

            var due = coordinator.Evaluate(0.5, 1, 1, now.AddMilliseconds(100));
            Assert.True(due.RefreshRequired);
            Assert.False(due.CanReuseCachedRepresentation);
        }
    }

    [Fact]
    public void Evaluate_InvalidatesSelectedBucketWhenObserverRevisionChanges()
    {
        var (coordinator, _) = CreateCoordinator();
        using (coordinator)
        {
            var now = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
            var initial = coordinator.Evaluate(2.5, 1, 7, now);
            Assert.True(initial.RefreshTicket.HasValue);
            Assert.True(initial.Bucket.TryCompleteRefresh(initial.RefreshTicket!.Value, now));

            var changedObserver = coordinator.Evaluate(2.5, 1, 8, now.AddMilliseconds(1));
            Assert.True(changedObserver.RefreshRequired);
            Assert.True(changedObserver.Bucket.DirtyReasons.HasFlag(BucketDirtyReason.ObserverChanged));
        }
    }

    [Fact]
    public void Evaluate_RejectsPolicyHorizonsWithoutBuckets()
    {
        var horizon = new EventHorizon("Interactive", 1, 10);
        var policy = new ObserverRepresentationPolicy(
        [
            new RepresentationHorizonRule(horizon, RepresentationTarget.Interactive)
        ]);
        using var coordinator = new ObserverRepresentationCoordinator(
            policy,
            Array.Empty<EventHorizonBucket>(),
            "missing-bucket-test");

        Assert.Throws<InvalidOperationException>(() =>
            coordinator.Evaluate(0.5, 1, 1, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Constructor_RejectsDuplicateBucketsForOneHorizon()
    {
        var horizon = new EventHorizon("Interactive", 1, 10);
        var policy = new ObserverRepresentationPolicy(
        [
            new RepresentationHorizonRule(horizon, RepresentationTarget.Interactive)
        ]);
        var buckets = new[]
        {
            new EventHorizonBucket("geometry", horizon, 10),
            new EventHorizonBucket("motion", horizon, 10)
        };

        Assert.Throws<ArgumentException>(() =>
            new ObserverRepresentationCoordinator(policy, buckets, "duplicate-bucket-test"));
    }

    private static (ObserverRepresentationCoordinator Coordinator, EventHorizonBucket[] Buckets) CreateCoordinator()
    {
        var rules = new[]
        {
            new RepresentationHorizonRule(new EventHorizon("Interactive", 1, 10), RepresentationTarget.Interactive),
            new RepresentationHorizonRule(new EventHorizon("Near", 5, 8), RepresentationTarget.Near),
            new RepresentationHorizonRule(new EventHorizon("Distant", 15, 4), RepresentationTarget.Distant),
            new RepresentationHorizonRule(new EventHorizon("Aggregate", 30, 2), RepresentationTarget.Aggregate),
            new RepresentationHorizonRule(new EventHorizon("Semantic", 100, 1), RepresentationTarget.Semantic)
        };
        var policy = new ObserverRepresentationPolicy(rules);
        var buckets = rules.Select(rule =>
            new EventHorizonBucket($"bucket-{rule.Horizon.Name}", rule.Horizon, rule.Horizon.UpdatesPerSecond))
            .ToArray();

        return (new ObserverRepresentationCoordinator(policy, buckets, "observer-coordinator-test"), buckets);
    }
}