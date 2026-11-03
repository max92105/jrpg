using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Objects.Composing;

/// <summary>
/// The object composer (decision 455): the Tiles panel's sheet view as the picker, the boxes and the canvas. The window
/// only turns mouse positions into canvas cells; a press to its release is one stroke. Save hands the composer to the
/// apply step, which returns false when nothing was made, and the window then stays open.
/// </summary>
internal sealed partial class ObjectComposerWindow : Window
{
    private readonly ObjectComposerViewModel _composer;
    private readonly TileImages _images;
    private readonly Func<ObjectComposerViewModel, bool> _tryApply;
    private MouseButton? _strokeButton;
    private GridCell? _lastStrokeCell;

    /// <param name="composer">The composer's state, edited here.</param>
    /// <param name="images">The main window's sheet images, shared by the picker and the canvas.</param>
    /// <param name="tryApply">Makes the object; false when it didn't.</param>
    public ObjectComposerWindow(ObjectComposerViewModel composer, TileImages images, Func<ObjectComposerViewModel, bool> tryApply)
    {
        InitializeComponent();
        DataContext = _composer = composer;
        (_images, _tryApply) = (images, tryApply);
        // The picker draws its sheet as soon as it has a view model, so it needs its images first.
        Picker.Images = images;
        Picker.DataContext = composer.Painting.Picker;
        composer.Painting.PropertyChanged += OnPaintingChanged;
        Closed += OnClosed;
        Loaded += (_, _) => IdBox.Focus();
        DrawCanvas();
    }

    private void OnPaintingChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CanvasPainting.Canvas))
        {
            DrawCanvas();
        }
    }

    private void DrawCanvas() => CanvasImage.Source = ObjectCanvasDrawing.Draw(_composer.Painting.Canvas, _images.CellImageOf);

    private void OnCanvasPressed(object sender, MouseButtonEventArgs e)
    {
        if (_strokeButton is not null || e.ChangedButton is not (MouseButton.Left or MouseButton.Right))
        {
            return;
        }

        // The id box would otherwise keep the focus, and its own Ctrl+Z would undo its text instead of the stroke.
        CanvasImage.Focus();
        _strokeButton = e.ChangedButton;
        _composer.Painting.BeginStroke();
        CanvasImage.CaptureMouse();
        StrokeAt(e.GetPosition(CanvasImage));
    }

    private void OnCanvasMouseMoved(object sender, MouseEventArgs e)
    {
        if (_strokeButton is not null)
        {
            StrokeAt(e.GetPosition(CanvasImage));
        }
    }

    private void OnCanvasReleased(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == _strokeButton)
        {
            CanvasImage.ReleaseMouseCapture();
        }
    }

    // Releasing the button, or losing the mouse to another window, ends the stroke.
    private void OnCanvasLostMouseCapture(object sender, MouseEventArgs e)
    {
        if (_strokeButton is null)
        {
            return;
        }

        _composer.Painting.EndStroke();
        (_strokeButton, _lastStrokeCell) = (null, null);
    }

    // A drag paints each cell it enters once, not once per mouse move.
    private void StrokeAt(Point position)
    {
        var cell = new GridCell((int)Math.Floor(position.X / MapDrawing.CellPixels), (int)Math.Floor(position.Y / MapDrawing.CellPixels));
        if (cell == _lastStrokeCell)
        {
            return;
        }

        _lastStrokeCell = cell;
        if (_strokeButton == MouseButton.Right)
        {
            _composer.Painting.EraseAt(cell);
        }
        else
        {
            _composer.Painting.PaintAt(cell);
        }
    }

    private void OnSaveClicked(object sender, RoutedEventArgs e)
    {
        if (_tryApply(_composer))
        {
            DialogResult = true;
        }
    }

    // The browser outlives the window: without a null data context the picker would stay subscribed to it, and every
    // closed composer would redraw its sheet on each change.
    private void OnClosed(object? sender, EventArgs e)
    {
        _composer.Painting.PropertyChanged -= OnPaintingChanged;
        Picker.DataContext = null;
    }
}
