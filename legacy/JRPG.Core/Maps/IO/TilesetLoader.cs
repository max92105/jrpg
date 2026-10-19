using JRPG.Core.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace JRPG.Core.Maps.IO
{
    public static class TilesetLoader
    {
        public static Dictionary<int, TileDefinition> Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Dictionary<int, TileDefinition>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
