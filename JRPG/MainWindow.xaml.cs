using JRPG.Core.Battles.Models;
using JRPG.Core.Battles.Models.Enemies;
using JRPG.Core.Systems;
using JRPG.Core.Utils;
using JRPG.UI.Battle;
using JRPG.UI.GameMenu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace JRPG
{
    public partial class MainWindow : Window
    {
        public Game game;
        public DialogueManager dialogueManager;

        private DateTime sessionStartTime;
        private int totalPlayTimeSeconds = 0;
        private Character draggedCharacter = null;

        private bool IsInputLocked => isTransitioning ||
                                    (dialogueManager?.IsActive ?? false) ||
                                    (CutsceneManager.IsPlaying) ||
                                    isInBattle;

        private bool isTransitioning = false;
        private bool isInBattle = false;
        private bool IsGameReady => game != null && GameCanvas != null && RootGrid.Children.Contains(GameCanvas);

        public MainWindow()
        {
            InitializeComponent();

            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;
            //Topmost = true;

            MenuBackground.Source = new BitmapImage(new Uri(Path.Combine(AssetPathResolver.GetBackgroundsPath("StartMenu.png"))));

            // ✅ Initialize systems once
            GameSystems.Initialize();

            // 🔹 Preload view to prevent layout lag
            Dispatcher.InvokeAsync(() =>
            {
                var preload = new SaveLoadView(
                    SaveLoadView.SaveLoadMode.Load,
                    _ => Task.CompletedTask,
                    () => { }
                );
                preload.Measure(new Size(1, 1));
                preload.Arrange(new Rect(0, 0, 1, 1));
            }, System.Windows.Threading.DispatcherPriority.Loaded);

            CompositionTarget.Rendering += (s, eventArgs) =>
            {
                if (StartScreen.Visibility == Visibility.Visible || GameMenuControl?.Visibility == Visibility.Visible)
                    return;

                if (dialogueManager != null && !dialogueManager.IsActive)
                    game?.Update();

                game?.Render();
            };
        }

        private void NewGame()
        {
            GameSystems.Reset(); // optional, if you want clean state
            InitializeGameState();

            game.LoadFromFile("start.json");
            game.playerX = 10.0;
            game.playerY = 3.0;

            GameCanvas.Visibility = Visibility.Collapsed;
        }

        private void InitializeGameState()
        {
            if (game == null)
                game = new Game(GameCanvas);

            if (dialogueManager == null)
                dialogueManager = new DialogueManager(DialogueUI.TextElement, DialogueUI.Container, DialogueUI.Portrait, DialogueUI.Speaker, DialogueUI.ContinueIcon);

            if (GameMenuControl == null)
            {
                GameMenuControl = new GameMenuControl();
                Panel.SetZIndex(GameMenuControl, 900);
                RootGrid.Children.Add(GameMenuControl);
            }

            GameMenuControl.ReturnToMainMenuRequested -= ReturnToMainMenuAsync;
            GameMenuControl.ReturnToMainMenuRequested += ReturnToMainMenuAsync;

            GameMenuControl.OnLoadSlotConfirmed = async (slotIndex) =>
            {
                var save = await GameSystems.System.LoadFromSlotAsync(slotIndex);
                if (save == null)
                {
                    MessageBox.Show("Failed to load save file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await FadeHelper.FadeToBlackAsync(FadeRect);
                StartScreen.Visibility = Visibility.Collapsed;
                GameCanvas.Visibility = Visibility.Visible;
                GameMenuControl.Visibility = Visibility.Collapsed;

                game.LoadFromFile(save.CurrentMap);
                game.playerX = save.PlayerX;
                game.playerY = save.PlayerY;

                await Task.Delay(300);
                await FadeHelper.FadeFromBlackAsync(FadeRect);

                GameMenuControl.GameMenuContent.Content = null;
            };

            GameMenuControl.Visibility = Visibility.Collapsed;
        }

        public async void StartBattle()
        {
            if (isTransitioning || isInBattle) return;
            isTransitioning = true;
            isInBattle = true;

            await FadeHelper.FadeToBlackAsync(FadeRect);

            List<Enemy> enemies = new List<Enemy>()
            {
                new Goblin() { SlotIndex = 0 },
                new Goblin() { SlotIndex = 1 },
                new Goblin() { SlotIndex = 2 }
            };

            var battleActors = new List<BattleActor>();
            battleActors.AddRange(enemies);
            battleActors.AddRange(GameSystems.Party.ActivePartyOrder);

            var battleUI = new BattleUI(battleActors);
            battleUI.SetBackground("Forest.png");
            battleUI.OnBattleEnd += EndBattle;

            RootGrid.Children.Add(battleUI);

            await FadeHelper.FadeFromBlackAsync(FadeRect);
            isTransitioning = false;
        }

        private async void EndBattle()
        {
            if (isTransitioning) return;
            isTransitioning = true;

            await FadeHelper.FadeToBlackAsync(FadeRect);

            var battleUI = RootGrid.Children.OfType<BattleUI>().FirstOrDefault();
            if (battleUI != null)
                RootGrid.Children.Remove(battleUI);

            Focus();
            Keyboard.Focus(GameCanvas);

            await FadeHelper.FadeFromBlackAsync(FadeRect);

            isInBattle = false;
            isTransitioning = false;
        }

        public void ShowDialogue(List<string> pages, string portraitPath = null, string speaker = null, Action callback = null)
        {
            dialogueManager.Show(pages, portraitPath, speaker, callback);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // 🔹 ESC during loading screen: Cancel and return to Start Menu
            if (e.Key == Key.Escape && OverlayViewHost.Visibility == Visibility.Visible)
            {
                _ = CancelLoadScreenAsync(); // run async without blocking
                return;
            }

            // 🔹 ESC toggles Game Menu if not on Start Screen
            if (e.Key == Key.Escape && StartScreen?.Visibility != Visibility.Visible)
            {
                if (GameMenuControl != null)
                {
                    bool showing = GameMenuControl.Visibility == Visibility.Visible;
                    GameMenuControl.Visibility = showing ? Visibility.Collapsed : Visibility.Visible;
                    Panel.SetZIndex(GameMenuControl, showing ? 0 : 1000);
                }
                return;
            }

            // 🔹 SPACE skips dialogue
            if (dialogueManager?.IsActive == true && e.Key == Key.Space)
            {
                dialogueManager.Skip();
                return;
            }

            // 🔹 F1 starts cutscene
            if (e.Key == Key.F1 && CutscenePlayer != null)
            {
                var cutsceneManager = new CutsceneManager(CutscenePlayer);
                cutsceneManager.StartIntroCutscene();
                return;
            }

            // 🔹 Block input if locked
            if (IsInputLocked) return;

            // 🔹 Forward key to game if active
            if (game != null && RootGrid.Children.Contains(GameCanvas))
                game.OnKeyDown(e.Key);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (IsInputLocked) return;

            if (IsGameReady)
                game.OnKeyUp(e.Key);
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            await FadeHelper.FadeToBlackAsync(FadeRect);

            // Switch screens AFTER full black
            NewGame();

            StartScreen.Visibility = Visibility.Collapsed;
            GameCanvas.Visibility = Visibility.Visible;

            // Small delay to make sure WPF refreshes layout
            await Task.Delay(300);

            await FadeHelper.FadeFromBlackAsync(FadeRect);

            Focus();
        }

        public async Task ReturnToMainMenuAsync()
        {
            await FadeHelper.FadeToBlackAsync(FadeRect);

            GameMenuControl.Visibility = Visibility.Collapsed;
            GameCanvas.Visibility = Visibility.Collapsed;
            StartScreen.Visibility = Visibility.Visible;
            StartScreen.Effect = null;
            StartMenuContent.Visibility = Visibility.Visible;

            await Task.Delay(300); // Optional delay for layout

            await FadeHelper.FadeFromBlackAsync(FadeRect);
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            ShowLoadScreen();
        }

        private async void ShowLoadScreen()
        {
            // Hide the main menu UI
            StartMenuContent.Visibility = Visibility.Collapsed;

            // Apply blur to background
            StartScreen.Effect = new BlurEffect
            {
                Radius = 8,
                RenderingBias = RenderingBias.Quality
            };

            // Prepare overlay visibility and opacity
            OverlayViewHost.Visibility = Visibility.Visible;
            OverlayViewHost.Opacity = 0;
            ContentOverlayViewHost.Visibility = Visibility.Visible;

            // Use a very lightweight placeholder (ensures layout pass without delay)
            ContentOverlayViewHost.Content = new Grid();

            // Start fade on OverlayViewHost immediately
            var fadeInTask = FadeHelper.FadeToAsync(OverlayViewHost, 1, 0.5); // Slower for visibility

            // Defer the actual heavy content setup *after* the fade begins
            await Task.Delay(10); // Let WPF breathe
            await Dispatcher.InvokeAsync(() =>
            {
                var loadView = new SaveLoadView(
                    SaveLoadView.SaveLoadMode.Load,
                    async (slotIndex) =>
                    {
                        var save = await GameSystems.System.LoadFromSlotAsync(slotIndex);
                        if (save == null)
                        {
                            MessageBox.Show("Failed to load save file.");
                            return;
                        }

                        await FadeHelper.FadeToBlackAsync(FadeRect);

                        InitializeGameState();

                        StartScreen.Visibility = Visibility.Collapsed;
                        GameCanvas.Visibility = Visibility.Visible;

                        game.LoadFromFile(save.CurrentMap);
                        game.playerX = save.PlayerX;
                        game.playerY = save.PlayerY;

                        ContentOverlayViewHost.Content = null;
                        ContentOverlayViewHost.Visibility = Visibility.Collapsed;
                        OverlayViewHost.Visibility = Visibility.Collapsed;

                        await Task.Delay(300);
                        await FadeHelper.FadeFromBlackAsync(FadeRect);
                    },
                    () => { _ = CancelLoadScreenAsync(); }
                );

                ContentOverlayViewHost.Content = loadView;
                ContentOverlayViewHost.UpdateLayout();
            }, System.Windows.Threading.DispatcherPriority.Background);

            await fadeInTask;
        }

        private async Task CancelLoadScreenAsync()
        {
            await FadeHelper.FadeToAsync(OverlayViewHost, 0, 0.3);

            // Hide overlays and reset content
            OverlayViewHost.Visibility = Visibility.Collapsed;
            ContentOverlayViewHost.Visibility = Visibility.Collapsed;
            ContentOverlayViewHost.Content = null;

            // Remove blur and show main menu again
            StartScreen.Effect = null;
            StartMenuContent.Visibility = Visibility.Visible;

            await FadeHelper.FadeToAsync(StartMenuContent, 1, 0.3);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}