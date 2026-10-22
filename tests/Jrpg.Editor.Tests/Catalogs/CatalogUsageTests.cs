using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

public sealed class CatalogUsageTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

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

    private static MapLayout WithTileAtTopLeft(MapLayout layout, int tileId)
    {
        var ground = layout.Layers[0];
        IReadOnlyList<int> row = [tileId, .. ground.Rows[0].Skip(1)];
        return layout with { Layers = [ground with { Rows = [row, .. ground.Rows.Skip(1)] }, .. layout.Layers.Skip(1)] };
    }
}
