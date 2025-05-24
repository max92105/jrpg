using JRPG.Core.Models;
using JRPG.Core.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JRPG.Core.Maps.IO
{
    public static class TileDatabase
    {
        private static Dictionary<int, TileDefinition> definitions = new Dictionary<int, TileDefinition>();
        private static string filePath = AssetPathResolver.GetTilePath("Tileset.json");

        public static IReadOnlyDictionary<int, TileDefinition> Definitions => definitions;

        public static void AddOrUpdate(TileDefinition def)
        {
            var dict = Definitions as Dictionary<int, TileDefinition>;
            if (dict != null)
            {
                dict[def.Id] = def;
            }
        }

        public static void Load()
        {
            definitions.Clear();

            if (!File.Exists(filePath))
                return;

            var json = File.ReadAllText(filePath);
            var raw = JsonConvert.DeserializeObject<Dictionary<string, TileDefinition>>(json);

            foreach (var kvp in raw)
            {
                if (int.TryParse(kvp.Key, out int id))
                    definitions[id] = kvp.Value;
            }
        }

        public static void Save()
        {
            var serializable = new Dictionary<string, TileDefinition>();
            foreach (var kvp in definitions)
                serializable[kvp.Key.ToString()] = kvp.Value;

            File.WriteAllText(filePath, JsonConvert.SerializeObject(serializable, Formatting.Indented));
        }

        public static void AddTiles(Dictionary<int, TileDefinition> newTiles)
        {
            foreach (var kvp in newTiles)
                definitions[kvp.Key] = kvp.Value;

            Save();
        }

        public static int GetNextId()
        {
            return Definitions.Keys.Any() ? Definitions.Keys.Max() + 1 : 1;
        }
    }
}
