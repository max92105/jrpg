using Jrpg.Core.Battle;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Battle;

// A member of a class at a level wearing nothing, as the battle builds its stats (decision 993): the balance table's
// reading of ClassSheet mapped by MemberBattleStats.
public sealed class ClassBattleStatsTests
{
    // Power 12 at level 1, +3 a level.
    private static readonly ClassDefinition _mage = new ClassBuilder()
        .WithPrimaries(new PrimaryStats(12, 6, 8, 5, 7), new PrimaryStats(3, 1, 2, 1, 2))
        .Build();

    // +1 % damage per Power point and a fire unarmed attack, each unlike CombatStats' default, so a mapping that drops
    // the damage percent or the attack type shows.
    private static readonly StatRules _rules =
        TestStatRules.Neutral with { DamagePercentPer100Power = 100, UnarmedDamageType = DamageType.Fire };

    [Fact]
    public void At_LevelOne_PowerIsTheBase() => Assert.Equal(12, ClassBattleStats.At(_mage, level: 1, _rules).Power);

    // 12 + 9 x 3.
    [Fact]
    public void At_LevelTen_PowerGrewNineTimes() => Assert.Equal(39, ClassBattleStats.At(_mage, level: 10, _rules).Power);

    [Fact]
    public void At_LevelZero_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => ClassBattleStats.At(_mage, level: 0, _rules));

    [Fact]
    public void At_SameClassAndLevel_EqualsARecruitedMembersStats()
    {
        var member = PartyMember.Recruit(
            new PartyMemberBuilder().BuildDefinition(), _mage, new Dictionary<string, SkillDefinition>(), _rules);
        member.GainExperience(30, new ExperienceTable([10, 20, 40]));

        var stats = ClassBattleStats.At(_mage, level: 3, _rules);

        Assert.Equal((3, Combatant.FromPartyMember(member).Stats), (member.Level, stats));
    }
}
