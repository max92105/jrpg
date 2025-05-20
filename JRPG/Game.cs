using JRPG.Core.Enums;
using JRPG.Core.Maps;
using JRPG.Core.Systems;
using JRPG.Core.Triggers.Models;
using JRPG.Core.Utils;
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
        public double playerY = 3.0;

        private double velocityX = 0;
        private double velocityY = 0;
        private double moveSpeed = 4.00;

        private Direction playerDirection = Direction.Down;
        private double cameraX = 0;
        private double cameraY = 0;

        private int lastTriggeredX = -1;
        private int lastTriggeredY = -1;

        private DateTime lastFrameTime = DateTime.Now;
        private MapManager mapManager;

        //public to be able to save
        public string currentMapFile = "start.json";

        private HashSet<Key> keysDown = new HashSet<Key>();

        public void OnKeyDown(Key key) => keysDown.Add(key);
        public void OnKeyUp(Key key) => keysDown.Remove(key);

        //for fast rendering
        private VisualHost mapVisualHost = new VisualHost();
        private DrawingVisual mapVisual = new DrawingVisual();

        //Animated Tile
        private double tileAnimationTimer = 0;
        private int tileAnimationFrame = 0;

        //better rendering
        private Dictionary<string, BitmapImage> imageCache = new Dictionary<string, BitmapImage>();
        private double lastCameraX = -1;
        private double lastCameraY = -1;
        private int lastRedrawnTileFrame = -1;

        public Game(Canvas canvas)
        {
            this.canvas = canvas;
            mapManager = new MapManager();
            mapManager.LoadFromFile(currentMapFile);

            playerBitmap = new BitmapImage(new Uri(Path.Combine(AssetPathResolver.GetAssetsRootPath(), "player.png")));

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

            for (int i = root.Children.Count - 1; i >= 0; i--)
            {
                if (root.Children[i] is VisualHost)
                {
                    root.Children.RemoveAt(i);
                }
            }

            mapVisual = new DrawingVisual();
            mapVisualHost = new VisualHost();
            mapVisualHost.SetVisual(mapVisual);

            mapVisualHost.Width = canvas.ActualWidth;
            mapVisualHost.Height = canvas.ActualHeight;

            mapVisualHost.HorizontalAlignment = HorizontalAlignment.Left;
            mapVisualHost.VerticalAlignment = VerticalAlignment.Top;

            root.Children.Insert(0, mapVisualHost); // Put behind everything

            canvas.Children.Add(playerImageControl);

            RedrawMapVisual();  // Initial draw
        }

        public void LoadFromFile(String currentMapFile)
        {
            mapManager.LoadFromFile(currentMapFile);
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

                    newX = Clamp(newX, playerRadius, mapManager.Width - playerRadius);
                    newY = Clamp(newY, playerRadius, mapManager.Height - playerRadius);

                    if (CanMoveTo(newX, newY))
                    {
                        playerX = newX;
                        playerY = newY;

                        int tileX = (int)Math.Floor(playerX);
                        int tileY = (int)Math.Floor(playerY);

                        if ((tileX != lastTriggeredX || tileY != lastTriggeredY))
                        {
                            var tileTrigger = mapManager.GetTileTrigger(tileX, tileY);
                            if (tileTrigger != null && tileTrigger.Type != TriggerType.None)
                            {
                                HandleTileTrigger(tileTrigger);
                                lastTriggeredX = tileX;
                                lastTriggeredY = tileY;
                            }
                            else 
                            {
                                Random random = new Random();

                                if (random.Next(1, 100) == 59)
                                {
                                    HandleTileTrigger(new TriggerBattle());
                                }
                            }
                        }

                        if (mapManager.GetTileTrigger(tileX, tileY) == null || mapManager.GetTileTrigger(tileX, tileY)?.Type == TriggerType.None)
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

            GameSystems.System.UpdatePlayerPosition(playerX, playerY);
        }

        private void UpdateCamera()
        {
            double tilesVisibleX = canvas.ActualWidth / MapConstant.TileSize;
            double tilesVisibleY = canvas.ActualHeight / MapConstant.TileSize;

            double targetX = playerX - tilesVisibleX / 2.0;
            double targetY = playerY - tilesVisibleY / 2.0;

            if (mapManager.Width > tilesVisibleX)
                targetX = Clamp(targetX, 0, mapManager.Width - tilesVisibleX);
            else
                targetX = (mapManager.Width - tilesVisibleX) / 2.0;

            if (mapManager.Height > tilesVisibleY)
                targetY = Clamp(targetY, 0, mapManager.Height - tilesVisibleY);
            else
                targetY = (mapManager.Height - tilesVisibleY) / 2.0;

            double easing = 0.35;
            cameraX += (targetX - cameraX) * easing;
            cameraY += (targetY - cameraY) * easing;

            bool cameraMoved = Math.Abs(cameraX - lastCameraX) > 0.01 || Math.Abs(cameraY - lastCameraY) > 0.01;
            bool tileFrameChanged = tileAnimationFrame != lastRedrawnTileFrame;

            if (cameraMoved || tileFrameChanged)
                RedrawMapVisual();

            lastCameraX = cameraX;
            lastCameraY = cameraY;
            lastRedrawnTileFrame = tileAnimationFrame;
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
                double time = DateTime.Now.TimeOfDay.TotalSeconds;

                int tilesVisibleX = (int)Math.Ceiling(canvas.ActualWidth / MapConstant.TileSize);
                int tilesVisibleY = (int)Math.Ceiling(canvas.ActualHeight / MapConstant.TileSize);
                int startX = (int)Math.Floor(cameraX);
                int startY = (int)Math.Floor(cameraY);

                //Rendering Tiles
                for (int y = 0; y < tilesVisibleY + 2; y++)
                {
                    for (int x = 0; x < tilesVisibleX + 2; x++)
                    {
                        int mapX = x + startX;
                        int mapY = y + startY;

                        if (mapX < 0 || mapY < 0 || mapX >= mapManager.Width || mapY >= mapManager.Height)
                            continue;

                        int tileId = mapManager.CurrentMap.GetTileIdAt(mapX, mapY);
                        if (!mapManager.CurrentMap.TileDefinitions.TryGetValue(tileId, out var def)) continue;

                        ImageSource imgSource = null;

                        if (def.Frames != null && def.Frames.Length > 0)
                        {
                            int frameIndex = (int)(time / def.FrameDuration) % def.Frames.Length;
                            var framePath = def.Frames[frameIndex];
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

                //Rendering Objects
                foreach (var obj in mapManager.GetObjects())
                {
                    if (!mapManager.CurrentMap.ObjectDefinitions.TryGetValue(obj.Id, out var def)) continue;
                    if (string.IsNullOrEmpty(def.SpritePath)) continue;

                    double screenX = (obj.X - 0.5 - cameraX) * MapConstant.TileSize;
                    double screenY = (obj.Y - 0.5 - cameraY) * MapConstant.TileSize;

                    if (screenX + MapConstant.TileSize < 0 || screenX > canvas.ActualWidth ||
                        screenY + MapConstant.TileSize < 0 || screenY > canvas.ActualHeight)
                        continue;

                    var img = GetCachedImage(def.SpritePath);
                    Rect rect = new Rect(screenX, screenY, MapConstant.TileSize, MapConstant.TileSize);
                    
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

        private bool CanMoveTo(double x, double y)
        {
            double left = x - playerRadius;
            double right = x + playerRadius;
            double top = y - playerRadius;
            double bottom = y + playerRadius;

            //Tile Collision
            for (int ty = (int)Math.Floor(top); ty <= (int)Math.Floor(bottom); ty++)
            {
                for (int tx = (int)Math.Floor(left); tx <= (int)Math.Floor(right); tx++)
                {
                    if (tx < 0 || ty < 0 || tx >= mapManager.Width || ty >= mapManager.Height)
                        return false;

                    int tileId = mapManager.CurrentMap.GetTileIdAt(tx, ty);
                    if (!mapManager.CurrentMap.TileDefinitions.TryGetValue(tileId, out var def) || !def.IsWalkable)
                        return false;
                }
            }

            //Object Collision
            foreach (var obj in mapManager.GetObjects())
            {
                if (!mapManager.CurrentMap.ObjectDefinitions.TryGetValue(obj.Id, out var def)) continue;
                if (!def.HasCollision || def.CollisionBox == null) continue;

                Rect box = def.CollisionBox.Value;

                double worldLeft = obj.X - 0.5 + (box.X / MapConstant.TileSize);
                double worldTop = obj.Y - 0.5 + (box.Y / MapConstant.TileSize);
                double worldRight = worldLeft + (box.Width / MapConstant.TileSize);
                double worldBottom = worldTop + (box.Height / MapConstant.TileSize);

                if (right > worldLeft && left < worldRight &&
                    bottom > worldTop && top < worldBottom)
                {
                    return false;
                }
            }

            return true;
        }

        private void HandleTileTrigger(Core.Triggers.Models.Trigger trigger)
        {
            isPaused = true;

            switch (trigger)
            {
                case TriggerWarp warp:
                    currentMapFile = warp.TargetMap;
                    mapManager.LoadFromFile(currentMapFile);
                    playerX = warp.TargetX;
                    playerY = warp.TargetY;

                    GameSystems.System.UpdateCurrentMap(currentMapFile);
                    GameSystems.System.UpdatePlayerPosition(playerX, playerY);
                    break;

                case TriggerDialogue dialogue:
                    var lines = dialogue.Lines ?? new List<string> { "..." };
                    var portrait = string.IsNullOrEmpty(dialogue.PortraitPath) ? "Fighter.png" : dialogue.PortraitPath;
                    var speaker = string.IsNullOrEmpty(dialogue.SpeakerName) ? "???" : dialogue.SpeakerName;

                    ((MainWindow)Application.Current.MainWindow).ShowDialogue(
                        lines,
                        portrait,
                        speaker,
                        () =>
                        {
                            keysDown.Clear();
                            isPaused = false;
                        });
                    return; // Don't resume yet; callback will unpause.

                case TriggerItem item:
                    MessageBox.Show($"You found an item: {item.ItemId}!");
                    // Optional: remove trigger to make it one-time
                    break;

                case TriggerBattle battle :
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ((MainWindow)Application.Current.MainWindow).StartBattle();
                    });
                    break;

                default:
                    break;
            }

            keysDown.Clear();
            isPaused = false;
        }

        private int Clamp(int val, int min, int max) => Math.Max(min, Math.Min(max, val));

        private double Clamp(double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}