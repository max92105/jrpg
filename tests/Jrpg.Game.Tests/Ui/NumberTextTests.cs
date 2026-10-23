using System.Globalization;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

public sealed class NumberTextTests
{
    // Swedish writes a negative number with U+2212, which the game's fonts may lack; the game always writes "-".
    [Fact]
    public void Of_NegativeUnderSwedishCulture_UsesTheAsciiMinus()
    {
        var before = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("sv-SE");
        try
        {
            var text = NumberText.Of(-12);

            Assert.Equal("-12", text);
        }
        finally
        {
            CultureInfo.CurrentCulture = before;
        }
    }
}
