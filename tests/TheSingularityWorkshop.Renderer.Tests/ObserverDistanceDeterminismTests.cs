using TheSingularityWorkshop.Renderer;
using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

/// <summary>
/// Locks down deterministic observer-to-horizon selection at boundaries and
/// across independent observers. This tests policy selection only; it does not
/// claim that any selected representation has been rendered or simulated.
/// </summary>
public sealed class ObserverDistanceDeterminismTests
{
    private static EventHorizon[] CreatePolicy() =>
    [
        new EventHorizon("Immediate", 10, 60),
        new EventHorizon("Local", 100, 15),
        new EventHorizon("Regional", 1_000, 1),
        new EventHorizon("Semantic", 10_000, 0.1)
    ];

    [Theory]
    [InlineData(0, "Immediate")]
    [InlineData(10, "Immediate")]
    [InlineData(10.0001, "Local")]
    [InlineData(100, "Local")]
    [InlineData(100.0001, "Regional")]
    [InlineData(1_000, "Regional")]
    [InlineData(1_000.0001, "Semantic")]
    [InlineData(10_000, "Semantic")]
    [InlineData(1_000_000, "Semantic")]
    public void Select_IsDeterministicAtBoundariesAndBeyond(
        double distance,
        string expectedName)
    {
        var policy = CreatePolicy();

        for (var iteration = 0; iteration < 5; iteration++)
        {
            Assert.Equal(expectedName, ObserverDistance.Select(distance, policy).Name);
        }
    }

    [Fact]
    public void IndependentObserversCanSelectDifferentHorizonsForSameWorldEntity()
    {
        var policy = CreatePolicy();

        // The world entity and its horizon policy are shared. Observer distances
        // are separate inputs, so one observer's result cannot overwrite another's.
        var firstObserver = ObserverDistance.Select(4, policy);
        var secondObserver = ObserverDistance.Select(420, policy);
        var thirdObserver = ObserverDistance.Select(80_000, policy);

        Assert.Equal("Immediate", firstObserver.Name);
        Assert.Equal("Regional", secondObserver.Name);
        Assert.Equal("Semantic", thirdObserver.Name);

        Assert.Equal("Immediate", ObserverDistance.Select(4, policy).Name);
        Assert.Equal("Regional", ObserverDistance.Select(420, policy).Name);
    }

    [Fact]
    public void SelectionDoesNotMutateTheSharedHorizonPolicy()
    {
        var policy = CreatePolicy();
        var before = policy.Select(horizon => (horizon.Name, horizon.MaximumDistance, horizon.UpdatesPerSecond)).ToArray();

        _ = ObserverDistance.Select(50, policy);
        _ = ObserverDistance.Select(5_000, policy);
        _ = ObserverDistance.Select(5, policy);

        var after = policy.Select(horizon => (horizon.Name, horizon.MaximumDistance, horizon.UpdatesPerSecond)).ToArray();

        Assert.Equal(before, after);
    }
}
