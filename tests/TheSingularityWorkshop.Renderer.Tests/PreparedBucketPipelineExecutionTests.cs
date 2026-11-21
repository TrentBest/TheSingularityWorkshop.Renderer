using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

[Collection(FsmApiSharedRegistryCollection.Name)]
public sealed class PreparedBucketPipelineExecutionTests
{
    [Fact]
    public void SuccessfulStagesExecuteInOrderBeforeFinalization()
    {
        var trace = new List<string>();
        var pipeline = CreatePipeline();
        using var execution = new PreparedBucketPipelineExecution(
            "Lighting.Success",
            pipeline,
            new Dictionary<string, Action>
            {
                ["PreProcessing"] = () => trace.Add("PreProcessing"),
                ["Processing"] = () => trace.Add("Processing"),
                ["PostProcessing"] = () => trace.Add("PostProcessing"),
                ["Finalization"] = () => trace.Add("FinalizationStage")
            },
            () => { trace.Add("Publish"); return true; });

        Assert.Equal(PreparedBucketPipelineExecutionState.Executing, execution.State);
        Assert.Equal(PreparedBucketPipelineExecutionState.Executing, execution.Advance());
        Assert.Equal(PreparedBucketPipelineExecutionState.Executing, execution.Advance());
        Assert.Equal(PreparedBucketPipelineExecutionState.Executing, execution.Advance());
        Assert.Equal(PreparedBucketPipelineExecutionState.Finalization, execution.Advance());
        Assert.False(execution.IsFinalized);
        Assert.Equal(PreparedBucketPipelineExecutionState.Completed, execution.Advance());
        Assert.True(execution.IsFinalized);
        Assert.Equal(
            new[] { "PreProcessing", "Processing", "PostProcessing", "FinalizationStage", "Publish" },
            trace);
    }

    [Fact]
    public void FailedStageTransitionsToFailedAndNeverRunsFinalization()
    {
        var finalized = false;
        var pipeline = CreatePipeline();
        using var execution = new PreparedBucketPipelineExecution(
            "Lighting.Failure",
            pipeline,
            new Dictionary<string, Action>
            {
                ["PreProcessing"] = () => { },
                ["Processing"] = () => throw new InvalidOperationException("stage failed"),
                ["PostProcessing"] = () => { },
                ["Finalization"] = () => { }
            },
            () => finalized = true);

        Assert.Equal(PreparedBucketPipelineExecutionState.Executing, execution.Advance());
        Assert.Equal(PreparedBucketPipelineExecutionState.Failed, execution.Advance());
        Assert.False(execution.Advance() == PreparedBucketPipelineExecutionState.Completed);
        Assert.False(finalized);
        Assert.False(execution.IsFinalized);
        Assert.IsType<InvalidOperationException>(execution.Failure);
    }

    [Fact]
    public void FinalizationFailureDoesNotClaimSuccessfulPublication()
    {
        using var execution = new PreparedBucketPipelineExecution(
            "Lighting.FinalizeFailure",
            CreatePipeline(),
            new Dictionary<string, Action>
            {
                ["PreProcessing"] = () => { },
                ["Processing"] = () => { },
                ["PostProcessing"] = () => { },
                ["Finalization"] = () => { }
            },
            () => throw new InvalidOperationException("publish failed"));

        for (var i = 0; i < 4; i++)
            execution.Advance();

        Assert.Equal(PreparedBucketPipelineExecutionState.Failed, execution.Advance());
        Assert.False(execution.IsFinalized);
        Assert.IsType<InvalidOperationException>(execution.Failure);
    }


    [Fact]
    public void RejectedPublicationGateDoesNotMarkPipelineFinalized()
    {
        using var execution = new PreparedBucketPipelineExecution(
            "Lighting.RejectedPublication",
            CreatePipeline(),
            new Dictionary<string, Action>
            {
                ["PreProcessing"] = () => { },
                ["Processing"] = () => { },
                ["PostProcessing"] = () => { },
                ["Finalization"] = () => { }
            },
            () => false);

        for (var i = 0; i < 4; i++)
            execution.Advance();

        Assert.Equal(PreparedBucketPipelineExecutionState.Failed, execution.Advance());
        Assert.False(execution.IsFinalized);
        Assert.IsType<InvalidOperationException>(execution.Failure);
    }

    [Fact]
    public void EveryPreparedStageRequiresExactlyOneRuntimeAction()
    {
        Assert.Throws<ArgumentException>(() =>
            new PreparedBucketPipelineExecution(
                "Lighting.MissingAction",
                CreatePipeline(),
                new Dictionary<string, Action> { ["PreProcessing"] = () => { } },
                () => true));
    }

    [Fact]
    public void DisposedExecutionCannotAdvance()
    {
        var execution = new PreparedBucketPipelineExecution(
            "Lighting.Disposed",
            CreatePipeline(),
            new Dictionary<string, Action>
            {
                ["PreProcessing"] = () => { },
                ["Processing"] = () => { },
                ["PostProcessing"] = () => { },
                ["Finalization"] = () => { }
            },
            () => true);

        execution.Dispose();

        Assert.Throws<ObjectDisposedException>(() => execution.Advance());
    }

    private static PreparedBucketPipeline CreatePipeline() =>
        PreparedBucketPipeline.Prepare(
            "Lighting",
            [
                new RendererPipelineStage("PreProcessing"),
                new RendererPipelineStage("Processing", ["PreProcessing"]),
                new RendererPipelineStage("PostProcessing", ["Processing"]),
                new RendererPipelineStage("Finalization", ["PostProcessing"])
            ]);
}
