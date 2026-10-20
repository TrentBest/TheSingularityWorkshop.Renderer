using System.Text.Json;
using System.Text.Json.Serialization;
using TheSingularityWorkshop.Renderer;

var outputPath = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath("artifacts/renderer-workload-trace.json");

var nearHorizon = new EventHorizon("Near", 100, 60);
var farHorizon = new EventHorizon("Far", 1_000, 2);
var nearBucket = new EventHorizonBucket("Instances", nearHorizon, nearHorizon.UpdatesPerSecond);
var farBucket = new EventHorizonBucket("Instances", farHorizon, farHorizon.UpdatesPerSecond);

var policy = new ObserverRepresentationPolicy(
[
    new RepresentationHorizonRule(nearHorizon, RepresentationTarget.Near),
    new RepresentationHorizonRule(farHorizon, RepresentationTarget.Distant)
]);

using var coordinator = new ObserverRepresentationCoordinator(
    policy,
    [nearBucket, farBucket],
    "array-first-workload-trace");

var rows = new List<TraceRow>();
var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
const int frameCount = 120;

for (var frame = 0; frame < frameCount; frame++)
{
    // Deterministic observer path: near, far, near. The source revision changes once,
    // and the observer revision changes once, so both invalidation paths are exercised.
    var distance = frame < 36 || frame >= 84 ? 25d : 250d;
    var sourceRevision = frame < 72 ? 1L : 2L;
    var observerRevision = frame < 60 ? 1L : 2L;
    var now = start.AddMilliseconds(frame * (1000d / 60d));

    var evaluation = coordinator.Evaluate(distance, sourceRevision, observerRevision, now);
    var refreshAccepted = false;
    if (evaluation.RefreshTicket is { } ticket)
        refreshAccepted = evaluation.Bucket.TryCompleteRefresh(ticket, now);

    var metrics = evaluation.Bucket.Metrics;
    rows.Add(new TraceRow(
        Frame: frame,
        TimeMilliseconds: (long)Math.Round(frame * (1000d / 60d)),
        Distance: distance,
        SourceRevision: sourceRevision,
        ObserverRevision: observerRevision,
        Horizon: evaluation.Decision.HorizonName,
        Representation: evaluation.ActiveRepresentation.ToString(),
        Bucket: evaluation.Bucket.Name,
        RefreshRequired: evaluation.RefreshRequired,
        RefreshAccepted: refreshAccepted,
        ReusedCachedRepresentation: evaluation.CanReuseCachedRepresentation,
        DirtyReasons: evaluation.Bucket.DirtyReasons.ToString(),
        RefreshAttempts: metrics.RefreshAttempts,
        AcceptedRefreshes: metrics.AcceptedRefreshes,
        RejectedRefreshes: metrics.RejectedRefreshes,
        Invalidations: metrics.Invalidations,
        ReuseCount: metrics.ReuseCount));
}

var trace = new TraceDocument(
    SchemaVersion: 1,
    Generator: "TheSingularityWorkshop.Renderer",
    Workload: "array-first-observer-horizon",
    FrameRateHz: 60,
    FrameCount: frameCount,
    Notes:
    [
        "Decisions are produced by ObserverRepresentationCoordinator and EventHorizonBucket.",
        "The sample simulates completion of refresh work immediately; it does not build visual geometry.",
        "This trace is policy evidence, not GPU output, browser runtime evidence, or a performance benchmark."
    ],
    Frames: rows);

var options = new JsonSerializerOptions { WriteIndented = true };
options.Converters.Add(new JsonStringEnumConverter());
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(trace, options));
Console.WriteLine($"Wrote {rows.Count} Renderer evaluations to {outputPath}.");
Console.WriteLine($"Refresh requests: {rows.Count(row => row.RefreshRequired)}; accepted: {rows.Count(row => row.RefreshAccepted)}; cached reuse decisions: {rows.Count(row => row.ReusedCachedRepresentation)}.");

internal sealed record TraceDocument(
    int SchemaVersion,
    string Generator,
    string Workload,
    int FrameRateHz,
    int FrameCount,
    string[] Notes,
    IReadOnlyList<TraceRow> Frames);

internal sealed record TraceRow(
    int Frame,
    long TimeMilliseconds,
    double Distance,
    long SourceRevision,
    long ObserverRevision,
    string Horizon,
    string Representation,
    string Bucket,
    bool RefreshRequired,
    bool RefreshAccepted,
    bool ReusedCachedRepresentation,
    string DirtyReasons,
    long RefreshAttempts,
    long AcceptedRefreshes,
    long RejectedRefreshes,
    long Invalidations,
    long ReuseCount);
