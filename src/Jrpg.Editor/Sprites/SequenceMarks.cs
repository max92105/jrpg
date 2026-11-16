using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Jrpg.Core.Sprites;

namespace Jrpg.Editor.Sprites;

/// <summary>
/// Drawn over a sprite sheet in the Character sprites page (decision 773): the picking facing's cells, each outlined
/// and numbered in pick order at its frame's bottom-right, a repeated cell showing each of its numbers ("2, 4"). Lines
/// and numbers keep their screen size at the shown scale, as <see cref="SpriteSheetMarks"/> does.
/// </summary>
internal static class SequenceMarks
{
    /// <summary>A pick number's size in screen pixels, as the row numbers are.</summary>
    public const double PickNumberPixels = 12;

    private static readonly Brush _pickBrush = Frozen(new SolidColorBrush(Color.FromArgb(0xF0, 0xFF, 0xD4, 0x5A)));
    private static readonly Typeface _typeface =
        new(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);

    /// <param name="imageSize">The image's size in its own pixels.</param>
    /// <param name="frame">A frame's size in image pixels, both at least 1.</param>
    /// <param name="cells">The picking facing's cells in pick order.</param>
    /// <param name="scale">How much the view scales the image, so lines and numbers keep their screen size.</param>
    public static DrawingImage Draw(
        (int Width, int Height) imageSize, (int Width, int Height) frame, IReadOnlyList<SpriteCell> cells, double scale)
    {
        var screenPixel = 1 / scale;
        var area = new Rect(0, 0, imageSize.Width, imageSize.Height);
        var drawing = new DrawingGroup { ClipGeometry = new RectangleGeometry(area) };
        using (var context = drawing.Open())
        {
            // Pins the marks to the image's top-left, since a drawing's origin follows what it draws.
            context.DrawRectangle(Brushes.Transparent, null, area);
            var outline = new Pen(_pickBrush, 2 * screenPixel);
            foreach (var picked in cells.Select((cell, index) => (cell, number: index + 1)).GroupBy(pick => pick.cell))
            {
                var box = new Rect(picked.Key.Column * frame.Width, picked.Key.Row * frame.Height, frame.Width, frame.Height);
                context.DrawRectangle(null, outline, box);
                DrawNumbers(context, string.Join(", ", picked.Select(pick => pick.number)), box, screenPixel);
            }
        }

        var image = new DrawingImage(drawing);
        image.Freeze();
        return image;
    }

    private static void DrawNumbers(DrawingContext context, string numbers, Rect box, double screenPixel)
    {
        var text = new FormattedText(
            numbers,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            _typeface,
            PickNumberPixels * screenPixel,
            _pickBrush,
            1);
        context.DrawText(text, new Point(box.Right - text.Width - (3 * screenPixel), box.Bottom - text.Height - screenPixel));
    }

    private static Brush Frozen(Brush brush)
    {
        brush.Freeze();
        return brush;
    }
}
