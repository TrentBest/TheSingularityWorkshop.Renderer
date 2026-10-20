using System.Globalization;
using System.Security;
using System.Text;
using System.Numerics;
using TheSingularityWorkshop.Renderer;

var outputPath = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath("docs/assets/render-frame-inspector.svg");

var view = new RenderView(
    new Vector3(12, 4, -8),
    Matrix4x4.Identity,
    Matrix4x4.Identity);

var commands = new[]
{
    new RenderDrawCommand(new RenderResourceId(11), new RenderResourceId(101),
        Matrix4x4.CreateTranslation(-1.5f, 0, 0), 36, 0, 0,
        RendererRepresentation.Distant, entity: new RenderEntityId(501)),
    new RenderDrawCommand(new RenderResourceId(12), new RenderResourceId(102),
        Matrix4x4.CreateTranslation(0, 0, 0), 240, 12, 0,
        RendererRepresentation.Near, entity: new RenderEntityId(502)),
    new RenderDrawCommand(new RenderResourceId(13), new RenderResourceId(103),
        Matrix4x4.CreateTranslation(1.5f, 0, 0), 900, 48, 0,
        RendererRepresentation.Interactive, entity: new RenderEntityId(503))
};

// The diagnostic consumes the same public frame contract a rendering backend receives.
var frame = new RenderFrame(view, commands);
var svg = new StringBuilder();
svg.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
svg.AppendLine("""<svg xmlns="http://www.w3.org/2000/svg" width="1080" height="560" viewBox="0 0 1080 560" role="img" aria-labelledby="title desc">""");
svg.AppendLine("""  <title id="title">Renderer RenderFrame inspector</title>""");
svg.AppendLine("""  <desc id="desc">A generated diagnostic view of one real RenderFrame object: observer position, ordered draw commands, resource identifiers, index ranges, semantic entity identifiers, and representation decisions. It is a CPU-side contract inspector, not a GPU rendering result.</desc>""");
svg.AppendLine("""  <defs><linearGradient id="bg" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#101c2a"/><stop offset="1" stop-color="#070b12"/></linearGradient></defs>""");
svg.AppendLine("""  <rect width="1080" height="560" rx="20" fill="url(#bg)"/>""");
svg.AppendLine("""  <text x="36" y="42" fill="#f4f8ff" font-family="Segoe UI, sans-serif" font-size="23" font-weight="700">RenderFrame / command inspector</text>""");
svg.AppendLine("""  <text x="36" y="68" fill="#9fb6cc" font-family="Segoe UI, sans-serif" font-size="13">Generated from TheSingularityWorkshop.Renderer public frame contract</text>""");
svg.AppendLine("""  <rect x="36" y="92" width="1008" height="76" rx="12" fill="#14283a" stroke="#31516a"/>""");
svg.AppendLine($"""  <text x="56" y="119" fill="#7fe0ee" font-family="Consolas, monospace" font-size="12">VIEW / OBSERVER</text>""");
svg.AppendLine($"""  <text x="56" y="145" fill="#f4f8ff" font-family="Consolas, monospace" font-size="16">({F(view.ObserverPosition.X)}, {F(view.ObserverPosition.Y)}, {F(view.ObserverPosition.Z)})</text>""");
svg.AppendLine($"""  <text x="1018" y="136" text-anchor="end" fill="#c4d3e1" font-family="Consolas, monospace" font-size="13">Commands: {frame.Commands.Count} · ordered snapshot</text>""");
svg.AppendLine("""  <text x="40" y="205" fill="#a9c0d4" font-family="Consolas, monospace" font-size="12">ORDER</text><text x="104" y="205" fill="#a9c0d4" font-family="Consolas, monospace" font-size="12">REPRESENTATION / ENTITY</text><text x="450" y="205" fill="#a9c0d4" font-family="Consolas, monospace" font-size="12">GEOMETRY / MATERIAL</text><text x="730" y="205" fill="#a9c0d4" font-family="Consolas, monospace" font-size="12">INDEX RANGE</text>""");

for (var i = 0; i < frame.Commands.Count; i++)
{
    var command = frame.Commands[i];
    var y = 220 + i * 84;
    var (accent, glyph) = command.Representation switch
    {
        RendererRepresentation.Semantic => ("#b9c6d4", "·"),
        RendererRepresentation.Aggregate => ("#55d6c2", "◎"),
        RendererRepresentation.Distant => ("#6eb6ff", "◇"),
        RendererRepresentation.Near => ("#f5bb62", "⬡"),
        RendererRepresentation.Interactive => ("#ffde78", "✦"),
        _ => ("#ffffff", "?")
    };
    svg.AppendLine($"""  <rect x="36" y="{y}" width="1008" height="70" rx="10" fill="#101f2d" stroke="#294257"/>""");
    svg.AppendLine($"""  <rect x="36" y="{y}" width="5" height="70" rx="2" fill="{accent}"/>""");
    svg.AppendLine($"""  <text x="57" y="{y + 40}" fill="{accent}" font-family="Segoe UI Symbol, Segoe UI, sans-serif" font-size="24">{glyph}</text>""");
    svg.AppendLine($"""  <text x="104" y="{y + 29}" fill="#f4f8ff" font-family="Consolas, monospace" font-size="13">{Esc(command.Representation.ToString().ToUpperInvariant())}</text>""");
    svg.AppendLine($"""  <text x="104" y="{y + 50}" fill="#91a9bd" font-family="Consolas, monospace" font-size="11">entity {Id(command.Entity.Value)}</text>""");
    svg.AppendLine($"""  <text x="450" y="{y + 29}" fill="#d9e7f3" font-family="Consolas, monospace" font-size="13">geo {Id(command.Geometry.Value)}</text>""");
    svg.AppendLine($"""  <text x="450" y="{y + 50}" fill="#91a9bd" font-family="Consolas, monospace" font-size="11">material {Id(command.Material.Value)}</text>""");
    svg.AppendLine($"""  <text x="730" y="{y + 29}" fill="#d9e7f3" font-family="Consolas, monospace" font-size="13">first {command.FirstIndex} · count {command.IndexCount}</text>""");
    svg.AppendLine($"""  <text x="730" y="{y + 50}" fill="#91a9bd" font-family="Consolas, monospace" font-size="11">instances {command.InstanceCount} · base {command.BaseVertex}</text>""");
}

svg.AppendLine("""  <path d="M36 490 H1044" stroke="#294257"/>""");
svg.AppendLine("""  <text x="36" y="516" fill="#c4d3e1" font-family="Segoe UI, sans-serif" font-size="12">This diagram is built from RenderFrame.View and RenderFrame.Commands at runtime.</text>""");
svg.AppendLine("""  <text x="36" y="538" fill="#7f9ab1" font-family="Consolas, monospace" font-size="11">CPU-side contract proof · no GPU, driver, swapchain, or rendered pixels are implied.</text>""");
svg.AppendLine("</svg>");

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(outputPath, svg.ToString(), new UTF8Encoding(false));
Console.WriteLine($"Wrote {outputPath} from RenderFrame with {frame.Commands.Count} ordered draw commands.");

static string F(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);
static string Id(ulong value) => value.ToString(CultureInfo.InvariantCulture);
static string Esc(string value) => SecurityElement.Escape(value) ?? value;
