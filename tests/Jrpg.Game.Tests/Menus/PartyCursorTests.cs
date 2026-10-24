using Jrpg.Core.Party;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;

namespace Jrpg.Game.Tests.Menus;

/// <summary>On the repository's new game: Sarxian leads alone, six recruits wait in the reserve, and four can be active.</summary>
public sealed class PartyCursorTests
{
    private readonly PartyState _party = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor).Party;

    [Fact]
    public void MoveInColumn_DownOnLastCard_WrapsToFirst()
    {
        var cursor = new PartyCursor(_party);
        cursor.SwitchColumn();
        cursor.MoveInColumn(-1);

        cursor.MoveInColumn(1);

        Assert.Same(_party.ReserveMembers[0], cursor.Selected);
    }

    [Fact]
    public void SwitchColumn_FromLowerRowThanReserve_SelectsLastReserveCard()
    {
        AddReserveMembersToActive(count: 4);
        var cursor = new PartyCursor(_party);
        cursor.MoveInColumn(-1);

        cursor.SwitchColumn();

        Assert.Same(_party.ReserveMembers[^1], cursor.Selected);
    }

    [Fact]
    public void SwitchColumn_EmptyReserve_StaysInActive()
    {
        var everyoneActive = new PartyState(_party.Members.Take(2).ToList(), _party.Members.Take(2).ToList(), new PartyRules(MaxActiveMembers: 4));
        var cursor = new PartyCursor(everyoneActive);

        cursor.SwitchColumn();

        Assert.False(cursor.IsOnReserve);
        Assert.Same(everyoneActive.Leader, cursor.Selected);
    }

    [Fact]
    public void KeepOnACard_LastReserveMemberJoined_MovesToActiveColumn()
    {
        var party = new PartyState(_party.Members.Take(2).ToList(), _party.Members.Take(1).ToList(), new PartyRules(MaxActiveMembers: 4));
        var cursor = new PartyCursor(party);
        cursor.SwitchColumn();
        party.TryAddToActive(cursor.Selected);

        cursor.KeepOnACard();

        Assert.False(cursor.IsOnReserve);
        Assert.Same(party.Leader, cursor.Selected);
    }

    [Fact]
    public void MoveToNextPlaceToSwap_Up_SkipsTheLeader()
    {
        AddReserveMembersToActive(count: 2);
        var cursor = new PartyCursor(_party);
        cursor.MoveInColumn(1);

        cursor.MoveToNextPlaceToSwap(-1);

        Assert.Same(_party.ActiveMembers[2], cursor.Selected);
    }

    [Fact]
    public void ReturnTo_FromReserve_SelectsThatActiveRow()
    {
        AddReserveMembersToActive(count: 2);
        var cursor = new PartyCursor(_party);
        cursor.SwitchColumn();

        cursor.ReturnTo(2);

        Assert.False(cursor.IsOnReserve);
        Assert.Same(_party.ActiveMembers[2], cursor.Selected);
    }

    [Fact]
    public void Reset_AfterMoving_SelectsTheLeader()
    {
        var cursor = new PartyCursor(_party);
        cursor.SwitchColumn();
        cursor.MoveInColumn(1);

        cursor.Reset();

        Assert.Same(_party.Leader, cursor.Selected);
    }

    private void AddReserveMembersToActive(int count)
    {
        for (var added = 0; added < count; added++)
        {
            Assert.True(_party.TryAddToActive(_party.ReserveMembers[0]));
        }
    }
}
