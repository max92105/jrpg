using System.Windows;
using Jrpg.Core.Geometry;

namespace Jrpg.Editor.Viewport;

/// <summary>
/// Where the map sits in the editor's view: a zoom and the screen position of the map's top-left pixel. Pure, so the
/// pan and zoom rules are tested without a window (legacy MapEditor camera).
/// </summary>
internal sealed class ViewportTransform
{
    public const double MinZoom = 0.1;
    public const double MaxZoom = 8;

    // Fit leaves a margin around the map, as legacy did, so its edges are visible.
    private const double FitMargin = 0.95;

    public double Zoom { get; private set; } = 1;

    /// <summary>The screen position of map pixel (0, 0).</summary>
    public Point Origin { get; private set; }

    /// <summary>Zooms while keeping the map point under <paramref name="screen"/> in place.</summary>
    public void ZoomAt(Point screen, double zoom)
    {
        var mapPoint = ScreenToMap(screen);
        Zoom = Math.Clamp(zoom, MinZoom, MaxZoom);
        Origin = new Point(screen.X - (mapPoint.X * Zoom), screen.Y - (mapPoint.Y * Zoom));
    }

    public void PanBy(Vector screenDelta) => Origin += screenDelta;

    /// <summary>Shows the whole map centered, zooming out if needed but never past 100 %.</summary>
    /// <param name="map">The map's size in map pixels.</param>
    /// <param name="view">The view's size in screen pixels.</param>
    public void Fit(Size map, Size view)
    {
        Zoom = Math.Clamp(Math.Min(view.Width / map.Width, view.Height / map.Height) * FitMargin, MinZoom, 1);
        Origin = new Point((view.Width - (map.Width * Zoom)) / 2, (view.Height - (map.Height * Zoom)) / 2);
    }

    public Point ScreenToMap(Point screen) => new((screen.X - Origin.X) / Zoom, (screen.Y - Origin.Y) / Zoom);

    /// <summary>The cell under a screen point; it may lie outside the map.</summary>
    public GridCell ScreenToCell(Point screen, double cellPixels)
    {
        var map = ScreenToMap(screen);
        return GridCell.Containing(new Point2D(map.X / cellPixels, map.Y / cellPixels));
    }
}
