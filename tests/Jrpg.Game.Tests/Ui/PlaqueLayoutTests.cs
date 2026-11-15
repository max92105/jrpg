using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Ui;

// A plaque hugging its text (decision 646): 14 px each side, 6 px above and below, centered on the x given.
public sealed class PlaqueLayoutTests
{
    private static readonly Vector2 _attack = new(72, 24);

    [Fact]
    public void Around_Text_PadsEachSide()
    {
        var plaque = PlaqueLayout.Around(_attack, 960, 190).Plaque;

        Assert.Equal((100, 36), (plaque.Width, plaque.Height));
    }

    [Fact]
    public void Around_Text_CentersOnTheGivenX()
    {
        var plaque = PlaqueLayout.Around(_attack, 960, 190).Plaque;

        Assert.Equal((910, 190), (plaque.X, plaque.Y));
    }

    [Fact]
    public void Around_Text_PlacesTheTextInsideThePadding()
    {
        Assert.Equal(new Vector2(924, 196), PlaqueLayout.Around(_attack, 960, 190).TextPosition);
    }
}
