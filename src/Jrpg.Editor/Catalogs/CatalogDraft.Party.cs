using Jrpg.Core.Party;

namespace Jrpg.Editor.Catalogs;

/// <summary>The classes, edited by the Classes page (decision 945), each session recorded as one undo step.</summary>
internal sealed partial class CatalogDraft
{
    /// <summary>
    /// Replaces the classes as one undo step (decision 945); nothing when they are the same by content, skills and
    /// resources in order included. No undo refusal: only party members name a class, and catalog steps undo in order,
    /// so a member naming a class the undo removes came in a later step, already undone.
    /// </summary>
    public void SetClasses(IReadOnlyList<ClassDefinition> classes)
    {
        var before = Current.Classes;
        var after = classes.ToDictionary(classDefinition => classDefinition.Id, StringComparer.Ordinal);
        if (HasSameRecords(before, after, IsSameClass))
        {
            return;
        }

        Do(new CatalogEdit(catalog => catalog with { Classes = after }, catalog => catalog with { Classes = before }));
    }

    // As an enemy's: the with-copy compares every field but the two lists, the lists by content and order (the skills'
    // order is the menu's, ADR 0020).
    private static bool IsSameClass(ClassDefinition before, ClassDefinition after) =>
        before with { SkillIds = after.SkillIds, Resources = after.Resources } == after
        && before.SkillIds.SequenceEqual(after.SkillIds)
        && before.Resources.SequenceEqual(after.Resources);
}
