using Jrpg.Core.Items;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Items;

// Each record is built anew, so its fixed affixes are a list of its own: equal only by content (decision 604).
public sealed class ItemDefinitionTests
{
    [Fact]
    public void Equals_SameFixedAffixesInOrder_AreEqual()
    {
        var first = Cleaver(Power(7), Crit(4));
        var second = Cleaver(Power(7), Crit(4));

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Equals_FixedAffixesInAnotherOrder_AreNotEqual()
    {
        var powerFirst = Cleaver(Power(7), Crit(4));
        var critFirst = Cleaver(Crit(4), Power(7));

        Assert.NotEqual(powerFirst, critFirst);
    }

    [Fact]
    public void Equals_SameFixedAffixesButAnotherPrice_AreNotEqual()
    {
        var cleaver = Cleaver(Power(7));
        var dearer = cleaver with { Price = 900, FixedAffixes = [Power(7)] };

        Assert.NotEqual(cleaver, dearer);
    }

    private static ItemDefinition Cleaver(params RolledAffix[] affixes) =>
        new ItemDefinitionBuilder().WithId("chiefs_cleaver").AsWeapon(30).WithPrice(600).Build() with
        {
            IsUnique = true,
            FixedAffixes = affixes,
        };

    private static RolledAffix Power(int value) => new("power", StatKind.Power, value);

    private static RolledAffix Crit(int value) => new("crit_chance", StatKind.CritChance, value);
}
