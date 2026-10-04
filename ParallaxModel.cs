namespace TheSingularityWorkshop.Renderer;

/// <summary>Backend-independent perspective and parallax calculations for observer-relative rendering.</summary>
public static class ParallaxModel
{
    /// <summary>Projects a camera-relative horizontal coordinate into a normalized image coordinate.</summary>
    public static double ProjectHorizontal(double horizontalPosition, double depth, double focalLength)
    {
        ValidateFinite(horizontalPosition, nameof(horizontalPosition));
        ValidatePositive(depth, nameof(depth));
        ValidatePositive(focalLength, nameof(focalLength));
        return focalLength * horizontalPosition / depth;
    }

    /// <summary>Calculates apparent horizontal image displacement caused by lateral observer movement.</summary>
    public static double LateralImageDisplacement(double observerLateralMovement, double depth, double focalLength)
    {
        ValidateFinite(observerLateralMovement, nameof(observerLateralMovement));
        ValidatePositive(depth, nameof(depth));
        ValidatePositive(focalLength, nameof(focalLength));
        return focalLength * Math.Abs(observerLateralMovement) / depth;
    }

    /// <summary>Calculates the change in projected separation between two depths after lateral observer movement.</summary>
    public static double RelativeLateralParallax(double observerLateralMovement, double nearDepth, double farDepth, double focalLength)
    {
        ValidateFinite(observerLateralMovement, nameof(observerLateralMovement));
        ValidatePositive(nearDepth, nameof(nearDepth));
        ValidatePositive(farDepth, nameof(farDepth));
        ValidatePositive(focalLength, nameof(focalLength));
        return focalLength * Math.Abs(observerLateralMovement) * Math.Abs((1.0 / nearDepth) - (1.0 / farDepth));
    }

    /// <summary>Calculates the signed view angle in radians for a camera-relative point.</summary>
    public static double ViewAngle(double horizontalPosition, double depth)
    {
        ValidateFinite(horizontalPosition, nameof(horizontalPosition));
        ValidatePositive(depth, nameof(depth));
        return Math.Atan2(horizontalPosition, depth);
    }

    private static void ValidateFinite(double value, string parameterName)
    {
        if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(parameterName);
    }

    private static void ValidatePositive(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value <= 0) throw new ArgumentOutOfRangeException(parameterName);
    }
}