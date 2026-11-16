using Jrpg.Core.Battle;
using Jrpg.Core.Loot;
using Jrpg.Data.Content;
using Jrpg.Editor.Loot;

namespace Jrpg.Editor.Tests.Loot;

// The key the page's hosts read back (IEditedCatalog): the chosen table's id.
public sealed class DropTableCatalogViewModelChoiceTests
{
    // Spare is chosen first (id order), so only Choose gives the wolf's id.
    [Fact]
    public void ChosenKey_ChosenTable_IsItsId()
    {
        var testWorld = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
        var catalog = new DropTableCatalogViewModel(testWorld with
        {
            DropTables = new Dictionary<string, DropTable>
            {
                ["wolf_drops"] = new("wolf_drops", [new DropEntry("potion", 30)]),
                ["spare"] = new("spare", [new DropEntry("potion", 5)]),
            },
            Enemies = new Dictionary<string, EnemyDefinition>(),
        });

        catalog.Choose("wolf_drops");

        Assert.Equal("wolf_drops", catalog.ChosenKey);
    }
}
