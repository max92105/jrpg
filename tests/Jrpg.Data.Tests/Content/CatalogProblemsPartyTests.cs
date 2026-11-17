using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// The classes' problem line (decision 944): the loader's own message for the edited list, the skills a class names
// checked against the content's skills and their levels against its experience table; the members' (decision 952), the
// class a member names checked against the content's classes.
public sealed class CatalogProblemsPartyTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private static readonly ClassDefinition _ranger =
        new("ranger", "Ranger", "A hunter who waits for the shot.", new ClassStats(160, 14), new ClassStats(15, 1));

    private static readonly PartyMemberDefinition _ilsa = new()
    {
        Id = "ilsa",
        Name = "Ilsa",
        ClassId = _ranger.Id,
        Summary = "A scout who never sleeps twice in one place.",
        PortraitFile = "Ilsa.png",
        BattleSpriteFile = "Ilsa.png",
        AttackEffectFile = "Arrow.png",
    };

    [Fact]
    public void OfClasses_ValidClasses_IsNull() => Assert.Null(CatalogProblems.OfClasses([_ranger], _catalog));

    [Fact]
    public void OfClasses_EmptyDescription_NamesTheClass()
    {
        var problem = CatalogProblems.OfClasses([_ranger with { Description = "" }], _catalog);

        Assert.Equal("classes.json: class 'ranger': description must not be empty.", problem);
    }

    [Fact]
    public void OfClasses_UnknownSkill_NamesIt()
    {
        var problem = CatalogProblems.OfClasses([_ranger with { Skills = [new("volley")] }], _catalog);

        Assert.Equal("classes.json: class 'ranger': skills has unknown skill id 'volley'.", problem);
    }

    // expose costs nothing, so the ranger without resources may name it and only its level is refused (decision 965).
    [Fact]
    public void OfClasses_SkillPastMaxLevel_NamesClassAndSkill()
    {
        var maxLevel = _catalog.Rules.Experience.MaxLevel;
        var ranger = _ranger with { Skills = [new("expose") { Level = maxLevel + 1 }] };

        var problem = CatalogProblems.OfClasses([ranger], _catalog);

        Assert.Equal(
            $"classes.json: class 'ranger': skills lists 'expose' at level {maxLevel + 1}, past the max level {maxLevel}.",
            problem);
    }

    [Fact]
    public void OfPartyMembers_ValidMembers_IsNull() =>
        Assert.Null(CatalogProblems.OfPartyMembers([_ilsa], WithRanger()));

    [Fact]
    public void OfPartyMembers_EmptySummary_NamesTheMember()
    {
        var problem = CatalogProblems.OfPartyMembers([_ilsa with { Summary = "" }], WithRanger());

        Assert.Equal("party-members.json: party member 'ilsa': summary must not be empty.", problem);
    }

    // The test world has no ranger: a member's class is checked against the content's classes.
    [Fact]
    public void OfPartyMembers_ClassTheContentLacks_NamesIt()
    {
        var problem = CatalogProblems.OfPartyMembers([_ilsa], _catalog);

        Assert.Equal("party-members.json: party member 'ilsa': classId 'ranger' is not a known class.", problem);
    }

    private static ContentCatalog WithRanger() =>
        _catalog with { Classes = new Dictionary<string, ClassDefinition>(_catalog.Classes) { [_ranger.Id] = _ranger } };
}
