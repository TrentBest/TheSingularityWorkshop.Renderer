using System.Numerics;
using System.Security;
using System.Text;
using System.Text.Json;
using TheSingularityWorkshop.Renderer;

var outputDirectory = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.GetFullPath("artifacts/blacksmith-semantic-proof");
Directory.CreateDirectory(outputDirectory);

var world = BlacksmithWorld.Create();
var assistant = new ObserverPolicy("WorkshopAssistant", new HashSet<ulong> { 1, 2, 3, 4, 5, 6, 7 }, "Operational + maintenance channels");
var player = new ObserverPolicy("Player", new HashSet<ulong> { 1, 2, 3, 4, 5, 6 }, "Direct line of sight");
var playerView = new RenderView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity);

var trace = new List<RevisionTrace>();
Capture("Initial", world.Snapshot());
world.Apply(new WorldEvent("HammerStrike", 3, "Being shaped by the blacksmith"));
Require(world.Snapshot().Entities.Single(x => x.Id == 3).State == "Freshly struck; heated", "Hammer strike must change authoritative workpiece state.");
Capture("HammerStrike", world.Snapshot());
world.Apply(new WorldEvent("Quench", 3, "Quenched; cooling"));
Capture("Quench", world.Snapshot());

var tracePath = Path.Combine(outputDirectory, "blacksmith-observation-trace.json");
var svgPath = Path.Combine(outputDirectory, "blacksmith-player-view.svg");
File.WriteAllText(tracePath, JsonSerializer.Serialize(trace, new JsonSerializerOptions { WriteIndented = true }));
File.WriteAllText(svgPath, BuildSvg(world.Snapshot(), BuildFrame(world.Snapshot(), player, playerView)), new UTF8Encoding(false));

Console.WriteLine($"PASS: {trace.Count} revisions projected for two observers.");
Console.WriteLine($"PASS: assistant sees fuel-valve state; player does not receive it.");
Console.WriteLine($"PASS: locked-cabinet contents remain unknown to both observers.");
Console.WriteLine($"PASS: player RenderFrame tracks the same authoritative revision as semantic observations.");
Console.WriteLine($"Wrote {tracePath}");
Console.WriteLine($"Wrote {svgPath}");

void Capture(string eventName, WorldSnapshot snapshot)
{
    var assistantObservation = Observe(snapshot, assistant);
    var playerObservation = Observe(snapshot, player);
    var frame = BuildFrame(snapshot, player, playerView);

    Require(assistantObservation.SourceRevision == snapshot.Revision, "Assistant observation revision mismatch.");
    Require(playerObservation.SourceRevision == snapshot.Revision, "Player observation revision mismatch.");
    Require(frame.Commands.Count == playerObservation.Entities.Count, "Visual commands and visible semantic entities diverged.");
    Require(assistantObservation.Entities.Any(x => x.Entity.Value == 7), "Assistant should have maintenance-channel access to the fuel valve.");
    Require(playerObservation.Entities.All(x => x.Entity.Value != 7), "Player must not receive hidden fuel-valve state.");
    Require(assistantObservation.Entities.All(x => x.Entity.Value != 8), "Assistant must not receive the locked cabinet identity.");
    Require(playerObservation.Entities.All(x => x.Entity.Value != 8), "Player must not receive the locked cabinet identity.");
    Require(assistantObservation.Relations.All(x => x.Subject.Value != 8 && x.Target.Value != 8), "Relations must not disclose unavailable entity identities.");
    Require(playerObservation.Relations.All(x => x.Subject.Value != 8 && x.Target.Value != 8), "Relations must not disclose unavailable entity identities.");

    trace.Add(new RevisionTrace(eventName, snapshot.Revision, assistantObservation, playerObservation,
        frame.Commands.Select(x => new DrawCommandTrace(x.Entity.Value, x.Geometry.Value, x.Material.Value, x.IndexCount, x.Representation.ToString())).ToArray()));
}

static TheSingularityWorkshop.Renderer.SemanticObservation Observe(WorldSnapshot snapshot, ObserverPolicy observer)
{
    var permitted = snapshot.Entities
        .Where(entity => observer.PermittedEntityIds.Contains(entity.Id))
        .ToArray();
    var ids = permitted.Select(entity => new RenderEntityId(entity.Id)).ToHashSet();
    var provenance = new SemanticProvenance("AuthoritativeBlacksmithWorld", snapshot.Revision);

    var entities = permitted.Select(entity =>
    {
        var knowledge = entity.Id == 7 ? SemanticKnowledge.Sensed : SemanticKnowledge.Observed;
        var confidence = knowledge == SemanticKnowledge.Sensed ? 0.98 : 1.0;
        var affordances = entity.Affordances.Select(value => new SemanticAffordance(value, true));
        return new SemanticEntityObservation(
            new RenderEntityId(entity.Id), entity.Category, entity.State, entity.Activity,
            entity.Hazard, knowledge, confidence, provenance, affordances);
    }).ToArray();

    var relations = new[]
    {
        (Subject: 1UL, Relation: "Heats", Target: 3UL),
        (Subject: 2UL, Relation: "Uses", Target: 4UL),
        (Subject: 3UL, Relation: "CanBeQuenchedIn", Target: 6UL)
    }
    .Where(relation => ids.Contains(new RenderEntityId(relation.Subject)) && ids.Contains(new RenderEntityId(relation.Target)))
    .Select(relation => new SemanticRelationObservation(
        new RenderEntityId(relation.Subject), relation.Relation, new RenderEntityId(relation.Target),
        SemanticKnowledge.Observed, 1.0, provenance))
    .ToArray();

    return new TheSingularityWorkshop.Renderer.SemanticObservation(
        observer.Name, snapshot.Revision, SemanticObservationFreshness.Current, entities, relations);
}

static RenderFrame BuildFrame(WorldSnapshot snapshot, ObserverPolicy observer, RenderView view)
{
    var commands = snapshot.Entities
        .Where(entity => observer.PermittedEntityIds.Contains(entity.Id) && entity.VisibleToPlayer)
        .Select(entity => new RenderDrawCommand(
            new RenderResourceId((ulong)(100 + entity.Id)),
            new RenderResourceId(entity.Id == 3 ? entity.State.StartsWith("Quenched", StringComparison.Ordinal) ? 303UL : entity.State.StartsWith("Freshly struck", StringComparison.Ordinal) ? 302UL : 301UL : (ulong)(200 + entity.Id)),
            Matrix4x4.CreateTranslation(entity.Position),
            entity.Id == 3 ? 96 : 36,
            0,
            0,
            entity.Id == 3 ? RendererRepresentation.Interactive : RendererRepresentation.Near,
            entity: new RenderEntityId(entity.Id)))
        .ToArray();
    return new RenderFrame(view, commands);
}

static string BuildSvg(WorldSnapshot snapshot, RenderFrame frame)
{
    var entities = snapshot.Entities.Where(x => x.VisibleToPlayer).ToDictionary(x => x.Id);
    var sb = new StringBuilder();
    sb.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
    sb.AppendLine("""<svg xmlns="http://www.w3.org/2000/svg" width="1000" height="560" viewBox="0 0 1000 560" role="img" aria-labelledby="title desc">""");
    sb.AppendLine("""<title id="title">Blacksmith shop — player-facing Renderer frame</title>""");
    sb.AppendLine($"""<desc id="desc">Diagnostic SVG composed from {frame.Commands.Count} actual RenderDrawCommand entries at authoritative world revision {snapshot.Revision}. It is not GPU output.</desc>""");
    sb.AppendLine("""<rect width="1000" height="560" fill="#171c24"/><rect x="28" y="28" width="944" height="504" rx="18" fill="#292b30" stroke="#59616c" stroke-width="2"/>""");
    sb.AppendLine("""<text x="52" y="66" fill="#f4e7d2" font-family="Segoe UI,sans-serif" font-size="24" font-weight="700">THE BLACKSMITH'S SHOP</text>""");
    sb.AppendLine($"""<text x="948" y="66" text-anchor="end" fill="#aab6c4" font-family="Consolas,monospace" font-size="13">WORLD REVISION {snapshot.Revision} · {frame.Commands.Count} DRAW COMMANDS</text>""");
    sb.AppendLine("""<rect x="76" y="130" width="260" height="230" rx="12" fill="#3b3029" stroke="#9b6743" stroke-width="3"/><path d="M120 310 Q160 170 210 300 Q260 180 300 310" fill="none" stroke="#f08a38" stroke-width="18" stroke-linecap="round"/><path d="M150 300 Q190 220 225 300" fill="none" stroke="#ffd36b" stroke-width="9" stroke-linecap="round"/>""");
    sb.AppendLine("""<text x="206" y="390" text-anchor="middle" fill="#f3c28c" font-family="Segoe UI,sans-serif" font-size="16">FORGE · ACTIVE</text>""");
    sb.AppendLine("""<rect x="395" y="310" width="240" height="48" rx="8" fill="#5a6068" stroke="#c0c7cf" stroke-width="3"/><path d="M430 310 L455 285 L580 285 L605 310" fill="#737b85" stroke="#c0c7cf" stroke-width="3"/>""");
    var workpiece = entities[3];
    var quenched = workpiece.State.StartsWith("Quenched", StringComparison.Ordinal);
    var freshlyStruck = workpiece.State.StartsWith("Freshly struck", StringComparison.Ordinal);
    sb.AppendLine($"""<path d="M470 272 L570 272" stroke="{(quenched ? "#7eb7d8" : "#ff8b36")}" stroke-width="16" stroke-linecap="round"/><text x="515" y="420" text-anchor="middle" fill="{(quenched ? "#9bd3ef" : "#ffb06b")}" font-family="Segoe UI,sans-serif" font-size="15">{SecurityElement.Escape(workpiece.State.ToUpperInvariant())}</text>""");
    if (freshlyStruck)
        sb.AppendLine("""<g stroke="#ffd36b" stroke-width="4" stroke-linecap="round"><path d="M520 245 l-10 -18 M535 248 l12 -17 M505 250 l-17 -4 M535 260 l18 6"/></g><text x="520" y="220" text-anchor="middle" fill="#ffd36b" font-family="Segoe UI,sans-serif" font-size="12">SPARKS</text>""");
    sb.AppendLine("""<circle cx="740" cy="330" r="66" fill="#31566b" stroke="#8ab6cb" stroke-width="4"/><path d="M690 330 Q740 310 790 330" stroke="#b7e4f3" stroke-width="4" fill="none"/><text x="740" y="420" text-anchor="middle" fill="#c4dce8" font-family="Segoe UI,sans-serif" font-size="15">QUENCH BARREL</text>""");
    sb.AppendLine("""<path d="M420 200 L475 145 L495 165 L440 220 Z" fill="#9da4aa" stroke="#e0e4e8" stroke-width="3"/><path d="M400 220 L445 175" stroke="#9a6843" stroke-width="13" stroke-linecap="round"/><text x="450" y="125" fill="#e5d7c7" font-family="Segoe UI,sans-serif" font-size="14">HAMMER</text>""");
    sb.AppendLine("""<text x="52" y="492" fill="#9eacba" font-family="Consolas,monospace" font-size="12">CPU-side SVG diagnostic · same source snapshot as semantic trace · not a pixel/GPU renderer</text>""");
    sb.AppendLine("</svg>");
    return sb.ToString();
}

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

sealed record ObserverPolicy(string Name, HashSet<ulong> PermittedEntityIds, string Channels);
sealed record WorldEvent(string Kind, ulong TargetEntityId, string ResultingActivity);
sealed record WorldEntity(ulong Id, string StableId, string Category, string State, string Activity, string Hazard, string[] Affordances, Vector3 Position, bool VisibleToPlayer);
sealed record WorldSnapshot(long Revision, IReadOnlyList<WorldEntity> Entities);
sealed record DrawCommandTrace(ulong EntityId, ulong GeometryId, ulong MaterialId, int IndexCount, string Representation);
sealed record RevisionTrace(string Event, long WorldRevision, TheSingularityWorkshop.Renderer.SemanticObservation Assistant, TheSingularityWorkshop.Renderer.SemanticObservation Player, IReadOnlyList<DrawCommandTrace> PlayerDrawCommands);

sealed class BlacksmithWorld
{
    private readonly List<WorldEntity> _entities;
    private long _revision;

    private BlacksmithWorld(List<WorldEntity> entities) => _entities = entities;

    public static BlacksmithWorld Create() => new(new List<WorldEntity>
    {
        new(1, "forge-01", "Forge", "Active", "Burning", "HighTemperature", new[] { "Heat" }, new Vector3(-3, 0, 0), true),
        new(2, "smith-01", "Person.Blacksmith", "Present", "Forging", "None", new[] { "Hammer", "ShapeMetal" }, new Vector3(0, 0, 0), true),
        new(3, "workpiece-01", "MetalWorkpiece", "Heated", "WaitingForStrike", "HotSurface", new[] { "Hammer", "Quench" }, new Vector3(1, 0, 0), true),
        new(4, "hammer-01", "Tool.Hammer", "Available", "OnWorkBench", "ImpactTool", new[] { "Strike" }, new Vector3(2, 0, 0), true),
        new(5, "tongs-01", "Tool.Tongs", "Available", "OnWorkBench", "None", new[] { "GripHotMetal" }, new Vector3(2, 1, 0), true),
        new(6, "quench-barrel-01", "WaterBarrel", "Ready", "Idle", "WaterContainer", new[] { "Quench" }, new Vector3(4, 0, 0), true),
        new(7, "fuel-valve-01", "FuelValve", "Open", "SupplyingForge", "FuelSystem", new[] { "ShutOff" }, new Vector3(-5, 1, 0), false),
        new(8, "locked-cabinet-01", "StorageCabinet", "Locked", "Closed", "None", Array.Empty<string>(), new Vector3(5, 3, 0), false)
    });

    public WorldSnapshot Snapshot() => new(_revision, _entities.ToArray());

    public void Apply(WorldEvent worldEvent)
    {
        var index = _entities.FindIndex(entity => entity.Id == worldEvent.TargetEntityId);
        if (index < 0) throw new InvalidOperationException($"Unknown target entity {worldEvent.TargetEntityId}.");
        var current = _entities[index];
        var state = worldEvent.Kind switch
        {
            "HammerStrike" => "Freshly struck; heated",
            "Quench" => "Quenched; cooling",
            _ => throw new ArgumentOutOfRangeException(nameof(worldEvent), worldEvent.Kind, "Unsupported fixture event.")
        };
        _entities[index] = current with { State = state, Activity = worldEvent.ResultingActivity };
        _revision++;
    }
}
