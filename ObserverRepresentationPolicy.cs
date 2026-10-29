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

/// <summary>A stable result from observer representation selection.</summary>
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
/// Selects an explicit representation rule for an observer using distance and an optional
/// minimum representation requirement.
/// </summary>
/// <remarks>
/// Distance provides the baseline horizon. A caller-supplied minimum representation is an
/// independent observer/experience signal: it may promote a distant entity when interaction,
/// inspection, or another correctness requirement needs richer output. The selected rule's
/// horizon and cadence travel together, so a promotion also selects a compatible bucket/cadence.
/// This policy does not infer intent or domain importance; the caller owns those facts.
/// It does not advance FSM_API, schedule refreshes, determine occlusion, or create a RenderFrame.
/// </remarks>
public sealed class ObserverRepresentationPolicy
{
    private readonly EventHorizon[] _horizons;
    private readonly RepresentationHorizonRule[] _rules;
    private readonly Dictionary<EventHorizon, RepresentationHorizonRule> _ruleByHorizon;

    /// <summary>Creates a policy from horizons ordered from nearest to farthest.</summary>
    /// <param name="rules">Ordered horizon-to-representation rules.</param>
    public ObserverRepresentationPolicy(IReadOnlyList<RepresentationHorizonRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        if (rules.Count == 0)
            throw new ArgumentException("At least one representation horizon rule is required.", nameof(rules));

        _horizons = new EventHorizon[rules.Count];
        _rules = new RepresentationHorizonRule[rules.Count];
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
            _rules[i] = rule;
            previousMaximum = horizon.MaximumDistance;
        }
    }

    /// <summary>Selects the horizon and its mapped representation for one observer distance.</summary>
    /// <param name="distance">Distance from the observer in world units.</param>
    /// <returns>The selected horizon, desired representation, and preferred evaluation cadence.</returns>
    public ObserverRepresentationDecision Select(double distance)
    {
        var horizon = ObserverDistance.Select(distance, _horizons);
        var rule = _ruleByHorizon[horizon];
        return new ObserverRepresentationDecision(distance, horizon, rule.Representation);
    }

    /// <summary>
    /// Selects using distance while honoring a caller-supplied minimum representation requirement.
    /// </summary>
    /// <param name="distance">Distance from the observer in world units.</param>
    /// <param name="minimumRepresentation">
    /// The least detailed representation acceptable to the caller. This is an input requirement,
    /// not an inferred property of distance or a guarantee of interaction semantics.
    /// </param>
    /// <returns>
    /// The distance-selected rule when it satisfies the requirement; otherwise the nearest rule
    /// in policy order that satisfies it. The selected horizon determines cadence and bucket identity.
    /// </returns>
    public ObserverRepresentationDecision Select(
        double distance,
        RepresentationTarget minimumRepresentation)
    {
        if (!Enum.IsDefined(minimumRepresentation))
            throw new ArgumentOutOfRangeException(nameof(minimumRepresentation));

        var distanceDecision = Select(distance);
        if ((int)distanceDecision.Representation >= (int)minimumRepresentation)
            return distanceDecision;

        // Pick the least-detailed configured target that satisfies the floor. If a target
        // appears at multiple horizons, preserve the first (nearest) rule in policy order.
        RepresentationHorizonRule? satisfyingRule = null;
        for (var i = 0; i < _rules.Length; i++)
        {
            var rule = _rules[i];
            if ((int)rule.Representation < (int)minimumRepresentation)
                continue;

            if (satisfyingRule is null ||
                (int)rule.Representation < (int)satisfyingRule.Representation)
            {
                satisfyingRule = rule;
            }
        }

        if (satisfyingRule is not null)
            return new ObserverRepresentationDecision(
                distance, satisfyingRule.Horizon, satisfyingRule.Representation);

        // This policy cannot satisfy the requested floor. Return its most detailed available
        // rule rather than claiming that an unsupported representation exists.
        var best = _rules[0];
        for (var i = 1; i < _rules.Length; i++)
        {
            if ((int)_rules[i].Representation > (int)best.Representation)
                best = _rules[i];
        }

        return new ObserverRepresentationDecision(distance, best.Horizon, best.Representation);
    }
}
