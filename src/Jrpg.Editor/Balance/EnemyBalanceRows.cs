using Jrpg.Core.Battle;
using Jrpg.Core.Content;

namespace Jrpg.Editor.Balance;

/// <summary>
/// The balance table's enemy rows (decision 992): each enemy's HP, power and basic attack, read from the combatant the
/// battle builds of it (Core's <see cref="Combatant.FromEnemy"/>).
/// </summary>
internal static class EnemyBalanceRows
{
    /// <returns>The enemies in id order (the catalog keeps no file order).</returns>
    public static IReadOnlyList<EnemyBalanceRow> Of(ContentCatalog content) =>
    [
        .. content.Enemies.Values
            .OrderBy(enemy => enemy.Id, StringComparer.Ordinal)
            .Select(Combatant.FromEnemy)
            .Select(enemy => new EnemyBalanceRow(
                enemy.Name,
                enemy.Vitals.MaxHp,
                enemy.Stats.Power,
                enemy.Stats.AttackDamage,
                enemy.Stats.AttackType)),
    ];
}
