using JRPG.Core.Battles;
using JRPG.Core.Battles.Models;
using JRPG.Core.Battles.Models.Enemies;
using JRPG.Core.System;
using JRPG.Core.System.Models;
using JRPG.UI.Battle;
using JRPG.UI.GameMenu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace JRPG
{
    public partial class MainWindow : Window
    {
        public Game game;
        public DialogueManager dialogueManager;
        private PartyManager partyManager;
        private InventoryManager inventoryManager;
        private EquipmentManager equipmentManager;

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

        public void StartBattle()
        {
            Fade(1, 0.5, () =>
            {
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

                Fade(0, 0.5);
            });
        }

        private void EndBattle()
        {
            Fade(1, 0.5, () =>
            {
                var battleUI = RootGrid.Children.OfType<BattleUI>().FirstOrDefault();
                if (battleUI != null)
                    RootGrid.Children.Remove(battleUI); // Only remove BattleUI

                Focus();
                Keyboard.Focus(GameCanvas);

                Fade(0, 0.5); // Fade back to game world
            });
        }

        public void Fade(double toOpacity, double durationSec, Action onComplete = null)
        {
            FadeRect.Visibility = Visibility.Visible;
            FadeRect.Width = ActualWidth;
            FadeRect.Height = ActualHeight;

            var storyboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                To = toOpacity,
                Duration = TimeSpan.FromSeconds(durationSec),
                FillBehavior = FillBehavior.Stop
            };

            Storyboard.SetTarget(animation, FadeRect);
            Storyboard.SetTargetProperty(animation, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(animation);

            storyboard.Completed += (s, e) =>
            {
                FadeRect.Opacity = toOpacity;
                if (toOpacity == 0)
                    FadeRect.Visibility = Visibility.Collapsed;

                onComplete?.Invoke();
            };

            storyboard.Begin();
        }

        private async Task FadeInOut(Action onFadeOutComplete)
        {
            FadeRect.Visibility = Visibility.Visible;

            // Fade In to Black
            for (double opacity = 0; opacity <= 1; opacity += 0.03)
            {
                FadeRect.Opacity = opacity;
                await Task.Delay(8); // ~60fps
            }

            onFadeOutComplete?.Invoke();

            // Fade Out to Clear
            for (double opacity = 1; opacity >= 0; opacity -= 0.03)
            {
                FadeRect.Opacity = opacity;
                await Task.Delay(8);
            }

            FadeRect.Visibility = Visibility.Collapsed;
        }

        private async Task FadeToBlack()
        {
            FadeRect.Visibility = Visibility.Visible;

            for (double opacity = 0; opacity <= 1; opacity += 0.03)
            {
                FadeRect.Opacity = opacity;
                await Task.Delay(8); // ~30 FPS
            }
        }

        private async Task FadeFromBlack()
        {
            for (double opacity = 1; opacity >= 0; opacity -= 0.03)
            {
                FadeRect.Opacity = opacity;
                await Task.Delay(8);
            }

            FadeRect.Visibility = Visibility.Collapsed;
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
            // Fade to black first
            await FadeToBlack();

            // Switch screens AFTER full black
            StartScreen.Visibility = Visibility.Collapsed;
            GameCanvas.Visibility = Visibility.Visible;

            // Small delay to make sure WPF refreshes layout
            await Task.Delay(500);

            // Then fade back in
            await FadeFromBlack();
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            GameMenuControl.Visibility = Visibility.Collapsed;
        }

        private async void ReturnToMenuButton_Click(object sender, RoutedEventArgs e)
        {
            await FadeInOut(() =>
            {
                GameMenuControl.Visibility = Visibility.Collapsed;
                GameCanvas.Visibility = Visibility.Collapsed;
                StartScreen.Visibility = Visibility.Visible;
            });
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var slot = AskSaveSlot();
            if (slot == -1) return;

            var sessionSeconds = (int)(DateTime.Now - sessionStartTime).TotalSeconds;
            var totalSeconds = totalPlayTimeSeconds + sessionSeconds;

            SaveGameManager.SaveGame(slot, partyManager.AllCharacters, inventoryManager.GetSerializableItems(), equipmentManager.WeaponStock, equipmentManager.ArmorStock, game.currentMapFile, game.playerX, game.playerY, partyManager.ActivePartyOrder, totalSeconds);
            MessageBox.Show($"Game Saved in Slot {slot}!");

            sessionStartTime = DateTime.Now;
            totalPlayTimeSeconds = totalSeconds;
        }

        private int AskSaveSlot()
        {
            var window = new Window
            {
                Title = "Select Save Slot",
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = System.Windows.Media.Brushes.Black,
                Owner = Application.Current.MainWindow
            };

            var stack = new StackPanel { Margin = new Thickness(20) };

            for (int i = 1; i <= 3; i++)
            {
                var slotPath = Path.Combine("Saves", $"save{i}.json");
                string display = $"Slot {i}: (Empty)";

                if (File.Exists(slotPath))
                {
                    try
                    {
                        string json = File.ReadAllText(slotPath);
                        var saveData = JsonSerializer.Deserialize<SaveData>(json);

                        if (saveData != null)
                        {
                            var playtime = TimeSpan.FromSeconds(saveData.PlayTimeSeconds);
                            string timeFormatted = $"{(int)playtime.TotalHours}h {playtime.Minutes}m {playtime.Seconds}s";

                            display = $"Slot {i}: Lv {saveData.PartyAverageLevel} - {saveData.LocationName}\n" +
                                      $"Saved: {saveData.SaveTimestamp:yyyy-MM-dd HH:mm} — Playtime: {timeFormatted}";
                        }
                    }
                    catch
                    {
                        display = $"Slot {i}: (Corrupt)";
                    }
                }

                var button = new Button
                {
                    Content = display,
                    Width = 300,
                    Height = 50,
                    Margin = new Thickness(5),
                    Tag = i
                };

                button.Click += (s, e) =>
                {
                    window.Tag = (int)((Button)s).Tag;
                    window.Close();
                };

                stack.Children.Add(button);
            }

            window.Content = stack;
            window.ShowDialog();

            if (window.Tag is int slot)
                return slot;
            return -1;
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var slot = AskSaveSlot();
            if (slot == -1) return;

            if (!SaveGameManager.SaveExists(slot))
            {
                MessageBox.Show("No save file found in that slot!");
                return;
            }

            if (!SaveGameManager.SaveExists(slot))
            {
                MessageBox.Show("No save file found!");
                return;
            }

            try
            {
                var saveData = SaveGameManager.LoadGame(slot);

                partyManager.LoadFromSaveData(saveData.Characters, saveData.ActivePartyOrder);
                inventoryManager.LoadFromSavedData(saveData.Inventory);
                equipmentManager.LoadFromSavedData(saveData.WeaponStock, saveData.ArmorStock);
                game.currentMapFile = saveData.CurrentMap;
                game.LoadFromFile(game.currentMapFile);

                game.playerX = saveData.PlayerX;
                game.playerY = saveData.PlayerY;

                //totalGold = saveData.Gold; Not Implemented Yet
                totalPlayTimeSeconds = saveData.PlayTimeSeconds;

                MessageBox.Show("Game Loaded!");

                // Fade to black first
                await FadeToBlack();

                // Switch screens AFTER full black
                StartScreen.Visibility = Visibility.Collapsed;
                GameCanvas.Visibility = Visibility.Visible;

                // Small delay to make sure WPF refreshes layout
                await Task.Delay(500);

                // Then fade back in
                await FadeFromBlack();

                sessionStartTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load save: {ex.Message}");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {        
            game = new Game(GameCanvas);

            dialogueManager = new DialogueManager(DialogueUI.TextElement, DialogueUI.Container, DialogueUI.Portrait, DialogueUI.Speaker, DialogueUI.ContinueIcon);
            inventoryManager = new InventoryManager();
            partyManager = new PartyManager();
            equipmentManager = new EquipmentManager();

            GameMenuControl = new GameMenuControl(partyManager, inventoryManager, equipmentManager);
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
    }
}