using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class SemanticObservationTests
{
    private static readonly SemanticProvenance Source = new("BlacksmithWorld", 4);

    [Fact]
    public void SnapshotPreservesObserverSpecificEntitiesRelationsAndAffordances()
    {
        var forge = Entity(1, "Forge", "Active", [new SemanticAffordance("Heat", true)]);
        var workpiece = Entity(2, "MetalWorkpiece", "Heated");
        var relation = new SemanticRelationObservation(
            forge.Entity, "Heats", workpiece.Entity,
            SemanticKnowledge.Sensed, 0.95, Source);

        var observation = new SemanticObservation(
            "WorkshopAssistant", 4, SemanticObservationFreshness.Current,
            [forge, workpiece], [relation]);

        Assert.Equal(4, observation.SourceRevision);
        Assert.Equal(SemanticObservationFreshness.Current, observation.Freshness);
        Assert.Equal("Heat", Assert.Single(observation.Entities[0].Affordances).Id);
        Assert.Equal("Heats", Assert.Single(observation.Relations).Relation);
    }

    [Fact]
    public void SnapshotCopiesCallerCollections()
    {
        var entities = new List<SemanticEntityObservation> { Entity(1, "Forge", "Active") };
        var observation = new SemanticObservation(
            "Player", 4, SemanticObservationFreshness.Current, entities);
        entities.Add(Entity(2, "Hammer", "Available"));

        Assert.Single(observation.Entities);
    }

    [Fact]
    public void SnapshotRejectsRelationThatWouldDiscloseUnavailableEntity()
    {
        var visible = Entity(1, "Forge", "Active");
        var hidden = new RenderEntityId(99);
        var relation = new SemanticRelationObservation(
            visible.Entity, "AdjacentTo", hidden,
            SemanticKnowledge.Inferred, 0.5, Source);

        Assert.Throws<ArgumentException>(() =>
            new SemanticObservation("Player", 4, SemanticObservationFreshness.Current, [visible], [relation]));
    }

    [Fact]
    public void SnapshotRejectsDuplicateEntityIdentity()
    {
        var first = Entity(1, "Forge", "Active");
        var second = Entity(1, "Forge", "Cooling");

        Assert.Throws<ArgumentException>(() =>
            new SemanticObservation("Player", 4, SemanticObservationFreshness.Current, [first, second]));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void EntityRejectsInvalidConfidence(double confidence)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SemanticEntityObservation(
                new RenderEntityId(1), "Forge", "Active", "Burning", "Hot",
                SemanticKnowledge.Observed, confidence, Source));
    }

    [Fact]
    public void SnapshotRejectsFutureProvenance()
    {
        var future = new SemanticProvenance("BlacksmithWorld", 5);
        var entity = new SemanticEntityObservation(
            new RenderEntityId(1), "Forge", "Active", "Burning", "Hot",
            SemanticKnowledge.Observed, 1, future);

        Assert.Throws<ArgumentException>(() =>
            new SemanticObservation("Player", 4, SemanticObservationFreshness.Current, [entity]));
    }


    [Fact]
    public void EntityRejectsDefaultProvenanceAndDefaultAffordance()
    {
        Assert.Throws<ArgumentException>(() =>
            new SemanticEntityObservation(
                new RenderEntityId(1), "Forge", "Active", "Burning", "Hot",
                SemanticKnowledge.Observed, 1, default));

        Assert.Throws<ArgumentException>(() =>
            Entity(1, "Forge", "Active", [default]));
    }

    [Fact]
    public void SnapshotRejectsDefaultRelationValue()
    {
        var entity = Entity(1, "Forge", "Active");
        Assert.Throws<ArgumentException>(() =>
            new SemanticObservation(
                "Player", 4, SemanticObservationFreshness.Current, [entity], [default]));
    }

    private static SemanticEntityObservation Entity(
        ulong id,
        string category,
        string state,
        IEnumerable<SemanticAffordance>? affordances = null) =>
        new(
            new RenderEntityId(id), category, state, "Operating", "None",
            SemanticKnowledge.Observed, 1, Source, affordances);
}
