using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// A member knows the skills of their class; an enemy knows the ones its tactics name (ADR 0034).
public sealed class CombatantKnowsTests
{
    private static readonly SkillDefinition _strike = new()
    {
        Id = "strike",
        Name = "Strike",
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.Damage, 7, Targeting.SingleEnemy),
    };

    [Fact]
    public void Knows_MemberWithTheSkill_IsTrue()
    {
        var member = Combatant.FromPartyMember(new PartyMemberBuilder().WithSkills(_strike).Build());

        Assert.True(member.Knows(_strike));
    }

    [Fact]
    public void Knows_MemberWithoutTheSkill_IsFalse()
    {
        var member = Combatant.FromPartyMember(new PartyMemberBuilder().Build());

        Assert.False(member.Knows(_strike));
    }

    [Fact]
    public void CanUse_EnemyWhoseTacticsNameTheSkill_IsTrue()
    {
        var battle = EnemysTurn(new EnemyAction { SkillId = "strike", Weight = 1 });

        Assert.True(battle.CanUse(_strike));
    }

    [Fact]
    public void CanUse_EnemyWhoseTacticsDontNameTheSkill_IsFalse()
    {
        var battle = EnemysTurn(new EnemyAction { Weight = 1 }, new EnemyAction { SkillId = "cleave", Weight = 1 });

        Assert.False(battle.CanUse(_strike));
    }

    // The shuffle value (0) swaps the enemy to the front, so it is the enemy's turn.
    private static BattleState EnemysTurn(params EnemyAction[] actions)
    {
        var member = Combatant.FromPartyMember(new PartyMemberBuilder().Build());
        var enemy = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithTactics(actions).Build());
        return new BattleState([member, enemy], new Inventory(), new FixedSequenceRandom(0));
    }
}
