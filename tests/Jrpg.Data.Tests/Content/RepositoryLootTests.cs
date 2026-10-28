using Jrpg.Core.Content;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// The loot in the repository's content (the loot plan, D2): the goblin drops from a table that exists.
public sealed class RepositoryLootTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    [Fact]
    public void Goblin_DropsFromATableThatExists()
    {
        var tableId = _catalog.Enemies["goblin"].DropTableId;

        Assert.NotNull(tableId);
        Assert.NotEmpty(_catalog.DropTables[tableId].Entries);
    }
}
