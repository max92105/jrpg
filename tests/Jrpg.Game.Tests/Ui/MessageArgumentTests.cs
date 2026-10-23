using System.Globalization;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

public sealed class MessageArgumentTests
{
    [Fact]
    public void Number_NegativeUnderSwedishCulture_UsesTheAsciiMinus()
    {
        var before = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("sv-SE");
        try
        {
            var argument = MessageArgument.Number("amount", -5);

            Assert.Equal(new MessageArgument("amount", "-5"), argument);
        }
        finally
        {
            CultureInfo.CurrentCulture = before;
        }
    }
}
