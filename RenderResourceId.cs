namespace TheSingularityWorkshop.Renderer;

/// <summary>Identifies a renderer-managed resource without exposing a graphics API handle.</summary>
/// <param name="Value">The stable resource identifier.</param>
public readonly record struct RenderResourceId(ulong Value)
{
    /// <summary>Gets a value indicating whether the identifier is non-zero.</summary>
    public bool IsValid => Value != 0;
}
