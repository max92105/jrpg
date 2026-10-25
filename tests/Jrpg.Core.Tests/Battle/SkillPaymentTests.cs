using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Battle;

// A mage starting each battle with 3 mana out of 10, and a cleric with 3 faith out of 5.
public sealed class SkillPaymentTests
{
    private static readonly ResourceDefinition _mana = new("mana", "Mana", Max: 10, StartsAt: 3);
    private static readonly ResourceDefinition _faith = new("faith", "Faith", Max: 5, StartsAt: 3);

    private readonly Combatant _mage = Combatant.FromPartyMember(PartyMember.Recruit(
        new PartyMemberBuilder().BuildDefinition(),
        new ClassBuilder().WithId("mage").WithResources(_mana).Build(),
        new Dictionary<string, SkillDefinition>()));

    private readonly Combatant _cleric = Combatant.FromPartyMember(PartyMember.Recruit(
        new PartyMemberBuilder().BuildDefinition(),
        new ClassBuilder().WithId("cleric").WithResources(_faith).Build(),
        new Dictionary<string, SkillDefinition>()));

    [Fact]
    public void CanPay_EnoughResource_ReturnsTrue()
    {
        var canPay = SkillPayment.CanPay(_mage, SkillChanging(-3));

        Assert.True(canPay);
    }

    [Fact]
    public void CanPay_ResourceShort_ReturnsFalse()
    {
        var canPay = SkillPayment.CanPay(_mage, SkillChanging(-4));

        Assert.False(canPay);
    }

    [Fact]
    public void LackingResourceOf_ResourceShort_ReturnsIt()
    {
        var lacking = SkillPayment.LackingResourceOf(_mage, SkillChanging(-4));

        Assert.Same(_mana, lacking);
    }

    [Fact]
    public void LackingResourceOf_NothingShort_ReturnsNull()
    {
        var lacking = SkillPayment.LackingResourceOf(_mage, SkillChanging(-3));

        Assert.Null(lacking);
    }

    [Fact]
    public void Pay_ResourceCost_SpendsAndReportsChange()
    {
        var events = new List<BattleEvent>();

        SkillPayment.Pay(_mage, SkillChanging(-2), events);

        Assert.Equal(1, _mage.Resources.AmountOf("mana"));
        Assert.Equal([new ResourceChanged(_mage, "mana", -2)], events);
    }

    [Fact]
    public void Pay_PositiveChange_GainsAndReports()
    {
        var events = new List<BattleEvent>();

        SkillPayment.Pay(_mage, SkillChanging(4), events);

        Assert.Equal(7, _mage.Resources.AmountOf("mana"));
        Assert.Equal([new ResourceChanged(_mage, "mana", 4)], events);
    }

    [Fact]
    public void Pay_GainAtMax_ReportsNothing()
    {
        _mage.Resources.Change("mana", 10);
        var events = new List<BattleEvent>();

        SkillPayment.Pay(_mage, SkillChanging(2), events);

        Assert.Equal(10, _mage.Resources.AmountOf("mana"));
        Assert.Empty(events);
    }

    [Fact]
    public void CanPay_RequirementMet_ReturnsTrue()
    {
        var canPay = SkillPayment.CanPay(_cleric, SkillNeedingFaith(3));

        Assert.True(canPay);
    }

    [Fact]
    public void CanPay_RequirementShort_ReturnsFalse()
    {
        var canPay = SkillPayment.CanPay(_cleric, SkillNeedingFaith(4));

        Assert.False(canPay);
    }

    [Fact]
    public void LackingResourceOf_RequirementShort_ReturnsIt()
    {
        var lacking = SkillPayment.LackingResourceOf(_cleric, SkillNeedingFaith(4));

        Assert.Same(_faith, lacking);
    }

    [Fact]
    public void Pay_RequirementOnly_SpendsNothing()
    {
        var events = new List<BattleEvent>();

        SkillPayment.Pay(_cleric, SkillNeedingFaith(3), events);

        Assert.Equal(3, _cleric.Resources.AmountOf("faith"));
        Assert.Empty(events);
    }

    private static SkillDefinition SkillNeedingFaith(int atLeast) =>
        SkillChanging(-1) with { ResourceChanges = [], Requirements = [new ResourceRequirement("faith", atLeast)] };

    private static SkillDefinition SkillChanging(int manaAmount) => new()
    {
        Id = "spell",
        Name = "Spell",
        Description = "",
        IconFile = "",
        MpCost = 0,
        ResourceChanges = [new ResourceChange("mana", manaAmount)],
        Effect = new Effect(EffectKind.Damage, 10, Targeting.SingleEnemy),
    };
}
