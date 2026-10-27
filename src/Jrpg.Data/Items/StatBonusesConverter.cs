using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;
using Jrpg.Core.Stats;

namespace Jrpg.Data.Items;

/// <summary>
/// Reads an item's <c>"bonuses": { "power": 5, "fireResistance": 10 }</c> into Core's <see cref="StatBonuses"/>, which
/// carries no JSON attributes (ADR 0002): each stat by its camelCase name, an unknown or repeated one refused by name.
/// </summary>
internal sealed class StatBonusesConverter : JsonConverter<StatBonuses>
{
    private static readonly FrozenDictionary<StatKind, string> _names =
        Enum.GetValues<StatKind>().ToFrozenDictionary(kind => kind, kind => JsonNamingPolicy.CamelCase.ConvertName(kind.ToString()));

    private static readonly FrozenDictionary<string, StatKind> _kinds = _names.ToFrozenDictionary(entry => entry.Value, entry => entry.Key);

    /// <summary>The name content uses for <paramref name="kind"/>, as validation messages write it.</summary>
    public static string NameOf(StatKind kind) => _names[kind];

    public override StatBonuses Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("bonuses must be an object of stat names and numbers.");
        }

        var values = new Dictionary<StatKind, int>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var name = reader.GetString()!;
            if (!_kinds.TryGetValue(name, out var kind))
            {
                throw new JsonException($"bonuses names an unknown stat '{name}'.");
            }

            reader.Read();
            if (!values.TryAdd(kind, reader.GetInt32()))
            {
                throw new JsonException($"bonuses names '{name}' more than once.");
            }
        }

        return new StatBonuses(values);
    }

    public override void Write(Utf8JsonWriter writer, StatBonuses value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var (kind, bonus) in value.Values)
        {
            writer.WriteNumber(_names[kind], bonus);
        }

        writer.WriteEndObject();
    }
}
