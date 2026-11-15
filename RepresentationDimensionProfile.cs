namespace TheSingularityWorkshop.Renderer;

/// <summary>
/// Relative fidelity for one independently controlled representation dimension.
/// </summary>
public enum RepresentationDetailLevel
{
    /// <summary>The dimension is omitted from this representation.</summary>
    Omitted = 0,
    /// <summary>Only a compact summary is retained.</summary>
    Summary = 1,
    /// <summary>A reduced but structured representation is retained.</summary>
    Reduced = 2,
    /// <summary>The highest fidelity currently supported by the source is retained.</summary>
    Full = 3
}

/// <summary>
/// Residency strategy for derived presentation data.
/// </summary>
public enum RepresentationResidency
{
    /// <summary>Do not retain a resident copy; reconstruct when requested.</summary>
    ReconstructOnDemand = 0,
    /// <summary>Keep data in a streamable or compressed tier.</summary>
    Streamed = 1,
    /// <summary>Keep data resident for immediate use.</summary>
    Resident = 2
}

/// <summary>
/// Describes an independently selectable set of Event Horizon representation dimensions.
/// </summary>
/// <remarks>
/// This profile describes presentation fidelity only. It does not alter authoritative domain
/// updates, source identity, or source revision. A zero update cadence means event-driven updates.
/// The profile is backend-neutral and does not imply that a corresponding Visualize/GPU path exists.
/// </remarks>
public sealed record RepresentationDimensionProfile
{
    /// <summary>Creates a multidimensional representation profile.</summary>
    public RepresentationDimensionProfile(
        RepresentationDetailLevel geometry,
        RepresentationDetailLevel volume,
        RepresentationDetailLevel particles,
        RepresentationDetailLevel animation,
        double updateCadenceHz,
        RepresentationResidency residency,
        RepresentationDetailLevel interactionPrecision,
        RepresentationDetailLevel semanticDetail,
        RepresentationDetailLevel history)
    {
        ValidateDetail(geometry, nameof(geometry));
        ValidateDetail(volume, nameof(volume));
        ValidateDetail(particles, nameof(particles));
        ValidateDetail(animation, nameof(animation));
        ValidateDetail(interactionPrecision, nameof(interactionPrecision));
        ValidateDetail(semanticDetail, nameof(semanticDetail));
        ValidateDetail(history, nameof(history));

        if (!Enum.IsDefined(residency))
            throw new ArgumentOutOfRangeException(nameof(residency));
        if (!double.IsFinite(updateCadenceHz) || updateCadenceHz < 0)
            throw new ArgumentOutOfRangeException(nameof(updateCadenceHz));

        Geometry = geometry;
        Volume = volume;
        Particles = particles;
        Animation = animation;
        UpdateCadenceHz = updateCadenceHz;
        Residency = residency;
        InteractionPrecision = interactionPrecision;
        SemanticDetail = semanticDetail;
        History = history;
    }

    /// <summary>Gets geometry fidelity.</summary>
    public RepresentationDetailLevel Geometry { get; }
    /// <summary>Gets volumetric fidelity.</summary>
    public RepresentationDetailLevel Volume { get; }
    /// <summary>Gets particle fidelity.</summary>
    public RepresentationDetailLevel Particles { get; }
    /// <summary>Gets animation fidelity.</summary>
    public RepresentationDetailLevel Animation { get; }
    /// <summary>Gets the requested update cadence; zero denotes event-driven updates.</summary>
    public double UpdateCadenceHz { get; }
    /// <summary>Gets the residency strategy.</summary>
    public RepresentationResidency Residency { get; }
    /// <summary>Gets interaction precision.</summary>
    public RepresentationDetailLevel InteractionPrecision { get; }
    /// <summary>Gets semantic information fidelity.</summary>
    public RepresentationDetailLevel SemanticDetail { get; }
    /// <summary>Gets retained history fidelity.</summary>
    public RepresentationDetailLevel History { get; }

    /// <summary>
    /// Returns whether this profile preserves at least the requested fidelity in every dimension.
    /// </summary>
    public bool MeetsOrExceeds(RepresentationDimensionProfile minimum) =>
        minimum is not null &&
        Geometry >= minimum.Geometry &&
        Volume >= minimum.Volume &&
        Particles >= minimum.Particles &&
        Animation >= minimum.Animation &&
        UpdateCadenceHz >= minimum.UpdateCadenceHz &&
        Residency >= minimum.Residency &&
        InteractionPrecision >= minimum.InteractionPrecision &&
        SemanticDetail >= minimum.SemanticDetail &&
        History >= minimum.History;

    private static void ValidateDetail(RepresentationDetailLevel value, string parameterName)
    {
        if (!Enum.IsDefined(value))
            throw new ArgumentOutOfRangeException(parameterName);
    }
}
