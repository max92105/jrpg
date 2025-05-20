using JRPG.Core.Battles.Models;
using JRPG.Core.Systems.SaveGames.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace JRPG.Core.Systems.SaveGames
{
    public class SaveGameManager
    {
        private readonly string SaveDirectory = "Saves";

        public void SaveGame(int slot,
            List<Character> characters,
            List<SavedInventoryEntry> playerInventory,
            string currentMapFile, 
            double playerX, 
            double playerY,
            List<Character> activePartyOrder,
            int totalPlayTimeSeconds
            )
        {
            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);

            var saveData = new SaveData
            {
                Characters = characters,
                Inventory = playerInventory,
                CurrentMap = currentMapFile,
                PlayerX = playerX,
                PlayerY = playerY,
                ActivePartyOrder = activePartyOrder.Select(c => c.Name).ToList(),
                //Gold = totalGold Not implemented yet
                PartyAverageLevel = (int)characters.Average(c => c.Level),
                LocationName = GetLocationName(currentMapFile),
                SaveTimestamp = DateTime.Now,
                PlayTimeSeconds = totalPlayTimeSeconds
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(saveData, options);

            File.WriteAllText(Path.Combine(SaveDirectory, $"save{slot}.json"), json);
        }

        public string GetLocationName(string mapFilename)
        {
            // Expand this later
            switch(mapFilename)
            {
                case "start.json":
                    return "Forest";
                case "start2.json":
                    return "Forest 2";
                default:
                    return "Unknown Location";
            };
        }

        public SaveData LoadGame(int slot)
        {
            string path = Path.Combine(SaveDirectory, $"save{slot}.json");

            if (!File.Exists(path))
                throw new FileNotFoundException($"Save file for slot {slot} not found.");

            string json = File.ReadAllText(path);
            var saveData = JsonSerializer.Deserialize<SaveData>(json);

            if (saveData == null)
                throw new Exception("Failed to deserialize save file.");

            return saveData;
        }

        public bool SaveExists(int slot)
        {
            return File.Exists(Path.Combine(SaveDirectory, $"save{slot}.json"));
        }

        public List<SaveDataPreview> GetAllSavePreviews()
        {
            var previews = new List<SaveDataPreview>();

            for (int i = 0; i < 20; i++) // example: 3 slots
            {
                string path = Path.Combine("Saves", $"save{i}.json");

                if (!File.Exists(path))
                {
                    previews.Add(new SaveDataPreview
                    {
                        SlotIndex = i,
                        IsEmpty = true
                    });
                    continue;
                }

                try
                {
                    var json = File.ReadAllText(path);
                    var data = JsonSerializer.Deserialize<SaveData>(json);

                    previews.Add(new SaveDataPreview
                    {
                        SlotIndex = i,
                        IsEmpty = false,
                        Timestamp = data.SaveTimestamp,
                        MapName = data.LocationName,
                        Party = data.Characters
                    });
                }
                catch
                {
                    previews.Add(new SaveDataPreview
                    {
                        SlotIndex = i,
                        IsCorrupt = true
                    });
                }
            }

            return previews;
        }
    }
}
