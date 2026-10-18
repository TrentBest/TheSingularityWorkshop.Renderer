using TheSingularityWorkshop.MicroBundleDomain;

namespace TheSingularityWorkshop.Renderer;

/// <summary>
/// Exposes the core Renderer as a MicroBundle capability.
/// </summary>
/// <remarks>
/// The Renderer package contains the rendering model and its MicroBundle adapter in the
/// same package deliberately. FSM_COS composes this capability; it does not need a direct
/// dependency on the Renderer package. Additional rendering capabilities can arrive as
/// separate MicroBundles without making the composition kernel aware of them.
/// </remarks>
public sealed class RendererMicroBundle : IMicroBundle
{
    /// <summary>Stable ontology name for the core rendering capability family.</summary>
    public const string Ontology = "Renderer";

    /// <summary>Stable identity of the core Renderer MicroBundle.</summary>
    public const ulong BundleId = 0x52454E4445520001UL;

    /// <summary>Creates the core Renderer MicroBundle.</summary>
    public RendererMicroBundle()
    {
        Descriptor = new MicroBundleDescriptor(
            BundleId,
            "0.1.0-alpha.2",
            providers: [new MicroBundleProvider(Ontology)]);

        Dependencies = Array.Empty<MicroBundleDependencyRequest>();
    }

    /// <summary>Gets the descriptor advertised for this Renderer capability bundle.</summary>
    public MicroBundleDescriptor Descriptor { get; }

    /// <summary>Gets the dependency requests required by this bundle.</summary>
    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies { get; }

    /// <summary>Gets the runtime identity assigned by the composition host during loading.</summary>
    public ulong RuntimeId { get; private set; }

    /// <summary>Gets the configuration bytes supplied for this bundle, or an empty value when none were supplied.</summary>
    public ReadOnlyMemory<byte> Configuration { get; private set; }

    /// <summary>Gets a value indicating whether this bundle has been loaded into a runtime.</summary>
    public bool IsLoaded { get; private set; }

    /// <inheritdoc />
    public void Load(IMicroBundleLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        RuntimeId = context.RuntimeId;
        Configuration = context.TryGetConfiguration(BundleId, out var configuration)
            ? configuration
            : ReadOnlyMemory<byte>.Empty;
        IsLoaded = true;
    }

    /// <inheritdoc />
    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentOutOfRangeException.ThrowIfNegative(roundIndex);
        return false;
    }
}
