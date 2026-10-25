using Jrpg.Core.Progression;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class StatusPageTests
{
    [Fact]
    public void Members_SecondMemberMadeActive_ListedBeforeTheReserve()
    {
        var session = NewSession();
        session.Party.TryAddToActive(session.Party.MemberById("teelle"));

        var page = NewPage(session);

        Assert.Equal(["sarxian", "teelle", "jazog", "zaruk", "zanyr", "thaali", "ragnir", "omer", "lazarus"], page.Members.Select(member => member.Id));
    }

    [Fact]
    public void Update_LeftOnFirstMember_WrapsToLast()
    {
        var page = NewPage(NewSession());

        page.Update(Pressing(GameButton.Left));

        Assert.Same(page.Members[^1], page.Shown);
    }

    [Fact]
    public void Update_NextTabOnLastMember_WrapsToFirst()
    {
        var page = NewPage(NewSession());
        page.Update(Pressing(GameButton.Left));

        page.Update(Pressing(GameButton.NextTab));

        Assert.Same(page.Members[0], page.Shown);
    }

    [Fact]
    public void ShownStats_Member_HasNoMp()
    {
        var session = NewSession();
        var page = NewPage(session);
        var leader = session.Party.Leader;
        var vitals = leader.Vitals;

        page.Open();

        string[] expected = [$"{vitals.Hp} / {vitals.MaxHp}", NumberText.Of(leader.TotalStats.Attack), NumberText.Of(leader.TotalStats.Defense)];
        Assert.Equal(expected, page.ShownStats);
    }

    [Fact]
    public void Open_AfterHpChanged_ShowsTheNewHp()
    {
        var session = NewSession();
        var page = NewPage(session);
        var vitals = session.Party.Leader.Vitals;
        vitals.TakeDamage(10);

        page.Open();

        Assert.Equal($"{vitals.Hp} / {vitals.MaxHp}", page.ShownStats[0]);
    }

    [Fact]
    public void Open_AfterMemberJoinedElsewhere_ListsThemAmongTheActive()
    {
        var session = NewSession();
        var page = NewPage(session);
        session.Party.TryAddToActive(session.Party.MemberById("teelle"));

        page.Open();

        Assert.Equal(["sarxian", "teelle", "jazog", "zaruk", "zanyr", "thaali", "ragnir", "omer", "lazarus"], page.Members.Select(member => member.Id));
    }

    [Fact]
    public void Open_AfterMovingToAnotherMember_ShowsTheFirst()
    {
        var page = NewPage(NewSession());
        page.Update(Pressing(GameButton.Right));

        page.Open();

        Assert.Same(page.Members[0], page.Shown);
    }

    [Fact]
    public void ToNextLevelText_PartWayThroughLevelTwo_ShowsWhatRemains()
    {
        var table = new ExperienceTable([100, 250, 400]);

        var text = StatusPage.ToNextLevelText(2, 70, table, "Max");

        Assert.Equal("180", text);
    }

    [Fact]
    public void ToNextLevelText_AtHighestLevel_ShowsMaxText()
    {
        var table = new ExperienceTable([100, 250, 400]);

        var text = StatusPage.ToNextLevelText(table.MaxLevel, 0, table, "Max");

        Assert.Equal("Max", text);
    }

    [Fact]
    public void ExperienceProgress_PartWayThroughLevelTwo_IsEarnedOverWhatTheLevelTakes()
    {
        var table = new ExperienceTable([100, 250, 400]);

        var progress = StatusPage.ExperienceProgress(2, 70, table);

        Assert.Equal((70, 250), progress);
    }

    [Fact]
    public void ExperienceProgress_AtHighestLevel_IsFull()
    {
        var table = new ExperienceTable([100, 250, 400]);

        var progress = StatusPage.ExperienceProgress(table.MaxLevel, 0, table);

        Assert.Equal((1, 1), progress);
    }

    private static GameSession NewSession() => StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);

    private static StatusPage NewPage(GameSession session) =>
        new(RepositoryScreenText.Text, new UiTextures(), session, StartMapScenarios.BelowClosedDoor);
}
