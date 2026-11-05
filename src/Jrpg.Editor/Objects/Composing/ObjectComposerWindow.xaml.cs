using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Tiles;
using Jrpg.Editor.Viewport;

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
    private readonly TileAnimationTime _animationTime;
    private readonly Func<ObjectComposerViewModel, bool> _tryApply;
    private MouseButton? _strokeButton;
    private GridCell? _lastStrokeCell;
    private double _scale = 1;

    // Where Ctrl + wheel left the mouse, which the next zoom keeps in place; the viewer's center for the bar's buttons.
    private Point? _zoomAnchor;

    /// <param name="composer">The composer's state, edited here.</param>
    /// <param name="images">The main window's sheet images, shared by the picker and the canvas.</param>
    /// <param name="animationTime">
    /// The editor's animation time, which the canvas's animated cells follow (decision 470).
    /// </param>
    /// <param name="tryApply">Makes the object; false when it didn't.</param>
    public ObjectComposerWindow(ObjectComposerViewModel composer, TileImages images, TileAnimationTime animationTime, Func<ObjectComposerViewModel, bool> tryApply)
    {
        InitializeComponent();
        DataContext = _composer = composer;
        (_images, _animationTime, _tryApply) = (images, animationTime, tryApply);
        // The picker draws its sheet as soon as it has a view model, so it needs its images first.
        Picker.Images = images;
        Picker.DataContext = composer.Painting.Picker;
        composer.Painting.PropertyChanged += OnPaintingChanged;
        composer.CanvasZoom.PropertyChanged += OnCanvasZoomChanged;
        animationTime.FramesChanged += OnFramesChanged;
        Closed += OnClosed;
        Loaded += (_, _) => IdBox.Focus();
        DrawCanvas();
    }

    private void OnPaintingChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CanvasPainting.Canvas) or nameof(CanvasPainting.Level))
        {
            DrawCanvas();
        }
    }

    private void OnFramesChanged(object? sender, EventArgs e) => DrawCanvas();

    private void DrawCanvas()
    {
        _scale = _composer.CanvasZoom.ScaleFor(MapDrawing.CellPixels);
        CanvasImage.Source = ObjectCanvasDrawing.Draw(_composer.Painting.Canvas, _composer.Painting.Level, cell => _images.CellImageAt(cell, _animationTime.Elapsed), 1 / _scale);
        CanvasImage.LayoutTransform = new ScaleTransform(_scale, _scale);
    }

    private void OnCanvasZoomChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CellZoom.CellPixels))
        {
            return;
        }

        var oldScale = _scale;
        DrawCanvas();
        var anchor = _zoomAnchor ?? new Point(CanvasScroller.ViewportWidth / 2, CanvasScroller.ViewportHeight / 2);
        _zoomAnchor = null;
        ZoomScrolling.KeepAnchor(CanvasScroller, anchor, oldScale, _scale);
    }

    // The plain wheel scrolls; a command at its bound doesn't run (the toolkit's Execute doesn't check).
    private void OnCanvasMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            return;
        }

        e.Handled = true;
        var command = e.Delta > 0 ? _composer.CanvasZoom.ZoomInCommand : _composer.CanvasZoom.ZoomOutCommand;
        if (command.CanExecute(null))
        {
            _zoomAnchor = e.GetPosition(CanvasScroller);
            command.Execute(null);
        }
    }

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

    // A drag paints each cell it enters once, not once per mouse move. The position is in the canvas's own pixels
    // whatever its zoom: GetPosition undoes the image's layout transform.
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
        _composer.CanvasZoom.PropertyChanged -= OnCanvasZoomChanged;
        _animationTime.FramesChanged -= OnFramesChanged;
        Picker.DataContext = null;
    }
}
