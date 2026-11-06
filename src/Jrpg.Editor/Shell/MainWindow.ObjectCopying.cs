using System.Windows;
using System.Windows.Input;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Shell;

/// <summary>
/// Copy, Cut and Paste in Objects mode (decision 512): the window's command bindings over
/// <see cref="Objects.ObjectCopying"/>. A focused text box handles the three keys itself first.
/// </summary>
internal sealed partial class MainWindow
{
    private bool IsObjectsModeWithMap => _viewModel.Mode == EditingMode.Objects && _viewModel.CurrentMap is not null;

    private void OnCopyCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        e.CanExecute = IsObjectsModeWithMap && Placing.Copying.CopyCommand.CanExecute(null);

    private void OnCopyExecuted(object sender, ExecutedRoutedEventArgs e) => Placing.Copying.CopyCommand.Execute(null);

    private void OnCutCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        e.CanExecute = IsObjectsModeWithMap && Placing.Copying.CutCommand.CanExecute(null);

    private void OnCutExecuted(object sender, ExecutedRoutedEventArgs e) => Placing.Copying.CutCommand.Execute(null);

    private void OnPasteCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = IsObjectsModeWithMap && Placing.Copying.CanPaste;

    // Centered on the mouse over the map; from the Edit menu, where the mouse isn't, on the middle of the view.
    private void OnPasteExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var center = _mapPoint ?? MapPointAtViewCenter();
        if (IsSnapping)
        {
            Placing.Copying.PasteOnGridCenteredAt(center);
        }
        else
        {
            Placing.Copying.PasteCenteredAt(center);
        }
    }

    private Point2D MapPointAtViewCenter()
    {
        var pixel = _viewport.ScreenToMap(new Point(MapViewport.ActualWidth / 2, MapViewport.ActualHeight / 2));
        return new Point2D(pixel.X / MapDrawing.CellPixels, pixel.Y / MapDrawing.CellPixels);
    }
}
