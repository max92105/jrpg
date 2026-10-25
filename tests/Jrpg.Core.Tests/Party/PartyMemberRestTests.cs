using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Party;

// A member with 100 max HP whose class has Clarity (lasting, max 3, starts at 3).
public sealed class PartyMemberRestTests
{
    [Fact]
    public void Rest_Wounded_FullHp()
    {
        var member = Member();
        member.Vitals.TakeDamage(60);

        member.Rest();

        Assert.Equal(100, member.Vitals.Hp);
    }

    [Fact]
    public void Rest_Fallen_Revived()
    {
        var member = Member();
        member.Vitals.TakeDamage(100);

        member.Rest();

        Assert.Equal(100, member.Vitals.Hp);
    }

    [Fact]
    public void Rest_LastingSpent_RefilledToStartsAt()
    {
        var member = Member();
        member.KeepLastingAmount("clarity", 0);

        member.Rest();

        Assert.Equal(3, member.LastingAmounts["clarity"]);
    }

    private static PartyMember Member()
    {
        var clarity = new ResourceDefinition("clarity", "Clarity", Max: 3, StartsAt: 3) { IsLasting = true };
        return PartyMember.Recruit(
            new PartyMemberBuilder().WithId("omer").BuildDefinition(),
            new ClassBuilder().WithId("mage").WithResources(clarity).Build(),
            new Dictionary<string, SkillDefinition>());
    }
}
