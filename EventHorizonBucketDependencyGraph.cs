namespace TheSingularityWorkshop.Renderer;

/// <summary>
/// Propagates invalidation through declared visual-data dependencies.
/// Edges point from an input bucket to buckets whose output depends on that input.
/// </summary>
/// <remarks>
/// The graph is backend- and scheduler-independent. Dependencies may cross Event Horizons;
/// a changed input invalidates only its transitive dependents. Cycles are rejected so an
/// invalidation cannot circulate indefinitely. Dispose the graph when its prepared pipeline
/// is retired to detach its event handlers.
/// </remarks>
public sealed class EventHorizonBucketDependencyGraph : IDisposable
{
    private readonly object _gate = new();
    private readonly Dictionary<EventHorizonBucket, HashSet<EventHorizonBucket>> _dependents = new();
    private readonly HashSet<EventHorizonBucket> _observed = new();
    private bool _disposed;

    /// <summary>Declares that <paramref name="dependent"/> uses output from <paramref name="source"/>.</summary>
    /// <returns>True when a new dependency edge was added; false when the edge already existed.</returns>
    public bool AddDependency(EventHorizonBucket source, EventHorizonBucket dependent)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(dependent);

        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (ReferenceEquals(source, dependent) || CanReachUnderLock(dependent, source))
                throw new ArgumentException("The dependency would introduce a cycle.", nameof(dependent));

            if (!_dependents.TryGetValue(source, out var targets))
            {
                targets = new HashSet<EventHorizonBucket>();
                _dependents.Add(source, targets);
            }

            if (!targets.Add(dependent))
                return false;

            ObserveUnderLock(source);
            ObserveUnderLock(dependent);
        }

        // The dependent may already hold output computed before this edge existed, or before
        // the source became valid. Invalidate it immediately. Notify outside the graph lock:
        // bucket notifications are synchronous and can re-enter this graph.
        dependent.MarkDirty(BucketDirtyReason.DependencyChanged);
        return true;
    }

    /// <summary>Returns a snapshot of the buckets directly invalidated by a source bucket.</summary>
    public IReadOnlyList<EventHorizonBucket> GetDependents(EventHorizonBucket source)
    {
        ArgumentNullException.ThrowIfNull(source);
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _dependents.TryGetValue(source, out var targets)
                ? targets.ToArray()
                : Array.Empty<EventHorizonBucket>();
        }
    }

    /// <summary>Detaches invalidation handlers. The graph cannot be reused after disposal.</summary>
    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
                return;

            _disposed = true;
            foreach (var bucket in _observed)
                bucket.Invalidated -= OnBucketInvalidated;

            _observed.Clear();
            _dependents.Clear();
        }
    }

    private void ObserveUnderLock(EventHorizonBucket bucket)
    {
        if (_observed.Add(bucket))
            bucket.Invalidated += OnBucketInvalidated;
    }

    private void OnBucketInvalidated(EventHorizonBucket source)
    {
        EventHorizonBucket[] targets;
        lock (_gate)
        {
            if (_disposed || !_dependents.TryGetValue(source, out var dependents))
                return;

            // Never call another bucket while holding the graph lock. Bucket notifications
            // are synchronous and can re-enter this graph as invalidation propagates.
            targets = dependents.ToArray();
        }

        foreach (var dependent in targets)
            dependent.MarkDirty(BucketDirtyReason.DependencyChanged);
    }

    private bool CanReachUnderLock(EventHorizonBucket start, EventHorizonBucket target)
    {
        var pending = new Stack<EventHorizonBucket>();
        var visited = new HashSet<EventHorizonBucket>();
        pending.Push(start);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!visited.Add(current))
                continue;
            if (ReferenceEquals(current, target))
                return true;

            if (_dependents.TryGetValue(current, out var next))
            {
                foreach (var bucket in next)
                    pending.Push(bucket);
            }
        }

        return false;
    }
}
