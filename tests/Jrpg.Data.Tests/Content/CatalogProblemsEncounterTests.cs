using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// The encounters' problem line (decision 908): the loader's own message for the edited list, its enemies checked
// against the content's.
public sealed class CatalogProblemsEncounterTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private static readonly EncounterDefinition _ambush = new() { Id = "ambush", EnemyIds = ["goblin", "goblin"] };

    [Fact]
    public void OfEncounters_ValidList_IsNull() => Assert.Null(CatalogProblems.OfEncounters([_ambush], _catalog));

    [Fact]
    public void OfEncounters_UnknownEnemy_NamesIt()
    {
        var problem = CatalogProblems.OfEncounters([_ambush with { EnemyIds = ["goblin", "dragon"] }], _catalog);

        Assert.Equal("encounters.json: encounter 'ambush': enemyIds has unknown enemy id 'dragon'.", problem);
    }
}
