using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Core.Party;

namespace Jrpg.Editor.Balance;

/// <summary>
/// The balance table's class skill rows (decision 992): each class's damaging and healing skills at the table's levels,
/// as a member of the class at each level wearing nothing lands them (Core's <see cref="ClassBattleStats"/>).
/// </summary>
internal static class ClassSkillBalanceRows
{
    // What a skill reads at a level below the one the class learns it at (ADR 0062).
    private const string NotLearned = "-";

    /// <returns>
    /// The classes in id order (the catalog keeps no file order), each one's skills in its list's order, a skill
    /// changing no HP left out.
    /// </returns>
    /// <param name="content">The catalog whose classes, skills and stat rules the rows read.</param>
    /// <param name="levels">The table's levels in the boxes' order, each from 1.</param>
    /// <param name="modifierPermille">The table's damage modifier, added to each member's modifiers' sum.</param>
    public static IReadOnlyList<SkillBalanceRow> Of(ContentCatalog content, IReadOnlyList<int> levels, int modifierPermille) =>
    [
        .. content.Classes.Values
            .OrderBy(characterClass => characterClass.Id, StringComparer.Ordinal)
            .SelectMany(characterClass => RowsOf(characterClass, content, levels, modifierPermille)),
    ];

    private static IEnumerable<SkillBalanceRow> RowsOf(
        ClassDefinition characterClass,
        ContentCatalog content,
        IReadOnlyList<int> levels,
        int modifierPermille)
    {
        var members = levels
            .Select(level => (Level: level, Stats: ClassBattleStats.At(characterClass, level, content.Rules.Stats)))
            .ToList();
        return characterClass.Skills
            .Select(entry => (LearnedAt: entry.Level, Skill: content.Skills[entry.SkillId]))
            .Where(entry => EffectAmounts.ChangesHp(entry.Skill.Effect.Kind))
            .Select(entry => new SkillBalanceRow(
                characterClass.Name,
                entry.Skill.Name,
                SkillLines.TierLine(entry.Skill.Effect),
                entry.Skill.Effect.Hits,
                entry.Skill.Effect.Kind != EffectKind.Damage,
                [
                    .. members.Select(member => member.Level < entry.LearnedAt
                        ? NotLearned
                        : SkillLines.AmountText(entry.Skill.Effect, member.Stats, modifierPermille)),
                ]));
    }
}
