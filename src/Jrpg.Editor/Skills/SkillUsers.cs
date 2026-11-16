using Jrpg.Core.Content;

namespace Jrpg.Editor.Skills;

/// <summary>
/// Who uses a skill, as the Skills page shows it read-only (decision 978): the page adds a skill to no class or enemy,
/// so the developer reads here which files still name it.
/// </summary>
internal static class SkillUsers
{
    /// <returns>
    /// "Learned by: Mage, Warlock", the classes listing the skill by id (the catalog keeps no file order), or "Learned
    /// by: no class".
    /// </returns>
    public static string LearnedBy(string skillId, ContentCatalog content)
    {
        var classes = content.Classes.Values
            .Where(characterClass => characterClass.SkillIds.Contains(skillId))
            .OrderBy(characterClass => characterClass.Id, StringComparer.Ordinal)
            .ToList();
        return classes.Count == 0
            ? "Learned by: no class"
            : $"Learned by: {string.Join(", ", classes.Select(characterClass => characterClass.Name))}";
    }

    /// <returns>
    /// "Used by: Goblin Chief", the enemies whose tactics or phases name the skill, by id as the classes, or empty when
    /// no enemy names it.
    /// </returns>
    public static string UsedBy(string skillId, ContentCatalog content)
    {
        var enemies = content.Enemies.Values
            .Where(enemy => enemy.NamesSkill(skillId))
            .OrderBy(enemy => enemy.Id, StringComparer.Ordinal)
            .ToList();
        return enemies.Count == 0 ? "" : $"Used by: {string.Join(", ", enemies.Select(enemy => enemy.Name))}";
    }
}
