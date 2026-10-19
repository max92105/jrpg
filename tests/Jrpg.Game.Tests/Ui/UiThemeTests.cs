using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Ui;

public sealed class UiThemeTests
{
    [Fact]
    public void BarLine_LegacyWhiteAt40Percent_IsPremultiplied()
    {
        Assert.Equal(new Color(0x66, 0x66, 0x66, 0x66), UiTheme.BarLine);
    }

    [Fact]
    public void NavFill_LegacyArgb_IsPremultipliedInChannelOrder()
    {
        // #D9141519: red 0x14, green 0x15 and blue 0x19, each scaled by alpha 0xD9 / 255 and truncated.
        Assert.Equal(new Color(17, 17, 21, 0xD9), UiTheme.NavFill);
    }
}
