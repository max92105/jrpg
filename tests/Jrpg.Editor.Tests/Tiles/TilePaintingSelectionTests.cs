using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

/// <summary>The map cell selection of Ctrl + drag in Tiles mode (decision 514), on the test world's forest.</summary>
public sealed class TilePaintingSelectionTests
{
    private static readonly string _contentRoot = Path.Combine(AppContext.BaseDirectory, "test-world");
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(_contentRoot);

    private readonly TilePaintingViewModel _painting;

    public TilePaintingSelectionTests()
    {
        var draft = new CatalogDraft(_catalog);
        var browser = new TileBrowserViewModel(draft, new SheetImages(() => ArtFolders.TilesheetImagesIn(ArtFolders.RootIn(_contentRoot)), _ => (1000, 1000)), _ => true, () => []);
        _painting = new TilePaintingViewModel(draft, new LayerChoiceViewModel(), browser) { OpenMap = new MapViewModel(_catalog.Maps["forest"]) };
    }

    [Fact]
    public void SelectMapCells_TwoCorners_SelectsTheRectangle()
    {
        _painting.SelectMapCells(new GridCell(4, 3), new GridCell(1, 2));

        Assert.Equal(new MapCellRectangle(new GridCell(1, 2), 4, 2), _painting.SelectedMapCells);
    }

    [Fact]
    public void SelectMapCells_PastTheMap_IsClipped()
    {
        var layout = _painting.OpenMap!.Map.Layout;

        _painting.SelectMapCells(new GridCell(layout.Width - 2, 0), new GridCell(layout.Width + 5, 1));

        Assert.Equal(new MapCellRectangle(new GridCell(layout.Width - 2, 0), 2, 2), _painting.SelectedMapCells);
    }

    [Fact]
    public void ClearMapSelection_Selected_EmptiesIt()
    {
        _painting.SelectMapCells(new GridCell(1, 1), new GridCell(2, 2));

        _painting.ClearMapSelection();

        Assert.Null(_painting.SelectedMapCells);
    }

    [Fact]
    public void OpenMap_Changed_ClearsTheSelection()
    {
        _painting.SelectMapCells(new GridCell(1, 1), new GridCell(2, 2));

        _painting.OpenMap = new MapViewModel(_catalog.Maps["start"]);

        Assert.Null(_painting.SelectedMapCells);
    }

    [Fact]
    public void MapShrunk_SelectionPastTheNewEdge_IsClipped()
    {
        var map = _painting.OpenMap!;
        var width = map.Map.Layout.Width;
        _painting.SelectMapCells(new GridCell(width - 3, 0), new GridCell(width - 1, 1));

        map.Do(new ResizeMapEdit(map.Map.Layout, width - 2, map.Map.Layout.Height, _catalog.Objects));

        Assert.Equal(new MapCellRectangle(new GridCell(width - 3, 0), 1, 2), _painting.SelectedMapCells);
    }

    [Fact]
    public void SelectMapCells_PaintsNothing()
    {
        _painting.SelectMapCells(new GridCell(1, 1), new GridCell(2, 2));

        Assert.False(_painting.OpenMap!.IsDirty);
    }
}
