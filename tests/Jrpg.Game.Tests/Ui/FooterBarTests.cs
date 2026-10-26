using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

public sealed class FooterBarTests
{
    [Fact]
    public void TextTop_LineShorterThanTheBar_CentersItInTheBar()
    {
        var top = FooterBar.TextTop(height: 64, lineHeight: 20);

        Assert.Equal(VirtualScreen.Height - 64 + 22, top);
    }

    [Fact]
    public void TextTop_OddGap_StaysOnAWholePixel()
    {
        var top = FooterBar.TextTop(height: 64, lineHeight: 21);

        Assert.Equal(VirtualScreen.Height - 64 + 21, top);
    }
}
