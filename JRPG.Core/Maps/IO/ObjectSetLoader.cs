using JRPG.Core.Maps.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace JRPG.Core.Maps.IO
{
    public static class ObjectSetLoader
    {
        public static Dictionary<int, ObjectDefinition> Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Dictionary<int, ObjectDefinition>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
