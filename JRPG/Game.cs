using JRPG.Core.Enums;
using JRPG.Core.Maps;
using JRPG.Core.Models;
using JRPG.Enums;
using JRPGPlayerDemo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JRPG
{
    public class Game
    {
     private bool isPaused = false;

        private Canvas canvas;
        private const int viewWidth = 16;
        private const int viewHeight = 12;

        private Image playerImageControl;
        private BitmapImage playerBitmap;
        private CroppedBitmap[,] playerFrames;

        private int animationFrame = 0;
        private double animationTimer = 0;
        private double animationSpeed = 10.0;
        private int totalAnimationFrames = 8;

        private bool isMoving = false;
        private int moveStartX, moveStartY, moveTargetX, moveTargetY;
        private double moveProgress = 0;

        private const double playerRadius = 0.1;

        //public to be able to save
        public double playerX = 10.0;
        public double playerY = 10.0;

        private double velocityX = 0;
        private double velocityY = 0;
        private double moveSpeed = 4.00;

        private Direction playerDirection = Direction.Down;
        private double cameraX = 0;
        private double cameraY = 0;

        private int lastTriggeredX = -1;
        private int lastTriggeredY = -1;

        private DateTime lastFrameTime = DateTime.Now;
        private Map map;

        //public to be able to save
        public string currentMapFile = "test1.map";

        private HashSet<Key> keysDown = new HashSet<Key>();

        public void OnKeyDown(Key key) => keysDown.Add(key);
        public void OnKeyUp(Key key) => keysDown.Remove(key);

        //for fast rendering
        private VisualHost mapVisualHost = new VisualHost();
        private DrawingVisual mapVisual = new DrawingVisual();

        //Animated Tile
        private double tileAnimationTimer = 0;
        private int tileAnimationFrame = 0;

        public Game(Canvas canvas)
        {
            this.canvas = canvas;
            map = new Map();
            map.LoadFromFile(currentMapFile);

            playerBitmap = new BitmapImage(new Uri(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Assets/player.png")));

            playerFrames = new CroppedBitmap[4, totalAnimationFrames];
            for (int dir = 0; dir < 4; dir++)
            {
                for (int frame = 0; frame < totalAnimationFrames; frame++)
                {
                    int cropX = frame * MapConstant.TileSize;
                    int cropY = dir * MapConstant.TileSize;

                    playerFrames[dir, frame] = new CroppedBitmap(playerBitmap, new Int32Rect(cropX, cropY, MapConstant.TileSize, MapConstant.TileSize));
                }
            }

            playerImageControl = new Image { Width = MapConstant.TileSize, Height = MapConstant.TileSize };

            canvas.Width = SystemParameters.PrimaryScreenWidth;
            canvas.Height = SystemParameters.PrimaryScreenHeight;

            var root = (Grid)Application.Current.MainWindow.FindName("RootGrid");

            mapVisual = new DrawingVisual();
            mapVisualHost = new VisualHost();
            mapVisualHost.SetVisual(mapVisual);

            mapVisualHost.Width = canvas.ActualWidth;
            mapVisualHost.Height = canvas.ActualHeight;
            Canvas.SetLeft(mapVisualHost, 0); // just in case it needs it
            Canvas.SetTop(mapVisualHost, 0);

            // Instead of: canvas.Children.Insert(0, mapVisualHost);
            root.Children.Insert(0, mapVisualHost); // Put behind everything

            canvas.Children.Add(playerImageControl);

            RedrawMapVisual();  // Initial draw
        }

        public void LoadFromFile(String currentMapFile)
        {
            map.LoadFromFile(currentMapFile);
        }

        public void Update()
        {
            double deltaTime = (DateTime.Now - lastFrameTime).TotalSeconds;
            lastFrameTime = DateTime.Now;

            velocityX = 0;
            velocityY = 0;

            if (!isPaused)
            {
                bool left = keysDown.Contains(Key.Left);
                bool right = keysDown.Contains(Key.Right);
                bool up = keysDown.Contains(Key.Up);
                bool down = keysDown.Contains(Key.Down);

                if (left && !right) { velocityX = -moveSpeed; playerDirection = Direction.Left; }
                if (right && !left) { velocityX = moveSpeed; playerDirection = Direction.Right; }
                if (up && !down) { velocityY = -moveSpeed; playerDirection = Direction.Up; }
                if (down && !up) { velocityY = moveSpeed; playerDirection = Direction.Down; }

                if (velocityX != 0 || velocityY != 0)
                {
                    animationTimer += deltaTime;
                    if (animationTimer >= 1.0 / animationSpeed)
                    {
                        animationFrame = (animationFrame + 1) % totalAnimationFrames;
                        animationTimer = 0;
                    }

                    double newX = playerX + velocityX * deltaTime;
                    double newY = playerY + velocityY * deltaTime;

                    newX = Clamp(newX, playerRadius, map.Width - playerRadius);
                    newY = Clamp(newY, playerRadius, map.Height - playerRadius);

                    if (CanMoveTo(newX, newY))
                    {
                        playerX = newX;
                        playerY = newY;

                        int tileX = (int)Math.Floor(playerX);
                        int tileY = (int)Math.Floor(playerY);

                        if ((tileX != lastTriggeredX || tileY != lastTriggeredY))
                        {
                            var tile = map.GetTileTrigger(tileX, tileY);
                            if (tile != null && tile.TriggerType != TileTriggerType.None)
                            {
                                HandleTileTrigger(tile);
                                lastTriggeredX = tileX;
                                lastTriggeredY = tileY;
                            }
                        }

                        if (map.GetTileTrigger(tileX, tileY) == null || map.GetTileTrigger(tileX, tileY)?.TriggerType == TileTriggerType.None)
                        {
                            lastTriggeredX = -1;
                            lastTriggeredY = -1;
                        }
                    }
                }
                else
                {
                    animationFrame = 0;
                }

                tileAnimationTimer += deltaTime;
                if (tileAnimationTimer >= 0.2) // change this to match shortest tile duration
                {
                    tileAnimationFrame++;
                    tileAnimationTimer = 0;
                }
            }

            UpdateCamera();
        }

        private void UpdateCamera()
        {
            double tilesVisibleX = canvas.ActualWidth / MapConstant.TileSize;
            double tilesVisibleY = canvas.ActualHeight / MapConstant.TileSize;

            double targetX = playerX - tilesVisibleX / 2.0;
            double targetY = playerY - tilesVisibleY / 2.0;

            if (map.Width > tilesVisibleX)
                targetX = Clamp(targetX, 0, map.Width - tilesVisibleX);
            else
                targetX = (map.Width - tilesVisibleX) / 2.0;

            if (map.Height > tilesVisibleY)
                targetY = Clamp(targetY, 0, map.Height - tilesVisibleY);
            else
                targetY = (map.Height - tilesVisibleY) / 2.0;

            double easing = 0.35;
            cameraX += (targetX - cameraX) * easing;
            cameraY += (targetY - cameraY) * easing;

            RedrawMapVisual();
        }

        public void Render()
        {
            for (int i = canvas.Children.Count - 1; i >= 0; i--)
                if (canvas.Children[i] != playerImageControl)
                    canvas.Children.RemoveAt(i);

            playerImageControl.Source = playerFrames[(int)playerDirection, animationFrame];

            double spriteOffsetX = 0.0;
            double spriteOffsetY = 0.0;

            // example: shift sprite up by 4px and left by 2px
            spriteOffsetX = -32.0 / MapConstant.TileSize;
            spriteOffsetY = -32.0 / MapConstant.TileSize;

            double drawX = (playerX - playerRadius + spriteOffsetX - cameraX) * MapConstant.TileSize;
            double drawY = (playerY - playerRadius + spriteOffsetY - cameraY) * MapConstant.TileSize;

            Canvas.SetLeft(playerImageControl, drawX);
            Canvas.SetTop(playerImageControl, drawY);

            Canvas.SetZIndex(playerImageControl, 10);
        }

        private void RedrawMapVisual()
        {
            using (var dc = mapVisual.RenderOpen())
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

                        int tileId = map.Tiles[mapX, mapY];
                        string spritePath = map.GetTileSprite(tileId);
                        if (string.IsNullOrEmpty(spritePath)) continue;

                        var img = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), spritePath)));
                        dc.DrawImage(img, new Rect((mapX - cameraX) * MapConstant.TileSize, (mapY - cameraY) * MapConstant.TileSize, MapConstant.TileSize, MapConstant.TileSize));
                    }
                }

                foreach (var obj in map.PlacedObjects)
                {
                    if (!map.ObjectDefinitions.TryGetValue(obj.Id, out var def)) continue;
                    if (string.IsNullOrEmpty(def.SpritePath)) continue;

                    double tileDrawX = obj.X - 0.5 - cameraX;
                    double tileDrawY = obj.Y - 0.5 - cameraY;

                    double tilesVisibleXf = canvas.ActualWidth / MapConstant.TileSize;
                    double tilesVisibleYf = canvas.ActualHeight / MapConstant.TileSize;

                    if (tileDrawX < -1 || tileDrawX > tilesVisibleXf + 1 ||
                        tileDrawY < -1 || tileDrawY > tilesVisibleYf + 1)
                        continue;

                    if (map.AnimatedTiles.TryGetValue(obj.Id, out var anim))
                    {
                        int frameCount = anim.Frames.Length;
                        int frameIndex = (tileAnimationFrame % frameCount);
                        var framePath = anim.Frames[frameIndex];
                        var img = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), framePath)));
                        dc.DrawImage(img, new Rect((tileDrawX - cameraX) * MapConstant.TileSize, (tileDrawY - cameraY) * MapConstant.TileSize, MapConstant.TileSize, MapConstant.TileSize));
                    }
                    else
                    {
                        var img = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), def.SpritePath)));
                        dc.DrawImage(img, new Rect((tileDrawX - cameraX) * MapConstant.TileSize, (tileDrawY - cameraY) * MapConstant.TileSize, MapConstant.TileSize, MapConstant.TileSize));
                    }
                   
                }
            }
        }

        private bool CanMoveTo(double x, double y)
        {
            double left = x - playerRadius;
            double right = x + playerRadius;
            double top = y - playerRadius;
            double bottom = y + playerRadius;

            // Tile-level collision
            for (int ty = (int)Math.Floor(top); ty <= (int)Math.Floor(bottom); ty++)
            {
                for (int tx = (int)Math.Floor(left); tx <= (int)Math.Floor(right); tx++)
                {
                    if (!map.IsWalkable(tx, ty))
                        return false;
                }
            }

            // Object-layer collision (pixel-precise)
            foreach (var obj in map.PlacedObjects) // <-- this list should be parsed from .map file
            {
                if (!map.ObjectDefinitions.TryGetValue(obj.Id, out var def)) continue;
                if (!def.HasCollision || def.CollisionBox == null) continue;

                // Convert the object's collision box from relative (within tile) to world position in tile space
                Rect box = def.CollisionBox.Value;
                double worldLeft = obj.X - 0.5 + (box.X / MapConstant.TileSize);
                double worldTop = obj.Y - 0.5 + (box.Y / MapConstant.TileSize);
                double worldRight = worldLeft + (box.Width / MapConstant.TileSize);
                double worldBottom = worldTop + (box.Height / MapConstant.TileSize);

                // Check overlap with player collision area
                if (right > worldLeft && left < worldRight &&
                    bottom > worldTop && top < worldBottom)
                {
                    return false;
                }
            }

            return true;
        }

        private void HandleTileTrigger(TileDefinition tile)
        {
            //isPaused = true;

            //switch (tile.TriggerType)
            //{
            //    case TileTriggerType.Warp:
            //        string[] parts = tile.TriggerData.Split(':');
            //        string targetMap = parts[0];
            //        string[] coords = parts[1].Split(',');
            //        int newX = int.Parse(coords[0]);
            //        int newY = int.Parse(coords[1]);

            //        currentMapFile = targetMap;
            //        map = new Map();
            //        map.LoadFromFile(currentMapFile);
            //        playerX = newX;
            //        playerY = newY;
            //        break;

            //    case TileTriggerType.Battle:
            //        Application.Current.Dispatcher.Invoke(() =>
            //        {
            //            ((MainWindow)Application.Current.MainWindow).StartBattle();
            //        });
            //        break;

            //    case TileTriggerType.Dialogue:
            //        isPaused = true;
            //        ((MainWindow)Application.Current.MainWindow).ShowDialogue(new List<string>
            //        {
            //            tile.TriggerData,
            //            "Long ago, our ancestors built this sacred tree.",
            //            "You must protect it now, young one."
            //        }, "Assets/figtherPortrait.png", "Fighter", () =>
            //        {
            //            keysDown.Clear();
            //            isPaused = false;
            //        });
            //        break;

            //    case TileTriggerType.Item:
            //        MessageBox.Show($"You found an item: {tile.TriggerData}!");
            //        tile.TriggerType = TileTriggerType.None; // One-time use
            //        break;

            //    case TileTriggerType.Switch:
            //        break;
            //}

            //keysDown.Clear();
            //isPaused = false;
        }

        private int Clamp(int val, int min, int max) => Math.Max(min, Math.Min(max, val));

        private double Clamp(double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}