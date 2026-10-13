namespace TheSingularityWorkshop.Renderer;

/// <summary>
/// Perception channels that an obstruction may block. These flags describe visibility
/// policy, not the physical laws or fidelity of a particular sensor implementation.
/// </summary>
[Flags]
public enum PerceptionChannel
{
    /// <summary>No perception channel.</summary>
    None = 0,
    /// <summary>Ordinary visible-light presentation.</summary>
    Visible = 1 << 0,
    /// <summary>Thermal or infrared presentation.</summary>
    Thermal = 1 << 1,
    /// <summary>Low-light presentation.</summary>
    LowLight = 1 << 2,
    /// <summary>A future radar-like or material-penetrating presentation.</summary>
    PenetratingSensor = 1 << 3
}

/// <summary>Coarse result for one subject tested against one obstruction rectangle.</summary>
public enum OcclusionOutcome
{
    /// <summary>The obstruction does not overlap, is behind the subject, or does not block this channel.</summary>
    Unobstructed,
    /// <summary>The subject is partially covered by a nearer obstruction.</summary>
    PartiallyOccluded,
    /// <summary>The rectangles overlap, but the obstruction shape is not proven to cover the full subject.</summary>
    OcclusionUncertain,
    /// <summary>An opaque, nearer obstruction is proven to fully cover the subject rectangle.</summary>
    FullyOccluded,
    /// <summary>Required bounds or depth values are invalid.</summary>
    InvalidInput
}

/// <summary>Result of a conservative, rectangle-based obstruction test.</summary>
/// <param name="Outcome">The pairwise occlusion outcome.</param>
/// <param name="SubjectCoverage">Fraction of the subject rectangle overlapped by the obstruction, from 0 to 1.</param>
/// <param name="RenderSubject">Whether this pairwise result permits the subject to be submitted for this channel.</param>
/// <param name="CompositeObstructionAfterSubject">Whether partial coverage should be composited after the subject.</param>
public readonly record struct OcclusionAssessment(
    OcclusionOutcome Outcome,
    double SubjectCoverage,
    bool RenderSubject,
    bool CompositeObstructionAfterSubject);

/// <summary>
/// Performs a cheap observer-relative broad-phase test before detailed visibility work.
/// Depth values use the caller's convention, provided smaller values mean nearer.
/// </summary>
public static class ObstructionVisibility
{
    /// <summary>
    /// Assesses one subject against one obstruction. This is a conservative rectangle test,
    /// not a pixel-perfect depth, silhouette, or multi-obstruction solver.
    /// </summary>
    /// <param name="subjectBounds">Projected rectangle for the subject.</param>
    /// <param name="subjectDepth">Subject depth; smaller means nearer.</param>
    /// <param name="obstructionBounds">Projected rectangle for the obstruction.</param>
    /// <param name="obstructionDepth">Obstruction depth; smaller means nearer.</param>
    /// <param name="channel">Active perception channel.</param>
    /// <param name="blockedChannels">Channels blocked by this obstruction.</param>
    /// <param name="opacity">Obstruction opacity from 0 (clear) to 1 (opaque).</param>
    /// <param name="fullCoverageGuaranteed">
    /// True only when the rectangle represents a known filled occluding surface, not merely
    /// the bounding rectangle of an irregular mesh. Defaults to false to avoid unsafe culling.
    /// </param>
    public static OcclusionAssessment Assess(
        ProjectedRect subjectBounds,
        double subjectDepth,
        ProjectedRect obstructionBounds,
        double obstructionDepth,
        PerceptionChannel channel,
        PerceptionChannel blockedChannels,
        double opacity = 1,
        bool fullCoverageGuaranteed = false)
    {
        if (!subjectBounds.IsValid ||
            !obstructionBounds.IsValid ||
            !double.IsFinite(subjectDepth) ||
            !double.IsFinite(obstructionDepth) ||
            !double.IsFinite(opacity) ||
            opacity < 0 ||
            opacity > 1 ||
            channel == PerceptionChannel.None)
        {
            // Invalid visibility data must never cull an entity. Preserve the subject and let a
            // later stage decide how to handle the incomplete obstruction result.
            return new OcclusionAssessment(OcclusionOutcome.InvalidInput, 0, true, false);
        }

        if (obstructionDepth >= subjectDepth ||
            !obstructionBounds.Overlaps(subjectBounds) ||
            (blockedChannels & channel) == 0 ||
            opacity <= 0)
        {
            return new OcclusionAssessment(OcclusionOutcome.Unobstructed, 0, true, false);
        }

        var coverage = Math.Clamp(
            obstructionBounds.IntersectionArea(subjectBounds) / subjectBounds.Area,
            0,
            1);

        if (opacity >= 1 && obstructionBounds.Contains(subjectBounds))
        {
            if (fullCoverageGuaranteed)
                return new OcclusionAssessment(OcclusionOutcome.FullyOccluded, 1, false, false);

            return new OcclusionAssessment(OcclusionOutcome.OcclusionUncertain, coverage, true, true);
        }

        return new OcclusionAssessment(OcclusionOutcome.PartiallyOccluded, coverage, true, true);
    }
}
