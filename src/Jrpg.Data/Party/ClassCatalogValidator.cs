using Jrpg.Core.Party;
using Jrpg.Data.Content;

namespace Jrpg.Data.Party;

internal static class ClassCatalogValidator
{
    public static void Validate(string fileName, IReadOnlyList<ClassDefinition> classes) =>
        new ContentListValidator<ClassDefinition>(fileName, "class", classDefinition => classDefinition.Id)
            .Validate(classes, FindProblem);

    private static string? FindProblem(ClassDefinition classDefinition) =>
        string.IsNullOrWhiteSpace(classDefinition.Name) ? "name must not be empty."
        : string.IsNullOrWhiteSpace(classDefinition.Description) ? "description must not be empty."
        : classDefinition.Base.MaxHp < 1 ? $"base.maxHp must be at least 1 (was {classDefinition.Base.MaxHp})."
        : FindNegativeStat("base", classDefinition.Base) ?? FindNegativeStat("growth", classDefinition.Growth);

    // Growth may be 0 for a stat a class never improves; a negative one would shrink it with every level.
    private static string? FindNegativeStat(string field, ClassStats stats) =>
        stats.MaxHp < 0 ? $"{field}.maxHp must be at least 0 (was {stats.MaxHp})."
        : stats.MaxMp < 0 ? $"{field}.maxMp must be at least 0 (was {stats.MaxMp})."
        : stats.Attack < 0 ? $"{field}.attack must be at least 0 (was {stats.Attack})."
        : stats.Defense < 0 ? $"{field}.defense must be at least 0 (was {stats.Defense})."
        : null;
}
