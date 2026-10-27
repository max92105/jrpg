using Jrpg.Core.Session;
using Jrpg.Core.Stats;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// The Changes section (decision 337): with a choice open, what the highlighted item would change; with none, what the
// highlighted slot's worn item gives. The rows are Weapon, Chest, then Remove all; the leader starts wearing nothing.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class EquipmentPageComparisonTests
{
    private readonly GameSession _session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);

    [Fact]
    public void Update_ChoiceHighlightsChainMail_ComparesItsResistances()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Down));
        page.Update(Pressing(GameButton.Confirm));

        while (page.Choice!.Highlighted?.Id != "chain_mail")
        {
            page.Update(Pressing(GameButton.Down));
        }

        var changes = page.Comparison!.Changes;
        Assert.Contains(changes, change => change.Kind == StatKind.Constitution && change.After > change.Before);
        Assert.Contains(changes, change => change.Kind == StatKind.SlashingResistance && change.After > change.Before);
    }

    [Fact]
    public void Update_SlotHighlighted_ShowsWhatTheWornItemGives()
    {
        WearChainMail();
        var page = NewPage();

        page.Update(Pressing(GameButton.Down));

        var constitution = Assert.Single(page.Comparison!.Changes, change => change.Kind == StatKind.Constitution);
        Assert.Equal(StartMapScenarios.BelowClosedDoor.Items["chain_mail"].Equipment!.Bonuses.Of(StatKind.Constitution), constitution.After - constitution.Before);
    }

    [Fact]
    public void Open_EmptyWeaponSlotHighlighted_ShowsNothing()
    {
        WearChainMail();

        var page = NewPage();

        Assert.Null(page.Comparison);
    }

    [Fact]
    public void Update_RemoveAllHighlighted_ShowsNothing()
    {
        WearChainMail();
        var page = NewPage();
        page.Update(Pressing(GameButton.Down));
        Assert.NotNull(page.Comparison);

        page.Update(Pressing(GameButton.Down));

        Assert.Null(page.Comparison);
    }

    [Fact]
    public void Update_ChoiceOnRemove_ShowsTheWornWeaponsAttackLost()
    {
        Assert.True(_session.Equipment.TryEquip(_session.Party.Leader, StartMapScenarios.BelowClosedDoor.Items["bronze_sword"]));
        var page = NewPage();

        page.Update(Pressing(GameButton.Confirm));

        Assert.Null(page.Choice!.Highlighted);
        var weaponDamage = Assert.Single(page.Comparison!.Changes, change => change.NameId == MessageIds.StatWeaponDamage);
        Assert.True(weaponDamage.After < weaponDamage.Before);
    }

    private void WearChainMail() =>
        Assert.True(_session.Equipment.TryEquip(_session.Party.Leader, StartMapScenarios.BelowClosedDoor.Items["chain_mail"]));

    private EquipmentPage NewPage() => new(RepositoryScreenText.Text, new UiTextures(), _session, StartMapScenarios.BelowClosedDoor);
}
