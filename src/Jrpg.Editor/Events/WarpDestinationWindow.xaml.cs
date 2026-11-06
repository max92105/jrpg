using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Viewport;

namespace Jrpg.Editor.Events;

/// <summary>
/// The warp step's destination picker (decision 506): the maps on the left, the chosen map's picture on the right, a
/// click choosing the arrival cell and a double-click choosing it and pressing OK.
/// </summary>
internal sealed partial class WarpDestinationWindow : Window
{
    private static readonly Brush _chosenCellBrush = Brushes.Cyan;

    private readonly WarpDestinationViewModel _picker;
    private readonly MapDrawing _drawing;
    private double _scale = 1;

    private WarpDestinationWindow(WarpDestinationViewModel picker, MapDrawing drawing)
    {
        InitializeComponent();
        DataContext = _picker = picker;
        _drawing = drawing;
        ChosenCell.Stroke = _chosenCellBrush;
        ChosenCell.Width = ChosenCell.Height = MapDrawing.CellPixels;
        picker.PropertyChanged += OnPickerChanged;
        // Held by the viewer's and the zoom's events, which live as long as this window.
        _ = new ZoomedScroller(PictureScroller, ApplyZoom, () => _scale) { Zoom = picker.Zoom };
        DrawMap();
    }

    /// <summary>
    /// Opens the picker on the step's destination; OK puts the picked map and cell in the step's boxes.
    /// </summary>
    /// <returns>True when a destination was taken; false on Cancel.</returns>
    public static bool TryPick(Window owner, EventStepViewModel step, WarpMaps maps, MapDrawing drawing)
    {
        var picker = step.StartPickingDestination(maps);
        if (new WarpDestinationWindow(picker, drawing) { Owner = owner }.ShowDialog() != true)
        {
            return false;
        }

        step.TakeDestination(picker);
        return true;
    }

    private void OnPickerChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WarpDestinationViewModel.Layout))
        {
            DrawMap();
        }
        else if (e.PropertyName == nameof(WarpDestinationViewModel.Cell))
        {
            PlaceChosenCell();
        }
    }

    private void DrawMap()
    {
        MapImage.Source = _picker.Layout is { } layout ? _drawing.Draw(layout, focusZ: null) : null;
        ApplyZoom();
    }

    // The grid and the cell's outline stay one and two screen pixels wide at every zoom.
    private void ApplyZoom()
    {
        _scale = _picker.Zoom.ScaleFor(MapDrawing.CellPixels);
        Picture.LayoutTransform = new ScaleTransform(_scale, _scale);
        GridImage.Source = _picker.Layout is { } layout ? MapDrawing.DrawGrid(layout, 1 / _scale) : null;
        PlaceChosenCell();
    }

    private void PlaceChosenCell()
    {
        ChosenCell.Visibility = _picker.Cell is null ? Visibility.Collapsed : Visibility.Visible;
        ChosenCell.StrokeThickness = 2 / _scale;
        if (_picker.Cell is { } cell)
        {
            Canvas.SetLeft(ChosenCell, cell.X * MapDrawing.CellPixels);
            Canvas.SetTop(ChosenCell, cell.Y * MapDrawing.CellPixels);
        }
    }



    // Under a layout transform the position is in the picture's own pixels, a cell each MapDrawing.CellPixels.
    private void OnPicturePressed(object sender, MouseButtonEventArgs e)
    {
        var position = e.GetPosition(Picture);
        _picker.ChooseCell(GridCell.Containing(new Point2D(position.X / MapDrawing.CellPixels, position.Y / MapDrawing.CellPixels)));
        if (e.ClickCount == 2)
        {
            CloseIfPicked();
        }
    }

    private void OnOkClicked(object sender, RoutedEventArgs e) => CloseIfPicked();

    private void CloseIfPicked()
    {
        if (_picker.Problem is null)
        {
            DialogResult = true;
        }
    }
}
