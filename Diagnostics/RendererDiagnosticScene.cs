namespace TheSingularityWorkshop.Renderer.Diagnostics;

/// <summary>
/// Produces a deterministic, host-neutral command workload for desktop and browser renderer probes.
/// This is diagnostic content, not an authoritative world model or production city scene.
/// </summary>
public static class RendererDiagnosticScene
{
    /// <summary>The number of alternative representations supplied for each entity.</summary>
    public const int RepresentationVariantCount = 5;

    /// <summary>Creates a repeatable grid of semantic entities, each with all five representation candidates.</summary>
    /// <param name="entityCount">Number of stable diagnostic entities to generate.</param>
    /// <returns>Commands ordered by entity, then from semantic through interactive detail.</returns>
    public static IReadOnlyList<RenderDrawCommand> CreateCommands(int entityCount = 1_000)
    {
        if (entityCount < 1)
            throw new ArgumentOutOfRangeException(nameof(entityCount));

        var commands = new List<RenderDrawCommand>(checked(entityCount * RepresentationVariantCount));
        const int columns = 40;

        for (var index = 0; index < entityCount; index++)
        {
            var column = index % columns;
            var row = index / columns;
            var x = (column / (double)(columns - 1) * 2d) - 1d;
            var y = (row / (double)Math.Max(1, (int)Math.Ceiling(entityCount / (double)columns) - 1) * 2d) - 1d;
            var entity = new RenderEntityId((ulong)index + 1UL);
            var transform = System.Numerics.Matrix4x4.CreateTranslation((float)x, (float)y, 0f);

            for (var detail = 0; detail < RepresentationVariantCount; detail++)
            {
                var representation = (RendererRepresentation)detail;
                var resource = new RenderResourceId((ulong)detail + 1UL);
                commands.Add(new RenderDrawCommand(
                    geometry: resource,
                    material: resource,
                    worldTransform: transform,
                    indexCount: representation switch
                    {
                        RendererRepresentation.Semantic => 1,
                        RendererRepresentation.Aggregate => 2,
                        RendererRepresentation.Distant => 3,
                        RendererRepresentation.Near => 6,
                        RendererRepresentation.Interactive => 12,
                        _ => throw new InvalidOperationException("Unexpected diagnostic representation.")
                    },
                    firstIndex: 0,
                    baseVertex: 0,
                    representation: representation,
                    entity: entity));
            }
        }

        return commands.AsReadOnly();
    }
}
