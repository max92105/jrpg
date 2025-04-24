using JRPG.Enums;
using JRPGPlayerDemo;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace JRPG
{
    public class Game
    {
        private Canvas canvas;
        private const int tileSize = 64;
        private const int viewWidth = 10;
        private const int viewHeight = 10;

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
        private double moveSpeed = 5;

        private int playerX = 1;
        private int playerY = 1;

        private Direction playerDirection = Direction.Down;
        private int cameraX = 0;
        private int cameraY = 0;

        private DateTime lastFrameTime = DateTime.Now;
        private Map map;
        private string currentMapFile = "village.map";

        public Game(Canvas canvas)
        {
            this.canvas = canvas;
            map = new Map();
            map.LoadFromFile(currentMapFile);

            playerBitmap = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Assets/player.png")));

            // Preload all animation frames
            playerFrames = new CroppedBitmap[4, totalAnimationFrames];
            for (int dir = 0; dir < 4; dir++)
            {
                for (int frame = 0; frame < totalAnimationFrames; frame++)
                {
                    //TODO Since my file is shit i need to tweak these lines
                    //int cropX = frame * tileSize;
                    //int cropY = dir * tileSize;
                    int cropX = frame * 48;
                    int cropY = dir * 64;

                    //playerFrames[dir, frame] = new CroppedBitmap(playerBitmap, new Int32Rect(cropX, cropY, tileSize, tileSize));
                    playerFrames[dir, frame] = new CroppedBitmap(playerBitmap, new Int32Rect(cropX, cropY, 48, 64));
                }
            }

            playerImageControl = new Image { Width = tileSize, Height = tileSize };
            canvas.Children.Add(playerImageControl);
        }

        public void Update()
        {
            double deltaTime = (DateTime.Now - lastFrameTime).TotalSeconds;
            lastFrameTime = DateTime.Now;

            if (isMoving)
            {
                moveProgress += moveSpeed * deltaTime;
                animationTimer += deltaTime;

                if (animationTimer >= 1.0 / animationSpeed)
                {
                    animationFrame = (animationFrame + 1) % totalAnimationFrames;
                    animationTimer = 0;
                }

                if (moveProgress >= 1.0)
                {
                    playerX = moveTargetX;
                    playerY = moveTargetY;
                    isMoving = false;
                    animationFrame = 0;

                    var tile = map.GetTileTrigger(playerX, playerY);
                    if (tile != null && tile.TriggerType != TileTriggerType.None)
                    {
                        HandleTileTrigger(tile);
                    }
                }
            }

            cameraX = Clamp(playerX - viewWidth / 2, 0, map.Width - viewWidth);
            cameraY = Clamp(playerY - viewHeight / 2, 0, map.Height - viewHeight);
        }

        public void Render()
        {
            for (int i = canvas.Children.Count - 1; i >= 0; i--)
            {
                if (canvas.Children[i] != playerImageControl)
                    canvas.Children.RemoveAt(i);
            }

            for (int y = 0; y < viewHeight; y++)
            {
                for (int x = 0; x < viewWidth; x++)
                {
                    int mapX = x + cameraX;
                    int mapY = y + cameraY;
                    int tileId = map.Tiles[mapX, mapY];

                    string spritePath = map.GetTileSprite(tileId);
                    if (!string.IsNullOrEmpty(spritePath))
                    {
                        var tile = new Image
                        {
                            Width = tileSize,
                            Height = tileSize,
                            Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), spritePath)))
                        };
                        Canvas.SetLeft(tile, x * tileSize);
                        Canvas.SetTop(tile, y * tileSize);
                        canvas.Children.Add(tile);
                    }
                }
            }

            double renderX = playerX;
            double renderY = playerY;
            if (isMoving)
            {
                renderX = moveStartX + (moveTargetX - moveStartX) * moveProgress;
                renderY = moveStartY + (moveTargetY - moveStartY) * moveProgress;
            }

            playerImageControl.Source = playerFrames[(int)playerDirection, animationFrame];
            Canvas.SetLeft(playerImageControl, (renderX - cameraX) * tileSize);
            Canvas.SetTop(playerImageControl, (renderY - cameraY) * tileSize);
            Canvas.SetZIndex(playerImageControl, 10);
        }

        public void OnKeyDown(Key key)
        {
            if (isMoving) return;

            int dx = 0, dy = 0;

            switch (key)
            {
                case Key.Up: dy = -1; playerDirection = Direction.Up; break;
                case Key.Down: dy = 1; playerDirection = Direction.Down; break;
                case Key.Left: dx = -1; playerDirection = Direction.Left; break;
                case Key.Right: dx = 1; playerDirection = Direction.Right; break;
            }

            int nextX = playerX + dx;
            int nextY = playerY + dy;

            if (map.IsWalkable(nextX, nextY))
            {
                moveStartX = playerX;
                moveStartY = playerY;
                moveTargetX = nextX;
                moveTargetY = nextY;
                isMoving = true;
                moveProgress = 0;
            }
        }

        private void HandleTileTrigger(TileType tile)
        {
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
                    MessageBox.Show(tile.TriggerData); // Display the message
                    break;

                case TileTriggerType.Item:
                    MessageBox.Show($"You found an item: {tile.TriggerData}!");
                    tile.TriggerType = TileTriggerType.None; // One-time use
                    break;

                case TileTriggerType.Switch:
                    break;
            }
        }

        private int Clamp(int val, int min, int max) => Math.Max(min, Math.Min(max, val));
    }
}