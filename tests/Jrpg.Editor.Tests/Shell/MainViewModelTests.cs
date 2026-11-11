using Jrpg.Core.Geometry;
using Jrpg.Data.Content;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// The map list, layers, objects and new maps; map settings, the sheet import, switching maps, saving, closing and undo
/// are tested in their own classes.
/// </summary>
public sealed class MainViewModelTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Constructor_TestWorld_ListsMapsAndOpensTheFirst()
    {
        var viewModel = _content.Open(_content.TestWorld);

        Assert.Null(viewModel.LoadError);
        Assert.All(["forest", "interior_1", "start"], id => Assert.Contains(id, viewModel.MapIds));
        Assert.Equal(viewModel.MapIds[0], viewModel.CurrentMap?.Map.Id);
    }

    [Fact]
    public void MapEntries_TestWorld_ShowsNameThenIdSortedByName()
    {
        var viewModel = _content.Open(_content.TestWorld);

        string[] knownMaps = ["Forest (forest)", "Interior1 (interior_1)", "start (start)"];

        var texts = viewModel.MapEntries.Select(entry => entry.Text);

        Assert.Equal(knownMaps, texts.Where(knownMaps.Contains));
    }

    // interior_1 has a ground layer (z -5) and a ground detail layer (z -4); the ground is chosen at the start.
    [Fact]
    public void LayerEntries_MapWithTwoLayers_ListsThemBottomFirstWithNames()
    {
        var viewModel = _content.Open(_content.TestWorld);

        viewModel.TryOpenMap("interior_1");

        string[] knownLayers = ["Ground (z -5)", "Ground detail (z -4)"];
        Assert.Equal(knownLayers, viewModel.LayerEntries.Select(entry => entry.Text).Where(knownLayers.Contains));
    }

    [Fact]
    public void LayerEntries_PaintedOnEmptyZ_DropsTheEmptyMark()
    {
        var viewModel = _content.Open(_content.TestWorld);
        Assert.DoesNotContain(viewModel.CurrentMap!.Map.Layout.Layers, layer => layer.Z == 2);
        viewModel.Layer.Z = 2;
        var tileId = viewModel.CurrentMap.Map.Layout.Layers[0].TileIdAt(new GridCell(0, 0));

        viewModel.CurrentMap.Do(new SetTilesEdit(2, [new TileChange(new GridCell(0, 0), 0, tileId)]));

        var entries = viewModel.LayerEntries.Select(entry => entry.Text).ToList();
        Assert.Contains("Layer (z 2)", entries);
        Assert.DoesNotContain("Layer (z 2), empty", entries);
    }

    [Fact]
    public void LayerEntries_ChosenZWithoutLayer_ListsItAsEmpty()
    {
        var viewModel = _content.Open(_content.TestWorld);
        Assert.DoesNotContain(viewModel.CurrentMap!.Map.Layout.Layers, layer => layer.Z == 2);

        viewModel.Layer.Z = 2;

        Assert.Contains("Layer (z 2), empty", viewModel.LayerEntries.Select(entry => entry.Text));
    }

    [Fact]
    public void ToggleCollisionCommand_ThenAnotherMap_StaysShown()
    {
        var viewModel = _content.Open(_content.TestWorld);
        Assert.False(viewModel.IsCollisionShown);

        viewModel.ToggleCollisionCommand.Execute(null);
        viewModel.TryOpenMap(viewModel.MapIds.First(id => id != viewModel.CurrentMap!.Map.Id));

        Assert.True(viewModel.IsCollisionShown);
    }

    [Fact]
    public void CreateNpc_NewId_AddsItAndChoosesIt()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var newNpc = viewModel.StartNewNpc()!;
        newNpc.Id = "guard";

        Assert.True(viewModel.CreateNpc(newNpc));
        viewModel.SaveCommand.Execute(null);

        Assert.Null(viewModel.SaveError);
        Assert.Equal(EditingMode.Objects, viewModel.Mode);
        Assert.Equal("guard", viewModel.Placing!.Browser.SelectedObject?.Id);
        var guard = ContentCatalogLoader.Load(_content.ContentCopy).Objects["guard"];
        Assert.Equal(("NPC", "npc_placeholder.png"), (guard.Category, guard.FieldSpriteFile));
    }

    [Fact]
    public void CanStartNewObject_ContentLoaded_IsTrue() => Assert.True(_content.Open(_content.TestWorld).CanStartNewObject);

    [Fact]
    public void ShowCursorAt_CellOnTheMap_NamesTheCellAndItsTopTile()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var tile = viewModel.CurrentMap!.TopTileAt(new GridCell(3, 2))!.Value;

        viewModel.ShowCursorAt(new GridCell(3, 2));

        Assert.Equal($"Cell (3, 2)   Tile {tile.TileId} on layer z {tile.LayerZ}", viewModel.CursorStatus);
    }

    [Fact]
    public void ShowCursorAt_CellWithoutTiles_SaysSo()
    {
        // The start map's one hole (seen in slice 3a).
        var viewModel = _content.Open(_content.TestWorld);
        viewModel.TryOpenMap("start");

        viewModel.ShowCursorAt(new GridCell(5, 13));

        Assert.Equal("Cell (5, 13)   No tile", viewModel.CursorStatus);
    }

    [Fact]
    public void ShowCursorAt_CellOffTheMap_ClearsTheStatus()
    {
        var viewModel = _content.Open(_content.TestWorld);
        viewModel.ShowCursorAt(new GridCell(3, 2));

        viewModel.ShowCursorAt(new GridCell(-1, 2));

        Assert.Equal("", viewModel.CursorStatus);
    }

    [Fact]
    public void TryOpenMap_AnotherMap_HandsItToPaintingAndPlacing()
    {
        var viewModel = _content.Open(_content.TestWorld);

        viewModel.TryOpenMap("start");

        Assert.Same(viewModel.CurrentMap, viewModel.Painting!.OpenMap);
        Assert.Same(viewModel.CurrentMap, viewModel.Placing!.OpenMap);
    }

    [Fact]
    public void TryCreateMap_InvalidSize_OpensNothing()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var newMap = viewModel.StartNewMap()!;
        newMap.WidthText = "0";

        Assert.False(viewModel.TryCreateMap(newMap));

        Assert.Equal("forest", viewModel.CurrentMap!.Map.Id);
    }

    [Fact]
    public void TryCreateMap_BlankName_OpensNothing()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var newMap = viewModel.StartNewMap()!;
        newMap.Name = "";

        Assert.False(viewModel.TryCreateMap(newMap));

        Assert.Equal("forest", viewModel.CurrentMap!.Map.Id);
    }

    [Fact]
    public void Constructor_BrokenContentFile_ShowsTheLoaderMessageAndNoMaps()
    {
        Directory.CreateDirectory(_content.ContentCopy);
        // The loader reads skills.json first, so a copy holding only that file stops there.
        File.WriteAllText(Path.Combine(_content.ContentCopy, "skills.json"), "[ {");

        var viewModel = _content.Open(_content.ContentCopy);

        Assert.StartsWith("skills.json", viewModel.LoadError, StringComparison.Ordinal);
        Assert.Empty(viewModel.MapIds);
        Assert.Null(viewModel.CurrentMap);
        Assert.Null(viewModel.Painting);
        Assert.Null(viewModel.Placing);
    }
}
