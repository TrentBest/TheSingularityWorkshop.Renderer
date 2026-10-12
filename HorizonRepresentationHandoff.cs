namespace TheSingularityWorkshop.Renderer;

/// <summary>
/// Describes one prepared visual representation and the revisions for which it was built.
/// </summary>
/// <param name="Id">Stable identity for this representation instance.</param>
/// <param name="HorizonName">The Event Horizon whose policy produced the representation.</param>
/// <param name="SourceRevision">Authoritative source revision represented by this output.</param>
/// <param name="ObserverRevision">Observer/view revision for which its coverage was evaluated.</param>
/// <param name="IsReady">Whether all required preparation work has completed successfully.</param>
/// <param name="CoversObserverView">Whether this representation covers the current observer view.</param>
public sealed record HorizonRepresentation(
    string Id,
    string HorizonName,
    long SourceRevision,
    long ObserverRevision,
    bool IsReady,
    bool CoversObserverView);

/// <summary>An atomic display-selection snapshot for an Event Horizon handoff.</summary>
/// <param name="Current">The representation that must remain visible until a handoff commits.</param>
/// <param name="Pending">The replacement being prepared, if any.</param>
/// <param name="Generation">Changes whenever the selection or pending candidate changes.</param>
public sealed record HorizonHandoffSnapshot(
    HorizonRepresentation Current,
    HorizonRepresentation? Pending,
    long Generation);

/// <summary>
/// Coordinates a gap-free, backend-independent handoff between Event Horizon representations.
/// </summary>
/// <remarks>
/// Staging or preparing a replacement never removes the current representation. A replacement
/// becomes current only when it is ready, covers the observer view, and matches the current
/// observer and source revisions. A consumer should read <see cref="Snapshot"/> once per frame
/// and use that immutable snapshot for its display selection, avoiding mixed-state reads.
/// This class does not schedule preparation, own graphics resources, or perform fades.
/// </remarks>
public sealed class HorizonRepresentationHandoff
{
    private readonly object _gate = new();
    private HorizonRepresentation _current;
    private HorizonRepresentation? _pending;
    private long _generation;

    /// <summary>Creates a handoff coordinator with an already-visible representation.</summary>
    /// <param name="initial">The initial representation that is already ready and view-covering.</param>
    public HorizonRepresentationHandoff(HorizonRepresentation initial)
    {
        Validate(initial, nameof(initial));
        if (!initial.IsReady || !initial.CoversObserverView)
            throw new ArgumentException("The initial representation must be ready and cover the observer view.", nameof(initial));

        _current = initial;
    }

    /// <summary>Gets an immutable snapshot of the active and pending representations.</summary>
    public HorizonHandoffSnapshot Snapshot
    {
        get
        {
            lock (_gate)
                return new HorizonHandoffSnapshot(_current, _pending, _generation);
        }
    }

    /// <summary>
    /// Stages or replaces a candidate without disturbing the currently visible representation.
    /// A later candidate supersedes earlier pending work, which makes reversals safe.
    /// </summary>
    public void Stage(HorizonRepresentation candidate)
    {
        Validate(candidate, nameof(candidate));

        lock (_gate)
        {
            if (string.Equals(candidate.Id, _current.Id, StringComparison.Ordinal))
                throw new ArgumentException("The pending representation must differ from the current representation.", nameof(candidate));

            _pending = candidate;
            _generation++;
        }
    }

    /// <summary>
    /// Updates readiness and view coverage after asynchronous preparation or reprojection.
    /// Updates are accepted only for the currently staged candidate identity.
    /// </summary>
    public bool UpdatePending(
        string candidateId,
        bool isReady,
        bool coversObserverView,
        long observerRevision)
    {
        if (string.IsNullOrWhiteSpace(candidateId))
            throw new ArgumentException("A candidate identity is required.", nameof(candidateId));

        lock (_gate)
        {
            if (_pending is null || !string.Equals(_pending.Id, candidateId, StringComparison.Ordinal))
                return false;

            _pending = _pending with
            {
                IsReady = isReady,
                CoversObserverView = coversObserverView,
                ObserverRevision = observerRevision
            };
            _generation++;
            return true;
        }
    }

    /// <summary>
    /// Atomically commits the pending representation only when it is ready and valid for the
    /// supplied current source and observer revisions. Failure leaves the current visible.
    /// </summary>
    public bool TryCommit(long currentSourceRevision, long currentObserverRevision)
    {
        lock (_gate)
        {
            if (_pending is not { IsReady: true, CoversObserverView: true } candidate ||
                candidate.SourceRevision != currentSourceRevision ||
                candidate.ObserverRevision != currentObserverRevision)
            {
                return false;
            }

            _current = candidate;
            _pending = null;
            _generation++;
            return true;
        }
    }

    /// <summary>Cancels pending work only if the supplied identity still names the pending candidate.</summary>
    public bool CancelPending(string candidateId)
    {
        if (string.IsNullOrWhiteSpace(candidateId))
            throw new ArgumentException("A candidate identity is required.", nameof(candidateId));

        lock (_gate)
        {
            if (_pending is null || !string.Equals(_pending.Id, candidateId, StringComparison.Ordinal))
                return false;

            _pending = null;
            _generation++;
            return true;
        }
    }

    private static void Validate(HorizonRepresentation representation, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(representation);
        if (string.IsNullOrWhiteSpace(representation.Id))
            throw new ArgumentException("A representation identity is required.", parameterName);
        if (string.IsNullOrWhiteSpace(representation.HorizonName))
            throw new ArgumentException("A horizon name is required.", parameterName);
    }
}
