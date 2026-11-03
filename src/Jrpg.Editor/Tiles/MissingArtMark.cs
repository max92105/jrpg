using System.Windows;
using System.Windows.Media;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// What the editor draws for a tile or an NPC it can't show (decision 442): magenta with a dark diagonal hatch, one
/// cell, built once and frozen. A placeholder look, so it is never mistaken for art.
/// </summary>
internal static class MissingArtMark
{
    private const double Side = 16;

    public static ImageSource Picture { get; } = Build();

    private static DrawingImage Build()
    {
        var hatch = new GeometryGroup();
        for (var offset = -Side; offset <= Side; offset += Side / 2)
        {
            hatch.Children.Add(new LineGeometry(new Point(offset, Side), new Point(offset + Side, 0)));
        }

        var drawing = new DrawingGroup
        {
            ClipGeometry = new RectangleGeometry(new Rect(0, 0, Side, Side)),
            Children =
            {
                new GeometryDrawing(new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0xFF)), null, new RectangleGeometry(new Rect(0, 0, Side, Side))),
                new GeometryDrawing(null, new Pen(new SolidColorBrush(Color.FromRgb(0x40, 0x00, 0x40)), 2), hatch),
            },
        };
        var picture = new DrawingImage(drawing);
        picture.Freeze();
        return picture;
    }
}
