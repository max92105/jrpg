using Jrpg.Core.Effects;
using Jrpg.Core.Skills;

namespace Jrpg.Core.Battle;

/// <summary>
/// Whether a rule's target can aim its skill, or the attack, after its trigger (decision 840, design.md section 3.3): a
/// target picks one combatant, so a skill hitting all, or aimed along the formation, fits automatic only; a foe without
/// the skill's statuses needs a skill granting a harmful one.
/// </summary>
public static class TacticTargetFit
{
    /// <param name="action">The rule, its trigger and its target.</param>
    /// <param name="skill">The skill it names; null is the attack.</param>
    public static bool Fits(EnemyAction action, SkillDefinition? skill)
    {
        if (action.On is not { } target)
        {
            return true;
        }

        return AimsFitting(target, action.When, skill).Contains(AimOf(skill));
    }

    /// <returns>
    /// The aim of <paramref name="skill"/>, or the attack's when it is null; Data's message names it.
    /// </returns>
    public static Targeting AimOf(SkillDefinition? skill) => skill?.Effect.Targeting ?? AttackEffect.Targeting;

    // Without a trigger, "that one" names nobody, so a subject target fits no aim; without a harmful grant, every foe
    // is without the skill's statuses, so that target would only be a random pick, which automatic already is.
    private static Targeting[] AimsFitting(TacticTarget target, TacticCondition? condition, SkillDefinition? skill) =>
        target switch
        {
            TacticTarget.Self => [Targeting.Self, Targeting.SingleAlly],
            TacticTarget.LowestHpFoe or TacticTarget.HighestHpFoe => [Targeting.SingleEnemy],
            TacticTarget.LowestHpOnItsSide => [Targeting.SingleAlly],
            TacticTarget.Subject => condition is null ? [] : AimsNaming(TacticConditionKinds.ShapeOf(condition.Kind).Subject),
            TacticTarget.UnafflictedFoe => GrantsHarm(skill) ? [Targeting.SingleEnemy] : [],
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Unknown tactic target."),
        };

    private static bool GrantsHarm(SkillDefinition? skill) =>
        skill is not null && skill.Effect.Statuses.Any(grant => grant.Status.IsHarmful);

    private static Targeting[] AimsNaming(TacticSubject subject) =>
        subject switch
        {
            TacticSubject.Itself => [Targeting.Self, Targeting.SingleAlly],
            TacticSubject.Allies => [Targeting.SingleAlly],
            TacticSubject.FallenAllies => [Targeting.SingleFallenAlly],
            TacticSubject.Foes => [Targeting.SingleEnemy],
            _ => throw new ArgumentOutOfRangeException(nameof(subject), subject, "Unknown tactic subject."),
        };
}
