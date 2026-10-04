using System.Numerics;

namespace TheSingularityWorkshop.Renderer;

/// <summary>Describes observer-relative view state required to produce a render frame.</summary>
public readonly record struct RenderView
{
    /// <summary>Initializes a new view.</summary>
    public RenderView(Vector3 observerPosition, Matrix4x4 view, Matrix4x4 projection)
    {
        if (!float.IsFinite(observerPosition.X) || !float.IsFinite(observerPosition.Y) || !float.IsFinite(observerPosition.Z))
            throw new ArgumentOutOfRangeException(nameof(observerPosition));

        ObserverPosition = observerPosition;
        View = view;
        Projection = projection;
    }

    /// <summary>Gets the observer position in world coordinates.</summary>
    public Vector3 ObserverPosition { get; }

    /// <summary>Gets the world-to-view transform.</summary>
    public Matrix4x4 View { get; }

    /// <summary>Gets the view-to-clip transform.</summary>
    public Matrix4x4 Projection { get; }
}
