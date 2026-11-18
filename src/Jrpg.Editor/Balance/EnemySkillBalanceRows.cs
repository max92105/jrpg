using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Effects;

namespace Jrpg.Editor.Balance;

/// <summary>
/// The balance table's enemy skill rows (decision 992): each enemy's damaging and healing skills at its own power, as
/// the battle builds the enemy's stats (Core's <see cref="Combatant.FromEnemy"/>).
/// </summary>
internal static class EnemySkillBalanceRows
{
    /// <returns>
    /// The enemies in id order (the catalog keeps no file order), each one's skills once, in the order its tactics then
    /// its phases' name them, a skill changing no HP left out.
    /// </returns>
    /// <param name="content">The catalog whose enemies and skills the rows read.</param>
    /// <param name="modifierPermille">The table's damage modifier, added to each enemy's modifiers' sum.</param>
    public static IReadOnlyList<EnemySkillBalanceRow> Of(ContentCatalog content, int modifierPermille) =>
    [
        .. content.Enemies.Values
            .OrderBy(enemy => enemy.Id, StringComparer.Ordinal)
            .SelectMany(enemy => RowsOf(enemy, content, modifierPermille)),
    ];

    private static IEnumerable<EnemySkillBalanceRow> RowsOf(EnemyDefinition enemy, ContentCatalog content, int modifierPermille)
    {
        var stats = Combatant.FromEnemy(enemy).Stats;
        return enemy.NamedSkillIds
            .Select(skillId => content.Skills[skillId])
            .Where(skill => EffectAmounts.ChangesHp(skill.Effect.Kind))
            .Select(skill => new EnemySkillBalanceRow(
                enemy.Name,
                stats.Power,
                skill.Name,
                SkillLines.TierLine(skill.Effect),
                skill.Effect.Hits,
                skill.Effect.Kind != EffectKind.Damage,
                SkillLines.AmountText(skill.Effect, stats, modifierPermille)));
    }
}
