using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Jrpg.Editor.Viewport;

/// <summary>
/// A picture in a scroll viewer, zoomed by a <see cref="CellZoom"/> (decision 483): Ctrl + wheel steps the zoom toward
/// the mouse, and a zoom change redraws the picture and scrolls so the point under the mouse, or the viewer's center
/// for the bar's buttons, stays in place. The plain wheel still scrolls. Shared by the Tiles panel's sheet, the
/// composer's canvas and the warp destination picker.
/// </summary>
internal sealed class ZoomedScroller
{
    private readonly ScrollViewer _viewer;
    private readonly Action _redraw;
    private readonly Func<double> _scale;
    private CellZoom? _zoom;

    // Where Ctrl + wheel left the mouse, which the next zoom keeps in place; null for the bar's buttons.
    private Point? _anchor;

    /// <param name="viewer">The scroll viewer holding the picture.</param>
    /// <param name="redraw">Draws the picture at the zoom now.</param>
    /// <param name="scale">The picture's scale as last drawn.</param>
    public ZoomedScroller(ScrollViewer viewer, Action redraw, Func<double> scale)
    {
        (_viewer, _redraw, _scale) = (viewer, redraw, scale);
        viewer.PreviewMouseWheel += OnMouseWheel;
    }

    /// <summary>The zoom followed; null lets go of the last one (a view whose data context left).</summary>
    public CellZoom? Zoom
    {
        get => _zoom;
        set
        {
            _zoom?.PropertyChanged -= OnZoomChanged;
            _zoom = value;
            _zoom?.PropertyChanged += OnZoomChanged;
        }
    }

    private void OnZoomChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CellZoom.CellPixels))
        {
            return;
        }

        var oldScale = _scale();
        _redraw();
        var anchor = _anchor ?? new Point(_viewer.ViewportWidth / 2, _viewer.ViewportHeight / 2);
        _anchor = null;
        ZoomScrolling.KeepAnchor(_viewer, anchor, oldScale, _scale());
    }

    // A command at its bound doesn't run (the toolkit's Execute doesn't check).
    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || _zoom is null)
        {
            return;
        }

        e.Handled = true;
        var command = e.Delta > 0 ? _zoom.ZoomInCommand : _zoom.ZoomOutCommand;
        if (command.CanExecute(null))
        {
            _anchor = e.GetPosition(_viewer);
            command.Execute(null);
        }
    }
}
