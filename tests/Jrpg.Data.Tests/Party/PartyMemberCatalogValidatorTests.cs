using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Data.Content;
using Jrpg.Data.Party;

namespace Jrpg.Data.Tests.Party;

public sealed class PartyMemberCatalogValidatorTests
{
    private const string FileName = "party-members.json";

    private static readonly PartyMemberReferences _references = new(
        ClassIds: new HashSet<string> { "warrior" },
        SkillIds: new HashSet<string> { "heal" },
        Experience: new ExperienceTable([100, 120]));

    private static readonly PartyMemberDefinition _sarxian = new()
    {
        Id = "sarxian",
        Name = "Sarxian",
        ClassId = "warrior",
        PortraitFile = "Sarxian.png",
        BattleSpriteFile = "Sarxian.png",
        AttackEffectFile = "AttackPlayer.gif",
        Level = 1,
        Experience = 50,
        Hp = 200,
        MaxHp = 300,
        Mp = 20,
        MaxMp = 30,
        Attack = 15,
        Defense = 8,
        SkillIds = ["heal"],
    };

    public static TheoryData<PartyMemberDefinition> ValidMembers => new()
    {
        _sarxian,
        _sarxian with { Level = 1, Experience = 99 },
        _sarxian with { Level = 3, Experience = 0 },
    };

    public static TheoryData<PartyMemberDefinition, string> InvalidMembers => new()
    {
        { _sarxian with { Name = " " }, "name must not be empty." },
        { _sarxian with { ClassId = "necromancer" }, "classId 'necromancer' is not a known class." },
        { _sarxian with { SkillIds = ["heal", "meteor"] }, "skillIds has unknown skill id 'meteor'." },
        { _sarxian with { SkillIds = ["heal", "heal"] }, "skillIds lists 'heal' more than once." },
        { _sarxian with { Attack = -1 }, "attack must be at least 0 (was -1)." },
        { _sarxian with { Defense = -2 }, "defense must be at least 0 (was -2)." },
        { _sarxian with { MaxHp = 0, Hp = 0 }, "maxHp must be at least 1 (was 0)." },
        { _sarxian with { Hp = -1 }, "hp must be at least 0 (was -1)." },
        { _sarxian with { Hp = 301 }, "hp must be at most maxHp (300) (was 301)." },
        { _sarxian with { MaxMp = -1, Mp = 0 }, "maxMp must be at least 0 (was -1)." },
        { _sarxian with { Mp = -1 }, "mp must be at least 0 (was -1)." },
        { _sarxian with { Mp = 31 }, "mp must be at most maxMp (30) (was 31)." },
        { _sarxian with { Level = 0 }, "level must be at least 1 (was 0)." },
        { _sarxian with { Level = 4, Experience = 0 }, "level must be at most the experience table's maximum (3) (was 4)." },
        { _sarxian with { Experience = -1 }, "experience must be at least 0 (was -1)." },
        { _sarxian with { Experience = 100 }, "experience must be below 100, the threshold for level 1 (was 100)." },
        { _sarxian with { Level = 3, Experience = 5 }, "experience must be 0 at the maximum level (was 5)." },
    };

    [Theory]
    [MemberData(nameof(ValidMembers))]
    public void Validate_ValidMember_DoesNotThrow(PartyMemberDefinition member)
    {
        var exception = Record.Exception(() => Validate(member));

        Assert.Null(exception);
    }

    [Theory]
    [MemberData(nameof(InvalidMembers))]
    public void Validate_InvalidField_ThrowsNamingTheMemberAndField(PartyMemberDefinition member, string problem)
    {
        var exception = Assert.Throws<ContentValidationException>(() => Validate(member));

        Assert.Equal($"party-members.json: party member 'sarxian': {problem}", exception.Message);
    }

    private static void Validate(PartyMemberDefinition member) =>
        PartyMemberCatalogValidator.Validate(FileName, [member], _references);
}
