using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

/// <summary>The sheet view's zoom (decision 482): the panel's for the session, on the test world.</summary>
public sealed class TileBrowserZoomTests
{
    private readonly TileBrowserViewModel _browser;

    public TileBrowserZoomTests()
    {
        var draft = new CatalogDraft(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));
        var files = draft.Current.Tilesheets.Values.Select(sheet => sheet.File).ToList();
        _browser = new TileBrowserViewModel(draft, new SheetImages(() => files, _ => (512, 512)), _ => true, () => []);
    }

    [Fact]
    public void Zoom_New_ShowsTheMapCellSize() => Assert.Equal(48, _browser.Zoom.CellPixels);

    [Fact]
    public void Zoom_ChoosingAnotherSheet_KeepsTheCellSize()
    {
        _browser.Zoom.ZoomInCommand.Execute(null);
        _browser.Zoom.ZoomInCommand.Execute(null);
        var shown = _browser.SelectedChoice;

        _browser.SelectedChoice = _browser.SheetChoices.First(choice => choice != shown);

        Assert.Equal(96, _browser.Zoom.CellPixels);
    }
}
