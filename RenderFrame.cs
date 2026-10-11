using System.Collections.Generic;

namespace TheSingularityWorkshop.Renderer;

/// <summary>The platform-neutral result of observer-relative rendering decisions for one frame.</summary>
public sealed class RenderFrame
{
    /// <summary>Creates a frame from a view and its draw commands.</summary>
    /// <remarks>
    /// The frame owns a stable command snapshot. The source sequence may be reused or mutated by its
    /// producer after construction without changing this frame. Backends should consume the snapshot
    /// directly and avoid making another copy unless their own lifetime or threading contract requires it.
    /// </remarks>
    public RenderFrame(RenderView view, IEnumerable<RenderDrawCommand> commands)
    {
        ArgumentNullException.ThrowIfNull(commands);
        View = view;
        Commands = Array.AsReadOnly(commands.ToArray());
    }

    /// <summary>Gets the observer-relative view.</summary>
    public RenderView View { get; }

    /// <summary>Gets the immutable ordered command snapshot selected for this frame.</summary>
    public IReadOnlyList<RenderDrawCommand> Commands { get; }
}
