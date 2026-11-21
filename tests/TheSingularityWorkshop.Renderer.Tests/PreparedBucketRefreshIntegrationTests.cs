using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

[Collection(FsmApiSharedRegistryCollection.Name)]
public sealed class PreparedBucketRefreshIntegrationTests
{
    private static readonly DateTimeOffset Epoch =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void StaleLightingPublicationFailsWithoutBlockingIndependentShadowsPipeline()
    {
        var horizon = new EventHorizon("Near", 100, 30);
        var lighting = new EventHorizonBucket("Lighting", horizon, 30);
        var shadows = new EventHorizonBucket("Shadows", horizon, 15);

        Assert.True(lighting.TryBeginRefresh(Epoch, out var lightingTicket));
        Assert.True(shadows.TryBeginRefresh(Epoch, out var shadowsTicket));

        var lightingStages = CreateStageActions();
        var shadowsStages = CreateStageActions();

        using var lightingExecution = new PreparedBucketPipelineExecution(
            "Near.Lighting.Stale",
            CreatePipeline("Lighting"),
            lightingStages,
            () => lighting.TryCompleteRefresh(lightingTicket, Epoch.AddMilliseconds(10)));

        using var shadowsExecution = new PreparedBucketPipelineExecution(
            "Near.Shadows.Valid",
            CreatePipeline("Shadows"),
            shadowsStages,
            () => shadows.TryCompleteRefresh(shadowsTicket, Epoch.AddMilliseconds(10)));

        // Each bucket advances its own FSM instance. No shared bucket state is implied.
        for (var i = 0; i < 4; i++)
        {
            lightingExecution.Advance();
            shadowsExecution.Advance();
        }

        Assert.Equal(PreparedBucketPipelineExecutionState.Finalization, lightingExecution.State);
        Assert.Equal(PreparedBucketPipelineExecutionState.Finalization, shadowsExecution.State);

        // A lighting source change invalidates only its ticket while both pipelines are in flight.
        lighting.SetSourceRevision(2);

        Assert.Equal(PreparedBucketPipelineExecutionState.Failed, lightingExecution.Advance());
        Assert.Equal(PreparedBucketPipelineExecutionState.Completed, shadowsExecution.Advance());

        Assert.False(lightingExecution.IsFinalized);
        Assert.IsType<InvalidOperationException>(lightingExecution.Failure);
        Assert.False(lighting.HasValidRepresentation);
        Assert.Equal(1, lighting.Metrics.RejectedRefreshes);
        Assert.Equal(0, lighting.Metrics.AcceptedRefreshes);

        Assert.True(shadowsExecution.IsFinalized);
        Assert.True(shadows.HasValidRepresentation);
        Assert.Equal(1, shadows.Metrics.AcceptedRefreshes);
        Assert.Equal(0, shadows.Metrics.RejectedRefreshes);
    }

    private static IReadOnlyDictionary<string, Action> CreateStageActions() =>
        new Dictionary<string, Action>
        {
            ["PreProcessing"] = () => { },
            ["Processing"] = () => { },
            ["PostProcessing"] = () => { },
            ["Finalization"] = () => { }
        };

    private static PreparedBucketPipeline CreatePipeline(string name) =>
        PreparedBucketPipeline.Prepare(
            name,
            [
                new RendererPipelineStage("PreProcessing"),
                new RendererPipelineStage("Processing", ["PreProcessing"]),
                new RendererPipelineStage("PostProcessing", ["Processing"]),
                new RendererPipelineStage("Finalization", ["PostProcessing"])
            ]);
}
