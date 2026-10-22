using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Which part of the map the screen shows. A map smaller than the screen sits centered; a larger one follows its target
/// but never shows past its edges.
/// </summary>
internal sealed class MapCamera
{
    // The share is defined per 60 Hz tick and applied per that tick, so the feel doesn't depend on the frame rate.
    private const double TicksPerSecond = 60;

    // Kept unrounded so small easing steps add up instead of rounding away.
    private double _originX;
    private double _originY;

    /// <summary>The map pixel at the screen's top-left, in whole pixels so tiles never land between pixels and show seams.</summary>
    public Vector2 Origin => new((float)Math.Round(_originX), (float)Math.Round(_originY));

    /// <summary>Jumps to show <paramref name="center"/> (in cells) without easing, as when a map is entered.</summary>
    public void SnapTo(Point2D center, MapLayout layout)
    {
        _originX = AxisTarget(center.X, layout.Width, VirtualScreen.Width);
        _originY = AxisTarget(center.Y, layout.Height, VirtualScreen.Height);
    }

    /// <summary>
    /// Eases toward showing <paramref name="target"/> (in cells): each 1/60 s closes <paramref name="sharePerTick"/> of
    /// the gap, so over <paramref name="delta"/> it closes <c>1 - (1 - share)^(delta × 60)</c>, and two half ticks
    /// equal one tick (developer decision 86). The share is 0..1.
    /// </summary>
    public void Follow(Point2D target, MapLayout layout, TimeSpan delta, double sharePerTick)
    {
        var closed = 1 - Math.Pow(1 - sharePerTick, delta.TotalSeconds * TicksPerSecond);
        _originX += (AxisTarget(target.X, layout.Width, VirtualScreen.Width) - _originX) * closed;
        _originY += (AxisTarget(target.Y, layout.Height, VirtualScreen.Height) - _originY) * closed;
    }

    private static double AxisTarget(double center, int mapCells, int screenPixels)
    {
        var mapPixels = mapCells * MapView.CellPixels;
        return mapPixels <= screenPixels
            ? (mapPixels - screenPixels) / 2.0
            : Math.Clamp((center * MapView.CellPixels) - (screenPixels / 2.0), 0, mapPixels - screenPixels);
    }
}
