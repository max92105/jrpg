using Jrpg.Core.Session;
using Jrpg.Core.Stats;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Menus;

// The Status page's Combat and Resistances (the plan's S1a): values as the sheet gives them, gear included.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class StatusPageSheetTests
{
    private static readonly UiMessages _messages = RepositoryScreenText.Text.Messages;

    private readonly GameSession _session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);

    [Fact]
    public void Open_Member_ShowsEightResistancesInTypeOrder()
    {
        var page = NewPage();

        page.Open();

        string[] expected = [.. Enum.GetValues<DamageType>().Select(type => _messages.Text(DamageTypeNames.IdOf(type)).ToUpperInvariant())];
        Assert.Equal(expected, page.ShownLabels.Resistances);
        Assert.Equal(expected.Length, page.ShownSheet.Resistances.Length);
    }

    [Fact]
    public void Open_Member_ShowsCritChanceAndDamageAsPercents()
    {
        var page = NewPage();

        page.Open();

        var sheet = _session.Party.Leader.Sheet;
        var chance = StatText.ValueOf(StatKind.CritChance, sheet.CritChancePermille, _messages);
        var damage = StatText.ValueOf(StatKind.CritDamage, sheet.CritDamagePermille, _messages);
        var expected = _messages.Format(MessageIds.StatusCritValue, new MessageArgument("chance", chance), new MessageArgument("damage", damage));
        Assert.Equal(expected, StatusPageTests.CombatValue(page, MessageIds.StatusCrit));
        Assert.EndsWith("%", chance, StringComparison.Ordinal);
    }

    [Fact]
    public void Open_LeaderWearingChainMail_ShowsItsSlashingResistance()
    {
        var page = NewPage();
        page.Open();
        var without = page.ShownSheet.Resistances[(int)DamageType.Slashing];
        var leader = _session.Party.Leader;
        Assert.True(_session.Equipment.TryEquip(leader, StartMapScenarios.BelowClosedDoor.Items["chain_mail"]));

        page.Open();

        var slashing = page.ShownSheet.Resistances[(int)DamageType.Slashing];
        Assert.Equal(StatText.ValueOf(StatKind.SlashingResistance, leader.Sheet.ValueOf(StatKind.SlashingResistance), _messages), slashing);
        Assert.NotEqual(without, slashing);
    }

    [Fact]
    public void ContentHeight_ProfileBandAndThreeSkills_FitsThePage()
    {
        Assert.True(
            StatusSheetView.ContentHeight <= MenuLayout.PageArea(TabFold.RailWidth).Height,
            $"The Status page needs {StatusSheetView.ContentHeight} px; the page area is {MenuLayout.PageArea(TabFold.RailWidth).Height}.");
    }

    private StatusPage NewPage() => new(RepositoryScreenText.Text, new UiTextures(), _session, StartMapScenarios.BelowClosedDoor);
}
