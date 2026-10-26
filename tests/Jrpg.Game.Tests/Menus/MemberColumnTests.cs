using Jrpg.Core.Party;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class MemberColumnTests
{
    // More members than the column shows, each its own recruit, so the scroll tests don't hang on today's party size.
    private readonly PartyMember[] _members = Recruits(MemberColumn.VisibleCount + 4);

    [Fact]
    public void Refresh_StatusPage_ListsActiveThenReserve()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        session.Party.TryAddToActive(session.Party.MemberById("teelle"));
        var page = new StatusPage(RepositoryScreenText.Text, new UiTextures(), session, StartMapScenarios.BelowClosedDoor);
        var column = new MemberColumn();

        column.Refresh(page.Members, page.Shown);

        Assert.Equal([.. session.Party.ActiveMembers, .. session.Party.ReserveMembers], column.Members);
    }

    [Fact]
    public void Refresh_ShownMember_IsMarked()
    {
        var page = NewStatusPage();
        page.Update(Pressing(GameButton.Right));
        var column = new MemberColumn();

        column.Refresh(page.Members, page.Shown);

        Assert.Equal(1, column.ShownIndex);
        Assert.Same(page.Shown, column.Members[1]);
    }

    [Fact]
    public void Refresh_ShownBelowTheView_ScrollsToIt()
    {
        var column = new MemberColumn();
        column.Refresh(_members, _members[0]);

        column.Refresh(_members, _members[^1]);

        Assert.Equal(_members.Length - MemberColumn.VisibleCount, column.FirstVisible);
    }

    [Fact]
    public void Refresh_SameOrder_KeepsTheScroll()
    {
        var column = new MemberColumn();
        column.Refresh(_members, _members[^1]);
        var scrolled = column.FirstVisible;

        column.Refresh([.. _members], _members[^2]);

        Assert.Equal(scrolled, column.FirstVisible);
    }

    [Fact]
    public void Refresh_ShownAboveTheView_ScrollsBackToIt()
    {
        var column = new MemberColumn();
        column.Refresh(_members, _members[^1]);

        column.Refresh(_members, _members[0]);

        Assert.Equal(0, column.FirstVisible);
    }

    private static PartyMember[] Recruits(int count)
    {
        var catalog = StartMapScenarios.BelowClosedDoor;
        var definition = catalog.PartyMembers.Values.First();
        return [.. Enumerable.Range(0, count).Select(_ => PartyMember.Recruit(definition, catalog.ClassOf(definition), catalog.Skills))];
    }

    private static StatusPage NewStatusPage() =>
        new(RepositoryScreenText.Text, new UiTextures(), StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor), StartMapScenarios.BelowClosedDoor);
}
