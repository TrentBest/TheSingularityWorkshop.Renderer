namespace TheSingularityWorkshop.Renderer;

/// <summary>
/// First-order performance model for Renderer scheduling overhead.
/// </summary>
/// <remarks>
/// The model is intentionally empirical. It converts measured FSM_API scheduling
/// observations into an estimate that can be compared with future Renderer benchmarks.
/// It is not a substitute for measurement of the final workload.
///
/// The initial calibration uses the Workshop FSM_API benchmark observations of
/// approximately 305.1 ns for one processing group and 15,736.6 ns for fifty groups.
/// A linear fit between those observations gives approximately 314.93 ns per
/// additional group. Measured allocation is approximately 360 bytes per group.
/// </remarks>
public sealed class RendererPerformanceModel
{
    /// <summary>
    /// Creates a performance model from empirical calibration points.
    /// </summary>
    public RendererPerformanceModel(
        double baseUpdateNanoseconds,
        double additionalGroupNanoseconds,
        double baseAllocationBytes,
        double additionalGroupAllocationBytes)
    {
        if (!double.IsFinite(baseUpdateNanoseconds) || baseUpdateNanoseconds < 0)
            throw new ArgumentOutOfRangeException(nameof(baseUpdateNanoseconds));
        if (!double.IsFinite(additionalGroupNanoseconds) || additionalGroupNanoseconds < 0)
            throw new ArgumentOutOfRangeException(nameof(additionalGroupNanoseconds));
        if (!double.IsFinite(baseAllocationBytes) || baseAllocationBytes < 0)
            throw new ArgumentOutOfRangeException(nameof(baseAllocationBytes));
        if (!double.IsFinite(additionalGroupAllocationBytes) || additionalGroupAllocationBytes < 0)
            throw new ArgumentOutOfRangeException(nameof(additionalGroupAllocationBytes));

        BaseUpdateNanoseconds = baseUpdateNanoseconds;
        AdditionalGroupNanoseconds = additionalGroupNanoseconds;
        BaseAllocationBytes = baseAllocationBytes;
        AdditionalGroupAllocationBytes = additionalGroupAllocationBytes;
    }

    /// <summary>
    /// Initial calibration derived from the published Workshop FSM_API benchmark observations.
    /// </summary>
    public static RendererPerformanceModel FromWorkshopFsmApiBenchmark() =>
        new(
            baseUpdateNanoseconds: 305.1,
            additionalGroupNanoseconds: (15_736.6 - 305.1) / 49.0,
            baseAllocationBytes: 360.0,
            additionalGroupAllocationBytes: 360.0);

    /// <summary>Measured base scheduling overhead for one processing group.</summary>
    public double BaseUpdateNanoseconds { get; }

    /// <summary>Estimated incremental scheduling overhead per additional group.</summary>
    public double AdditionalGroupNanoseconds { get; }

    /// <summary>Baseline allocation component of the calibration.</summary>
    public double BaseAllocationBytes { get; }

    /// <summary>Estimated incremental allocation per processing group.</summary>
    public double AdditionalGroupAllocationBytes { get; }

    /// <summary>
    /// Estimates one scheduler update's elapsed time for the supplied group count.
    /// </summary>
    public double EstimateUpdateNanoseconds(int processingGroups)
    {
        ValidateGroupCount(processingGroups);
        return BaseUpdateNanoseconds + (processingGroups - 1) * AdditionalGroupNanoseconds;
    }

    /// <summary>
    /// Estimates scheduler updates per second from the calibrated overhead alone.
    /// </summary>
    public double EstimateUpdatesPerSecond(int processingGroups)
    {
        var nanoseconds = EstimateUpdateNanoseconds(processingGroups);
        return nanoseconds == 0 ? double.PositiveInfinity : 1_000_000_000.0 / nanoseconds;
    }

    /// <summary>
    /// Estimates CPU milliseconds consumed per second at a specified scheduler cadence.
    /// </summary>
    public double EstimateCpuMillisecondsPerSecond(int processingGroups, double schedulerUpdatesPerSecond)
    {
        ValidateGroupCount(processingGroups);

        if (!double.IsFinite(schedulerUpdatesPerSecond) || schedulerUpdatesPerSecond < 0)
            throw new ArgumentOutOfRangeException(nameof(schedulerUpdatesPerSecond));

        return EstimateUpdateNanoseconds(processingGroups) * schedulerUpdatesPerSecond / 1_000_000.0;
    }

    /// <summary>
    /// Estimates bytes attributed to the calibrated processing-group overhead.
    /// </summary>
    public double EstimateAllocationBytes(int processingGroups)
    {
        ValidateGroupCount(processingGroups);
        return BaseAllocationBytes + (processingGroups - 1) * AdditionalGroupAllocationBytes;
    }

    private static void ValidateGroupCount(int processingGroups)
    {
        if (processingGroups < 1)
            throw new ArgumentOutOfRangeException(nameof(processingGroups));
    }
}
