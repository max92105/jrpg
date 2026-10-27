using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;
using Jrpg.Core.Stats;

namespace Jrpg.Data.Battle;

/// <summary>
/// Reads an enemy's <c>"resistances": { "fire": -25 }</c> into Core's <see cref="Resistances"/>, which carries no JSON
/// attributes (ADR 0002): whole percents by damage type, held in permille (ADR 0026); a type left out is 0, an unknown
/// or repeated one is refused by name.
/// </summary>
internal sealed class ResistancesConverter : JsonConverter<Resistances>
{
    private static readonly FrozenDictionary<DamageType, string> _names =
        Enum.GetValues<DamageType>().ToFrozenDictionary(type => type, type => JsonNamingPolicy.CamelCase.ConvertName(type.ToString()));

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

    // Nothing writes enemies.json yet; the enemy editor will add the writing with its round-trip test.
    public override void Write(Utf8JsonWriter writer, Resistances value, JsonSerializerOptions options) =>
        throw new NotSupportedException("Enemy resistances are only read.");
}
