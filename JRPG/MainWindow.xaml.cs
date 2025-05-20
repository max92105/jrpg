using JRPG.Core.Battles.Models;
using JRPG.Core.Battles.Models.Enemies;
using JRPG.Core.Systems;
using JRPG.Core.Utils;
using JRPG.UI.Battle;
using JRPG.UI.GameMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace JRPG
{
    public partial class MainWindow : Window
    {
        public Game game;
        public DialogueManager dialogueManager;
        private GameSystemManager gameSystemManager;

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
            GameSystems.Initialize(); // includes GameSystems.System
            gameSystemManager = GameSystems.System;

            game = new Game(GameCanvas);
            dialogueManager = new DialogueManager(DialogueUI.TextElement, DialogueUI.Container, DialogueUI.Portrait, DialogueUI.Speaker, DialogueUI.ContinueIcon);

            GameMenuControl = new GameMenuControl();
            GameMenuControl.OnLoadSlotConfirmed = async (slotIndex) =>
            {
                var save = await gameSystemManager.LoadFromSlotAsync(slotIndex);
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
            Panel.SetZIndex(GameMenuControl, 1000);
            RootGrid.Children.Add(GameMenuControl);

            GameCanvas.Visibility = Visibility.Collapsed;
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
            // ESC toggles GameMenu unless on StartScreen
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

            // SPACE skips dialogue
            if (dialogueManager?.IsActive == true && e.Key == Key.Space)
            {
                dialogueManager.Skip();
                return;
            }

            // F1 starts cutscene
            if (e.Key == Key.F1 && CutscenePlayer != null)
            {
                var cutsceneManager = new CutsceneManager(CutscenePlayer);
                cutsceneManager.StartIntroCutscene();
                return;
            }

            if (IsInputLocked) return;

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

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            //await gameSystemManager.LoadGameAsync(StartScreen, GameCanvas);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}