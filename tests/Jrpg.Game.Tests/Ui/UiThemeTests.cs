using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Ui;

public sealed class UiThemeTests
{
    [Fact]
    public void BarLine_WhiteAt40Percent_IsPremultiplied()
    {
        Assert.Equal(new Color(0x66, 0x66, 0x66, 0x66), UiTheme.BarLine);
    }

    [Fact]
    public void NavFill_StraightArgb_IsPremultipliedInChannelOrder()
    {
        // #D9141519: red 0x14, green 0x15 and blue 0x19, each scaled by alpha 0xD9 / 255 and truncated.
        Assert.Equal(new Color(17, 17, 21, 0xD9), UiTheme.NavFill);
    }

    [Fact]
    public void PreviewColorOf_Rise_IsPositive()
    {
        Assert.Equal(UiTheme.Positive, UiTheme.PreviewColorOf(1));
    }

    [Fact]
    public void PreviewColorOf_Drop_IsNegative()
    {
        Assert.Equal(UiTheme.Negative, UiTheme.PreviewColorOf(-1));
    }

    [Fact]
    public void PreviewColorOf_NoChange_IsPrimaryText()
    {
        Assert.Equal(UiTheme.TextPrimary, UiTheme.PreviewColorOf(0));
    }

    [Theory]
    [InlineData("#5B8CFF")]
    [InlineData("#5b8cff")]
    public void ColorFromHex_Valid_Parses(string hex)
    {
        Assert.Equal(new Color(0x5B, 0x8C, 0xFF, 0xFF), UiTheme.ColorFromHex(hex));
    }
}
