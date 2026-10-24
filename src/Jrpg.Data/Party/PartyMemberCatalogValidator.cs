using Jrpg.Core.Party;
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

        if (string.IsNullOrWhiteSpace(member.Summary))
        {
            return "summary must not be empty.";
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
        return repeatedSkillId is null ? null : $"skillIds lists '{repeatedSkillId}' more than once.";
    }
}
