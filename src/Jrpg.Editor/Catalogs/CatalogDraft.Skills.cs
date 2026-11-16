using Jrpg.Core.Effects;
using Jrpg.Core.Skills;
using Jrpg.Data.Content;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The battle model the Skills and Effects pages edit (decision 815): the skills, and the tiered effects they name,
/// each as one undo step.
/// </summary>
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

    /// <summary>
    /// Replaces the tiered effects as one undo step whose do and undo both rebind the catalog, so everything naming an
    /// effect follows it (decision 975); nothing when they are the same by content. No undo refusal: catalog steps undo
    /// in order, so a skill naming an effect the undo removes came in a later step, already undone.
    /// </summary>
    public void SetTieredEffects(IReadOnlyList<TieredEffect> effects)
    {
        var before = Current.TieredEffects;
        var after = effects.ToDictionary(effect => effect.Id, StringComparer.Ordinal);
        if (HasSameRecords(before, after, EqualityComparer<TieredEffect>.Default.Equals))
        {
            return;
        }

        Do(new CatalogEdit(
            catalog => CatalogReferences.Rebound(catalog with { TieredEffects = after }),
            catalog => CatalogReferences.Rebound(catalog with { TieredEffects = before })));
    }

    // Records compare lists by reference: the with-copy compares every field but the cost lists, the lists by content;
    // the effect compares its own grants by content.
    private static bool IsSameSkill(SkillDefinition before, SkillDefinition after) =>
        before with { ResourceChanges = after.ResourceChanges, Requirements = after.Requirements } == after
        && before.ResourceChanges.SequenceEqual(after.ResourceChanges)
        && before.Requirements.SequenceEqual(after.Requirements);
}
