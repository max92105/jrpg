using JRPG.Core.Battles.Models;
using JRPG.Core.Battles.Models.Enemies;
using JRPG.Core.Systems;
using JRPG.Core.Systems.Equipments;
using JRPG.Core.Systems.Inventories;
using JRPG.Core.Systems.Parties;
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

        private bool IsInputLocked => isTransitioning || dialogueManager.IsActive || CutsceneManager.IsPlaying || isInBattle;
        private bool isTransitioning = false;
        private bool isInBattle = false;

        public MainWindow()
        {
            InitializeComponent();

            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;
            //Topmost = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            game = new Game(GameCanvas);

            dialogueManager = new DialogueManager(DialogueUI.TextElement, DialogueUI.Container, DialogueUI.Portrait, DialogueUI.Speaker, DialogueUI.ContinueIcon);

            GameSystems.Initialize();

            gameSystemManager = new GameSystemManager(
               game,
               FadeRect
           );

            gameSystemManager.StartSessionTimer();

            GameMenuControl = new GameMenuControl(gameSystemManager);
            GameMenuControl.Visibility = Visibility.Collapsed;
            Panel.SetZIndex(GameMenuControl, 1000);
            RootGrid.Children.Add(GameMenuControl);

            GameCanvas.Visibility = Visibility.Collapsed; // Hide gameplay at start     

            CompositionTarget.Rendering += (s, eventArgs) =>
            {
                if (StartScreen.Visibility == Visibility.Visible || GameMenuControl.Visibility == Visibility.Visible)
                    return; // Don't update world if Start Menu or Game Menu open

                if (!dialogueManager.IsActive)
                    game.Update();

                game.Render();
            };

            Loaded += (s, routedEvent) => Keyboard.Focus(GameCanvas);
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
            // ESC should always work unless on StartScreen
            if (e.Key == Key.Escape && StartScreen.Visibility != Visibility.Visible)
            {
                if (GameMenuControl.Visibility == Visibility.Visible)
                {
                    GameMenuControl.Visibility = Visibility.Collapsed;
                    Panel.SetZIndex(GameMenuControl, 0);
                }
                else
                {
                    GameMenuControl.Visibility = Visibility.Visible;
                    Panel.SetZIndex(GameMenuControl, 1000);
                }
                return;
            }

            // SPACE should always work when dialogue is active
            if (dialogueManager.IsActive && e.Key == Key.Space)
            {
                dialogueManager.Skip();
                return;
            }

            // Allow cutscene launch even during gameplay (can limit if needed)
            if (e.Key == Key.F1)
            {
                var cutsceneManager = new CutsceneManager(CutscenePlayer);
                cutsceneManager.StartIntroCutscene();
                return;
            }

            if (IsInputLocked) return;

            if (RootGrid.Children.Contains(GameCanvas))
                game.OnKeyDown(e.Key);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (IsInputLocked) return;

            if (RootGrid.Children.Contains(GameCanvas))
                game.OnKeyUp(e.Key);
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            await FadeHelper.FadeToBlackAsync(FadeRect);

            // Switch screens AFTER full black
            StartScreen.Visibility = Visibility.Collapsed;
            GameCanvas.Visibility = Visibility.Visible;

            // Small delay to make sure WPF refreshes layout
            await Task.Delay(300);

            await FadeHelper.FadeFromBlackAsync(FadeRect);
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            GameMenuControl.Visibility = Visibility.Collapsed;
        }

        private async void ReturnToMenuButton_Click(object sender, RoutedEventArgs e)
        {
            await FadeHelper.FadeToBlackAsync(FadeRect);

            GameMenuControl.Visibility = Visibility.Collapsed;
            GameCanvas.Visibility = Visibility.Collapsed;
            StartScreen.Visibility = Visibility.Visible;

            await Task.Delay(300);

            await FadeHelper.FadeFromBlackAsync(FadeRect);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            gameSystemManager.SaveGame();
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            await gameSystemManager.LoadGameAsync(StartScreen, GameCanvas);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            gameSystemManager.ExitGame();
        }
    }
}