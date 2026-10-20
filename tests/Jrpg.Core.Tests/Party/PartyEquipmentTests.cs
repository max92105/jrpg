using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Party;

public sealed class PartyEquipmentTests
{
    private readonly Inventory _inventory = new();
    private readonly PartyEquipment _equipment;
    private readonly PartyMember _warrior = new PartyMemberBuilder().WithId("sarxian").WithClass("warrior").Build();
    private readonly ItemDefinition _bronzeSword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(5, "warrior").Build();
    private readonly ItemDefinition _ironSword = new ItemDefinitionBuilder().WithId("iron_sword").AsWeapon(8, "warrior").Build();

    public PartyEquipmentTests()
    {
        _equipment = new PartyEquipment(_inventory);
    }

    [Fact]
    public void TryEquip_Success_RemovesCopyFromInventory()
    {
        _inventory.Add(_bronzeSword, 2);

        var equipped = _equipment.TryEquip(_warrior, _bronzeSword);

        Assert.True(equipped);
        Assert.Same(_bronzeSword, _warrior.Weapon);
        Assert.Equal(1, _inventory.QuantityOf(_bronzeSword.Id));
    }

    [Fact]
    public void TryEquip_NotEquipment_ReturnsFalse()
    {
        var potion = new ItemDefinitionBuilder().Build();
        _inventory.Add(potion, 1);

        var equipped = _equipment.TryEquip(_warrior, potion);

        Assert.False(equipped);
        Assert.Equal(1, _inventory.QuantityOf(potion.Id));
    }

    [Fact]
    public void TryEquip_ClassNotAllowed_ReturnsFalse()
    {
        var staff = new ItemDefinitionBuilder().WithId("magic_staff").AsWeapon(6, "mage").Build();
        _inventory.Add(staff, 1);

        var equipped = _equipment.TryEquip(_warrior, staff);

        Assert.False(equipped);
        Assert.Null(_warrior.Weapon);
        Assert.Equal(1, _inventory.QuantityOf(staff.Id));
    }

    [Fact]
    public void TryEquip_NoCopyInInventory_ReturnsFalse()
    {
        var equipped = _equipment.TryEquip(_warrior, _bronzeSword);

        Assert.False(equipped);
        Assert.Null(_warrior.Weapon);
    }

    [Fact]
    public void TryEquip_SlotOccupied_ReturnsPreviousItemToInventory()
    {
        _inventory.Add(_bronzeSword, 1);
        _inventory.Add(_ironSword, 1);
        _equipment.TryEquip(_warrior, _bronzeSword);

        var equipped = _equipment.TryEquip(_warrior, _ironSword);

        Assert.True(equipped);
        Assert.Same(_ironSword, _warrior.Weapon);
        Assert.Equal(1, _inventory.QuantityOf(_bronzeSword.Id));
        Assert.Equal(0, _inventory.QuantityOf(_ironSword.Id));
    }

    [Fact]
    public void TryEquip_SameItemAlreadyWorn_ReturnsTrueAndChangesNothing()
    {
        var potion = new ItemDefinitionBuilder().Build();
        _inventory.Add(_bronzeSword, 2);
        _inventory.Add(potion, 3);
        _equipment.TryEquip(_warrior, _bronzeSword);

        var equipped = _equipment.TryEquip(_warrior, _bronzeSword);

        Assert.True(equipped);
        Assert.Same(_bronzeSword, _warrior.Weapon);
        Assert.Equal([new InventoryEntry("bronze_sword", 1), new InventoryEntry("potion", 3)], _inventory.Entries);
    }

    [Fact]
    public void TryEquip_ReplacedItemStackFull_ReturnsFalseAndChangesNothing()
    {
        var bronzeSword = new ItemDefinitionBuilder().WithId("bronze_sword").WithMaxStack(1).AsWeapon(5, "warrior").Build();
        _inventory.Add(bronzeSword, 1);
        _equipment.TryEquip(_warrior, bronzeSword);
        _inventory.Add(bronzeSword, 1);
        _inventory.Add(_ironSword, 1);

        var equipped = _equipment.TryEquip(_warrior, _ironSword);

        Assert.False(equipped);
        Assert.Same(bronzeSword, _warrior.Weapon);
        Assert.Equal(1, _inventory.QuantityOf(_ironSword.Id));
    }

    [Fact]
    public void TryEquip_AfterUnequipFromOtherWearer_Succeeds()
    {
        var knight = new PartyMemberBuilder().WithId("jazog").WithClass("warrior").Build();
        _inventory.Add(_bronzeSword, 1);
        _equipment.TryEquip(knight, _bronzeSword);
        _equipment.TryUnequip(knight, EquipmentSlot.Weapon);

        var equipped = _equipment.TryEquip(_warrior, _bronzeSword);

        Assert.True(equipped);
        Assert.Null(knight.Weapon);
        Assert.Same(_bronzeSword, _warrior.Weapon);
    }

    [Fact]
    public void TryUnequip_WornItem_ReturnsItToInventory()
    {
        _inventory.Add(_bronzeSword, 1);
        _equipment.TryEquip(_warrior, _bronzeSword);

        var unequipped = _equipment.TryUnequip(_warrior, EquipmentSlot.Weapon);

        Assert.True(unequipped);
        Assert.Null(_warrior.Weapon);
        Assert.Equal(1, _inventory.QuantityOf(_bronzeSword.Id));
    }

    [Fact]
    public void TryUnequip_WornArmor_ReturnsItToInventory()
    {
        var chainMail = new ItemDefinitionBuilder().WithId("chain_mail").AsArmor(6, "warrior").Build();
        _inventory.Add(chainMail, 1);
        _equipment.TryEquip(_warrior, chainMail);

        var unequipped = _equipment.TryUnequip(_warrior, EquipmentSlot.Armor);

        Assert.True(unequipped);
        Assert.Null(_warrior.Armor);
        Assert.Equal(1, _inventory.QuantityOf(chainMail.Id));
    }

    [Fact]
    public void TryUnequip_EmptySlot_ReturnsFalse()
    {
        var unequipped = _equipment.TryUnequip(_warrior, EquipmentSlot.Armor);

        Assert.False(unequipped);
    }

    [Fact]
    public void TryUnequip_InventoryStackFull_ReturnsFalseAndKeepsItem()
    {
        var bronzeSword = new ItemDefinitionBuilder().WithId("bronze_sword").WithMaxStack(1).AsWeapon(5, "warrior").Build();
        _inventory.Add(bronzeSword, 1);
        _equipment.TryEquip(_warrior, bronzeSword);
        _inventory.Add(bronzeSword, 1);

        var unequipped = _equipment.TryUnequip(_warrior, EquipmentSlot.Weapon);

        Assert.False(unequipped);
        Assert.Same(bronzeSword, _warrior.Weapon);
        Assert.Equal(1, _inventory.QuantityOf(bronzeSword.Id));
    }

    [Fact]
    public void WearableFor_OtherClassItem_IsExcluded()
    {
        var staff = new ItemDefinitionBuilder().WithId("magic_staff").AsWeapon(6, "mage").Build();
        _inventory.Add(_ironSword, 1);
        _inventory.Add(staff, 1);
        _inventory.Add(_bronzeSword, 1);

        var wearable = _equipment.WearableFor(_warrior, EquipmentSlot.Weapon, Catalog(_ironSword, staff, _bronzeSword));

        Assert.Equal([_ironSword, _bronzeSword], wearable);
    }

    [Fact]
    public void WearableFor_ArmorSlot_ExcludesWeapons()
    {
        var chainMail = new ItemDefinitionBuilder().WithId("chain_mail").AsArmor(6, "warrior").Build();
        _inventory.Add(_bronzeSword, 1);
        _inventory.Add(chainMail, 1);

        var wearable = _equipment.WearableFor(_warrior, EquipmentSlot.Armor, Catalog(_bronzeSword, chainMail));

        Assert.Equal([chainMail], wearable);
    }

    private static Dictionary<string, ItemDefinition> Catalog(params ItemDefinition[] items) => items.ToDictionary(item => item.Id);
}
