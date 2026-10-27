using Jrpg.Core.Effects;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;

namespace Jrpg.Core.Battle;

/// <summary>
/// The effect a skill lands once its user's dominant force has empowered it (decision 195). It is judged before the
/// skill pays, so a skill's own gain can't make it the one it empowers.
/// </summary>
internal static class SkillEmpowerment
{
    /// <param name="actor">Who uses the skill.</param>
    /// <param name="skill">The skill, whose resource gains say which forces it feeds.</param>
    /// <param name="effect">The skill's effect as its user lands it (<see cref="EffectScaling"/>), so a dominant force
    /// raises the value Power already raised.</param>
    /// <returns>The effect, its power raised when a force the skill feeds dominates: the granted status's power for a
    /// grant, the effect's own otherwise.</returns>
    public static Effect EffectOf(Combatant actor, SkillDefinition skill, Effect effect)
    {
        if (DominantFedForce(actor, skill) is not { } rule)
        {
            return effect;
        }

        return effect is { Kind: EffectKind.GrantStatus, Status: { } grant }
            ? effect with { Status = grant with { Power = DominanceFormula.Empowered(grant.Power, rule.BonusPercent) } }
            : effect with { Power = DominanceFormula.Empowered(effect.Power, rule.BonusPercent) };
    }

    private static DominanceRule? DominantFedForce(Combatant actor, SkillDefinition skill) =>
        actor.Resources.Definitions
            .Where(resource => resource.Dominance is not null && actor.Resources.IsDominant(resource.Id))
            .FirstOrDefault(resource => skill.ResourceChanges.Any(change => change.ResourceId == resource.Id && change.Amount > 0))
            ?.Dominance;
}
