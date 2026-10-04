namespace TheSingularityWorkshop.Renderer;

using System.Collections.Generic;

/// <summary>
/// Provides a small, backend-independent representation-selection primitive
/// based on observer distance.
/// </summary>
public static class ObserverDistance
{
    /// <summary>
    /// Selects the first horizon whose maximum distance contains the supplied distance.
    /// Horizons must be ordered from nearest to farthest.
    /// </summary>
    /// <param name="distance">Distance from the observer in world units.</param>
    /// <param name="horizons">Horizon policies ordered from nearest to farthest.</param>
    /// <returns>The matching event horizon.</returns>
    public static EventHorizon Select(double distance, IReadOnlyList<EventHorizon> horizons)
    {
        if (double.IsNaN(distance) || double.IsInfinity(distance) || distance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(distance));
        }

        ArgumentNullException.ThrowIfNull(horizons);

        if (horizons.Count == 0)
        {
            throw new ArgumentException("At least one event horizon is required.", nameof(horizons));
        }

        var previousMaximum = -1d;

        // Indexing avoids interface-enumerator allocation in the hot selection path.
        for (var i = 0; i < horizons.Count; i++)
        {
            var horizon = horizons[i];
            ArgumentNullException.ThrowIfNull(horizon);

            if (horizon.MaximumDistance < previousMaximum)
            {
                throw new ArgumentException(
                    "Event horizons must be ordered from nearest to farthest.",
                    nameof(horizons));
            }

            previousMaximum = horizon.MaximumDistance;
        }

        for (var i = 0; i < horizons.Count; i++)
        {
            var horizon = horizons[i];

            if (distance <= horizon.MaximumDistance)
            {
                return horizon;
            }
        }

        return horizons[^1];
    }
}
