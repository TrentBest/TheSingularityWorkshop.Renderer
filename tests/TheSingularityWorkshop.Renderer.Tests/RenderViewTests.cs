using System.Numerics;
using TheSingularityWorkshop.Renderer;
using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class RenderViewTests
{
    [Fact]
    public void AcceptsFiniteViewState()
    {
        var view = new RenderView(Vector3.One, Matrix4x4.Identity, Matrix4x4.Identity);

        Assert.Equal(Vector3.One, view.ObserverPosition);
        Assert.Equal(Matrix4x4.Identity, view.View);
        Assert.Equal(Matrix4x4.Identity, view.Projection);
    }

    [Fact]
    public void RejectsNonFiniteObserverPosition()
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RenderView(new Vector3(0, float.NaN, 0), Matrix4x4.Identity, Matrix4x4.Identity));

        Assert.Equal("observerPosition", error.ParamName);
    }

    [Fact]
    public void RejectsNonFiniteViewMatrix()
    {
        var matrix = Matrix4x4.Identity;
        matrix.M21 = float.NaN;

        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RenderView(Vector3.Zero, matrix, Matrix4x4.Identity));

        Assert.Equal("view", error.ParamName);
        Assert.Contains("finite", error.Message);
    }

    [Fact]
    public void RejectsNonFiniteProjectionMatrix()
    {
        var matrix = Matrix4x4.Identity;
        matrix.M44 = float.NegativeInfinity;

        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RenderView(Vector3.Zero, Matrix4x4.Identity, matrix));

        Assert.Equal("projection", error.ParamName);
        Assert.Contains("finite", error.Message);
    }
}
