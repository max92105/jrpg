using Jrpg.Core.Effects;
using Jrpg.Core.Randomness;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;

namespace Jrpg.Core.Battle;

/// <summary>
/// An effect landing on its targets, in order: each of its hits, the status it grants to a target still standing, and
/// what a fall sets off (ADR 0019, ADR 0021). A damaging hit rolls to land first, then to crit (decision 325), then to
/// be blocked (decision 348); one that misses is no hit at all, and a damage effect that missed every hit leaves no
/// status behind (decision 326). A stun rolls against its target's crowd control resistance (decision 324). Each landed
/// damaging hit heals its living hitter by their lifesteal's share of the HP it took (decision 346); an item's hits
/// never do (decision 321).
/// </summary>
/// <param name="combatants">Everyone in the battle, for the triggers a fall sets off.</param>
/// <param name="rolls">The battle's rolls, for whether each hit lands and crits, and whether a stun takes.</param>
internal sealed class EffectLanding(IReadOnlyList<Combatant> combatants, ChanceRolls rolls)
{
    /// <param name="actor">Who used it, for the gains a landed hit triggers.</param>
    /// <param name="effect">What lands.</param>
    /// <param name="targets">Who it lands on, already checked as legal.</param>
    /// <param name="events">Where what happened is reported.</param>
    public void Land(Combatant actor, Effect effect, IReadOnlyList<Combatant> targets, List<BattleEvent> events) =>
        LandOn(new HitSource(actor, actor.CurrentStats.LifeStealPermille, actor.CurrentStats.ArmorPenetrationPermille), effect, targets, events);

    /// <summary>
    /// Lands an item's effect: its hits carry none of the user's lifesteal or penetration (decision 321).
    /// </summary>
    /// <param name="actor">Who used the item, for the gains a landed hit triggers.</param>
    /// <param name="effect">What lands.</param>
    /// <param name="targets">Who it lands on, already checked as legal.</param>
    /// <param name="events">Where what happened is reported.</param>
    public void LandFromItem(Combatant actor, Effect effect, IReadOnlyList<Combatant> targets, List<BattleEvent> events) =>
        LandOn(new HitSource(actor, LifeStealPermille: 0, PenetrationPermille: 0), effect, targets, events);

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

    private void LandOn(HitSource source, Effect effect, IReadOnlyList<Combatant> targets, List<BattleEvent> events)
    {
        foreach (var target in targets)
        {
            var wasAlive = target.IsAlive;
            var hasLanded = LandHits(source, effect, target, events);
            if (wasAlive && !target.IsAlive)
            {
                Fall(target, combatants, events);
            }
            else if (effect.Status is { } grant && target.IsAlive && hasLanded)
            {
                Grant(grant, target, events);
            }
        }
    }

    private static HitAffinity AffinityOf(EffectKind kind, Mitigation mitigation) =>
        kind == EffectKind.Damage ? DamageFormula.AffinityOf(mitigation.ResistancePermille) : HitAffinity.Neutral;

    // An untyped hit, which only tests build, meets no resistance. The hitter's penetration takes its points off before
    // the cue reads it, so a penetrated hit no longer reads "Resisted" (decision 364).
    private static Mitigation MitigationOf(Combatant target, DamageType? type, HitSource source)
    {
        var stats = target.CurrentStats;
        return new Mitigation(
            type is { } hitType ? DamageFormula.PenetratedResistance(stats.Resistances.Of(hitType), source.PenetrationPermille) : 0,
            StatFormula.PermilleOf(StatusModifiers.ExtraDamageTakenPercent(target.Statuses)));
    }

    // Each hit is resolved and reported on its own (decision 206); the hits stop once the target falls.
    private bool LandHits(HitSource source, Effect effect, Combatant target, List<BattleEvent> events)
    {
        var actor = source.Actor;
        var hasLanded = false;
        for (var hit = 0; hit < effect.Hits && (hit == 0 || target.IsAlive); hit++)
        {
            if (effect.Kind == EffectKind.Damage && !Lands(actor, target))
            {
                events.Add(new HitMissed(target));
                continue;
            }

            hasLanded = true;
            var isCritical = effect.Kind == EffectKind.Damage && rolls.Succeeds(actor.CurrentStats.CritChancePermille);
            var mitigation = MitigationOf(target, effect.DamageType, source) with { IsBlocked = RollsBlocked(effect, target, events) };
            var crit = isCritical ? actor.CurrentStats.CritDamagePermille : DamageFormula.NormalHit;
            var amount = EffectResolver.Apply(effect, target.Vitals, crit, mitigation);
            if (effect.Kind != EffectKind.GrantStatus)
            {
                events.Add(new EffectApplied(target, effect.Kind, amount, AffinityOf(effect.Kind, mitigation), isCritical));
            }

            if (effect.Kind == EffectKind.Damage)
            {
                ResourceTriggers.OnHitTaken(target, amount, events);
                ResourceTriggers.OnHitLanded(actor, amount, events);
                StealLife(source, amount, events);
            }
        }

        return hasLanded;
    }

    // A landed damaging hit rolls its target's block after its crit (decision 348), reported before the hit's number.
    private bool RollsBlocked(Effect effect, Combatant target, List<BattleEvent> events)
    {
        if (effect.Kind != EffectKind.Damage || !rolls.Succeeds(target.CurrentStats.BlockChancePermille))
        {
            return false;
        }

        events.Add(new HitBlocked(target));
        return true;
    }

    // RestoreHp gives a fallen hitter nothing, and a hitter at max HP nothing more, so neither is reported.
    private static void StealLife(HitSource source, int hpTaken, List<BattleEvent> events)
    {
        var healed = source.Actor.Vitals.RestoreHp(DamageFormula.LifeStolen(hpTaken, source.LifeStealPermille));
        if (healed > 0)
        {
            events.Add(new LifeStolen(source.Actor, healed));
        }
    }

    // A stun rolls its chance against the target's crowd control resistance (decision 324); the other statuses land.
    private void Grant(StatusGrant grant, Combatant target, List<BattleEvent> events)
    {
        if (grant.Kind == StatusKind.Stun
            && !rolls.Succeeds(GrantScaling.StunChancePermille(grant.Power, target.CurrentStats.CrowdControlResistancePermille)))
        {
            events.Add(new StatusResisted(target, grant.Kind));
            return;
        }

        target.Statuses.Apply(grant);
        events.Add(new StatusApplied(target, grant.Kind, grant.Turns));
    }

    // Only damage rolls (decision 325): a heal, a revive or a status alone never misses.
    private bool Lands(Combatant actor, Combatant target) =>
        rolls.Succeeds(HitFormula.ChancePermille(actor.CurrentStats.AccuracyPermille, target.CurrentStats.DodgePermille));

    /// <summary>Who lands the hits, and what their hits carry from them.</summary>
    private sealed record HitSource(Combatant Actor, int LifeStealPermille, int PenetrationPermille);
}
