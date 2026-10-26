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

        var text = StatusSheet.ToNextLevelText(2, 70, table, "Max");

        Assert.Equal("180", text);
    }

    [Fact]
    public void ToNextLevelText_AtHighestLevel_ShowsMaxText()
    {
        var table = new ExperienceTable([100, 250, 400]);

        var text = StatusSheet.ToNextLevelText(table.MaxLevel, 0, table, "Max");

        Assert.Equal("Max", text);
    }

    [Fact]
    public void ExperienceProgress_PartWayThroughLevelTwo_IsEarnedOverWhatTheLevelTakes()
    {
        var table = new ExperienceTable([100, 250, 400]);

        var progress = StatusSheet.ExperienceProgress(2, 70, table);

        Assert.Equal((70, 250), progress);
    }

    [Fact]
    public void ExperienceProgress_AtHighestLevel_IsFull()
    {
        var table = new ExperienceTable([100, 250, 400]);

        var progress = StatusSheet.ExperienceProgress(table.MaxLevel, 0, table);

        Assert.Equal((1, 1), progress);
    }

    [Fact]
    public void Open_Mage_ShowsManaAndClarity()
    {
        var page = NewPage(NewSession());

        ShowMember(page, "omer");

        var resources = page.Shown.Class.Resources;
        var mana = resources.Single(resource => resource.Id == "mana");
        var clarity = resources.Single(resource => resource.Id == "clarity");
        Assert.Contains(new StatusLine(mana.Name, NumberText.Of(mana.Max)), page.ShownResources);
        Assert.Contains(new StatusLine(clarity.Name, $"{clarity.StartsAt} / {clarity.Max}"), page.ShownResources);
    }

    [Fact]
    public void Open_LastingResourceSpent_ShowsItsAmountOutOfMax()
    {
        var session = SessionWithLastingAmount("omer", "clarity", 1);
        var page = NewPage(session);

        ShowMember(page, "omer");

        var clarity = page.Shown.Class.Resources.Single(resource => resource.Id == "clarity");
        Assert.Contains(new StatusLine(clarity.Name, $"1 / {clarity.Max}"), page.ShownResources);
    }

    [Fact]
    public void Open_Rogue_ShowsReadinessChargesOfSpeed()
    {
        var page = NewPage(NewSession());

        ShowMember(page, "lazarus");

        var readiness = page.Shown.Class.Resources.Single(resource => resource.Id == "readiness");
        var charges = Math.Max(1, page.Shown.Speed / readiness.MaxFromSpeed!.SpeedPerCharge);
        Assert.Contains(new StatusLine(readiness.Name, NumberText.Of(charges)), page.ShownResources);
    }

    [Fact]
    public void Open_Member_ListsTheClassSkillsInOrder()
    {
        var page = NewPage(NewSession());

        ShowMember(page, "omer");

        Assert.Equal(page.Shown.Skills.Select(skill => new StatusLine(skill.Name, skill.Description)), page.ShownSkills);
    }

    [Fact]
    public void Open_Member_ShowsSpeed()
    {
        var page = NewPage(NewSession());

        ShowMember(page, "omer");

        Assert.Equal(NumberText.Of(page.Shown.Speed), page.ShownSpeed);
    }

    // Moves right through the members until the one wanted is shown.
    private static void ShowMember(StatusPage page, string memberId)
    {
        for (var step = 0; step < page.Members.Count && page.Shown.Id != memberId; step++)
        {
            page.Update(Pressing(GameButton.Right));
        }
    }

    // The whole party, with one member's lasting resource left at an amount, as after a battle.
    private static GameSession SessionWithLastingAmount(string memberId, string resourceId, int amount)
    {
        var save = NewSession().Capture(DateTimeOffset.UnixEpoch);
        var members = save.Members.Select(member => member.Id == memberId
            ? member with { LastingAmounts = new Dictionary<string, int> { [resourceId] = amount } }
            : member);
        return GameSession.Restore(save with { Members = [.. members] }, StartMapScenarios.BelowClosedDoor, new SeededRandom(1));
    }

    private static GameSession NewSession() => StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);

    private static StatusPage NewPage(GameSession session) =>
        new(RepositoryScreenText.Text, new UiTextures(), session, StartMapScenarios.BelowClosedDoor);
}
