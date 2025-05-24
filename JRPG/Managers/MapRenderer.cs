using JRPG.Core.Maps;
using JRPG.Core.Maps.IO;
using JRPG.Core.Maps.Models;
using JRPG.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JRPG.Managers
{
    public class MapRenderer
    {
        public VisualHost VisualHost { get; }
        private DrawingVisual mapVisual;

        private readonly Dictionary<string, BitmapImage> imageCache = new Dictionary<string, BitmapImage>();
        private double tileAnimationTimer = 0;
        private int tileAnimationFrame = 0;

        private double lastCameraX = -1;
        private double lastCameraY = -1;
        private int lastRedrawnTileFrame = -1;

        private readonly Canvas canvas;

        public MapRenderer(Canvas canvas)
        {
            this.canvas = canvas;

            mapVisual = new DrawingVisual();
            VisualHost = new VisualHost();
            VisualHost.SetVisual(mapVisual);

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

        public void RedrawIfNeeded(double cameraX, double cameraY, Map map)
        {
            bool cameraMoved = Math.Abs(cameraX - lastCameraX) > 0.01 || Math.Abs(cameraY - lastCameraY) > 0.01;
            bool tileFrameChanged = tileAnimationFrame != lastRedrawnTileFrame;

            if (cameraMoved || tileFrameChanged)
            {
                Redraw(cameraX, cameraY, map);
                lastCameraX = cameraX;
                lastCameraY = cameraY;
                lastRedrawnTileFrame = tileAnimationFrame;
            }
        }

        public void Redraw(double cameraX, double cameraY, Map map)
        {
            using (var dc = mapVisual.RenderOpen())
            {
                double time = DateTime.Now.TimeOfDay.TotalSeconds;

                int tilesVisibleX = (int)Math.Ceiling(canvas.ActualWidth / MapConstant.TileSize);
                int tilesVisibleY = (int)Math.Ceiling(canvas.ActualHeight / MapConstant.TileSize);
                int startX = (int)Math.Floor(cameraX);
                int startY = (int)Math.Floor(cameraY);

                // Draw tiles
                for (int y = 0; y < tilesVisibleY + 2; y++)
                {
                    for (int x = 0; x < tilesVisibleX + 2; x++)
                    {
                        int mapX = x + startX;
                        int mapY = y + startY;

                        if (mapX < 0 || mapY < 0 || mapX >= map.Width || mapY >= map.Height)
                            continue;

                        int tileId = map.GetTileIdAt(mapX, mapY);
                        if (!TileDatabase.Definitions.TryGetValue(tileId, out var def)) continue;

                        ImageSource imgSource = null;

                        if (def.Frames != null && def.Frames.Length > 0)
                        {
                            int frameIndex = (int)(time / def.FrameDuration) % def.Frames.Length;
                            string framePath = def.Frames[frameIndex];
                            imgSource = GetCachedImage(framePath);
                        }
                        else if (!string.IsNullOrEmpty(def.Sprite))
                        {
                            imgSource = GetCachedImage(def.Sprite);
                        }

                        if (imgSource != null)
                        {
                            double drawX = (mapX - cameraX) * MapConstant.TileSize;
                            double drawY = (mapY - cameraY) * MapConstant.TileSize;
                            dc.DrawImage(imgSource, new Rect(drawX, drawY, MapConstant.TileSize, MapConstant.TileSize));
                        }
                    }
                }

                // Draw objects
                foreach (var obj in map.Objects)
                {
                    if (!map.ObjectDefinitions.TryGetValue(obj.Id, out var def)) continue;
                    if (string.IsNullOrEmpty(def.SpritePath)) continue;

                    var img = GetCachedImage(def.SpritePath);
                    double screenX = (obj.X - 0.5 - cameraX) * MapConstant.TileSize;
                    double screenY = (obj.Y - 0.5 - cameraY) * MapConstant.TileSize;

                    if (screenX + MapConstant.TileSize < 0 || screenX > canvas.ActualWidth ||
                        screenY + MapConstant.TileSize < 0 || screenY > canvas.ActualHeight)
                        continue;

                    dc.DrawImage(img, new Rect(screenX, screenY, MapConstant.TileSize, MapConstant.TileSize));
                }
            }
        }

        private BitmapImage GetCachedImage(string path)
        {
            if (imageCache.TryGetValue(path, out var cached))
                return cached;

            var bitmap = new BitmapImage(new Uri(Path.Combine(AssetPathResolver.GetAssetsRootPath(), path)));
            imageCache[path] = bitmap;
            return bitmap;
        }
    }
}
