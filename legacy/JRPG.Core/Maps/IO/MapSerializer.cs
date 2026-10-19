using JRPG.Core.Maps.Models;
using JRPG.Core.Triggers.IO;
using System.Globalization;
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
            var map = JsonSerializer.Deserialize<Map>(json, Options);

            // Objects saved before uids existed get one derived from their type and position, so it
            // is the same every time the map loads (the game remembers opened chests by uid)
            if (map?.Objects != null)
            {
                foreach (var obj in map.Objects)
                {
                    if (string.IsNullOrEmpty(obj.Uid))
                        obj.Uid = string.Format(CultureInfo.InvariantCulture, "{0}@{1:0.###},{2:0.###}", obj.Id, obj.X, obj.Y);
                }
            }

            return map;
        }
    }
}
