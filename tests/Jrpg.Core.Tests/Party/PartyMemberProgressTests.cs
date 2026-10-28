using Jrpg.Core.Items;
using Jrpg.Core.Party;

namespace Jrpg.Core.Tests.Party;

public sealed class PartyMemberProgressTests
{
    [Fact]
    public void Equals_SameWornInOtherOrder_IsTrue()
    {
        var first = Progress(new Dictionary<EquipmentSlot, SavedGear> { [EquipmentSlot.Weapon] = new SavedGear("bronze_sword", Rarity.Common, []), [EquipmentSlot.FirstRing] = new SavedGear("power_ring", Rarity.Common, []) });
        var second = Progress(new Dictionary<EquipmentSlot, SavedGear> { [EquipmentSlot.FirstRing] = new SavedGear("power_ring", Rarity.Common, []), [EquipmentSlot.Weapon] = new SavedGear("bronze_sword", Rarity.Common, []) });

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Equals_RingInTheOtherSlot_IsFalse()
    {
        var first = Progress(new Dictionary<EquipmentSlot, SavedGear> { [EquipmentSlot.FirstRing] = new SavedGear("power_ring", Rarity.Common, []) });
        var second = Progress(new Dictionary<EquipmentSlot, SavedGear> { [EquipmentSlot.SecondRing] = new SavedGear("power_ring", Rarity.Common, []) });

        Assert.NotEqual(first, second);
    }

    // A saved piece's affixes are a list, compared by content: two saves of one piece are the same progress.
    [Fact]
    public void Equals_SameSavedGearInSeparateLists_IsTrue()
    {
        var first = Progress(new Dictionary<EquipmentSlot, SavedGear> { [EquipmentSlot.Weapon] = new("bronze_sword", Rarity.Rare, [new("power", 4), new("crit_chance", 2)]) });
        var second = Progress(new Dictionary<EquipmentSlot, SavedGear> { [EquipmentSlot.Weapon] = new("bronze_sword", Rarity.Rare, [new("power", 4), new("crit_chance", 2)]) });

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Equals_AffixesInOtherOrder_IsFalse()
    {
        var first = Progress(new Dictionary<EquipmentSlot, SavedGear> { [EquipmentSlot.Weapon] = new("bronze_sword", Rarity.Rare, [new("power", 4), new("crit_chance", 2)]) });
        var second = Progress(new Dictionary<EquipmentSlot, SavedGear> { [EquipmentSlot.Weapon] = new("bronze_sword", Rarity.Rare, [new("crit_chance", 2), new("power", 4)]) });

        Assert.NotEqual(first, second);
    }

    private static PartyMemberProgress Progress(IReadOnlyDictionary<EquipmentSlot, SavedGear> worn) =>
        new("sarxian", Level: 1, Experience: 0, Hp: 100, worn, new Dictionary<string, int>());
}
