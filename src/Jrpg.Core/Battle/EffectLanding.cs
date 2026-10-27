using Jrpg.Core.Effects;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;

namespace Jrpg.Core.Battle;

/// <summary>
/// An effect landing on its targets, in order: each of its hits, the status it grants to a target still standing, and
/// what a fall sets off (ADR 0019, ADR 0021). A damaging hit rolls to land first (decision 325); one that misses is no
/// hit at all, and a damage effect that missed every hit leaves no status behind (decision 326).
/// </summary>
/// <param name="combatants">Everyone in the battle, for the triggers a fall sets off.</param>
/// <param name="rolls">The battle's rolls, for whether each hit lands.</param>
internal sealed class EffectLanding(IReadOnlyList<Combatant> combatants, ChanceRolls rolls)
{
    /// <param name="actor">Who used it, for the gains a landed hit triggers.</param>
    /// <param name="effect">What lands.</param>
    /// <param name="targets">Who it lands on, already checked as legal.</param>
    /// <param name="events">Where what happened is reported.</param>
    public void Land(Combatant actor, Effect effect, IReadOnlyList<Combatant> targets, List<BattleEvent> events)
    {
        foreach (var target in targets)
        {
            var wasAlive = target.IsAlive;
            var hasLanded = LandHits(actor, effect, target, events);
            if (wasAlive && !target.IsAlive)
            {
                Fall(target, combatants, events);
            }
            else if (effect.Status is { } grant && target.IsAlive && hasLanded)
            {
                target.Statuses.Apply(grant);
                events.Add(new StatusApplied(target, grant.Kind, grant.Turns));
            }
        }
    }

    /// <summary>Reports a fall, clears the fallen's statuses and runs what a fall triggers.</summary>
    public static void Fall(Combatant fallen, IReadOnlyList<Combatant> combatants, List<BattleEvent> events)
    {
        events.Add(new CombatantDefeated(fallen));
        foreach (var status in fallen.Statuses.Clear())
        {
            events.Add(new StatusEnded(fallen, status));
        }

        ResourceTriggers.OnCombatantFallen(fallen, combatants, events);
    }

    private static HitAffinity AffinityOf(EffectKind kind, Mitigation mitigation) =>
        kind == EffectKind.Damage ? DamageFormula.AffinityOf(mitigation.ResistancePermille) : HitAffinity.Neutral;

    // An untyped hit, which only tests build, meets no resistance.
    private static Mitigation MitigationOf(Combatant target, DamageType? type)
    {
        var stats = target.CurrentStats;
        return new Mitigation(
            type is { } hitType ? stats.Resistances.Of(hitType) : 0,
            StatFormula.PermilleOf(StatusModifiers.ExtraDamageTakenPercent(target.Statuses)));
    }

    // Each hit is resolved and reported on its own (decision 206); the hits stop once the target falls.
    private bool LandHits(Combatant actor, Effect effect, Combatant target, List<BattleEvent> events)
    {
        var hasLanded = false;
        for (var hit = 0; hit < effect.Hits && (hit == 0 || target.IsAlive); hit++)
        {
            if (effect.Kind == EffectKind.Damage && !Lands(actor, target))
            {
                events.Add(new HitMissed(target));
                continue;
            }

            hasLanded = true;
            var mitigation = MitigationOf(target, effect.DamageType);
            var amount = EffectResolver.Apply(effect, target.Vitals, mitigation);
            if (effect.Kind != EffectKind.GrantStatus)
            {
                events.Add(new EffectApplied(target, effect.Kind, amount, AffinityOf(effect.Kind, mitigation)));
            }

            if (effect.Kind == EffectKind.Damage)
            {
                ResourceTriggers.OnHitTaken(target, amount, events);
                ResourceTriggers.OnHitLanded(actor, amount, events);
            }
        }

        return hasLanded;
    }

    // Only damage rolls (decision 325): a heal, a revive or a status alone never misses.
    private bool Lands(Combatant actor, Combatant target) =>
        rolls.Succeeds(HitFormula.ChancePermille(actor.CurrentStats.AccuracyPermille, target.CurrentStats.DodgePermille));
}
