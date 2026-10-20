using Jrpg.Game.Battle;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Battle;

public sealed class CommandPanelPlacementTests
{
    [Fact]
    public void Place_BottomRowActor_ClampsAboveMaxHeight()
    {
        var spot = BattleLayout.PartySpot(index: 0, count: 1);

        var topLeft = CommandPanelPlacement.Place(spot);

        Assert.Equal(new Point(spot.X - 40 - 360, 1080 - 420), topLeft);
    }

    [Fact]
    public void Place_TopRowActor_StaysBelowHud()
    {
        var spot = new Rectangle(1424, 200, 96, 96);

        var topLeft = CommandPanelPlacement.Place(spot);

        Assert.Equal(260, topLeft.Y);
    }
}
