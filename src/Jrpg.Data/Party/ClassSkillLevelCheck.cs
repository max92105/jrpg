using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Data.Content;

namespace Jrpg.Data.Party;

/// <summary>
/// Every class skill's level within the experience table (decision 965): a skill learned past the max level could never
/// be learned. Run once both are read, as the classes come before <c>rules.json</c>; the classes' own check refuses a
/// level below 1.
/// </summary>
internal static class ClassSkillLevelCheck
{
    public static void Check(string fileName, IReadOnlyList<ClassDefinition> classes, ExperienceTable table) =>
        new ContentListValidator<ClassDefinition>(fileName, "class", classDefinition => classDefinition.Id)
            .Validate(classes, classDefinition => FindProblem(classDefinition, table.MaxLevel));

    private static string? FindProblem(ClassDefinition classDefinition, int maxLevel)
    {
        var unreachable = classDefinition.Skills.FirstOrDefault(skill => skill.Level > maxLevel);
        return unreachable is null
            ? null
            : $"skills lists '{unreachable.SkillId}' at level {unreachable.Level}, past the max level {maxLevel}.";
    }
}
