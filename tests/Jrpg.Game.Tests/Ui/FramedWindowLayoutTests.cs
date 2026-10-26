using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Ui;

public sealed class FramedWindowLayoutTests
{
    private static readonly Point _center = new(960, 300);
    private static readonly Point _bodySize = new(200, 40);

    [Fact]
    public void Untitled_Frame_IsTheBodyPlusPaddingAroundTheCenter()
    {
        var layout = FramedWindowLayout.Untitled(_center, _bodySize);

        // 32 x 24 padding on each side of the 200 x 40 body.
        Assert.Equal(new Rectangle(960 - 132, 300 - 44, 264, 88), layout.Frame);
    }

    [Fact]
    public void Untitled_Body_IsInsideTheInnerFrame()
    {
        var layout = FramedWindowLayout.Untitled(_center, _bodySize);

        Assert.True(layout.InnerFrame.Contains(layout.Body));
        Assert.Equal(new Rectangle(960 - 100, 300 - 20, 200, 40), layout.Body);
    }

    [Fact]
    public void Untitled_InnerFrame_IsInsetFromTheFrame()
    {
        var layout = FramedWindowLayout.Untitled(_center, _bodySize);

        var expected = layout.Frame;
        expected.Inflate(-4, -4);
        Assert.Equal(expected, layout.InnerFrame);
    }
}
