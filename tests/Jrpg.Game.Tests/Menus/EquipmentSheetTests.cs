using Jrpg.Core.Items;
using Jrpg.Core.Session;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;

namespace Jrpg.Game.Tests.Menus;

/// <summary>
/// On the repository's new game: the leader, a fighter, starts wearing nothing, with bronze swords in the bag.
/// </summary>
public sealed class EquipmentSheetTests
{
    private readonly GameSession _session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);

    [Fact]
    public void Of_WearingBronzeSword_PutsItInTheWeaponSlot()
    {
        var sword = WearBronzeSword();

        var sheet = EquipmentSheet.Of(_session.Party.Leader);

        Assert.Same(sword, sheet.Worn[EquipmentSheet.Slots.IndexOf(EquipmentSlot.Weapon)]?.Base);
    }

    [Fact]
    public void Of_Member_ListsNineSlotsInOrder()
    {
        var sheet = EquipmentSheet.Of(_session.Party.Leader);

        EquipmentSlot[] expected =
        [
            EquipmentSlot.Weapon, EquipmentSlot.OffHand, EquipmentSlot.Head, EquipmentSlot.Chest, EquipmentSlot.Hands,
            EquipmentSlot.Feet, EquipmentSlot.Amulet, EquipmentSlot.FirstRing, EquipmentSlot.SecondRing,
        ];
        Assert.Equal(expected, EquipmentSheet.Slots);
        Assert.Equal(expected.Length, sheet.Worn.Length);
    }

    [Fact]
    public void Of_NothingWorn_EverySlotIsEmpty()
    {
        var sheet = EquipmentSheet.Of(_session.Party.Leader);

        Assert.All(sheet.Worn, Assert.Null);
    }

    private ItemDefinition WearBronzeSword()
    {
        var sword = StartMapScenarios.BelowClosedDoor.Items["bronze_sword"];
        Assert.True(_session.Equipment.TryEquip(_session.Party.Leader, sword));
        return sword;
    }
}
