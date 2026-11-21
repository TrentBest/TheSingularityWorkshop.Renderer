namespace TheSingularityWorkshop.Renderer;

/// <summary>
/// Connects observer-distance policy, FSM-backed representation state, and independently refreshed horizon buckets.
/// </summary>
/// <remarks>
/// The coordinator is scoped to one observer context. Each named bucket owns its own FSM representation state,
/// source/observer revision tracking, refresh cadence, and cache validity—even when several buckets share one
/// Event Horizon. A host prepares the actual representation and must call
/// <see cref="EventHorizonBucket.TryCompleteRefresh"/> only after that output is ready to publish.
/// This class is not a scheduler, graphics backend, or frame builder.
/// </remarks>
public sealed class ObserverRepresentationCoordinator : IDisposable
{
    private readonly object _gate = new();
    private readonly ObserverRepresentationPolicy _policy;
    private readonly Dictionary<EventHorizon, Dictionary<string, EventHorizonBucket>> _buckets;
    private readonly Dictionary<EventHorizonBucket, long> _observerRevisionByBucket = new();
    private readonly Dictionary<EventHorizonBucket, RendererComputationMachine> _machinesByBucket;
    private RepresentationTarget _lastActiveRepresentation = RepresentationTarget.Semantic;
    private bool _disposed;

    /// <summary>Creates a coordinator over a policy and its named horizon buckets.</summary>
    /// <param name="policy">The observer-distance policy.</param>
    /// <param name="buckets">One or more uniquely named buckets for each supported horizon.</param>
    /// <param name="contextName">Unique FSM_API context-name prefix for this observer coordinator.</param>
    public ObserverRepresentationCoordinator(
        ObserverRepresentationPolicy policy,
        IEnumerable<EventHorizonBucket> buckets,
        string contextName)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(buckets);
        ArgumentException.ThrowIfNullOrWhiteSpace(contextName);

        _policy = policy;
        _buckets = new Dictionary<EventHorizon, Dictionary<string, EventHorizonBucket>>();
        foreach (var bucket in buckets)
        {
            ArgumentNullException.ThrowIfNull(bucket);
            if (!_buckets.TryGetValue(bucket.Horizon, out var horizonBuckets))
            {
                horizonBuckets = new Dictionary<string, EventHorizonBucket>(StringComparer.Ordinal);
                _buckets.Add(bucket.Horizon, horizonBuckets);
            }

            if (!horizonBuckets.TryAdd(bucket.Name, bucket))
            {
                throw new ArgumentException(
                    $"More than one bucket named '{bucket.Name}' was supplied for Event Horizon '{bucket.Horizon.Name}'.",
                    nameof(buckets));
            }
        }

        if (_buckets.Count == 0)
            throw new ArgumentException("At least one Event Horizon bucket is required.", nameof(buckets));

        var machines = new Dictionary<EventHorizonBucket, RendererComputationMachine>();
        try
        {
            var contextIndex = 0;
            foreach (var bucket in _buckets.Values.SelectMany(horizonBuckets => horizonBuckets.Values))
            {
                var context = new RendererComputationContext($"{contextName}:{contextIndex++}");
                machines.Add(bucket, new RendererComputationMachine(context));
            }
        }
        catch
        {
            foreach (var machine in machines.Values)
                machine.Dispose();
            throw;
        }

        _machinesByBucket = machines;
    }

    /// <summary>
    /// Gets the representation active after the most recent evaluation.
    /// For multiple buckets, use each evaluation's ActiveRepresentation for its own state.
    /// </summary>
    public RepresentationTarget ActiveRepresentation
    {
        get
        {
            lock (_gate)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                return _lastActiveRepresentation;
            }
        }
    }

    /// <summary>
    /// Evaluates one observer sample for one named bucket, updates that bucket's FSM representation,
    /// and checks its independent freshness/cadence state.
    /// </summary>
    /// <param name="distance">Observer-to-entity distance in world units.</param>
    /// <param name="sourceRevision">Revision of the authoritative source data.</param>
    /// <param name="observerRevision">Revision of the observer view used for coverage.</param>
    /// <param name="now">Deterministic time used for bucket cadence checks.</param>
    /// <param name="minimumRepresentation">Optional caller-supplied floor for representation detail, such as an interaction or inspection requirement.</param>
    /// <param name="bucketName">Name of the bucket within the selected horizon. May be omitted only when that horizon has exactly one bucket.</param>
    /// <returns>A decision and either a refresh ticket or confirmation that valid cached output can be reused.</returns>
    public ObserverRepresentationEvaluation Evaluate(
        double distance,
        long sourceRevision,
        long observerRevision,
        DateTimeOffset now,
        RepresentationTarget? minimumRepresentation = null,
        string? bucketName = null)
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var decision = minimumRepresentation is { } minimum
                ? _policy.Select(distance, minimum)
                : _policy.Select(distance);
            var bucket = SelectBucket(decision.Horizon, bucketName);
            var machine = _machinesByBucket[bucket];

            bucket.SetSourceRevision(sourceRevision);
            if (_observerRevisionByBucket.TryGetValue(bucket, out var previousObserverRevision) &&
                previousObserverRevision != observerRevision)
            {
                bucket.MarkDirty(BucketDirtyReason.ObserverChanged);
            }

            _observerRevisionByBucket[bucket] = observerRevision;
            machine.Context.DesiredRepresentation = decision.Representation;
            var activeRepresentation = machine.Advance();
            _lastActiveRepresentation = activeRepresentation;

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

    /// <summary>Releases every FSM instance owned by this observer coordinator.</summary>
    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
                return;

            _disposed = true;
            foreach (var machine in _machinesByBucket.Values)
                machine.Dispose();
        }
    }

    private EventHorizonBucket SelectBucket(EventHorizon horizon, string? bucketName)
    {
        if (!_buckets.TryGetValue(horizon, out var horizonBuckets))
        {
            throw new InvalidOperationException(
                $"No Event Horizon buckets were supplied for selected horizon '{horizon.Name}'.");
        }

        if (bucketName is null)
        {
            if (horizonBuckets.Count == 1)
                return horizonBuckets.Values.First();

            throw new InvalidOperationException(
                $"Event Horizon '{horizon.Name}' has multiple buckets ({string.Join(", ", horizonBuckets.Keys)}); specify bucketName.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(bucketName);
        if (!horizonBuckets.TryGetValue(bucketName, out var bucket))
        {
            throw new InvalidOperationException(
                $"Event Horizon '{horizon.Name}' has no bucket named '{bucketName}'.");
        }

        return bucket;
    }
}

/// <summary>The output of one observer representation evaluation.</summary>
/// <param name="Decision">The selected horizon and desired representation.</param>
/// <param name="ActiveRepresentation">The representation active after this bucket's FSM update.</param>
/// <param name="Bucket">The named bucket associated with the selected horizon.</param>
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
