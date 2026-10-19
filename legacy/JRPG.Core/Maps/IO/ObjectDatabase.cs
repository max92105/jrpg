using JRPG.Core.Maps.Models;
using JRPG.Core.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JRPG.Core.Maps.IO
{
    public static class ObjectDatabase
    {
        private static Dictionary<int, ObjectDefinition> definitions = new Dictionary<int, ObjectDefinition>();
        private static string filePath = AssetPathResolver.GetPath("Objects/Objects.json");

        public static IReadOnlyDictionary<int, ObjectDefinition> Definitions => definitions;

        public static void Load()
        {
            definitions.Clear();

            if (!File.Exists(filePath))
                return;

            var json = File.ReadAllText(filePath);
            var list = JsonConvert.DeserializeObject<List<ObjectDefinition>>(json);

            if (list != null)
            {
                foreach (var obj in list)
                {
                    definitions[obj.Id] = obj;
                }
            }
        }

        public static void Save()
        {
            var list = definitions.Values.OrderBy(o => o.Id).ToList();
            var json = JsonConvert.SerializeObject(list, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static void AddOrUpdate(ObjectDefinition def)
        {
            definitions[def.Id] = def;
        }

        public static int GetNextId()
        {
            return definitions.Keys.Any() ? definitions.Keys.Max() + 1 : 1;
        }
    }
}