using Jrpg.Core.Geometry;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Shell;
using Jrpg.Editor.Tests.Tiles;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Shell;

public sealed class MainViewModelTests : IDisposable
{
    private static readonly string _repositoryContent = Path.Combine(AppContext.BaseDirectory, "content");

    private readonly string _contentCopy = Path.Combine(Path.GetTempPath(), $"jrpg-editor-{Guid.NewGuid():N}");
    private readonly List<string> _questions = [];

    public void Dispose()
    {
        if (Directory.Exists(_contentCopy))
        {
            Directory.Delete(_contentCopy, recursive: true);
        }
    }

    [Fact]
    public void Constructor_RepositoryContent_ListsMapsAndOpensTheFirst()
    {
        var viewModel = Open(_repositoryContent);

        Assert.Null(viewModel.LoadError);
        Assert.Equal(["forest", "interior_1", "start"], viewModel.MapIds);
        Assert.Equal("forest", viewModel.CurrentMap?.Map.Id);
    }

    [Fact]
    public void MapEntries_RepositoryContent_ShowsNameThenIdSortedByName()
    {
        var viewModel = Open(_repositoryContent);

        var texts = viewModel.MapEntries.Select(entry => entry.Text);

        Assert.Equal(["Forest (forest)", "Interior1 (interior_1)", "start (start)"], texts);
    }

    // interior_1 has a ground layer (z -5) and a ground detail layer (z -4); the ground is chosen at the start.
    [Fact]
    public void LayerEntries_MapWithTwoLayers_ListsThemBottomFirstWithNames()
    {
        var viewModel = Open(_repositoryContent);

        viewModel.TryOpenMap("interior_1");

        Assert.Equal(["Ground (z -5)", "Ground detail (z -4)"], viewModel.LayerEntries.Select(entry => entry.Text));
    }

    [Fact]
    public void LayerEntries_PaintedOnEmptyZ_DropsTheEmptyMark()
    {
        var viewModel = Open(_repositoryContent);
        viewModel.Layer.Z = 2;
        var tileId = viewModel.CurrentMap!.Map.Layout.Layers[0].TileIdAt(new GridCell(0, 0));

        viewModel.CurrentMap.Do(new SetTilesEdit(2, [new TileChange(new GridCell(0, 0), 0, tileId)]));

        Assert.Equal(["Ground (z -5)", "Layer (z 2)"], viewModel.LayerEntries.Select(entry => entry.Text));
    }

    [Fact]
    public void LayerEntries_ChosenZWithoutLayer_ListsItAsEmpty()
    {
        var viewModel = Open(_repositoryContent);

        viewModel.Layer.Z = 2;

        Assert.Equal(["Ground (z -5)", "Layer (z 2), empty"], viewModel.LayerEntries.Select(entry => entry.Text));
    }

    [Fact]
    public void TryOpenMap_NoUnsavedChanges_OpensItWithoutAsking()
    {
        var viewModel = Open(_repositoryContent);

        Assert.True(viewModel.TryOpenMap("start"));

        Assert.Equal("start", viewModel.CurrentMap?.Map.Id);
        Assert.Empty(_questions);
    }

    [Fact]
    public void TryOpenMap_UnsavedChangesAndUserKeepsThem_StaysOnTheMap()
    {
        var viewModel = Open(_repositoryContent, answer: false);
        PaintTopLeft(viewModel);

        Assert.False(viewModel.TryOpenMap("start"));

        Assert.Equal("forest", viewModel.CurrentMap?.Map.Id);
        Assert.Equal(["Map 'forest' has unsaved changes. Discard them?"], _questions);
    }

    [Fact]
    public void TryOpenMap_UnsavedChangesAndUserDiscards_OpensTheOther()
    {
        var viewModel = Open(_repositoryContent, answer: true);
        PaintTopLeft(viewModel);

        Assert.True(viewModel.TryOpenMap("start"));

        Assert.Equal("start", viewModel.CurrentMap?.Map.Id);
    }

    [Fact]
    public void ConfirmClose_UnsavedChanges_AsksTheUser()
    {
        var viewModel = Open(_repositoryContent, answer: false);
        PaintTopLeft(viewModel);

        Assert.False(viewModel.ConfirmClose());
        Assert.Single(_questions);
    }

    [Fact]
    public void ConfirmClose_MidStroke_AsksTheUser()
    {
        var viewModel = Open(_repositoryContent, answer: false);
        viewModel.Painting!.StartErasing(new GridCell(0, 0));

        Assert.False(viewModel.ConfirmClose());
        Assert.Single(_questions);
    }

    [Fact]
    public void Save_ValidEdit_WritesTheMapAndClearsUnsavedChanges()
    {
        var viewModel = Open(CopyOfRepositoryContent());
        var painted = PaintTopLeft(viewModel);

        viewModel.SaveCommand.Execute(null);

        Assert.False(viewModel.CurrentMap!.IsDirty);
        Assert.Null(viewModel.SaveError);
        Assert.Equal(painted, ContentCatalogLoader.Load(_contentCopy).Maps["forest"].Layout.Layers[0].TileIdAt(new GridCell(0, 0)));
    }

    [Fact]
    public void Save_MapTheGameWouldReject_ShowsTheLoaderMessageAndStaysUnsaved()
    {
        var viewModel = Open(CopyOfRepositoryContent());
        var forest = viewModel.CurrentMap!;
        forest.Do(new SetTilesEdit(forest.Map.Layout.Layers[0].Z, [new TileChange(new GridCell(0, 0), forest.Map.Layout.Layers[0].TileIdAt(new GridCell(0, 0)), 99_999)]));

        viewModel.SaveCommand.Execute(null);

        Assert.StartsWith("map-layouts/forest.json: ", viewModel.SaveError, StringComparison.Ordinal);
        Assert.True(forest.IsDirty);
    }

    [Fact]
    public void ConfirmClose_UnsavedCatalogChange_AsksTheUser()
    {
        var viewModel = Open(_repositoryContent);
        BlockFirstWalkableTile(viewModel);

        Assert.False(viewModel.ConfirmClose());
        Assert.Equal(["The tiles have unsaved changes. Discard them?"], _questions);
    }

    [Fact]
    public void ConfirmClose_UnsavedMapAndCatalogChanges_AsksOnceNamingBoth()
    {
        var viewModel = Open(_repositoryContent);
        BlockFirstWalkableTile(viewModel);
        PaintTopLeft(viewModel);

        Assert.False(viewModel.ConfirmClose());
        Assert.Equal([$"Map '{viewModel.CurrentMap!.Map.Id}' and the tiles have unsaved changes. Discard them?"], _questions);
    }

    [Fact]
    public void TryOpenMap_UnsavedCatalogChangeOnly_OpensWithoutAsking()
    {
        var viewModel = Open(_repositoryContent);
        BlockFirstWalkableTile(viewModel);
        var otherMapId = viewModel.MapIds.First(id => id != viewModel.CurrentMap!.Map.Id);

        Assert.True(viewModel.TryOpenMap(otherMapId));

        Assert.Empty(_questions);
        Assert.True(viewModel.HasUnsavedChanges);
    }

    [Fact]
    public void Undo_BlockedTileMarkThenPaint_UndoesThePaintFirst()
    {
        var viewModel = Open(_repositoryContent);
        var blockedId = BlockFirstWalkableTile(viewModel);
        PaintTopLeft(viewModel);

        viewModel.UndoCommand.Execute(null);

        Assert.False(viewModel.CurrentMap!.IsDirty);
        Assert.False(viewModel.Catalog!.Tiles[blockedId].IsWalkable);
        viewModel.UndoCommand.Execute(null);
        Assert.True(viewModel.Catalog!.Tiles[blockedId].IsWalkable);
        Assert.False(viewModel.HasUnsavedChanges);
    }

    [Fact]
    public void Redo_AfterUndoingMarkAndPaint_RedoesTheMarkFirst()
    {
        var viewModel = Open(_repositoryContent);
        var blockedId = BlockFirstWalkableTile(viewModel);
        PaintTopLeft(viewModel);
        viewModel.UndoCommand.Execute(null);
        viewModel.UndoCommand.Execute(null);

        viewModel.RedoCommand.Execute(null);

        Assert.False(viewModel.Catalog!.Tiles[blockedId].IsWalkable);
        Assert.False(viewModel.CurrentMap!.IsDirty);
        viewModel.RedoCommand.Execute(null);
        Assert.True(viewModel.CurrentMap.IsDirty);
    }

    [Fact]
    public void Redo_NewTilePaintedOffTheMapAfterUndo_HasNothingToRedo()
    {
        var viewModel = Open(CopyOfRepositoryContent());
        viewModel.Content!.AddBlockedTile(new SheetCell(viewModel.Catalog!.Tilesheets.Keys.First(), 999, 999));
        viewModel.UndoCommand.Execute(null);
        ChooseCellNotATile(viewModel);

        viewModel.Painting!.PaintRectangle(new GridCell(-5, -5), new GridCell(-4, -4));

        Assert.False(viewModel.RedoCommand.CanExecute(null));
    }

    [Fact]
    public void Undo_NewBlockedTileUsedOnAnotherMap_RefusesAndNamesTheMap()
    {
        var viewModel = Open(CopyOfRepositoryContent());
        var cell = ChooseCellNotATile(viewModel);
        viewModel.Content!.AddBlockedTile(cell);
        var tileId = viewModel.Content.TileAt(cell)!.Id;
        viewModel.Painting!.StartPainting(new GridCell(0, 0));
        viewModel.Painting.EndStroke();
        var paintedMapId = viewModel.CurrentMap!.Map.Id;
        viewModel.SaveCommand.Execute(null);
        viewModel.TryOpenMap(viewModel.MapIds.First(id => id != paintedMapId));

        viewModel.UndoCommand.Execute(null);

        Assert.Equal($"Can't undo: tile {tileId} is used on map {paintedMapId}.", viewModel.UndoRefusal);
        Assert.NotNull(viewModel.Content.TileAt(cell));
        Assert.False(viewModel.RedoCommand.CanExecute(null));
    }

    [Fact]
    public void Undo_SetUpSheetWithTilesCut_RefusesNamingTheTile()
    {
        var viewModel = Open(_repositoryContent);
        var browser = viewModel.Painting!.Browser;
        browser.SelectedChoice = browser.SheetChoices.First(choice => choice.File == "Grass3.png");
        browser.SetUpSheetCommand.Execute(null);
        browser.ApplyCutCommand.Execute(null);
        browser.SelectCellAt(1, 1);
        var tileId = viewModel.Content!.AddTile(browser.SelectedCell!.Value);

        viewModel.UndoCommand.Execute(null);

        Assert.Equal($"Can't undo: tile {tileId} is cut from sheet grass_3.", viewModel.UndoRefusal);
        Assert.True(viewModel.Catalog!.Tilesheets.ContainsKey("grass_3"));
    }

    [Fact]
    public void ToggleCollisionCommand_ThenAnotherMap_StaysShown()
    {
        var viewModel = Open(_repositoryContent);
        Assert.False(viewModel.IsCollisionShown);

        viewModel.ToggleCollisionCommand.Execute(null);
        viewModel.TryOpenMap(viewModel.MapIds.First(id => id != viewModel.CurrentMap!.Map.Id));

        Assert.True(viewModel.IsCollisionShown);
    }

    [Fact]
    public void ImportSheetImages_NewImage_ShowsItAndReportsImported()
    {
        var viewModel = Open(CopyOfRepositoryContent());
        var source = PngHeaderFile.Write(Path.Combine(_contentCopy, "import-source", "Cave.png"), 96, 48);

        viewModel.ImportSheetImages([source]);

        Assert.Equal(new SheetChoice("Cave.png", null), viewModel.Painting!.Browser.SelectedChoice);
        Assert.Equal("Imported Cave.png.", viewModel.ImportStatus);
    }

    // Grass3.png has no sheet, so it can be replaced by any size; Set up's guess reads the size: 96 x 48 is 2 x 1 cells of 48.
    [Fact]
    public void ImportSheetImages_ReplacedImage_ReadsTheNewSize()
    {
        var viewModel = Open(CopyOfRepositoryContent(), answer: true);
        var browser = viewModel.Painting!.Browser;
        browser.SelectedChoice = browser.SheetChoices.First(choice => choice.File == "Grass3.png");
        browser.SetUpSheetCommand.Execute(null);
        browser.CancelCutCommand.Execute(null);
        var source = PngHeaderFile.Write(Path.Combine(_contentCopy, "import-source", "Grass3.png"), 96, 48);

        viewModel.ImportSheetImages([source]);
        browser.SetUpSheetCommand.Execute(null);

        Assert.Equal((2, 1), (browser.Cut!.Columns, browser.Cut.Rows));
    }

    [Fact]
    public void Undo_RefusedThenAnEdit_ClearsTheRefusal()
    {
        var viewModel = Open(CopyOfRepositoryContent());
        var cell = ChooseCellNotATile(viewModel);
        viewModel.Content!.AddBlockedTile(cell);
        viewModel.Painting!.StartPainting(new GridCell(0, 0));
        viewModel.Painting.EndStroke();
        viewModel.SaveCommand.Execute(null);
        viewModel.TryOpenMap(viewModel.MapIds.First(id => id != viewModel.CurrentMap!.Map.Id));
        viewModel.UndoCommand.Execute(null);

        BlockFirstWalkableTile(viewModel);

        Assert.Null(viewModel.UndoRefusal);
    }

    [Fact]
    public void Undo_CatalogStepAfterSwitchingMaps_UndoesIt()
    {
        var viewModel = Open(_repositoryContent);
        var blockedId = BlockFirstWalkableTile(viewModel);
        viewModel.TryOpenMap(viewModel.MapIds.First(id => id != viewModel.CurrentMap!.Map.Id));

        viewModel.UndoCommand.Execute(null);

        Assert.True(viewModel.Catalog!.Tiles[blockedId].IsWalkable);
    }

    [Fact]
    public void CreateObject_CellsNotTilesYet_AddsTilesAndTheObjectThenChoosesIt()
    {
        var viewModel = Open(CopyOfRepositoryContent());
        var cells = ChooseTwoCellsNotTiles(viewModel);
        var newObject = viewModel.StartNewObject()!;
        newObject.Id = "tree_4";
        newObject.Category = "General";

        Assert.True(viewModel.CreateObject(newObject));
        viewModel.SaveCommand.Execute(null);

        Assert.Null(viewModel.SaveError);
        Assert.Equal(EditingMode.Objects, viewModel.Mode);
        Assert.Equal("tree_4", viewModel.Placing!.Browser.SelectedObject?.Id);
        var saved = ContentCatalogLoader.Load(_contentCopy);
        var tree = saved.Objects["tree_4"];
        Assert.Equal(("General", 2, 1), (tree.Category, tree.Width, tree.Height));
        Assert.Equal(cells.Cells, tree.Tiles.Select(tile => saved.Tiles[tile.TileId] is var cut ? new SheetCell(cut.SheetId, cut.Column, cut.Row) : default));
    }

    [Fact]
    public void CreateNpc_NewId_AddsItAndChoosesIt()
    {
        var viewModel = Open(CopyOfRepositoryContent());
        var newNpc = viewModel.StartNewNpc()!;
        newNpc.Id = "guard";

        Assert.True(viewModel.CreateNpc(newNpc));
        viewModel.SaveCommand.Execute(null);

        Assert.Null(viewModel.SaveError);
        Assert.Equal(EditingMode.Objects, viewModel.Mode);
        Assert.Equal("guard", viewModel.Placing!.Browser.SelectedObject?.Id);
        var guard = ContentCatalogLoader.Load(_contentCopy).Objects["guard"];
        Assert.Equal(("NPC", "npc_placeholder.png"), (guard.Category, guard.FieldSpriteFile));
    }

    [Fact]
    public void CanStartNewObject_RectangleChosen_IsTrue()
    {
        var viewModel = Open(CopyOfRepositoryContent());
        Assert.False(viewModel.CanStartNewObject);
        var changed = new List<string?>();
        viewModel.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        ChooseTwoCellsNotTiles(viewModel);

        Assert.True(viewModel.CanStartNewObject);
        Assert.Contains(nameof(MainViewModel.CanStartNewObject), changed);
    }

    [Fact]
    public void CreateObject_IdTaken_ReturnsFalseAndKeepsTheObject()
    {
        var viewModel = Open(CopyOfRepositoryContent());
        var tree = viewModel.Catalog!.Objects["tree"];
        ChooseTwoCellsNotTiles(viewModel);
        var newObject = viewModel.StartNewObject()!;
        newObject.Id = "tree";
        newObject.Category = "General";

        Assert.False(viewModel.CreateObject(newObject));

        Assert.Same(tree, viewModel.Catalog.Objects["tree"]);
        Assert.False(viewModel.Content!.IsDirty);
    }

    [Fact]
    public void Undo_NewObjectPlacedOnASavedMap_RefusesAndNamesTheMap()
    {
        var viewModel = Open(CopyOfRepositoryContent());
        ChooseTwoCellsNotTiles(viewModel);
        var newObject = viewModel.StartNewObject()!;
        newObject.Id = "tree_4";
        newObject.Category = "General";
        viewModel.CreateObject(newObject);
        viewModel.Placing!.PlaceObjectAt(new Point2D(2, 2));
        var placedOn = viewModel.CurrentMap!.Map.Id;
        viewModel.SaveCommand.Execute(null);
        viewModel.TryOpenMap(viewModel.MapIds.First(id => id != placedOn));

        viewModel.UndoCommand.Execute(null);

        Assert.Equal($"Can't undo: object tree_4 is placed on map {placedOn}.", viewModel.UndoRefusal);
        Assert.True(viewModel.Catalog!.Objects.ContainsKey("tree_4"));
    }

    [Fact]
    public void Save_ObjectOutlineChanged_LoadsBackWithTheNewOutline()
    {
        var viewModel = Open(CopyOfRepositoryContent());
        var placing = viewModel.Placing!;
        placing.Browser.SelectedObject = viewModel.Catalog!.Objects["chest"];
        var shapes = placing.StartEditingShapes()!;
        shapes.ClearShapeCommand.Execute(null);
        placing.ApplyShapes(shapes);

        viewModel.SaveCommand.Execute(null);

        Assert.Null(viewModel.SaveError);
        Assert.Empty(ContentCatalogLoader.Load(_contentCopy).Objects["chest"].CollisionPolygon);
    }

    [Fact]
    public void Save_CatalogChangedMapUnchanged_WritesTilesAndClearsUnsavedChanges()
    {
        var viewModel = Open(CopyOfRepositoryContent());
        var blockedId = BlockFirstWalkableTile(viewModel);

        viewModel.SaveCommand.Execute(null);

        Assert.Null(viewModel.SaveError);
        Assert.False(viewModel.HasUnsavedChanges);
        Assert.False(ContentCatalogLoader.Load(_contentCopy).Tiles[blockedId].IsWalkable);
    }

    [Fact]
    public void Save_MapPaintedWithNewTile_WritesTileAndMapThatLoadBack()
    {
        var viewModel = Open(CopyOfRepositoryContent());
        var cell = ChooseCellNotATile(viewModel);
        viewModel.Painting!.StartPainting(new GridCell(0, 0));
        viewModel.Painting.EndStroke();

        viewModel.SaveCommand.Execute(null);

        Assert.Null(viewModel.SaveError);
        var loaded = ContentCatalogLoader.Load(_contentCopy);
        var tile = loaded.Tiles.Values.Single(tile => new SheetCell(tile.SheetId, tile.Column, tile.Row) == cell);
        var layer = loaded.Maps["forest"].Layout.Layers.Single(layer => layer.Z == viewModel.Layer.Z);
        Assert.Equal(tile.Id, layer.TileIdAt(new GridCell(0, 0)));
    }

    [Fact]
    public void Save_CatalogTheArtCheckRefuses_ShowsMessageAndWritesNothing()
    {
        var viewModel = Open(CopyOfRepositoryContent());
        var filesBefore = TilesAndForestBytes();
        viewModel.Content!.AddTile(new SheetCell(viewModel.Catalog!.Tilesheets.Keys.First(), 9999, 0));
        PaintTopLeft(viewModel);

        viewModel.SaveCommand.Execute(null);

        Assert.Contains("lies outside", viewModel.SaveError, StringComparison.Ordinal);
        Assert.Equal(filesBefore, TilesAndForestBytes());
        Assert.True(viewModel.CurrentMap!.IsDirty);
    }

    [Fact]
    public void ShowCursorAt_CellOnTheMap_NamesTheCellAndItsTopTile()
    {
        var viewModel = Open(_repositoryContent);
        var tile = viewModel.CurrentMap!.TopTileAt(new GridCell(3, 2))!.Value;

        viewModel.ShowCursorAt(new GridCell(3, 2));

        Assert.Equal($"Cell (3, 2)   Tile {tile.TileId} on layer z {tile.LayerZ}", viewModel.CursorStatus);
    }

    [Fact]
    public void ShowCursorAt_CellWithoutTiles_SaysSo()
    {
        // The start map's one hole, left by the legacy editor (seen in slice 3a).
        var viewModel = Open(_repositoryContent);
        viewModel.TryOpenMap("start");

        viewModel.ShowCursorAt(new GridCell(5, 13));

        Assert.Equal("Cell (5, 13)   No tile", viewModel.CursorStatus);
    }

    [Fact]
    public void ShowCursorAt_CellOffTheMap_ClearsTheStatus()
    {
        var viewModel = Open(_repositoryContent);
        viewModel.ShowCursorAt(new GridCell(3, 2));

        viewModel.ShowCursorAt(new GridCell(-1, 2));

        Assert.Equal("", viewModel.CursorStatus);
    }

    [Fact]
    public void TryOpenMap_AnotherMap_HandsItToPaintingAndPlacing()
    {
        var viewModel = Open(_repositoryContent);

        viewModel.TryOpenMap("start");

        Assert.Same(viewModel.CurrentMap, viewModel.Painting!.OpenMap);
        Assert.Same(viewModel.CurrentMap, viewModel.Placing!.OpenMap);
    }

    [Fact]
    public void TryApplySettings_ShrinkRefused_KeepsTheMapAsItIs()
    {
        var viewModel = Open(_repositoryContent, answer: false);
        var settings = viewModel.StartEditingSettings()!;
        settings.WidthText = "1";
        settings.DisplayName = "Small forest";

        Assert.False(viewModel.TryApplySettings(settings));

        Assert.StartsWith("Resizing to 1 x ", Assert.Single(_questions), StringComparison.Ordinal);
        Assert.False(viewModel.CurrentMap!.IsDirty);
    }

    [Fact]
    public void TryApplySettings_ShrinkAccepted_ResizesAndRenamesAsTwoUndoSteps()
    {
        var viewModel = Open(_repositoryContent, answer: true);
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
        var viewModel = Open(_repositoryContent);
        var settings = viewModel.StartEditingSettings()!;
        settings.HeightText = "100";

        Assert.True(viewModel.TryApplySettings(settings));

        Assert.Equal(100, viewModel.CurrentMap!.Map.Layout.Height);
        Assert.Empty(_questions);
    }

    [Fact]
    public void TryApplySettings_NothingChanged_AddsNoUndoStep()
    {
        var viewModel = Open(_repositoryContent);

        Assert.True(viewModel.TryApplySettings(viewModel.StartEditingSettings()!));

        Assert.False(viewModel.CurrentMap!.UndoCommand.CanExecute(null));
    }

    [Fact]
    public void TryApplySettings_EmptySizeBox_AppliesNothing()
    {
        var viewModel = Open(_repositoryContent);
        var settings = viewModel.StartEditingSettings()!;
        settings.HeightText = "";

        Assert.False(viewModel.TryApplySettings(settings));
        Assert.False(viewModel.CurrentMap!.IsDirty);
    }

    [Fact]
    public void TryCreateMap_ValidFields_OpensItUnsavedOutsideTheList()
    {
        var viewModel = Open(_repositoryContent);
        var newMap = viewModel.StartNewMap()!;
        newMap.Name = "Old Mill";

        Assert.True(viewModel.TryCreateMap(newMap));

        Assert.Equal("old_mill", viewModel.CurrentMap!.Map.Id);
        Assert.True(viewModel.CurrentMap.IsDirty);
        Assert.DoesNotContain("old_mill", viewModel.MapIds);
    }

    [Fact]
    public void Save_NewMap_WritesItAndAddsItToTheList()
    {
        var viewModel = Open(CopyOfRepositoryContent());
        var newMap = viewModel.StartNewMap()!;
        newMap.Name = "Old Mill";
        viewModel.TryCreateMap(newMap);

        viewModel.SaveCommand.Execute(null);

        Assert.Null(viewModel.SaveError);
        Assert.Equal(["forest", "interior_1", "old_mill", "start"], viewModel.MapIds);
        Assert.Equal(viewModel.MapIds, viewModel.EventEditing!.MapIds);
        Assert.False(viewModel.CurrentMap!.IsDirty);
        Assert.Equal("Old Mill", ContentCatalogLoader.Load(_contentCopy).Maps["old_mill"].Settings.DisplayName);
    }

    [Fact]
    public void Save_NewMap_AddsItsEntry()
    {
        var viewModel = Open(CopyOfRepositoryContent());
        var newMap = viewModel.StartNewMap()!;
        newMap.Name = "Old Mill";
        viewModel.TryCreateMap(newMap);

        viewModel.SaveCommand.Execute(null);

        Assert.Contains(new MapEntry("old_mill", "Old Mill (old_mill)"), viewModel.MapEntries);
    }

    [Fact]
    public void TryCreateMap_UnsavedChangesKept_StaysOnTheOpenMap()
    {
        var viewModel = Open(_repositoryContent, answer: false);
        PaintTopLeft(viewModel);

        Assert.False(viewModel.TryCreateMap(viewModel.StartNewMap()!));

        Assert.Equal("forest", viewModel.CurrentMap!.Map.Id);
    }

    [Fact]
    public void TryCreateMap_InvalidSize_OpensNothing()
    {
        var viewModel = Open(_repositoryContent);
        var newMap = viewModel.StartNewMap()!;
        newMap.WidthText = "0";

        Assert.False(viewModel.TryCreateMap(newMap));

        Assert.Equal("forest", viewModel.CurrentMap!.Map.Id);
    }

    [Fact]
    public void TryCreateMap_BlankName_OpensNothing()
    {
        var viewModel = Open(_repositoryContent);
        var newMap = viewModel.StartNewMap()!;
        newMap.Name = "";

        Assert.False(viewModel.TryCreateMap(newMap));

        Assert.Equal("forest", viewModel.CurrentMap!.Map.Id);
    }

    [Fact]
    public void Constructor_BrokenContentFile_ShowsTheLoaderMessageAndNoMaps()
    {
        Directory.CreateDirectory(_contentCopy);
        File.WriteAllText(Path.Combine(_contentCopy, "classes.json"), "[ {");

        var viewModel = Open(_contentCopy);

        Assert.StartsWith("classes.json", viewModel.LoadError, StringComparison.Ordinal);
        Assert.Empty(viewModel.MapIds);
        Assert.Null(viewModel.CurrentMap);
        Assert.Null(viewModel.Painting);
        Assert.Null(viewModel.Placing);
    }

    private MainViewModel Open(string contentRoot, bool answer = false) =>
        new(contentRoot, question =>
        {
            _questions.Add(question);
            return answer;
        });

    // Paints the open map's top-left ground cell with another tile already used on the map, so the result is valid content.
    private static int PaintTopLeft(MainViewModel viewModel)
    {
        var map = viewModel.CurrentMap!;
        var ground = map.Map.Layout.Layers[0];
        var before = ground.TileIdAt(new GridCell(0, 0));
        var after = ground.Rows.SelectMany(row => row).First(id => id != before && id != 0);
        map.Do(new SetTilesEdit(ground.Z, [new TileChange(new GridCell(0, 0), before, after)]));
        return after;
    }

    // Chooses, with Shift + drag, the first two cells side by side of the shown sheet that aren't tiles yet.
    private SheetCellRectangle ChooseTwoCellsNotTiles(MainViewModel viewModel)
    {
        var browser = viewModel.Painting!.Browser;
        var sheet = browser.SelectedSheet!;
        var (width, height) = PngSize.Read(Path.Combine(ArtFolders.RootIn(_contentCopy), ArtFolders.Tilesheets, sheet.File));
        var left = (from row in Enumerable.Range(0, sheet.RowsIn(height))
                    from column in Enumerable.Range(0, sheet.ColumnsIn(width) - 1)
                    select new SheetCell(sheet.Id, column, row))
            .First(cell => viewModel.Content!.TileAt(cell) is null && viewModel.Content.TileAt(cell with { Column = cell.Column + 1 }) is null);
        browser.SelectCellsBetween(
            new Point2D(sheet.CellLeft(left.Column), sheet.CellTop(left.Row)),
            new Point2D(sheet.CellLeft(left.Column + 1), sheet.CellTop(left.Row)));
        return browser.SelectedCells!.Value;
    }

    // Clicks the first cell of the shown sheet that isn't a tile yet, sized by its image in the content copy.
    private SheetCell ChooseCellNotATile(MainViewModel viewModel)
    {
        var browser = viewModel.Painting!.Browser;
        var sheet = browser.SelectedSheet!;
        var (width, height) = PngSize.Read(Path.Combine(ArtFolders.RootIn(_contentCopy), ArtFolders.Tilesheets, sheet.File));
        var cell = (from row in Enumerable.Range(0, sheet.RowsIn(height))
                    from column in Enumerable.Range(0, sheet.ColumnsIn(width))
                    select new SheetCell(sheet.Id, column, row)).First(cell => viewModel.Content!.TileAt(cell) is null);
        browser.SelectCellAt(sheet.CellLeft(cell.Column), sheet.CellTop(cell.Row));
        return cell;
    }

    private static int BlockFirstWalkableTile(MainViewModel viewModel)
    {
        var tileId = viewModel.Catalog!.Tiles.Values.First(tile => tile.IsWalkable).Id;
        viewModel.Content!.SetWalkable(tileId, isWalkable: false);
        return tileId;
    }

    private byte[] TilesAndForestBytes() =>
        [.. File.ReadAllBytes(Path.Combine(_contentCopy, "tiles.json")),
         .. File.ReadAllBytes(Path.Combine(_contentCopy, "map-layouts", "forest.json"))];

    private string CopyOfRepositoryContent()
    {
        foreach (var file in Directory.EnumerateFiles(_repositoryContent, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(_contentCopy, Path.GetRelativePath(_repositoryContent, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }

        return _contentCopy;
    }
}
