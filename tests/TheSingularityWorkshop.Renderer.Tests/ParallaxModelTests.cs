using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class ParallaxModelTests
{
    [Fact]
    public void ProjectHorizontalUsesPerspectiveDepth()
    {
        Assert.Equal(2.0, ParallaxModel.ProjectHorizontal(10.0, 5.0, 1.0), 10);
        Assert.Equal(1.0, ParallaxModel.ProjectHorizontal(10.0, 10.0, 1.0), 10);
    }

    [Fact]
    public void LateralImageDisplacementDecreasesWithDistance()
    {
        var near = ParallaxModel.LateralImageDisplacement(10.0, 10.0, 960.0);
        var far = ParallaxModel.LateralImageDisplacement(10.0, 300_000.0, 960.0);
        Assert.Equal(960.0, near, 10);
        Assert.Equal(0.032, far, 10);
    }

    [Fact]
    public void RelativeParallaxIsDifferenceOfInverseDepths()
    {
        var parallax = ParallaxModel.RelativeLateralParallax(10.0, 10.0, 300_000.0, 960.0);
        Assert.Equal(959.968, parallax, 10);
    }

    [Fact]
    public void ViewAngleUsesPinholeGeometry()
    {
        var angle = ParallaxModel.ViewAngle(1.0, 1.0);
        Assert.Equal(Math.PI / 4.0, angle, 10);
    }
}