using TheSingularityWorkshop.Renderer;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class EventHorizonTests
{
    [Fact]
    public void Constructor_StoresPolicy()
    {
        var horizon = new EventHorizon("Near", 10, 60);

        Assert.Equal("Near", horizon.Name);
        Assert.Equal(10, horizon.MaximumDistance);
        Assert.Equal(60, horizon.UpdatesPerSecond);
    }

    [Fact]
    public void Constructor_RejectsInvalidDistance()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new EventHorizon("Near", -1, 60));
    }

    [Fact]
    public void Constructor_RejectsInvalidFrequency()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new EventHorizon("Near", 10, 0));
    }

    [Fact]
    public void Select_ReturnsNearestMatchingHorizon()
    {
        var horizons = new[]
        {
            new EventHorizon("Near", 10, 60),
            new EventHorizon("Context", 50, 15),
            new EventHorizon("Distant", 250, 1)
        };

        Assert.Equal("Near", ObserverDistance.Select(8, horizons).Name);
        Assert.Equal("Context", ObserverDistance.Select(25, horizons).Name);
        Assert.Equal("Distant", ObserverDistance.Select(500, horizons).Name);
    }

    [Fact]
    public void Select_RejectsOutOfOrderHorizons()
    {
        var horizons = new[]
        {
            new EventHorizon("Far", 100, 1),
            new EventHorizon("Near", 10, 60)
        };

        Assert.Throws<ArgumentException>(() => ObserverDistance.Select(20, horizons));
    }
}
