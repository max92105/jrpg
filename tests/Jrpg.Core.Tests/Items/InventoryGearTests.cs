using Jrpg.Core.Items;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Items;

// Owned gear is a list of pieces beside the stacks (ADR 0029): one row per piece, no cap (decision 345).
public sealed class InventoryGearTests
{
    private readonly Inventory _inventory = new();
    private readonly ItemDefinition _sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(24).WithMaxStack(1).Build();

    [Fact]
    public void Add_Equipment_AddsOnePlainPiecePerCopy()
    {
        var lost = _inventory.Add(_sword, 3);

        Assert.Equal([GearPiece.Plain(_sword), GearPiece.Plain(_sword), GearPiece.Plain(_sword)], _inventory.Gear);
        Assert.Equal(0, lost);
        Assert.Empty(_inventory.Entries);
    }

    // An upgraded piece keeps its row (forge plan, M3a): the shield after it stays after it.
    [Fact]
    public void TryReplace_OwnedPiece_KeepsItsPlace()
    {
        var shield = GearPiece.Plain(new ItemDefinitionBuilder().WithId("shield").AsWorn(EquipmentKind.OffHand).Build());
        _inventory.AddGear(GearPiece.Plain(_sword));
        _inventory.AddGear(shield);
        var magic = GearPiece.Rolled(_sword, Rarity.Magic, []);

        var replaced = _inventory.TryReplace(GearPiece.Plain(_sword), magic);

        Assert.True(replaced);
        Assert.Equal([magic, shield], _inventory.Gear);
    }

    [Fact]
    public void TryReplace_NotOwned_ReturnsFalseAndChangesNothing()
    {
        _inventory.AddGear(GearPiece.Plain(_sword));

        var replaced = _inventory.TryReplace(GearPiece.Rolled(_sword, Rarity.Rare, []), GearPiece.Rolled(_sword, Rarity.Magic, []));

        Assert.False(replaced);
        Assert.Equal([GearPiece.Plain(_sword)], _inventory.Gear);
    }

    [Fact]
    public void TryRemoveGear_EqualPiece_RemovesOne()
    {
        _inventory.Add(_sword, 2);

        var removed = _inventory.TryRemoveGear(GearPiece.Plain(_sword));

        Assert.True(removed);
        Assert.Equal(1, _inventory.GearCountOf(_sword.Id));
    }

    [Fact]
    public void TryRemoveGear_NoEqualPiece_ReturnsFalse()
    {
        var removed = _inventory.TryRemoveGear(GearPiece.Plain(_sword));

        Assert.False(removed);
    }

    [Fact]
    public void TryRemoveGear_RolledPiece_LeavesThePlainOne()
    {
        var rolled = GearPiece.Rolled(_sword, Rarity.Magic, [new RolledAffix("power", StatKind.Power, 3)]);
        _inventory.Add(_sword, 1);
        _inventory.AddGear(rolled);

        Assert.True(_inventory.TryRemoveGear(rolled));

        Assert.Equal([GearPiece.Plain(_sword)], _inventory.Gear);
    }

    [Fact]
    public void GearCountOf_TwoSwordPieces_IsTwo()
    {
        _inventory.Add(_sword, 1);
        _inventory.AddGear(GearPiece.Rolled(_sword, Rarity.Rare, []));

        var count = _inventory.GearCountOf(_sword.Id);

        Assert.Equal(2, count);
    }

    [Fact]
    public void QuantityOf_EquipmentBase_IsZero()
    {
        _inventory.Add(_sword, 2);

        var quantity = _inventory.QuantityOf(_sword.Id);

        Assert.Equal(0, quantity);
    }
}
