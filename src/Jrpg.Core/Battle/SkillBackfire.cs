using Jrpg.Core.Randomness;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;

namespace Jrpg.Core.Battle;

/// <summary>
/// Whether a spell turns on its caster (decision 194): one roll per backfire-prone resource the skill changed, after
/// its effect. It rolls only while that resource holds points, so other battles draw the same random numbers.
/// </summary>
internal sealed class SkillBackfire(IReadOnlyList<Combatant> combatants, IRandom random)
{
    // A spell that already won the battle doesn't roll: a backfire can't turn a victory into a defeat.
    public void Roll(Combatant caster, SkillDefinition skill, List<BattleEvent> events)
    {
        foreach (var resource in caster.Resources.Definitions)
        {
            if (resource.Backfire is not { } rule || !CanBackfire(caster, skill, resource.Id))
            {
                continue;
            }

            var chance = BackfireFormula.ChancePercent(caster.Resources.AmountOf(resource.Id), rule);
            if (random.NextBelow(100) < chance)
            {
                Backfire(caster, resource.Id, BackfireFormula.Damage(caster.Vitals.MaxHp, rule), events);
            }
        }
    }

    private bool CanBackfire(Combatant caster, SkillDefinition skill, string resourceId) =>
        skill.ResourceChanges.Any(change => change.ResourceId == resourceId)
        && caster.Resources.AmountOf(resourceId) > 0
        && caster.IsAlive
        && combatants.Any(combatant => combatant.Side == BattleSide.Enemies && combatant.IsAlive);

    private void Backfire(Combatant caster, string resourceId, int damage, List<BattleEvent> events)
    {
        var hpLost = caster.Vitals.TakeDamage(damage);
        events.Add(new Backfired(caster, resourceId, hpLost));
        if (!caster.IsAlive)
        {
            EffectLanding.Fall(caster, combatants, events);
        }
    }
}
