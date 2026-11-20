namespace TheSingularityWorkshop.Renderer;

/// <summary>
/// Describes one visible contribution supplied by an authoritative source.
/// </summary>
/// <remarks>
/// This is a presentation-side aggregation input. Renderer does not own or modify the source state.
/// A source may appear only once in a snapshot; its revision identifies the exact contribution used.
/// </remarks>
public readonly record struct RenderContribution
{
    /// <summary>Creates a contribution eligible for provenance-preserving visual aggregation.</summary>
    public RenderContribution(
        string sourceId,
        long sourceRevision,
        string aggregationKey,
        double estimatedSourceError = 0,
        bool isHazard = false,
        bool requiresInteraction = false)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            throw new ArgumentException("A stable authoritative source ID is required.", nameof(sourceId));
        if (sourceRevision < 0)
            throw new ArgumentOutOfRangeException(nameof(sourceRevision));
        if (string.IsNullOrWhiteSpace(aggregationKey))
            throw new ArgumentException("An aggregation compatibility key is required.", nameof(aggregationKey));
        if (!double.IsFinite(estimatedSourceError) || estimatedSourceError < 0)
            throw new ArgumentOutOfRangeException(nameof(estimatedSourceError));

        SourceId = sourceId;
        SourceRevision = sourceRevision;
        AggregationKey = aggregationKey;
        EstimatedSourceError = estimatedSourceError;
        IsHazard = isHazard;
        RequiresInteraction = requiresInteraction;
    }

    /// <summary>Gets the stable identity assigned by the authoritative source owner.</summary>
    public string SourceId { get; }
    /// <summary>Gets the source revision represented by this contribution.</summary>
    public long SourceRevision { get; }
    /// <summary>Gets the caller-defined key whose members are semantically safe to combine for presentation.</summary>
    public string AggregationKey { get; }
    /// <summary>Gets the source representation's declared error before aggregation.</summary>
    public double EstimatedSourceError { get; }
    /// <summary>Gets whether this contribution carries hazard information that must remain individually visible.</summary>
    public bool IsHazard { get; }
    /// <summary>Gets whether this contribution requires individual interaction precision.</summary>
    public bool RequiresInteraction { get; }
}

/// <summary>Identifies one authoritative source revision retained by an aggregate.</summary>
/// <param name="SourceId">Stable source identity.</param>
/// <param name="SourceRevision">Exact source revision represented.</param>
public readonly record struct RenderContributionProvenance(string SourceId, long SourceRevision);

/// <summary>A presentation aggregate with a complete, deterministic list of source provenance.</summary>
public sealed class RenderContributionAggregate
{
    internal RenderContributionAggregate(
        string aggregationKey,
        IReadOnlyList<RenderContributionProvenance> sources,
        double estimatedSourceError,
        double aggregationError)
    {
        AggregationKey = aggregationKey;
        Sources = sources;
        EstimatedSourceError = estimatedSourceError;
        AggregationError = aggregationError;
    }

    /// <summary>Gets the compatibility key used to form this presentation aggregate.</summary>
    public string AggregationKey { get; }
    /// <summary>Gets every authoritative source and exact revision represented by this result.</summary>
    public IReadOnlyList<RenderContributionProvenance> Sources { get; }
    /// <summary>Gets the sum of declared source representation errors.</summary>
    public double EstimatedSourceError { get; }
    /// <summary>Gets the additional conservative error allowance introduced by aggregation.</summary>
    public double AggregationError { get; }
    /// <summary>Gets the conservative total error estimate for this presentation result.</summary>
    public double EstimatedTotalError => EstimatedSourceError + AggregationError;
    /// <summary>Gets whether multiple authoritative sources are represented together.</summary>
    public bool IsAggregated => Sources.Count > 1;
}

/// <summary>Counts contributions and the number of actual multi-source aggregates produced.</summary>
/// <param name="InputContributions">Contributions supplied to the aggregation pass.</param>
/// <param name="OutputAggregates">Presentation results, including preserved single-source results.</param>
/// <param name="AggregatedSources">Sources contained in multi-source aggregates.</param>
/// <param name="PreservedIndividually">Sources kept as individual results.</param>
public readonly record struct RenderContributionAggregationMetrics(
    int InputContributions,
    int OutputAggregates,
    int AggregatedSources,
    int PreservedIndividually);

/// <summary>The immutable result and accounting from a provenance-preserving aggregation pass.</summary>
public sealed class RenderContributionAggregationResult
{
    internal RenderContributionAggregationResult(
        IReadOnlyList<RenderContributionAggregate> aggregates,
        RenderContributionAggregationMetrics metrics)
    {
        Aggregates = aggregates;
        Metrics = metrics;
    }

    /// <summary>Gets deterministic presentation aggregates in compatibility-key/source-ID order.</summary>
    public IReadOnlyList<RenderContributionAggregate> Aggregates { get; }
    /// <summary>Gets contribution and aggregation counts.</summary>
    public RenderContributionAggregationMetrics Metrics { get; }
}

/// <summary>
/// Groups compatible visible contributions for presentation without merging authoritative identity,
/// source revisions, hazard information, or interaction-critical contributions.
/// </summary>
/// <remarks>
/// Compatibility is deliberately supplied by the caller through an aggregation key. The key must
/// encode all domain-specific constraints (for example region, units, phenomenon, and representation
/// semantics). Renderer does not infer physical compatibility. Results retain every source ID and
/// revision so a consumer can promote back to individual contributions. This is a CPU-side policy
/// operation; it does not combine geometry, fields, pixels, or GPU resources.
/// </remarks>
public static class RenderContributionAggregator
{
    /// <summary>Creates deterministic, error-bounded presentation groups.</summary>
    /// <param name="contributions">One current contribution per authoritative source.</param>
    /// <param name="maximumAggregateError">Maximum permitted total error for a multi-source aggregate.</param>
    /// <param name="additionalSourceError">Conservative error allowance added for each source after the first.</param>
    /// <param name="minimumSources">Minimum source count required to create an aggregate; values below two are invalid.</param>
    public static RenderContributionAggregationResult Aggregate(
        IEnumerable<RenderContribution> contributions,
        double maximumAggregateError,
        double additionalSourceError,
        int minimumSources = 2)
    {
        ArgumentNullException.ThrowIfNull(contributions);
        if (!double.IsFinite(maximumAggregateError) || maximumAggregateError < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumAggregateError));
        if (!double.IsFinite(additionalSourceError) || additionalSourceError < 0)
            throw new ArgumentOutOfRangeException(nameof(additionalSourceError));
        if (minimumSources < 2)
            throw new ArgumentOutOfRangeException(nameof(minimumSources));

        var input = contributions.ToArray();
        var duplicateSource = input
            .GroupBy(item => item.SourceId, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateSource is not null)
            throw new ArgumentException(
                $"Source '{duplicateSource.Key}' appears more than once in the snapshot.",
                nameof(contributions));

        var output = new List<RenderContributionAggregate>();
        foreach (var compatibilityGroup in input
                     .OrderBy(item => item.AggregationKey, StringComparer.Ordinal)
                     .ThenBy(item => item.SourceId, StringComparer.Ordinal)
                     .GroupBy(item => item.AggregationKey, StringComparer.Ordinal))
        {
            var eligible = compatibilityGroup
                .Where(item => !item.IsHazard && !item.RequiresInteraction)
                .ToArray();
            var protectedContributions = compatibilityGroup
                .Where(item => item.IsHazard || item.RequiresInteraction)
                .ToArray();

            foreach (var item in protectedContributions)
                output.Add(CreateSingle(item));

            var current = new List<RenderContribution>();
            var sourceError = 0d;
            foreach (var item in eligible)
            {
                var nextSourceError = sourceError + item.EstimatedSourceError;
                var nextCount = current.Count + 1;
                var nextAggregationError = additionalSourceError * (nextCount - 1);
                var nextTotalError = nextSourceError + nextAggregationError;

                if (current.Count > 0 && nextTotalError > maximumAggregateError)
                {
                    FlushOrPreserveIndividually(current, sourceError, additionalSourceError, minimumSources, output);
                    current.Clear();
                    sourceError = 0;
                    nextSourceError = item.EstimatedSourceError;
                    nextCount = 1;
                    nextAggregationError = 0;
                    nextTotalError = nextSourceError;
                }

                if (nextCount == 1 && nextTotalError > maximumAggregateError)
                {
                    output.Add(CreateSingle(item));
                }
                else
                {
                    current.Add(item);
                    sourceError = nextSourceError;
                }
            }

            if (current.Count > 0)
                FlushOrPreserveIndividually(current, sourceError, additionalSourceError, minimumSources, output);
        }

        output = output
            .OrderBy(item => item.AggregationKey, StringComparer.Ordinal)
            .ThenBy(item => item.Sources[0].SourceId, StringComparer.Ordinal)
            .ToList();

        var aggregatedSources = output
            .Where(item => item.IsAggregated)
            .Sum(item => item.Sources.Count);
        var metrics = new RenderContributionAggregationMetrics(
            input.Length,
            output.Count,
            aggregatedSources,
            input.Length - aggregatedSources);

        return new RenderContributionAggregationResult(output.AsReadOnly(), metrics);
    }

    private static RenderContributionAggregate CreateSingle(RenderContribution contribution) =>
        new(
            contribution.AggregationKey,
            Array.AsReadOnly(new[] { new RenderContributionProvenance(contribution.SourceId, contribution.SourceRevision) }),
            contribution.EstimatedSourceError,
            0);

    private static void FlushOrPreserveIndividually(
        IReadOnlyList<RenderContribution> contributions,
        double sourceError,
        double additionalSourceError,
        int minimumSources,
        ICollection<RenderContributionAggregate> output)
    {
        if (contributions.Count < minimumSources)
        {
            foreach (var contribution in contributions)
                output.Add(CreateSingle(contribution));
            return;
        }

        Flush(contributions, sourceError, additionalSourceError, output);
    }

    private static void Flush(
        IReadOnlyList<RenderContribution> contributions,
        double sourceError,
        double additionalSourceError,
        ICollection<RenderContributionAggregate> output)
    {
        var aggregationError = additionalSourceError * (contributions.Count - 1);
        var sources = contributions
            .Select(item => new RenderContributionProvenance(item.SourceId, item.SourceRevision))
            .ToArray();
        output.Add(new RenderContributionAggregate(
            contributions[0].AggregationKey,
            Array.AsReadOnly(sources),
            sourceError,
            aggregationError));
    }
}
