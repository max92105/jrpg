using Jrpg.Core.Battle;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Battle;

// Every resource here follows Will at one point per 10; the members have 25 Will, so 2 points.
public sealed class ResourceStartsTests
{
    private static readonly WillScaling _raiseStart = new(WillPerPoint: 10, WillEffect.RaiseStart);
    private static readonly WillScaling _raiseMax = new(WillPerPoint: 10, WillEffect.RaiseMax);

    [Fact]
    public void For_WillRaisesStart_StartsHigherWithinMax()
    {
        var mana = new ResourceDefinition("mana", "Mana", Max: 4, StartsAt: 3) { WillScaling = _raiseStart };

        var start = ResourceStarts.For(Member(mana)).Single();

        Assert.Equal(new ResourceStart(mana, Max: 4, Amount: 4), start);
    }

    [Fact]
    public void For_WillRaisesMaxOfFullResource_StartsFullAtNewMax()
    {
        var stamina = new ResourceDefinition("stamina", "Stamina", Max: 6, StartsAt: 6) { WillScaling = _raiseMax };

        var start = ResourceStarts.For(Member(stamina)).Single();

        Assert.Equal(new ResourceStart(stamina, Max: 8, Amount: 8), start);
    }

    [Fact]
    public void For_WillRaisesMaxOfEmptyResource_StartsWhereItDid()
    {
        var ferocity = new ResourceDefinition("ferocity", "Ferocity", Max: 10, StartsAt: 0) { WillScaling = _raiseMax };

        var start = ResourceStarts.For(Member(ferocity)).Single();

        Assert.Equal(new ResourceStart(ferocity, Max: 12, Amount: 0), start);
    }

    // Speed 30 at 10 per charge gives 3 charges; Will adds its 2 to both the max and the start.
    [Fact]
    public void For_WillRaisesMaxOfSpeedCharges_AddsCharges()
    {
        var readiness = new ResourceDefinition("readiness", "Readiness") { MaxFromSpeed = new SpeedScaling(10), WillScaling = _raiseMax };

        var start = ResourceStarts.For(Member(readiness)).Single();

        Assert.Equal(new ResourceStart(readiness, Max: 5, Amount: 5), start);
    }

    // Will raises the start first, then the strain lowers it: 3 + 2, less 6 for the three missing clarity points at 2
    // each, is 0; the other way round it would be 2.
    [Fact]
    public void For_WillThenStrain_StrainLowersTheRaisedStart()
    {
        var mana = new ResourceDefinition("mana", "Mana", Max: 10, StartsAt: 3) { WillScaling = _raiseStart };
        var clarity = new ResourceDefinition("clarity", "Clarity", Max: 3, StartsAt: 3) { IsLasting = true, Strain = new StrainRule("mana", PerMissingPoint: 2) };
        var member = Member(mana, clarity);
        member.KeepLastingAmount("clarity", 0);

        var start = ResourceStarts.For(member)[0];

        Assert.Equal(new ResourceStart(mana, Max: 10, Amount: 0), start);
    }

    [Fact]
    public void For_NoWillScaling_IsTheDefinitions()
    {
        var mana = new ResourceDefinition("mana", "Mana", Max: 10, StartsAt: 3);

        var start = ResourceStarts.For(Member(mana)).Single();

        Assert.Equal(ResourceStart.Fixed(mana), start);
    }

    private static PartyMember Member(params ResourceDefinition[] resources) => PartyMember.Recruit(
        new PartyMemberBuilder().BuildDefinition(),
        new ClassBuilder().WithBase(new ClassStats(MaxHp: 100, Speed: 30)).WithPrimaries(PrimaryStats.None with { Will = 25 }, PrimaryStats.None).WithResources(resources).Build(),
        new Dictionary<string, SkillDefinition>(),
        TestStatRules.Neutral);
}
