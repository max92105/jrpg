using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.Core.Systems.SaveGames.Models;
using JRPG.Core.Utils;
using JRPG.Enums;
using JRPG.UI.Battle;
using JRPG.UI.GameMenu;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace JRPG.Managers
{
    public static class SceneManager
    {
        public static GameManager CurrentGame { get; private set; }

        private static Grid RootGrid;
        private static Rectangle FadeRect;
        private static Canvas GameCanvas;
        private static Grid StartScreen;
        private static GameMenuControl GameMenuControl;
        private static FrameworkElement OverlayViewHost;
        private static ContentPresenter ContentOverlayViewHost;
        private static UIElement StartMenuContent;
        
        public static void Initialize(
            Grid rootGrid,
            Rectangle fadeRect,
            Canvas gameCanvas,
            Grid startScreen,
            GameMenuControl gameMenuControl,
            FrameworkElement overlayViewHost,
            ContentPresenter contentOverlayViewHost,
            UIElement startMenuContent)
        {
            RootGrid = rootGrid;
            FadeRect = fadeRect;
            GameCanvas = gameCanvas;
            StartScreen = startScreen;
            GameMenuControl= gameMenuControl;
            OverlayViewHost = overlayViewHost;
            ContentOverlayViewHost = contentOverlayViewHost;
            StartMenuContent = startMenuContent;
        }

        public static void Update()
        {
            if (GameStateManager.IsExploration)
                CurrentGame?.Update();
        }

        public static void Render(double interpolation)
        {
            if (GameStateManager.IsExploration)
                CurrentGame?.Render(interpolation);
        }

        // Short pause on black between scenes
        private const int BlackPauseMs = 150;

        // Ignores clicks/keys that would start a second transition while one is running
        private static bool isTransitioning;

        public static async Task StartNewGameAsync()
        {
            if (isTransitioning) return;
            isTransitioning = true;

            try
            {
                // Fade first: building the game (map load + first draw) happens behind the
                // black screen instead of freezing the menu right after the click
                await FadeHelper.FadeToBlackAsync(FadeRect);

                GameSystems.Reset();

                CurrentGame?.Dispose();
                CurrentGame = new GameManager(GameCanvas);
                CurrentGame.LoadMapAndPositionPlayer("start.json", 10.0, 3.0);

                GameStateManager.SetState(SceneType.Exploration);

                GameCanvas.Visibility = Visibility.Visible;
                StartScreen.Visibility = Visibility.Collapsed;

                await Task.Delay(BlackPauseMs);
                await FadeHelper.FadeFromBlackAsync(FadeRect);
            }
            finally
            {
                isTransitioning = false;
            }
        }

        public static async Task ShowLoadScreenAsync()
        {
            // Hide the start menu
            StartMenuContent.Visibility = Visibility.Collapsed;

            // Blur effect on the start screen
            StartScreen.Effect = new BlurEffect
            {
                Radius = 8,
                RenderingBias = RenderingBias.Performance // full-screen blur, re-rendered during the fade
            };

            // Prepare overlay fade in
            OverlayViewHost.Visibility = Visibility.Visible;
            OverlayViewHost.Opacity = 0;
            ContentOverlayViewHost.Visibility = Visibility.Visible;

            ContentOverlayViewHost.Content = new Grid(); // placeholder

            var fadeInTask = FadeHelper.FadeToAsync(OverlayViewHost, 1, 0.5);

            await Task.Delay(10); // let WPF breathe

            var loadView = new SaveLoadView(
                SaveLoadView.SaveLoadMode.Load,
                async (slotIndex) =>
                {
                    var save = await GameSystems.System.LoadFromSlotAsync(slotIndex);
                    if (save == null)
                    {
                        MessageBox.Show("Failed to load save file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    await LoadGameAsync(save);

                    ContentOverlayViewHost.Content = null;
                    ContentOverlayViewHost.Visibility = Visibility.Collapsed;
                    OverlayViewHost.Visibility = Visibility.Collapsed;
                },
                () =>
                {
                    _ = CancelLoadScreenAsync();
                }
            );

            ContentOverlayViewHost.Content = loadView;
            ContentOverlayViewHost.UpdateLayout();

            await fadeInTask;
        }

        public static async Task LoadGameAsync(SaveData save)
        {
            if (isTransitioning) return;
            isTransitioning = true;

            try
            {
                // Fade first, then build the game behind the black screen
                await FadeHelper.FadeToBlackAsync(FadeRect);

                GameSystems.Reset();

                CurrentGame?.Dispose();
                CurrentGame = new GameManager(GameCanvas);
                CurrentGame.LoadMapAndPositionPlayer(save.CurrentMap, save.PlayerX, save.PlayerY);

                GameSystems.Inventory.LoadFromSavedData(save.Inventory);
                GameSystems.Party.LoadFromSaveData(save.Characters, save.ActivePartyOrder);

                // Loading from the in-game menu: close and reset it
                GameMenuControl.Close();
                GameStateManager.SetState(SceneType.Exploration);

                GameCanvas.Visibility = Visibility.Visible;
                OverlayViewHost.Visibility = Visibility.Collapsed;
                StartScreen.Visibility = Visibility.Collapsed;
                StartScreen.Effect = null;

                await Task.Delay(BlackPauseMs);
                await FadeHelper.FadeFromBlackAsync(FadeRect);
            }
            finally
            {
                isTransitioning = false;
            }
        }

        public static async Task CancelLoadScreenAsync()
        {
            await FadeHelper.FadeToAsync(OverlayViewHost, 0, 0.3);

            StartScreen.Effect = null;

            OverlayViewHost.Visibility = Visibility.Collapsed;
            ContentOverlayViewHost.Visibility = Visibility.Collapsed;

            ContentOverlayViewHost.Dispatcher.Invoke(() =>
            {
                ContentOverlayViewHost.SetValue(ContentPresenter.ContentProperty, null);
            });

            StartMenuContent.Visibility = Visibility.Visible;
            StartMenuContent.Opacity = 1;
        }

        public static async Task ReturnToMainMenuAsync()
        {
            if (isTransitioning) return;
            isTransitioning = true;

            try
            {
                await FadeHelper.FadeToBlackAsync(FadeRect);

                GameMenuControl?.Close();
                GameStateManager.SetState(SceneType.Menu);

                GameCanvas.Visibility = Visibility.Collapsed;
                StartScreen.Visibility = Visibility.Visible;
                StartScreen.Effect = null;
                StartMenuContent.Visibility = Visibility.Visible;

                await Task.Delay(BlackPauseMs);
                await FadeHelper.FadeFromBlackAsync(FadeRect);
            }
            finally
            {
                isTransitioning = false;
            }
        }

        public static async Task StartBattleAsync(List<Enemy> enemies)
        {
            GameFlowManager.StartBattle();
            await FadeHelper.FadeToBlackAsync(FadeRect);

            GameStateManager.SetState(SceneType.Battle);

            var battleActors = new List<BattleActor>();
            battleActors.AddRange(enemies);
            battleActors.AddRange(GameSystems.Party.ActivePartyOrder);

            var battleUI = new BattleUI(battleActors);
            battleUI.SetBackground("Forest.png");
            battleUI.OnBattleEnd += async () =>
            {
                await EndBattleAsync(RootGrid, FadeRect);
            };

            RootGrid.Children.Add(battleUI);
            await FadeHelper.FadeFromBlackAsync(FadeRect);
        }

        public static async Task EndBattleAsync(Grid rootGrid, Rectangle fadeRect)
        {
            await FadeHelper.FadeToBlackAsync(fadeRect);

            GameStateManager.SetState(SceneType.Exploration);

            var battleUI = rootGrid.Children.OfType<BattleUI>().FirstOrDefault();
            if (battleUI != null)
                rootGrid.Children.Remove(battleUI);

            GameFlowManager.EndBattle();

            await FadeHelper.FadeFromBlackAsync(fadeRect);
        }
    }
}
