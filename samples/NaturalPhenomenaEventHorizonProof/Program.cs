using System.Security;
using System.Text;
using System.Text.Json;
using TheSingularityWorkshop.Renderer;

var outputDirectory = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath("artifacts/natural-phenomena-event-horizon-proof");
Directory.CreateDirectory(outputDirectory);

const string sourceId = "volcano/ash-cloud/regional";
const long sourceRevision = 2;
var candidates = CreateCandidates();
var minimum = new RepresentationDimensionProfile(
    RepresentationDetailLevel.Summary,
    RepresentationDetailLevel.Summary,
    RepresentationDetailLevel.Omitted,
    RepresentationDetailLevel.Omitted,
    1,
    RepresentationResidency.ReconstructOnDemand,
    RepresentationDetailLevel.Omitted,
    RepresentationDetailLevel.Summary,
    RepresentationDetailLevel.Summary);
var policy = new DeterministicRepresentationPolicy();

var observers = new[]
{
    new ObserverBudget(
        new RepresentationObservation("first-person", RepresentationObserverKind.FirstPerson, sourceId, sourceRevision, 0.9, 0.95, 0.9),
        new RepresentationResourceBudget(4, 5, 8 * 1024 * 1024, 4 * 1024 * 1024, 50, 0.25)),
    new ObserverBudget(
        new RepresentationObservation("distant-camera", RepresentationObserverKind.Camera, sourceId, sourceRevision, 0.2, 0.8, 0.2),
        new RepresentationResourceBudget(1.5, 1, 1024 * 1024, 512 * 1024, 200, 0.3)),
    new ObserverBudget(
        new RepresentationObservation("ai-map", RepresentationObserverKind.AiMap, sourceId, sourceRevision, 0.1, 0.7, 0.95),
        new RepresentationResourceBudget(0.5, 0.2, 128 * 1024, 32 * 1024, 250, 0.5))
};

var decisions = observers.Select(observer =>
    policy.Select(observer.Observation, minimum, observer.Budget, candidates)).ToArray();

Require(decisions[0].Selected?.Id == "FullVisual", "First-person observer should retain full visual fidelity.");
Require(decisions[1].Selected?.Id == "RegionalVolume", "Distant camera should select a reduced regional volume.");
Require(decisions[2].Selected?.Id == "HazardMarker", "AI/map observer should preserve semantic hazard detail under the tight visual budget.");
Require(decisions.All(x => x.SourceId == sourceId && x.SourceRevision == sourceRevision),
    "Every observer decision must retain the same authoritative source identity and revision.");

var replay = policy.Select(observers[2].Observation, minimum, observers[2].Budget, candidates);
Require(replay.Selected?.Id == decisions[2].Selected?.Id &&
        replay.CandidateScores.SequenceEqual(decisions[2].CandidateScores),
    "Identical AI/map inputs must produce an identical selection and score trace.");

var contributions = new[]
{
    new RenderContribution("ash-cell-north", 2, "ash:regional-density:kg-m3", 0.005),
    new RenderContribution("ash-cell-south", 2, "ash:regional-density:kg-m3", 0.005),
    new RenderContribution("ash-cell-east", 2, "ash:regional-density:kg-m3", 0.005),
    new RenderContribution("hazard-plume-north", 2, "ash:regional-density:kg-m3", 0.005, isHazard: true),
    new RenderContribution("rescue-corridor-01", 2, "ash:regional-density:kg-m3", 0.005, requiresInteraction: true)
};
var aggregation = RenderContributionAggregator.Aggregate(
    contributions, maximumAggregateError: 0.04, additionalSourceError: 0.005);
Require(aggregation.Aggregates.Count(x => x.IsAggregated) == 1,
    "Compatible ordinary ash cells should form one aggregate.");
Require(aggregation.Aggregates.Count(x => !x.IsAggregated) == 2,
    "Hazard and interaction-critical sources must remain individual.");
Require(aggregation.Aggregates.SelectMany(x => x.Sources).Count() == contributions.Length,
    "Aggregation must preserve every source.");

var dependencyTrace = RunDependencyProof();
var sourceLifecycle = new[]
{
    new SourceLifecycle("lightning-event/transient", 1, "Transient event", "Lightning flash"),
    new SourceLifecycle(sourceId, sourceRevision, "Evolving field", "Regional ash density"),
    new SourceLifecycle("volcano/deposited-ash/surface", 1, "Persistent aftermath", "Deposited ash surface")
};

var trace = new NaturalPhenomenaTrace(
    sourceLifecycle,
    decisions.Select((decision, index) => new ObserverDecisionTrace(
        decision.ObserverId,
        decision.ObserverKind.ToString(),
        decision.SourceId,
        decision.SourceRevision,
        decision.Selected!.Id,
        decision.ReducedDimensions,
        decision.RejectedCandidates,
        decision.CandidateScores)).ToArray(),
    aggregation.Aggregates.Select(item => new AggregateTrace(
        item.AggregationKey,
        item.Sources,
        item.IsAggregated,
        item.EstimatedTotalError)).ToArray(),
    dependencyTrace);

var jsonPath = Path.Combine(outputDirectory, "natural-phenomena-trace.json");
var svgPath = Path.Combine(outputDirectory, "natural-phenomena-event-horizons.svg");
File.WriteAllText(jsonPath, JsonSerializer.Serialize(trace, new JsonSerializerOptions { WriteIndented = true }));
File.WriteAllText(svgPath, BuildSvg(trace), new UTF8Encoding(false));

Console.WriteLine("PASS: first-person, camera, and AI/map observers selected deterministic independent profiles.");
Console.WriteLine("PASS: all decisions preserve the authoritative ash source identity and revision.");
Console.WriteLine("PASS: hazard and interaction-critical contributions remain individual.");
Console.WriteLine("PASS: transient, evolving-field, observer-history, and persistent-aftermath dependencies invalidate only declared outputs.");
Console.WriteLine($"Wrote {jsonPath}");
Console.WriteLine($"Wrote {svgPath}");

static RepresentationCandidate[] CreateCandidates() =>
[
    new RepresentationCandidate(
        "FullVisual",
        new RepresentationDimensionProfile(
            RepresentationDetailLevel.Full, RepresentationDetailLevel.Full,
            RepresentationDetailLevel.Full, RepresentationDetailLevel.Full,
            60, RepresentationResidency.Resident,
            RepresentationDetailLevel.Full, RepresentationDetailLevel.Full,
            RepresentationDetailLevel.Full),
        new RepresentationResourceCost(3, 4, 4 * 1024 * 1024, 2 * 1024 * 1024, 20, 0.1)),
    new RepresentationCandidate(
        "RegionalVolume",
        new RepresentationDimensionProfile(
            RepresentationDetailLevel.Summary, RepresentationDetailLevel.Reduced,
            RepresentationDetailLevel.Summary, RepresentationDetailLevel.Summary,
            5, RepresentationResidency.Streamed,
            RepresentationDetailLevel.Omitted, RepresentationDetailLevel.Full,
            RepresentationDetailLevel.Reduced),
        new RepresentationResourceCost(1, 0.8, 512 * 1024, 256 * 1024, 120, 0.2)),
    new RepresentationCandidate(
        "HazardMarker",
        new RepresentationDimensionProfile(
            RepresentationDetailLevel.Summary, RepresentationDetailLevel.Summary,
            RepresentationDetailLevel.Omitted, RepresentationDetailLevel.Omitted,
            1, RepresentationResidency.ReconstructOnDemand,
            RepresentationDetailLevel.Reduced, RepresentationDetailLevel.Full,
            RepresentationDetailLevel.Summary),
        new RepresentationResourceCost(0.2, 0.1, 64 * 1024, 8 * 1024, 150, 0.1))
];

static IReadOnlyList<BucketTrace> RunDependencyProof()
{
    var transient = new EventHorizon("Transient", 1, 60);
    var regional = new EventHorizon("Regional", 1000, 5);
    var persistent = new EventHorizon("Persistent", double.MaxValue, 1);
    var observer = new EventHorizon("Observer", double.MaxValue, 60);

    var lightning = new EventHorizonBucket("Lightning event", transient, 60);
    var ashDensity = new EventHorizonBucket("Ash density field", regional, 5);
    var depositedAsh = new EventHorizonBucket("Deposited ash", persistent, 1);
    var camera = new EventHorizonBucket("Camera state", observer, 60);
    var shadows = new EventHorizonBucket("Shadow history", transient, 30);
    var temporalLighting = new EventHorizonBucket("Temporal lighting history", transient, 30);
    var volumeIntegration = new EventHorizonBucket("Volume integration", regional, 5);
    var viewHistory = new EventHorizonBucket("View history", observer, 60);
    var surfaceMaterials = new EventHorizonBucket("Surface materials", persistent, 1);
    var surfaceLighting = new EventHorizonBucket("Surface lighting", persistent, 1);

    using var graph = new EventHorizonBucketDependencyGraph();
    graph.AddDependency(lightning, shadows);
    graph.AddDependency(lightning, temporalLighting);
    graph.AddDependency(ashDensity, volumeIntegration);
    graph.AddDependency(camera, viewHistory);
    graph.AddDependency(depositedAsh, surfaceMaterials);
    graph.AddDependency(depositedAsh, surfaceLighting);

    var all = new[]
    {
        lightning, ashDensity, depositedAsh, camera, shadows,
        temporalLighting, volumeIntegration, viewHistory, surfaceMaterials, surfaceLighting
    };
    var epoch = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    foreach (var bucket in all)
        Refresh(bucket, epoch, 0);

    lightning.SetSourceRevision(1);
    Require(!shadows.HasValidRepresentation && !temporalLighting.HasValidRepresentation,
        "Lightning must invalidate shadow and temporal-lighting histories.");
    Require(volumeIntegration.HasValidRepresentation && surfaceMaterials.HasValidRepresentation,
        "Lightning must not invalidate ash integration or persistent surfaces.");

    Refresh(shadows, epoch.AddSeconds(1), 0);
    Refresh(temporalLighting, epoch.AddSeconds(1), 0);
    ashDensity.SetSourceRevision(2);
    Require(!volumeIntegration.HasValidRepresentation, "Ash density must invalidate volume integration.");
    Require(shadows.HasValidRepresentation && surfaceMaterials.HasValidRepresentation,
        "Ash density must not invalidate lighting history or persistent surfaces.");

    Refresh(volumeIntegration, epoch.AddSeconds(2), 0);
    camera.MarkDirty(BucketDirtyReason.ObserverChanged);
    Require(!viewHistory.HasValidRepresentation && volumeIntegration.HasValidRepresentation,
        "Camera changes must invalidate view history only.");

    Refresh(viewHistory, epoch.AddSeconds(3), 0);
    depositedAsh.SetSourceRevision(1);
    Require(!surfaceMaterials.HasValidRepresentation && !surfaceLighting.HasValidRepresentation,
        "Deposited ash must invalidate persistent surface materials and lighting.");
    Require(shadows.HasValidRepresentation && volumeIntegration.HasValidRepresentation && viewHistory.HasValidRepresentation,
        "Persistent aftermath must not invalidate unrelated transient, volume, or observer outputs.");

    return all.Select(bucket => new BucketTrace(
        bucket.Name,
        bucket.HasValidRepresentation,
        bucket.DirtyReasons.ToString(),
        bucket.Metrics.Invalidations,
        bucket.Metrics.AcceptedRefreshes,
        bucket.Metrics.ReuseCount)).ToArray();
}

static void Refresh(EventHorizonBucket bucket, DateTimeOffset now, long revision)
{
    bucket.SetSourceRevision(revision);
    if (!bucket.TryBeginRefresh(now, out var ticket))
        throw new InvalidOperationException($"Refresh was unexpectedly not due for {bucket.Name}.");
    if (!bucket.TryCompleteRefresh(ticket, now.AddMilliseconds(1)))
        throw new InvalidOperationException($"Refresh completion was rejected for {bucket.Name}.");
}

static string BuildSvg(NaturalPhenomenaTrace trace)
{
    var sb = new StringBuilder();
    sb.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
    sb.AppendLine("""<svg xmlns="http://www.w3.org/2000/svg" width="1160" height="760" viewBox="0 0 1160 760" role="img" aria-labelledby="title desc">""");
    sb.AppendLine("""<title id="title">Natural phenomena: multidimensional Event Horizon proof</title>""");
    sb.AppendLine("""<desc id="desc">Deterministic CPU-side representation policy, contribution aggregation, and dependency invalidation trace for a volcanic ash field with transient lightning and persistent deposited ash. No simulation or GPU execution is claimed.</desc>""");
    sb.AppendLine("""<rect width="1160" height="760" fill="#101923"/><text x="36" y="48" fill="#f4f7fb" font-family="Segoe UI,sans-serif" font-size="25" font-weight="700">ONE PHENOMENON · THREE OBSERVERS · SEPARATE LIFECYCLES</text>""");
    sb.AppendLine("""<text x="36" y="76" fill="#a9bacb" font-family="Consolas,monospace" font-size="13">Volcanic ash field · deterministic CPU policy proof · source identity and revision retained</text>""");
    sb.AppendLine("""<rect x="36" y="105" width="1088" height="90" rx="12" fill="#1b2b3a" stroke="#466176"/><text x="58" y="135" fill="#f2d6a2" font-family="Segoe UI,sans-serif" font-size="15" font-weight="700">SOURCE LIFECYCLES</text>""");
    sb.AppendLine("""<text x="58" y="164" fill="#f4f7fb" font-family="Consolas,monospace" font-size="13">Lightning event (transient)  →  Ash density field (evolving)  →  Deposited ash (persistent aftermath)</text>""");
    sb.AppendLine("""<text x="36" y="230" fill="#f4f7fb" font-family="Segoe UI,sans-serif" font-size="18" font-weight="700">OBSERVER-RELATIVE MULTIDIMENSIONAL SELECTION</text>""");
    for (var i = 0; i < trace.Observers.Length; i++)
    {
        var observer = trace.Observers[i];
        var y = 250 + i * 86;
        sb.AppendLine($"""<rect x="36" y="{y}" width="1088" height="70" rx="10" fill="#1b2b3a" stroke="#466176"/><text x="56" y="{y + 27}" fill="#f4f7fb" font-family="Segoe UI,sans-serif" font-size="15" font-weight="700">{Esc(observer.ObserverId)} · {Esc(observer.Selected)}</text><text x="56" y="{y + 51}" fill="#a9bacb" font-family="Consolas,monospace" font-size="11">source {Esc(observer.SourceId)} @ r{observer.SourceRevision} · reduced: {Esc(string.Join(", ", observer.ReducedDimensions))}</text>""");
    }
    sb.AppendLine("""<text x="36" y="535" fill="#f4f7fb" font-family="Segoe UI,sans-serif" font-size="18" font-weight="700">DEPENDENCY INVALIDATION</text>""");
    var invalidated = trace.Buckets.Where(x => !x.IsValid).ToArray();
    var valid = trace.Buckets.Where(x => x.IsValid).ToArray();
    sb.AppendLine($"""<rect x="36" y="554" width="530" height="142" rx="10" fill="#2d2419" stroke="#b88742"/><text x="56" y="582" fill="#ffd28a" font-family="Segoe UI,sans-serif" font-size="14" font-weight="700">INVALIDATED PATHS ({invalidated.Length})</text>""");
    for (var i = 0; i < invalidated.Length; i++)
        sb.AppendLine($"""<text x="56" y="{610 + i * 20}" fill="#f4f7fb" font-family="Consolas,monospace" font-size="11">{Esc(invalidated[i].Name)} · {Esc(invalidated[i].DirtyReasons)}</text>""");
    sb.AppendLine($"""<rect x="594" y="554" width="530" height="142" rx="10" fill="#183027" stroke="#4c9c72"/><text x="614" y="582" fill="#8fe0ac" font-family="Segoe UI,sans-serif" font-size="14" font-weight="700">UNAFFECTED / REUSABLE ({valid.Length})</text>""");
    for (var i = 0; i < valid.Length; i++)
        sb.AppendLine($"""<text x="614" y="{610 + i * 20}" fill="#f4f7fb" font-family="Consolas,monospace" font-size="11">{Esc(valid[i].Name)} · accepted {valid[i].AcceptedRefreshes} · reuse {valid[i].ReuseCount}</text>""");
    sb.AppendLine("""<text x="36" y="730" fill="#a9bacb" font-family="Segoe UI,sans-serif" font-size="12">Budget costs are estimates. This proof does not simulate ash, lightning, surface deposition, shaders, particles, or GPU work.</text></svg>""");
    return sb.ToString();
}

static string Esc(string value) => SecurityElement.Escape(value) ?? value;

static void Require(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

sealed record ObserverBudget(RepresentationObservation Observation, RepresentationResourceBudget Budget);
sealed record SourceLifecycle(string SourceId, long Revision, string Lifecycle, string Description);
sealed record ObserverDecisionTrace(string ObserverId, string ObserverKind, string SourceId, long SourceRevision, string Selected, IReadOnlyList<string> ReducedDimensions, IReadOnlyList<string> RejectedCandidates, IReadOnlyList<string> CandidateScores);
sealed record AggregateTrace(string AggregationKey, IReadOnlyList<RenderContributionProvenance> Sources, bool IsAggregated, double EstimatedTotalError);
sealed record BucketTrace(string Name, bool IsValid, string DirtyReasons, long Invalidations, long AcceptedRefreshes, long ReuseCount);
sealed record NaturalPhenomenaTrace(IReadOnlyList<SourceLifecycle> Sources, IReadOnlyList<ObserverDecisionTrace> Observers, IReadOnlyList<AggregateTrace> Aggregates, IReadOnlyList<BucketTrace> Buckets);
