using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Jrpg.Editor.Sprites;

/// <summary>
/// Drawn over a sprite sheet in the Character sprites page and the strip tool (decision 990): its frame grid, each
/// row's number from 1 on the left edge (decision 568) and each column's from 1 centered on the top edge (decision
/// 773), so rows and picked cells are read off the image. Line widths are set in screen pixels at the shown scale, as
/// the tile browser's marks are.
/// </summary>
internal static class SpriteSheetMarks
{
    private static readonly Brush _gridBrush = Frozen(new SolidColorBrush(Color.FromArgb(0x60, 0xFF, 0xFF, 0xFF)));
    private static readonly Brush _numberBrush = Frozen(new SolidColorBrush(Color.FromArgb(0xE0, 0xFF, 0xE0, 0x60)));
    private static readonly Typeface _typeface = new("Segoe UI");

    // Where a one-digit row number ends, in screen pixels: it starts 2 px in and is about 7 px wide.
    private const double RowNumberRight = 12;

    /// <param name="imageSize">The image's size in its own pixels.</param>
    /// <param name="frameWidth">A frame's width in image pixels; no grid when below 1.</param>
    /// <param name="frameHeight">A frame's height in image pixels; no grid when below 1.</param>
    /// <param name="scale">How much the view scales the image, so lines and numbers keep their screen size.</param>
    public static DrawingImage Draw((int Width, int Height) imageSize, int frameWidth, int frameHeight, double scale)
    {
        var screenPixel = 1 / scale;
        var area = new Rect(0, 0, imageSize.Width, imageSize.Height);
        var drawing = new DrawingGroup { ClipGeometry = new RectangleGeometry(area) };
        using (var context = drawing.Open())
        {
            // Pins the marks to the image's top-left, since a drawing's origin follows what it draws.
            context.DrawRectangle(Brushes.Transparent, null, area);
            if (frameWidth >= 1 && frameHeight >= 1)
            {
                DrawGrid(context, imageSize, (frameWidth, frameHeight), screenPixel);
                DrawColumnNumbers(context, imageSize.Width, frameWidth, screenPixel);
            }
        }

        var image = new DrawingImage(drawing);
        image.Freeze();
        return image;
    }

    /// <summary>
    /// The row whose number <see cref="Draw"/> writes under a point, from 0, for a click picking the whole row (the
    /// strip tool, decision 990): the number's own box, null elsewhere and for a partial row, which has no number.
    /// </summary>
    /// <param name="point">The point in image pixels.</param>
    /// <param name="imageHeight">The image's height in its own pixels.</param>
    /// <param name="frameHeight">A frame's height in image pixels; no row when below 1.</param>
    /// <param name="scale">How much the view scales the image, as the marks were drawn at.</param>
    public static int? RowAt(Point point, int imageHeight, int frameHeight, double scale)
    {
        if (frameHeight < 1 || point.Y < 0)
        {
            return null;
        }

        var row = (int)Math.Floor(point.Y / frameHeight);
        if (row >= imageHeight / frameHeight)
        {
            return null;
        }

        var screenPixel = 1 / scale;
        var number = NumberText(row + 1, screenPixel);
        var box = new Rect(RowNumberOrigin(row, frameHeight, screenPixel), new Size(number.Width, number.Height));
        return box.Contains(point) ? row : null;
    }

    // 2 px in from the left edge and 1 px down from the row's top, in screen pixels.
    private static Point RowNumberOrigin(int row, int frameHeight, double screenPixel) =>
        new(2 * screenPixel, (row * frameHeight) + screenPixel);

    private static void DrawGrid(
        DrawingContext context, (int Width, int Height) imageSize, (int Width, int Height) frame, double screenPixel)
    {
        var gridPen = new Pen(_gridBrush, screenPixel);
        for (var row = 0; (row + 1) * frame.Height <= imageSize.Height; row++)
        {
            for (var column = 0; (column + 1) * frame.Width <= imageSize.Width; column++)
            {
                var cell = new Rect(column * frame.Width, row * frame.Height, frame.Width, frame.Height);
                context.DrawRectangle(null, gridPen, cell);
            }

            context.DrawText(NumberText(row + 1, screenPixel), RowNumberOrigin(row, frame.Height, screenPixel));
        }
    }

    private static void DrawColumnNumbers(DrawingContext context, int imageWidth, int frameWidth, double screenPixel)
    {
        for (var column = 0; (column + 1) * frameWidth <= imageWidth; column++)
        {
            var number = NumberText(column + 1, screenPixel);
            var left = (column * frameWidth) + ((frameWidth - number.Width) / 2);

            // Clear of the first row's number at the left edge, which a small zoom would put under column 1's.
            context.DrawText(number, new Point(Math.Max(left, RowNumberRight * screenPixel), screenPixel));
        }
    }

    private static FormattedText NumberText(int number, double screenPixel) => new(
        number.ToString(CultureInfo.InvariantCulture),
        CultureInfo.InvariantCulture,
        FlowDirection.LeftToRight,
        _typeface,
        12 * screenPixel,
        _numberBrush,
        1);

    private static Brush Frozen(Brush brush)
    {
        brush.Freeze();
        return brush;
    }
}
