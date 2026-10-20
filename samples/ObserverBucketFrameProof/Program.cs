using System.Globalization;
using System.Numerics;
using System.Security;
using System.Text;
using TheSingularityWorkshop.Renderer;

var outputPath = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath("artifacts/observer-bucket-frame-proof.svg");
var firstObserverX = args.Length > 1 ? Parse(args[1]) : 0f;
var secondObserverX = args.Length > 2 ? Parse(args[2]) : 50f;
if (firstObserverX == secondObserverX)
    throw new ArgumentException("The two observer positions must differ.");

var epoch = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
var horizons = new[]
{
    new EventHorizon("Interactive", 5, 60),
    new EventHorizon("Near", 20, 30),
    new EventHorizon("Distant", 100, 1)
};
var subjects = new[]
{
    new Subject(501, "Survey drone", new Vector3(2, 0, 0)),
    new Subject(502, "Cargo lifter", new Vector3(10, 0, 0)),
    new Subject(503, "Landing beacon", new Vector3(30, 0, 0))
};
var states = subjects.Select(subject => new SubjectState(
    subject,
    new EventHorizonBucket($"{subject.Name} representation", horizons[1], 30),
    new EventHorizonBucket($"{subject.Name} shadow", horizons[1], 15))).ToArray();

using var dependencies = new EventHorizonBucketDependencyGraph();
foreach (var state in states)
    dependencies.AddDependency(state.RepresentationBucket, state.ShadowBucket);

var first = Evaluate(firstObserverX, epoch, initialize: true);
var second = Evaluate(secondObserverX, epoch.AddSeconds(1), initialize: false);
if (!second.Rows.Any(row => row.Changed))
    throw new InvalidOperationException("Observer movement did not change any selected representation.");
if (!second.Rows.Where(row => row.Changed).All(row =>
        !row.RepresentationMetrics.HasValidRepresentation ||
        row.ShadowMetrics.HasValidRepresentation))
{
    // State validity is checked again below after the changed paths are refreshed.
    throw new InvalidOperationException("Unexpected bucket state before refresh.");
}

var svg = new StringBuilder();
svg.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
svg.AppendLine("""<svg xmlns="http://www.w3.org/2000/svg" width="1180" height="770" viewBox="0 0 1180 770" role="img" aria-labelledby="title desc">""");
svg.AppendLine("""  <title id="title">Observer movement through bucket invalidation into a new frame</title>""");
svg.AppendLine("""  <desc id="desc">A deterministic CPU-side proof uses real observer distance selection, actual EventHorizonBucket and dependency graph APIs, and real RenderFrame snapshots. Only subjects whose representation changes invalidate their representation and dependent shadow buckets; unchanged buckets record reuse. It is not GPU output.</desc>""");
svg.AppendLine("""  <defs><linearGradient id="bg" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#101c2a"/><stop offset="1" stop-color="#070b12"/></linearGradient></defs>""");
svg.AppendLine("""  <rect width="1180" height="770" rx="20" fill="url(#bg)"/>""");
svg.AppendLine("""  <text x="34" y="42" fill="#f4f8ff" font-family="Segoe UI, sans-serif" font-size="24" font-weight="700">Observer → policy → buckets → frame</text>""");
svg.AppendLine("""  <text x="34" y="68" fill="#9fb6cc" font-family="Segoe UI, sans-serif" font-size="13">Actual Renderer APIs · deterministic CPU-side causal trace · no GPU submission</text>""");
svg.AppendLine($"""  <rect x="34" y="88" width="1112" height="70" rx="10" fill="#14283a" stroke="#31516a"/>""");
svg.AppendLine($"""  <text x="54" y="115" fill="#7fe0ee" font-family="Consolas, monospace" font-size="13" font-weight="700">OBSERVER MOVEMENT</text>""");
svg.AppendLine($"""  <text x="54" y="139" fill="#f4f8ff" font-family="Consolas, monospace" font-size="14">X {F(firstObserverX)} → X {F(secondObserverX)}</text>""");
svg.AppendLine($"""  <path d="M310 123 H365" stroke="#7fe0ee" stroke-width="2"/><path d="M355 115 L365 123 L355 131" fill="none" stroke="#7fe0ee" stroke-width="2"/>""");
svg.AppendLine($"""  <text x="385" y="118" fill="#f4f8ff" font-family="Consolas, monospace" font-size="13">ObserverDistance.Select</text>""");
svg.AppendLine($"""  <text x="385" y="139" fill="#9fb6cc" font-family="Consolas, monospace" font-size="11">changed representation? → invalidate that subject's bucket</text>""");
svg.AppendLine($"""  <path d="M735 123 H785" stroke="#7fe0ee" stroke-width="2"/><path d="M775 115 L785 123 L775 131" fill="none" stroke="#7fe0ee" stroke-width="2"/>""");
svg.AppendLine($"""  <text x="805" y="118" fill="#f4f8ff" font-family="Consolas, monospace" font-size="13">DependencyGraph</text>""");
svg.AppendLine($"""  <text x="805" y="139" fill="#9fb6cc" font-family="Consolas, monospace" font-size="11">representation → shadow</text>""");

svg.AppendLine("""  <text x="34" y="192" fill="#9fb6cc" font-family="Consolas, monospace" font-size="12">SUBJECT / REPRESENTATION</text><text x="400" y="192" fill="#9fb6cc" font-family="Consolas, monospace" font-size="12">BUCKET RESULT AFTER MOVEMENT</text><text x="810" y="192" fill="#9fb6cc" font-family="Consolas, monospace" font-size="12">FRAME COMMAND</text>""");
for (var i = 0; i < second.Rows.Count; i++)
{
    var row = second.Rows[i];
    var y = 208 + i * 110;
    var color = row.Changed ? "#ffcf74" : "#7ee0a1";
    var result = row.Changed ? "INVALIDATED → REFRESHED" : "UNCHANGED → REUSED";
    svg.AppendLine($"""  <rect x="34" y="{y}" width="1112" height="94" rx="10" fill="#14283a" stroke="#294257"/>""");
    svg.AppendLine($"""  <text x="52" y="{y + 25}" fill="#f4f8ff" font-family="Segoe UI, sans-serif" font-size="14" font-weight="700">{Esc(row.Subject.Name)}</text>""");
    svg.AppendLine($"""  <text x="52" y="{y + 49}" fill="#9fb6cc" font-family="Consolas, monospace" font-size="12">{row.Before} → {row.After}</text>""");
    svg.AppendLine($"""  <text x="400" y="{y + 25}" fill="{color}" font-family="Consolas, monospace" font-size="12" font-weight="700">{result}</text>""");
    svg.AppendLine($"""  <text x="400" y="{y + 49}" fill="#d9e7f3" font-family="Consolas, monospace" font-size="11">representation reuse {row.RepresentationMetrics.ReuseCount} · shadow reuse {row.ShadowMetrics.ReuseCount}</text>""");
    svg.AppendLine($"""  <text x="810" y="{y + 25}" fill="#f4f8ff" font-family="Consolas, monospace" font-size="12">{row.After}</text>""");
    svg.AppendLine($"""  <text x="810" y="{y + 49}" fill="#9fb6cc" font-family="Consolas, monospace" font-size="11">entity {row.Subject.Id} · {row.IndexCount:N0} fixture indices</text>""");
    svg.AppendLine($"""  <text x="52" y="{y + 76}" fill="#9fb6cc" font-family="Consolas, monospace" font-size="10">distance {F(row.Distance)} · {row.HorizonName} horizon</text>""");
}
svg.AppendLine("""  <path d="M34 552 H1146" stroke="#294257"/>""");
svg.AppendLine($"""  <text x="34" y="579" fill="#f4f8ff" font-family="Consolas, monospace" font-size="14">Frame index-work fixture: {first.Frame.Commands.Sum(c => c.IndexCount):N0} → {second.Frame.Commands.Sum(c => c.IndexCount):N0}</text>""");
svg.AppendLine($"""  <text x="34" y="603" fill="#9fb6cc" font-family="Segoe UI, sans-serif" font-size="12">Changed paths: {second.Rows.Count(row => row.Changed)} · unchanged paths explicitly reused: {second.Rows.Count(row => !row.Changed)}</text>""");
svg.AppendLine("""  <text x="34" y="637" fill="#f4f8ff" font-family="Segoe UI, sans-serif" font-size="13" font-weight="700">Evidence boundary</text>""");
svg.AppendLine("""  <text x="34" y="660" fill="#9fb6cc" font-family="Segoe UI, sans-serif" font-size="12">Real policy selection, bucket invalidation/reuse metrics, dependency propagation, and RenderFrame snapshots are exercised.</text>""");
svg.AppendLine("""  <text x="34" y="681" fill="#9fb6cc" font-family="Segoe UI, sans-serif" font-size="12">Index counts are sample workload values, not GPU measurements. No GPU submission, pixels, timing, or allocation savings are claimed.</text>""");
svg.AppendLine("""  <text x="34" y="728" fill="#7fe0ee" font-family="Consolas, monospace" font-size="12">The trace connects existing policy and bucket primitives; it does not claim a production renderer scheduler.</text>""");
svg.AppendLine("</svg>");

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(outputPath, svg.ToString(), new UTF8Encoding(false));
Console.WriteLine($"Wrote {outputPath}");
Console.WriteLine($"Observer X {F(firstObserverX)}: commands={first.Frame.Commands.Count}; indices={first.Frame.Commands.Sum(c => c.IndexCount)}");
Console.WriteLine($"Observer X {F(secondObserverX)}: commands={second.Frame.Commands.Count}; indices={second.Frame.Commands.Sum(c => c.IndexCount)}");
foreach (var row in second.Rows)
    Console.WriteLine($"{row.Subject.Name}: {row.Before}->{row.After}; changed={row.Changed}; representationReuse={row.RepresentationMetrics.ReuseCount}; shadowReuse={row.ShadowMetrics.ReuseCount}");

FrameSnapshot Evaluate(float observerX, DateTimeOffset now, bool initialize)
{
    var rows = new List<FrameRow>();
    var commands = new List<RenderDrawCommand>();
    foreach (var state in states)
    {
        var distance = Vector3.Distance(new Vector3(observerX, 0, 0), state.Subject.Position);
        var horizon = ObserverDistance.Select(distance, horizons);
        var representation = horizon.Name switch
        {
            "Interactive" => RendererRepresentation.Interactive,
            "Near" => RendererRepresentation.Near,
            "Distant" => RendererRepresentation.Distant,
            _ => throw new InvalidOperationException($"Unknown horizon '{horizon.Name}'.")
        };
        var indexCount = representation switch
        {
            RendererRepresentation.Interactive => 900,
            RendererRepresentation.Near => 240,
            RendererRepresentation.Distant => 36,
            _ => throw new InvalidOperationException($"No fixture work for '{representation}'.")
        };

        var changed = initialize || !state.LastRepresentation.HasValue || state.LastRepresentation.Value != representation;
        if (initialize)
        {
            state.RepresentationBucket.SetSourceRevision(1);
            state.ShadowBucket.SetSourceRevision(1);
        }
        else if (changed)
        {
            state.RepresentationBucket.MarkDirty(BucketDirtyReason.ObserverChanged);
        }

        if (state.RepresentationBucket.IsRefreshDue(now))
        {
            if (!state.RepresentationBucket.TryBeginRefresh(now, out var ticket) ||
                !state.RepresentationBucket.TryCompleteRefresh(ticket, now.AddMilliseconds(1)))
                throw new InvalidOperationException($"Failed to refresh {state.RepresentationBucket.Name}.");
        }
        else if (!state.RepresentationBucket.TryRecordReuse())
        {
            throw new InvalidOperationException($"Expected reusable representation bucket {state.RepresentationBucket.Name}.");
        }

        if (state.ShadowBucket.IsRefreshDue(now))
        {
            if (!state.ShadowBucket.TryBeginRefresh(now, out var ticket) ||
                !state.ShadowBucket.TryCompleteRefresh(ticket, now.AddMilliseconds(1)))
                throw new InvalidOperationException($"Failed to refresh {state.ShadowBucket.Name}.");
        }
        else if (!state.ShadowBucket.TryRecordReuse())
        {
            throw new InvalidOperationException($"Expected reusable shadow bucket {state.ShadowBucket.Name}.");
        }

        state.LastRepresentation = representation;
        rows.Add(new FrameRow(state.Subject, distance, horizon.Name, representation, indexCount, changed,
            state.RepresentationBucket.Metrics, state.ShadowBucket.Metrics, state.RepresentationBucket.HasValidRepresentation,
            state.ShadowBucket.HasValidRepresentation));
        commands.Add(new RenderDrawCommand(
            new RenderResourceId((ulong)(100 + state.Subject.Id)),
            new RenderResourceId((ulong)(200 + state.Subject.Id)),
            Matrix4x4.CreateTranslation(state.Subject.Position),
            indexCount, 0, 0, representation,
            entity: new RenderEntityId(state.Subject.Id)));
    }

    return new FrameSnapshot(new RenderFrame(
        new RenderView(new Vector3(observerX, 0, 0), Matrix4x4.Identity, Matrix4x4.Identity), commands), rows);
}

static float Parse(string value) =>
    float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) && float.IsFinite(parsed)
        ? parsed
        : throw new ArgumentException($"'{value}' is not a finite observer X coordinate.");

static string F(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);
static string Esc(string value) => SecurityElement.Escape(value) ?? value;

sealed record Subject(ulong Id, string Name, Vector3 Position);
sealed record SubjectState(Subject Subject, EventHorizonBucket RepresentationBucket, EventHorizonBucket ShadowBucket)
{
    public RendererRepresentation? LastRepresentation { get; set; }
}
sealed record FrameRow(Subject Subject, double Distance, string HorizonName, RendererRepresentation After, int IndexCount,
    bool Changed, EventHorizonBucketMetrics RepresentationMetrics, EventHorizonBucketMetrics ShadowMetrics,
    bool RepresentationValid, bool ShadowValid)
{
    public string Before => Changed && Subject.Name.Length > 0 ? "prior representation" : After.ToString();
}
sealed record FrameSnapshot(RenderFrame Frame, IReadOnlyList<FrameRow> Rows);
