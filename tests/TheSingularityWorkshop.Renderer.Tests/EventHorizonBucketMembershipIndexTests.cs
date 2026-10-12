using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class EventHorizonBucketMembershipIndexTests
{
    private static readonly DateTimeOffset Epoch =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void AddingMemberInvalidatesOnlyItsAssignedBucket()
    {
        var environment = Bucket("Environment", "Far", 1);
        var zombies = Bucket("Zombies", "Far", 1);
        Complete(environment);
        Complete(zombies);
        var index = new EventHorizonBucketMembershipIndex();

        Assert.True(index.Add("tree:42", environment));

        Assert.False(environment.HasValidRepresentation);
        Assert.True(zombies.HasValidRepresentation);
        Assert.Equal(new[] { "tree:42" }, index.GetMembers(environment));
        Assert.Empty(index.GetMembers(zombies));
        Assert.Equal(1, index.Count);
    }

    [Fact]
    public void ReaddingSameMemberToSameBucketIsANoOp()
    {
        var bucket = Bucket("Zombies", "Far", 1);
        var index = new EventHorizonBucketMembershipIndex();

        Assert.True(index.Add("zombie-cohort:9", bucket));
        Complete(bucket);

        Assert.False(index.Add("zombie-cohort:9", bucket));
        Assert.True(bucket.HasValidRepresentation);
        Assert.Equal(1, index.Count);
    }

    [Fact]
    public void MemberCannotBeDuplicatedAcrossBucketsWithinOneDomain()
    {
        var near = Bucket("Zombies", "Near", 30);
        var far = Bucket("Zombies", "Far", 1);
        var index = new EventHorizonBucketMembershipIndex();
        index.Add("cohort:9", near);

        Assert.Throws<InvalidOperationException>(() => index.Add("cohort:9", far));
        Assert.Same(near, index.FindBucket("cohort:9"));
        Assert.Empty(index.GetMembers(far));
    }

    [Fact]
    public void MovingMemberInvalidatesOnlySourceAndDestinationBuckets()
    {
        var near = Bucket("Zombies", "Near", 30);
        var far = Bucket("Zombies", "Far", 1);
        var environment = Bucket("Environment", "Far", 1);
        var index = new EventHorizonBucketMembershipIndex();
        index.Add("cohort:9", near);
        Complete(near);
        Complete(far);
        Complete(environment);

        Assert.True(index.Move("cohort:9", far));

        Assert.False(near.HasValidRepresentation);
        Assert.False(far.HasValidRepresentation);
        Assert.True(environment.HasValidRepresentation);
        Assert.Empty(index.GetMembers(near));
        Assert.Equal(new[] { "cohort:9" }, index.GetMembers(far));
        Assert.Same(far, index.FindBucket("cohort:9"));
    }

    [Fact]
    public void UnknownOrSameBucketMoveDoesNotMutateMembership()
    {
        var bucket = Bucket("Motion", "Near", 30);
        var index = new EventHorizonBucketMembershipIndex();
        index.Add("soldier:12", bucket);
        Complete(bucket);

        Assert.False(index.Move("unknown", bucket));
        Assert.False(index.Move("soldier:12", bucket));
        Assert.True(bucket.HasValidRepresentation);
        Assert.Equal(new[] { "soldier:12" }, index.GetMembers(bucket));
    }

    [Fact]
    public void RemovingMemberInvalidatesItsFormerBucketOnly()
    {
        var near = Bucket("Motion", "Near", 30);
        var far = Bucket("Motion", "Far", 1);
        var index = new EventHorizonBucketMembershipIndex();
        index.Add("soldier:12", near);
        Complete(near);
        Complete(far);

        Assert.True(index.Remove("soldier:12"));

        Assert.False(near.HasValidRepresentation);
        Assert.True(far.HasValidRepresentation);
        Assert.Null(index.FindBucket("soldier:12"));
        Assert.Empty(index.GetMembers(near));
        Assert.Equal(0, index.Count);
        Assert.False(index.Remove("soldier:12"));
    }

    [Fact]
    public void SeparateDomainIndexesCanTrackSameEntityIndependently()
    {
        var geometry = Bucket("Geometry", "Near", 30);
        var motion = Bucket("Motion", "Near", 30);
        var geometryIndex = new EventHorizonBucketMembershipIndex();
        var motionIndex = new EventHorizonBucketMembershipIndex();

        Assert.True(geometryIndex.Add("soldier:12", geometry));
        Assert.True(motionIndex.Add("soldier:12", motion));

        Assert.Same(geometry, geometryIndex.FindBucket("soldier:12"));
        Assert.Same(motion, motionIndex.FindBucket("soldier:12"));
    }

    [Fact]
    public void MemberIdentityMustBeNonEmpty()
    {
        var index = new EventHorizonBucketMembershipIndex();
        var bucket = Bucket("Environment", "Far", 1);

        Assert.Throws<ArgumentException>(() => index.Add(" ", bucket));
        Assert.Throws<ArgumentException>(() => index.Remove(""));
        Assert.Throws<ArgumentException>(() => index.FindBucket(" "));
        Assert.Throws<ArgumentNullException>(() => index.Add("tree:1", null!));
    }

    private static EventHorizonBucket Bucket(string name, string horizon, double frequency) =>
        new(name, new EventHorizon(horizon, 100, frequency), frequency);

    private static void Complete(EventHorizonBucket bucket)
    {
        Assert.True(bucket.TryBeginRefresh(Epoch, out var ticket));
        Assert.True(bucket.TryCompleteRefresh(ticket, Epoch.AddMilliseconds(1)));
    }
}
