using System.Collections.ObjectModel;

namespace TheSingularityWorkshop.Renderer;

/// <summary>
/// Describes one meaningful unit of work in a bucket-local processing pipeline.
/// </summary>
/// <param name="Id">Stable, case-sensitive stage identity.</param>
/// <param name="RequiredStageIds">Stages that must appear earlier in the prepared sequence.</param>
public sealed record RendererPipelineStage
{
    public RendererPipelineStage(string id, IEnumerable<string>? requiredStageIds = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("A pipeline stage id is required.", nameof(id));

        Id = id;
        var requirements = (requiredStageIds ?? Array.Empty<string>()).ToArray();
        if (requirements.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Required stage ids cannot be empty.", nameof(requiredStageIds));
        if (requirements.Distinct(StringComparer.Ordinal).Count() != requirements.Length)
            throw new ArgumentException("A stage cannot require the same stage more than once.", nameof(requiredStageIds));

        RequiredStageIds = Array.AsReadOnly(requirements);
    }

    public string Id { get; }

    public IReadOnlyList<string> RequiredStageIds { get; }
}

/// <summary>
/// A forge-time contribution that inserts a stage immediately before an existing stage.
/// Contributions are applied in the order supplied by the composition host.
/// </summary>
public sealed record RendererPipelineStageContribution
{
    public RendererPipelineStageContribution(RendererPipelineStage stage, string insertBeforeStageId)
    {
        ArgumentNullException.ThrowIfNull(stage);
        if (string.IsNullOrWhiteSpace(insertBeforeStageId))
            throw new ArgumentException("An insertion anchor is required.", nameof(insertBeforeStageId));

        Stage = stage;
        InsertBeforeStageId = insertBeforeStageId;
    }

    public RendererPipelineStage Stage { get; }

    public string InsertBeforeStageId { get; }
}

/// <summary>
/// Immutable, forge-prepared stage order and resolved configuration for one bucket pipeline.
/// </summary>
/// <remarks>
/// This type performs deterministic preparation only. It does not arbitrate MicroBundles,
/// execute stages, schedule frames, or depend on a presentation backend. The composition host
/// supplies the already-selected base stages and ordered contributions. Runtime consumes the
/// resulting stage sequence and resolved settings without repeating this preparation.
/// </remarks>
public sealed class PreparedBucketPipeline
{
    private PreparedBucketPipeline(
        string name,
        IReadOnlyList<RendererPipelineStage> stages,
        IReadOnlyDictionary<string, string> settings)
    {
        Name = name;
        Stages = stages;
        Settings = settings;
    }

    public string Name { get; }

    public IReadOnlyList<RendererPipelineStage> Stages { get; }

    /// <summary>Gets the merged bundle defaults and explicit experience overrides.</summary>
    public IReadOnlyDictionary<string, string> Settings { get; }

    /// <summary>
    /// Prepares a pipeline from bundle-provided defaults and optional experience overrides.
    /// Missing overrides inherit defaults; an empty override set is valid.
    /// </summary>
    public static PreparedBucketPipeline Prepare(
        string name,
        IEnumerable<RendererPipelineStage> defaultStages,
        IReadOnlyDictionary<string, string>? bundleDefaults = null,
        IReadOnlyDictionary<string, string>? experienceOverrides = null,
        IEnumerable<RendererPipelineStageContribution>? contributions = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A pipeline name is required.", nameof(name));
        ArgumentNullException.ThrowIfNull(defaultStages);

        var stages = defaultStages.ToList();
        if (stages.Any(stage => stage is null))
            throw new ArgumentException("Pipeline stages cannot contain null entries.", nameof(defaultStages));

        foreach (var contribution in contributions ?? Array.Empty<RendererPipelineStageContribution>())
        {
            if (contribution is null)
                throw new ArgumentException("Pipeline contributions cannot contain null entries.", nameof(contributions));

            var anchorIndex = stages.FindIndex(stage =>
                string.Equals(stage.Id, contribution.InsertBeforeStageId, StringComparison.Ordinal));
            if (anchorIndex < 0)
                throw new ArgumentException(
                    $"Insertion anchor '{contribution.InsertBeforeStageId}' does not exist.",
                    nameof(contributions));

            stages.Insert(anchorIndex, contribution.Stage);
        }

        ValidateStages(stages);

        var resolved = new Dictionary<string, string>(StringComparer.Ordinal);
        CopySettings(bundleDefaults, resolved, nameof(bundleDefaults));
        CopySettings(experienceOverrides, resolved, nameof(experienceOverrides));

        return new PreparedBucketPipeline(
            name,
            Array.AsReadOnly(stages.ToArray()),
            new ReadOnlyDictionary<string, string>(resolved));
    }

    private static void CopySettings(
        IReadOnlyDictionary<string, string>? source,
        IDictionary<string, string> destination,
        string parameterName)
    {
        if (source is null)
            return;

        foreach (var pair in source)
        {
            if (string.IsNullOrWhiteSpace(pair.Key))
                throw new ArgumentException("Configuration keys cannot be empty.", parameterName);
            if (pair.Value is null)
                throw new ArgumentException($"Configuration value '{pair.Key}' cannot be null.", parameterName);

            destination[pair.Key] = pair.Value;
        }
    }

    private static void ValidateStages(IReadOnlyList<RendererPipelineStage> stages)
    {
        var positions = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var index = 0; index < stages.Count; index++)
        {
            var stage = stages[index];
            if (!positions.TryAdd(stage.Id, index))
                throw new ArgumentException($"Duplicate pipeline stage id '{stage.Id}'.", nameof(stages));
        }

        for (var index = 0; index < stages.Count; index++)
        {
            foreach (var requiredId in stages[index].RequiredStageIds)
            {
                if (!positions.TryGetValue(requiredId, out var requiredIndex))
                    throw new ArgumentException(
                        $"Stage '{stages[index].Id}' requires missing stage '{requiredId}'.",
                        nameof(stages));
                if (requiredIndex >= index)
                    throw new ArgumentException(
                        $"Required stage '{requiredId}' must appear before '{stages[index].Id}'.",
                        nameof(stages));
            }
        }
    }
}
