using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Party;

public sealed class PartyEquipmentTests
{
    private readonly Inventory _inventory = new();
    private readonly PartyEquipment _equipment;
    private readonly PartyMember _fighter = new PartyMemberBuilder().WithId("sarxian").WithClass("fighter").Build();
    private readonly ItemDefinition _bronzeSword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(5).Build();
    private readonly ItemDefinition _ironSword = new ItemDefinitionBuilder().WithId("iron_sword").AsWeapon(8).Build();

    public PartyEquipmentTests()
    {
        _equipment = new PartyEquipment(_inventory);
    }

    [Fact]
    public void TryEquip_Success_RemovesCopyFromInventory()
    {
        _inventory.Add(_bronzeSword, 2);

        var equipped = _equipment.TryEquip(_fighter, GearPiece.Plain(_bronzeSword));

        Assert.True(equipped);
        Assert.Same(_bronzeSword, _fighter.Weapon?.Base);
        Assert.Equal(1, _inventory.GearCountOf(_bronzeSword.Id));
    }

    [Fact]
    public void TryEquip_AnyClass_WearsAnyItem()
    {
        var staff = new ItemDefinitionBuilder().WithId("magic_staff").AsWeapon(6).Build();
        _inventory.Add(staff, 1);

        var equipped = _equipment.TryEquip(_fighter, GearPiece.Plain(staff));

        Assert.True(equipped);
        Assert.Same(staff, _fighter.Weapon?.Base);
        Assert.Equal(0, _inventory.GearCountOf(staff.Id));
    }

    [Fact]
    public void TryEquip_NoCopyInInventory_ReturnsFalse()
    {
        var equipped = _equipment.TryEquip(_fighter, GearPiece.Plain(_bronzeSword));

        Assert.False(equipped);
        Assert.Null(_fighter.Weapon);
    }

    [Fact]
    public void TryEquip_SlotOccupied_ReturnsPreviousItemToInventory()
    {
        _inventory.Add(_bronzeSword, 1);
        _inventory.Add(_ironSword, 1);
        _equipment.TryEquip(_fighter, GearPiece.Plain(_bronzeSword));

        var equipped = _equipment.TryEquip(_fighter, GearPiece.Plain(_ironSword));

        Assert.True(equipped);
        Assert.Same(_ironSword, _fighter.Weapon?.Base);
        Assert.Equal(1, _inventory.GearCountOf(_bronzeSword.Id));
        Assert.Equal(0, _inventory.GearCountOf(_ironSword.Id));
    }

    [Fact]
    public void TryEquip_SameItemAlreadyWorn_ReturnsTrueAndChangesNothing()
    {
        var potion = new ItemDefinitionBuilder().Build();
        _inventory.Add(_bronzeSword, 2);
        _inventory.Add(potion, 3);
        _equipment.TryEquip(_fighter, GearPiece.Plain(_bronzeSword));

        var equipped = _equipment.TryEquip(_fighter, GearPiece.Plain(_bronzeSword));

        Assert.True(equipped);
        Assert.Same(_bronzeSword, _fighter.Weapon?.Base);
        Assert.Equal([GearPiece.Plain(_bronzeSword)], _inventory.Gear);
        Assert.Equal([new InventoryEntry("potion", 3)], _inventory.Entries);
    }

    [Fact]
    public void TryEquip_AfterUnequipFromOtherWearer_Succeeds()
    {
        var knight = new PartyMemberBuilder().WithId("jazog").WithClass("fighter").Build();
        _inventory.Add(_bronzeSword, 1);
        _equipment.TryEquip(knight, GearPiece.Plain(_bronzeSword));
        _equipment.TryUnequip(knight, EquipmentSlot.Weapon);

        var equipped = _equipment.TryEquip(_fighter, GearPiece.Plain(_bronzeSword));

        Assert.True(equipped);
        Assert.Null(knight.Weapon);
        Assert.Same(_bronzeSword, _fighter.Weapon?.Base);
    }

    [Fact]
    public void TryUnequip_WornItem_ReturnsItToInventory()
    {
        _inventory.Add(_bronzeSword, 1);
        _equipment.TryEquip(_fighter, GearPiece.Plain(_bronzeSword));

        var unequipped = _equipment.TryUnequip(_fighter, EquipmentSlot.Weapon);

        Assert.True(unequipped);
        Assert.Null(_fighter.Weapon);
        Assert.Equal(1, _inventory.GearCountOf(_bronzeSword.Id));
    }

    [Fact]
    public void TryUnequip_WornArmor_ReturnsItToInventory()
    {
        var chainMail = new ItemDefinitionBuilder().WithId("chain_mail").AsChest().Build();
        _inventory.Add(chainMail, 1);
        _equipment.TryEquip(_fighter, GearPiece.Plain(chainMail));

        var unequipped = _equipment.TryUnequip(_fighter, EquipmentSlot.Chest);

        Assert.True(unequipped);
        Assert.Null(_fighter.EquippedIn(EquipmentSlot.Chest));
        Assert.Equal(1, _inventory.GearCountOf(chainMail.Id));
    }

    [Fact]
    public void TryUnequip_EmptySlot_ReturnsFalse()
    {
        var unequipped = _equipment.TryUnequip(_fighter, EquipmentSlot.Chest);

        Assert.False(unequipped);
    }

    // Gear has no stack to fill (decision 345): the piece goes back however many are owned.
    [Fact]
    public void TryUnequip_ManyPiecesOwned_PutsPieceBack()
    {
        _inventory.Add(_bronzeSword, 100);
        Assert.True(_equipment.TryEquip(_fighter, GearPiece.Plain(_bronzeSword)));

        var unequipped = _equipment.TryUnequip(_fighter, EquipmentSlot.Weapon);

        Assert.True(unequipped);
        Assert.Equal(100, _inventory.GearCountOf(_bronzeSword.Id));
    }

    [Fact]
    public void OwnedFor_WeaponSlot_ListsEveryOwnedWeaponWhateverTheClass()
    {
        var staff = new ItemDefinitionBuilder().WithId("magic_staff").AsWeapon(6).Build();
        _inventory.Add(_ironSword, 1);
        _inventory.Add(staff, 1);
        _inventory.Add(_bronzeSword, 1);

        var wearable = _equipment.OwnedFor(EquipmentSlot.Weapon);

        Assert.Equal([GearPiece.Plain(_ironSword), GearPiece.Plain(staff), GearPiece.Plain(_bronzeSword)], wearable);
    }

    [Fact]
    public void OwnedFor_ArmorSlot_ExcludesWeapons()
    {
        var chainMail = new ItemDefinitionBuilder().WithId("chain_mail").AsChest().Build();
        _inventory.Add(_bronzeSword, 1);
        _inventory.Add(chainMail, 1);

        var wearable = _equipment.OwnedFor(EquipmentSlot.Chest);

        Assert.Equal([GearPiece.Plain(chainMail)], wearable);
    }

    // The piece keeps its rolls on the member, and the plain sword beside it stays owned.
    [Fact]
    public void TryEquip_RolledPieceOwned_WearsThatPiece()
    {
        var rolled = GearPiece.Rolled(_bronzeSword, Rarity.Magic, [new RolledAffix("power", StatKind.Power, 3)]);
        _inventory.Add(_bronzeSword, 1);
        _inventory.AddGear(rolled);

        var equipped = _equipment.TryEquip(_fighter, rolled);

        Assert.True(equipped);
        Assert.Equal(rolled, _fighter.Weapon);
        Assert.Equal([GearPiece.Plain(_bronzeSword)], _inventory.Gear);
    }
}
