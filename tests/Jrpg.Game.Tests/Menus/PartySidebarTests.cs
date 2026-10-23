using Jrpg.Core.Party;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;

namespace Jrpg.Game.Tests.Menus;

public sealed class PartySidebarTests
{
    private readonly PartyState _party = GameSession.StartNew(StartMapScenarios.BelowClosedDoor, new SeededRandom(1)).Party;

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
    public void Refresh_ShownMember_MarksItsCard()
    {
        var sidebar = NewSidebar();
        var shown = _party.ReserveMembers[1];

        sidebar.Refresh(shown);

        Assert.Same(shown, sidebar.Cards[sidebar.ShownIndex ?? -1].Member);
    }

    [Fact]
    public void Refresh_NoShownMember_MarksNone()
    {
        var sidebar = NewSidebar();
        sidebar.Refresh(_party.Leader);
        Assert.Equal(0, sidebar.ShownIndex);

        sidebar.Refresh(null);

        Assert.Null(sidebar.ShownIndex);
    }

    // The start has more recruits than cards in view, so showing the last one scrolls the view to end on it.
    [Fact]
    public void Refresh_ShownMemberBelowTheWindow_ScrollsToShowIt()
    {
        var sidebar = NewSidebar();
        Assert.True(sidebar.Cards.Count > PartySidebar.VisibleCards, "The test needs more recruits than cards in view.");
        Assert.Equal(0, sidebar.FirstVisible);

        sidebar.Refresh(_party.ReserveMembers[^1]);

        Assert.Equal(sidebar.Cards.Count - PartySidebar.VisibleCards, sidebar.FirstVisible);
    }

    [Fact]
    public void Refresh_ShownMemberAlreadyVisible_KeepsTheWindow()
    {
        var sidebar = NewSidebar();
        sidebar.Refresh(_party.ReserveMembers[^1]);
        var scrolled = sidebar.FirstVisible;
        Assert.NotEqual(0, scrolled);

        sidebar.Refresh(_party.ReserveMembers[^2]);

        Assert.Equal(scrolled, sidebar.FirstVisible);
    }

    [Fact]
    public void Refresh_MemberMovedToReserve_ListsItAfterTheActive()
    {
        var leaving = _party.ReserveMembers[0];
        _party.TryAddToActive(leaving);
        var sidebar = NewSidebar();
        Assert.False(sidebar.Cards[1].IsReserve);

        _party.TryMoveToReserve(leaving);
        sidebar.Refresh(null);

        var card = sidebar.Cards.Single(card => ReferenceEquals(card.Member, leaving));
        Assert.True(card.IsReserve);
        Assert.Equal([.. _party.ActiveMembers, .. _party.ReserveMembers], sidebar.Cards.Select(card => card.Member));
    }

    [Fact]
    public void Refresh_NothingChanged_KeepsTheSameCards()
    {
        var sidebar = NewSidebar();
        var before = sidebar.Cards[0];

        sidebar.Refresh(null);

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
        sidebar.Refresh(null);

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
        sidebar.Refresh(null);

        Assert.Equal([_party.Leader, second, first], sidebar.Cards.Take(_party.ActiveMembers.Count).Select(card => card.Member));
    }

    private PartySidebar NewSidebar() => new(_party);
}
