using System.Text.Json.Nodes;
using Jrpg.Core.Content;
using Jrpg.Core.Loot;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// drop-tables.json is saved with the other catalogs (ADR 0045): sorted by id, the entries in their order (the roll's),
// a quantity of 1 left out (decision 600), checked against the items as the loader checks it.
public sealed class ContentWriterDropTableTests : IDisposable
{
    private static readonly DropEntry _potion = new("potion", 30);
    private static readonly DropEntry _scrap = new("scrap", 20);

    private readonly CopiedContentRoot _content = new("writer-drop-tables", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterDropTableTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void WriteMapCatalogs_DropTables_LoadTheSameAfterRelay()
    {
        _writer.WriteMapCatalogs(_catalog);

        var reloaded = ContentCatalogLoader.Load(_content.Path).DropTables;
        Assert.Equal(_catalog.DropTables.Keys.Order(StringComparer.Ordinal), reloaded.Keys.Order(StringComparer.Ordinal));
        Assert.All(_catalog.DropTables.Values, table => Assert.Equal(table, reloaded[table.Id]));
    }

    [Fact]
    public void WriteMapCatalogs_SingleCopyEntry_LeavesQuantitiesOut()
    {
        _writer.WriteMapCatalogs(WithTable(new DropTable("slime", [_potion])));

        var entry = WrittenEntries("slime")[0];
        Assert.False(entry.ContainsKey("minQuantity"));
        Assert.False(entry.ContainsKey("maxQuantity"));
    }

    [Fact]
    public void WriteMapCatalogs_EntryOfTwoToThree_WritesBothQuantities()
    {
        _writer.WriteMapCatalogs(WithTable(new DropTable("slime", [_scrap with { MinQuantity = 2, MaxQuantity = 3 }])));

        var entry = WrittenEntries("slime")[0];
        Assert.Equal((2, 3), ((int?)entry["minQuantity"], (int?)entry["maxQuantity"]));
    }

    // The other side of the filter: each quantity is left out on its own, so one of two writes only the max.
    [Fact]
    public void WriteMapCatalogs_EntryOfOneToTwo_WritesOnlyMaxQuantity()
    {
        _writer.WriteMapCatalogs(WithTable(new DropTable("slime", [_scrap with { MaxQuantity = 2 }])));

        var entry = WrittenEntries("slime")[0];
        Assert.False(entry.ContainsKey("minQuantity"));
        Assert.Equal(2, (int?)entry["maxQuantity"]);
    }

    // Given zeta before alpha, and alpha's entries scrap before potion: the tables sort, the entries keep their order.
    [Fact]
    public void WriteMapCatalogs_NewTables_WritesThemSortedWithEntriesInOrder()
    {
        var tables = new Dictionary<string, DropTable>(_catalog.DropTables)
        {
            ["zeta"] = new DropTable("zeta", [_potion]),
            ["alpha"] = new DropTable("alpha", [_scrap, _potion]),
        };

        _writer.WriteMapCatalogs(_catalog with { DropTables = tables });

        var ids = WrittenTables().Select(table => (string?)table["id"]).ToList();
        Assert.Equal(ids.Order(StringComparer.Ordinal), ids);
        Assert.Equal(["scrap", "potion"], WrittenEntries("alpha").Select(entry => (string?)entry["itemId"]));
    }

    [Fact]
    public void WriteMapCatalogs_UnknownDropItem_WritesNothing()
    {
        var before = File.ReadAllBytes(_content.PathOf("drop-tables.json"));
        var goblin = _catalog.DropTables["goblin"];
        var broken = goblin with { Entries = [new DropEntry("no_such_item", 10), .. goblin.Entries.Skip(1)] };

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMapCatalogs(WithTable(broken)));

        var expected = "drop-tables.json: drop table 'goblin': entries[0]: itemId has unknown item id 'no_such_item'.";
        Assert.Equal(expected, exception.Message);
        Assert.Equal(before, File.ReadAllBytes(_content.PathOf("drop-tables.json")));
    }

    private ContentCatalog WithTable(DropTable table) =>
        _catalog with { DropTables = new Dictionary<string, DropTable>(_catalog.DropTables) { [table.Id] = table } };

    private List<JsonNode> WrittenTables() =>
        [.. JsonNode.Parse(File.ReadAllText(_content.PathOf("drop-tables.json")))!.AsArray().Select(table => table!)];

    private List<JsonObject> WrittenEntries(string tableId) =>
        [.. WrittenTables()
            .Single(table => (string?)table["id"] == tableId)["entries"]!
            .AsArray()
            .Select(entry => entry!.AsObject())];
}
