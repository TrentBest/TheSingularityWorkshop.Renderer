namespace TheSingularityWorkshop.Renderer;

/// <summary>
/// Connects observer-distance policy, FSM-backed representation state, and per-horizon bucket freshness.
/// </summary>
/// <remarks>
/// The coordinator decides whether a selected bucket needs refresh work and returns its ticket. A host
/// still prepares the actual representation and must call <see cref="EventHorizonBucket.TryCompleteRefresh"/>
/// only after that output is ready to publish. This class is not a scheduler, graphics backend, or frame builder.
/// Create a separate coordinator per independent observer context; one observer's active representation is not
/// authoritative world state and must not be shared as another observer's answer.
/// </remarks>
public sealed class ObserverRepresentationCoordinator : IDisposable
{
    private readonly object _gate = new();
    private readonly ObserverRepresentationPolicy _policy;
    private readonly Dictionary<EventHorizon, EventHorizonBucket> _buckets;
    private readonly Dictionary<EventHorizonBucket, long> _observerRevisionByBucket = new();
    private readonly RendererComputationContext _context;
    private readonly RendererComputationMachine _machine;
    private bool _disposed;

    /// <summary>Creates a coordinator over a policy and its horizon-specific buckets.</summary>
    /// <param name="policy">The observer-distance policy.</param>
    /// <param name="buckets">One bucket for each horizon that may be selected by the policy.</param>
    /// <param name="contextName">Unique FSM_API context name for this coordinator.</param>
    public ObserverRepresentationCoordinator(
        ObserverRepresentationPolicy policy,
        IEnumerable<EventHorizonBucket> buckets,
        string contextName)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(buckets);
        ArgumentException.ThrowIfNullOrWhiteSpace(contextName);

        _policy = policy;
        _buckets = new Dictionary<EventHorizon, EventHorizonBucket>();
        foreach (var bucket in buckets)
        {
            ArgumentNullException.ThrowIfNull(bucket);
            if (!_buckets.TryAdd(bucket.Horizon, bucket))
            {
                throw new ArgumentException(
                    $"More than one bucket was supplied for Event Horizon '{bucket.Horizon.Name}'.",
                    nameof(buckets));
            }
        }

        if (_buckets.Count == 0)
            throw new ArgumentException("At least one Event Horizon bucket is required.", nameof(buckets));

        _context = new RendererComputationContext(contextName);
        _machine = new RendererComputationMachine(_context);
    }

    /// <summary>Gets the representation currently active in the coordinator's FSM.</summary>
    public RepresentationTarget ActiveRepresentation
    {
        get
        {
            lock (_gate)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                return _machine.CurrentRepresentation;
            }
        }
    }

    /// <summary>
    /// Evaluates one observer sample, updates the FSM representation, and checks the selected bucket's freshness.
    /// </summary>
    /// <param name="distance">Observer-to-entity distance in world units.</param>
    /// <param name="sourceRevision">Revision of the authoritative source data.</param>
    /// <param name="observerRevision">Revision of the observer view used for coverage.</param>
    /// <param name="now">Deterministic time used for bucket cadence checks.</param>
    /// <returns>A decision and either a refresh ticket or confirmation that valid cached output can be reused.</returns>
    public ObserverRepresentationEvaluation Evaluate(
        double distance,
        long sourceRevision,
        long observerRevision,
        DateTimeOffset now)
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var decision = _policy.Select(distance);
            if (!_buckets.TryGetValue(decision.Horizon, out var bucket))
            {
                throw new InvalidOperationException(
                    $"No Event Horizon bucket was supplied for selected horizon '{decision.Horizon.Name}'.");
            }

            bucket.SetSourceRevision(sourceRevision);
            if (_observerRevisionByBucket.TryGetValue(bucket, out var previousObserverRevision) &&
                previousObserverRevision != observerRevision)
            {
                bucket.MarkDirty(BucketDirtyReason.ObserverChanged);
            }

            _observerRevisionByBucket[bucket] = observerRevision;
            _context.DesiredRepresentation = decision.Representation;
            var activeRepresentation = _machine.Advance();

            if (bucket.TryBeginRefresh(now, out var ticket))
            {
                return new ObserverRepresentationEvaluation(
                    decision,
                    activeRepresentation,
                    bucket,
                    ticket,
                    CanReuseCachedRepresentation: false);
            }

            var canReuse = bucket.TryRecordReuse();
            return new ObserverRepresentationEvaluation(
                decision,
                activeRepresentation,
                bucket,
                RefreshTicket: null,
                CanReuseCachedRepresentation: canReuse);
        }
    }

    /// <summary>Releases the FSM instance owned by this coordinator.</summary>
    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
                return;

            _disposed = true;
            _machine.Dispose();
        }
    }
}

/// <summary>The output of one observer representation evaluation.</summary>
/// <param name="Decision">The selected horizon and desired representation.</param>
/// <param name="ActiveRepresentation">The representation active after the FSM update.</param>
/// <param name="Bucket">The bucket associated with the selected horizon.</param>
/// <param name="RefreshTicket">A ticket for new work, or null when no refresh is due.</param>
/// <param name="CanReuseCachedRepresentation">Whether the bucket confirmed its cached representation is valid and reusable.</param>
public sealed record ObserverRepresentationEvaluation(
    ObserverRepresentationDecision Decision,
    RepresentationTarget ActiveRepresentation,
    EventHorizonBucket Bucket,
    BucketRefreshTicket? RefreshTicket,
    bool CanReuseCachedRepresentation)
{
    /// <summary>Gets whether the host must prepare replacement output for this evaluation.</summary>
    public bool RefreshRequired => RefreshTicket.HasValue;
}