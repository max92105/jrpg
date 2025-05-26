using JRPG.Core.Enums;
using JRPG.Core.Maps;
using JRPG.Core.Systems;
using JRPG.Core.Triggers.Models;
using JRPG.Managers;
using JRPG.Systems;
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
    public class GameManager
    {
        // Core
        public PlayerManager Player { get; private set; }
        private MapManager mapManager;
        private MapRenderer mapRenderer;
        private CameraManager camera;
        private TriggerHandler triggerHandler;

        // Runtime
        private readonly Canvas canvas;
        private readonly Image playerImageControl;
        private CroppedBitmap[,] playerFrames;
        private HashSet<Key> keysDown = new HashSet<Key>();

        private bool isPaused = false;
        private int lastTriggeredX = -1;
        private int lastTriggeredY = -1;
        private DateTime lastFrameTime = DateTime.Now;

        private static readonly Random rng = new Random();


        // State
        public string currentMapFile = "start.json";

        public GameManager(Canvas canvas)
        {
            this.canvas = canvas;
            canvas.Width = SystemParameters.PrimaryScreenWidth;
            canvas.Height = SystemParameters.PrimaryScreenHeight;

            // Managers
            mapManager = new MapManager();
            mapManager.LoadFromFile(currentMapFile);

            Player = new PlayerManager();
            Player.SetPosition(10, 3); // optional: load from save

            camera = new CameraManager(30, 17);
            mapRenderer = new MapRenderer(canvas);
            triggerHandler = new TriggerHandler(
                onWarp: (mapId, x, y) =>
                {
                    currentMapFile = mapId;
                    mapManager.LoadFromFile(mapId);
                    Player.SetPosition(x, y);
                    GameSystems.System.UpdateCurrentMap(mapId);
                    GameSystems.System.UpdatePlayerPosition(x, y);
                },
                onDialogue: (lines, speaker, portrait, resumeCallback) =>
                {
                    ((MainWindow)Application.Current.MainWindow).ShowDialogue(
                        lines, portrait, speaker,
                        () =>
                        {
                            keysDown.Clear();
                            isPaused = false;
                            resumeCallback?.Invoke();
                        });
                    isPaused = true;
                },
                onItemFound: (itemId) => MessageBox.Show($"You found an item: {itemId}!"),
                onBattleStart: () => Application.Current.Dispatcher.Invoke(() =>
                {
                   
                })
            );

            // Add map rendering layer (ZIndex 0)
            canvas.Children.Add(mapRenderer.VisualHost);
            Canvas.SetZIndex(mapRenderer.VisualHost, 0);

            playerImageControl = new Image
            {
                Width = MapConstant.TileSize,
                Height = MapConstant.TileSize,
                Stretch = Stretch.None,
                SnapsToDevicePixels = true
            };

            canvas.Children.Add(playerImageControl);
            Canvas.SetZIndex(playerImageControl, 10);

            LoadPlayerSpriteSheet();

            camera.SnapTo(Player.X, Player.Y, mapManager.CurrentMap);
            mapRenderer.Redraw(camera.X, camera.Y, mapManager.CurrentMap);
        }

        public void Dispose()
        {
            if (playerImageControl != null && canvas.Children.Contains(playerImageControl))
                canvas.Children.Remove(playerImageControl);

            if (mapRenderer?.VisualHost != null && canvas.Children.Contains(mapRenderer.VisualHost))
                canvas.Children.Remove(mapRenderer.VisualHost);

            canvas.Children.Clear(); // Full wipe
            keysDown.Clear();
        }

        public void LoadMapAndPositionPlayer(string mapId, double x, double y)
        {
            currentMapFile = mapId;
            mapManager.LoadFromFile(mapId);
            Player.SetPosition(x, y);
            camera.SnapTo(x, y, mapManager.CurrentMap);
            GameSystems.System.UpdateCurrentMap(mapId);
            GameSystems.System.UpdatePlayerPosition(x, y);
        }

        private void LoadPlayerSpriteSheet()
        {
            var spritePath = Path.Combine(Core.Utils.AssetPathResolver.GetAssetsRootPath(), "player.png");
            var playerBitmap = new BitmapImage(new Uri(spritePath));
            playerFrames = new CroppedBitmap[4, 8];

            for (int dir = 0; dir < 4; dir++)
            {
                for (int frame = 0; frame < 8; frame++)
                {
                    int cropX = frame * MapConstant.TileSize;
                    int cropY = dir * MapConstant.TileSize;
                    playerFrames[dir, frame] = new CroppedBitmap(playerBitmap, new Int32Rect(cropX, cropY, MapConstant.TileSize, MapConstant.TileSize));
                }
            }
        }

        public void OnKeyDown(Key key) => keysDown.Add(key);
        public void OnKeyUp(Key key) => keysDown.Remove(key);

        public void ResetInput()
        {
            keysDown.Clear();
        }

        public void Update()
        {
            double deltaTime = (DateTime.Now - lastFrameTime).TotalSeconds;
            lastFrameTime = DateTime.Now;

            if (!isPaused)
            {
                bool left = Managers.Input.InputManager.IsKeyDown(Key.Left);
                bool right = Managers.Input.InputManager.IsKeyDown(Key.Right);
                bool up = Managers.Input.InputManager.IsKeyDown(Key.Up);
                bool down = Managers.Input.InputManager.IsKeyDown(Key.Down);

                Player.UpdateMovement(up, down, left, right, deltaTime, mapManager.CurrentMap);
                camera.Update(Player.X, Player.Y, mapManager.CurrentMap);
                mapRenderer.Update(deltaTime);
                mapRenderer.RedrawIfNeeded(camera.X, camera.Y, mapManager.CurrentMap);

                if (Player.IsMoving)
                {
                    CheckForTriggers();
                    camera.SetEasing(0.35);
                }
                else
                { 
                    camera.SetEasing(0.1);
                }
            }

            GameSystems.System.UpdatePlayerPosition(Player.X, Player.Y);
            lastFrameTime = DateTime.Now;
        }

        public void Render(double interpolationFactor)
        {
            playerImageControl.Source = playerFrames[(int)Player.Direction, Player.AnimationFrame];

            // Interpolate player position
            double interpX = Player.PreviousX + (Player.X - Player.PreviousX) * interpolationFactor;
            double interpY = Player.PreviousY + (Player.Y - Player.PreviousY) * interpolationFactor;

            // Interpolate camera position
            double interpCamX = camera.PreviousX + (camera.X - camera.PreviousX) * interpolationFactor;
            double interpCamY = camera.PreviousY + (camera.Y - camera.PreviousY) * interpolationFactor;

            // Pixel offset so sprite is centered and visually balanced
            double spriteOffsetX = -32.0 / MapConstant.TileSize;
            double spriteOffsetY = -32.0 / MapConstant.TileSize;

            double drawX = (interpX - Player.Radius + spriteOffsetX - interpCamX) * MapConstant.TileSize;
            double drawY = (interpY - Player.Radius + spriteOffsetY - interpCamY) * MapConstant.TileSize;

            Canvas.SetLeft(playerImageControl, drawX);
            Canvas.SetTop(playerImageControl, drawY);
        }

        private void CheckForTriggers()
        {
            int tileX = (int)Math.Floor(Player.X);
            int tileY = (int)Math.Floor(Player.Y);

            var tileTrigger = mapManager.GetTileTrigger(tileX, tileY);

            if ((tileX != lastTriggeredX || tileY != lastTriggeredY) && tileTrigger?.Type != TriggerType.None)
            {
                triggerHandler.HandleTrigger(tileTrigger);
                lastTriggeredX = tileX;
                lastTriggeredY = tileY;
            }
            else if (tileTrigger == null || tileTrigger.Type == TriggerType.None)
            {
                lastTriggeredX = -1;
                lastTriggeredY = -1;

                if (rng.Next(1, 500) == 59)
                    triggerHandler.HandleTrigger(new TriggerBattle());
            }
        }
    }
}