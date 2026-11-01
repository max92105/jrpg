namespace Jrpg.Core.Battle;

/// <summary>
/// Enters the phases standing enemies' HP has reached (decisions 404, 405). A phase is latched: an enemy only ever
/// moves to a deeper one, so healing above a threshold changes nothing. Thresholds descend in content's order, which
/// Data checks.
/// </summary>
internal static class EnemyPhases
{
    public static void EnterReached(IReadOnlyList<Combatant> combatants, List<BattleEvent> events)
    {
        foreach (var enemy in combatants.Where(combatant => combatant.IsAlive))
        {
            if (DeepestReached(enemy) is { } phase && IsDeeper(phase, enemy.EnemyPhase))
            {
                enemy.EnemyPhase = phase;
                events.Add(new EnemyPhaseEntered(enemy, phase));
            }
        }
    }

    // Whole numbers, so nothing rounds; "below" is strict.
    private static EnemyPhase? DeepestReached(Combatant combatant) =>
        combatant.Enemy?.Phases.LastOrDefault(phase => combatant.Vitals.Hp * 100 < combatant.Vitals.MaxHp * phase.BelowHpPercent);

    private static bool IsDeeper(EnemyPhase phase, EnemyPhase? current) =>
        current is null || phase.BelowHpPercent < current.BelowHpPercent;
}
