using TheSingularityWorkshop.ProtocolAi;

namespace TheSingularityWorkshop.Renderer;

/// <summary>
/// Associates an integer-backed ProtocolAi semantic symbol with a normalized spatial
/// anchor in a rendered form.
/// </summary>
/// <remarks>
/// Renderer does not define the vocabulary. A consuming experience may define symbols
/// such as anatomical regions, clothing attachment points, equipment sockets, or other
/// semantic regions and refer to them through <see cref="ProtocolReference"/>.
/// </remarks>
public readonly record struct SemanticAnchor
{
    public SemanticAnchor(ProtocolReference reference, double x, double y, double z)
    {
        if (reference.ProtocolId == 0 || reference.SymbolId == 0)
            throw new ArgumentException("A semantic anchor requires a valid protocol reference.", nameof(reference));

        if (!double.IsFinite(x) || !double.IsFinite(y) || !double.IsFinite(z))
            throw new ArgumentException("Anchor coordinates must be finite.");

        Reference = reference;
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>Protocol-qualified semantic identity of the anchor.</summary>
    public ProtocolReference Reference { get; }

    /// <summary>Normalized local X coordinate.</summary>
    public double X { get; }

    /// <summary>Normalized local Y coordinate.</summary>
    public double Y { get; }

    /// <summary>Normalized local Z coordinate.</summary>
    public double Z { get; }
}
