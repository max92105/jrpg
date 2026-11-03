using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

/// <summary>
/// The sheet purge (decision 442) on a scratch sheet of the tests' own: two tiles, painted on maps and built into a
/// crate as each test needs; nothing in the test world uses it otherwise.
/// </summary>
public sealed class SheetPurgeTests
{
    private static readonly ContentCatalog _testWorld = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
    private static readonly int _scratchTile = _testWorld.Tiles.Keys.Max() + 1;
    private static readonly int _chestTile = _testWorld.Objects["chest"].Tiles[0].TileId;

    private static readonly ContentCatalog _catalog = _testWorld with
    {
        Tilesheets = new Dictionary<string, TilesheetDefinition>(_testWorld.Tilesheets)
        {
            ["scratch"] = new() { Id = "scratch", File = "Scratch.png", TileWidth = 16, TileHeight = 16, Margin = 0, Spacing = 0 },
        },
        Tiles = new Dictionary<int, TileDefinition>(_testWorld.Tiles)
        {
            [_scratchTile] = new() { Id = _scratchTile, IsWalkable = true, SheetId = "scratch", Column = 0, Row = 0 },
            [_scratchTile + 1] = new() { Id = _scratchTile + 1, IsWalkable = true, SheetId = "scratch", Column = 1, Row = 0 },
        },
    };

    [Fact]
    public void Purge_SheetUsedOnTwoMaps_ErasesItsCellsOnBoth()
    {
        var catalog = Painted(Painted(_catalog, "forest", new GridCell(0, 0)), "start", new GridCell(0, 0));

        var result = SheetPurge.Purge(catalog, "scratch");

        Assert.Equal(TileLayer.EmptyTileId, result.Catalog.Maps["forest"].Layout.Layers[0].TileIdAt(new GridCell(0, 0)));
        Assert.Equal(TileLayer.EmptyTileId, result.Catalog.Maps["start"].Layout.Layers[0].TileIdAt(new GridCell(0, 0)));
        Assert.Equal(["forest", "start"], result.ChangedMapIds);
        Assert.False(result.Catalog.Tilesheets.ContainsKey("scratch"));
        Assert.False(result.Catalog.Tiles.ContainsKey(_scratchTile));
    }

    [Fact]
    public void Purge_ObjectMadeOfItsTiles_RemovesTheObjectAndItsPlacements()
    {
        var catalog = WithCrate(_catalog, [_scratchTile]);
        catalog = Placed(catalog, "start", "crate_1");

        var result = SheetPurge.Purge(catalog, "scratch");

        Assert.False(result.Catalog.Objects.ContainsKey("crate"));
        Assert.DoesNotContain(result.Catalog.Maps["start"].Layout.Objects, placed => placed.Uid == "crate_1");
    }

    [Fact]
    public void Purge_ObjectMixingSheets_IsRemovedToo()
    {
        var result = SheetPurge.Purge(WithCrate(_catalog, [_chestTile, _scratchTile]), "scratch");

        Assert.False(result.Catalog.Objects.ContainsKey("crate"));
        Assert.True(result.Catalog.Tiles.ContainsKey(_chestTile));
    }

    [Fact]
    public void Purge_EventSwapsToARemovedObject_RefusesNamingTheMapAndObject()
    {
        var catalog = WithEventOnFirstStartObject(WithCrate(_catalog, [_scratchTile]), new MapEvent
        {
            Id = "swap_to_crate",
            Activation = EventActivation.Interact,
            Prompt = "Swap",
            Steps = [new SwapObjectStep("crate")],
        });

        Assert.Equal("Can't remove sheet scratch: an event on map start names object crate, which is made of its tiles.", SheetPurge.RefusalOf(catalog, "scratch"));
        Assert.Throws<InvalidOperationException>(() => SheetPurge.Purge(catalog, "scratch"));
    }

    [Fact]
    public void Purge_EventGatedOnARemovedObject_Refuses()
    {
        var catalog = WithEventOnFirstStartObject(WithCrate(_catalog, [_scratchTile]), new MapEvent
        {
            Id = "only_on_crate",
            Activation = EventActivation.Interact,
            Prompt = "Look",
            OnlyWhenObjectId = "crate",
            Steps = [new SwapObjectStep("chest")],
        });

        Assert.NotNull(SheetPurge.RefusalOf(catalog, "scratch"));
    }

    // The crate's own event names the crate; the placement goes with the purge, so nothing that stays names it.
    [Fact]
    public void Purge_EventOnARemovedPlacementNamingARemovedObject_IsNotRefused()
    {
        var catalog = Placed(WithCrate(_catalog, [_scratchTile]), "start", "crate_1", withEvent: true);
        var start = catalog.Maps["start"];
        var layout = start.Layout with
        {
            Objects = [.. start.Layout.Objects.Select(placed => placed.Uid == "crate_1"
                ? placed with { Events = [placed.Events[0] with { OnlyWhenObjectId = "crate", Steps = [new SwapObjectStep("crate")] }] }
                : placed)],
        };
        catalog = catalog with { Maps = new Dictionary<string, MapDefinition>(catalog.Maps) { ["start"] = new MapDefinition(layout, start.Settings) } };

        var refusal = SheetPurge.RefusalOf(catalog, "scratch");

        Assert.Null(refusal);
        Assert.DoesNotContain(SheetPurge.Purge(catalog, "scratch").Catalog.Maps["start"].Layout.Objects, placed => placed.Uid == "crate_1");
    }

    [Fact]
    public void Purge_OtherSheetsCellsAndObjects_AreUntouched()
    {
        var catalog = Painted(_catalog, "forest", new GridCell(0, 0));
        var forestBefore = catalog.Maps["forest"].Layout;

        var result = SheetPurge.Purge(catalog, "scratch");

        var forestAfter = result.Catalog.Maps["forest"].Layout;
        Assert.Equal(forestBefore.Layers[0].TileIdAt(new GridCell(1, 0)), forestAfter.Layers[0].TileIdAt(new GridCell(1, 0)));
        Assert.Equal(forestBefore.Objects.Count, forestAfter.Objects.Count);
        Assert.Equal(_catalog.Objects.Keys.Order(), result.Catalog.Objects.Keys.Order());
    }

    // Two cells on the forest and one on the start map; the crate placed twice on the start map, one with an event.
    [Fact]
    public void Purge_CountsCellsObjectsAndPlacementsWithEvents()
    {
        var catalog = Painted(Painted(Painted(_catalog, "forest", new GridCell(0, 0)), "forest", new GridCell(1, 0)), "start", new GridCell(0, 0));
        catalog = Placed(Placed(WithCrate(catalog, [_scratchTile]), "start", "crate_1"), "start", "crate_2", withEvent: true);

        var result = SheetPurge.Purge(catalog, "scratch");

        Assert.Equal((3, 1, 2, 1), (result.Cells, result.Objects, result.Placements, result.PlacementsWithEvents));
    }

    [Fact]
    public void Purge_MapsWithoutItsTiles_AreNotListedAsChanged()
    {
        var result = SheetPurge.Purge(Painted(_catalog, "forest", new GridCell(0, 0)), "scratch");

        Assert.Equal(["forest"], result.ChangedMapIds);
    }

    private static ContentCatalog Painted(ContentCatalog catalog, string mapId, GridCell cell)
    {
        var map = catalog.Maps[mapId];
        var ground = map.Layout.Layers[0];
        var rows = ground.Rows.Select((row, y) => y == cell.Y ? (IReadOnlyList<int>)[.. row.Select((id, x) => x == cell.X ? _scratchTile : id)] : row).ToList();
        var layout = map.Layout with { Layers = [ground with { Rows = rows }, .. map.Layout.Layers.Skip(1)] };
        return catalog with { Maps = new Dictionary<string, MapDefinition>(catalog.Maps) { [mapId] = new MapDefinition(layout, map.Settings) } };
    }

    private static ContentCatalog WithCrate(ContentCatalog catalog, IReadOnlyList<int> tileIds)
    {
        var crate = catalog.Objects["chest"] with { Id = "crate", Tiles = [.. tileIds.Select((id, x) => new ObjectTile(x, 0, id))] };
        return catalog with { Objects = new Dictionary<string, ObjectDefinition>(catalog.Objects) { ["crate"] = crate } };
    }

    private static ContentCatalog Placed(ContentCatalog catalog, string mapId, string uid, bool withEvent = false)
    {
        var map = catalog.Maps[mapId];
        var events = withEvent ? new[] { new MapEvent { Id = $"{uid}_0", Activation = EventActivation.Interact, Prompt = "Open", Steps = [new GiveItemStep("potion", 1)] } } : [];
        var placed = new PlacedObject { Uid = uid, ObjectId = "crate", Position = new Point2D(1, 1), Z = 1, Events = events };
        var layout = map.Layout with { Objects = [.. map.Layout.Objects, placed] };
        return catalog with { Maps = new Dictionary<string, MapDefinition>(catalog.Maps) { [mapId] = new MapDefinition(layout, map.Settings) } };
    }

    private static ContentCatalog WithEventOnFirstStartObject(ContentCatalog catalog, MapEvent mapEvent)
    {
        var start = catalog.Maps["start"];
        var first = start.Layout.Objects[0];
        var layout = start.Layout with { Objects = [first with { Events = [.. first.Events, mapEvent] }, .. start.Layout.Objects.Skip(1)] };
        return catalog with { Maps = new Dictionary<string, MapDefinition>(catalog.Maps) { ["start"] = new MapDefinition(layout, start.Settings) } };
    }
}
