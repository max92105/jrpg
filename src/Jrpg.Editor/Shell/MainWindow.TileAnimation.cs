using System.Windows;
using System.Windows.Media;
using Jrpg.Editor.Catalogs;
using Microsoft.Win32;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The main window's animated tiles (ADR 0039): Tiles > Import animations, and the render tick moving the editor's
/// animation time on (decision 470) and the Animate panel's preview with it.
/// </summary>
internal sealed partial class MainWindow
{
    private TimeSpan? _lastRenderingTime;
    private SheetCell? _shownPreviewCell;

    private void OnImportAnimationsClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title = "Import animations", Filter = "Tiled tilesets (*.tsx)|*.tsx" };
        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.AnimationImport?.Import(dialog.FileName);
        }
    }

    // WPF raises Rendering once per frame it draws (about 16 ms); the main window lives as long as the editor, so the
    // static event never outlives it.
    private void OnRendering(object? sender, EventArgs e)
    {
        if (e is not RenderingEventArgs rendering || _viewModel.AnimationTime is not { } time)
        {
            return;
        }

        if (_lastRenderingTime is { } last && rendering.RenderingTime > last)
        {
            time.Advance(rendering.RenderingTime - last);
        }

        _lastRenderingTime = rendering.RenderingTime;
        ShowAnimationPreview(time.Elapsed);
    }

    // The preview's frame comes from the typed boxes, not the catalog, so it is compared here each tick: a new cell (a
    // frame change, or a box typed into) is the only redraw.
    private void ShowAnimationPreview(TimeSpan elapsed)
    {
        if (_viewModel.Animating is not { IsOpen: true } animating || TileBrowser.Images is not { } images)
        {
            _shownPreviewCell = null;
            return;
        }

        var cell = animating.PreviewCellAt(elapsed);
        if (cell != _shownPreviewCell)
        {
            _shownPreviewCell = cell;
            AnimationPanel.ShowPreview(images.CellImageOf(cell));
        }
    }
}
