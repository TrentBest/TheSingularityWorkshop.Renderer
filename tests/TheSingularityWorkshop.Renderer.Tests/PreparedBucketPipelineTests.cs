using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class PreparedBucketPipelineTests
{
    private static readonly RendererPipelineStage[] DefaultStages =
    [
        new("PreProcessing"),
        new("Processing", ["PreProcessing"]),
        new("PostProcessing", ["Processing"]),
        new("Finalization", ["PostProcessing"])
    ];

    [Fact]
    public void EmptyExperienceConfigurationUsesBundleDefaultsAndDefaultStageOrder()
    {
        var defaults = new Dictionary<string, string>
        {
            ["quality"] = "balanced",
            ["shadow-cadence"] = "15"
        };

        var prepared = PreparedBucketPipeline.Prepare("Lighting", DefaultStages, defaults);

        Assert.Equal(new[] { "PreProcessing", "Processing", "PostProcessing", "Finalization" },
            prepared.Stages.Select(stage => stage.Id));
        Assert.Equal("balanced", prepared.Settings["quality"]);
        Assert.Equal("15", prepared.Settings["shadow-cadence"]);
    }

    [Fact]
    public void ExplicitExperienceValuesOverrideOnlyMatchingDefaults()
    {
        var prepared = PreparedBucketPipeline.Prepare(
            "Lighting",
            DefaultStages,
            new Dictionary<string, string>
            {
                ["quality"] = "balanced",
                ["shadow-cadence"] = "15",
                ["ambient"] = "enabled"
            },
            new Dictionary<string, string> { ["quality"] = "high" });

        Assert.Equal("high", prepared.Settings["quality"]);
        Assert.Equal("15", prepared.Settings["shadow-cadence"]);
        Assert.Equal("enabled", prepared.Settings["ambient"]);
    }

    [Fact]
    public void OptionalContributionIsInsertedAtItsDeclaredPosition()
    {
        var prepared = PreparedBucketPipeline.Prepare(
            "Lighting",
            DefaultStages,
            contributions:
            [
                new RendererPipelineStageContribution(
                    new RendererPipelineStage("ParticleEffects", ["Processing"]),
                    "PostProcessing")
            ]);

        Assert.Equal(
            new[] { "PreProcessing", "Processing", "ParticleEffects", "PostProcessing", "Finalization" },
            prepared.Stages.Select(stage => stage.Id));
    }

    [Fact]
    public void DayAndNightAreDistinctPreparedConfigurationsWithoutRuntimeBranching()
    {
        var daylight = PreparedBucketPipeline.Prepare(
            "Lighting.Day",
            DefaultStages,
            new Dictionary<string, string>
            {
                ["quality"] = "balanced",
                ["light-aggregation"] = "clustered"
            });

        var night = PreparedBucketPipeline.Prepare(
            "Lighting.Night",
            DefaultStages,
            new Dictionary<string, string>
            {
                ["quality"] = "high",
                ["light-aggregation"] = "individual"
            },
            contributions:
            [
                new RendererPipelineStageContribution(
                    new RendererPipelineStage("NightLighting", ["Processing"]),
                    "PostProcessing")
            ]);

        Assert.Equal("balanced", daylight.Settings["quality"]);
        Assert.Equal("high", night.Settings["quality"]);
        Assert.DoesNotContain(daylight.Stages, stage => stage.Id == "NightLighting");
        Assert.Contains(night.Stages, stage => stage.Id == "NightLighting");
    }

    [Fact]
    public void MissingOrOutOfOrderStageDependenciesAreRejected()
    {
        Assert.Throws<ArgumentException>(() =>
            PreparedBucketPipeline.Prepare(
                "Invalid.Missing",
                [new RendererPipelineStage("Finalization", ["Processing"])]));

        Assert.Throws<ArgumentException>(() =>
            PreparedBucketPipeline.Prepare(
                "Invalid.Order",
                [
                    new RendererPipelineStage("Finalization", ["Processing"]),
                    new RendererPipelineStage("Processing")
                ]));
    }

    [Fact]
    public void DuplicateStagesAndUnknownInsertionAnchorsAreRejected()
    {
        Assert.Throws<ArgumentException>(() =>
            PreparedBucketPipeline.Prepare(
                "Invalid.Duplicate",
                [new RendererPipelineStage("Processing"), new RendererPipelineStage("Processing")]));

        Assert.Throws<ArgumentException>(() =>
            PreparedBucketPipeline.Prepare(
                "Invalid.Anchor",
                DefaultStages,
                contributions:
                [
                    new RendererPipelineStageContribution(
                        new RendererPipelineStage("Particles"),
                        "DoesNotExist")
                ]));
    }

    [Fact]
    public void PreparedStagesAndSettingsCannotBeMutatedThroughPublicCollections()
    {
        var defaults = new Dictionary<string, string> { ["quality"] = "balanced" };
        var prepared = PreparedBucketPipeline.Prepare("Lighting", DefaultStages, defaults);

        defaults["quality"] = "changed";
        Assert.Equal("balanced", prepared.Settings["quality"]);
        Assert.Throws<NotSupportedException>(() =>
            ((IDictionary<string, string>)prepared.Settings).Add("new", "value"));
    }
}
