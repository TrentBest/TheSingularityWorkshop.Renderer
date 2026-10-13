namespace TheSingularityWorkshop.Renderer;

/// <summary>
/// A normalized observer-relative rectangle. X/Y and width/height are expressed in
/// projection-space units; the type deliberately does not prescribe pixels or a graphics API.
/// </summary>
/// <param name="X">Left edge.</param>
/// <param name="Y">Top edge.</param>
/// <param name="Width">Rectangle width.</param>
/// <param name="Height">Rectangle height.</param>
public readonly record struct ProjectedRect(double X, double Y, double Width, double Height)
{
    /// <summary>Whether all coordinates are finite and dimensions are positive.</summary>
    public bool IsValid =>
        double.IsFinite(X) &&
        double.IsFinite(Y) &&
        double.IsFinite(Width) &&
        double.IsFinite(Height) &&
        Width > 0 &&
        Height > 0;

    /// <summary>Area in the rectangle's projection-space units.</summary>
    public double Area => IsValid ? Width * Height : 0;

    /// <summary>Whether this rectangle has a positive-area intersection with another.</summary>
    public bool Overlaps(ProjectedRect other) =>
        IsValid &&
        other.IsValid &&
        X < other.X + other.Width &&
        X + Width > other.X &&
        Y < other.Y + other.Height &&
        Y + Height > other.Y;

    /// <summary>Whether this rectangle fully contains another, including shared edges.</summary>
    public bool Contains(ProjectedRect other) =>
        IsValid &&
        other.IsValid &&
        X <= other.X &&
        Y <= other.Y &&
        X + Width >= other.X + other.Width &&
        Y + Height >= other.Y + other.Height;

    /// <summary>Area of the intersection, or zero when the rectangles do not overlap.</summary>
    public double IntersectionArea(ProjectedRect other)
    {
        if (!Overlaps(other))
            return 0;

        var left = Math.Max(X, other.X);
        var top = Math.Max(Y, other.Y);
        var right = Math.Min(X + Width, other.X + other.Width);
        var bottom = Math.Min(Y + Height, other.Y + other.Height);
        return (right - left) * (bottom - top);
    }
}
