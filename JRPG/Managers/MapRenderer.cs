using JRPG.Core.Maps;
using JRPG.Core.Maps.IO;
using JRPG.Core.Maps.Models;
using JRPG.Core.Models;
using JRPG.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace JRPG.Managers
{
    public class MapRenderer
    {
        private struct Drawable
        {
            public int Z;
            public double DrawY; // Used for sorting (vertical position)
            public Action<DrawingContext> DrawAction;
        }

        public VisualHost VisualHost { get; }
        private DrawingVisual tileVisual;
        private DrawingVisual objectVisual;

        private readonly Dictionary<string, BitmapImage> imageCache = new Dictionary<string, BitmapImage>();
        private double tileAnimationTimer = 0;
        private int tileAnimationFrame = 0;

        private double lastCameraX = -1;
        private double lastCameraY = -1;

        private int lastTileCameraX = -1;
        private int lastTileCameraY = -1;

        private int lastRedrawnTileFrame = -1;

        private readonly Canvas canvas;

        public MapRenderer(Canvas canvas)
        {
            this.canvas = canvas;

            tileVisual = new DrawingVisual();
            objectVisual = new DrawingVisual();

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
            var drawables = new List<Drawable>();

            AddTileDrawables(drawables, cameraX, cameraY, map);
            AddPlayerDrawable(drawables, cameraX, cameraY, playerX, playerY, playerFrame);
            AddObjectDrawables(drawables, cameraX, cameraY, map);

            // Sort all by vertical Y and draw
            using (var dc = tileVisual.RenderOpen())
            {
                foreach (var d in drawables.OrderBy(d => d.Z).ThenBy(d => d.DrawY))
                    d.DrawAction(dc);
            }
        }

        private void AddTileDrawables(List<Drawable> drawables, double cameraX, double cameraY, Map map)
        {
            int tilesVisibleX = (int)Math.Ceiling(canvas.ActualWidth / MapConstant.TileSize);
            int tilesVisibleY = (int)Math.Ceiling(canvas.ActualHeight / MapConstant.TileSize);
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
                        var img = GetTileImage(def);
                        if (img == null) continue;

                        drawables.Add(new Drawable
                        {
                            Z = tile.Z,
                            DrawY = drawY + MapConstant.TileSize,
                            DrawAction = dc => dc.DrawImage(img, new Rect(drawX, drawY, MapConstant.TileSize, MapConstant.TileSize))
                        });
                    }
                }
            }
        }

        private void AddPlayerDrawable(List<Drawable> drawables, double cameraX, double cameraY, double playerX, double playerY, CroppedBitmap frame)
        {
            double drawX = (playerX - cameraX) * MapConstant.TileSize - (MapConstant.TileSize / 2);
            double drawY = (playerY - cameraY) * MapConstant.TileSize - (MapConstant.TileSize / 2);

            drawables.Add(new Drawable
            {
                Z = 1,
                DrawY = drawY + MapConstant.TileSize,
                DrawAction = dc => dc.DrawImage(frame, new Rect(drawX, drawY, MapConstant.TileSize, MapConstant.TileSize))
            });
        }

        private void AddObjectDrawables(List<Drawable> drawables, double cameraX, double cameraY, Map map)
        {
            foreach (var obj in map.Objects)
            {
                if (!ObjectDatabase.Definitions.TryGetValue(obj.Id, out var def) || def.Tiles == null) continue;
                int minX = def.Tiles.Min(t => t.X);
                int minY = def.Tiles.Min(t => t.Y);

                foreach (var entry in def.Tiles)
                {
                    if (!TileDatabase.Definitions.TryGetValue(entry.TileId, out var tileDef)) continue;
                    var img = GetTileImage(tileDef);
                    if (img == null) continue;

                    double drawX = (obj.X + (entry.X - minX) - cameraX) * MapConstant.TileSize;
                    double drawY = (obj.Y + (entry.Y - minY) - cameraY) * MapConstant.TileSize;

                    drawables.Add(new Drawable
                    {
                        Z = obj.Z,
                        DrawY = drawY + MapConstant.TileSize,
                        DrawAction = dc => dc.DrawImage(img, new Rect(drawX, drawY, MapConstant.TileSize, MapConstant.TileSize))
                    });
                }
            }
        }

        private ImageSource GetTileImage(TileDefinition def)
        {
            if (!string.IsNullOrEmpty(def.Tilesheet) && def.SheetX.HasValue && def.SheetY.HasValue)
            {
                var fullSheet = GetCachedImage(def.Tilesheet);
                if (fullSheet == null) return null;

                var rect = new Int32Rect(
                    def.SheetX.Value * MapConstant.TileSize,
                    def.SheetY.Value * MapConstant.TileSize,
                    MapConstant.TileSize,
                    MapConstant.TileSize
                );

                return new CroppedBitmap(fullSheet, rect);
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
