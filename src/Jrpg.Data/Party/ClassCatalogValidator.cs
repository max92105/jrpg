using Jrpg.Core.Party;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;
using Jrpg.Data.Resources;

namespace Jrpg.Data.Party;

internal static class ClassCatalogValidator
{
    public static void Validate(string fileName, IReadOnlyList<ClassDefinition> classes, ClassReferences references) =>
        new ContentListValidator<ClassDefinition>(fileName, "class", classDefinition => classDefinition.Id)
            .Validate(classes, classDefinition => FindProblem(classDefinition, references));

    private static string? FindProblem(ClassDefinition classDefinition, ClassReferences references) =>
        string.IsNullOrWhiteSpace(classDefinition.Name) ? "name must not be empty."
        : string.IsNullOrWhiteSpace(classDefinition.Description) ? "description must not be empty."
        : classDefinition.Base.MaxHp < 1 ? $"base.maxHp must be at least 1 (was {classDefinition.Base.MaxHp})."
        : FindNegativeStat("base", classDefinition.Base)
            ?? FindNegativeStat("growth", classDefinition.Growth)
            ?? ResourceDefinitionValidator.FindProblem(classDefinition.Resources)
            ?? FindSkillProblem(classDefinition.SkillIds, references)
            ?? FindForeignResourceProblem(classDefinition, references)
            ?? FindUnreachableRequirementProblem(classDefinition, references);

    // Growth may be 0 for a stat a class never improves; a negative one would shrink it with every level.
    private static string? FindNegativeStat(string field, ClassStats stats) =>
        stats.MaxHp < 0 ? $"{field}.maxHp must be at least 0 (was {stats.MaxHp})."
        : stats.Defense < 0 ? $"{field}.defense must be at least 0 (was {stats.Defense})."
        : stats.Speed < 0 ? $"{field}.speed must be at least 0 (was {stats.Speed})."
        : stats.Accuracy < 0 ? $"{field}.accuracy must be at least 0 (was {stats.Accuracy})."
        : FindNegativePrimary($"{field}.primaries", stats.Primaries);

    private static string? FindNegativePrimary(string field, PrimaryStats primaries) =>
        primaries.Power < 0 ? $"{field}.power must be at least 0 (was {primaries.Power})."
        : primaries.Agility < 0 ? $"{field}.agility must be at least 0 (was {primaries.Agility})."
        : primaries.Intelligence < 0 ? $"{field}.intelligence must be at least 0 (was {primaries.Intelligence})."
        : primaries.Constitution < 0 ? $"{field}.constitution must be at least 0 (was {primaries.Constitution})."
        : primaries.Will < 0 ? $"{field}.will must be at least 0 (was {primaries.Will})."
        : null;

    private static string? FindSkillProblem(IReadOnlyList<string> skillIds, ClassReferences references)
    {
        // The JSON reader lets a null list element through, and a frozen lookup throws on a null key.
        if (skillIds.Any(string.IsNullOrWhiteSpace))
        {
            return "skillIds must not contain an empty id.";
        }

        var unknownSkillId = skillIds.FirstOrDefault(id => !references.Skills.ContainsKey(id));
        if (unknownSkillId is not null)
        {
            return $"skillIds has unknown skill id '{unknownSkillId}'.";
        }

        var repeatedSkillId = skillIds.GroupBy(id => id).FirstOrDefault(group => group.Count() > 1)?.Key;
        return repeatedSkillId is null ? null : $"skillIds lists '{repeatedSkillId}' more than once.";
    }

    // A class's skills only use its own resources (ADR 0020); this runs once the skill ids are known valid.
    private static string? FindForeignResourceProblem(ClassDefinition classDefinition, ClassReferences references)
    {
        var ownIds = classDefinition.Resources.Select(resource => resource.Id).ToHashSet();
        var foreign = classDefinition.SkillIds
            .SelectMany(
                id => ResourceIdsUsedBy(references.Skills[id]),
                (id, resourceId) => (SkillId: id, ResourceId: resourceId))
            .FirstOrDefault(use => !ownIds.Contains(use.ResourceId));
        return foreign.SkillId is null
            ? null
            : $"skillIds lists '{foreign.SkillId}', which uses '{foreign.ResourceId}', a resource {classDefinition.Id} doesn't have.";
    }

    // A requirement above the max could never be met, so the skill would stay greyed for good.
    private static string? FindUnreachableRequirementProblem(ClassDefinition classDefinition, ClassReferences references)
    {
        // A speed-scaled max is only known in battle, so its requirements are not checked here.
        var maxima = classDefinition.Resources
            .Where(resource => resource.MaxFromSpeed is null)
            .ToDictionary(resource => resource.Id, resource => resource.Max);
        var (skillId, requirement) = classDefinition.SkillIds
            .SelectMany(id => references.Skills[id].Requirements, (id, requirement) => (id, requirement))
            .FirstOrDefault(use => maxima.TryGetValue(use.requirement.ResourceId, out var max) && use.requirement.AtLeast > max);
        if (skillId is null)
        {
            return null;
        }

        var (resourceId, atLeast) = (requirement.ResourceId, requirement.AtLeast);
        return $"skillIds lists '{skillId}', which needs {atLeast} {resourceId}, above its max of {maxima[resourceId]}.";
    }

    private static IEnumerable<string> ResourceIdsUsedBy(SkillDefinition skill) =>
        skill.ResourceChanges.Select(change => change.ResourceId)
            .Concat(skill.Requirements.Select(requirement => requirement.ResourceId));
}
