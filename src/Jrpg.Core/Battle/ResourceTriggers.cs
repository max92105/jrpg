namespace Jrpg.Core.Battle;

/// <summary>The resource gains that battle happenings trigger (ADR 0019); a new trigger is one method here.</summary>
internal static class ResourceTriggers
{
    /// <summary>Applies <c>PerHitTaken</c> to a target the hit cost HP; a hit for 0 does nothing.</summary>
    public static void OnHitTaken(Combatant target, int hpLost, List<BattleEvent> events)
    {
        if (hpLost == 0)
        {
            return;
        }

        foreach (var resource in target.Resources.Definitions)
        {
            target.ChangeResource(resource.Id, resource.Gains.PerHitTaken, events);
        }
    }

    /// <summary>Applies <c>PerAllyFallen</c> to each living combatant on the fallen one's side.</summary>
    public static void OnCombatantFallen(Combatant fallen, IReadOnlyList<Combatant> combatants, List<BattleEvent> events)
    {
        foreach (var ally in combatants.Where(combatant => combatant.Side == fallen.Side && combatant.IsAlive))
        {
            foreach (var resource in ally.Resources.Definitions)
            {
                ally.ChangeResource(resource.Id, resource.Gains.PerAllyFallen, events);
            }
        }
    }
}
