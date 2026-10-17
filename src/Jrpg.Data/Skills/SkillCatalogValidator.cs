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

        return skill.MpCost < 0
            ? $"mpCost must be at least 0 (was {skill.MpCost})."
            : EffectValidator.FindProblem(skill.Effect);
    }
}
