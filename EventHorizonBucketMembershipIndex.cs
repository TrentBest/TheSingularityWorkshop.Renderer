namespace TheSingularityWorkshop.Renderer;

/// <summary>
/// Tracks membership for one visual domain across Event Horizon buckets.
/// </summary>
/// <remarks>
/// One index represents one membership dimension (for example, the Zombies cohort or
/// environment geometry). A member can therefore belong to Geometry and Motion at the same
/// time by being tracked in separate indexes. Within one index, a member occupies at most one
/// bucket; use <see cref="Move"/> to transfer it between horizons without duplicating membership.
/// Membership operations invalidate only the bucket or buckets whose membership changed.
/// </remarks>
public sealed class EventHorizonBucketMembershipIndex
{
    private readonly object _gate = new();
    private readonly Dictionary<string, EventHorizonBucket> _bucketByMember =
        new(StringComparer.Ordinal);
    private readonly Dictionary<EventHorizonBucket, SortedSet<string>> _membersByBucket = new();

    /// <summary>Adds a member to a bucket in this membership domain.</summary>
    /// <returns>False if the member already belongs to the supplied bucket.</returns>
    /// <exception cref="InvalidOperationException">
    /// The member already belongs to a different bucket; use <see cref="Move"/> instead.
    /// </exception>
    public bool Add(string memberId, EventHorizonBucket bucket)
    {
        ValidateMemberId(memberId);
        ArgumentNullException.ThrowIfNull(bucket);

        lock (_gate)
        {
            if (_bucketByMember.TryGetValue(memberId, out var current))
            {
                if (ReferenceEquals(current, bucket))
                    return false;

                throw new InvalidOperationException(
                    $"Member '{memberId}' already belongs to bucket '{current.Name}'. Use Move to transfer it.");
            }

            _bucketByMember.Add(memberId, bucket);
            GetOrCreateMembersUnderLock(bucket).Add(memberId);
            bucket.MarkDirty(BucketDirtyReason.MembershipChanged);
            return true;
        }
    }

    /// <summary>Removes a member from its current bucket.</summary>
    /// <returns>False if the member is not currently tracked.</returns>
    public bool Remove(string memberId)
    {
        ValidateMemberId(memberId);

        lock (_gate)
        {
            if (!_bucketByMember.Remove(memberId, out var bucket))
                return false;

            _membersByBucket[bucket].Remove(memberId);
            bucket.MarkDirty(BucketDirtyReason.MembershipChanged);
            return true;
        }
    }

    /// <summary>
    /// Moves a member to another bucket in this domain. Both affected buckets are invalidated
    /// before the operation releases the index lock, so another membership operation cannot
    /// observe the transfer as complete while either cached representation remains valid.
    /// </summary>
    /// <returns>False if the member is unknown or already belongs to the destination bucket.</returns>
    public bool Move(string memberId, EventHorizonBucket destination)
    {
        ValidateMemberId(memberId);
        ArgumentNullException.ThrowIfNull(destination);

        lock (_gate)
        {
            if (!_bucketByMember.TryGetValue(memberId, out var source))
                return false;

            if (ReferenceEquals(source, destination))
                return false;

            _membersByBucket[source].Remove(memberId);
            GetOrCreateMembersUnderLock(destination).Add(memberId);
            _bucketByMember[memberId] = destination;

            source.MarkDirty(BucketDirtyReason.MembershipChanged);
            destination.MarkDirty(BucketDirtyReason.MembershipChanged);
            return true;
        }
    }

    /// <summary>Finds the bucket currently assigned to a member, or null when it is untracked.</summary>
    public EventHorizonBucket? FindBucket(string memberId)
    {
        ValidateMemberId(memberId);
        lock (_gate)
            return _bucketByMember.GetValueOrDefault(memberId);
    }

    /// <summary>Returns a deterministic, read-only snapshot of the members in a bucket.</summary>
    public IReadOnlyList<string> GetMembers(EventHorizonBucket bucket)
    {
        ArgumentNullException.ThrowIfNull(bucket);

        lock (_gate)
        {
            return _membersByBucket.TryGetValue(bucket, out var members)
                ? members.ToArray()
                : Array.Empty<string>();
        }
    }

    /// <summary>Gets the number of tracked members in this membership domain.</summary>
    public int Count
    {
        get { lock (_gate) return _bucketByMember.Count; }
    }

    private SortedSet<string> GetOrCreateMembersUnderLock(EventHorizonBucket bucket)
    {
        if (!_membersByBucket.TryGetValue(bucket, out var members))
        {
            members = new SortedSet<string>(StringComparer.Ordinal);
            _membersByBucket.Add(bucket, members);
        }

        return members;
    }

    private static void ValidateMemberId(string memberId)
    {
        if (string.IsNullOrWhiteSpace(memberId))
            throw new ArgumentException("A stable member identity is required.", nameof(memberId));
    }
}
