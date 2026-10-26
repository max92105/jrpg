using Jrpg.Core.Party;
using Jrpg.Core.Session;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Menus;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class MemberColumnTests
{
    [Fact]
    public void Refresh_StatusPage_ListsActiveThenReserve()
    {
        var session = NewSession();
        session.Party.TryAddToActive(session.Party.MemberById("teelle"));
        var page = new StatusPage(RepositoryScreenText.Text, new UiTextures(), session, StartMapScenarios.BelowClosedDoor);
        var column = new MemberColumn();

        column.Refresh(page.Members, page.Shown);

        var party = session.Party;
        Assert.Equal([.. party.ActiveMembers, .. party.ReserveMembers], column.Members);
    }

    [Fact]
    public void Refresh_ShownMember_IsMarked()
    {
        var members = AllMembers();
        var column = new MemberColumn();

        column.Refresh(members, members[3]);

        Assert.Equal(3, column.ShownIndex);
    }

    [Fact]
    public void Refresh_ShownBelowTheView_ScrollsToIt()
    {
        var members = AllMembers();
        Assert.True(members.Count > MemberColumn.VisibleCount);
        var column = new MemberColumn();

        column.Refresh(members, members[^1]);

        Assert.Equal(members.Count - MemberColumn.VisibleCount, column.FirstVisible);
    }

    [Fact]
    public void Refresh_SameOrder_KeepsTheScroll()
    {
        var members = AllMembers();
        var column = new MemberColumn();
        column.Refresh(members, members[^1]);
        var scrolled = column.FirstVisible;
        var kept = column.Members;

        column.Refresh([.. members], members[scrolled]);

        Assert.Equal(scrolled, column.FirstVisible);
        Assert.Same(kept, column.Members);
    }

    [Fact]
    public void Refresh_MemberNotListed_Throws()
    {
        var members = AllMembers();
        var column = new MemberColumn();

        Assert.Throws<ArgumentException>(() => column.Refresh(members.Skip(1).ToArray(), members[0]));
    }

    private static GameSession NewSession() => StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);

    private static IReadOnlyList<PartyMember> AllMembers() => MemberOrder.ActiveThenReserve(NewSession().Party);
}
