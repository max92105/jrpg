using Jrpg.Core.Party;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;

namespace Jrpg.Game.Tests.Menus;

// Every menu list's pointer glides between its entries (decision 262): the category rail, the member column, the Party
// page's cards and the Inventory's sidebar. On the repository's start with the whole party: Sarxian leads alone.
public sealed class MenuListMotionTests
{
    private static readonly TimeSpan _glideDone = TimeSpan.FromSeconds(1);

    private readonly PartyState _party = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor).Party;

    [Fact]
    public void CategoryRail_MoveDownThenAdvance_ReachesTheNextTab()
    {
        var rail = new CategoryRail();
        rail.MoveDown();
        Assert.Equal(0, rail.Selection.Position);

        rail.Selection.Advance(_glideDone);

        Assert.Equal(1, rail.Selection.Position);
    }

    [Fact]
    public void CategoryRail_ResetAfterMoving_IsOnTheFirstTabAtOnce()
    {
        var rail = new CategoryRail();
        rail.MoveDown();
        rail.Selection.Advance(_glideDone);

        rail.Reset();

        Assert.Equal(0, rail.Selection.Position);
    }

    [Fact]
    public void MemberColumn_NextMemberShownThenAdvance_ReachesIt()
    {
        var members = MemberOrder.ActiveThenReserve(_party);
        var column = new MemberColumn();
        column.Refresh(members, members[0]);

        column.Refresh(members, members[1]);
        column.Selection.Advance(_glideDone);

        Assert.Equal(1, column.Selection.Position);
    }

    [Fact]
    public void PartyCursor_MoveInColumnThenAdvance_PointerReachesTheNextCard()
    {
        var cursor = new PartyCursor(_party);
        cursor.SwitchColumn();

        cursor.MoveInColumn(1);
        cursor.Pointer.Advance(_glideDone);

        Assert.Equal(1, cursor.Pointer.Position);
    }

    [Fact]
    public void PartyCursor_SwitchColumn_PointerJumpsAtOnce()
    {
        var cursor = new PartyCursor(_party);
        cursor.SwitchColumn();
        cursor.MoveInColumn(1);
        cursor.MoveInColumn(1);
        cursor.Pointer.Advance(_glideDone);

        cursor.SwitchColumn();

        Assert.Equal(0, cursor.Pointer.Position);
    }

    [Fact]
    public void PartySidebar_AimingStarts_PointerIsOnTheCardAtOnce()
    {
        var sidebar = new PartySidebar(_party);
        var members = MemberOrder.ActiveThenReserve(_party);

        sidebar.Refresh(members[2]);

        Assert.Equal(2, sidebar.Pointer.Position);
    }

    [Fact]
    public void PartySidebar_NextCardPointedThenAdvance_PointerReachesIt()
    {
        var sidebar = new PartySidebar(_party);
        var members = MemberOrder.ActiveThenReserve(_party);
        sidebar.Refresh(members[0]);

        sidebar.Refresh(members[1]);
        Assert.Equal(0, sidebar.Pointer.Position);
        sidebar.Pointer.Advance(_glideDone);

        Assert.Equal(1, sidebar.Pointer.Position);
    }
}
