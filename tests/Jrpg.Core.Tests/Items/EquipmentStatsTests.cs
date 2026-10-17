using Jrpg.Core.Items;

namespace Jrpg.Core.Tests.Items;

public sealed class EquipmentStatsTests
{
    private readonly EquipmentStats _bronzeSword = new(EquipmentSlot.Weapon, AttackBonus: 5, DefenseBonus: 0, AllowedClassIds: ["warrior", "knight"]);

    [Fact]
    public void CanBeWornBy_ClassAllowed_ReturnsTrue()
    {
        var canBeWorn = _bronzeSword.CanBeWornBy("knight");

        Assert.True(canBeWorn);
    }

    [Fact]
    public void CanBeWornBy_ClassNotAllowed_ReturnsFalse()
    {
        var canBeWorn = _bronzeSword.CanBeWornBy("mage");

        Assert.False(canBeWorn);
    }

    [Fact]
    public void CanBeWornBy_EmptyAllowedClasses_ReturnsFalse()
    {
        var unwearable = _bronzeSword with { AllowedClassIds = [] };

        var canBeWorn = unwearable.CanBeWornBy("warrior");

        Assert.False(canBeWorn);
    }
}
