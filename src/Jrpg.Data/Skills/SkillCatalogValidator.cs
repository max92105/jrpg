using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Data.Content;
using Jrpg.Data.Effects;

namespace Jrpg.Data.Skills;

internal static class SkillCatalogValidator
{
    public static void Validate(string fileName, IReadOnlyList<SkillDefinition> skills) =>
        new ContentListValidator<SkillDefinition>(fileName, "skill", skill => skill.Id)
            .Validate(skills, FindProblem);

    private static string? FindProblem(SkillDefinition skill)
    {
        if (string.IsNullOrWhiteSpace(skill.Name))
        {
            return "name must not be empty.";
        }

        return FindResourceChangeProblem(skill.ResourceChanges)
            ?? FindRequirementProblem(skill.Requirements)
            ?? EffectValidator.FindProblem(skill.Effect);
    }

    private static string? FindResourceChangeProblem(IReadOnlyList<ResourceChange> changes)
    {
        // The JSON reader lets a null list element through.
        if (changes.Any(change => change is null))
        {
            return "resourceChanges must not contain a null entry.";
        }

        // Each cost is checked against the amount alone, so two costs of one resource could pay less than their sum.
        var repeatedId = changes.GroupBy(change => change.ResourceId).FirstOrDefault(group => group.Count() > 1)?.Key;
        if (repeatedId is not null)
        {
            return $"resourceChanges lists '{repeatedId}' more than once.";
        }

        var idle = changes.FirstOrDefault(change => change.Amount == 0);
        return idle is null ? null : $"resourceChanges has a 0 amount for '{idle.ResourceId}', which does nothing.";
    }

    private static string? FindRequirementProblem(IReadOnlyList<ResourceRequirement> requirements)
    {
        if (requirements.Any(requirement => requirement is null))
        {
            return "requirements must not contain a null entry.";
        }

        var idle = requirements.FirstOrDefault(requirement => requirement.AtLeast < 1);
        return idle is null
            ? null
            : $"requirements: atLeast for '{idle.ResourceId}' must be at least 1 (was {idle.AtLeast}).";
    }
}
