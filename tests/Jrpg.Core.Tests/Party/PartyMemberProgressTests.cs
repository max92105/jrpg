using Jrpg.Core.Items;
using Jrpg.Core.Party;

namespace Jrpg.Core.Tests.Party;

public sealed class PartyMemberProgressTests
{
    [Fact]
    public void Equals_SameWornInOtherOrder_IsTrue()
    {
        var first = Progress(new Dictionary<EquipmentSlot, string> { [EquipmentSlot.Weapon] = "bronze_sword", [EquipmentSlot.FirstRing] = "power_ring" });
        var second = Progress(new Dictionary<EquipmentSlot, string> { [EquipmentSlot.FirstRing] = "power_ring", [EquipmentSlot.Weapon] = "bronze_sword" });

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Equals_RingInTheOtherSlot_IsFalse()
    {
        var first = Progress(new Dictionary<EquipmentSlot, string> { [EquipmentSlot.FirstRing] = "power_ring" });
        var second = Progress(new Dictionary<EquipmentSlot, string> { [EquipmentSlot.SecondRing] = "power_ring" });

        Assert.NotEqual(first, second);
    }

    private static PartyMemberProgress Progress(IReadOnlyDictionary<EquipmentSlot, string> worn) =>
        new("sarxian", Level: 1, Experience: 0, Hp: 100, worn, new Dictionary<string, int>());
}
