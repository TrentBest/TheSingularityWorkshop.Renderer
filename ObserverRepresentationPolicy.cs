namespace TheSingularityWorkshop.Renderer;

/// <summary>Associates an Event Horizon policy with the computational representation it selects.</summary>
/// <remarks>
/// This is an explicit bridge between distance/horizon selection and FSM representation state.
/// Horizon names remain descriptive; callers do not need to parse names to determine representation.
/// </remarks>
public sealed record RepresentationHorizonRule
{
    /// <summary>Creates a representation rule for one Event Horizon.</summary>
    public RepresentationHorizonRule(EventHorizon horizon, RepresentationTarget representation)
    {
        ArgumentNullException.ThrowIfNull(horizon);
        if (!Enum.IsDefined(representation))
            throw new ArgumentOutOfRangeException(nameof(representation));

        Horizon = horizon;
        Representation = representation;
    }

    /// <summary>Gets the horizon used to select this rule.</summary>
    public EventHorizon Horizon { get; }

    /// <summary>Gets the representation desired when this horizon is selected.</summary>
    public RepresentationTarget Representation { get; }
}

/// <summary>A stable result from observer-distance representation selection.</summary>
/// <param name="Distance">Observer-to-entity distance in the caller's world units.</param>
/// <param name="Horizon">The selected horizon object, retained for downstream bucket and cadence selection.</param>
/// <param name="Representation">Representation requested by the selected rule.</param>
/// <remarks>
/// Retaining the horizon object lets downstream stages map the decision to its bucket without
/// parsing a display name or reconstructing horizon identity.
/// </remarks>
public readonly record struct ObserverRepresentationDecision(
    double Distance,
    EventHorizon Horizon,
    RepresentationTarget Representation)
{
    /// <summary>Gets the selected horizon's display name.</summary>
    public string HorizonName => Horizon.Name;

    /// <summary>Gets the selected horizon's preferred evaluation cadence.</summary>
    public double UpdatesPerSecond => Horizon.UpdatesPerSecond;
}

/// <summary>
/// Selects an explicit representation rule for an observer distance using Renderer Event Horizon policies.
/// </summary>
/// <remarks>
/// This policy composes existing distance selection with an explicit horizon-to-representation mapping.
/// It does not advance FSM_API, schedule bucket refreshes, determine occlusion, or create a RenderFrame.
/// Those responsibilities remain separate so a host can inspect the decision before executing work.
/// </remarks>
public sealed class ObserverRepresentationPolicy
{
    private readonly EventHorizon[] _horizons;
    private readonly Dictionary<EventHorizon, RepresentationHorizonRule> _ruleByHorizon;

    /// <summary>Creates a policy from horizons ordered from nearest to farthest.</summary>
    /// <param name="rules">Ordered horizon-to-representation rules.</param>
    public ObserverRepresentationPolicy(IReadOnlyList<RepresentationHorizonRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        if (rules.Count == 0)
            throw new ArgumentException("At least one representation horizon rule is required.", nameof(rules));

        _horizons = new EventHorizon[rules.Count];
        _ruleByHorizon = new Dictionary<EventHorizon, RepresentationHorizonRule>();
        var previousMaximum = -1d;

        for (var i = 0; i < rules.Count; i++)
        {
            var rule = rules[i] ?? throw new ArgumentException(
                $"Representation horizon rule at index {i} is null.", nameof(rules));
            var horizon = rule.Horizon;

            if (horizon.MaximumDistance < previousMaximum)
                throw new ArgumentException(
                    "Representation horizon rules must be ordered from nearest to farthest.",
                    nameof(rules));

            if (!_ruleByHorizon.TryAdd(horizon, rule))
                throw new ArgumentException(
                    $"Event Horizon '{horizon.Name}' appears more than once in the policy.",
                    nameof(rules));

            _horizons[i] = horizon;
            previousMaximum = horizon.MaximumDistance;
        }
    }

    /// <summary>Selects the horizon and its mapped representation for one observer distance.</summary>
    /// <param name="distance">Distance from the observer in world units.</param>
    /// <returns>The selected horizon name, desired representation, and preferred evaluation cadence.</returns>
    public ObserverRepresentationDecision Select(double distance)
    {
        var horizon = ObserverDistance.Select(distance, _horizons);
        var rule = _ruleByHorizon[horizon];
        return new ObserverRepresentationDecision(distance, horizon, rule.Representation);
    }
}