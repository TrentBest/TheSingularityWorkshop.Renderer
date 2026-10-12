namespace TheSingularityWorkshop.Renderer;

[Flags]
public enum BucketDirtyReason
{
    None = 0,
    Initial = 1 << 0,
    SourceChanged = 1 << 1,
    MembershipChanged = 1 << 2,
    ObserverChanged = 1 << 3,
    ResourceChanged = 1 << 4,
    DependencyChanged = 1 << 5,
    Explicit = 1 << 6,
    FreshnessDeadline = 1 << 7
}

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

    /// <summary>Creates a bucket with its own refresh cadence.</summary>
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
    }

    /// <summary>
    /// Records a new authoritative source revision and invalidates output built from an older one.
    /// Repeating the same revision does not invalidate the cache.
    /// </summary>
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
    }

    /// <summary>Returns whether a refresh is required at the supplied deterministic time.</summary>
    public bool IsRefreshDue(DateTimeOffset now)
    {
        lock (_gate)
            return IsRefreshDueUnderLock(now);
    }

    /// <summary>
    /// Starts a refresh attempt if work is due. Build into separate output and publish it only
    /// if <see cref="TryCompleteRefresh"/> accepts the returned ticket.
    /// </summary>
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
