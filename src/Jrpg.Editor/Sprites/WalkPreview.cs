using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Jrpg.Core.Geometry;
using Jrpg.Core.Sprites;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Sprites;

/// <summary>
/// The four facings walking side by side as the game draws them (ADR 0043): each frame at its record's scale over a 48
/// px cell outline, feet on the outline's bottom, so the feet line is set by eye. Reads Core's walk cell for the time,
/// as the game's player drawing does; one per window, remembering the cells drawn last so a render tick redraws only
/// when a frame moved on.
/// </summary>
internal sealed class WalkPreview
{
    private const int CellPixels = (int)MapDrawing.CellPixels;
    private const int Gap = 16;

    private static readonly Facing[] _facings = [Facing.Down, Facing.Left, Facing.Right, Facing.Up];
    private static readonly Pen _cellPen = FrozenPen(Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF));

    private readonly SpriteCell?[] _lastCells = new SpriteCell?[4];

    /// <summary>Whether some facing shows another frame now than at the last <see cref="Draw"/>.</summary>
    public bool HasMovedOn(FieldSpriteSheet sheet, TimeSpan elapsed)
    {
        for (var index = 0; index < _facings.Length; index++)
        {
            if (sheet.WalkCellAt(_facings[index], elapsed) != _lastCells[index])
            {
                return true;
            }
        }

        return false;
    }

    /// <param name="sheet">The record as set up, without a problem.</param>
    /// <param name="image">The sheet's image.</param>
    /// <param name="elapsed">The editor's animation time.</param>
    public DrawingImage Draw(FieldSpriteSheet sheet, BitmapSource image, TimeSpan elapsed)
    {
        var over = sheet.AreaOver(CellPixels);
        var column = Math.Max(CellPixels, over.Width) + Gap;
        var top = Math.Max(0, -over.Top);
        var drawing = new DrawingGroup();
        using (var context = drawing.Open())
        {
            context.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, (4 * column) - Gap, top + CellPixels + 1));
            for (var index = 0; index < _facings.Length; index++)
            {
                var cell = sheet.WalkCellAt(_facings[index], elapsed);
                _lastCells[index] = cell;
                var cellLeft = (index * column) + ((column - Gap - CellPixels) / 2);
                context.DrawRectangle(null, _cellPen, new Rect(cellLeft + 0.5, top + 0.5, CellPixels, CellPixels));
                var frame = new Int32Rect(sheet.FrameLeft(cell), sheet.FrameTop(cell), sheet.FrameWidth, sheet.FrameHeight);
                if (frame.X + frame.Width <= image.PixelWidth && frame.Y + frame.Height <= image.PixelHeight)
                {
                    var area = new Rect(cellLeft + over.Left, top + over.Top, over.Width, over.Height);
                    context.DrawImage(new CroppedBitmap(image, frame), area);
                }
            }
        }

        var picture = new DrawingImage(drawing);
        picture.Freeze();
        return picture;
    }

    private static Pen FrozenPen(Color color)
    {
        var pen = new Pen(new SolidColorBrush(color), 1);
        pen.Freeze();
        return pen;
    }
}
