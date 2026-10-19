using JRPG.Core.Systems;
using JRPG.Core.Systems.SaveGames.Models;
using System;
using System.Threading.Tasks;

namespace JRPG
{
    public class GameSystemManager
    {
        private string currentMapFile;
        private double playerX;
        private double playerY;

        private DateTime sessionStartTime;
        private int totalPlayTimeSeconds;

        public GameSystemManager()
        {

        }

        public void UpdatePlayerPosition(double x, double y)
        {
            this.playerX = x;
            this.playerY = y;
        }

        public void UpdateCurrentMap(string mapFile)
        {
            this.currentMapFile = mapFile;
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

        public async Task SaveToSlotAsync(int slot)
        {
            int sessionSeconds = (int)(DateTime.Now - sessionStartTime).TotalSeconds;
            int totalSeconds = totalPlayTimeSeconds + sessionSeconds;

            GameSystems.Save.SaveGame(
                slot,
                GameSystems.Party.AllCharacters,
                GameSystems.Inventory.GetSerializableItems(),
                currentMapFile,
                playerX,
                playerY,
                GameSystems.Party.ActivePartyOrder,
                totalSeconds
            );

            totalPlayTimeSeconds = totalSeconds;
            sessionStartTime = DateTime.Now;

            await Task.CompletedTask;
        }

        public async Task<SaveData> LoadFromSlotAsync(int slot)
        {
            if (!GameSystems.Save.SaveExists(slot))
                return null;

            try
            {
                var saveData = GameSystems.Save.LoadGame(slot);

                // Update internal values
                this.currentMapFile = saveData.CurrentMap;
                this.playerX = saveData.PlayerX;
                this.playerY = saveData.PlayerY;

                ResetPlaytime(saveData.PlayTimeSeconds);

                // Apply to game systems
                GameSystems.Party.LoadFromSaveData(saveData.Characters, saveData.ActivePartyOrder);
                GameSystems.Inventory.LoadFromSavedData(saveData.Inventory);

                return saveData;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Total play time: what was saved plus the current session.</summary>
        public TimeSpan GetPlayTime() => TimeSpan.FromSeconds(totalPlayTimeSeconds) + (DateTime.Now - sessionStartTime);

        public string GetCurrentMapFile() => currentMapFile;
        public (double x, double y) GetPlayerPosition() => (playerX, playerY);
    }
}
