using TheSingularityWorkshop.FSM_COS;
using TheSingularityWorkshop.MicroBundleDomain;
using TheSingularityWorkshop.Renderer;

const ulong runtimeId = 501;
var renderer = new RendererMicroBundle();
var catalog = new SampleCatalog(renderer);
var manifest = new RuntimeManifest(
    RuntimeId: runtimeId,
    Bundles: [new MicroBundleManifestEntry(RendererMicroBundle.BundleId, "0.1.0-alpha.2")]);

var assembly = new FsmCos(catalog).Execute(manifest);

if (!assembly.TryGetBundle<RendererMicroBundle>(RendererMicroBundle.BundleId, out var selected) ||
    !ReferenceEquals(renderer, selected))
{
    throw new InvalidOperationException("FSM_COS did not return the explicitly requested Renderer capability.");
}

if (!renderer.IsLoaded || renderer.RuntimeId != runtimeId)
    throw new InvalidOperationException("The selected Renderer capability did not receive the expected load context.");

if (assembly.Bundles.Count != 1 || assembly.Bundles[0].Id != RendererMicroBundle.BundleId)
    throw new InvalidOperationException("The composed runtime contains unexpected capabilities.");

Console.WriteLine($"PASS: manifest opted in to Renderer bundle 0x{renderer.Id:X16}.");
Console.WriteLine($"PASS: RuntimeAssembly returned Renderer; loaded={renderer.IsLoaded}; runtime={renderer.RuntimeId}.");
Console.WriteLine("SCOPE: lifecycle/composition proof only; no Visualize or graphics backend is activated.");

sealed class SampleCatalog(IMicroBundle renderer) : IMicroBundleCatalog
{
    public bool TryResolve(ulong bundleId, string version, out IMicroBundle? bundle)
    {
        if (renderer.Id == bundleId &&
            string.Equals(renderer.Descriptor.Version, version, StringComparison.Ordinal))
        {
            bundle = renderer;
            return true;
        }

        bundle = null;
        return false;
    }

    public bool TryResolve(ulong bundleId, out IMicroBundle? bundle)
    {
        if (renderer.Id == bundleId)
        {
            bundle = renderer;
            return true;
        }

        bundle = null;
        return false;
    }
}
