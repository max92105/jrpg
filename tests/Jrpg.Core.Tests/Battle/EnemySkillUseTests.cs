using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;

namespace Jrpg.Core.Tests.Battle;

// Enemies hold no class resources (ADR 0019) and stand in no formation (ADR 0023), so some skills are refused them.
public sealed class EnemySkillUseTests
{
    private static readonly Dictionary<string, SkillDefinition> _skills = new[]
    {
        Skill("fire_blast", Targeting.SingleEnemy) with { ResourceChanges = [new ResourceChange("mana", -2)] },
        Skill("prayer", Targeting.SingleEnemy) with { Requirements = [new ResourceRequirement("faith", 1)] },
        Skill("aura", Targeting.AlliesInRange),
        Skill("ready", Targeting.OtherCombatant),
        Skill("strike", Targeting.SingleEnemy),
    }.ToDictionary(skill => skill.Id);

    [Theory]
    [InlineData("fire_blast", EnemySkillRefusal.UsesClassResource)]
    [InlineData("prayer", EnemySkillRefusal.UsesClassResource)]
    [InlineData("aura", EnemySkillRefusal.AimsAlongFormation)]
    [InlineData("ready", EnemySkillRefusal.PlacesReadiedTurn)]
    [InlineData("strike", null)]
    public void RefusalOf_Skill_MatchesTheRule(string skillId, EnemySkillRefusal? expected)
    {
        var refusal = EnemySkillUse.RefusalOf(_skills[skillId]);

        Assert.Equal(expected, refusal);
    }

    private static SkillDefinition Skill(string id, Targeting aim) => new()
    {
        Id = id,
        Name = id,
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.Damage, 5, aim),
    };
}
