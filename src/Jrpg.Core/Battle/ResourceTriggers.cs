using Jrpg.Core.Resources;

namespace Jrpg.Core.Battle;

/// <summary>The resource gains that battle happenings trigger (ADR 0019); a new trigger is one method here.</summary>
internal static class ResourceTriggers
{
    /// <summary>
    /// Applies <c>PerHitTaken</c> to a target the hit cost HP, and <c>PerHeavyHit</c> too when the hit was heavy
    /// (<see cref="HeavyHitRule"/>); a hit for 0 does nothing.
    /// </summary>
    public static void OnHitTaken(Combatant target, int hpLost, List<BattleEvent> events)
    {
        if (hpLost == 0)
        {
            return;
        }

        foreach (var resource in target.Resources.Definitions)
        {
            var gains = resource.Gains;
            var isHeavy = gains.PerHeavyHit != 0 && HeavyHitRule.IsHeavy(hpLost, target.Vitals.MaxHp, gains.HeavyHitPercent);
            target.ChangeResource(resource.Id, gains.PerHitTaken + (isHeavy ? gains.PerHeavyHit : 0), events);
        }
    }

    /// <summary>
    /// Applies <c>PerAllyFallen</c> to each living combatant on the fallen one's side, and <c>PerEnemyFallen</c> to
    /// each living one on the other side.
    /// </summary>
    public static void OnCombatantFallen(Combatant fallen, IReadOnlyList<Combatant> combatants, List<BattleEvent> events)
    {
        foreach (var witness in combatants.Where(combatant => combatant.IsAlive))
        {
            foreach (var resource in witness.Resources.Definitions)
            {
                var gains = resource.Gains;
                witness.ChangeResource(resource.Id, witness.Side == fallen.Side ? gains.PerAllyFallen : gains.PerEnemyFallen, events);
            }
        }
    }
}
