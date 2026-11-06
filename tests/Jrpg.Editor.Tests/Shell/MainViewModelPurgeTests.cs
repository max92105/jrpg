using Jrpg.Data.Art;
using Jrpg.Data.Content;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// Remove sheet on a sheet whose image is missing (decision 442), through the editor: asked, written, reopened.
/// </summary>
public sealed class MainViewModelPurgeTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void RemoveSheet_Purged_ReopensTheOpenMapWithoutTheCells()
    {
        var (viewModel, root) = OpenWithCityMissing();
        var cityTiles = viewModel.Catalog!.Tiles.Values.Where(tile => tile.SheetId == "city").Select(tile => tile.Id).ToHashSet();
        var openId = viewModel.CurrentMap!.Map.Id;

        PurgeCity(viewModel);

        Assert.DoesNotContain(viewModel.CurrentMap!.Map.Layout.Layers, layer => layer.Rows.Any(row => row.Any(cityTiles.Contains)));
        Assert.Equal(openId, viewModel.CurrentMap.Map.Id);
        Assert.False(ContentCatalogLoader.Load(root).Tilesheets.ContainsKey("city"));
        Assert.DoesNotContain("City.png", viewModel.Art!.Warning ?? "", StringComparison.Ordinal);
    }

    // What the window does when the catalogs change: draw the open map's tiles and objects from them. The open map must
    // already be the purged one by then, or a city cell names a tile that is gone.
    [Fact]
    public void RemoveSheet_Purged_OpenMapIsReplacedBeforeTheCatalogsChange()
    {
        var (viewModel, _) = OpenWithCityMissing();
        var drawn = 0;
        viewModel.Content!.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(viewModel.Content.Current))
            {
                return;
            }

            var layout = viewModel.CurrentMap!.Map.Layout;
            var catalog = viewModel.Content.Current;
            drawn += layout.Layers.Sum(layer => layer.Rows.Sum(row => row.Count(id => id != 0 && catalog.Tiles[id] is not null)));
            drawn += layout.Objects.Count(placed => catalog.Objects[placed.ObjectId] is not null);
        };

        PurgeCity(viewModel);

        Assert.True(drawn > 0);
    }

    [Fact]
    public void RemoveSheet_Purged_ClearsUndo()
    {
        var (viewModel, _) = OpenWithCityMissing();
        viewModel.Content!.SetWalkable(viewModel.Catalog!.Tiles.Values.First(tile => tile.SheetId != "city").Id, isWalkable: false);
        viewModel.SaveCommand.Execute(null);
        Assert.True(viewModel.History!.UndoCommand.CanExecute(null));

        PurgeCity(viewModel);

        Assert.False(viewModel.History.UndoCommand.CanExecute(null));
        Assert.False(viewModel.HasUnsavedChanges);
    }

    // The rebuilt map is one of the open maps: its next edit is unsaved and the close question names it.
    [Fact]
    public void RemoveSheet_PurgedThenPainted_CountsAsUnsaved()
    {
        var (viewModel, _) = OpenWithCityMissing();
        PurgeCity(viewModel);

        EditorTestContent.PaintTopLeft(viewModel);

        Assert.True(viewModel.HasUnsavedChanges);
        Assert.False(viewModel.ConfirmClose());
        Assert.Single(_content.Questions);
    }

    [Fact]
    public void RemoveSheet_PurgedThenAnotherMapAndBack_ShowsTheRebuiltMap()
    {
        var (viewModel, _) = OpenWithCityMissing();
        var openId = viewModel.CurrentMap!.Map.Id;
        PurgeCity(viewModel);
        var rebuilt = viewModel.CurrentMap;

        viewModel.TryOpenMap(viewModel.MapIds.First(id => id != openId));
        viewModel.TryOpenMap(openId);

        Assert.Same(rebuilt, viewModel.CurrentMap);
    }

    private (Editor.Shell.MainViewModel ViewModel, string Root) OpenWithCityMissing()
    {
        var root = _content.CopyOfTestWorld();
        File.Delete(Path.Combine(ArtFolders.RootIn(root), ArtFolders.Tilesheets, "City.png"));
        return (_content.Open(root), root);
    }

    private static void PurgeCity(Editor.Shell.MainViewModel viewModel)
    {
        var browser = viewModel.Painting!.Browser;
        browser.SelectedChoice = browser.SheetChoices.Single(choice => choice.Sheet?.Id == "city");
        viewModel.Removal!.RemoveSheetCommand.Execute(null);
        Assert.NotNull(viewModel.Removal.PurgeQuestion);
        viewModel.Removal.ApplyPurge();
    }
}
