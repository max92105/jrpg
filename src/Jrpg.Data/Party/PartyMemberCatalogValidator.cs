using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Data.Content;

namespace Jrpg.Data.Party;

internal static class PartyMemberCatalogValidator
{
    public static void Validate(string fileName, IReadOnlyList<PartyMemberDefinition> members, PartyMemberReferences references) =>
        new ContentListValidator<PartyMemberDefinition>(fileName, "party member", member => member.Id)
            .Validate(members, member => FindProblem(member, references));

    private static string? FindProblem(PartyMemberDefinition member, PartyMemberReferences references)
    {
        if (string.IsNullOrWhiteSpace(member.Name))
        {
            return "name must not be empty.";
        }

        if (!references.ClassIds.Contains(member.ClassId))
        {
            return $"classId '{member.ClassId}' is not a known class.";
        }

        var unknownSkillId = member.SkillIds.FirstOrDefault(id => !references.SkillIds.Contains(id));
        if (unknownSkillId is not null)
        {
            return $"skillIds has unknown skill id '{unknownSkillId}'.";
        }

        var repeatedSkillId = member.SkillIds.GroupBy(id => id).FirstOrDefault(group => group.Count() > 1)?.Key;
        if (repeatedSkillId is not null)
        {
            return $"skillIds lists '{repeatedSkillId}' more than once.";
        }

        return AtLeast("attack", member.Attack, 0)
            ?? AtLeast("defense", member.Defense, 0)
            ?? FindVitalsProblem(member)
            ?? FindProgressProblem(member, references.Experience);
    }

    private static string? FindVitalsProblem(PartyMemberDefinition member) =>
        AtLeast("maxHp", member.MaxHp, 1)
            ?? AtLeast("hp", member.Hp, 0)
            ?? AtMost("hp", member.Hp, "maxHp", member.MaxHp)
            ?? AtLeast("maxMp", member.MaxMp, 0)
            ?? AtLeast("mp", member.Mp, 0)
            ?? AtMost("mp", member.Mp, "maxMp", member.MaxMp);

    private static string? FindProgressProblem(PartyMemberDefinition member, ExperienceTable experience)
    {
        var levelProblem = AtLeast("level", member.Level, 1) ?? AtMost("level", member.Level, "the experience table's maximum", experience.MaxLevel);
        if (levelProblem is not null)
        {
            return levelProblem;
        }

        if (experience.IsMaxLevel(member.Level))
        {
            return member.Experience == 0 ? null : $"experience must be 0 at the maximum level (was {member.Experience}).";
        }

        // At or past the threshold, the first experience gain would report a step that goes backwards.
        var threshold = experience.ExperienceToReachNext(member.Level);
        return AtLeast("experience", member.Experience, 0)
            ?? (member.Experience < threshold ? null : $"experience must be below {threshold}, the threshold for level {member.Level} (was {member.Experience}).");
    }

    private static string? AtLeast(string field, int value, int minimum) =>
        value < minimum ? $"{field} must be at least {minimum} (was {value})." : null;

    private static string? AtMost(string field, int value, string limitName, int limit) =>
        value > limit ? $"{field} must be at most {limitName} ({limit}) (was {value})." : null;
}
