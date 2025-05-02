using JRPG.Core.Maps.Models;
using JRPG.Core.Triggers.IO;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JRPG.Core.Maps.IO
{
    public static class MapSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            Converters = 
            {
                new JsonStringEnumConverter(),
                new TriggerJsonConverter() 
            }
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
