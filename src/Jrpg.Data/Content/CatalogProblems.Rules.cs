using Jrpg.Core.Content;
using Jrpg.Data.Crafting;

namespace Jrpg.Data.Content;

/// <summary>
/// What the loader would refuse in <c>rules.json</c>, written whole by the editor (ADR 0061, decision 955): its
/// sections' own rules, the classes' skill levels against its experience table, and the Forge's materials.
/// </summary>
public static partial class CatalogProblems
{
    /// <summary>
    /// The first problem in the rules, in the loader's order: a section's own rule, a class skill learned past the
    /// table's max level (decision 965), then a Forge material the items lack; null when the loader would take them.
    /// </summary>
    /// <param name="rules">The rules as edited.</param>
    /// <param name="content">The classes whose skills the table must reach, and the items the Forge names.</param>
    public static string? OfRules(GameRules rules, ContentCatalog content) =>
        FirstProblem(() =>
        {
            CheckRules(rules, content);
            CraftingRulesValidator.Validate(ContentCatalogLoader.RulesFile, rules.Crafting, content.Items);
        });

    /// <summary>
    /// The checks the loader makes on <c>rules.json</c> as it reads it: every section's own rule, then the classes'
    /// skill levels against its experience table, so a shortened table stranding a class skill is refused here too. The
    /// Forge's materials are left to <see cref="CheckItems"/>, which Save all runs too: decision 1020 checks them once.
    /// </summary>
    /// <param name="rules">The rules as they will be written.</param>
    /// <param name="content">The classes whose skills the table must reach, their own rules already checked.</param>
    /// <exception cref="ContentValidationException">The first problem, naming the file.</exception>
    internal static void CheckRules(GameRules rules, ContentCatalog content)
    {
        GameRulesValidator.Validate(ContentCatalogLoader.RulesFile, rules);
        CheckClassSkillLevels(ClassesById(content), rules.Experience);
    }
}
