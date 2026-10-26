using Jrpg.Core.Party;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;

namespace Jrpg.Game.Tests.Menus;

public sealed class PartySidebarTests
{
    private readonly PartyState _party = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor).Party;

    [Fact]
    public void Cards_ActiveMembers_InPartyOrder()
    {
        var joining = _party.ReserveMembers[0];
        _party.TryAddToActive(joining);

        var sidebar = NewSidebar();

        Assert.Equal([_party.Leader, joining], sidebar.Cards.Take(_party.ActiveMembers.Count).Select(card => card.Member));
    }

    // Decision 146: the sidebar now lists the reserve too, after the active members (it used to leave them out).
    [Fact]
    public void Cards_ReserveMember_IsListedAfterTheActive()
    {
        var sidebar = NewSidebar();

        Assert.Equal([.. _party.ActiveMembers, .. _party.ReserveMembers], sidebar.Cards.Select(card => card.Member));
    }

    [Fact]
    public void Cards_ReserveMember_IsReserve()
    {
        var sidebar = NewSidebar();

        Assert.False(sidebar.Cards[0].IsReserve);
        Assert.True(sidebar.Cards[1].IsReserve);
    }

    [Fact]
    public void Refresh_MemberMovedToReserve_ListsItAfterTheActive()
    {
        var leaving = _party.ReserveMembers[0];
        _party.TryAddToActive(leaving);
        var sidebar = NewSidebar();
        Assert.False(sidebar.Cards[1].IsReserve);

        _party.TryMoveToReserve(leaving);
        sidebar.Refresh();

        var card = sidebar.Cards.Single(card => ReferenceEquals(card.Member, leaving));
        Assert.True(card.IsReserve);
        Assert.Equal([.. _party.ActiveMembers, .. _party.ReserveMembers], sidebar.Cards.Select(card => card.Member));
    }

    [Fact]
    public void Refresh_NothingChanged_KeepsTheSameCards()
    {
        var sidebar = NewSidebar();
        var before = sidebar.Cards[0];

        sidebar.Refresh();

        Assert.Same(before, sidebar.Cards[0]);
    }

    [Fact]
    public void Refresh_MemberHealed_RebuildsOnlyThatCard()
    {
        var joining = _party.ReserveMembers[0];
        _party.TryAddToActive(joining);
        joining.Vitals.TakeDamage(5);
        var sidebar = NewSidebar();
        var leaderCard = sidebar.Cards[0];

        joining.Vitals.RestoreHp(5);
        sidebar.Refresh();

        Assert.Same(leaderCard, sidebar.Cards[0]);
        Assert.Equal(joining.Vitals.Hp, sidebar.Cards[1].Hp);
    }

    [Fact]
    public void Refresh_ActiveOrderChanged_ReordersTheCards()
    {
        var first = _party.ReserveMembers[0];
        var second = _party.ReserveMembers[1];
        _party.TryAddToActive(first);
        _party.TryAddToActive(second);
        var sidebar = NewSidebar();

        _party.Reorder([_party.Leader, second, first]);
        sidebar.Refresh();

        Assert.Equal([_party.Leader, second, first], sidebar.Cards.Take(_party.ActiveMembers.Count).Select(card => card.Member));
    }

    private PartySidebar NewSidebar() => new(_party);
}
