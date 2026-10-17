namespace TheSingularityWorkshop.Renderer;

/// <summary>Counts the work represented by one observer-relative command reduction.</summary>
/// <param name="InputCommands">Commands considered by the reducer.</param>
/// <param name="OutputCommands">Commands retained for the requested detail ceiling.</param>
/// <param name="OmittedCommands">Commands excluded because they belong to a more detailed representation.</param>
/// <param name="EntitiesConsidered">Distinct assigned semantic entity IDs encountered.</param>
/// <param name="EntitiesRepresented">Distinct assigned semantic entity IDs with retained commands.</param>
/// <param name="UnidentifiedCommandsPreserved">Unidentified commands retained without entity-level variant selection.</param>
public readonly record struct RenderCommandReductionMetrics(
    int InputCommands,
    int OutputCommands,
    int OmittedCommands,
    int EntitiesConsidered,
    int EntitiesRepresented,
    int UnidentifiedCommandsPreserved);

/// <summary>The selected commands and explicit accounting from a representation reduction pass.</summary>
public sealed class RenderCommandReductionResult
{
    internal RenderCommandReductionResult(
        IReadOnlyList<RenderDrawCommand> commands,
        RenderCommandReductionMetrics metrics)
    {
        Commands = commands;
        Metrics = metrics;
    }

    /// <summary>Gets the ordered commands selected for presentation.</summary>
    public IReadOnlyList<RenderDrawCommand> Commands { get; }

    /// <summary>Gets deterministic command-count metrics for this pass.</summary>
    public RenderCommandReductionMetrics Metrics { get; }
}

/// <summary>
/// Selects the richest available representation at or below an observer's detail ceiling for each
/// identified entity, retaining all commands belonging to that selected representation.
/// </summary>
/// <remarks>
/// This is a command-selection stage, not geometry aggregation, visibility culling, a scheduler, or a
/// backend. It produces real command-count reduction only when the input contains alternative
/// representations for the same entity or commands above the requested ceiling. Unidentified commands
/// are handled independently because grouping them would risk merging unrelated subjects. The caller
/// owns stable source ordering; ties at the same entity and representation preserve every command in
/// their original order, supporting multi-part representations.
/// </remarks>
public static class RenderCommandReducer
{
    /// <summary>Reduces candidate commands to representations no more detailed than the requested ceiling.</summary>
    /// <param name="commands">Candidate commands, potentially containing multiple representation variants per entity.</param>
    /// <param name="maximumDetail">The richest representation permitted in the result.</param>
    public static RenderCommandReductionResult Reduce(
        IEnumerable<RenderDrawCommand> commands,
        RendererRepresentation maximumDetail)
    {
        ArgumentNullException.ThrowIfNull(commands);
        if (!Enum.IsDefined(maximumDetail))
            throw new ArgumentOutOfRangeException(nameof(maximumDetail));

        var input = commands.ToArray();
        var selectedByEntity = new Dictionary<RenderEntityId, RendererRepresentation>();
        var unidentified = new List<RenderDrawCommand>();

        foreach (var command in input)
        {
            if (!command.Entity.IsValid)
            {
                if (command.Representation <= maximumDetail)
                    unidentified.Add(command);
                continue;
            }

            if (command.Representation > maximumDetail)
                continue;

            if (!selectedByEntity.TryGetValue(command.Entity, out var selected) ||
                command.Representation > selected)
            {
                selectedByEntity[command.Entity] = command.Representation;
            }
        }

        var output = new List<RenderDrawCommand>(input.Length);
        var representedEntities = new HashSet<RenderEntityId>();
        foreach (var command in input)
        {
            if (!command.Entity.IsValid)
            {
                if (command.Representation <= maximumDetail)
                    output.Add(command);
                continue;
            }

            if (selectedByEntity.TryGetValue(command.Entity, out var selected) &&
                command.Representation == selected)
            {
                output.Add(command);
                representedEntities.Add(command.Entity);
            }
        }

        var identifiedCount = input
            .Where(command => command.Entity.IsValid)
            .Select(command => command.Entity)
            .Distinct()
            .Count();
        var metrics = new RenderCommandReductionMetrics(
            input.Length,
            output.Count,
            input.Length - output.Count,
            identifiedCount,
            representedEntities.Count,
            unidentified.Count);

        return new RenderCommandReductionResult(output.AsReadOnly(), metrics);
    }
}
