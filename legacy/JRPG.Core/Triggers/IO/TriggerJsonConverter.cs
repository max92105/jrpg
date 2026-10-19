using JRPG.Core.Triggers.Models;
using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using JRPG.Core.Enums;

namespace JRPG.Core.Triggers.IO
{
    public class TriggerJsonConverter : JsonConverter<Trigger>
    {
        public override Trigger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;

                if (!root.TryGetProperty("type", out var typeProp))
                    throw new JsonException("Missing 'type' field in trigger");

                var typeStr = typeProp.GetString();

                TriggerType triggerType = (TriggerType)Enum.Parse(typeof(TriggerType), typeStr);

                switch (triggerType)
                {
                    case TriggerType.Warp:
                        return JsonSerializer.Deserialize<TriggerWarp>(root.GetRawText(), options);
                    case TriggerType.Dialogue:
                        return JsonSerializer.Deserialize<TriggerDialogue>(root.GetRawText(), options);
                    case TriggerType.Item:
                        return JsonSerializer.Deserialize<TriggerItem>(root.GetRawText(), options);
                    case TriggerType.Battle:
                        return JsonSerializer.Deserialize<TriggerBattle>(root.GetRawText(), options);
                    default:
                        throw new JsonException($"Unknown TriggerType '{typeStr}'");
                };
            }
        }

        public override void Write(Utf8JsonWriter writer, Trigger value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
        }
    }
}