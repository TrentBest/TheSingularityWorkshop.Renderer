using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class HorizonRepresentationHandoffTests
{
    [Fact]
    public void StagingUnreadyCandidateKeepsCurrentRepresentationVisible()
    {
        var handoff = CreateHandoff();
        var initial = handoff.Snapshot;

        handoff.Stage(Representation("near", "Near", ready: false, covers: false));

        var staged = handoff.Snapshot;
        Assert.Equal("far", staged.Current.Id);
        Assert.Equal("near", staged.Pending!.Id);
        Assert.True(staged.Generation > initial.Generation);
        Assert.False(handoff.TryCommit(currentSourceRevision: 7, currentObserverRevision: 12));
        Assert.Equal("far", handoff.Snapshot.Current.Id);
    }

    [Fact]
    public void CandidateCommitsOnlyAfterReadyAndViewCoverageAreConfirmed()
    {
        var handoff = CreateHandoff();
        handoff.Stage(Representation("near", "Near", ready: false, covers: false));

        Assert.True(handoff.UpdatePending("near", isReady: true, coversObserverView: false, observerRevision: 12));
        Assert.False(handoff.TryCommit(7, 12));
        Assert.Equal("far", handoff.Snapshot.Current.Id);

        Assert.True(handoff.UpdatePending("near", isReady: true, coversObserverView: true, observerRevision: 12));
        Assert.True(handoff.TryCommit(7, 12));
        Assert.Equal("near", handoff.Snapshot.Current.Id);
        Assert.Null(handoff.Snapshot.Pending);
    }

    [Theory]
    [InlineData(8, 12)]
    [InlineData(7, 13)]
    public void StaleSourceOrObserverRevisionCannotCommit(long sourceRevision, long observerRevision)
    {
        var handoff = CreateHandoff();
        handoff.Stage(Representation("near", "Near", ready: true, covers: true));

        Assert.False(handoff.TryCommit(sourceRevision, observerRevision));
        Assert.Equal("far", handoff.Snapshot.Current.Id);
        Assert.Equal("near", handoff.Snapshot.Pending!.Id);
    }

    [Fact]
    public void NewCandidateSupersedesOldPendingWorkWhenObserverReversesDirection()
    {
        var handoff = CreateHandoff();
        handoff.Stage(Representation("near", "Near", ready: false, covers: false));
        handoff.Stage(Representation("far-refreshed", "Far", ready: true, covers: true));

        var snapshot = handoff.Snapshot;
        Assert.Equal("far", snapshot.Current.Id);
        Assert.Equal("far-refreshed", snapshot.Pending!.Id);
        Assert.False(handoff.UpdatePending("near", true, true, 12));
        Assert.True(handoff.TryCommit(7, 12));
        Assert.Equal("far-refreshed", handoff.Snapshot.Current.Id);
    }

    [Fact]
    public void OldCandidateCannotCommitAfterPendingWorkWasCancelledOrReplaced()
    {
        var handoff = CreateHandoff();
        handoff.Stage(Representation("near", "Near", ready: true, covers: true));

        Assert.False(handoff.CancelPending("different"));
        Assert.True(handoff.CancelPending("near"));
        Assert.False(handoff.TryCommit(7, 12));
        Assert.Equal("far", handoff.Snapshot.Current.Id);
    }

    [Fact]
    public void CurrentRepresentationCannotAlsoBeStagedAsPending()
    {
        var handoff = CreateHandoff();

        Assert.Throws<ArgumentException>(() => handoff.Stage(Representation("far", "Far", true, true)));
    }

    [Fact]
    public void InitialRepresentationMustAlreadyBeReadyAndCoverTheView()
    {
        Assert.Throws<ArgumentException>(() => new HorizonRepresentationHandoff(
            Representation("far", "Far", ready: false, covers: true)));
        Assert.Throws<ArgumentException>(() => new HorizonRepresentationHandoff(
            Representation("far", "Far", ready: true, covers: false)));
    }

    [Fact]
    public void RepresentationRequiresIdentityAndHorizon()
    {
        Assert.Throws<ArgumentException>(() => new HorizonRepresentationHandoff(
            new HorizonRepresentation(" ", "Far", 7, 12, true, true)));
        Assert.Throws<ArgumentException>(() => new HorizonRepresentationHandoff(
            new HorizonRepresentation("far", " ", 7, 12, true, true)));
    }

    private static HorizonRepresentationHandoff CreateHandoff() =>
        new(Representation("far", "Far", ready: true, covers: true));

    private static HorizonRepresentation Representation(
        string id,
        string horizon,
        bool ready,
        bool covers) =>
        new(id, horizon, SourceRevision: 7, ObserverRevision: 12, IsReady: ready, CoversObserverView: covers);
}
