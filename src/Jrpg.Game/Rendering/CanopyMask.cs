using Jrpg.Core.Geometry;

namespace Jrpg.Game.Rendering;

/// <summary>Which pixels of an object's picture lie inside its above-player zones, so only those are drawn over the player.</summary>
internal static class CanopyMask
{
    /// <summary>
    /// One flag per pixel, row by row: whether the pixel's center lies inside any zone. Zones use the object's 0..1
    /// coordinates, so the same outline fits whatever the picture's size.
    /// </summary>
    public static bool[] Build(IReadOnlyList<IReadOnlyList<Point2D>> zones, int width, int height)
    {
        var mask = new bool[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                mask[(y * width) + x] = IsInsideAny(zones, new Point2D((x + 0.5) / width, (y + 0.5) / height));
            }
        }

        return mask;
    }

    // A loop rather than Any, which would allocate a closure for each of a house's hundred thousand pixels.
    private static bool IsInsideAny(IReadOnlyList<IReadOnlyList<Point2D>> zones, Point2D point)
    {
        for (var index = 0; index < zones.Count; index++)
        {
            if (Polygon.Contains(zones[index], point))
            {
                return true;
            }
        }

        return false;
    }
}
