using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class RendererPerformanceModelTests
{
    [Fact]
    public void WorkshopCalibration_ReproducesTheMeasuredFiftyGroupPoint()
    {
        var model = RendererPerformanceModel.FromWorkshopFsmApiBenchmark();

        Assert.Equal(305.1, model.EstimateUpdateNanoseconds(1), precision: 3);
        Assert.Equal(15_736.6, model.EstimateUpdateNanoseconds(50), precision: 3);
        Assert.Equal(18_000, model.EstimateAllocationBytes(50), precision: 3);
    }

    [Fact]
    public void ModelCanEstimateCpuBudget()
    {
        var model = RendererPerformanceModel.FromWorkshopFsmApiBenchmark();

        var milliseconds = model.EstimateCpuMillisecondsPerSecond(
            processingGroups: 50,
            schedulerUpdatesPerSecond: 60);

        Assert.Equal(0.944196, milliseconds, precision: 5);
    }
}
