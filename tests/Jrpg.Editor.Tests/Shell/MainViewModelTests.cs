using Jrpg.Core.Geometry;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Shell;
using Jrpg.Editor.Tests.Tiles;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// Opening maps, unsaved-change questions, objects, map settings and new maps; saving and undo have their own classes.
/// </summary>
public sealed class MainViewModelTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Constructor_RepositoryContent_ListsMapsAndOpensTheFirst()
    {
        var viewModel = _content.Open(_content.RepositoryContent);

        Assert.Null(viewModel.LoadError);
        Assert.All(["forest", "interior_1", "start"], id => Assert.Contains(id, viewModel.MapIds));
        Assert.Equal(viewModel.MapIds[0], viewModel.CurrentMap?.Map.Id);
    }

    [Fact]
    public void MapEntries_RepositoryContent_ShowsNameThenIdSortedByName()
    {
        var viewModel = _content.Open(_content.RepositoryContent);

        string[] knownMaps = ["Forest (forest)", "Interior1 (interior_1)", "start (start)"];

        var texts = viewModel.MapEntries.Select(entry => entry.Text);

        Assert.Equal(knownMaps, texts.Where(knownMaps.Contains));
    }

    // interior_1 has a ground layer (z -5) and a ground detail layer (z -4); the ground is chosen at the start.
    [Fact]
    public void LayerEntries_MapWithTwoLayers_ListsThemBottomFirstWithNames()
    {
        var viewModel = _content.Open(_content.RepositoryContent);

        viewModel.TryOpenMap("interior_1");

        string[] knownLayers = ["Ground (z -5)", "Ground detail (z -4)"];
        Assert.Equal(knownLayers, viewModel.LayerEntries.Select(entry => entry.Text).Where(knownLayers.Contains));
    }

    [Fact]
    public void LayerEntries_PaintedOnEmptyZ_DropsTheEmptyMark()
    {
        var viewModel = _content.Open(_content.RepositoryContent);
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
        var viewModel = _content.Open(_content.RepositoryContent);
        Assert.DoesNotContain(viewModel.CurrentMap!.Map.Layout.Layers, layer => layer.Z == 2);

        viewModel.Layer.Z = 2;

        Assert.Contains("Layer (z 2), empty", viewModel.LayerEntries.Select(entry => entry.Text));
    }

    [Fact]
    public void TryOpenMap_NoUnsavedChanges_OpensItWithoutAsking()
    {
        var viewModel = _content.Open(_content.RepositoryContent);

        Assert.True(viewModel.TryOpenMap("start"));

        Assert.Equal("start", viewModel.CurrentMap?.Map.Id);
        Assert.Empty(_content.Questions);
    }

    [Fact]
    public void TryOpenMap_UnsavedChangesAndUserKeepsThem_StaysOnTheMap()
    {
        var viewModel = _content.Open(_content.RepositoryContent, answer: false);
        EditorTestContent.PaintTopLeft(viewModel);

        Assert.False(viewModel.TryOpenMap("start"));

        Assert.Equal("forest", viewModel.CurrentMap?.Map.Id);
        Assert.Equal(["Map 'forest' has unsaved changes. Discard them?"], _content.Questions);
    }

    [Fact]
    public void TryOpenMap_UnsavedChangesAndUserDiscards_OpensTheOther()
    {
        var viewModel = _content.Open(_content.RepositoryContent, answer: true);
        EditorTestContent.PaintTopLeft(viewModel);

        Assert.True(viewModel.TryOpenMap("start"));

        Assert.Equal("start", viewModel.CurrentMap?.Map.Id);
    }

    [Fact]
    public void ConfirmClose_UnsavedChanges_AsksTheUser()
    {
        var viewModel = _content.Open(_content.RepositoryContent, answer: false);
        EditorTestContent.PaintTopLeft(viewModel);

        Assert.False(viewModel.ConfirmClose());
        Assert.Single(_content.Questions);
    }

    [Fact]
    public void ConfirmClose_MidStroke_AsksTheUser()
    {
        var viewModel = _content.Open(_content.RepositoryContent, answer: false);
        viewModel.Painting!.StartErasing(new GridCell(0, 0));

        Assert.False(viewModel.ConfirmClose());
        Assert.Single(_content.Questions);
    }

    [Fact]
    public void ConfirmClose_UnsavedCatalogChange_AsksTheUser()
    {
        var viewModel = _content.Open(_content.RepositoryContent);
        EditorTestContent.BlockFirstWalkableTile(viewModel);

        Assert.False(viewModel.ConfirmClose());
        Assert.Equal(["The tiles have unsaved changes. Discard them?"], _content.Questions);
    }

    [Fact]
    public void ConfirmClose_UnsavedMapAndCatalogChanges_AsksOnceNamingBoth()
    {
        var viewModel = _content.Open(_content.RepositoryContent);
        EditorTestContent.BlockFirstWalkableTile(viewModel);
        EditorTestContent.PaintTopLeft(viewModel);

        Assert.False(viewModel.ConfirmClose());
        Assert.Equal([$"Map '{viewModel.CurrentMap!.Map.Id}' and the tiles have unsaved changes. Discard them?"], _content.Questions);
    }

    [Fact]
    public void TryOpenMap_UnsavedCatalogChangeOnly_OpensWithoutAsking()
    {
        var viewModel = _content.Open(_content.RepositoryContent);
        EditorTestContent.BlockFirstWalkableTile(viewModel);
        var otherMapId = viewModel.MapIds.First(id => id != viewModel.CurrentMap!.Map.Id);

        Assert.True(viewModel.TryOpenMap(otherMapId));

        Assert.Empty(_content.Questions);
        Assert.True(viewModel.HasUnsavedChanges);
    }

    [Fact]
    public void ToggleCollisionCommand_ThenAnotherMap_StaysShown()
    {
        var viewModel = _content.Open(_content.RepositoryContent);
        Assert.False(viewModel.IsCollisionShown);

        viewModel.ToggleCollisionCommand.Execute(null);
        viewModel.TryOpenMap(viewModel.MapIds.First(id => id != viewModel.CurrentMap!.Map.Id));

        Assert.True(viewModel.IsCollisionShown);
    }

    [Fact]
    public void ImportSheetImages_NewImage_ShowsItAndReportsImported()
    {
        var viewModel = _content.Open(_content.CopyOfRepositoryContent());
        var source = PngHeaderFile.Write(Path.Combine(_content.ContentCopy, "import-source", "Cave.png"), 96, 48);

        viewModel.ImportSheetImages([source]);

        Assert.Equal(new SheetChoice("Cave.png", null), viewModel.Painting!.Browser.SelectedChoice);
        Assert.Equal("Imported Cave.png.", viewModel.ImportStatus);
    }

    // Grass3.png has no sheet, so it can be replaced by any size; Set up's guess reads the size: 96 x 48 is 2 x 1 cells of 48.
    [Fact]
    public void ImportSheetImages_ReplacedImage_ReadsTheNewSize()
    {
        var viewModel = _content.Open(_content.CopyOfRepositoryContent(), answer: true);
        var browser = viewModel.Painting!.Browser;
        browser.SelectedChoice = browser.SheetChoices.First(choice => choice.File == "Grass3.png");
        browser.SetUpSheetCommand.Execute(null);
        browser.CancelCutCommand.Execute(null);
        var source = PngHeaderFile.Write(Path.Combine(_content.ContentCopy, "import-source", "Grass3.png"), 96, 48);

        viewModel.ImportSheetImages([source]);
        browser.SetUpSheetCommand.Execute(null);

        Assert.Equal((2, 1), (browser.Cut!.Columns, browser.Cut.Rows));
    }

    [Fact]
    public void CreateObject_CellsNotTilesYet_AddsTilesAndTheObjectThenChoosesIt()
    {
        var viewModel = _content.Open(_content.CopyOfRepositoryContent());
        var cells = _content.ChooseTwoCellsNotTiles(viewModel);
        var newObject = viewModel.StartNewObject()!;
        newObject.Id = "tree_4";
        newObject.Category = "General";

        Assert.True(viewModel.CreateObject(newObject));
        viewModel.SaveCommand.Execute(null);

        Assert.Null(viewModel.SaveError);
        Assert.Equal(EditingMode.Objects, viewModel.Mode);
        Assert.Equal("tree_4", viewModel.Placing!.Browser.SelectedObject?.Id);
        var saved = ContentCatalogLoader.Load(_content.ContentCopy);
        var tree = saved.Objects["tree_4"];
        Assert.Equal(("General", 2, 1), (tree.Category, tree.Width, tree.Height));
        Assert.Equal(cells.Cells, tree.Tiles.Select(tile => saved.Tiles[tile.TileId] is var cut ? new SheetCell(cut.SheetId, cut.Column, cut.Row) : default));
    }

    [Fact]
    public void CreateNpc_NewId_AddsItAndChoosesIt()
    {
        var viewModel = _content.Open(_content.CopyOfRepositoryContent());
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
    public void CanStartNewObject_RectangleChosen_IsTrue()
    {
        var viewModel = _content.Open(_content.CopyOfRepositoryContent());
        Assert.False(viewModel.CanStartNewObject);
        var changed = new List<string?>();
        viewModel.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        _content.ChooseTwoCellsNotTiles(viewModel);

        Assert.True(viewModel.CanStartNewObject);
        Assert.Contains(nameof(MainViewModel.CanStartNewObject), changed);
    }

    [Fact]
    public void CreateObject_IdTaken_ReturnsFalseAndKeepsTheObject()
    {
        var viewModel = _content.Open(_content.CopyOfRepositoryContent());
        var tree = viewModel.Catalog!.Objects["tree"];
        _content.ChooseTwoCellsNotTiles(viewModel);
        var newObject = viewModel.StartNewObject()!;
        newObject.Id = "tree";
        newObject.Category = "General";

        Assert.False(viewModel.CreateObject(newObject));

        Assert.Same(tree, viewModel.Catalog.Objects["tree"]);
        Assert.False(viewModel.Content!.IsDirty);
    }

    [Fact]
    public void ShowCursorAt_CellOnTheMap_NamesTheCellAndItsTopTile()
    {
        var viewModel = _content.Open(_content.RepositoryContent);
        var tile = viewModel.CurrentMap!.TopTileAt(new GridCell(3, 2))!.Value;

        viewModel.ShowCursorAt(new GridCell(3, 2));

        Assert.Equal($"Cell (3, 2)   Tile {tile.TileId} on layer z {tile.LayerZ}", viewModel.CursorStatus);
    }

    [Fact]
    public void ShowCursorAt_CellWithoutTiles_SaysSo()
    {
        // The start map's one hole (seen in slice 3a).
        var viewModel = _content.Open(_content.RepositoryContent);
        viewModel.TryOpenMap("start");

        viewModel.ShowCursorAt(new GridCell(5, 13));

        Assert.Equal("Cell (5, 13)   No tile", viewModel.CursorStatus);
    }

    [Fact]
    public void ShowCursorAt_CellOffTheMap_ClearsTheStatus()
    {
        var viewModel = _content.Open(_content.RepositoryContent);
        viewModel.ShowCursorAt(new GridCell(3, 2));

        viewModel.ShowCursorAt(new GridCell(-1, 2));

        Assert.Equal("", viewModel.CursorStatus);
    }

    [Fact]
    public void TryOpenMap_AnotherMap_HandsItToPaintingAndPlacing()
    {
        var viewModel = _content.Open(_content.RepositoryContent);

        viewModel.TryOpenMap("start");

        Assert.Same(viewModel.CurrentMap, viewModel.Painting!.OpenMap);
        Assert.Same(viewModel.CurrentMap, viewModel.Placing!.OpenMap);
    }

    [Fact]
    public void TryApplySettings_ShrinkRefused_KeepsTheMapAsItIs()
    {
        var viewModel = _content.Open(_content.RepositoryContent, answer: false);
        var settings = viewModel.StartEditingSettings()!;
        settings.WidthText = "1";
        settings.DisplayName = "Small forest";

        Assert.False(viewModel.TryApplySettings(settings));

        Assert.StartsWith("Resizing to 1 x ", Assert.Single(_content.Questions), StringComparison.Ordinal);
        Assert.False(viewModel.CurrentMap!.IsDirty);
    }

    [Fact]
    public void TryApplySettings_ShrinkAccepted_ResizesAndRenamesAsTwoUndoSteps()
    {
        var viewModel = _content.Open(_content.RepositoryContent, answer: true);
        var settings = viewModel.StartEditingSettings()!;
        settings.WidthText = "1";
        settings.DisplayName = "Small forest";

        Assert.True(viewModel.TryApplySettings(settings));

        var map = viewModel.CurrentMap!;
        Assert.Equal((1, "Small forest"), (map.Map.Layout.Width, map.Map.Settings.DisplayName));
        map.UndoCommand.Execute(null);
        Assert.Equal((1, "Forest"), (map.Map.Layout.Width, map.Map.Settings.DisplayName));
        map.UndoCommand.Execute(null);
        Assert.False(map.IsDirty);
    }

    [Fact]
    public void TryApplySettings_Grow_ResizesWithoutAsking()
    {
        var viewModel = _content.Open(_content.RepositoryContent);
        var settings = viewModel.StartEditingSettings()!;
        settings.HeightText = "100";

        Assert.True(viewModel.TryApplySettings(settings));

        Assert.Equal(100, viewModel.CurrentMap!.Map.Layout.Height);
        Assert.Empty(_content.Questions);
    }

    [Fact]
    public void TryApplySettings_NothingChanged_AddsNoUndoStep()
    {
        var viewModel = _content.Open(_content.RepositoryContent);

        Assert.True(viewModel.TryApplySettings(viewModel.StartEditingSettings()!));

        Assert.False(viewModel.CurrentMap!.UndoCommand.CanExecute(null));
    }

    [Fact]
    public void TryApplySettings_EmptySizeBox_AppliesNothing()
    {
        var viewModel = _content.Open(_content.RepositoryContent);
        var settings = viewModel.StartEditingSettings()!;
        settings.HeightText = "";

        Assert.False(viewModel.TryApplySettings(settings));
        Assert.False(viewModel.CurrentMap!.IsDirty);
    }

    [Fact]
    public void TryCreateMap_ValidFields_OpensItUnsavedOutsideTheList()
    {
        var viewModel = _content.Open(_content.RepositoryContent);
        var newMap = viewModel.StartNewMap()!;
        newMap.Name = "Old Mill";

        Assert.True(viewModel.TryCreateMap(newMap));

        Assert.Equal("old_mill", viewModel.CurrentMap!.Map.Id);
        Assert.True(viewModel.CurrentMap.IsDirty);
        Assert.DoesNotContain("old_mill", viewModel.MapIds);
    }

    [Fact]
    public void TryCreateMap_UnsavedChangesKept_StaysOnTheOpenMap()
    {
        var viewModel = _content.Open(_content.RepositoryContent, answer: false);
        EditorTestContent.PaintTopLeft(viewModel);

        Assert.False(viewModel.TryCreateMap(viewModel.StartNewMap()!));

        Assert.Equal("forest", viewModel.CurrentMap!.Map.Id);
    }

    [Fact]
    public void TryCreateMap_InvalidSize_OpensNothing()
    {
        var viewModel = _content.Open(_content.RepositoryContent);
        var newMap = viewModel.StartNewMap()!;
        newMap.WidthText = "0";

        Assert.False(viewModel.TryCreateMap(newMap));

        Assert.Equal("forest", viewModel.CurrentMap!.Map.Id);
    }

    [Fact]
    public void TryCreateMap_BlankName_OpensNothing()
    {
        var viewModel = _content.Open(_content.RepositoryContent);
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
