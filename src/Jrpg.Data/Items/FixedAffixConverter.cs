using System.Text.Json;
using System.Text.Json.Serialization;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;

namespace Jrpg.Data.Items;

/// <summary>
/// Reads a unique's fixed affix, <c>{ "affixId": "power", "value": 5 }</c>, as a <see cref="RolledAffix"/> with the
/// pool's stat (ADR 0058, decision 789's mechanism), so a piece built from the record needs no catalog; writes it back
/// as its id and value only, since the stat is the pool's to say.
/// </summary>
/// <param name="pool">The affixes already loaded from <c>affixes.json</c>, by id; only reading names them.</param>
internal sealed class FixedAffixConverter(IReadOnlyDictionary<string, AffixDefinition> pool) : JsonConverter<RolledAffix>
{
    private const string AffixIdField = "affixId";
    private const string ValueField = "value";

    public override RolledAffix Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("a fixed affix must be an object of affixId and value.");
        }

        string? affixId = null;
        int? value = null;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var field = reader.GetString();
            reader.Read();
            if (field == AffixIdField && affixId is null)
            {
                affixId = ReadAffixId(ref reader);
            }
            else if (field == ValueField && value is null)
            {
                value = ReadValue(ref reader);
            }
            else
            {
                throw new JsonException($"a fixed affix holds affixId and value once each, nothing else (found '{field}').");
            }
        }

        return Resolved(
            affixId ?? throw new JsonException("a fixed affix needs its affixId."),
            value ?? throw new JsonException("a fixed affix needs its value."));
    }

    public override void Write(Utf8JsonWriter writer, RolledAffix value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(AffixIdField, value.AffixId);
        writer.WriteNumber(ValueField, value.Value);
        writer.WriteEndObject();
    }

    private static string ReadAffixId(ref Utf8JsonReader reader) =>
        reader.TokenType == JsonTokenType.String
            ? reader.GetString()!
            : throw new JsonException("a fixed affix's affixId must be a string.");

    private static int ReadValue(ref Utf8JsonReader reader) =>
        reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var value)
            ? value
            : throw new JsonException("a fixed affix's value must be a whole number.");

    private RolledAffix Resolved(string affixId, int value) =>
        pool.TryGetValue(affixId, out var affix)
            ? new RolledAffix(affixId, affix.Stat, value)
            : throw new JsonException($"no affix '{affixId}'.");
}
