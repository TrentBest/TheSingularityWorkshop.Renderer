namespace TheSingularityWorkshop.Renderer;

/// <summary>Separately estimated resource costs for one presentation candidate.</summary>
/// <remarks>Values are estimates supplied by the host, not measured GPU execution.</remarks>
public sealed record RepresentationResourceCost
{
    public RepresentationResourceCost(double cpuPreparationMilliseconds, double gpuExecutionMilliseconds,
        long residentBytes, long transferBytes, double freshnessLatencyMilliseconds, double uncertainty)
    {
        if (!double.IsFinite(cpuPreparationMilliseconds) || cpuPreparationMilliseconds < 0)
            throw new ArgumentOutOfRangeException(nameof(cpuPreparationMilliseconds));
        if (!double.IsFinite(gpuExecutionMilliseconds) || gpuExecutionMilliseconds < 0)
            throw new ArgumentOutOfRangeException(nameof(gpuExecutionMilliseconds));
        if (residentBytes < 0) throw new ArgumentOutOfRangeException(nameof(residentBytes));
        if (transferBytes < 0) throw new ArgumentOutOfRangeException(nameof(transferBytes));
        if (!double.IsFinite(freshnessLatencyMilliseconds) || freshnessLatencyMilliseconds < 0)
            throw new ArgumentOutOfRangeException(nameof(freshnessLatencyMilliseconds));
        if (!double.IsFinite(uncertainty) || uncertainty < 0)
            throw new ArgumentOutOfRangeException(nameof(uncertainty));

        CpuPreparationMilliseconds = cpuPreparationMilliseconds;
        GpuExecutionMilliseconds = gpuExecutionMilliseconds;
        ResidentBytes = residentBytes;
        TransferBytes = transferBytes;
        FreshnessLatencyMilliseconds = freshnessLatencyMilliseconds;
        Uncertainty = uncertainty;
    }

    public double CpuPreparationMilliseconds { get; }
    public double GpuExecutionMilliseconds { get; }
    public long ResidentBytes { get; }
    public long TransferBytes { get; }
    public double FreshnessLatencyMilliseconds { get; }
    public double Uncertainty { get; }
}

/// <summary>Independent ceilings for one observer's representation selection.</summary>
public sealed record RepresentationResourceBudget
{
    public RepresentationResourceBudget(double cpuPreparationMilliseconds, double gpuExecutionMilliseconds,
        long residentBytes, long transferBytes, double maximumFreshnessLatencyMilliseconds, double maximumUncertainty)
    {
        if (!double.IsFinite(cpuPreparationMilliseconds) || cpuPreparationMilliseconds < 0)
            throw new ArgumentOutOfRangeException(nameof(cpuPreparationMilliseconds));
        if (!double.IsFinite(gpuExecutionMilliseconds) || gpuExecutionMilliseconds < 0)
            throw new ArgumentOutOfRangeException(nameof(gpuExecutionMilliseconds));
        if (residentBytes < 0) throw new ArgumentOutOfRangeException(nameof(residentBytes));
        if (transferBytes < 0) throw new ArgumentOutOfRangeException(nameof(transferBytes));
        if (!double.IsFinite(maximumFreshnessLatencyMilliseconds) || maximumFreshnessLatencyMilliseconds < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumFreshnessLatencyMilliseconds));
        if (!double.IsFinite(maximumUncertainty) || maximumUncertainty < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumUncertainty));

        CpuPreparationMilliseconds = cpuPreparationMilliseconds;
        GpuExecutionMilliseconds = gpuExecutionMilliseconds;
        ResidentBytes = residentBytes;
        TransferBytes = transferBytes;
        MaximumFreshnessLatencyMilliseconds = maximumFreshnessLatencyMilliseconds;
        MaximumUncertainty = maximumUncertainty;
    }

    public double CpuPreparationMilliseconds { get; }
    public double GpuExecutionMilliseconds { get; }
    public long ResidentBytes { get; }
    public long TransferBytes { get; }
    public double MaximumFreshnessLatencyMilliseconds { get; }
    public double MaximumUncertainty { get; }

    internal IReadOnlyList<string> GetExceededLimits(RepresentationResourceCost cost)
    {
        var reasons = new List<string>();
        if (cost.CpuPreparationMilliseconds > CpuPreparationMilliseconds) reasons.Add("CPU preparation budget");
        if (cost.GpuExecutionMilliseconds > GpuExecutionMilliseconds) reasons.Add("GPU execution estimate budget");
        if (cost.ResidentBytes > ResidentBytes) reasons.Add("resident memory budget");
        if (cost.TransferBytes > TransferBytes) reasons.Add("transfer budget");
        if (cost.FreshnessLatencyMilliseconds > MaximumFreshnessLatencyMilliseconds) reasons.Add("freshness latency limit");
        if (cost.Uncertainty > MaximumUncertainty) reasons.Add("uncertainty limit");
        return reasons;
    }
}

/// <summary>A named, backend-neutral candidate for representing one authoritative source.</summary>
public sealed record RepresentationCandidate
{
    public RepresentationCandidate(string id, RepresentationDimensionProfile profile, RepresentationResourceCost cost)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(cost);
        Id = id;
        Profile = profile;
        Cost = cost;
    }

    public string Id { get; }
    public RepresentationDimensionProfile Profile { get; }
    public RepresentationResourceCost Cost { get; }
}

/// <summary>Observer and source provenance for a deterministic representation decision.</summary>
public sealed record RepresentationObservation
{
    public RepresentationObservation(string observerId, RepresentationObserverKind observerKind,
        string sourceId, long sourceRevision, double projectedSize, double visibility, double semanticSalience)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(observerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceId);
        if (!Enum.IsDefined(observerKind)) throw new ArgumentOutOfRangeException(nameof(observerKind));
        ValidateUnit(projectedSize, nameof(projectedSize));
        ValidateUnit(visibility, nameof(visibility));
        ValidateUnit(semanticSalience, nameof(semanticSalience));
        ObserverId = observerId;
        ObserverKind = observerKind;
        SourceId = sourceId;
        SourceRevision = sourceRevision;
        ProjectedSize = projectedSize;
        Visibility = visibility;
        SemanticSalience = semanticSalience;
    }

    public string ObserverId { get; }
    public RepresentationObserverKind ObserverKind { get; }
    public string SourceId { get; }
    public long SourceRevision { get; }
    /// <summary>Projected fraction of the observer viewport, in [0,1].</summary>
    public double ProjectedSize { get; }
    /// <summary>Visibility confidence, in [0,1].</summary>
    public double Visibility { get; }
    /// <summary>Domain-supplied semantic salience, in [0,1].</summary>
    public double SemanticSalience { get; }

    private static void ValidateUnit(double value, string name)
    {
        if (!double.IsFinite(value) || value < 0 || value > 1)
            throw new ArgumentOutOfRangeException(name);
    }
}

/// <summary>Observer classes supported by the initial CPU policy proof.</summary>
public enum RepresentationObserverKind { FirstPerson, Camera, AiMap }

/// <summary>Selection result with stable provenance and explainable fallback reasons.</summary>
public sealed record RepresentationSelectionDecision(
    string ObserverId, RepresentationObserverKind ObserverKind, string SourceId, long SourceRevision,
    RepresentationCandidate? Selected, IReadOnlyList<string> RejectedCandidates, IReadOnlyList<string> ReducedDimensions)
{
    public bool HasSelection => Selected is not null;
}

/// <summary>
/// Deterministically selects the richest candidate that satisfies independent resource ceilings
/// and a component-wise minimum fidelity contract.
/// </summary>
/// <remarks>
/// Candidate costs and semantic inputs are host/domain supplied estimates. This policy does not
/// update authoritative sources, schedule work, implement graphics, or claim measured GPU execution.
/// Stable ordinal ID ordering resolves equal-quality candidates.
/// </remarks>
public sealed class DeterministicRepresentationPolicy
{
    public RepresentationSelectionDecision Select(RepresentationObservation observation,
        RepresentationDimensionProfile minimum, RepresentationResourceBudget budget,
        IReadOnlyList<RepresentationCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(observation);
        ArgumentNullException.ThrowIfNull(minimum);
        ArgumentNullException.ThrowIfNull(budget);
        ArgumentNullException.ThrowIfNull(candidates);

        var rejected = new List<string>();
        var feasible = new List<RepresentationCandidate>();
        foreach (var candidate in candidates)
        {
            ArgumentNullException.ThrowIfNull(candidate);
            if (!candidate.Profile.MeetsOrExceeds(minimum))
            {
                rejected.Add($"{candidate.Id}: below required fidelity");
                continue;
            }

            var exceeded = budget.GetExceededLimits(candidate.Cost);
            if (exceeded.Count != 0)
            {
                rejected.Add($"{candidate.Id}: exceeds {string.Join(", ", exceeded)}");
                continue;
            }

            feasible.Add(candidate);
        }

        var selected = feasible
            .OrderByDescending(candidate => Quality(candidate.Profile))
            .ThenBy(candidate => candidate.Id, StringComparer.Ordinal)
            .FirstOrDefault();

        var reduced = selected is null
            ? Array.Empty<string>()
            : GetReducedDimensions(selected.Profile);

        return new RepresentationSelectionDecision(observation.ObserverId, observation.ObserverKind,
            observation.SourceId, observation.SourceRevision, selected, rejected.AsReadOnly(), reduced);
    }

    private static long Quality(RepresentationDimensionProfile profile) =>
        (long)profile.Geometry + (long)profile.Volume + (long)profile.Particles +
        (long)profile.Animation + (long)profile.InteractionPrecision +
        (long)profile.SemanticDetail + (long)profile.History +
        (long)profile.Residency * 3 + (long)Math.Min(profile.UpdateCadenceHz, 1000);

    private static string[] GetReducedDimensions(RepresentationDimensionProfile profile)
    {
        var reduced = new List<string>();
        if (profile.Geometry != RepresentationDetailLevel.Full) reduced.Add(nameof(profile.Geometry));
        if (profile.Volume != RepresentationDetailLevel.Full) reduced.Add(nameof(profile.Volume));
        if (profile.Particles != RepresentationDetailLevel.Full) reduced.Add(nameof(profile.Particles));
        if (profile.Animation != RepresentationDetailLevel.Full) reduced.Add(nameof(profile.Animation));
        if (profile.InteractionPrecision != RepresentationDetailLevel.Full) reduced.Add(nameof(profile.InteractionPrecision));
        if (profile.SemanticDetail != RepresentationDetailLevel.Full) reduced.Add(nameof(profile.SemanticDetail));
        if (profile.History != RepresentationDetailLevel.Full) reduced.Add(nameof(profile.History));
        if (profile.Residency != RepresentationResidency.Resident) reduced.Add(nameof(profile.Residency));
        return reduced.ToArray();
    }
}
