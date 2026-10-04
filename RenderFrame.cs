using System.Collections.Generic;

namespace TheSingularityWorkshop.Renderer;

/// <summary>The platform-neutral result of observer-relative rendering decisions for one frame.</summary>
public sealed class RenderFrame
{
    /// <summary>Creates a frame from a view and its draw commands.</summary>
    public RenderFrame(RenderView view, IEnumerable<RenderDrawCommand> commands)
    {
        ArgumentNullException.ThrowIfNull(commands);
        View = view;
        Commands = commands is RenderDrawCommand[] array ? array : [.. commands];
    }

    /// <summary>Gets the observer-relative view.</summary>
    public RenderView View { get; }
    /// <summary>Gets the command snapshot selected for this frame.</summary>
    public IReadOnlyList<RenderDrawCommand> Commands { get; }
}
