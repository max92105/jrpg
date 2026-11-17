using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// The classes' problem line (decision 944): the loader's own message for the edited list, the skills a class names
// checked against the content's skills.
public sealed class CatalogProblemsPartyTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private static readonly ClassDefinition _ranger =
        new("ranger", "Ranger", "A hunter who waits for the shot.", new ClassStats(160, 14), new ClassStats(15, 1));

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
        var problem = CatalogProblems.OfClasses([_ranger with { SkillIds = ["volley"] }], _catalog);

        Assert.Equal("classes.json: class 'ranger': skillIds has unknown skill id 'volley'.", problem);
    }
}
