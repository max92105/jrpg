using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;

namespace Jrpg.Data.Battle;

/// <summary>
/// Reads an enemy's <c>"resistances": { "fire": -25 }</c> into Core's <see cref="Resistances"/>, which carries no JSON
/// attributes (ADR 0002): whole percents by damage type, held in permille (ADR 0026); a type left out is 0, an unknown
/// or repeated one is refused by name. Writes them back the same way, for the editor's enemies (decision 587).
/// </summary>
internal sealed class ResistancesConverter : JsonConverter<Resistances>
{
    private static readonly FrozenDictionary<DamageType, string> _names =
        Enum.GetValues<DamageType>().ToFrozenDictionary(type => type, ContentJsonOptions.EnumName);

    private static readonly FrozenDictionary<string, DamageType> _types = _names.ToFrozenDictionary(entry => entry.Value, entry => entry.Key);

    /// <summary>The name content uses for <paramref name="type"/>, as validation messages write it.</summary>
    public static string NameOf(DamageType type) => _names[type];

    public override Resistances Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("resistances must be an object of damage types and percents.");
        }

        var percents = new Dictionary<DamageType, int>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var name = reader.GetString()!;
            if (!_types.TryGetValue(name, out var type))
            {
                throw new JsonException($"resistances names an unknown damage type '{name}'.");
            }

            reader.Read();
            if (!percents.TryAdd(type, reader.GetInt32()))
            {
                throw new JsonException($"resistances names '{name}' more than once.");
            }
        }

        return Resistances.From(type => StatFormula.PermilleOf(percents.GetValueOrDefault(type)));
    }

    // Whole percents by type in the enum's order, a type at 0 percent left out (decision 587): exact for content, which
    // loads percents as permille times ten; a stray permille under one percent is left out too, not written as 0.
    public override void Write(Utf8JsonWriter writer, Resistances value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var type in Enum.GetValues<DamageType>())
        {
            var percent = StatFormula.WholePercentOf(value.Of(type));
            if (percent != 0)
            {
                writer.WriteNumber(_names[type], percent);
            }
        }

        writer.WriteEndObject();
    }
}
