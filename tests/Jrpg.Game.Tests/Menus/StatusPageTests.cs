using Jrpg.Core.Progression;
using Jrpg.Core.Randomness;
using Jrpg.Core.Resources;
using Jrpg.Core.Session;
using Jrpg.Core.Stats;
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

        string[] expected =
        [
            $"{vitals.Hp} / {vitals.MaxHp}", NumberText.Of(StatFormula.Scaled(leader.TotalStats.Attack, leader.Sheet.DamagePermille)),
            NumberText.Of(leader.TotalStats.Defense),
        ];
        Assert.Equal(expected, page.ShownSheet.Combat.Take(3));
    }

    [Fact]
    public void Open_AfterHpChanged_ShowsTheNewHp()
    {
        var session = NewSession();
        var page = NewPage(session);
        var vitals = session.Party.Leader.Vitals;
        vitals.TakeDamage(10);

        page.Open();

        Assert.Equal($"{vitals.Hp} / {vitals.MaxHp}", page.ShownSheet.Combat[StatusSheet.HpLine]);
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
    public void Open_Member_ShowsSpeed()
    {
        var session = NewSession();
        var page = NewPage(session);

        page.Open();

        Assert.Equal(NumberText.Of(session.Party.Leader.Sheet.Speed), CombatValue(page, MessageIds.StatusSpeed));
    }

    [Fact]
    public void Open_Mage_ShowsManaAndClarity()
    {
        var page = NewPage(NewSession());

        ShowMemberOfClass(page, "mage");

        var resources = page.Shown.Class.Resources;
        var mana = resources.Single(resource => resource.Id == "mana");
        var clarity = resources.Single(resource => resource.Id == "clarity");
        StatusSheet.Line[] expected = [new(mana.Name.ToUpperInvariant(), NumberText.Of(mana.Max)), new(clarity.Name.ToUpperInvariant(), $"{clarity.StartsAt}/{clarity.Max}")];
        Assert.Equal(expected, page.ShownSheet.Resources);
    }

    [Fact]
    public void Open_LastingResourceSpent_ShowsItsAmountOutOfMax()
    {
        var mageId = NewSession().Party.Members.First(member => member.Class.Id == "mage").Id;
        var page = NewPage(SessionWithLastingAmount(mageId, "clarity", 1));

        ShowMemberOfClass(page, "mage");

        var clarity = page.Shown.Class.Resources.Single(resource => resource.Id == "clarity");
        Assert.Contains(new StatusSheet.Line(clarity.Name.ToUpperInvariant(), $"1/{clarity.Max}"), page.ShownSheet.Resources);
    }

    [Fact]
    public void Open_Rogue_ShowsReadinessMaxFromSpeed()
    {
        var page = NewPage(NewSession());

        ShowMemberOfClass(page, "rogue");

        var readiness = page.Shown.Class.Resources.Single(resource => resource.Id == "readiness");
        var charges = ReadinessFormula.Charges(page.Shown.Sheet.Speed, readiness.MaxFromSpeed!.SpeedPerCharge);
        Assert.Equal(new StatusSheet.Line(readiness.Name.ToUpperInvariant(), NumberText.Of(charges)), page.ShownSheet.Resources[0]);
    }

    [Fact]
    public void Open_Member_ListsTheClassSkillsInOrder()
    {
        var session = NewSession();
        var page = NewPage(session);

        page.Open();

        var expected = session.Party.Leader.Skills.Select(skill => new StatusSheet.Line(skill.Name, skill.Description));
        Assert.Equal(expected, page.ShownSheet.Skills);
    }

    [Fact]
    public void Open_FirstMember_LabelsAreInCaps()
    {
        var page = NewPage(NewSession());

        page.Open();

        var messages = RepositoryScreenText.Text.Messages;
        Assert.Equal(messages.Text(MessageIds.StatWeaponDamage).ToUpperInvariant(), page.ShownLabels.Combat[1]);
        Assert.Contains(messages.Text(MessageIds.StatusSpeed).ToUpperInvariant(), page.ShownLabels.Combat);
        Assert.Equal(messages.Text(MessageIds.StatusLevelShort).ToUpperInvariant(), page.ShownLabels.Progress[0]);
        Assert.Equal(messages.Text(MessageIds.StatusNextLevel).ToUpperInvariant(), page.ShownLabels.Progress[2]);
    }

    // A combat line's value, found by its label rather than its place.
    internal static string CombatValue(StatusPage page, string labelId)
    {
        var label = RepositoryScreenText.Text.Messages.Text(labelId).ToUpperInvariant();
        return page.ShownSheet.Combat[Array.IndexOf(page.ShownLabels.Combat, label)];
    }

    // Goes through the members with Right until one of the class shows.
    private static void ShowMemberOfClass(StatusPage page, string classId)
    {
        page.Open();
        while (page.Shown.Class.Id != classId)
        {
            page.Update(Pressing(GameButton.Right));
        }
    }

    // The save's way in: Core keeps lasting amounts only through a battle or a save.
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
