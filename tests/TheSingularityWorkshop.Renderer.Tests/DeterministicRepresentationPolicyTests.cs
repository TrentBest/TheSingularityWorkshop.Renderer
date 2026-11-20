using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class DeterministicRepresentationPolicyTests
{
    private static RepresentationDimensionProfile Profile(RepresentationDetailLevel detail) =>
        new(detail, detail, detail, detail, 30, RepresentationResidency.Resident,
            detail, detail, detail);

    private static RepresentationResourceCost Cost(double cpu, long memory) =>
        new(cpu, cpu / 2, memory, memory / 4, 20, 0.1);

    private static RepresentationResourceBudget Budget() =>
        new(5, 3, 1024, 256, 50, 0.5);

    private static RepresentationObservation Observation(RepresentationObserverKind kind = RepresentationObserverKind.Camera) =>
        new("observer-A", kind, "phenomenon/ash-cloud-1", 42, 0.7, 0.9, 0.8);

    [Fact]
    public void Select_ChoosesRichestCandidateWithinEveryIndependentBudget()
    {
        var candidates = new[]
        {
            new RepresentationCandidate("summary", Profile(RepresentationDetailLevel.Summary), Cost(1, 128)),
            new RepresentationCandidate("full-too-expensive", Profile(RepresentationDetailLevel.Full), Cost(6, 128)),
            new RepresentationCandidate("reduced", Profile(RepresentationDetailLevel.Reduced), Cost(2, 256))
        };

        var decision = new DeterministicRepresentationPolicy().Select(
            Observation(RepresentationObserverKind.FirstPerson), Profile(RepresentationDetailLevel.Summary),
            Budget(), candidates);

        Assert.Equal("reduced", decision.Selected!.Id);
        Assert.Equal("observer-A", decision.ObserverId);
        Assert.Equal("phenomenon/ash-cloud-1", decision.SourceId);
        Assert.Equal(42, decision.SourceRevision);
        Assert.Contains(decision.RejectedCandidates, reason => reason.Contains("CPU preparation budget"));
    }

    [Fact]
    public void Select_UsesOrdinalIdTieBreakForEqualQuality()
    {
        var candidates = new[]
        {
            new RepresentationCandidate("zeta", Profile(RepresentationDetailLevel.Reduced), Cost(1, 128)),
            new RepresentationCandidate("alpha", Profile(RepresentationDetailLevel.Reduced), Cost(1, 128))
        };

        var decision = new DeterministicRepresentationPolicy().Select(
            Observation(RepresentationObserverKind.AiMap), Profile(RepresentationDetailLevel.Summary),
            Budget(), candidates);

        Assert.Equal("alpha", decision.Selected!.Id);
        Assert.Equal(RepresentationObserverKind.AiMap, decision.ObserverKind);
    }

    [Fact]
    public void Select_ReturnsNoSelectionWhenAllCandidatesViolateMinimumOrBudget()
    {
        var candidates = new[]
        {
            new RepresentationCandidate("too-simple", Profile(RepresentationDetailLevel.Summary), Cost(1, 128)),
            new RepresentationCandidate("too-costly", Profile(RepresentationDetailLevel.Full), Cost(1, 4096))
        };

        var decision = new DeterministicRepresentationPolicy().Select(
            Observation(), Profile(RepresentationDetailLevel.Reduced), Budget(), candidates);

        Assert.False(decision.HasSelection);
        Assert.Equal(2, decision.RejectedCandidates.Count);
    }

    [Fact]
    public void Observation_RejectsOutOfRangeSignals()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RepresentationObservation("observer", RepresentationObserverKind.Camera,
                "source", 1, 1.1, 0, 0));
    }
    [Fact]
    public void Select_ProjectedSizeAndVisibilityFavorVisualGeometry()
    {
        var geometry = new RepresentationDimensionProfile(
            RepresentationDetailLevel.Full, RepresentationDetailLevel.Omitted,
            RepresentationDetailLevel.Omitted, RepresentationDetailLevel.Omitted, 0,
            RepresentationResidency.ReconstructOnDemand, RepresentationDetailLevel.Omitted,
            RepresentationDetailLevel.Omitted, RepresentationDetailLevel.Omitted);
        var semantic = new RepresentationDimensionProfile(
            RepresentationDetailLevel.Omitted, RepresentationDetailLevel.Omitted,
            RepresentationDetailLevel.Omitted, RepresentationDetailLevel.Omitted, 0,
            RepresentationResidency.ReconstructOnDemand, RepresentationDetailLevel.Omitted,
            RepresentationDetailLevel.Full, RepresentationDetailLevel.Omitted);
        var candidates = new[]
        {
            new RepresentationCandidate("geometry", geometry, Cost(1, 128)),
            new RepresentationCandidate("semantic", semantic, Cost(1, 128))
        };
        var minimum = new RepresentationDimensionProfile(
            RepresentationDetailLevel.Omitted, RepresentationDetailLevel.Omitted,
            RepresentationDetailLevel.Omitted, RepresentationDetailLevel.Omitted, 0,
            RepresentationResidency.ReconstructOnDemand, RepresentationDetailLevel.Omitted,
            RepresentationDetailLevel.Omitted, RepresentationDetailLevel.Omitted);
        var observation = new RepresentationObservation("camera", RepresentationObserverKind.Camera,
            "source", 7, 1, 1, 0.1);

        var decision = new DeterministicRepresentationPolicy().Select(observation, minimum, Budget(), candidates);

        Assert.Equal("geometry", decision.Selected!.Id);
        Assert.Equal(2, decision.CandidateScores.Count);
    }

    [Fact]
    public void Select_SemanticSalienceFavorsSemanticDetailForAiMapObserver()
    {
        var geometry = new RepresentationDimensionProfile(
            RepresentationDetailLevel.Full, RepresentationDetailLevel.Omitted,
            RepresentationDetailLevel.Omitted, RepresentationDetailLevel.Omitted, 0,
            RepresentationResidency.ReconstructOnDemand, RepresentationDetailLevel.Omitted,
            RepresentationDetailLevel.Omitted, RepresentationDetailLevel.Omitted);
        var semantic = new RepresentationDimensionProfile(
            RepresentationDetailLevel.Omitted, RepresentationDetailLevel.Omitted,
            RepresentationDetailLevel.Omitted, RepresentationDetailLevel.Omitted, 0,
            RepresentationResidency.ReconstructOnDemand, RepresentationDetailLevel.Omitted,
            RepresentationDetailLevel.Full, RepresentationDetailLevel.Omitted);
        var candidates = new[]
        {
            new RepresentationCandidate("geometry", geometry, Cost(1, 128)),
            new RepresentationCandidate("semantic", semantic, Cost(1, 128))
        };
        var minimum = new RepresentationDimensionProfile(
            RepresentationDetailLevel.Omitted, RepresentationDetailLevel.Omitted,
            RepresentationDetailLevel.Omitted, RepresentationDetailLevel.Omitted, 0,
            RepresentationResidency.ReconstructOnDemand, RepresentationDetailLevel.Omitted,
            RepresentationDetailLevel.Omitted, RepresentationDetailLevel.Omitted);
        var observation = new RepresentationObservation("map", RepresentationObserverKind.AiMap,
            "hazard", 9, 0.1, 1, 1);

        var decision = new DeterministicRepresentationPolicy().Select(observation, minimum, Budget(), candidates);

        Assert.Equal("semantic", decision.Selected!.Id);
        Assert.Contains("semantic:", decision.CandidateScores[0]);
    }

    [Fact]
    public void Select_HysteresisRetainsCurrentFeasibleCandidateWithinSwitchMargin()
    {
        var candidates = new[]
        {
            new RepresentationCandidate("a-challenger", Profile(RepresentationDetailLevel.Reduced), Cost(1, 128)),
            new RepresentationCandidate("z-current", Profile(RepresentationDetailLevel.Reduced), Cost(1, 128))
        };
        var policy = new DeterministicRepresentationPolicy();

        var first = policy.Select(Observation(), Profile(RepresentationDetailLevel.Summary),
            Budget(), candidates, "z-current", 0);
        var replay = policy.Select(Observation(), Profile(RepresentationDetailLevel.Summary),
            Budget(), candidates, "z-current", 0);

        Assert.Equal("z-current", first.Selected!.Id);
        Assert.True(first.HysteresisApplied);
        Assert.Equal(first.Selected.Id, replay.Selected!.Id);
        Assert.Equal(first.CandidateScores, replay.CandidateScores);
    }

    [Fact]
    public void Select_HysteresisCannotPreserveCandidateBelowHardMinimum()
    {
        var candidates = new[]
        {
            new RepresentationCandidate("current", Profile(RepresentationDetailLevel.Summary), Cost(1, 128)),
            new RepresentationCandidate("required", Profile(RepresentationDetailLevel.Reduced), Cost(1, 128))
        };

        var decision = new DeterministicRepresentationPolicy().Select(
            Observation(), Profile(RepresentationDetailLevel.Reduced), Budget(),
            candidates, "current", double.MaxValue);

        Assert.Equal("required", decision.Selected!.Id);
        Assert.False(decision.HysteresisApplied);
        Assert.Contains(decision.RejectedCandidates, reason => reason.StartsWith("current:"));
    }

    [Fact]
    public void Select_HysteresisCannotPreserveCandidateOverBudget()
    {
        var candidates = new[]
        {
            new RepresentationCandidate("current", Profile(RepresentationDetailLevel.Full), Cost(1, 4096)),
            new RepresentationCandidate("fallback", Profile(RepresentationDetailLevel.Reduced), Cost(1, 128))
        };

        var decision = new DeterministicRepresentationPolicy().Select(
            Observation(), Profile(RepresentationDetailLevel.Summary), Budget(),
            candidates, "current", double.MaxValue);

        Assert.Equal("fallback", decision.Selected!.Id);
        Assert.False(decision.HysteresisApplied);
        Assert.Contains(decision.RejectedCandidates, reason => reason.Contains("resident memory budget"));
    }

    [Fact]
    public void Select_RejectsInvalidHysteresisMargin()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DeterministicRepresentationPolicy().Select(
                Observation(), Profile(RepresentationDetailLevel.Summary), Budget(),
                Array.Empty<RepresentationCandidate>(), null, double.NaN));
    }

}
