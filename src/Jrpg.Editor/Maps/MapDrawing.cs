using System.Windows;
using System.Windows.Media;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Maps;

/// <summary>
/// Draws a map as the game does (ADR 0012): tile layers, objects and their canopies in Core's <see cref="DrawOrderKey"/>
/// order, at the game's 48 pixels per cell. Canopies are the object's tiles again, clipped to its above-player zones, as
/// the legacy editor did.
/// </summary>
internal sealed class MapDrawing(CatalogDraft catalog, TileImages tileImages)
{
    /// <summary>Pixels per map cell at 100 % zoom, the same as the game's virtual screen.</summary>
    public const double CellPixels = 48;

    // Legacy "dim other layers": faint enough to tell apart, clear enough to paint against.
    private const double DimmedOpacity = 0.3;

    // Faint enough to read the tiles through.
    private static readonly SolidColorBrush _gridBrush = CreateGridBrush();

    private static readonly IComparer<MapPart> _byDrawOrder = Comparer<MapPart>.Create((left, right) => left.Key.CompareTo(right.Key));

    /// <param name="layout">The map to draw.</param>
    /// <param name="focusZ">When set, everything on another Z is drawn faint, to show what the layer at this Z holds.</param>
    public DrawingImage Draw(MapLayout layout, int? focusZ)
    {
        // Clipped to the map, as the game's camera shows it; objects may reach past an edge, and an image's origin
        // follows what it draws, so the transparent fill also pins cell (0, 0) to the image's top-left.
        var mapArea = new Rect(0, 0, layout.Width * CellPixels, layout.Height * CellPixels);
        var drawing = new DrawingGroup { ClipGeometry = new RectangleGeometry(mapArea) };
        using (var context = drawing.Open())
        {
            context.DrawRectangle(Brushes.Transparent, null, mapArea);
            foreach (var part in PartsInDrawOrder(layout))
            {
                var isDimmed = focusZ is { } z && part.Z != z;
                if (isDimmed)
                {
                    context.PushOpacity(DimmedOpacity);
                }

                DrawPart(context, layout, part);
                if (isDimmed)
                {
                    context.Pop();
                }
            }
        }

        var image = new DrawingImage(drawing);
        image.Freeze();
        return image;
    }

    /// <summary>One object on its own, at the map's scale: the browser's preview and the outline of where it would go.</summary>
    public DrawingImage DrawObject(ObjectDefinition definition)
    {
        var area = new Rect(0, 0, definition.Width * CellPixels, definition.Height * CellPixels);
        var drawing = new DrawingGroup { ClipGeometry = new RectangleGeometry(area) };
        using (var context = drawing.Open())
        {
            context.DrawRectangle(Brushes.Transparent, null, area);
            foreach (var tile in definition.Tiles)
            {
                context.DrawImage(tileImages.TileImageOf(tile.TileId), CellRect(tile.X, tile.Y));
            }
        }

        var image = new DrawingImage(drawing);
        image.Freeze();
        return image;
    }

    /// <summary>Lines between the map's cells, drawn over the map when the grid is shown.</summary>
    /// <param name="layout">The map whose cells are outlined.</param>
    /// <param name="lineWidth">In map pixels; one screen pixel is 1 / zoom, so the lines stay visible when zoomed out.</param>
    public static DrawingImage DrawGrid(MapLayout layout, double lineWidth)
    {
        var width = layout.Width * CellPixels;
        var height = layout.Height * CellPixels;
        var lines = new GeometryGroup();
        for (var x = 0; x <= layout.Width; x++)
        {
            lines.Children.Add(new LineGeometry(new Point(x * CellPixels, 0), new Point(x * CellPixels, height)));
        }

        for (var y = 0; y <= layout.Height; y++)
        {
            lines.Children.Add(new LineGeometry(new Point(0, y * CellPixels), new Point(width, y * CellPixels)));
        }

        var image = new DrawingImage(new GeometryDrawing(null, new Pen(_gridBrush, lineWidth), lines));
        image.Freeze();
        return image;
    }

    private List<MapPart> PartsInDrawOrder(MapLayout layout)
    {
        var parts = new List<MapPart>();
        for (var index = 0; index < layout.Layers.Count; index++)
        {
            parts.Add(new MapPart(DrawOrderKey.ForTile(layout.Layers[index].Z, index), layout.Layers[index].Z, MapPartKind.Layer, index));
        }

        for (var index = 0; index < layout.Objects.Count; index++)
        {
            var placed = layout.Objects[index];
            var definition = catalog.Current.Objects[placed.ObjectId];
            var baseY = placed.Position.Y + definition.Height;
            parts.Add(new MapPart(DrawOrderKey.ForSprite(placed.Z, baseY, index), placed.Z, MapPartKind.Object, index));

            var canopyZ = MapLayers.CanopyZOf(placed.Z);
            if (definition.AbovePlayerZones.Count > 0 && canopyZ > placed.Z)
            {
                parts.Add(new MapPart(DrawOrderKey.ForSprite(canopyZ, baseY, index), canopyZ, MapPartKind.Canopy, index));
            }
        }

        parts.Sort(_byDrawOrder);
        return parts;
    }

    private void DrawPart(DrawingContext context, MapLayout layout, MapPart part)
    {
        switch (part.Kind)
        {
            case MapPartKind.Layer:
                DrawLayer(context, layout, layout.Layers[part.Index]);
                break;
            case MapPartKind.Object:
                DrawObject(context, layout.Objects[part.Index]);
                break;
            case MapPartKind.Canopy:
                DrawCanopy(context, layout.Objects[part.Index]);
                break;
        }
    }

    private void DrawLayer(DrawingContext context, MapLayout layout, TileLayer layer)
    {
        for (var y = 0; y < layout.Height; y++)
        {
            for (var x = 0; x < layout.Width; x++)
            {
                var tileId = layer.Rows[y][x];
                if (tileId != TileLayer.EmptyTileId)
                {
                    context.DrawImage(tileImages.TileImageOf(tileId), CellRect(x, y));
                }
            }
        }
    }

    private void DrawObject(DrawingContext context, PlacedObject placed)
    {
        foreach (var tile in catalog.Current.Objects[placed.ObjectId].Tiles)
        {
            context.DrawImage(tileImages.TileImageOf(tile.TileId), CellRect(placed.Position.X + tile.X, placed.Position.Y + tile.Y));
        }
    }

    private void DrawCanopy(DrawingContext context, PlacedObject placed)
    {
        // One clip for all zones (nonzero fill: their union), so overlapping zones draw each pixel once, as the game's mask does.
        var definition = catalog.Current.Objects[placed.ObjectId];
        var zones = new GeometryGroup { FillRule = FillRule.Nonzero };
        foreach (var zone in definition.AbovePlayerZones)
        {
            // Zone points are 0..1 over the object's cells, like its collision outline.
            var outline = zone.Select(point => new Point(
                (placed.Position.X + (point.X * definition.Width)) * CellPixels,
                (placed.Position.Y + (point.Y * definition.Height)) * CellPixels)).ToArray();
            zones.Children.Add(new PathGeometry([new PathFigure(outline[0], [new PolyLineSegment(outline.Skip(1), isStroked: false)], closed: true)]));
        }

        context.PushClip(zones);
        DrawObject(context, placed);
        context.Pop();
    }

    private static SolidColorBrush CreateGridBrush()
    {
        var brush = new SolidColorBrush(Color.FromArgb(0x60, 0xFF, 0xFF, 0xFF));
        brush.Freeze();
        return brush;
    }

    private static Rect CellRect(double x, double y) => new(x * CellPixels, y * CellPixels, CellPixels, CellPixels);

    private enum MapPartKind
    {
        Layer,
        Object,
        Canopy,
    }

    private readonly record struct MapPart(DrawOrderKey Key, int Z, MapPartKind Kind, int Index);
}
