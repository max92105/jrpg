using Jrpg.Core.Skills;

namespace Jrpg.Editor.Catalogs;

/// <summary>The battle model the Skills page edits (decision 815): the skills as one undo step.</summary>
internal sealed partial class CatalogDraft
{
    /// <summary>
    /// Replaces the skills as one undo step (decision 974); nothing when they are the same by content, costs and the
    /// effect's grants in order included. No undo refusal: classes and enemies name skills, but the editor writes
    /// neither, so none of them can name a skill the undo removes.
    /// </summary>
    public void SetSkills(IReadOnlyList<SkillDefinition> skills)
    {
        var before = Current.Skills;
        var after = skills.ToDictionary(skill => skill.Id, StringComparer.Ordinal);
        if (HasSameRecords(before, after, IsSameSkill))
        {
            return;
        }

        Do(new CatalogEdit(catalog => catalog with { Skills = after }, catalog => catalog with { Skills = before }));
    }

    // Records compare lists by reference: the with-copy compares every field but the cost lists, the lists by content;
    // the effect compares its own grants by content.
    private static bool IsSameSkill(SkillDefinition before, SkillDefinition after) =>
        before with { ResourceChanges = after.ResourceChanges, Requirements = after.Requirements } == after
        && before.ResourceChanges.SequenceEqual(after.ResourceChanges)
        && before.Requirements.SequenceEqual(after.Requirements);
}
