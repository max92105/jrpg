using System.Collections.Frozen;
using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;
using Jrpg.Data.Items;
using Jrpg.Data.Party;
using Jrpg.Data.Skills;

namespace Jrpg.Data.Content;

public static class ContentCatalogLoader
{
    private const string ClassesFile = "classes.json";
    private const string RulesFile = "rules.json";
    private const string ItemsFile = "items.json";
    private const string SkillsFile = "skills.json";
    private const string PartyMembersFile = "party-members.json";

    /// <summary>Loads and validates every content file under <paramref name="contentRoot"/>.</summary>
    /// <exception cref="ContentValidationException">A file is malformed or breaks a content rule.</exception>
    public static ContentCatalog Load(string contentRoot)
    {
        var classes = ContentFileReader.Read<IReadOnlyList<ClassDefinition>>(Path.Combine(contentRoot, ClassesFile));
        ClassCatalogValidator.Validate(ClassesFile, classes);
        var classIds = classes.Select(classDefinition => classDefinition.Id).ToFrozenSet();

        var rules = ContentFileReader.Read<GameRules>(Path.Combine(contentRoot, RulesFile));
        GameRulesValidator.Validate(RulesFile, rules);

        var items = ContentFileReader.Read<IReadOnlyList<ItemDefinition>>(Path.Combine(contentRoot, ItemsFile));
        ItemCatalogValidator.Validate(ItemsFile, items, classIds);

        var skills = ContentFileReader.Read<IReadOnlyList<SkillDefinition>>(Path.Combine(contentRoot, SkillsFile));
        SkillCatalogValidator.Validate(SkillsFile, skills);

        var members = ContentFileReader.Read<IReadOnlyList<PartyMemberDefinition>>(Path.Combine(contentRoot, PartyMembersFile));
        var memberReferences = new PartyMemberReferences(classIds, skills.Select(skill => skill.Id).ToFrozenSet(), rules.Experience);
        PartyMemberCatalogValidator.Validate(PartyMembersFile, members, memberReferences);

        return new ContentCatalog
        {
            Items = items.ToFrozenDictionary(item => item.Id),
            Classes = classes.ToFrozenDictionary(classDefinition => classDefinition.Id),
            Skills = skills.ToFrozenDictionary(skill => skill.Id),
            PartyMembers = members.ToFrozenDictionary(member => member.Id),
            Rules = rules,
        };
    }
}
