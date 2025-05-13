using JRPG.Core.Battles.Models;
using JRPG.Core.System.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace JRPG.Core.System
{
    public class SaveGameManager
    {
        private static readonly string SaveDirectory = "Saves";

        public static void SaveGame(int slot,
            List<Character> characters,
            List<InventoryItem> playerInventory,
            Dictionary<string, int> weaponStock,
            Dictionary<string, int> armorStock,
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
                WeaponStock = weaponStock,
                ArmorStock = armorStock,
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

        public static string GetLocationName(string mapFilename)
        {
            // Expand this later
            switch(mapFilename)
            {
                case "village.map":
                    return "Village of Beginnings";
                case "house.map":
                    return "House";
                case "forest.map":
                    return "Mystic Forest";
                case "cave.map":
                    return "Goblin Cave";
                default:
                    return "Unknown Location";
            };
        }

        public static SaveData LoadGame(int slot)
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

        public static bool SaveExists(int slot)
        {
            return File.Exists(Path.Combine(SaveDirectory, $"save{slot}.json"));
        }
    }
}
