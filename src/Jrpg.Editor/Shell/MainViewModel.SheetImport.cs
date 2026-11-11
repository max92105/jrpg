using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Shell;

/// <summary>
/// Tiles > Import sheet images: the images copied into the tilesheet art through Data, asking before replacing one, the
/// last shown in the Tiles panel and the outcome in the status bar.
/// </summary>
internal sealed partial class MainViewModel
{
    /// <summary>What the last import copied and refused ("Imported Forest2.png"); empty before any import.</summary>
    [ObservableProperty]
    public partial string ImportStatus { get; private set; } = "";

    /// <summary>
    /// The images an import copied, raised before the last is shown, so cached pictures of replaced files are dropped.
    /// </summary>
    public event EventHandler<IReadOnlyList<string>>? SheetImagesImported;

    /// <summary>
    /// Copies images into the tilesheet art (asking before replacing one), then shows the last one copied, "(not set
    /// up)" if it has no sheet. A file copy, not an undo step.
    /// </summary>
    /// <param name="paths">The images chosen in the file dialog.</param>
    public void ImportSheetImages(IReadOnlyList<string> paths)
    {
        if (Content is null || Painting is null)
        {
            return;
        }

        var result = new SheetImport(_writer, Content)
            .Import(paths, file => _confirm($"{file} is already in the tilesheet art. Replace it?"));
        foreach (var file in result.Imported)
        {
            _sheetImageSizes.Forget(file);
        }

        if (result.LastImported is { } last)
        {
            Art!.Refresh();
            SheetImagesImported?.Invoke(this, result.Imported);
            Painting.Browser.Show(last);
        }

        ImportStatus = result.Summary;
    }
}
