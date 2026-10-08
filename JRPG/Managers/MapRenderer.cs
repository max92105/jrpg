using JRPG.Core.Maps;
using JRPG.Core.Maps.IO;
using JRPG.Core.Maps.Models;
using JRPG.Core.Models;
using JRPG.Core.Utils;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JRPG.Managers
{
    public class MapRenderer
    {
        private struct Drawable
        {
            public int Z;
            public double DrawY; // Used for sorting (vertical position)
            public int Order;    // Insertion order, keeps the sort stable
            public ImageSource Image;
            public Rect Rect;
        }

        private static readonly Comparison<Drawable> DrawOrder = (a, b) =>
        {
            int cmp = a.Z.CompareTo(b.Z);
            if (cmp != 0) return cmp;
            cmp = a.DrawY.CompareTo(b.DrawY);
            return cmp != 0 ? cmp : a.Order.CompareTo(b.Order);
        };

        public VisualHost VisualHost { get; }
        private DrawingVisual tileVisual;
        private DrawingVisual objectVisual;

        private readonly Dictionary<string, BitmapImage> imageCache = new Dictionary<string, BitmapImage>();
        private readonly Dictionary<int, ImageSource> tileImageCache = new Dictionary<int, ImageSource>();
        private readonly List<Drawable> drawables = new List<Drawable>();

        private double tileAnimationTimer = 0;
        private int tileAnimationFrame = 0;

        // State of the last redraw, used to skip frames where nothing changed
        private double lastCameraX = double.NaN;
        private double lastCameraY = double.NaN;
        private double lastPlayerX = double.NaN;
        private double lastPlayerY = double.NaN;
        private double lastViewWidth = -1;
        private double lastViewHeight = -1;
        private ImageSource lastPlayerFrame;
        private Map lastMap;
        private int lastRedrawnTileFrame = -1;

        private readonly Canvas canvas;
        private double dpiScale = 1.0;

        public MapRenderer(Canvas canvas)
        {
            this.canvas = canvas;

            tileVisual = new DrawingVisual();
            objectVisual = new DrawingVisual();

            // Pixel-art friendly: no smoothing when scaled (smoothing blends tile borders with
            // transparent pixels) and no anti-aliased edges between neighbouring tiles
            foreach (var visual in new[] { tileVisual, objectVisual })
            {
                RenderOptions.SetBitmapScalingMode(visual, BitmapScalingMode.NearestNeighbor);
                RenderOptions.SetEdgeMode(visual, EdgeMode.Aliased);
            }

            VisualHost = new VisualHost();
            VisualHost.SetVisuals(new[] { tileVisual, objectVisual });

            VisualHost.Width = canvas.ActualWidth;
            VisualHost.Height = canvas.ActualHeight;

            VisualHost.HorizontalAlignment = HorizontalAlignment.Left;
            VisualHost.VerticalAlignment = VerticalAlignment.Top;
        }

        public void Update(double deltaTime)
        {
            tileAnimationTimer += deltaTime;
            if (tileAnimationTimer >= 0.2)
            {
                tileAnimationFrame++;
                tileAnimationTimer = 0;
            }
        }

        public void Redraw(double cameraX, double cameraY, Map map, double playerX, double playerY, CroppedBitmap playerFrame)
        {
            double viewWidth = canvas.ActualWidth;
            double viewHeight = canvas.ActualHeight;

            if (cameraX == lastCameraX && cameraY == lastCameraY &&
                playerX == lastPlayerX && playerY == lastPlayerY &&
                viewWidth == lastViewWidth && viewHeight == lastViewHeight &&
                ReferenceEquals(playerFrame, lastPlayerFrame) &&
                ReferenceEquals(map, lastMap) &&
                tileAnimationFrame == lastRedrawnTileFrame)
                return;

            lastCameraX = cameraX;
            lastCameraY = cameraY;
            lastPlayerX = playerX;
            lastPlayerY = playerY;
            lastViewWidth = viewWidth;
            lastViewHeight = viewHeight;
            lastPlayerFrame = playerFrame;
            lastMap = map;
            lastRedrawnTileFrame = tileAnimationFrame;

            drawables.Clear();
            dpiScale = VisualTreeHelper.GetDpi(VisualHost).DpiScaleX;

            AddTileDrawables(cameraX, cameraY, map, viewWidth, viewHeight);
            AddPlayerDrawable(cameraX, cameraY, playerX, playerY, playerFrame);
            AddObjectDrawables(cameraX, cameraY, map, viewWidth, viewHeight);

            // Sort all by Z then vertical Y and draw
            drawables.Sort(DrawOrder);

            using (var dc = tileVisual.RenderOpen())
            {
                foreach (var d in drawables)
                    dc.DrawImage(d.Image, d.Rect);
            }
        }

        private void AddDrawable(int z, ImageSource image, double drawX, double drawY)
        {
            // Snap to whole device pixels: a tile at a fractional position gets its edges
            // anti-aliased, which shows up as thin lines between tiles while the camera moves.
            // Rounding in device pixels (not DIPs) keeps this correct with Windows display scaling.
            drawX = Math.Round(drawX * dpiScale) / dpiScale;
            drawY = Math.Round(drawY * dpiScale) / dpiScale;

            drawables.Add(new Drawable
            {
                Z = z,
                DrawY = drawY + MapConstant.TileSize,
                Order = drawables.Count,
                Image = image,
                Rect = new Rect(drawX, drawY, MapConstant.TileSize, MapConstant.TileSize)
            });
        }

        private void AddTileDrawables(double cameraX, double cameraY, Map map, double viewWidth, double viewHeight)
        {
            int tilesVisibleX = (int)Math.Ceiling(viewWidth / MapConstant.TileSize);
            int tilesVisibleY = (int)Math.Ceiling(viewHeight / MapConstant.TileSize);
            int startX = (int)Math.Floor(cameraX);
            int startY = (int)Math.Floor(cameraY);

            for (int y = 0; y < tilesVisibleY + 2; y++)
            {
                for (int x = 0; x < tilesVisibleX + 2; x++)
                {
                    int mapX = x + startX;
                    int mapY = y + startY;
                    if (mapX < 0 || mapY < 0 || mapX >= map.Width || mapY >= map.Height)
                        continue;

                    double drawX = (mapX - cameraX) * MapConstant.TileSize;
                    double drawY = (mapY - cameraY) * MapConstant.TileSize;

                    foreach (var tile in map.GetTilesAt(mapX, mapY))
                    {
                        if (!TileDatabase.Definitions.TryGetValue(tile.Id, out var def)) continue;
                        var img = GetTileImage(tile.Id, def);
                        if (img == null) continue;

                        AddDrawable(tile.Z, img, drawX, drawY);
                    }
                }
            }
        }

        private void AddPlayerDrawable(double cameraX, double cameraY, double playerX, double playerY, CroppedBitmap frame)
        {
            double drawX = (playerX - cameraX) * MapConstant.TileSize - (MapConstant.TileSize / 2);
            double drawY = (playerY - cameraY) * MapConstant.TileSize - (MapConstant.TileSize / 2);

            AddDrawable(1, frame, drawX, drawY);
        }

        private void AddObjectDrawables(double cameraX, double cameraY, Map map, double viewWidth, double viewHeight)
        {
            foreach (var obj in map.Objects)
            {
                if (!ObjectDatabase.Definitions.TryGetValue(obj.Id, out var def) || def.Tiles == null || def.Tiles.Count == 0) continue;

                int minX = int.MaxValue;
                int minY = int.MaxValue;
                foreach (var entry in def.Tiles)
                {
                    if (entry.X < minX) minX = entry.X;
                    if (entry.Y < minY) minY = entry.Y;
                }

                foreach (var entry in def.Tiles)
                {
                    double drawX = (obj.X + (entry.X - minX) - cameraX) * MapConstant.TileSize;
                    double drawY = (obj.Y + (entry.Y - minY) - cameraY) * MapConstant.TileSize;

                    // Skip tiles outside the view
                    if (drawX + MapConstant.TileSize < 0 || drawY + MapConstant.TileSize < 0 || drawX > viewWidth || drawY > viewHeight)
                        continue;

                    if (!TileDatabase.Definitions.TryGetValue(entry.TileId, out var tileDef)) continue;
                    var img = GetTileImage(entry.TileId, tileDef);
                    if (img == null) continue;

                    // ✅ Override key
                    int z = obj.Z;
                    if (obj.ZOverrides != null && obj.ZOverrides.Count > 0 &&
                        obj.ZOverrides.TryGetValue($"{entry.X},{entry.Y}", out int overrideZ))
                    {
                        z = overrideZ;
                    }

                    AddDrawable(z, img, drawX, drawY);
                }
            }
        }

        private ImageSource GetTileImage(int tileId, TileDefinition def)
        {
            if (!string.IsNullOrEmpty(def.Tilesheet) && def.SheetX.HasValue && def.SheetY.HasValue)
            {
                if (tileImageCache.TryGetValue(tileId, out var cachedTile))
                    return cachedTile;

                var fullSheet = GetCachedImage(def.Tilesheet);
                if (fullSheet == null) return null;

                var rect = new Int32Rect(
                    def.SheetX.Value * MapConstant.TileSize,
                    def.SheetY.Value * MapConstant.TileSize,
                    MapConstant.TileSize,
                    MapConstant.TileSize
                );

                var cropped = new CroppedBitmap(fullSheet, rect);
                cropped.Freeze();

                tileImageCache[tileId] = cropped;
                return cropped;
            }

            if (def.Frames != null && def.Frames.Length > 0)
            {
                int frameIndex = tileAnimationFrame % def.Frames.Length;
                return GetCachedImage(def.Frames[frameIndex]);
            }

            if (!string.IsNullOrEmpty(def.Sprite))
            {
                return GetCachedImage(def.Sprite);
            }

            return null;
        }

        private BitmapImage GetCachedImage(string path)
        {
            if (imageCache.TryGetValue(path, out var cached))
                return cached;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(AssetPathResolver.GetTilePath(path), UriKind.Absolute);
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.EndInit();
            bitmap.Freeze();

            imageCache[path] = bitmap;
            return bitmap;
        }
    }
}
