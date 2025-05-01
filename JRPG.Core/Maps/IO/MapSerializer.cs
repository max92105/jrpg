using JRPG.Core.Maps.Models;
using System.IO;
using System.Text.Json;

namespace JRPG.Core.Maps.IO
{
    public static class MapSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public static void Save(string path, Map map)
        {
            string json = JsonSerializer.Serialize(map, Options);
            File.WriteAllText(path, json);
        }

        public static Map Load(string path)
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Map>(json, Options);
        }
    }
}
