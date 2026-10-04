using TheSingularityWorkshop.ProtocolAi;

namespace TheSingularityWorkshop.Renderer;

/// <summary>
/// Describes a rendered form through protocol-defined semantic anchors rather than
/// renderer-owned vocabulary.
/// </summary>
/// <remarks>
/// This allows a form, character, garment, equipment set, or other representation to
/// expose stable semantic attachment points without teaching the Renderer what each
/// word means. ProtocolAi supplies the deterministic identity; the Renderer supplies
/// the spatial relationship.
/// </remarks>
public sealed class SemanticForm
{
    private readonly IReadOnlyList<SemanticAnchor> _anchors;

    /// <summary>
    /// Creates a form from its semantic anchors.
    /// </summary>
    public SemanticForm(IEnumerable<SemanticAnchor> anchors)
    {
        ArgumentNullException.ThrowIfNull(anchors);

        var values = anchors.ToArray();
        if (values.Length == 0)
            throw new ArgumentException("At least one semantic anchor is required.", nameof(anchors));

        if (values.Select(x => x.Reference).Distinct().Count() != values.Length)
            throw new ArgumentException("A semantic form cannot define the same protocol reference more than once.", nameof(anchors));

        _anchors = Array.AsReadOnly(values);
    }

    /// <summary>Semantic anchors exposed by this form.</summary>
    public IReadOnlyList<SemanticAnchor> Anchors => _anchors;

    /// <summary>
    /// Finds an anchor by its protocol-qualified semantic reference.
    /// </summary>
    public bool TryGetAnchor(ProtocolReference reference, out SemanticAnchor anchor)
    {
        foreach (var candidate in _anchors)
        {
            if (candidate.Reference == reference)
            {
                anchor = candidate;
                return true;
            }
        }

        anchor = default;
        return false;
    }
}
