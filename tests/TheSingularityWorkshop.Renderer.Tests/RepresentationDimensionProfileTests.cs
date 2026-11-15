using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class RepresentationDimensionProfileTests
{
    [Fact]
    public void MeetsOrExceeds_ComparesEveryDimensionIndependently()
    {
        var actual = new RepresentationDimensionProfile(
            RepresentationDetailLevel.Full,
            RepresentationDetailLevel.Summary,
            RepresentationDetailLevel.Reduced,
            RepresentationDetailLevel.Omitted,
            30,
            RepresentationResidency.Streamed,
            RepresentationDetailLevel.Full,
            RepresentationDetailLevel.Summary,
            RepresentationDetailLevel.Reduced);

        var minimum = new RepresentationDimensionProfile(
            RepresentationDetailLevel.Reduced,
            RepresentationDetailLevel.Summary,
            RepresentationDetailLevel.Reduced,
            RepresentationDetailLevel.Omitted,
            20,
            RepresentationResidency.Streamed,
            RepresentationDetailLevel.Full,
            RepresentationDetailLevel.Summary,
            RepresentationDetailLevel.Summary);

        Assert.True(actual.MeetsOrExceeds(minimum));
    }

    [Fact]
    public void MeetsOrExceeds_DoesNotTradeOneDimensionForAnother()
    {
        var actual = new RepresentationDimensionProfile(
            RepresentationDetailLevel.Full,
            RepresentationDetailLevel.Full,
            RepresentationDetailLevel.Full,
            RepresentationDetailLevel.Full,
            60,
            RepresentationResidency.Resident,
            RepresentationDetailLevel.Summary,
            RepresentationDetailLevel.Full,
            RepresentationDetailLevel.Full);

        var minimum = new RepresentationDimensionProfile(
            RepresentationDetailLevel.Summary,
            RepresentationDetailLevel.Summary,
            RepresentationDetailLevel.Summary,
            RepresentationDetailLevel.Summary,
            30,
            RepresentationResidency.Streamed,
            RepresentationDetailLevel.Reduced,
            RepresentationDetailLevel.Summary,
            RepresentationDetailLevel.Summary);

        Assert.False(actual.MeetsOrExceeds(minimum));
    }

    [Fact]
    public void ZeroCadenceIsAnExplicitEventDrivenProfile()
    {
        var profile = CreateProfile(updateCadenceHz: 0);

        Assert.Equal(0, profile.UpdateCadenceHz);
        Assert.True(profile.MeetsOrExceeds(CreateProfile(updateCadenceHz: 0)));
        Assert.False(profile.MeetsOrExceeds(CreateProfile(updateCadenceHz: 1)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void ConstructorRejectsInvalidCadence(double cadence)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateProfile(updateCadenceHz: cadence));
    }

    [Fact]
    public void ConstructorRejectsUndefinedDimension()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RepresentationDimensionProfile(
                (RepresentationDetailLevel)99,
                RepresentationDetailLevel.Full,
                RepresentationDetailLevel.Full,
                RepresentationDetailLevel.Full,
                60,
                RepresentationResidency.Resident,
                RepresentationDetailLevel.Full,
                RepresentationDetailLevel.Full,
                RepresentationDetailLevel.Full));
    }

    private static RepresentationDimensionProfile CreateProfile(double updateCadenceHz = 30) =>
        new(
            RepresentationDetailLevel.Full,
            RepresentationDetailLevel.Full,
            RepresentationDetailLevel.Full,
            RepresentationDetailLevel.Full,
            updateCadenceHz,
            RepresentationResidency.Resident,
            RepresentationDetailLevel.Full,
            RepresentationDetailLevel.Full,
            RepresentationDetailLevel.Full);
}
