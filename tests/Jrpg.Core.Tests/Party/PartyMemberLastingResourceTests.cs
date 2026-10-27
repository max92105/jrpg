using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Party;

// Clarity (max 3, starts at 3) is lasting; mana is not.
public sealed class PartyMemberLastingResourceTests
{
    [Fact]
    public void Recruit_ClassWithLastingResource_StartsAtStartsAt()
    {
        var member = Mage();

        Assert.Equal(new Dictionary<string, int> { ["clarity"] = 3 }, member.LastingAmounts);
    }

    [Fact]
    public void KeepLastingAmount_AboveMax_Clamps()
    {
        var member = Mage();

        member.KeepLastingAmount("clarity", 5);

        Assert.Equal(3, member.LastingAmounts["clarity"]);
    }

    [Fact]
    public void KeepLastingAmount_BelowZero_Clamps()
    {
        var member = Mage();

        member.KeepLastingAmount("clarity", -2);

        Assert.Equal(0, member.LastingAmounts["clarity"]);
    }

    [Fact]
    public void KeepLastingAmount_ResourceNotLasting_Throws()
    {
        var member = Mage();

        Assert.Throws<ArgumentException>(() => member.KeepLastingAmount("mana", 2));
    }

    private static PartyMember Mage()
    {
        var clarity = new ResourceDefinition("clarity", "Clarity", Max: 3, StartsAt: 3) { IsLasting = true };
        var mana = new ResourceDefinition("mana", "Mana", Max: 10, StartsAt: 3);
        return PartyMember.Recruit(
            new PartyMemberBuilder().WithId("omer").BuildDefinition(),
            new ClassBuilder().WithId("mage").WithResources(clarity, mana).Build(),
            new Dictionary<string, SkillDefinition>(),
            TestStatRules.Neutral);
    }
}
