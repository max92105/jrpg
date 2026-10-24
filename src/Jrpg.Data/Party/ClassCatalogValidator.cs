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
        : null;
}
