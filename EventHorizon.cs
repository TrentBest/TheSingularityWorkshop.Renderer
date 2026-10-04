namespace TheSingularityWorkshop.Renderer;

/// <summary>
/// Describes a computational-detail boundary used to select how an observable
/// entity should be represented for an observer.
/// </summary>
public sealed class EventHorizon
{
    /// <summary>
    /// Creates an event horizon.
    /// </summary>
    /// <param name="name">Human-readable policy name.</param>
    /// <param name="maximumDistance">Maximum distance, in world units, for this horizon.</param>
    /// <param name="updatesPerSecond">Preferred evaluation frequency for this horizon.</param>
    public EventHorizon(string name, double maximumDistance, double updatesPerSecond)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("An event horizon name is required.", nameof(name));
        }

        if (double.IsNaN(maximumDistance) || double.IsInfinity(maximumDistance) || maximumDistance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumDistance));
        }

        if (double.IsNaN(updatesPerSecond) || double.IsInfinity(updatesPerSecond) || updatesPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(updatesPerSecond));
        }

        Name = name;
        MaximumDistance = maximumDistance;
        UpdatesPerSecond = updatesPerSecond;
    }

    /// <summary>Gets the human-readable horizon name.</summary>
    public string Name { get; }

    /// <summary>Gets the maximum distance covered by this horizon.</summary>
    public double MaximumDistance { get; }

    /// <summary>Gets the preferred evaluation frequency for this horizon.</summary>
    public double UpdatesPerSecond { get; }
}
