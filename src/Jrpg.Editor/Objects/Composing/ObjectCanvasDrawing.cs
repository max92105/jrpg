using System.Windows;
using System.Windows.Media;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Objects.Composing;

/// <summary>
/// The composer's canvas as a picture: each painted cell's image at the map's cell size, levels from the base up, those
/// above the chosen level faded (decision 479), with the grid.
/// </summary>
internal static class ObjectCanvasDrawing
{
    // One pixel at the window's 1:1 scale.
    private const double GridLineWidth = 1;

    // What will cover the cells being painted stays visible without hiding them (placeholder look).
    private const double UpperLevelOpacity = 0.4;

    /// <param name="canvas">The canvas drawn.</param>
    /// <param name="chosenLevel">The level strokes paint; the levels above it are faded.</param>
    /// <param name="imageOf">
    /// Each sheet cell's image: <see cref="Tiles.TileImages.CellImageOf"/>, or the frame shown at a time.
    /// </param>
    /// <returns>Frozen, <see cref="MapDrawing.CellPixels"/> per cell, clipped to the canvas.</returns>
    public static DrawingImage Draw(ObjectCanvas canvas, int chosenLevel, Func<SheetCell, ImageSource> imageOf)
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

            context.DrawGeometry(null, new Pen(MapDrawing.GridBrush, GridLineWidth), MapDrawing.GridLines(canvas.Width, canvas.Height));
        }

        var image = new DrawingImage(drawing);
        image.Freeze();
        return image;
    }

    private static Rect CellArea(GridCell at) =>
        new(at.X * MapDrawing.CellPixels, at.Y * MapDrawing.CellPixels, MapDrawing.CellPixels, MapDrawing.CellPixels);
}
