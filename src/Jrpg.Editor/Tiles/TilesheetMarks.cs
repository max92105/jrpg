using System.Windows;
using System.Windows.Media;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// Drawn over a tilesheet in the tile browser: its grid, the cells that are tiles (crossed out when blocked), and the
/// chosen cell. The browser shows a sheet's cells at the map's cell size, so line widths are set in screen pixels at that
/// scale.
/// </summary>
internal static class TilesheetMarks
{
    private static readonly Brush _gridBrush = Frozen(new SolidColorBrush(Color.FromArgb(0x50, 0xFF, 0xFF, 0xFF)));
    private static readonly Brush _tileFill = Frozen(new SolidColorBrush(Color.FromArgb(0x30, 0x00, 0xFF, 0x00)));
    private static readonly Brush _blockedBrush = Frozen(new SolidColorBrush(Color.FromArgb(0xC0, 0xFF, 0x30, 0x30)));

    /// <summary>How much the browser scales a sheet's image, so its cells show at the map's cell size.</summary>
    public static double DisplayScaleOf(TilesheetDefinition sheet) => MapDrawing.CellPixels / sheet.TileWidth;

    /// <param name="sheet">How the image is cut into cells.</param>
    /// <param name="imageSize">The image's size in its own pixels.</param>
    /// <param name="tiles">The tiles cut from this sheet.</param>
    /// <param name="selected">The chosen cell, outlined if it is on this sheet, a tile or not.</param>
    public static DrawingImage Draw(TilesheetDefinition sheet, Size imageSize, IReadOnlyList<TileDefinition> tiles, SheetCell? selected)
    {
        var screenPixel = 1 / DisplayScaleOf(sheet);
        var gridPen = new Pen(_gridBrush, screenPixel);
        // Clipped to the image, so lines on its edges don't widen the drawing and shift it against the image.
        var drawing = new DrawingGroup { ClipGeometry = new RectangleGeometry(new Rect(imageSize)) };
        using (var context = drawing.Open())
        {
            // Pins the marks to the image's top-left, since a drawing's origin follows what it draws.
            context.DrawRectangle(Brushes.Transparent, null, new Rect(imageSize));
            for (var row = 0; sheet.CellTop(row) + sheet.TileHeight <= imageSize.Height; row++)
            {
                for (var column = 0; sheet.CellLeft(column) + sheet.TileWidth <= imageSize.Width; column++)
                {
                    context.DrawRectangle(null, gridPen, CellRect(sheet, column, row));
                }
            }

            var blockedPen = new Pen(_blockedBrush, 2 * screenPixel);
            foreach (var tile in tiles)
            {
                var tileRect = CellRect(sheet, tile.Column, tile.Row);
                context.DrawRectangle(_tileFill, null, tileRect);
                if (!tile.IsWalkable)
                {
                    context.DrawLine(blockedPen, tileRect.TopLeft, tileRect.BottomRight);
                    context.DrawLine(blockedPen, tileRect.TopRight, tileRect.BottomLeft);
                }
            }

            if (selected is { } cell && cell.SheetId == sheet.Id)
            {
                context.DrawRectangle(null, new Pen(Brushes.Cyan, 3 * screenPixel), CellRect(sheet, cell.Column, cell.Row));
            }
        }

        var image = new DrawingImage(drawing);
        image.Freeze();
        return image;
    }

    private static Rect CellRect(TilesheetDefinition sheet, int column, int row) =>
        new(sheet.CellLeft(column), sheet.CellTop(row), sheet.TileWidth, sheet.TileHeight);

    private static T Frozen<T>(T freezable)
        where T : Freezable
    {
        freezable.Freeze();
        return freezable;
    }
}
