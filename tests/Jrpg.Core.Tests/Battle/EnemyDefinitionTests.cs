using Jrpg.Core.Battle;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Battle;

// The skills an enemy knows: those its tactics then its phases name, the one walk the battle, Data's check and the
// editor's balance table share.
public sealed class EnemyDefinitionTests
{
    // Tactics: the attack, cleave, mend; the phase: smash, cleave again.
    private static readonly EnemyDefinition _chief = new EnemyDefinitionBuilder()
        .WithTactics(Uses(skillId: null), Uses("cleave"), Uses("mend"))
        .WithPhases(new EnemyPhase { BelowHpPercent = 50, Tactics = [Uses("smash"), Uses("cleave")] })
        .Build();

    [Fact]
    public void NamedSkillIds_SkillInTacticsAndAPhase_OnceInTacticsOrderWithoutTheAttack()
    {
        var named = _chief.NamedSkillIds;

        Assert.Equal(["cleave", "mend", "smash"], named);
    }

    [Fact]
    public void NamesSkill_SkillOnlyInAPhase_IsTrue()
    {
        var names = _chief.NamesSkill("smash");

        Assert.True(names);
    }

    [Fact]
    public void NamesSkill_SkillNamedNowhere_IsFalse()
    {
        var names = _chief.NamesSkill("fireball");

        Assert.False(names);
    }

    private static EnemyAction Uses(string? skillId) => new() { SkillId = skillId, Weight = 1 };
}
