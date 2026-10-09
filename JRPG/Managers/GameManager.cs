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
        private static readonly Random rng = new Random();
        private const int EncounterOneIn = 500; // chance per update while walking; raise for fewer battles

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

            camera = new CameraManager(canvas.Width / MapConstant.TileSize, canvas.Height / MapConstant.TileSize);
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

            LoadPlayerSpriteSheet();

            camera.SnapTo(Player.X, Player.Y, mapManager.CurrentMap);

            var frame = playerFrames[(int)Player.Direction, Player.AnimationFrame];
            mapRenderer.Redraw(camera.X, camera.Y, mapManager.CurrentMap, Player.X, Player.Y, frame);
        }

        public void Dispose()
        {
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
            playerBitmap.Freeze();
            playerFrames = new CroppedBitmap[4, 8];

            for (int dir = 0; dir < 4; dir++)
            {
                for (int frame = 0; frame < 8; frame++)
                {
                    int cropX = frame * MapConstant.TileSize;
                    int cropY = dir * MapConstant.TileSize;
                    playerFrames[dir, frame] = new CroppedBitmap(playerBitmap, new Int32Rect(cropX, cropY, MapConstant.TileSize, MapConstant.TileSize));
                    playerFrames[dir, frame].Freeze();
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
            // Update runs on GameLoopManager's fixed timestep, so the step is always the same.
            // (Measuring time here gave ~0 on catch-up steps and a huge jump after closing the menu.)
            double deltaTime = GameLoopManager.TargetDelta;

            if (!isPaused)
            {
                bool left = Managers.Input.InputManager.IsKeyDown(Key.Left);
                bool right = Managers.Input.InputManager.IsKeyDown(Key.Right);
                bool up = Managers.Input.InputManager.IsKeyDown(Key.Up);
                bool down = Managers.Input.InputManager.IsKeyDown(Key.Down);

                Player.UpdateMovement(up, down, left, right, deltaTime, mapManager.CurrentMap);
                camera.Update(Player.X, Player.Y, mapManager.CurrentMap);
                mapRenderer.Update(deltaTime);

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
        }

        public void Render(double interpolationFactor)
        {
            double interpX = Player.PreviousX + (Player.X - Player.PreviousX) * interpolationFactor;
            double interpY = Player.PreviousY + (Player.Y - Player.PreviousY) * interpolationFactor;

            double interpCamX = camera.PreviousX + (camera.X - camera.PreviousX) * interpolationFactor;
            double interpCamY = camera.PreviousY + (camera.Y - camera.PreviousY) * interpolationFactor;

            var frame = playerFrames[(int)Player.Direction, Player.AnimationFrame];
            mapRenderer.Redraw(interpCamX, interpCamY, mapManager.CurrentMap, interpX, interpY, frame);
        }

        private void CheckForTriggers()
        {
            int tileX = (int)Math.Floor(Player.X);
            int tileY = (int)Math.Floor(Player.Y);

            var tileTrigger = mapManager.GetTileTrigger(tileX, tileY);
            bool hasTrigger = tileTrigger != null && tileTrigger.Type != TriggerType.None;

            if (hasTrigger)
            {
                // Fire once when entering the tile, not every update while standing on it
                if (tileX != lastTriggeredX || tileY != lastTriggeredY)
                {
                    triggerHandler.HandleTrigger(tileTrigger);
                    lastTriggeredX = tileX;
                    lastTriggeredY = tileY;
                }
                return;
            }

            lastTriggeredX = -1;
            lastTriggeredY = -1;

            // Random encounter: only roll when the player actually moved (not when pushing against a wall).
            // Rolled once per fixed update (60/s), so on average one battle every ~8 seconds of walking.
            // Maps can turn random battles off (Map Properties in the editor), e.g. towns
            if (!mapManager.CurrentMap.RandomEncounters)
                return;

            bool moved = Player.X != Player.PreviousX || Player.Y != Player.PreviousY;
            if (moved && rng.Next(EncounterOneIn) == 0)
                triggerHandler.HandleTrigger(new TriggerBattle());
        }
    }
}