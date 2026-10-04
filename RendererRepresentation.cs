namespace TheSingularityWorkshop.Renderer;

/// <summary>
/// Describes the conceptual kind of representation selected for an observable entity.
/// </summary>
public enum RendererRepresentation
{
    /// <summary>Only semantic existence is required.</summary>
    Semantic = 0,

    /// <summary>An aggregate environmental representation is sufficient.</summary>
    Aggregate = 1,

    /// <summary>A distant visual representation is sufficient.</summary>
    Distant = 2,

    /// <summary>A near-field representation is required.</summary>
    Near = 3,

    /// <summary>Interactive detail is required.</summary>
    Interactive = 4
}
