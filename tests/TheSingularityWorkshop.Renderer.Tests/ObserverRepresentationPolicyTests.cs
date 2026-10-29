using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class ObserverRepresentationPolicyTests
{
    [Theory]
    [InlineData(0.5, "Interactive", RepresentationTarget.Interactive, 10)]
    [InlineData(1.0, "Interactive", RepresentationTarget.Interactive, 10)]
    [InlineData(2.5, "Near", RepresentationTarget.Near, 8)]
    [InlineData(8.0, "Distant", RepresentationTarget.Distant, 4)]
    [InlineData(20.0, "Aggregate", RepresentationTarget.Aggregate, 2)]
    [InlineData(45.0, "Semantic", RepresentationTarget.Semantic, 1)]
    [InlineData(500.0, "Semantic", RepresentationTarget.Semantic, 1)]
    public void Select_UsesTheSelectedHorizonRule(
        double distance,
        string expectedHorizon,
        RepresentationTarget expectedRepresentation,
        double expectedCadence)
    {
        var decision = CreatePolicy().Select(distance);

        Assert.Equal(distance, decision.Distance);
        Assert.Equal(expectedHorizon, decision.HorizonName);
        Assert.Equal(expectedRepresentation, decision.Representation);
        Assert.Equal(expectedCadence, decision.UpdatesPerSecond);
    }

    [Theory]
    [InlineData(500.0, RepresentationTarget.Interactive, "Interactive", RepresentationTarget.Interactive, 10)]
    [InlineData(500.0, RepresentationTarget.Near, "Near", RepresentationTarget.Near, 8)]
    [InlineData(8.0, RepresentationTarget.Semantic, "Distant", RepresentationTarget.Distant, 4)]
    public void Select_WithMinimumRepresentationCombinesDistanceAndCallerRequirement(
        double distance,
        RepresentationTarget minimumRepresentation,
        string expectedHorizon,
        RepresentationTarget expectedRepresentation,
        double expectedCadence)
    {
        var decision = CreatePolicy().Select(distance, minimumRepresentation);

        Assert.Equal(distance, decision.Distance);
        Assert.Equal(expectedHorizon, decision.HorizonName);
        Assert.Equal(expectedRepresentation, decision.Representation);
        Assert.Equal(expectedCadence, decision.UpdatesPerSecond);
    }

    [Fact]
    public void Select_WithMinimumRepresentationRejectsUndefinedTarget()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreatePolicy().Select(500, (RepresentationTarget)99));
    }

    [Fact]
    public void Select_WithUnsupportedMinimumUsesMostDetailedAvailableRule()
    {
        var policy = new ObserverRepresentationPolicy(
        [
            new RepresentationHorizonRule(new EventHorizon("Distant", 100, 1), RepresentationTarget.Distant),
            new RepresentationHorizonRule(new EventHorizon("Semantic", 500, 0.5), RepresentationTarget.Semantic)
        ]);

        var decision = policy.Select(500, RepresentationTarget.Interactive);

        Assert.Equal("Distant", decision.HorizonName);
        Assert.Equal(RepresentationTarget.Distant, decision.Representation);
    }

    [Fact]
    public void Constructor_RejectsEmptyRules()
    {
        Assert.Throws<ArgumentException>(() =>
            new ObserverRepresentationPolicy(Array.Empty<RepresentationHorizonRule>()));
    }

    [Fact]
    public void Constructor_RejectsRulesOutOfDistanceOrder()
    {
        var rules = new[]
        {
            new RepresentationHorizonRule(
                new EventHorizon("Near", 10, 5),
                RepresentationTarget.Near),
            new RepresentationHorizonRule(
                new EventHorizon("Close", 2, 10),
                RepresentationTarget.Interactive)
        };

        Assert.Throws<ArgumentException>(() => new ObserverRepresentationPolicy(rules));
    }

    [Fact]
    public void Constructor_RejectsTheSameHorizonBeingMappedTwice()
    {
        var horizon = new EventHorizon("Near", 10, 5);
        var rules = new[]
        {
            new RepresentationHorizonRule(horizon, RepresentationTarget.Near),
            new RepresentationHorizonRule(horizon, RepresentationTarget.Interactive)
        };

        Assert.Throws<ArgumentException>(() => new ObserverRepresentationPolicy(rules));
    }

    [Fact]
    public void Constructor_RejectsUndefinedRepresentation()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RepresentationHorizonRule(
                new EventHorizon("Unknown", 10, 5),
                (RepresentationTarget)99));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Select_RejectsInvalidDistance(double distance)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreatePolicy().Select(distance));
    }

    private static ObserverRepresentationPolicy CreatePolicy() => new(
    [
        new RepresentationHorizonRule(
            new EventHorizon(nameof(RepresentationTarget.Interactive), 1, 10),
            RepresentationTarget.Interactive),
        new RepresentationHorizonRule(
            new EventHorizon(nameof(RepresentationTarget.Near), 5, 8),
            RepresentationTarget.Near),
        new RepresentationHorizonRule(
            new EventHorizon(nameof(RepresentationTarget.Distant), 15, 4),
            RepresentationTarget.Distant),
        new RepresentationHorizonRule(
            new EventHorizon(nameof(RepresentationTarget.Aggregate), 30, 2),
            RepresentationTarget.Aggregate),
        new RepresentationHorizonRule(
            new EventHorizon(nameof(RepresentationTarget.Semantic), 100, 1),
            RepresentationTarget.Semantic)
    ]);
}