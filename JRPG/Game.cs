using JRPG.Core.Enums;
using JRPG.Core.Models;
using JRPG.Enums;
using JRPGPlayerDemo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace JRPG
{
    public class Game
    {
     private bool isPaused = false;

        private Canvas canvas;
        private const int tileSize = 64;
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
        public string currentMapFile = "village.map";

        private HashSet<Key> keysDown = new HashSet<Key>();

        public void OnKeyDown(Key key) => keysDown.Add(key);
        public void OnKeyUp(Key key) => keysDown.Remove(key);

        public Game(Canvas canvas)
        {
            this.canvas = canvas;
            map = new Map();
            map.LoadFromFile(currentMapFile);

            playerBitmap = new BitmapImage(new Uri(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Assets/player.png")));

            // Preload all animation frames
            playerFrames = new CroppedBitmap[4, totalAnimationFrames];
            for (int dir = 0; dir < 4; dir++)
            {
                for (int frame = 0; frame < totalAnimationFrames; frame++)
                {
                    int cropX = frame * tileSize;
                    int cropY = dir * tileSize;

                    playerFrames[dir, frame] = new CroppedBitmap(playerBitmap, new Int32Rect(cropX, cropY, tileSize, tileSize));
                }
            }

            playerImageControl = new Image { Width = tileSize, Height = tileSize };
            canvas.Children.Add(playerImageControl);
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

                        // Only trigger if player stepped on a new trigger tile
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

                        // If no trigger under the player, clear last triggered position
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
            }

            UpdateCamera();
        }

        private void UpdateCamera()
        {
            double tilesVisibleX = canvas.ActualWidth / tileSize;
            double tilesVisibleY = canvas.ActualHeight / tileSize;

            double halfScreenX = tilesVisibleX / 2.0;
            double halfScreenY = tilesVisibleY / 2.0;

            double targetX = (playerX - playerRadius + (-32.0 / tileSize)) - halfScreenX + 0.5;
            double targetY = (playerY - playerRadius + (-32.0 / tileSize)) - halfScreenY + 0.5;

            targetX = Math.Max(0, Math.Min(targetX, map.Width - tilesVisibleX));
            targetY = Math.Max(0, Math.Min(targetY, map.Height - tilesVisibleY));

            double easing = 0.25;
            cameraX += (targetX - cameraX) * easing;
            cameraY += (targetY - cameraY) * easing;
        }

        public void Render()
        {
            for (int i = canvas.Children.Count - 1; i >= 0; i--)
                if (canvas.Children[i] != playerImageControl)
                    canvas.Children.RemoveAt(i);

            int startX = (int)Math.Floor(cameraX);
            int startY = (int)Math.Floor(cameraY);

            int tilesVisibleX = (int)Math.Ceiling(canvas.ActualWidth / tileSize) + 2;
            int tilesVisibleY = (int)Math.Ceiling(canvas.ActualHeight / tileSize) + 2;

            for (int y = 0; y < viewHeight + 2; y++)
            {
                for (int x = 0; x < viewWidth + 2; x++)
                {
                    int mapX = x + startX;
                    int mapY = y + startY;

                    if (mapX < 0 || mapY < 0 || mapX >= map.Width || mapY >= map.Height)
                        continue;

                    int tileId = map.Tiles[mapX, mapY];
                    string spritePath = map.GetTileSprite(tileId);

                    if (!string.IsNullOrEmpty(spritePath))
                    {
                        var tile = new Image
                        {
                            Width = tileSize,
                            Height = tileSize,
                            Source = new BitmapImage(new Uri(System.IO.Path.Combine(Directory.GetCurrentDirectory(), spritePath)))
                        };

                        double mapDrawX = (mapX - cameraX) * tileSize;
                        double mapDrawY = (mapY - cameraY) * tileSize;

                        Canvas.SetLeft(tile, mapDrawX);
                        Canvas.SetTop(tile, mapDrawY);

                        canvas.Children.Add(tile);
                    }
                }
            }

            playerImageControl.Source = playerFrames[(int)playerDirection, animationFrame];

            double spriteOffsetX = 0.0;
            double spriteOffsetY = 0.0;

            // example: shift sprite up by 4px and left by 2px
            spriteOffsetX = -32.0 / tileSize;
            spriteOffsetY = -32.0 / tileSize;

            double drawX = (playerX - playerRadius + spriteOffsetX - cameraX) * tileSize;
            double drawY = (playerY - playerRadius + spriteOffsetY - cameraY) * tileSize;

            Canvas.SetLeft(playerImageControl, drawX);
            Canvas.SetTop(playerImageControl, drawY);

            Canvas.SetZIndex(playerImageControl, 10);
        }

        private bool CanMoveTo(double x, double y)
        {
            double left = x - playerRadius;
            double right = x + playerRadius;
            double top = y - playerRadius;
            double bottom = y + playerRadius;

            for (int ty = (int)Math.Floor(top); ty <= (int)Math.Floor(bottom); ty++)
            {
                for (int tx = (int)Math.Floor(left); tx <= (int)Math.Floor(right); tx++)
                {
                    if (!map.IsWalkable(tx, ty))
                        return false;
                }
            }

            return true;
        }

        private void HandleTileTrigger(TileType tile)
        {
            isPaused = true;

            switch (tile.TriggerType)
            {
                case TileTriggerType.Warp:
                    string[] parts = tile.TriggerData.Split(':');
                    string targetMap = parts[0];
                    string[] coords = parts[1].Split(',');
                    int newX = int.Parse(coords[0]);
                    int newY = int.Parse(coords[1]);

                    currentMapFile = targetMap;
                    map = new Map();
                    map.LoadFromFile(currentMapFile);
                    playerX = newX;
                    playerY = newY;
                    break;

                case TileTriggerType.Battle:
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ((MainWindow)Application.Current.MainWindow).StartBattle();
                    });
                    break;

                case TileTriggerType.Dialogue:
                    isPaused = true;
                    ((MainWindow)Application.Current.MainWindow).ShowDialogue(new List<string>
                    {
                        tile.TriggerData,
                        "Long ago, our ancestors built this sacred tree.",
                        "You must protect it now, young one."
                    }, "Assets/figtherPortrait.png", "Fighter", () =>
                    {
                        keysDown.Clear();
                        isPaused = false;
                    });
                    break;

                case TileTriggerType.Item:
                    MessageBox.Show($"You found an item: {tile.TriggerData}!");
                    tile.TriggerType = TileTriggerType.None; // One-time use
                    break;

                case TileTriggerType.Switch:
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