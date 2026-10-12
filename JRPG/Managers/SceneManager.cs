using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.Core.Systems.SaveGames.Models;
using System;
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

        // Only one scene change at a time (warp, battle start, new game, load...): two fades on the
        // same black screen fight each other, and the second change can land in the middle of the first
        private static bool isTransitioning;

        /// <summary>True while the screen is fading for a scene change. Nothing else should start one.</summary>
        public static bool IsTransitioning => isTransitioning;

        /// <summary>
        /// Quick fade to black, runs the change (e.g. switch map), fades back.
        /// Returns false, without doing anything, if another scene change is already running.
        /// The screen always comes back from black, even if the change fails.
        /// </summary>
        public static async Task<bool> TransitionAsync(Action whileBlack)
        {
            if (isTransitioning)
                return false;
            isTransitioning = true;

            try
            {
                await FadeHelper.FadeToBlackAsync(FadeRect, 0.25);
                try
                {
                    whileBlack();
                }
                finally
                {
                    await Task.Delay(80);
                    await FadeHelper.FadeFromBlackAsync(FadeRect, 0.25);
                }
            }
            finally
            {
                isTransitioning = false;
            }

            return true;
        }

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
                GameSystems.World.LoadFrom(save.CompletedEvents, save.ObjectSwaps); // before maps load

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

                ShowTitleScreen();

                await Task.Delay(BlackPauseMs);
                await FadeHelper.FadeFromBlackAsync(FadeRect);
            }
            finally
            {
                isTransitioning = false;
            }
        }

        /// <summary>Swaps to the title screen. Call while the screen is black.</summary>
        private static void ShowTitleScreen()
        {
            GameMenuControl?.Close();
            GameStateManager.SetState(SceneType.Menu);

            GameCanvas.Visibility = Visibility.Collapsed;
            StartScreen.Visibility = Visibility.Visible;
            StartScreen.Effect = null;
            StartMenuContent.Visibility = Visibility.Visible;
        }

        /// <summary>Starts a battle. Does nothing if another scene change (e.g. a warp) is running.</summary>
        public static async Task StartBattleAsync(List<Enemy> enemies)
        {
            if (isTransitioning)
                return;
            isTransitioning = true;

            try
            {
                GameFlowManager.StartBattle();
                await FadeHelper.FadeToBlackAsync(FadeRect);

                GameStateManager.SetState(SceneType.Battle);

                var battleActors = new List<BattleActor>();
                battleActors.AddRange(enemies);
                battleActors.AddRange(GameSystems.Party.ActivePartyOrder);

                var battleUI = new BattleUI(battleActors);
                battleUI.SetBackground("Forest.png");
                battleUI.OnBattleEnd += async exit => await EndBattleAsync(exit);

                RootGrid.Children.Add(battleUI);
                await FadeHelper.FadeFromBlackAsync(FadeRect);
            }
            finally
            {
                isTransitioning = false;
            }
        }

        /// <summary>
        /// Removes the battle screen and goes where the battle says:
        /// back to the map (victory, escape), or the title screen (defeat), with the Load list open if asked.
        /// </summary>
        public static async Task EndBattleAsync(BattleExit exit)
        {
            await FadeHelper.FadeToBlackAsync(FadeRect);

            var battleUI = RootGrid.Children.OfType<BattleUI>().FirstOrDefault();
            if (battleUI != null)
                RootGrid.Children.Remove(battleUI);

            if (exit == BattleExit.BackToMap)
                GameFlowManager.EndBattle();
            else
                ShowTitleScreen();

            await Task.Delay(BlackPauseMs);
            await FadeHelper.FadeFromBlackAsync(FadeRect);

            if (exit == BattleExit.LoadSave)
                await ShowLoadScreenAsync();
        }
    }
}
