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

        // The 400 px panel, 40 px left of the actor, its 470 px tallest height kept on screen (SZ2 sizes).
        Assert.Equal(new Point(spot.X - 40 - 400, 1080 - 470), topLeft);
    }

    [Fact]
    public void Place_TopRowActor_StaysBelowHud()
    {
        var spot = new Rectangle(1424, 200, 96, 96);

        var topLeft = CommandPanelPlacement.Place(spot);

        Assert.Equal(260, topLeft.Y);
    }
}
