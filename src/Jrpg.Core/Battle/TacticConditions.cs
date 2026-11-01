namespace Jrpg.Core.Battle;

/// <summary>
/// Who meets a tactic's condition for an enemy now (decision 402). An ally is another combatant on the enemy's side,
/// never the enemy itself; "below" is strict, compared in whole numbers so nothing rounds.
/// </summary>
internal static class TacticConditions
{
    /// <exception cref="ArgumentOutOfRangeException">A kind this switch doesn't know.</exception>
    public static IReadOnlyList<Combatant> CombatantsMeeting(TacticCondition condition, Combatant enemy, BattleState battle) =>
        condition.Kind switch
        {
            TacticConditionKind.OwnHpBelow => IsBelow(enemy, condition.Percent) ? [enemy] : [],
            TacticConditionKind.AllyHpBelow => [.. AlliesOf(enemy, battle).Where(ally => ally.IsAlive && IsBelow(ally, condition.Percent))],
            TacticConditionKind.AllyFallen => [.. AlliesOf(enemy, battle).Where(ally => !ally.IsAlive)],
            _ => throw new ArgumentOutOfRangeException(nameof(condition), condition.Kind, "Unknown tactic condition."),
        };

    public static bool Holds(TacticCondition condition, Combatant enemy, BattleState battle) =>
        CombatantsMeeting(condition, enemy, battle).Count > 0;

    private static IEnumerable<Combatant> AlliesOf(Combatant enemy, BattleState battle) =>
        battle.Combatants.Where(combatant => combatant.Side == enemy.Side && combatant != enemy);

    private static bool IsBelow(Combatant combatant, int percent) =>
        combatant.Vitals.Hp * 100 < combatant.Vitals.MaxHp * percent;
}
