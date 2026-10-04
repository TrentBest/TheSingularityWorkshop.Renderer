using TheSingularityWorkshop.ProtocolAi;
using Xunit;

namespace TheSingularityWorkshop.Renderer.Tests;

public sealed class SemanticFormTests
{
    [Fact]
    public void SemanticForm_PreservesProtocolDefinedAnchors()
    {
        var protocol = new ProtocolBuilder(1, "Form")
            .Define(1, "Head", "head")
            .Define(2, "LeftHand", "hand.left")
            .Build();

        var head = protocol.Reference("Head");
        var hand = protocol.Reference("LeftHand");

        var form = new SemanticForm(
        [
            new SemanticAnchor(head, 0.5, 0.9, 0.0),
            new SemanticAnchor(hand, 0.2, 0.6, 0.0)
        ]);

        Assert.True(form.TryGetAnchor(head, out var headAnchor));
        Assert.Equal(0.5, headAnchor.X);
        Assert.True(form.TryGetAnchor(hand, out _));
    }
}
