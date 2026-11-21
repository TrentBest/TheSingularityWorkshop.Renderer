namespace TheSingularityWorkshop.Renderer;

/// <summary>Separately estimated resource costs for one presentation candidate.</summary>
/// <remarks>Values are estimates supplied by the host, not measured GPU execution.</remarks>
public sealed record RepresentationResourceCost
{
    /// <summary>Creates estimated resource costs for one presentation candidate.</summary>
    /// <param name="cpuPreparationMilliseconds">Estimated CPU preparation time in milliseconds.</param>
    /// <param name="gpuExecutionMilliseconds">Estimated GPU execution time in milliseconds.</param>
    /// <param name="residentBytes">Estimated resident memory in bytes.</param>
    /// <param name="transferBytes">Estimated transfer volume in bytes.</param>
    /// <param name="freshnessLatencyMilliseconds">Estimated freshness latency in milliseconds.</param>
    /// <param name="uncertainty">Estimated non-negative host-defined uncertainty.</param>
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

    /// <summary>Estimated CPU preparation time in milliseconds.</summary>
    public double CpuPreparationMilliseconds { get; }
    /// <summary>Estimated GPU execution time in milliseconds; not a measured result.</summary>
    public double GpuExecutionMilliseconds { get; }
    /// <summary>Estimated resident memory in bytes.</summary>
    public long ResidentBytes { get; }
    /// <summary>Estimated transfer volume in bytes.</summary>
    public long TransferBytes { get; }
    /// <summary>Estimated freshness latency in milliseconds.</summary>
    public double FreshnessLatencyMilliseconds { get; }
    /// <summary>Estimated uncertainty, expressed as a non-negative host-defined value.</summary>
    public double Uncertainty { get; }
}

/// <summary>Independent ceilings for one observer's representation selection.</summary>
public sealed record RepresentationResourceBudget
{
    /// <summary>Creates independent ceilings for each resource dimension.</summary>
    /// <param name="cpuPreparationMilliseconds">Maximum CPU preparation time in milliseconds.</param>
    /// <param name="gpuExecutionMilliseconds">Maximum estimated GPU execution time in milliseconds.</param>
    /// <param name="residentBytes">Maximum resident memory in bytes.</param>
    /// <param name="transferBytes">Maximum transfer volume in bytes.</param>
    /// <param name="maximumFreshnessLatencyMilliseconds">Maximum freshness latency in milliseconds.</param>
    /// <param name="maximumUncertainty">Maximum host-defined uncertainty.</param>
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

    /// <summary>Maximum CPU preparation time in milliseconds.</summary>
    public double CpuPreparationMilliseconds { get; }
    /// <summary>Maximum estimated GPU execution time in milliseconds.</summary>
    public double GpuExecutionMilliseconds { get; }
    /// <summary>Maximum resident memory in bytes.</summary>
    public long ResidentBytes { get; }
    /// <summary>Maximum transfer volume in bytes.</summary>
    public long TransferBytes { get; }
    /// <summary>Maximum freshness latency in milliseconds.</summary>
    public double MaximumFreshnessLatencyMilliseconds { get; }
    /// <summary>Maximum host-defined uncertainty.</summary>
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
    /// <summary>Creates a named candidate with its multidimensional fidelity and estimated costs.</summary>
    /// <param name="id">Stable candidate identifier used for deterministic tie-breaking.</param>
    /// <param name="profile">The candidate's independent representation dimensions.</param>
    /// <param name="cost">Host-supplied estimated resource costs.</param>
    public RepresentationCandidate(string id, RepresentationDimensionProfile profile, RepresentationResourceCost cost)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(cost);
        Id = id;
        Profile = profile;
        Cost = cost;
    }

    /// <summary>Stable candidate identifier.</summary>
    public string Id { get; }
    /// <summary>Independent fidelity dimensions supplied by this candidate.</summary>
    public RepresentationDimensionProfile Profile { get; }
    /// <summary>Estimated resource costs supplied by the host.</summary>
    public RepresentationResourceCost Cost { get; }
}

/// <summary>Intent that changes the relative value of visual, semantic, and interaction fidelity.</summary>
public enum RepresentationIntent
{
    /// <summary>Passive observation without a specific task.</summary>
    Observe,
    /// <summary>Find a safe or useful path through the environment.</summary>
    Navigate,
    /// <summary>Inspect a subject or its visible structure.</summary>
    Inspect,
    /// <summary>Act on or manipulate a subject.</summary>
    Interact,
    /// <summary>Respond to a domain-identified hazard or urgent condition.</summary>
    RespondToHazard
}

/// <summary>Observer and source provenance for a deterministic representation decision.</summary>
public sealed record RepresentationObservation
{
    /// <summary>Creates an observer/source observation with normalized presentation signals.</summary>
    /// <param name="observerId">Stable observer identifier.</param>
    /// <param name="observerKind">The observer's role.</param>
    /// <param name="sourceId">Stable authoritative source identifier.</param>
    /// <param name="sourceRevision">Revision of the observed source.</param>
    /// <param name="projectedSize">Projected viewport fraction in [0,1].</param>
    /// <param name="visibility">Visibility confidence in [0,1].</param>
    /// <param name="semanticSalience">Domain-supplied semantic salience in [0,1].</param>
    /// <param name="intent">The observer task that changes the relative value of representation dimensions.</param>
    public RepresentationObservation(string observerId, RepresentationObserverKind observerKind,
        string sourceId, long sourceRevision, double projectedSize, double visibility, double semanticSalience,
        RepresentationIntent intent = RepresentationIntent.Observe)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(observerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceId);
        if (!Enum.IsDefined(observerKind)) throw new ArgumentOutOfRangeException(nameof(observerKind));
        if (!Enum.IsDefined(intent)) throw new ArgumentOutOfRangeException(nameof(intent));
        ArgumentOutOfRangeException.ThrowIfNegative(sourceRevision);
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
        Intent = intent;
    }

    /// <summary>Stable observer identifier.</summary>
    public string ObserverId { get; }
    /// <summary>Observer role.</summary>
    public RepresentationObserverKind ObserverKind { get; }
    /// <summary>Stable authoritative source identifier.</summary>
    public string SourceId { get; }
    /// <summary>Observed authoritative source revision.</summary>
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
/// <summary>Observer classes supported by the initial CPU policy proof.</summary>
public enum RepresentationObserverKind
{
    /// <summary>First-person human observer.</summary>
    FirstPerson,
    /// <summary>Detached or cinematic camera observer.</summary>
    Camera,
    /// <summary>AI or map-oriented observer.</summary>
    AiMap
}

/// <summary>Selection result with stable provenance and explainable fallback reasons.</summary>
public sealed record RepresentationSelectionDecision(
    string ObserverId, RepresentationObserverKind ObserverKind, string SourceId, long SourceRevision,
    RepresentationCandidate? Selected, IReadOnlyList<string> RejectedCandidates, IReadOnlyList<string> ReducedDimensions)
{
    /// <summary>Gets whether a feasible candidate was selected.</summary>
    public bool HasSelection => Selected is not null;

    /// <summary>Feasible candidates and their deterministic observer-weighted scores, highest first.</summary>
    public IReadOnlyList<string> CandidateScores { get; init; } = Array.Empty<string>();

    /// <summary>Whether the current feasible candidate was retained by temporal hysteresis.</summary>
    public bool HysteresisApplied { get; init; }
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
    /// <summary>Selects the highest-ranked feasible candidate deterministically.</summary>
    /// <param name="observation">Observer and authoritative source provenance.</param>
    /// <param name="minimum">Component-wise minimum fidelity that must be preserved.</param>
    /// <param name="budget">Independent resource ceilings.</param>
    /// <param name="candidates">Available representation candidates.</param>
    /// <returns>A decision containing the selected candidate, or an explicit no-selection result.</returns>
    public RepresentationSelectionDecision Select(RepresentationObservation observation,
        RepresentationDimensionProfile minimum, RepresentationResourceBudget budget,
        IReadOnlyList<RepresentationCandidate> candidates) =>
        Select(observation, minimum, budget, candidates, null, 0);

    /// <summary>
    /// Selects a feasible representation while retaining the current feasible candidate until
    /// a challenger exceeds it by the configured quality margin.
    /// </summary>
    /// <param name="observation">Observer and authoritative source provenance.</param>
    /// <param name="minimum">Component-wise minimum fidelity that must be preserved.</param>
    /// <param name="budget">Independent resource ceilings.</param>
    /// <param name="candidates">Available representation candidates.</param>
    /// <param name="previousDecision">Previous decision for the same observer and authoritative source, if any.</param>
    /// <param name="minimumSwitchAdvantage">Required score advantage before changing candidates.</param>
    /// <returns>A deterministic decision; infeasible current candidates never receive hysteresis protection.</returns>
    public RepresentationSelectionDecision Select(RepresentationObservation observation,
        RepresentationDimensionProfile minimum, RepresentationResourceBudget budget,
        IReadOnlyList<RepresentationCandidate> candidates, RepresentationSelectionDecision? previousDecision,
        double minimumSwitchAdvantage)
    {
        if (!double.IsFinite(minimumSwitchAdvantage) || minimumSwitchAdvantage < 0)
            throw new ArgumentOutOfRangeException(nameof(minimumSwitchAdvantage));
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

        var ranked = feasible
            .Select(candidate => (Candidate: candidate, Score: Quality(candidate.Profile, observation)))
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Candidate.Id, StringComparer.Ordinal)
            .ToArray();
        var challenger = ranked.FirstOrDefault();
        var selected = challenger.Candidate;
        var hysteresisApplied = false;

        var previousCandidateId = previousDecision is not null &&
            StringComparer.Ordinal.Equals(previousDecision.ObserverId, observation.ObserverId) &&
            StringComparer.Ordinal.Equals(previousDecision.SourceId, observation.SourceId) &&
            previousDecision.HasSelection
                ? previousDecision.Selected!.Id
                : null;

        if (selected is not null && previousCandidateId is not null &&
            !StringComparer.Ordinal.Equals(selected.Id, previousCandidateId))
        {
            var current = ranked.FirstOrDefault(item =>
                StringComparer.Ordinal.Equals(item.Candidate.Id, previousCandidateId));
            if (current.Candidate is not null &&
                challenger.Score - current.Score <= minimumSwitchAdvantage)
            {
                selected = current.Candidate;
                hysteresisApplied = true;
            }
        }

        var reduced = selected is null
            ? Array.Empty<string>()
            : GetReducedDimensions(selected.Profile);

        return new RepresentationSelectionDecision(observation.ObserverId, observation.ObserverKind,
            observation.SourceId, observation.SourceRevision, selected, rejected.AsReadOnly(), reduced)
        {
            CandidateScores = ranked.Select(item => $"{item.Candidate.Id}: {item.Score:F3}").ToArray(),
            HysteresisApplied = hysteresisApplied
        };
    }

    // Transparent policy weights: viewport footprint values geometry/volume/particles;
    // visibility gates their relevance; semantic salience values semantic and interaction detail.
    // Observer-role weights are explicit rather than inferred from distance or graphics backend.
    private static double Quality(RepresentationDimensionProfile profile, RepresentationObservation observation)
    {
        var visualWeight = observation.ProjectedSize * observation.Visibility;
        var semanticWeight = observation.SemanticSalience;
        var geometryWeight = (observation.ObserverKind == RepresentationObserverKind.Camera ? 1.25 : 1.0) *
            (observation.Intent == RepresentationIntent.Inspect ? 1.5 : 1.0);
        var interactionWeight = (observation.ObserverKind == RepresentationObserverKind.FirstPerson ? 1.5 : 0.75) *
            (observation.Intent switch
            {
                RepresentationIntent.Interact => 4.0,
                RepresentationIntent.Inspect => 1.5,
                _ => 1.0
            });
        var mapSemanticWeight = (observation.ObserverKind == RepresentationObserverKind.AiMap ? 1.5 : 1.0) *
            (observation.Intent switch
            {
                RepresentationIntent.Navigate => 1.25,
                RepresentationIntent.RespondToHazard => 1.75,
                _ => 1.0
            });
        var historyIntentWeight = observation.Intent == RepresentationIntent.RespondToHazard ? 1.5 : 1.0;

        return (double)profile.Geometry * visualWeight * geometryWeight * 100 +
            (double)profile.Volume * visualWeight * 80 +
            (double)profile.Particles * visualWeight * 60 +
            (double)profile.Animation * visualWeight * 30 +
            (double)profile.InteractionPrecision * semanticWeight * interactionWeight * 50 +
            (double)profile.SemanticDetail * semanticWeight * mapSemanticWeight * 100 +
            (double)profile.History * observation.Visibility * historyIntentWeight * 20 +
            (double)profile.Residency * 3 + Math.Min(profile.UpdateCadenceHz, 1000) * visualWeight;
    }

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
