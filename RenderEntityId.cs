namespace TheSingularityWorkshop.Renderer;

/// <summary>
/// Identifies a stable semantic subject in a world or scene snapshot.
/// </summary>
/// <remarks>
/// This identity is distinct from geometry and material resource IDs. A subject may change
/// representation while retaining its identity. Aggregate representations should use the
/// stable identity of their aggregate/cohort; they must not pretend one draw command identifies
/// every member represented by that aggregate.
/// </remarks>
public readonly record struct RenderEntityId(ulong Value)
{
    /// <summary>Gets whether this identifier is assigned; zero is reserved as invalid.</summary>
    public bool IsValid => Value != 0;

    /// <summary>Gets the invalid/unassigned identifier.</summary>
    public static RenderEntityId Invalid => default;
}
