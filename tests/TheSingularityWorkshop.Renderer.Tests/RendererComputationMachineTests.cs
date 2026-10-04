using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class RendererComputationMachineTests
{
    [Fact]
    public void RepresentationComputationUsesFsmStateTransitions()
    {
        var context = new RendererComputationContext(
            "Tree-001",
            RepresentationTarget.Near);

        using var machine = new RendererComputationMachine(context);

        Assert.Equal(RepresentationTarget.Semantic, machine.CurrentRepresentation);
        Assert.Equal(RepresentationTarget.Semantic, context.ActiveRepresentation);

        Assert.Equal(RepresentationTarget.Near, machine.Advance());
        Assert.Equal(RepresentationTarget.Near, context.ActiveRepresentation);

        context.DesiredRepresentation = RepresentationTarget.Interactive;

        Assert.Equal(RepresentationTarget.Interactive, machine.Advance());
        Assert.Equal(RepresentationTarget.Interactive, context.ActiveRepresentation);

        context.DesiredRepresentation = RepresentationTarget.Semantic;

        Assert.Equal(RepresentationTarget.Semantic, machine.Advance());
        Assert.Equal(RepresentationTarget.Semantic, context.ActiveRepresentation);
    }

    [Fact]
    public void ContextRequiresAName()
    {
        Assert.Throws<ArgumentException>(
            () => new RendererComputationContext(" "));
    }

    [Fact]
    public void MachineRejectsNullContext()
    {
        Assert.Throws<ArgumentNullException>(
            () => new RendererComputationMachine(null!));
    }

    [Fact]
    public void DisposedMachineCannotAdvance()
    {
        using var machine = new RendererComputationMachine(
            new RendererComputationContext("Tree-Disposed"));

        machine.Dispose();

        Assert.Throws<ObjectDisposedException>(() => machine.Advance());
    }
}
