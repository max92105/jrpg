using Jrpg.Core.Effects;
using Jrpg.Core.Skills;

namespace Jrpg.Core.Battle;

/// <summary>
/// Whether an enemy can use a skill, and if not, why; content's check of an enemy's actions reads it.
/// </summary>
public static class EnemySkillUse
{
    /// <returns>Why an enemy can't use <paramref name="skill"/>; null when it can.</returns>
    public static EnemySkillRefusal? RefusalOf(SkillDefinition skill)
    {
        // Enemies have no class resources (ADR 0019) and no formation (ADR 0023); quick skills must spend a resource,
        // so the first rule covers them.
        if (skill.ResourceChanges.Count > 0 || skill.Requirements.Count > 0)
        {
            return EnemySkillRefusal.UsesClassResource;
        }

        return skill.Effect.Targeting switch
        {
            Targeting.AlliesInRange => EnemySkillRefusal.AimsAlongFormation,
            Targeting.OtherCombatant => EnemySkillRefusal.PlacesReadiedTurn,
            _ => null,
        };
    }
}
