using Jrpg.Core.Effects;

namespace Jrpg.Core.Battle;

/// <summary>
/// An effect landing on its targets, in order: the change itself, the status it grants to a target still standing, and
/// what a fall sets off (ADR 0019, ADR 0021).
/// </summary>
internal static class EffectLanding
{
    /// <param name="effect">What lands.</param>
    /// <param name="targets">Who it lands on, already checked as legal.</param>
    /// <param name="combatants">Everyone in the battle, for the triggers a fall sets off.</param>
    /// <param name="events">Where what happened is reported.</param>
    public static void Land(Effect effect, IReadOnlyList<Combatant> targets, IReadOnlyList<Combatant> combatants, List<BattleEvent> events)
    {
        foreach (var target in targets)
        {
            var wasAlive = target.IsAlive;
            var amount = EffectResolver.Apply(effect, target.Vitals, target.CurrentStats.Defense);
            if (effect.Kind != EffectKind.GrantStatus)
            {
                events.Add(new EffectApplied(target, effect.Kind, amount));
            }

            if (effect.Kind == EffectKind.Damage)
            {
                ResourceTriggers.OnHitTaken(target, amount, events);
            }

            if (wasAlive && !target.IsAlive)
            {
                Fall(target, combatants, events);
            }
            else if (effect.Status is { } grant && target.IsAlive)
            {
                target.Statuses.Apply(grant);
                events.Add(new StatusApplied(target, grant.Kind, grant.Turns));
            }
        }
    }

    private static void Fall(Combatant fallen, IReadOnlyList<Combatant> combatants, List<BattleEvent> events)
    {
        events.Add(new CombatantDefeated(fallen));
        foreach (var status in fallen.Statuses.Clear())
        {
            events.Add(new StatusEnded(fallen, status));
        }

        ResourceTriggers.OnCombatantFallen(fallen, combatants, events);
    }
}
