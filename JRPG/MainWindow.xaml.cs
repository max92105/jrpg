using JRPG.Core.Battles.Models;
using JRPG.Core.Battles.Models.Enemies;
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
        private PartyManager partyManager;
        private InventoryManager inventoryManager;
        private EquipmentManager equipmentManager;
        private GameSystemManager gameSystemManager;

        private DateTime sessionStartTime;
        private int totalPlayTimeSeconds = 0;
        private Character draggedCharacter = null;

        public MainWindow()
        {
            InitializeComponent();

            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            game = new Game(GameCanvas);

            dialogueManager = new DialogueManager(DialogueUI.TextElement, DialogueUI.Container, DialogueUI.Portrait, DialogueUI.Speaker, DialogueUI.ContinueIcon);
            inventoryManager = new InventoryManager();
            partyManager = new PartyManager();
            equipmentManager = new EquipmentManager();

            gameSystemManager = new GameSystemManager(
               partyManager,
               inventoryManager,
               equipmentManager,
               game,
               FadeRect
           );

            gameSystemManager.StartSessionTimer();

            GameMenuControl = new GameMenuControl(partyManager, inventoryManager, equipmentManager, gameSystemManager);
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
            await FadeHelper.FadeToBlackAsync(FadeRect);

            List<Enemy> enemies = new List<Enemy>()
            {
                new Goblin()
                {
                    SlotIndex = 0,
                },
                new Goblin()
                {
                    SlotIndex = 1,
                },
                new Goblin()
                {
                    SlotIndex = 2,
                }
            };

            List<BattleActor> battleActors = new List<BattleActor>();

            battleActors.AddRange(enemies);
            battleActors.AddRange(partyManager.ActivePartyOrder);

            var battleUI = new BattleUI(battleActors, inventoryManager);

            battleUI.SetBackground("Forest.png");

            battleUI.OnBattleEnd += EndBattle;

            RootGrid.Children.Add(battleUI);

            await FadeHelper.FadeFromBlackAsync(FadeRect);
        }

        private async void EndBattle()
        {
            await FadeHelper.FadeToBlackAsync(FadeRect);

            var battleUI = RootGrid.Children.OfType<BattleUI>().FirstOrDefault();
            if (battleUI != null)
                RootGrid.Children.Remove(battleUI); // Only remove BattleUI

            Focus();
            Keyboard.Focus(GameCanvas);

            await FadeHelper.FadeFromBlackAsync(FadeRect);
        }


        public void ShowDialogue(List<string> pages, string portraitPath = null, string speaker = null, Action callback = null)
        {
            dialogueManager.Show(pages, portraitPath, speaker, callback);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

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
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (dialogueManager.IsActive && e.Key == Key.Space)
            {
                dialogueManager.Skip();
                return;
            }

            if (e.Key == Key.F1)
            {
                CutsceneManager cutsceneManager = new CutsceneManager(CutscenePlayer);
                cutsceneManager.StartIntroCutscene();
            }

            if (!dialogueManager.IsActive && RootGrid.Children.Contains(GameCanvas))
                game.OnKeyDown(e.Key);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (!dialogueManager.IsActive && RootGrid.Children.Contains(GameCanvas))
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