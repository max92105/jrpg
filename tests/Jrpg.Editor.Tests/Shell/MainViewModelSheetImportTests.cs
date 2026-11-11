using Jrpg.Editor.Tests.Tiles;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>Tiles > Import sheet images through the editor, on a copy of the test world.</summary>
public sealed class MainViewModelSheetImportTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void ImportSheetImages_NewImage_ShowsItAndReportsImported()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var source = PngHeaderFile.Write(Path.Combine(_content.ContentCopy, "import-source", "Cave.png"), 96, 48);

        viewModel.ImportSheetImages([source]);

        Assert.Equal(new SheetChoice("Cave.png", null), viewModel.Painting!.Browser.SelectedChoice);
        Assert.Equal("Imported Cave.png.", viewModel.ImportStatus);
    }

    // In the test world, Grass3.png has no sheet, so it can be replaced by any size; Set up's guess reads the size: 96
    // x 48 is 6 x 3 cells of 16.
    [Fact]
    public void ImportSheetImages_ReplacedImage_ReadsTheNewSize()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld(), answer: true);
        var browser = viewModel.Painting!.Browser;
        browser.SelectedChoice = browser.SheetChoices.First(choice => choice.File == "Grass3.png");
        browser.SetUpSheetCommand.Execute(null);
        browser.CancelCutCommand.Execute(null);
        var source = PngHeaderFile.Write(Path.Combine(_content.ContentCopy, "import-source", "Grass3.png"), 96, 48);

        viewModel.ImportSheetImages([source]);
        browser.SetUpSheetCommand.Execute(null);

        Assert.Equal((6, 3), (browser.Cut!.Columns, browser.Cut.Rows));
    }
}
