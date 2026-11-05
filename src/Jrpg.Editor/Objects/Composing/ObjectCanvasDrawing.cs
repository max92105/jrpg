using System.Windows;
using System.Windows.Media;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Objects.Composing;

/// <summary>
/// The composer's canvas as a picture with its grid: each painted cell's image at the map's cell size, which the window
/// scales to the canvas's zoom (decision 484), levels from the base up, those above the chosen one faded (decision
/// 479).
/// </summary>
internal static class ObjectCanvasDrawing
{
    // What will cover the cells being painted stays visible without hiding them (placeholder look).
    private const double UpperLevelOpacity = 0.4;

    /// <param name="canvas">The canvas drawn.</param>
    /// <param name="chosenLevel">The level strokes paint; the levels above it are faded.</param>
    /// <param name="imageOf">
    /// Each sheet cell's image: <see cref="Tiles.TileImages.CellImageOf"/>, or the frame shown at a time.
    /// </param>
    /// <param name="gridLineWidth">In canvas pixels: one screen pixel is 1 / the canvas's scale.</param>
    /// <returns>Frozen, <see cref="MapDrawing.CellPixels"/> per cell, clipped to the canvas.</returns>
    public static DrawingImage Draw(ObjectCanvas canvas, int chosenLevel, Func<SheetCell, ImageSource> imageOf, double gridLineWidth)
    {
        var area = new Rect(0, 0, canvas.Width * MapDrawing.CellPixels, canvas.Height * MapDrawing.CellPixels);
        var drawing = new DrawingGroup { ClipGeometry = new RectangleGeometry(area) };
        using (var context = drawing.Open())
        {
            context.DrawRectangle(Brushes.Transparent, null, area);
            foreach (var (at, cell) in canvas.Cells.OrderBy(pair => pair.Key.Level))
            {
                var isFaded = at.Level > chosenLevel;
                if (isFaded)
                {
                    context.PushOpacity(UpperLevelOpacity);
                }

                context.DrawImage(imageOf(cell), CellArea(at.Cell));
                if (isFaded)
                {
                    context.Pop();
                }
            }

            context.DrawGeometry(null, new Pen(MapDrawing.GridBrush, gridLineWidth), MapDrawing.GridLines(canvas.Width, canvas.Height));
        }

        var image = new DrawingImage(drawing);
        image.Freeze();
        return image;
    }

    private static Rect CellArea(GridCell at) =>
        new(at.X * MapDrawing.CellPixels, at.Y * MapDrawing.CellPixels, MapDrawing.CellPixels, MapDrawing.CellPixels);
}
