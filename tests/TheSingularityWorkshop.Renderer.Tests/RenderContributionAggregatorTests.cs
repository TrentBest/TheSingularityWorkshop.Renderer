using TheSingularityWorkshop.Renderer;
using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class RenderContributionAggregatorTests
{
    private static RenderContribution Contribution(
        string source,
        long revision = 1,
        string key = "ash:region-1:kg-m3",
        double error = 0.01,
        bool hazard = false,
        bool interaction = false) =>
        new(source, revision, key, error, hazard, interaction);

    [Fact]
    public void AggregatesCompatibleSourcesAndPreservesEverySourceRevision()
    {
        var result = RenderContributionAggregator.Aggregate(
            [
                Contribution("source-c", revision: 7),
                Contribution("source-a", revision: 3),
                Contribution("source-b", revision: 5)
            ],
            maximumAggregateError: 0.1,
            additionalSourceError: 0.02);

        var aggregate = Assert.Single(result.Aggregates);
        Assert.True(aggregate.IsAggregated);
        Assert.Equal(
            new[]
            {
                new RenderContributionProvenance("source-a", 3),
                new RenderContributionProvenance("source-b", 5),
                new RenderContributionProvenance("source-c", 7)
            },
            aggregate.Sources);
        Assert.Equal(0.03, aggregate.EstimatedSourceError, 10);
        Assert.Equal(0.04, aggregate.AggregationError, 10);
        Assert.Equal(3, result.Metrics.AggregatedSources);
        Assert.Equal(0, result.Metrics.PreservedIndividually);
    }

    [Fact]
    public void HazardAndInteractionCriticalContributionsAreNeverAggregated()
    {
        var result = RenderContributionAggregator.Aggregate(
            [
                Contribution("hazard-a", hazard: true),
                Contribution("hazard-b", hazard: true),
                Contribution("interactive-a", interaction: true),
                Contribution("ordinary-a"),
                Contribution("ordinary-b")
            ],
            maximumAggregateError: 1,
            additionalSourceError: 0);

        Assert.Equal(4, result.Aggregates.Count);
        Assert.Equal(1, result.Aggregates.Count(item => item.IsAggregated));
        Assert.Equal(3, result.Aggregates.Count(item => !item.IsAggregated));
        Assert.Contains(result.Aggregates, item => item.Sources.Single().SourceId == "hazard-a");
        Assert.Contains(result.Aggregates, item => item.Sources.Single().SourceId == "hazard-b");
        Assert.Contains(result.Aggregates, item => item.Sources.Single().SourceId == "interactive-a");
    }

    [Fact]
    public void DifferentCompatibilityKeysRemainSeparate()
    {
        var result = RenderContributionAggregator.Aggregate(
            [
                Contribution("ash-a", key: "ash:north"),
                Contribution("ash-b", key: "ash:north"),
                Contribution("gas-a", key: "gas:north"),
                Contribution("gas-b", key: "gas:north")
            ],
            maximumAggregateError: 1,
            additionalSourceError: 0);

        Assert.Equal(2, result.Aggregates.Count);
        Assert.All(result.Aggregates, item => Assert.Equal(2, item.Sources.Count));
        Assert.Equal(
            new[] { "ash:north", "gas:north" },
            result.Aggregates.Select(item => item.AggregationKey));
    }

    [Fact]
    public void ErrorBudgetSplitsGroupsWithoutDroppingContributions()
    {
        var result = RenderContributionAggregator.Aggregate(
            [
                Contribution("a", error: 0.02),
                Contribution("b", error: 0.02),
                Contribution("c", error: 0.02),
                Contribution("d", error: 0.02)
            ],
            maximumAggregateError: 0.07,
            additionalSourceError: 0.02);

        Assert.Equal(2, result.Aggregates.Count);
        Assert.All(result.Aggregates, item => Assert.Equal(2, item.Sources.Count));
        Assert.All(result.Aggregates, item => Assert.True(item.EstimatedTotalError <= 0.07));
        Assert.Equal(4, result.Aggregates.Sum(item => item.Sources.Count));
    }

    [Fact]
    public void AContributionWhoseOwnErrorExceedsBudgetIsPreservedIndividually()
    {
        var result = RenderContributionAggregator.Aggregate(
            [
                Contribution("over-budget", error: 0.5),
                Contribution("ordinary-a"),
                Contribution("ordinary-b")
            ],
            maximumAggregateError: 0.1,
            additionalSourceError: 0.01);

        Assert.Equal(2, result.Aggregates.Count);
        var preserved = Assert.Single(result.Aggregates, item => item.Sources.Any(source => source.SourceId == "over-budget"));
        Assert.False(preserved.IsAggregated);
        Assert.Equal(0.5, preserved.EstimatedTotalError, 10);
    }

    [Fact]
    public void HonorsMinimumSourceCountWithoutPrematurelyFlushingEligibleContributions()
    {
        var result = RenderContributionAggregator.Aggregate(
            [Contribution("a"), Contribution("b"), Contribution("c")],
            maximumAggregateError: 1,
            additionalSourceError: 0,
            minimumSources: 3);

        var aggregate = Assert.Single(result.Aggregates);
        Assert.Equal(3, aggregate.Sources.Count);
        Assert.True(aggregate.IsAggregated);
    }

    [Fact]
    public void PreservesSubMinimumRemainderAsIndividualSources()
    {
        var result = RenderContributionAggregator.Aggregate(
            [Contribution("a"), Contribution("b"), Contribution("c"), Contribution("d")],
            maximumAggregateError: 0.09,
            additionalSourceError: 0.02,
            minimumSources: 3);

        Assert.Equal(2, result.Aggregates.Count);
        Assert.Equal(3, Assert.Single(result.Aggregates, item => item.IsAggregated).Sources.Count);
        var remainder = Assert.Single(result.Aggregates, item => !item.IsAggregated);
        Assert.Equal("d", remainder.Sources[0].SourceId);
    }

    [Fact]
    public void RejectsDuplicateSourceIdentityInOneSnapshotEvenWhenRevisionsDiffer()
    {
        Assert.Throws<ArgumentException>(() =>
            RenderContributionAggregator.Aggregate(
                [Contribution("same-source", revision: 1), Contribution("same-source", revision: 2)],
                maximumAggregateError: 1,
                additionalSourceError: 0));
    }

    [Fact]
    public void RejectsInvalidBudgetsAndMinimumSourceCount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RenderContributionAggregator.Aggregate([], double.NaN, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RenderContributionAggregator.Aggregate([], 1, double.PositiveInfinity));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RenderContributionAggregator.Aggregate([], 1, 0, minimumSources: 1));
    }

    [Fact]
    public void AggregationOrderAndProvenanceAreDeterministicRegardlessOfInputOrder()
    {
        var first = new[]
        {
            Contribution("c", revision: 3),
            Contribution("a", revision: 1),
            Contribution("b", revision: 2)
        };
        var second = first.Reverse();

        var left = RenderContributionAggregator.Aggregate(first, 1, 0.01);
        var right = RenderContributionAggregator.Aggregate(second, 1, 0.01);

        Assert.Equal(
            left.Aggregates.Select(item => (item.AggregationKey, item.EstimatedSourceError, item.AggregationError)),
            right.Aggregates.Select(item => (item.AggregationKey, item.EstimatedSourceError, item.AggregationError)));
        Assert.Equal(left.Aggregates[0].Sources, right.Aggregates[0].Sources);
    }
}
