using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The window's Tiles mode on the map: paint, erase, fill and pick, the rectangle tool, the tool keys, and the outline
/// of what a click would change.
/// </summary>
internal sealed partial class MainWindow
{
    private GridCell? _brushCell;

    // Where a rectangle drag started; it is filled when its button is released.
    private GridCell? _rectangleStart;

    // Anywhere but a text or combo box (whose typed letters pick an entry), so tools change while browsing tiles too.
    private bool HandleToolKey(Key key)
    {
        if (_viewModel.Painting is not { } painting || Keyboard.FocusedElement is TextBox or ComboBox or ComboBoxItem)
        {
            return false;
        }

        switch (key)
        {
            case Key.OemOpenBrackets:
                painting.ShrinkBrush();
                return true;
            case Key.OemCloseBrackets:
                painting.GrowBrush();
                return true;
            case Key.B:
                painting.Tool = TileTool.Brush;
                return true;
            case Key.R:
                painting.Tool = TileTool.Rectangle;
                return true;
            case Key.F:
                painting.Tool = TileTool.Fill;
                return true;
            default:
                return false;
        }
    }

    private bool HandlePaintButton(MouseButton button, GridCell cell)
    {
        if (button == MouseButton.Left && Keyboard.Modifiers == ModifierKeys.Alt)
        {
            Painting.PickTileAt(cell);
            return true;
        }

        if (button == MouseButton.Left && !Painting.HasChoice)
        {
            _viewModel.ShowPaintHint();
            return true;
        }

        // Shift + click fills with any tool.
        if (button == MouseButton.Left && (Keyboard.Modifiers == ModifierKeys.Shift || Painting.Tool == TileTool.Fill))
        {
            Painting.FloodFill(cell);
            return true;
        }

        return button is MouseButton.Left or MouseButton.Right && StartDrag(button, cell);
    }

    // Left paints, right erases: with the brush as the mouse moves, with the rectangle tool on the release.
    private bool StartDrag(MouseButton button, GridCell cell)
    {
        if (Painting.Tool == TileTool.Rectangle)
        {
            _rectangleStart = cell;
        }
        else if (button == MouseButton.Left)
        {
            Painting.StartPainting(cell);
        }
        else
        {
            Painting.StartErasing(cell);
        }

        _strokeButton = button;
        MapViewport.CaptureMouse();
        return true;
    }

    private void FinishRectangle(GridCell start, GridCell end)
    {
        if (_strokeButton == MouseButton.Left)
        {
            Painting.PaintRectangle(start, end);
        }
        else
        {
            Painting.EraseRectangle(start, end);
        }
    }

    /// <summary>
    /// Outlines what a click or drag at the cell would change: the brush, the rectangle being dragged, or one cell.
    /// </summary>
    /// <param name="cell">The cell under the mouse, or null to hide the outline.</param>
    private void ShowBrushAt(GridCell? cell)
    {
        _brushCell = cell;
        if (cell is not { } at || _viewModel.CurrentMap is null || _viewModel.Mode != EditingMode.Tiles)
        {
            BrushOutline.Visibility = Visibility.Collapsed;
            return;
        }

        var (corner, oppositeCorner) = _rectangleStart is { } start ? (start, at) : SquareCorners(at);
        Canvas.SetLeft(BrushOutline, Math.Min(corner.X, oppositeCorner.X) * MapDrawing.CellPixels);
        Canvas.SetTop(BrushOutline, Math.Min(corner.Y, oppositeCorner.Y) * MapDrawing.CellPixels);
        BrushOutline.Width = (Math.Abs(oppositeCorner.X - corner.X) + 1) * MapDrawing.CellPixels;
        BrushOutline.Height = (Math.Abs(oppositeCorner.Y - corner.Y) + 1) * MapDrawing.CellPixels;
        BrushOutline.StrokeThickness = 2 / _viewport.Zoom;
        BrushOutline.Visibility = Visibility.Visible;
    }

    // Only the brush is wider than one cell.
    private (GridCell Corner, GridCell OppositeCorner) SquareCorners(GridCell center)
    {
        var size = Painting.Tool == TileTool.Brush ? Painting.BrushSize : 1;
        var topLeft = BrushCells.Square(center, size).First();
        return (topLeft, new GridCell(topLeft.X + size - 1, topLeft.Y + size - 1));
    }
}
