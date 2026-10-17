using TheSingularityWorkshop.FSM_COS;
using TheSingularityWorkshop.MicroBundleDomain;

const ulong runtimeId = 502;
var capability = new DataOnlyBundle();
var catalog = new SampleCatalog(capability);
var manifest = new RuntimeManifest(
    RuntimeId: runtimeId,
    Bundles: [new MicroBundleManifestEntry(capability.Id, "1.0.0")]);

var assembly = new FsmCos(catalog).Execute(manifest);

if (!assembly.TryGetBundle<DataOnlyBundle>(capability.Id, out var selected) ||
    !ReferenceEquals(capability, selected) ||
    !capability.IsLoaded ||
    capability.RuntimeId != runtimeId)
{
    throw new InvalidOperationException("The non-rendering composition did not complete as expected.");
}

if (assembly.Bundles.Any(bundle => bundle.GetType().Assembly.GetName().Name?.Contains("Renderer", StringComparison.OrdinalIgnoreCase) == true))
    throw new InvalidOperationException("The non-rendering runtime unexpectedly contains a rendering capability.");

Console.WriteLine($"PASS: non-rendering manifest composed {assembly.Bundles.Count} data-only capability.");
Console.WriteLine("PASS: this project has no Renderer, Visualize, GUI.SVG, or graphics-backend project/package reference.");

sealed class SampleCatalog(IMicroBundle capability) : IMicroBundleCatalog
{
    public bool TryResolve(ulong bundleId, string version, out IMicroBundle? bundle)
    {
        if (capability.Id == bundleId &&
            string.Equals(capability.Descriptor.Version, version, StringComparison.Ordinal))
        {
            bundle = capability;
            return true;
        }

        bundle = null;
        return false;
    }

    public bool TryResolve(ulong bundleId, out IMicroBundle? bundle)
    {
        if (capability.Id == bundleId)
        {
            bundle = capability;
            return true;
        }

        bundle = null;
        return false;
    }
}

sealed class DataOnlyBundle : IMicroBundle
{
    public MicroBundleDescriptor Descriptor { get; } = new(0x4441544100000001UL, "1.0.0");
    public ulong Id => Descriptor.Id;
    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies { get; } = [];

    public ulong RuntimeId { get; private set; }
    public bool IsLoaded { get; private set; }

    public void Load(IMicroBundleLoadContext context)
    {
        RuntimeId = context.RuntimeId;
        IsLoaded = true;
    }

    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex) => false;
}
