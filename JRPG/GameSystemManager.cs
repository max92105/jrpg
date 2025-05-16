using JRPG.Core.Systems;
using JRPG.Core.Systems.Equipments;
using JRPG.Core.Systems.Inventories;
using JRPG.Core.Systems.Parties;
using JRPG.Core.Systems.SaveGames;
using JRPG.Core.Systems.SaveGames.Models;
using JRPG.Core.Utils;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace JRPG
{
    public class GameSystemManager
    {
        private readonly Game game;
        private readonly FrameworkElement fadeTarget;

        private DateTime sessionStartTime;
        private int totalPlayTimeSeconds;

        public GameSystemManager(
            Game game,
            FrameworkElement fadeTarget)
        {
            this.game = game;
            this.fadeTarget = fadeTarget;
        }

        public void ExitGame()
        {
            Application.Current.Shutdown();
        }

        public void StartSessionTimer()
        {
            sessionStartTime = DateTime.Now;
        }

        public void ResetPlaytime(int savedSeconds)
        {
            totalPlayTimeSeconds = savedSeconds;
            sessionStartTime = DateTime.Now;
        }

        public void SaveGame()
        {
            int slot = AskSaveSlot();
            if (slot == -1) return;

            int sessionSeconds = (int)(DateTime.Now - sessionStartTime).TotalSeconds;
            int totalSeconds = totalPlayTimeSeconds + sessionSeconds;

            SaveGameManager.SaveGame(
                slot,
                GameSystems.Party.AllCharacters,
                GameSystems.Inventory.GetSerializableItems(),
                game.currentMapFile,
                game.playerX,
                game.playerY,
                GameSystems.Party.ActivePartyOrder,
                totalSeconds
            );

            MessageBox.Show($"Game Saved in Slot {slot}!");
            totalPlayTimeSeconds = totalSeconds;
            sessionStartTime = DateTime.Now;
        }

        public async Task<bool> LoadGameAsync(Grid startScreen, Canvas gameCanvas)
        {
            int slot = AskSaveSlot();
            if (slot == -1) return false;

            if (!SaveGameManager.SaveExists(slot))
            {
                MessageBox.Show("No save file found!");
                return false;
            }

            try
            {
                var saveData = SaveGameManager.LoadGame(slot);

                GameSystems.Party.LoadFromSaveData(saveData.Characters, saveData.ActivePartyOrder);
                GameSystems.Inventory.LoadFromSavedData(saveData.Inventory);
                game.currentMapFile = saveData.CurrentMap;
                game.LoadFromFile(game.currentMapFile);
                game.playerX = saveData.PlayerX;
                game.playerY = saveData.PlayerY;

                ResetPlaytime(saveData.PlayTimeSeconds);

                MessageBox.Show("Game Loaded!");

                await FadeHelper.FadeToBlackAsync(fadeTarget);

                startScreen.Visibility = Visibility.Collapsed;
                gameCanvas.Visibility = Visibility.Visible;

                await Task.Delay(300);

                await FadeHelper.FadeFromBlackAsync(fadeTarget);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load save: {ex.Message}");
                return false;
            }
        }

        private int AskSaveSlot()
        {
            var window = new Window
            {
                Title = "Select Save Slot",
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = Brushes.Black,
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
    }
}
