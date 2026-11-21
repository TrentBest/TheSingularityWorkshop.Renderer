namespace TheSingularityWorkshop.Renderer;

/// <summary>Describes how an observer acquired or currently holds semantic knowledge.</summary>
public enum SemanticKnowledge
{
    /// <summary>Directly observed through an enabled observation channel.</summary>
    Observed,
    /// <summary>Acquired through a non-visual sensor or explicit channel.</summary>
    Sensed,
    /// <summary>Derived from other permitted observations.</summary>
    Inferred,
    /// <summary>Previously identified, but currently blocked from direct observation.</summary>
    Occluded,
    /// <summary>The observer has no justified knowledge of this subject.</summary>
    Unknown,
    /// <summary>The observation is older than the source state it is intended to describe.</summary>
    Stale
}

/// <summary>Freshness of an observer's semantic snapshot relative to its authoritative source.</summary>
public enum SemanticObservationFreshness
{
    /// <summary>The observation was derived from the declared source revision.</summary>
    Current,
    /// <summary>The source has advanced or the observation is otherwise known to be out of date.</summary>
    Stale
}

/// <summary>Identifies the source and exact revision supporting a semantic claim.</summary>
public readonly record struct SemanticProvenance
{
    /// <summary>Creates explicit provenance for a semantic claim.</summary>
    public SemanticProvenance(string sourceId, long sourceRevision)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceId);
        ArgumentOutOfRangeException.ThrowIfNegative(sourceRevision);
        SourceId = sourceId;
        SourceRevision = sourceRevision;
    }

    /// <summary>Stable identifier of the source contract or channel.</summary>
    public string SourceId { get; }

    /// <summary>Exact revision of that source.</summary>
    public long SourceRevision { get; }

    /// <summary>Whether this value was created with a non-empty source and valid revision.</summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(SourceId) && SourceRevision >= 0;
}

/// <summary>A named action or capability that an observer is justified in associating with an entity.</summary>
public readonly record struct SemanticAffordance
{
    /// <summary>Creates an affordance observation.</summary>
    public SemanticAffordance(string id, bool isAvailable)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Id = id;
        IsAvailable = isAvailable;
    }

    /// <summary>Stable, producer-defined affordance identifier.</summary>
    public string Id { get; }

    /// <summary>Whether the affordance is currently available to this observer or actor.</summary>
    public bool IsAvailable { get; }
}

/// <summary>Observer-relative semantic information about one stable subject.</summary>
public sealed class SemanticEntityObservation
{
    private readonly IReadOnlyList<SemanticAffordance> _affordances;

    /// <summary>Creates a validated semantic entity observation.</summary>
    public SemanticEntityObservation(
        RenderEntityId entity,
        string category,
        string state,
        string activity,
        string hazard,
        SemanticKnowledge knowledge,
        double confidence,
        SemanticProvenance provenance,
        IEnumerable<SemanticAffordance>? affordances = null)
    {
        if (!entity.IsValid)
            throw new ArgumentException("A semantic observation requires a stable entity identity.", nameof(entity));
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(activity);
        ArgumentException.ThrowIfNullOrWhiteSpace(hazard);
        if (!Enum.IsDefined(knowledge) || knowledge == SemanticKnowledge.Unknown)
            throw new ArgumentOutOfRangeException(nameof(knowledge), "Unknown subjects must not be represented with a disclosed entity identity.");
        if (!provenance.IsValid)
            throw new ArgumentException("Valid provenance is required.", nameof(provenance));
        ValidateConfidence(confidence);

        var values = (affordances ?? []).ToArray();
        if (values.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() != values.Length)
            throw new ArgumentException("Affordance identifiers must be unique within an entity observation.", nameof(affordances));

        Entity = entity;
        Category = category;
        State = state;
        Activity = activity;
        Hazard = hazard;
        Knowledge = knowledge;
        Confidence = confidence;
        Provenance = provenance;
        _affordances = Array.AsReadOnly(values);
    }

    /// <summary>Stable identity of the observed subject, independent of its current representation.</summary>
    public RenderEntityId Entity { get; }
    /// <summary>Producer-defined semantic category.</summary>
    public string Category { get; }
    /// <summary>Current state made available to this observer.</summary>
    public string State { get; }
    /// <summary>Current activity made available to this observer.</summary>
    public string Activity { get; }
    /// <summary>Hazard classification made available to this observer.</summary>
    public string Hazard { get; }
    /// <summary>How the observer knows this information.</summary>
    public SemanticKnowledge Knowledge { get; }
    /// <summary>Producer-assigned confidence in this observation, from zero to one.</summary>
    public double Confidence { get; }
    /// <summary>Source and revision supporting this observation.</summary>
    public SemanticProvenance Provenance { get; }
    /// <summary>Observer-relevant actions or capabilities associated with this entity.</summary>
    public IReadOnlyList<SemanticAffordance> Affordances => _affordances;

    internal static void ValidateConfidence(double confidence)
    {
        if (!double.IsFinite(confidence) || confidence is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(confidence), "Confidence must be finite and between zero and one.");
    }
}

/// <summary>A typed relation between two subjects already present in the same observer snapshot.</summary>
public readonly record struct SemanticRelationObservation
{
    /// <summary>Creates a relation observation with explicit provenance and confidence.</summary>
    public SemanticRelationObservation(
        RenderEntityId subject,
        string relation,
        RenderEntityId target,
        SemanticKnowledge knowledge,
        double confidence,
        SemanticProvenance provenance)
    {
        if (!subject.IsValid)
            throw new ArgumentException("A relation requires a valid subject identity.", nameof(subject));
        if (!target.IsValid)
            throw new ArgumentException("A relation requires a valid target identity.", nameof(target));
        if (subject == target)
            throw new ArgumentException("A relation cannot connect an entity to itself.", nameof(target));
        ArgumentException.ThrowIfNullOrWhiteSpace(relation);
        if (!Enum.IsDefined(knowledge) || knowledge == SemanticKnowledge.Unknown)
            throw new ArgumentOutOfRangeException(nameof(knowledge), "Unknown relations must not disclose entity identities.");
        if (!provenance.IsValid)
            throw new ArgumentException("Valid provenance is required.", nameof(provenance));
        SemanticEntityObservation.ValidateConfidence(confidence);

        Subject = subject;
        Relation = relation;
        Target = target;
        Knowledge = knowledge;
        Confidence = confidence;
        Provenance = provenance;
    }

    /// <summary>Stable identity of the relation subject.</summary>
    public RenderEntityId Subject { get; }
    /// <summary>Producer-defined relation identifier.</summary>
    public string Relation { get; }
    /// <summary>Stable identity of the relation target.</summary>
    public RenderEntityId Target { get; }
    /// <summary>How the observer knows this relation.</summary>
    public SemanticKnowledge Knowledge { get; }
    /// <summary>Producer-assigned confidence in this relation, from zero to one.</summary>
    public double Confidence { get; }
    /// <summary>Source and revision supporting this relation.</summary>
    public SemanticProvenance Provenance { get; }
}

/// <summary>
/// Immutable semantic projection for one observer and one authoritative source revision.
/// Only subjects the observer is permitted to know are included; hidden entity identities
/// must not be placed in an "unknown" list because doing so would disclose their existence.
/// </summary>
public sealed class SemanticObservation
{
    private readonly IReadOnlyList<SemanticEntityObservation> _entities;
    private readonly IReadOnlyList<SemanticRelationObservation> _relations;

    /// <summary>Creates a validated observer-relative semantic snapshot.</summary>
    public SemanticObservation(
        string observerId,
        long sourceRevision,
        SemanticObservationFreshness freshness,
        IEnumerable<SemanticEntityObservation> entities,
        IEnumerable<SemanticRelationObservation>? relations = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(observerId);
        ArgumentOutOfRangeException.ThrowIfNegative(sourceRevision);
        if (!Enum.IsDefined(freshness))
            throw new ArgumentOutOfRangeException(nameof(freshness));
        ArgumentNullException.ThrowIfNull(entities);

        var entityValues = entities.ToArray();
        if (entityValues.Select(x => x.Entity).Distinct().Count() != entityValues.Length)
            throw new ArgumentException("A semantic snapshot cannot contain the same entity more than once.", nameof(entities));
        if (entityValues.Any(x => x.Provenance.SourceRevision > sourceRevision))
            throw new ArgumentException("An entity observation cannot originate from a future source revision.", nameof(entities));

        var relationValues = (relations ?? []).ToArray();
        var entityIds = entityValues.Select(x => x.Entity).ToHashSet();
        if (relationValues.Any(x => !entityIds.Contains(x.Subject) || !entityIds.Contains(x.Target)))
            throw new ArgumentException("Both endpoints of every relation must be present in this observer's semantic snapshot.", nameof(relations));
        if (relationValues.Any(x => x.Provenance.SourceRevision > sourceRevision))
            throw new ArgumentException("A relation cannot originate from a future source revision.", nameof(relations));

        ObserverId = observerId;
        SourceRevision = sourceRevision;
        Freshness = freshness;
        _entities = Array.AsReadOnly(entityValues);
        _relations = Array.AsReadOnly(relationValues);
    }

    /// <summary>Stable identifier for the observer or observing role.</summary>
    public string ObserverId { get; }
    /// <summary>Authoritative source revision projected into this observation.</summary>
    public long SourceRevision { get; }
    /// <summary>Whether this projection is current or stale relative to its source.</summary>
    public SemanticObservationFreshness Freshness { get; }
    /// <summary>Only entity knowledge available to this observer.</summary>
    public IReadOnlyList<SemanticEntityObservation> Entities => _entities;
    /// <summary>Relations whose two endpoints are both available to this observer.</summary>
    public IReadOnlyList<SemanticRelationObservation> Relations => _relations;
}
