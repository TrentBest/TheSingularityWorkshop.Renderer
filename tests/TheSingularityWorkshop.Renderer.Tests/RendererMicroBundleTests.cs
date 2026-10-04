using TheSingularityWorkshop.MicroBundleDomain;
using TheSingularityWorkshop.Renderer;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class RendererMicroBundleTests
{
    [Fact]
    public void DescribesRendererAsIndependentCapability()
    {
        var bundle = new RendererMicroBundle();

        Assert.Equal(RendererMicroBundle.BundleId, bundle.Id);
        Assert.Equal(RendererMicroBundle.Ontology, bundle.Descriptor.Providers.Single().Id);
        Assert.Empty(bundle.Dependencies);
        Assert.Equal("0.1.0-alpha.2", bundle.Descriptor.Version);
    }

    [Fact]
    public void LoadsOpaqueConfigurationWithoutDefiningItsFormat()
    {
        var bundle = new RendererMicroBundle();
        var configuration = new byte[] { 1, 2, 3, 5, 8 };
        var context = new TestLoadContext(42, configuration);

        bundle.Load(context);

        Assert.True(bundle.IsLoaded);
        Assert.Equal(42UL, bundle.RuntimeId);
        Assert.Equal(configuration, bundle.Configuration.ToArray());
    }

    [Fact]
    public void DoesNotArbitrateWithoutACompositionChange()
    {
        var bundle = new RendererMicroBundle();
        var context = new TestArbitrationContext(42, [bundle]);

        Assert.False(bundle.Arbitrate(context, 0));
    }

    private sealed class TestLoadContext(ulong runtimeId, byte[] configuration) : IMicroBundleLoadContext
    {
        public ulong RuntimeId { get; } = runtimeId;

        public bool TryGetConfiguration(ulong bundleId, out ReadOnlyMemory<byte> value)
        {
            value = bundleId == RendererMicroBundle.BundleId
                ? configuration
                : ReadOnlyMemory<byte>.Empty;
            return bundleId == RendererMicroBundle.BundleId;
        }
    }

    private sealed class TestArbitrationContext(ulong runtimeId, IReadOnlyList<IMicroBundle> bundles)
        : IMicroBundleArbitrationContext
    {
        public ulong RuntimeId { get; } = runtimeId;
        public IReadOnlyList<IMicroBundle> Bundles { get; } = bundles;
        public object? ExperienceContext => null;
    }
}
