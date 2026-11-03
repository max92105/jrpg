using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

public sealed class CatalogUsageTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    [Fact]
    public void UsersOfTile_TileOnSavedMapAndOpenMap_ListsBoth()
    {
        var unusedTileId = _catalog.Tiles.Keys.Max() + 1;
        var forest = _catalog.Maps["forest"].Layout;
        var start = _catalog.Maps["start"].Layout;
        var forestWithIt = new MapDefinition(WithTileAtTopLeft(forest, unusedTileId), _catalog.Maps["forest"].Settings);
        var saved = _catalog with { Maps = new Dictionary<string, MapDefinition>(_catalog.Maps) { ["forest"] = forestWithIt } };

        var users = CatalogUsage.UsersOfTile(unusedTileId, saved, WithTileAtTopLeft(start, unusedTileId));

        Assert.Equal(["map forest", "map start"], users);
    }

    [Fact]
    public void UsersOfTile_OpenMapNoLongerUsesIt_IgnoresItsSavedVersion()
    {
        var unusedTileId = _catalog.Tiles.Keys.Max() + 1;
        var forest = _catalog.Maps["forest"];
        var forestWithIt = new MapDefinition(WithTileAtTopLeft(forest.Layout, unusedTileId), forest.Settings);
        var saved = _catalog with { Maps = new Dictionary<string, MapDefinition>(_catalog.Maps) { ["forest"] = forestWithIt } };

        Assert.Empty(CatalogUsage.UsersOfTile(unusedTileId, saved, forest.Layout));
    }

    [Fact]
    public void UsersOfTile_TileOfAnObject_NamesTheObject()
    {
        var tileId = _catalog.Objects["chest"].Tiles[0].TileId;

        Assert.Contains("object chest", CatalogUsage.UsersOfTile(tileId, _catalog, openLayout: null));
    }

    [Fact]
    public void UsersOfTile_UnusedTile_IsEmpty()
    {
        Assert.Empty(CatalogUsage.UsersOfTile(_catalog.Tiles.Keys.Max() + 1, _catalog, openLayout: null));
    }

    // The chest's sheet, with one of its tiles also painted on the open forest.
    [Fact]
    public void OfSheet_SheetUsedByMapAndObject_CountsBoth()
    {
        var chestTile = _catalog.Tiles[_catalog.Objects["chest"].Tiles[0].TileId];
        var forest = WithTileAtTopLeft(_catalog.Maps["forest"].Layout, chestTile.Id);

        var usage = CatalogUsage.OfSheet(_catalog, chestTile.SheetId, forest);

        Assert.Equal(CatalogUsage.TilesCutFrom(chestTile.SheetId, _catalog), usage.Tiles.Select(tile => tile.Id));
        Assert.Contains("forest", usage.MapIds);
        Assert.Contains("chest", usage.ObjectIds);
    }

    [Fact]
    public void OfSheet_UnusedSheet_IsEmpty()
    {
        var usage = CatalogUsage.OfSheet(_catalog, "no_such_sheet", openLayout: null);

        Assert.Empty(usage.Tiles);
        Assert.Empty(usage.MapIds);
        Assert.Empty(usage.ObjectIds);
    }

    // A new object no saved map places, placed on the open start map only.
    [Fact]
    public void UsersOfObject_PlacedOnOpenMap_ListsIt()
    {
        var start = _catalog.Maps["start"].Layout;
        var placed = new PlacedObject { Uid = "tree_4_1", ObjectId = "tree_4", Position = new Point2D(1, 1), Z = 1 };

        var users = CatalogUsage.UsersOfObject(_catalog, "tree_4", start with { Objects = [.. start.Objects, placed] });

        Assert.Equal(["map start"], users);
    }

    // A swap only works in an object's event (the validator refuses it on a cell), so the start map's first placed
    // object gets one; the new object is placed nowhere.
    [Fact]
    public void UsersOfObject_SwapStepNamesIt_NamesTheMap()
    {
        var users = CatalogUsage.UsersOfObject(_catalog, "tree_4", WithSwapOnFirstObject(_catalog.Maps["start"].Layout, "tree_4"));

        Assert.Equal(["map start"], users);
    }

    [Fact]
    public void UsersOfObject_AnotherObjectSwappedTo_IsEmpty()
    {
        var users = CatalogUsage.UsersOfObject(_catalog, "tree_4", WithSwapOnFirstObject(_catalog.Maps["start"].Layout, "tree_5"));

        Assert.Empty(users);
    }

    [Fact]
    public void UsersOfObject_SwapOnlyInASavedMap_NamesTheMap()
    {
        var forest = _catalog.Maps["forest"];
        var swapping = new MapDefinition(WithSwapOnFirstObject(forest.Layout, "tree_4"), forest.Settings);
        var saved = _catalog with { Maps = new Dictionary<string, MapDefinition>(_catalog.Maps) { ["forest"] = swapping } };

        var users = CatalogUsage.UsersOfObject(saved, "tree_4", openLayout: null);

        Assert.Equal(["map forest"], users);
    }

    private static MapLayout WithSwapOnFirstObject(MapLayout layout, string objectId)
    {
        var first = layout.Objects[0];
        var swapping = new MapEvent { Id = $"{first.Uid}_swap", Activation = EventActivation.Interact, Steps = [new SwapObjectStep(objectId)] };
        return layout with { Objects = [first with { Events = [.. first.Events, swapping] }, .. layout.Objects.Skip(1)] };
    }

    private static MapLayout WithTileAtTopLeft(MapLayout layout, int tileId)
    {
        var ground = layout.Layers[0];
        IReadOnlyList<int> row = [tileId, .. ground.Rows[0].Skip(1)];
        return layout with { Layers = [ground with { Rows = [row, .. ground.Rows.Skip(1)] }, .. layout.Layers.Skip(1)] };
    }
}
