using JRPG.Core.Enums;
using JRPG.Core.Events.Models;
using JRPG.Core.Maps;
using JRPG.Core.Maps.IO;
using JRPG.Core.Maps.Models;
using JRPG.Enums;
using JRPG.Core.Systems;
using JRPG.Core.Triggers.Models;
using JRPG.Managers;
using JRPG.Systems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        // Object events (chests, doors, signs...)
        private const double InteractReach = 0.6;   // how far in front of the player we look, in tiles
        private bool eventRunning;
        private bool interactWasDown = true;        // a key already held doesn't count as a new press
        private bool touchArmed = true;             // off right after a warp until the player stops (no door ping-pong)
        private PlacedObject promptTarget;          // object whose "[E] ..." prompt is shown
        private Border promptBox;
        private TextBlock promptText;

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

            // "[E] Open" prompt shown above the object the player can interact with
            promptText = new TextBlock { Foreground = Brushes.White, FontSize = 16 };
            promptBox = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(160, 255, 255, 255)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 3, 8, 3),
                Child = promptText,
                Visibility = Visibility.Collapsed,
                IsHitTestVisible = false
            };
            canvas.Children.Add(promptBox);
            Canvas.SetZIndex(promptBox, 20);

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

                if (!isPaused)
                    UpdateObjectEvents();
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

            UpdatePrompt(interpCamX, interpCamY);
        }

        // === Object events (chests, doors, signs...) ===

        private void UpdateObjectEvents()
        {
            if (eventRunning)
                return;

            Func<Key, bool> input = Managers.Input.InputManager.IsKeyDown;
            bool interactDown = input(Key.E) || input(Key.Space) || input(Key.Enter);
            bool interactPressed = interactDown && !interactWasDown;
            interactWasDown = interactDown;

            if (!Player.IsMoving)
                touchArmed = true;

            // Interact: the player faces the object and presses E / Space / Enter
            promptTarget = FindEventInFront(EventActivation.Interact);
            if (interactPressed && promptTarget != null)
            {
                _ = RunEventAsync(promptTarget);
                return;
            }

            // Touch: the player walks into the object (doors)
            if (touchArmed && Player.IsMoving)
            {
                var touched = FindEventInFront(EventActivation.Touch);
                if (touched != null)
                    _ = RunEventAsync(touched);
            }
        }

        /// <summary>The object with an event of this kind just in front of the player, if any.</summary>
        private PlacedObject FindEventInFront(EventActivation activation)
        {
            double dx = Player.Direction == Direction.Left ? -1 : Player.Direction == Direction.Right ? 1 : 0;
            double dy = Player.Direction == Direction.Up ? -1 : Player.Direction == Direction.Down ? 1 : 0;
            double px = Player.X + dx * InteractReach;
            double py = Player.Y + dy * InteractReach;

            var objects = mapManager.CurrentMap.Objects;
            for (int i = objects.Count - 1; i >= 0; i--) // top-most first
            {
                var obj = objects[i];
                var ev = obj.Event;
                if (ev == null || ev.Activation != activation || ev.Actions == null || ev.Actions.Count == 0)
                    continue;
                if (ev.Once && GameSystems.World.IsEventDone(currentMapFile, obj.Uid))
                    continue;
                if (!ObjectDatabase.Definitions.TryGetValue(obj.Id, out var def))
                    continue;

                const double margin = 0.1;
                var (_, _, width, height) = def.GetTileBounds();
                if (px >= obj.X - margin && px <= obj.X + width + margin &&
                    py >= obj.Y - margin && py <= obj.Y + height + margin)
                    return obj;
            }

            return null;
        }

        private async Task RunEventAsync(PlacedObject obj)
        {
            var ev = obj.Event;
            string mapFile = currentMapFile;

            eventRunning = true;
            isPaused = true;
            promptTarget = null;

            // Mark it first, so a battle or warp in the middle can't make it run twice
            if (ev.Once)
                GameSystems.World.MarkEventDone(mapFile, obj.Uid);

            try
            {
                foreach (var action in ev.Actions.ToList())
                {
                    switch (action.Type)
                    {
                        case EventActionType.Dialogue:
                            if (action.Lines != null && action.Lines.Count > 0)
                                await ShowDialogueAsync(action.Lines, action.Portrait, action.Speaker);
                            break;

                        case EventActionType.GiveItem:
                            var item = GameSystems.Inventory.CreateItemById(action.ItemId);
                            if (item == null)
                            {
                                await ShowDialogueAsync(new List<string> { $"(Unknown item \"{action.ItemId}\")" });
                                break;
                            }
                            int quantity = Math.Max(1, action.Quantity);
                            GameSystems.Inventory.Add(item, quantity);
                            await ShowDialogueAsync(new List<string> { quantity > 1 ? $"Obtained {item.Name} x{quantity}!" : $"Obtained {item.Name}!" });
                            break;

                        case EventActionType.SwapObject:
                            if (ObjectDatabase.Definitions.ContainsKey(action.ObjectId))
                            {
                                obj.Id = action.ObjectId;
                                GameSystems.World.SetObjectSwap(mapFile, obj.Uid, action.ObjectId); // stays swapped
                                mapRenderer.Invalidate();
                            }
                            break;

                        case EventActionType.Warp:
                            await WarpAsync(action.TargetMap, action.TargetX + 0.5, action.TargetY + 0.5);
                            break;

                        case EventActionType.Battle:
                            // The battle takes over the screen; the rest of the event is skipped
                            triggerHandler.HandleTrigger(new TriggerBattle());
                            return;
                    }
                }
            }
            finally
            {
                eventRunning = false;
                isPaused = false;
                interactWasDown = true; // the key that closed the last message must be released first
            }
        }

        private async Task WarpAsync(string targetMap, double x, double y)
        {
            if (string.IsNullOrEmpty(targetMap))
                return;

            if (!File.Exists(Core.Utils.AssetPathResolver.GetPath(Path.Combine("Maps", targetMap))))
            {
                await ShowDialogueAsync(new List<string> { $"(Map \"{targetMap}\" not found)" });
                return;
            }

            await SceneManager.TransitionAsync(() =>
            {
                LoadMapAndPositionPlayer(targetMap, x, y);
                lastTriggeredX = lastTriggeredY = -1;
                mapRenderer.Invalidate();
            });

            touchArmed = false; // don't walk straight back through the door you arrived at
        }

        private static Task ShowDialogueAsync(List<string> lines, string portrait = null, string speaker = null)
        {
            var done = new TaskCompletionSource<bool>();
            ((MainWindow)Application.Current.MainWindow).ShowDialogue(lines, portrait, speaker, () => done.TrySetResult(true));
            return done.Task;
        }

        private void UpdatePrompt(double cameraX, double cameraY)
        {
            var obj = promptTarget;
            if (obj == null || eventRunning || isPaused || !ObjectDatabase.Definitions.TryGetValue(obj.Id, out var def))
            {
                promptBox.Visibility = Visibility.Collapsed;
                return;
            }

            string prompt = string.IsNullOrWhiteSpace(obj.Event.Prompt) ? "Check" : obj.Event.Prompt;
            promptText.Text = $"[E] {prompt}";
            promptBox.Visibility = Visibility.Visible;
            promptBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            // Centered above the object
            var (_, _, width, _) = def.GetTileBounds();
            double left = (obj.X + width / 2.0 - cameraX) * MapConstant.TileSize - promptBox.DesiredSize.Width / 2;
            double top = (obj.Y - cameraY) * MapConstant.TileSize - promptBox.DesiredSize.Height - 6;
            Canvas.SetLeft(promptBox, Math.Round(left));
            Canvas.SetTop(promptBox, Math.Round(top));
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