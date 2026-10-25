namespace Jrpg.Core.Battle;

/// <summary>The resource gains that battle happenings trigger (ADR 0019); a new trigger is one method here.</summary>
internal static class ResourceTriggers
{
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
