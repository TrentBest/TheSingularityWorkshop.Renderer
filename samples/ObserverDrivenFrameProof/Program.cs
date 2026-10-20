using System.Globalization;
using System.Security;
using System.Text;
using System.Numerics;
using TheSingularityWorkshop.Renderer;

var outputPath = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath("artifacts/observer-driven-frame-proof.svg");
var firstObserverX = args.Length > 1 ? Parse(args[1]) : 0f;
var secondObserverX = args.Length > 2 ? Parse(args[2]) : 50f;
if (firstObserverX == secondObserverX)
    throw new ArgumentException("The two observer positions must differ to demonstrate a policy change.");

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

var first = BuildFrame(firstObserverX);
var second = BuildFrame(secondObserverX);
if (!first.Rows.Zip(second.Rows).Any(pair => pair.First.Representation != pair.Second.Representation))
    throw new InvalidOperationException("The selected representations did not change; choose observer positions that cross a horizon.");
if (first.TotalIndices == second.TotalIndices)
    throw new InvalidOperationException("The policy change did not alter the selected index-work total.");

var svg = new StringBuilder();
svg.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
svg.AppendLine("""<svg xmlns="http://www.w3.org/2000/svg" width="1120" height="610" viewBox="0 0 1120 610" role="img" aria-labelledby="title desc">""");
svg.AppendLine("""  <title id="title">Observer-driven Event Horizon frame proof</title>""");
svg.AppendLine("""  <desc id="desc">Two RenderFrames are built by applying Renderer.ObserverDistance to the same three entities at different observer positions. The selected representation and index-count work change because of the actual distance policy. This is a CPU-side contract proof, not GPU output.</desc>""");
svg.AppendLine("""  <defs><linearGradient id="bg" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#101c2a"/><stop offset="1" stop-color="#070b12"/></linearGradient></defs>""");
svg.AppendLine("""  <rect width="1120" height="610" rx="20" fill="url(#bg)"/>""");
svg.AppendLine("""  <text x="36" y="42" fill="#f4f8ff" font-family="Segoe UI, sans-serif" font-size="24" font-weight="700">The observer moves. The selected work changes.</text>""");
svg.AppendLine("""  <text x="36" y="68" fill="#9fb6cc" font-family="Segoe UI, sans-serif" font-size="13">Renderer.ObserverDistance → representation policy → actual RenderFrame command snapshots</text>""");

DrawFrame(first, 36, "A · First observer");
DrawFrame(second, 576, "B · Moved observer");

svg.AppendLine("""  <path d="M36 540 H1084" stroke="#294257"/>""");
svg.AppendLine($"""  <text x="36" y="566" fill="#f4f8ff" font-family="Consolas, monospace" font-size="14">Index work: {first.TotalIndices:N0} → {second.TotalIndices:N0}</text>""");
svg.AppendLine($"""  <text x="36" y="589" fill="#9fb6cc" font-family="Segoe UI, sans-serif" font-size="12">Three ordered CPU draw commands per frame; representation-specific index counts change. No GPU submission or pixel output is claimed.</text>""");
svg.AppendLine("</svg>");

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(outputPath, svg.ToString(), new UTF8Encoding(false));
Console.WriteLine($"Wrote {outputPath}");
Console.WriteLine($"Observer X {F(firstObserverX)}: {Summary(first)}");
Console.WriteLine($"Observer X {F(secondObserverX)}: {Summary(second)}");
Console.WriteLine($"Selected index work: {first.TotalIndices} -> {second.TotalIndices}");

FrameSnapshot BuildFrame(float observerX)
{
    var observerPosition = new Vector3(observerX, 0, 0);
    var view = new RenderView(observerPosition, Matrix4x4.Identity, Matrix4x4.Identity);
    var rows = new List<Row>();
    var commands = new List<RenderDrawCommand>();

    foreach (var subject in subjects)
    {
        var distance = Vector3.Distance(observerPosition, subject.Position);
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
            _ => throw new InvalidOperationException($"No work fixture for '{representation}'.")
        };

        rows.Add(new Row(subject.Name, distance, horizon.Name, representation, indexCount));
        commands.Add(new RenderDrawCommand(
            new RenderResourceId((ulong)(100 + subject.Id)),
            new RenderResourceId((ulong)(200 + subject.Id)),
            Matrix4x4.CreateTranslation(subject.Position),
            indexCount, 0, 0, representation,
            entity: new RenderEntityId(subject.Id)));
    }

    return new FrameSnapshot(observerPosition, new RenderFrame(view, commands), rows);
}

void DrawFrame(FrameSnapshot snapshot, int x, string heading)
{
    svg.AppendLine($"""  <rect x="{x}" y="92" width="508" height="420" rx="14" fill="#14283a" stroke="#31516a"/>""");
    svg.AppendLine($"""  <text x="{x + 20}" y="122" fill="#7fe0ee" font-family="Segoe UI, sans-serif" font-size="16" font-weight="700">{heading}</text>""");
    svg.AppendLine($"""  <text x="{x + 20}" y="148" fill="#f4f8ff" font-family="Consolas, monospace" font-size="13">Observer X = {F(snapshot.View.ObserverPosition.X)} · Commands = {snapshot.Frame.Commands.Count}</text>""");
    svg.AppendLine($"""  <text x="{x + 20}" y="178" fill="#9fb6cc" font-family="Consolas, monospace" font-size="11">SUBJECT / DISTANCE</text><text x="{x + 280}" y="178" fill="#9fb6cc" font-family="Consolas, monospace" font-size="11">REPRESENTATION / INDICES</text>""");

    for (var i = 0; i < snapshot.Rows.Count; i++)
    {
        var row = snapshot.Rows[i];
        var y = 194 + i * 92;
        var color = row.Representation switch
        {
            RendererRepresentation.Interactive => "#ffcf74",
            RendererRepresentation.Near => "#63d8f2",
            _ => "#69aaff"
        };
        svg.AppendLine($"""  <rect x="{x + 16}" y="{y}" width="476" height="78" rx="9" fill="#101f2d" stroke="#294257"/>""");
        svg.AppendLine($"""  <text x="{x + 30}" y="{y + 27}" fill="#f4f8ff" font-family="Segoe UI, sans-serif" font-size="14" font-weight="700">{Esc(row.Name)}</text>""");
        svg.AppendLine($"""  <text x="{x + 30}" y="{y + 51}" fill="#9fb6cc" font-family="Consolas, monospace" font-size="12">distance {F(row.Distance)}</text>""");
        svg.AppendLine($"""  <text x="{x + 280}" y="{y + 27}" fill="{color}" font-family="Consolas, monospace" font-size="13" font-weight="700">{row.Representation}</text>""");
        svg.AppendLine($"""  <text x="{x + 280}" y="{y + 51}" fill="#d9e7f3" font-family="Consolas, monospace" font-size="12">{row.IndexCount:N0} selected indices</text>""");
    }
}

static string Summary(FrameSnapshot snapshot) =>
    string.Join(", ", snapshot.Rows.Select(row => $"{row.Name}={row.Representation}")) +
    $"; commands={snapshot.Frame.Commands.Count}; indices={snapshot.TotalIndices}";

static float Parse(string value) =>
    float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) && float.IsFinite(parsed)
        ? parsed
        : throw new ArgumentException($"'{value}' is not a finite observer X coordinate.");

static string F(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);
static string Esc(string value) => SecurityElement.Escape(value) ?? value;

sealed record Subject(ulong Id, string Name, Vector3 Position);
sealed record Row(string Name, double Distance, string HorizonName, RendererRepresentation Representation, int IndexCount);
sealed record FrameSnapshot(Vector3 ObserverPosition, RenderFrame Frame, IReadOnlyList<Row> Rows)
{
    public int TotalIndices => Frame.Commands.Sum(command => command.IndexCount);
    public RenderView View => Frame.View;
}
