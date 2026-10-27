using Jrpg.Core.Items;

namespace Jrpg.Core.Tests.Items;

public sealed class EquipmentFitTests
{
    [Fact]
    public void SlotsFor_Chest_IsChest()
    {
        Assert.Equal([EquipmentSlot.Chest], EquipmentFit.SlotsFor(EquipmentKind.Chest));
    }

    [Fact]
    public void Fits_WeaponInChest_IsFalse()
    {
        Assert.False(EquipmentFit.Fits(EquipmentKind.Weapon, EquipmentSlot.Chest));
    }

    // The member has no ring slot until G1a-2, so a ring fits nowhere and can't be equipped.
    [Fact]
    public void SlotsFor_RingBeforeTheRingSlots_IsEmpty()
    {
        Assert.Empty(EquipmentFit.SlotsFor(EquipmentKind.Ring));
    }
}
