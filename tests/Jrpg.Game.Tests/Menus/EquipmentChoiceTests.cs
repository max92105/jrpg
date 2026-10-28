using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Stats;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

public sealed class EquipmentChoiceTests
{
    private static readonly IReadOnlyDictionary<string, Core.Items.ItemDefinition> _items = StartMapScenarios.BelowClosedDoor.Items;

    private readonly PartyState _party = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor).Party;

    [Fact]
    public void Update_ConfirmFirstRow_PicksRemove()
    {
        var choice = new EquipmentChoice([GearPiece.Plain(_items["bronze_sword"])], []);

        var pick = choice.Update(Pressing(GameButton.Confirm));

        Assert.NotNull(pick);
        Assert.Null(pick.Piece);
    }

    [Fact]
    public void Update_ConfirmAfterDown_PicksTheFirstItem()
    {
        var choice = new EquipmentChoice([GearPiece.Plain(_items["bronze_sword"]), GearPiece.Plain(_items["iron_sword"])], []);
        choice.Update(Pressing(GameButton.Down));

        var pick = choice.Update(Pressing(GameButton.Confirm));

        Assert.Equal(GearPiece.Plain(_items["bronze_sword"]), pick?.Piece);
        Assert.Equal(GearPiece.Plain(_items["bronze_sword"]), choice.Highlighted);
    }

    [Fact]
    public void Rows_WornByOthers_ListedAfterTheOwnedOnes()
    {
        var wearer = _party.MemberById("zanyr");
        var choice = new EquipmentChoice([GearPiece.Plain(_items["bronze_sword"])], [new WornItem(wearer, GearPiece.Plain(_items["iron_sword"]))]);

        choice.Update(Pressing(GameButton.Down));
        choice.Update(Pressing(GameButton.Down));

        Assert.Equal(3, choice.RowCount);
        Assert.Equal(GearPiece.Plain(_items["iron_sword"]), choice.Highlighted);
        Assert.Same(wearer, choice.HighlightedWearer);
    }

    [Fact]
    public void Update_ConfirmWornRow_PicksItWithItsWearer()
    {
        var wearer = _party.MemberById("zanyr");
        var choice = new EquipmentChoice([], [new WornItem(wearer, GearPiece.Plain(_items["iron_sword"]))]);
        choice.Update(Pressing(GameButton.Down));

        var pick = choice.Update(Pressing(GameButton.Confirm));

        Assert.Equal(new EquipmentPick(GearPiece.Plain(_items["iron_sword"]), wearer), pick);
    }

    [Fact]
    public void Update_ConfirmOwnedRow_HasNoGiver()
    {
        var choice = new EquipmentChoice([GearPiece.Plain(_items["bronze_sword"])], [new WornItem(_party.MemberById("zanyr"), GearPiece.Plain(_items["iron_sword"]))]);
        choice.Update(Pressing(GameButton.Down));

        var pick = choice.Update(Pressing(GameButton.Confirm));

        Assert.Equal(new EquipmentPick(GearPiece.Plain(_items["bronze_sword"]), Giver: null), pick);
    }

    [Fact]
    public void Comparison_SwordIntoAnEmptySlot_ReplacesTheUnarmedDamage()
    {
        var sword = _items["bronze_sword"];
        var leader = _party.Leader;

        var comparison = StatComparison.Between(leader.Sheet, leader.SheetWith(GearPiece.Plain(sword)));

        Assert.Equal(WeaponDamage(leader, sword.Equipment!.Damage) - WeaponDamage(leader, Unarmed), WeaponDamageChange(comparison));
    }

    [Fact]
    public void Comparison_StrongerWeapon_RaisesWeaponDamage()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        var leader = session.Party.Leader;
        session.Equipment.TryEquip(leader, GearPiece.Plain(_items["bronze_sword"]));
        var ironSword = _items["iron_sword"];

        var comparison = StatComparison.Between(leader.Sheet, leader.SheetWith(GearPiece.Plain(ironSword)));

        Assert.Equal(WeaponDamage(leader, ironSword.Equipment!.Damage) - WeaponDamage(leader, _items["bronze_sword"].Equipment!.Damage), WeaponDamageChange(comparison));
        Assert.True(WeaponDamageChange(comparison) > 0);
    }

    [Fact]
    public void Comparison_RemovingTheWornWeapon_FallsBackToUnarmed()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        var leader = session.Party.Leader;
        session.Equipment.TryEquip(leader, GearPiece.Plain(_items["bronze_sword"]));
        var weapon = Core.Items.EquipmentSlot.Weapon;

        var comparison = StatComparison.Between(leader.Sheet, leader.SheetWithout(weapon));

        Assert.Equal(WeaponDamage(leader, Unarmed) - WeaponDamage(leader, _items["bronze_sword"].Equipment!.Damage), WeaponDamageChange(comparison));
    }

    private static int WeaponDamageChange(StatComparison comparison)
    {
        var weaponDamage = comparison.Changes.Single(change => change.NameId == MessageIds.StatWeaponDamage);
        return weaponDamage.After - weaponDamage.Before;
    }

    private static int Unarmed => StartMapScenarios.BelowClosedDoor.Rules.Stats.UnarmedDamage;

    // The raw damage as the leader deals it: none of these swords changes Power.
    private static int WeaponDamage(PartyMember member, int raw) => StatFormula.Scaled(raw, member.Sheet.DamagePermille);

    // Two pieces of one base are two rows; the highlight is the piece on its row, rolls included.
    [Fact]
    public void Highlighted_OwnedRow_IsThatPiece()
    {
        var sword = _items["bronze_sword"];
        var rolled = GearPiece.Rolled(sword, Rarity.Rare, [new RolledAffix("power", StatKind.Power, 4)]);
        var choice = new EquipmentChoice([GearPiece.Plain(sword), rolled], []);
        choice.Update(Pressing(GameButton.Down));

        choice.Update(Pressing(GameButton.Down));

        Assert.Equal(rolled, choice.Highlighted);
    }
}
