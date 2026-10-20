using System.Globalization;
using System.Security;
using System.Text;
using TheSingularityWorkshop.Renderer;

var outputPath = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath("artifacts/bucket-invalidation-reuse-proof.svg");

var epoch = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
var nearHorizon = new EventHorizon("Near", 100, 30);
var farHorizon = new EventHorizon("Far", 10000, 1);
var geometry = new EventHorizonBucket("Environment geometry", nearHorizon, 30);
var lighting = new EventHorizonBucket("Lighting", nearHorizon, 15);
var distantMotion = new EventHorizonBucket("Distant motion", farHorizon, 1);
var shadow = new EventHorizonBucket("Shadow map", nearHorizon, 15);

using var dependencies = new EventHorizonBucketDependencyGraph();
dependencies.AddDependency(geometry, shadow);
dependencies.AddDependency(lighting, shadow);

Refresh(geometry, epoch, 1);
Refresh(lighting, epoch, 1);
Refresh(distantMotion, epoch, 1);
Refresh(shadow, epoch, 1);

// An unchanged source revision is a no-op. The cached geometry remains reusable.
geometry.SetSourceRevision(1);
if (!geometry.TryRecordReuse())
    throw new InvalidOperationException("Expected unchanged geometry to remain reusable.");

// A source change invalidates geometry and only its declared dependent shadow output.
// Lighting and distant motion must remain valid and independently reusable.
geometry.SetSourceRevision(2);
if (geometry.HasValidRepresentation || shadow.HasValidRepresentation)
    throw new InvalidOperationException("Source change did not invalidate geometry and its dependent shadow bucket.");
if (!lighting.HasValidRepresentation || !distantMotion.HasValidRepresentation)
    throw new InvalidOperationException("Invalidation escaped the declared dependency path.");
if (!lighting.TryRecordReuse() || !distantMotion.TryRecordReuse())
    throw new InvalidOperationException("Unaffected buckets should record cache reuse.");

// Accept the replacement geometry, then the dependent shadow output.
Refresh(geometry, epoch.AddMilliseconds(10), 2);
Refresh(shadow, epoch.AddMilliseconds(20), 1);
if (!geometry.HasValidRepresentation || !shadow.HasValidRepresentation)
    throw new InvalidOperationException("Expected refreshed dependent outputs to become reusable.");

var buckets = new[] { geometry, lighting, distantMotion, shadow };
var svg = new StringBuilder();
svg.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
svg.AppendLine("""<svg xmlns="http://www.w3.org/2000/svg" width="1120" height="660" viewBox="0 0 1120 660" role="img" aria-labelledby="title desc">""");
svg.AppendLine("""  <title id="title">Event Horizon bucket invalidation and reuse proof</title>""");
svg.AppendLine("""  <desc id="desc">Runs actual EventHorizonBucket refresh, source revision, reuse metrics, and dependency graph APIs. A geometry source change invalidates geometry and its declared shadow dependent while lighting and distant motion remain reusable. This is deterministic CPU-side policy evidence, not GPU work.</desc>""");
svg.AppendLine("""  <defs><linearGradient id="bg" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#101c2a"/><stop offset="1" stop-color="#070b12"/></linearGradient></defs>""");
svg.AppendLine("""  <rect width="1120" height="660" rx="20" fill="url(#bg)"/>""");
svg.AppendLine("""  <text x="36" y="42" fill="#f4f8ff" font-family="Segoe UI, sans-serif" font-size="24" font-weight="700">Invalidate the changed path. Reuse the rest.</text>""");
svg.AppendLine("""  <text x="36" y="69" fill="#9fb6cc" font-family="Segoe UI, sans-serif" font-size="13">Actual EventHorizonBucket + EventHorizonBucketDependencyGraph API · deterministic CPU-side proof</text>""");
svg.AppendLine("""  <rect x="36" y="94" width="1048" height="82" rx="12" fill="#14283a" stroke="#31516a"/>""");
svg.AppendLine("""  <text x="56" y="121" fill="#7fe0ee" font-family="Consolas, monospace" font-size="13" font-weight="700">SOURCE REVISION</text>""");
svg.AppendLine("""  <text x="56" y="147" fill="#f4f8ff" font-family="Consolas, monospace" font-size="14">Environment geometry: revision 1 → revision 2</text>""");
svg.AppendLine("""  <path d="M300 135 H366" stroke="#ffcf74" stroke-width="2"/><path d="M356 127 L366 135 L356 143" fill="none" stroke="#ffcf74" stroke-width="2"/>""");
svg.AppendLine("""  <rect x="382" y="108" width="190" height="52" rx="8" fill="#3b2b1b" stroke="#ffcf74"/>""");
svg.AppendLine("""  <text x="477" y="130" text-anchor="middle" fill="#ffcf74" font-family="Consolas, monospace" font-size="12" font-weight="700">GEOMETRY DIRTY</text>""");
svg.AppendLine("""  <text x="477" y="147" text-anchor="middle" fill="#f4f8ff" font-family="Consolas, monospace" font-size="10">SourceChanged</text>""");
svg.AppendLine("""  <path d="M572 135 H638" stroke="#ffcf74" stroke-width="2"/><path d="M628 127 L638 135 L628 143" fill="none" stroke="#ffcf74" stroke-width="2"/>""");
svg.AppendLine("""  <rect x="654" y="108" width="190" height="52" rx="8" fill="#3b2b1b" stroke="#ffcf74"/>""");
svg.AppendLine("""  <text x="749" y="130" text-anchor="middle" fill="#ffcf74" font-family="Consolas, monospace" font-size="12" font-weight="700">SHADOW DIRTY</text>""");
svg.AppendLine("""  <text x="749" y="147" text-anchor="middle" fill="#f4f8ff" font-family="Consolas, monospace" font-size="10">DependencyChanged</text>""");
svg.AppendLine("""  <text x="36" y="210" fill="#9fb6cc" font-family="Consolas, monospace" font-size="12">BUCKET · CADENCE</text><text x="355" y="210" fill="#9fb6cc" font-family="Consolas, monospace" font-size="12">ACTUAL METRICS AFTER SCENARIO</text>""");

for (var i = 0; i < buckets.Length; i++)
{
    var bucket = buckets[i];
    var metrics = bucket.Metrics;
    var y = 226 + i * 88;
    var wasInvalidated = metrics.Invalidations > 0;
    var state = wasInvalidated ? "INVALIDATED → REFRESHED" : "VALID / REUSABLE";
    var color = wasInvalidated ? "#ffcf74" : "#7ee0a1";
    svg.AppendLine($"""  <rect x="36" y="{y}" width="1048" height="74" rx="10" fill="#14283a" stroke="#294257"/>""");
    svg.AppendLine($"""  <text x="54" y="{y + 24}" fill="#f4f8ff" font-family="Segoe UI, sans-serif" font-size="14" font-weight="700">{Esc(bucket.Name)}</text>""");
    svg.AppendLine($"""  <text x="54" y="{y + 48}" fill="#9fb6cc" font-family="Consolas, monospace" font-size="12">{F(bucket.UpdatesPerSecond)} updates/s · {bucket.Horizon.Name} horizon</text>""");
    svg.AppendLine($"""  <text x="355" y="{y + 24}" fill="{color}" font-family="Consolas, monospace" font-size="12" font-weight="700">{state}</text>""");
    svg.AppendLine($"""  <text x="355" y="{y + 48}" fill="#d9e7f3" font-family="Consolas, monospace" font-size="11">refreshes accepted {metrics.AcceptedRefreshes} · invalidations {metrics.Invalidations} · reuse {metrics.ReuseCount} · rejected {metrics.RejectedRefreshes}</text>""");
}
svg.AppendLine("""  <path d="M36 594 H1084" stroke="#294257"/>""");
svg.AppendLine("""  <text x="36" y="620" fill="#f4f8ff" font-family="Segoe UI, sans-serif" font-size="13">What this proves: explicit source revision invalidation, declared dependency propagation, independent bucket validity, and consumer-confirmed reuse.</text>""");
svg.AppendLine("""  <text x="36" y="642" fill="#9fb6cc" font-family="Segoe UI, sans-serif" font-size="12">Not claimed: GPU submission, rendered pixels, skipped GPU work, timing, or allocation savings.</text>""");
svg.AppendLine("</svg>");

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(outputPath, svg.ToString(), new UTF8Encoding(false));
Console.WriteLine($"Wrote {outputPath}");
foreach (var bucket in buckets)
{
    var m = bucket.Metrics;
    Console.WriteLine($"{bucket.Name}: valid={bucket.HasValidRepresentation}; accepted={m.AcceptedRefreshes}; invalidations={m.Invalidations}; reuse={m.ReuseCount}; rejected={m.RejectedRefreshes}; reasons={m.ObservedInvalidationReasons}");
}

static void Refresh(EventHorizonBucket bucket, DateTimeOffset now, long revision)
{
    bucket.SetSourceRevision(revision);
    if (!bucket.TryBeginRefresh(now, out var ticket))
        throw new InvalidOperationException($"Refresh was unexpectedly not due for {bucket.Name}.");
    if (!bucket.TryCompleteRefresh(ticket, now.AddMilliseconds(1)))
        throw new InvalidOperationException($"Refresh completion was rejected for {bucket.Name}.");
}

static string F(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);
static string Esc(string value) => SecurityElement.Escape(value) ?? value;
