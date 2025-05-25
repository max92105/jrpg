using JRPG.Core.Maps.Models;
using JRPG.Core.Models;
using JRPG.Core.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JRPG.Core.Maps.IO
{
    public static class TileGroupDatabase
    {
        private static Dictionary<int, TileGroup> groups = new Dictionary<int, TileGroup>();
        private static string filePath = AssetPathResolver.GetTilePath("TileGroups.json");

        public static IReadOnlyDictionary<int, TileGroup> Groups => groups;

        public static void Load()
        {
            groups.Clear();
            if (!File.Exists(filePath)) return;

            var json = File.ReadAllText(filePath);
            var raw = JsonConvert.DeserializeObject<List<TileGroup>>(json);

            foreach (var group in raw)
            {
                if (group != null)
                    groups[group.Id] = group;
            }
        }

        public static void Save()
        {
            var list = groups.Values.ToList();
            var json = JsonConvert.SerializeObject(list, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static void AddOrUpdate(TileGroup group)
        {
            if (group == null) return;

            groups[group.Id] = group;
            Save();
        }

        public static int GetNextId()
        {
            return groups.Count > 0 ? groups.Keys.Max() + 1 : 1;
        }

        public static void Remove(int id)
        {
            if (groups.Remove(id))
                Save();
        }
    }
}
