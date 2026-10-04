using TheSingularityWorkshop.FSM_API;
using FSMApi = TheSingularityWorkshop.FSM_API.FSM_API;

namespace TheSingularityWorkshop.Renderer;

/// <summary>
/// Identifies the computational representation currently desired for an observable entity.
/// </summary>
public enum RepresentationTarget
{
    /// <summary>Semantic existence with minimal presentation computation.</summary>
    Semantic = 0,

    /// <summary>Aggregate representation for contextual visibility.</summary>
    Aggregate = 1,

    /// <summary>Distant representation with reduced presentation computation.</summary>
    Distant = 2,

    /// <summary>Near representation with increased presentation computation.</summary>
    Near = 3,

    /// <summary>Interactive representation with the highest currently justified computation.</summary>
    Interactive = 4
}

/// <summary>
/// FSM context carrying the decision state for one renderer computation instance.
/// </summary>
/// <remarks>
/// This is deliberately not a graphics object. The FSM decides computational responsibility;
/// a later presentation layer may consume the resulting state to produce pixels, geometry,
/// audio, haptics, or another observable output.
/// </remarks>
public sealed class RendererComputationContext : IStateContext
{
    /// <summary>
    /// Creates a renderer computation context.
    /// </summary>
    /// <param name="name">Unique context name used by FSM_API.</param>
    /// <param name="desiredRepresentation">Representation justified by the observer and renderer policy.</param>
    public RendererComputationContext(
        string name,
        RepresentationTarget desiredRepresentation = RepresentationTarget.Semantic)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A renderer computation context name is required.", nameof(name));
        }

        Name = name;
        DesiredRepresentation = desiredRepresentation;
        ActiveRepresentation = RepresentationTarget.Semantic;
    }

    /// <summary>Gets or sets the FSM context identity.</summary>
    public string Name { get; set; }

    /// <summary>Gets or sets the representation currently justified by policy.</summary>
    public RepresentationTarget DesiredRepresentation { get; set; }

    /// <summary>Gets the representation whose computation is currently active.</summary>
    public RepresentationTarget ActiveRepresentation { get; internal set; }

    /// <summary>Gets or sets whether this computation context remains active.</summary>
    public bool IsValid { get; set; } = true;
}

/// <summary>
/// Drives renderer representation computation through FSM_API.
/// </summary>
/// <remarks>
/// The FSM is intentionally used at the computational boundary rather than the graphics boundary.
/// Representation changes are therefore explicit state transitions that can later trigger independent
/// geometry, material, animation, simulation, streaming, or interaction work.
/// </remarks>
public sealed class RendererComputationMachine : IDisposable
{
    private const string ProcessingGroup = "Renderer";
    private const string DefinitionName = "Renderer.Representation";

    private static readonly object DefinitionLock = new();
    private readonly RendererComputationContext _context;
    private readonly FSMHandle _handle;
    private bool _disposed;

    /// <summary>
    /// Creates a renderer computation machine for one observable entity.
    /// </summary>
    /// <param name="context">The renderer computation context to manage.</param>
    public RendererComputationMachine(RendererComputationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
        EnsureDefinition();
        _handle = FSMApi.Create.CreateInstance(DefinitionName, _context, ProcessingGroup);
    }

    /// <summary>Gets the computation context managed by this machine.</summary>
    public RendererComputationContext Context => _context;

    /// <summary>Gets the FSM's current representation state.</summary>
    public RepresentationTarget CurrentRepresentation =>
        Enum.TryParse<RepresentationTarget>(_handle.CurrentState, out var result)
            ? result
            : throw new InvalidOperationException($"Unknown renderer representation state '{_handle.CurrentState}'.");

    /// <summary>
    /// Advances renderer computation by one FSM update.
    /// </summary>
    /// <returns>The representation active after the update.</returns>
    public RepresentationTarget Advance()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _handle.Update(ProcessingGroup);
        return CurrentRepresentation;
    }

    /// <summary>Releases the FSM instance owned by this machine.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        FSMApi.Interaction.DestroyInstance(_handle);
        _context.IsValid = false;
        _disposed = true;
    }

    private static void EnsureDefinition()
    {
        lock (DefinitionLock)
        {
            if (FSMApi.Interaction.Exists(DefinitionName, ProcessingGroup))
            {
                return;
            }

            FSMApi.Create.CreateFiniteStateMachine(
                DefinitionName,
                processRate: -1,
                processingGroup: ProcessingGroup)
            .State(
                nameof(RepresentationTarget.Semantic),
                onEnter: null,
                onUpdate: null,
                onExit: null)
            .State(
                nameof(RepresentationTarget.Aggregate),
                onEnter: null,
                onUpdate: null,
                onExit: null)
            .State(
                nameof(RepresentationTarget.Distant),
                onEnter: null,
                onUpdate: null,
                onExit: null)
            .State(
                nameof(RepresentationTarget.Near),
                onEnter: null,
                onUpdate: null,
                onExit: null)
            .State(
                nameof(RepresentationTarget.Interactive),
                onEnter: null,
                onUpdate: null,
                onExit: null)
            .WithInitialState(nameof(RepresentationTarget.Semantic))
            .Transition(nameof(RepresentationTarget.Semantic), nameof(RepresentationTarget.Aggregate),
                context => ShouldTransition(context, RepresentationTarget.Aggregate))
            .Transition(nameof(RepresentationTarget.Semantic), nameof(RepresentationTarget.Distant),
                context => ShouldTransition(context, RepresentationTarget.Distant))
            .Transition(nameof(RepresentationTarget.Semantic), nameof(RepresentationTarget.Near),
                context => ShouldTransition(context, RepresentationTarget.Near))
            .Transition(nameof(RepresentationTarget.Semantic), nameof(RepresentationTarget.Interactive),
                context => ShouldTransition(context, RepresentationTarget.Interactive))
            .Transition(nameof(RepresentationTarget.Aggregate), nameof(RepresentationTarget.Semantic),
                context => ShouldTransition(context, RepresentationTarget.Semantic))
            .Transition(nameof(RepresentationTarget.Aggregate), nameof(RepresentationTarget.Distant),
                context => ShouldTransition(context, RepresentationTarget.Distant))
            .Transition(nameof(RepresentationTarget.Aggregate), nameof(RepresentationTarget.Near),
                context => ShouldTransition(context, RepresentationTarget.Near))
            .Transition(nameof(RepresentationTarget.Aggregate), nameof(RepresentationTarget.Interactive),
                context => ShouldTransition(context, RepresentationTarget.Interactive))
            .Transition(nameof(RepresentationTarget.Distant), nameof(RepresentationTarget.Semantic),
                context => ShouldTransition(context, RepresentationTarget.Semantic))
            .Transition(nameof(RepresentationTarget.Distant), nameof(RepresentationTarget.Aggregate),
                context => ShouldTransition(context, RepresentationTarget.Aggregate))
            .Transition(nameof(RepresentationTarget.Distant), nameof(RepresentationTarget.Near),
                context => ShouldTransition(context, RepresentationTarget.Near))
            .Transition(nameof(RepresentationTarget.Distant), nameof(RepresentationTarget.Interactive),
                context => ShouldTransition(context, RepresentationTarget.Interactive))
            .Transition(nameof(RepresentationTarget.Near), nameof(RepresentationTarget.Semantic),
                context => ShouldTransition(context, RepresentationTarget.Semantic))
            .Transition(nameof(RepresentationTarget.Near), nameof(RepresentationTarget.Aggregate),
                context => ShouldTransition(context, RepresentationTarget.Aggregate))
            .Transition(nameof(RepresentationTarget.Near), nameof(RepresentationTarget.Distant),
                context => ShouldTransition(context, RepresentationTarget.Distant))
            .Transition(nameof(RepresentationTarget.Near), nameof(RepresentationTarget.Interactive),
                context => ShouldTransition(context, RepresentationTarget.Interactive))
            .Transition(nameof(RepresentationTarget.Interactive), nameof(RepresentationTarget.Semantic),
                context => ShouldTransition(context, RepresentationTarget.Semantic))
            .Transition(nameof(RepresentationTarget.Interactive), nameof(RepresentationTarget.Aggregate),
                context => ShouldTransition(context, RepresentationTarget.Aggregate))
            .Transition(nameof(RepresentationTarget.Interactive), nameof(RepresentationTarget.Distant),
                context => ShouldTransition(context, RepresentationTarget.Distant))
            .Transition(nameof(RepresentationTarget.Interactive), nameof(RepresentationTarget.Near),
                context => ShouldTransition(context, RepresentationTarget.Near))
            .BuildDefinition();
        }
    }

    private static bool ShouldTransition(
        IStateContext stateContext,
        RepresentationTarget target)
    {
        var context = (RendererComputationContext)stateContext;
        if (!context.IsValid || context.DesiredRepresentation != target || context.ActiveRepresentation == target)
        {
            return false;
        }

        context.ActiveRepresentation = target;
        return true;
    }

}
