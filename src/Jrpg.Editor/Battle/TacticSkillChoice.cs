using Jrpg.Core.Battle;
using Jrpg.Core.Content;

namespace Jrpg.Editor.Battle;

/// <summary>One skill as a rule's Use box offers it, or the attack (decision 851).</summary>
/// <param name="Id">
/// The skill's id, which the rule names; empty for the attack, as the Drop table box's empty one is none.
/// </param>
/// <param name="Name">The name the game shows, which the box lists.</param>
internal sealed record TacticSkillChoice(string Id, string Name)
{
    /// <returns>
    /// The attack, then the content's skills an enemy can use (Core's <see cref="EnemySkillUse"/>) by name, since the
    /// catalog keeps no file order. Data refuses an enemy naming any other skill, the Skills page's check included, so
    /// a box over these never misses a rule's skill.
    /// </returns>
    public static IReadOnlyList<TacticSkillChoice> AllIn(ContentCatalog content) =>
    [
        new("", "Attack"),
        .. content.Skills.Values
            .Where(skill => EnemySkillUse.RefusalOf(skill) is null)
            .OrderBy(skill => skill.Name, StringComparer.Ordinal)
            .Select(skill => new TacticSkillChoice(skill.Id, skill.Name)),
    ];
}
