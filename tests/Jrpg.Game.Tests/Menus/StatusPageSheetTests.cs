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
    public void Open_Leader_ShowsWeaponDamageRaisedByPower()
    {
        var page = NewPage();

        page.Open();

        var leader = _session.Party.Leader;
        var shown = StatusPageTests.CombatValue(page, MessageIds.StatWeaponDamage);
        Assert.Equal(NumberText.Of(StatFormula.Scaled(leader.TotalStats.Attack, leader.Sheet.DamagePermille)), shown);
        Assert.NotEqual(NumberText.Of(leader.TotalStats.Attack), shown);
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
    public void Open_Member_ShowsPowerBesideItsDamagePercent()
    {
        var page = NewPage();

        page.Open();

        var sheet = _session.Party.Leader.Sheet;
        var power = page.ShownSheet.Attributes[0];
        Assert.Equal(NumberText.Of(sheet.Primaries.Power), power.Value);
        Assert.StartsWith($"+{StatFormula.WholePercentOf(sheet.DamagePermille - 1000)} %", power.Caption, StringComparison.Ordinal);
    }

    [Fact]
    public void Open_LeaderWearingTheMagicStaff_ShowsItInIntelligenceAndItsCaption()
    {
        var page = NewPage();
        page.Open();
        var without = page.ShownSheet.Attributes[2];
        Assert.True(_session.Equipment.TryEquip(_session.Party.Leader, StartMapScenarios.BelowClosedDoor.Items["magic_staff"]));

        page.Open();

        var with = page.ShownSheet.Attributes[2];
        Assert.Equal(NumberText.Of(_session.Party.Leader.Sheet.Primaries.Intelligence), with.Value);
        Assert.NotEqual(without.Value, with.Value);
        Assert.NotEqual(without.Caption, with.Caption);
    }

    [Fact]
    public void Open_FireWeakMember_MarksFireAsWeakness()
    {
        WearTestArmor(StatKind.FireResistance, -100);
        var page = NewPage();

        page.Open();

        Assert.Equal(ResistanceTone.Weak, page.ShownSheet.ResistanceTones[(int)DamageType.Fire]);
        Assert.Equal(ResistanceTone.Plain, page.ShownSheet.ResistanceTones[(int)DamageType.Ice]);
    }

    [Fact]
    public void Open_SlashingAtTheCap_MarksItCapped()
    {
        WearTestArmor(StatKind.SlashingResistance, 100);
        var page = NewPage();

        page.Open();

        Assert.Equal(ResistanceTone.Capped, page.ShownSheet.ResistanceTones[(int)DamageType.Slashing]);
    }

    [Fact]
    public void ContentHeight_ProfileBandAndThreeSkills_FitsThePage()
    {
        Assert.True(
            StatusSheetView.ContentHeight <= MenuLayout.PageArea(TabFold.RailWidth).Height,
            $"The Status page needs {StatusSheetView.ContentHeight} px; the page area is {MenuLayout.PageArea(TabFold.RailWidth).Height}.");
    }

    private void WearTestArmor(StatKind kind, int bonus)
    {
        var mail = StartMapScenarios.BelowClosedDoor.Items["chain_mail"];
        var bonuses = new StatBonuses(new Dictionary<StatKind, int> { [kind] = bonus });
        var armor = mail with { Id = "test_armor", Equipment = mail.Equipment! with { Bonuses = bonuses } };
        _session.Inventory.Add(armor, 1);
        Assert.True(_session.Equipment.TryEquip(_session.Party.Leader, armor));
    }

    private StatusPage NewPage() => new(RepositoryScreenText.Text, new UiTextures(), _session, StartMapScenarios.BelowClosedDoor);
}
