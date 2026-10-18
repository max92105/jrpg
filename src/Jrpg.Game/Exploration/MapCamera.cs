using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Which part of the map the screen shows. A map smaller than the screen sits centered; a larger one follows its target
/// but never shows past its edges (legacy <c>CameraManager</c>; following arrives with walking in part 3b).
/// </summary>
internal sealed class MapCamera
{
    /// <summary>The map pixel at the screen's top-left, in whole pixels so tiles never land between pixels and show seams.</summary>
    public Vector2 Origin { get; private set; }

    /// <summary>Jumps to show <paramref name="center"/> (in cells) without easing, as when a map is entered.</summary>
    public void SnapTo(Point2D center, MapLayout layout) =>
        Origin = new Vector2(
            AxisOrigin(center.X, layout.Width, VirtualScreen.Width),
            AxisOrigin(center.Y, layout.Height, VirtualScreen.Height));

    private static float AxisOrigin(double center, int mapCells, int screenPixels)
    {
        var mapPixels = mapCells * MapView.CellPixels;
        var origin = mapPixels <= screenPixels
            ? (mapPixels - screenPixels) / 2.0
            : Math.Clamp((center * MapView.CellPixels) - (screenPixels / 2.0), 0, mapPixels - screenPixels);
        return (float)Math.Round(origin);
    }
}
