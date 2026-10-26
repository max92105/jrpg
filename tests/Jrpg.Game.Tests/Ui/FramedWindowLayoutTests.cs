using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Ui;

public sealed class FramedWindowLayoutTests
{
    private static readonly Point _center = new(960, 300);
    private static readonly Point _bodySize = new(200, 40);
    private const float TitleLineHeight = 36;

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

    [Fact]
    public void Untitled_HasTitle_IsFalse()
    {
        var layout = FramedWindowLayout.Untitled(_center, _bodySize);

        Assert.False(layout.HasTitle);
    }

    [Fact]
    public void Titled_Body_StartsBelowTheSeparator()
    {
        var layout = FramedWindowLayout.Titled(_center, _bodySize, TitleLineHeight);

        Assert.Equal(layout.SeparatorY + 1 + 14, layout.Body.Top);
        Assert.Equal(_bodySize, layout.Body.Size);
    }

    [Fact]
    public void Titled_TitleCenter_IsAboveTheSeparator()
    {
        var layout = FramedWindowLayout.Titled(_center, _bodySize, TitleLineHeight);

        // The title's line starts 24 px into the frame, and the separator is 14 px under it.
        Assert.Equal(layout.Frame.Y + 24 + (TitleLineHeight / 2), layout.TitleCenter.Y);
        Assert.Equal(layout.Frame.Y + 24 + (int)TitleLineHeight + 14, layout.SeparatorY);
    }

    [Fact]
    public void Titled_Frame_IsCenteredOnTheCenter()
    {
        var layout = FramedWindowLayout.Titled(_center, _bodySize, TitleLineHeight);

        Assert.Equal(_center, layout.Frame.Center);
    }
}
