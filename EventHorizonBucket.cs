namespace TheSingularityWorkshop.Renderer;

/// <summary>Identifies why a cached bucket representation may no longer be reused.</summary>
[Flags]
public enum BucketDirtyReason
{
    /// <summary>No known invalidation reason.</summary>
    None = 0,
    /// <summary>The bucket has not yet produced a valid representation.</summary>
    Initial = 1 << 0,
    /// <summary>Source state used to build the representation changed.</summary>
    SourceChanged = 1 << 1,
    /// <summary>Entities or cohorts entered or left the bucket.</summary>
    MembershipChanged = 1 << 2,
    /// <summary>Observer movement or orientation invalidated this representation.</summary>
    ObserverChanged = 1 << 3,
    /// <summary>A referenced geometry, material, texture, or other resource changed.</summary>
    ResourceChanged = 1 << 4,
    /// <summary>A dependency changed and invalidated this bucket's derived output.</summary>
    DependencyChanged = 1 << 5,
    /// <summary>An explicit policy or caller request invalidated the representation.</summary>
    Explicit = 1 << 6,
    /// <summary>The configured freshness deadline elapsed.</summary>
    FreshnessDeadline = 1 << 7
}

/// <summary>Identifies one attempt to refresh one specific bucket instance.</summary>
/// <param name="BucketId">Unique identity of the bucket that issued the ticket.</param>
/// <param name="BucketName">Human-readable name of the bucket.</param>
/// <param name="InvalidationGeneration">Generation observed when refresh work began.</param>
/// <param name="SourceRevision">Source revision captured when refresh work began.</param>
/// <param name="StartedAt">Time at which the refresh attempt began.</param>
public readonly record struct BucketRefreshTicket(
    Guid BucketId,
    string BucketName,
    long InvalidationGeneration,
    long SourceRevision,
    DateTimeOffset StartedAt);

/// <summary>Backend-free validity and cadence state for one named visual bucket within an Event Horizon.</summary>
/// <remarks>
/// This is a deterministic policy primitive, not a scheduler or a graphics resource. FSM_API remains
/// responsible for orchestrating meaningful processing groups. A horizon may contain multiple
/// independent buckets, each with its own cadence and invalidation history.
/// </remarks>
public sealed class EventHorizonBucket
{
    private readonly object _gate = new();
    private readonly Guid _bucketId = Guid.NewGuid();
    private BucketDirtyReason _dirtyReasons = BucketDirtyReason.Initial;
    private long _invalidationGeneration;
    private long _sourceRevision;
    private long _cachedSourceRevision;
    private DateTimeOffset? _lastRefreshAt;
    private bool _hasValidRepresentation;

    // Dependency graphs observe invalidation only after the bucket lock is released.
    internal event Action<EventHorizonBucket>? Invalidated;

    /// <summary>Creates a bucket with its own refresh cadence.</summary>
    /// <param name="name">Domain-defined bucket name.</param>
    /// <param name="horizon">Event Horizon that contains this bucket.</param>
    /// <param name="updatesPerSecond">Preferred refresh frequency, representable as a positive TimeSpan interval.</param>
    public EventHorizonBucket(string name, EventHorizon horizon, double updatesPerSecond)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A bucket name is required.", nameof(name));
        ArgumentNullException.ThrowIfNull(horizon);
        if (!double.IsFinite(updatesPerSecond) || updatesPerSecond <= 0)
            throw new ArgumentOutOfRangeException(nameof(updatesPerSecond));

        var intervalSeconds = 1d / updatesPerSecond;
        // TimeSpan has 100 ns resolution. Reject frequencies that round the interval to zero;
        // otherwise an ostensibly rate-limited bucket becomes due on every scheduling check.
        if (!double.IsFinite(intervalSeconds) ||
            intervalSeconds > TimeSpan.MaxValue.TotalSeconds ||
            intervalSeconds < 1d / TimeSpan.TicksPerSecond)
        {
            throw new ArgumentOutOfRangeException(nameof(updatesPerSecond));
        }

        Name = name;
        Horizon = horizon;
        UpdatesPerSecond = updatesPerSecond;
        RefreshInterval = TimeSpan.FromSeconds(intervalSeconds);
    }

    /// <summary>Gets the domain-defined bucket name.</summary>
    public string Name { get; }

    /// <summary>Gets the containing Event Horizon.</summary>
    public EventHorizon Horizon { get; }

    /// <summary>Gets the preferred refresh frequency for this bucket.</summary>
    public double UpdatesPerSecond { get; }

    /// <summary>Gets the derived minimum interval between scheduled refreshes.</summary>
    public TimeSpan RefreshInterval { get; }

    /// <summary>Gets the current reasons that prevent cached output from being reused.</summary>
    public BucketDirtyReason DirtyReasons
    {
        get { lock (_gate) return _dirtyReasons; }
    }

    /// <summary>Gets whether a completed representation is currently reusable.</summary>
    public bool HasValidRepresentation
    {
        get { lock (_gate) return _hasValidRepresentation && _dirtyReasons == BucketDirtyReason.None; }
    }

    /// <summary>Gets the source revision represented by the most recently accepted refresh.</summary>
    public long CachedSourceRevision
    {
        get { lock (_gate) return _cachedSourceRevision; }
    }

    /// <summary>Gets the time at which the last refresh was accepted, if any.</summary>
    public DateTimeOffset? LastRefreshAt
    {
        get { lock (_gate) return _lastRefreshAt; }
    }

    /// <summary>Marks this bucket stale without invalidating unrelated buckets.</summary>
    /// <param name="reason">One or more defined reasons that invalidate the current representation.</param>
    public void MarkDirty(BucketDirtyReason reason)
    {
        const BucketDirtyReason allReasons =
            BucketDirtyReason.Initial |
            BucketDirtyReason.SourceChanged |
            BucketDirtyReason.MembershipChanged |
            BucketDirtyReason.ObserverChanged |
            BucketDirtyReason.ResourceChanged |
            BucketDirtyReason.DependencyChanged |
            BucketDirtyReason.Explicit |
            BucketDirtyReason.FreshnessDeadline;

        if (reason == BucketDirtyReason.None || (reason & ~allReasons) != 0)
            throw new ArgumentOutOfRangeException(nameof(reason));

        lock (_gate)
        {
            _dirtyReasons |= reason;
            _invalidationGeneration++;
        }

        Invalidated?.Invoke(this);
    }

    /// <summary>
    /// Records a new authoritative source revision and invalidates output built from an older one.
    /// Repeating the same revision does not invalidate the cache.
    /// </summary>
    /// <param name="sourceRevision">Current authoritative source revision.</param>
    public void SetSourceRevision(long sourceRevision)
    {
        lock (_gate)
        {
            if (_sourceRevision == sourceRevision)
                return;

            _sourceRevision = sourceRevision;
            _dirtyReasons |= BucketDirtyReason.SourceChanged;
            _invalidationGeneration++;
        }

        Invalidated?.Invoke(this);
    }

    /// <summary>Returns whether a refresh is required at the supplied deterministic time.</summary>
    /// <param name="now">Time against which the bucket cadence is evaluated.</param>
    public bool IsRefreshDue(DateTimeOffset now)
    {
        lock (_gate)
            return IsRefreshDueUnderLock(now);
    }

    /// <summary>
    /// Starts a refresh attempt if work is due. Build into separate output and publish it only
    /// if <see cref="TryCompleteRefresh"/> accepts the returned ticket.
    /// </summary>
    /// <param name="now">Time at which the refresh attempt starts.</param>
    /// <param name="ticket">Receives a ticket that must accompany the completed output.</param>
    /// <returns>True if refresh work is due and a ticket was issued.</returns>
    public bool TryBeginRefresh(DateTimeOffset now, out BucketRefreshTicket ticket)
    {
        lock (_gate)
        {
            if (!IsRefreshDueUnderLock(now))
            {
                ticket = default;
                return false;
            }

            ticket = new BucketRefreshTicket(_bucketId, Name, _invalidationGeneration, _sourceRevision, now);
            return true;
        }
    }

    /// <summary>
    /// Accepts completed output only if no invalidation or source revision change occurred
    /// while it was being produced. Rejected output must not replace the last valid cache.
    /// </summary>
    /// <param name="ticket">Ticket issued for this bucket's refresh attempt.</param>
    /// <param name="completedAt">Time at which refresh work completed.</param>
    /// <returns>True if the output is accepted as the current representation.</returns>
    public bool TryCompleteRefresh(BucketRefreshTicket ticket, DateTimeOffset completedAt)
    {
        lock (_gate)
        {
            if (ticket.BucketId != _bucketId ||
                !string.Equals(ticket.BucketName, Name, StringComparison.Ordinal) ||
                completedAt < ticket.StartedAt ||
                ticket.InvalidationGeneration != _invalidationGeneration ||
                ticket.SourceRevision != _sourceRevision)
            {
                return false;
            }

            _cachedSourceRevision = ticket.SourceRevision;
            _lastRefreshAt = completedAt;
            _hasValidRepresentation = true;
            _dirtyReasons = BucketDirtyReason.None;

            // Invalidate duplicate/in-flight tickets from the same generation. Only one
            // completion may publish for a given invalidation generation.
            _invalidationGeneration++;
            return true;
        }
    }

    private bool IsRefreshDueUnderLock(DateTimeOffset now)
    {
        if (!_hasValidRepresentation || _dirtyReasons != BucketDirtyReason.None)
            return true;

        return !_lastRefreshAt.HasValue || now - _lastRefreshAt.Value >= RefreshInterval;
    }
}
