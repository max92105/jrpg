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
    public void SlotsFor_Ring_IsBothRingSlots()
    {
        Assert.Equal([EquipmentSlot.FirstRing, EquipmentSlot.SecondRing], EquipmentFit.SlotsFor(EquipmentKind.Ring));
    }

    [Fact]
    public void Fits_WeaponInChest_IsFalse()
    {
        Assert.False(EquipmentFit.Fits(EquipmentKind.Weapon, EquipmentSlot.Chest));
    }

    [Fact]
    public void Fits_HelmInChest_IsFalse()
    {
        Assert.False(EquipmentFit.Fits(EquipmentKind.Head, EquipmentSlot.Chest));
    }
}
