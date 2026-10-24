using Jrpg.Core.Party;
using Jrpg.Data.Content;
using Jrpg.Data.Party;

namespace Jrpg.Data.Tests.Party;

public sealed class PartyMemberCatalogValidatorTests
{
    private const string FileName = "party-members.json";

    private static readonly PartyMemberReferences _references = new(
        ClassIds: new HashSet<string> { "warrior" },
        SkillIds: new HashSet<string> { "heal" });

    private static readonly PartyMemberDefinition _sarxian = new()
    {
        Id = "sarxian",
        Name = "Sarxian",
        ClassId = "warrior",
        Summary = "A warlock who bargained for fire.",
        PortraitFile = "Sarxian.png",
        BattleSpriteFile = "Sarxian.png",
        AttackEffectFile = "AttackPlayer.png",
        SkillIds = ["heal"],
    };

    public static TheoryData<PartyMemberDefinition> ValidMembers => new()
    {
        _sarxian,
    };

    public static TheoryData<PartyMemberDefinition, string> InvalidMembers => new()
    {
        { _sarxian with { Name = " " }, "name must not be empty." },
        { _sarxian with { Summary = " " }, "summary must not be empty." },
        { _sarxian with { ClassId = "necromancer" }, "classId 'necromancer' is not a known class." },
        { _sarxian with { SkillIds = ["heal", "meteor"] }, "skillIds has unknown skill id 'meteor'." },
        { _sarxian with { SkillIds = ["heal", "heal"] }, "skillIds lists 'heal' more than once." },
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
