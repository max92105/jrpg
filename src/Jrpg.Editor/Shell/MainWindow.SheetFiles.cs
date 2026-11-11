using System.ComponentModel;
using System.Windows;
using Jrpg.Editor.Tiles;
using Microsoft.Win32;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The Tiles panel's questions about sheet files: the import's file dialog, the pictures of replaced images dropped,
/// and Remove sheet's purge question (decision 442).
/// </summary>
internal sealed partial class MainWindow
{
    // Remove sheet on a sheet whose image is missing waits for this answer before it writes (decision 442).
    private void OnRemovalChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SheetRemoval.PurgeQuestion)
            || sender is not SheetRemoval { PurgeQuestion: { } question } removal)
        {
            return;
        }

        if (Ask(question.Text))
        {
            removal.ApplyPurge();
        }
        else
        {
            removal.DropPurge();
        }
    }

    private void OnImportSheetImageClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Import sheet images",
            Filter = "PNG images (*.png)|*.png",
            Multiselect = true,
        };
        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.ImportSheetImages(dialog.FileNames);
        }
    }

    // A replaced file's picture is cached by the sheet browser, the map and the object preview alike.
    private void RedrawImportedImages(TileImages tileImages, IReadOnlyList<string> files)
    {
        foreach (var file in files)
        {
            tileImages.Forget(file);
        }

        TileBrowser.RedrawSheet();
        DrawCurrentMap();
        ObjectBrowser.ShowPreview();
    }
}
