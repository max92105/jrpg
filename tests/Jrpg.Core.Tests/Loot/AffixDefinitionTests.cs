using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Loot;

// Each affix is built anew, so its kinds are a list of its own: equal only by content, in order (decision 604).
public sealed class AffixDefinitionTests
{
    [Fact]
    public void Equals_SameKindsInOrder_AreEqual()
    {
        var first = Keen(EquipmentKind.Weapon, EquipmentKind.Ring);
        var second = Keen(EquipmentKind.Weapon, EquipmentKind.Ring);

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Equals_KindsInAnotherOrder_AreNotEqual()
    {
        var weaponFirst = Keen(EquipmentKind.Weapon, EquipmentKind.Ring);
        var ringFirst = Keen(EquipmentKind.Ring, EquipmentKind.Weapon);

        Assert.NotEqual(weaponFirst, ringFirst);
    }

    [Fact]
    public void Equals_SameKindsButAnotherGoldValue_AreNotEqual()
    {
        var keen = Keen(EquipmentKind.Weapon);
        var dearer = keen with { GoldValue = 90, Kinds = [EquipmentKind.Weapon] };

        Assert.NotEqual(keen, dearer);
    }

    private static AffixDefinition Keen(params EquipmentKind[] kinds) =>
        new()
        {
            Id = "keen",
            Stat = StatKind.CritChance,
            Min = 2,
            Max = 6,
            Kinds = kinds,
            Weight = 5,
            GoldValue = 40,
        };
}
