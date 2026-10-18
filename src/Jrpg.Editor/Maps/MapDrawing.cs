using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;

namespace Jrpg.Editor.Maps;

/// <summary>
/// Draws a map as the game does (ADR 0012): tile layers, objects and their canopies in Core's <see cref="DrawOrderKey"/>
/// order, at the game's 48 pixels per cell. Canopies are the object's tiles again, clipped to its above-player zones, as
/// the legacy editor did. Tile images are cut once and kept for the editor's lifetime.
/// </summary>
internal sealed class MapDrawing(ContentCatalog catalog, string artRoot)
{
    /// <summary>Pixels per map cell at 100 % zoom, the same as the game's virtual screen.</summary>
    public const double CellPixels = 48;

    // Faint enough to read the tiles through.
    private static readonly SolidColorBrush _gridBrush = CreateGridBrush();

    private static readonly IComparer<MapPart> _byDrawOrder = Comparer<MapPart>.Create((left, right) => left.Key.CompareTo(right.Key));

    private readonly Dictionary<string, BitmapSource> _sheets = [];
    private readonly Dictionary<int, ImageSource> _tiles = [];

    public DrawingImage Draw(MapLayout layout)
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
                DrawPart(context, layout, part);
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
            parts.Add(new MapPart(DrawOrderKey.ForTile(layout.Layers[index].Z, index), MapPartKind.Layer, index));
        }

        for (var index = 0; index < layout.Objects.Count; index++)
        {
            var placed = layout.Objects[index];
            var definition = catalog.Objects[placed.ObjectId];
            var baseY = placed.Position.Y + definition.Height;
            parts.Add(new MapPart(DrawOrderKey.ForSprite(placed.Z, baseY, index), MapPartKind.Object, index));

            var canopyZ = MapLayers.CanopyZOf(placed.Z);
            if (definition.AbovePlayerZones.Count > 0 && canopyZ > placed.Z)
            {
                parts.Add(new MapPart(DrawOrderKey.ForSprite(canopyZ, baseY, index), MapPartKind.Canopy, index));
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
                    context.DrawImage(TileImageOf(tileId), CellRect(x, y));
                }
            }
        }
    }

    private void DrawObject(DrawingContext context, PlacedObject placed)
    {
        foreach (var tile in catalog.Objects[placed.ObjectId].Tiles)
        {
            context.DrawImage(TileImageOf(tile.TileId), CellRect(placed.Position.X + tile.X, placed.Position.Y + tile.Y));
        }
    }

    private void DrawCanopy(DrawingContext context, PlacedObject placed)
    {
        // One clip for all zones (nonzero fill: their union), so overlapping zones draw each pixel once, as the game's mask does.
        var definition = catalog.Objects[placed.ObjectId];
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

    private ImageSource TileImageOf(int tileId)
    {
        if (!_tiles.TryGetValue(tileId, out var image))
        {
            var tile = catalog.Tiles[tileId];
            var sheet = catalog.Tilesheets[tile.SheetId];
            var cell = new Int32Rect(sheet.CellLeft(tile.Column), sheet.CellTop(tile.Row), sheet.TileWidth, sheet.TileHeight);
            image = new CroppedBitmap(SheetImageOf(sheet.File), cell);
            image.Freeze();
            _tiles.Add(tileId, image);
        }

        return image;
    }

    private BitmapSource SheetImageOf(string file)
    {
        if (!_sheets.TryGetValue(file, out var sheet))
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(Path.Combine(artRoot, ArtFolders.Tilesheets, file));
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            sheet = bitmap;
            _sheets.Add(file, sheet);
        }

        return sheet;
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

    private readonly record struct MapPart(DrawOrderKey Key, MapPartKind Kind, int Index);
}
