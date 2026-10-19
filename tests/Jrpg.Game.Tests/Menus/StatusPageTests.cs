using Jrpg.Core.Progression;
using Jrpg.Core.Randomness;
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

        Assert.Equal(["sarxian", "teelle", "jazog", "zaruk", "zanyr", "thaali", "kamilia"], page.Members.Select(member => member.Id));
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

    private static GameSession NewSession() => GameSession.StartNew(StartMapScenarios.BelowClosedDoor, new SeededRandom(1));

    private static StatusPage NewPage(GameSession session) =>
        new(RepositoryScreenText.Text, new UiTextures(), session, StartMapScenarios.BelowClosedDoor);
}
