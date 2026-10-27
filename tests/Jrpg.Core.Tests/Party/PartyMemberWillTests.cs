using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Party;

// Faith (max 5, starts full, lasting) follows Will at one point per 10 (decision 319). The cleric has 14 Will of their
// own: one point, so Faith runs to 6.
public sealed class PartyMemberWillTests
{
    private static readonly ResourceDefinition _faith =
        new("faith", "Faith", Max: 5, StartsAt: 5) { IsLasting = true, WillScaling = new WillScaling(WillPerPoint: 10, WillEffect.RaiseMax) };

    private readonly Inventory _inventory = new();

    [Fact]
    public void Recruit_FaithWithWill_StartsAtRaisedMax()
    {
        var cleric = Cleric(will: 14);

        Assert.Equal(6, cleric.LastingAmounts["faith"]);
    }

    [Fact]
    public void Rest_FaithWithWill_RefillsToRaisedMax()
    {
        var cleric = Cleric(will: 14);
        cleric.KeepLastingAmount("faith", 2);

        cleric.Rest();

        Assert.Equal(6, cleric.LastingAmounts["faith"]);
    }

    [Fact]
    public void HasAnythingToRest_FaithAtRaisedMax_IsFalse()
    {
        var cleric = Cleric(will: 14);

        Assert.False(cleric.HasAnythingToRest);
    }

    // 4 Will plus the amulet's 6 reach the point; taking it off drops the max back to 5, and the 6 held with it.
    [Fact]
    public void TakeOff_WillGear_ClampsFaith()
    {
        var cleric = Cleric(will: 4);
        var amulet = new ItemDefinitionBuilder().WithId("amulet").AsArmor().WithBonuses(new StatBonuses(new Dictionary<StatKind, int> { [StatKind.Will] = 6 })).Build();
        _inventory.Add(amulet, 1);
        Assert.True(new PartyEquipment(_inventory).TryEquip(cleric, amulet));
        cleric.Rest();
        Assert.Equal(6, cleric.LastingAmounts["faith"]);

        Assert.True(new PartyEquipment(_inventory).TryUnequip(cleric, EquipmentSlot.Armor));

        Assert.Equal(5, cleric.LastingAmounts["faith"]);
    }

    private static PartyMember Cleric(int will) => PartyMember.Recruit(
        new PartyMemberBuilder().BuildDefinition(),
        new ClassBuilder().WithPrimaries(PrimaryStats.None with { Will = will }, PrimaryStats.None).WithResources(_faith).Build(),
        new Dictionary<string, SkillDefinition>(),
        TestStatRules.Neutral);
}
