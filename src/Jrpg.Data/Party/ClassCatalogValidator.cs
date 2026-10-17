using Jrpg.Core.Party;
using Jrpg.Data.Content;

namespace Jrpg.Data.Party;

internal static class ClassCatalogValidator
{
    public static void Validate(string fileName, IReadOnlyList<ClassDefinition> classes) =>
        new ContentListValidator<ClassDefinition>(fileName, "class", classDefinition => classDefinition.Id)
            .Validate(classes, classDefinition => string.IsNullOrWhiteSpace(classDefinition.Name) ? "name must not be empty." : null);
}
