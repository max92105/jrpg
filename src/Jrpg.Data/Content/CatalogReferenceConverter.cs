using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jrpg.Data.Content;

/// <summary>
/// Reads a content id as the record it names, from a catalog already loaded, and writes a record back as its id
/// (decision 789): a skill's tiered effect, later a grant's status. The battle then reads records and never a catalog.
/// </summary>
/// <typeparam name="T">The record named.</typeparam>
/// <param name="records">The catalog the ids name, by id.</param>
/// <param name="idOf">A record's id, written back in its place.</param>
/// <param name="recordName">What the record is called in a refusal, such as "effect".</param>
internal sealed class CatalogReferenceConverter<T>(
    IReadOnlyDictionary<string, T> records, Func<T, string> idOf, string recordName) : JsonConverter<T>
    where T : class
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"the {recordName} must be named by its id, a string.");
        }

        var id = reader.GetString()!;
        return records.TryGetValue(id, out var record) ? record : throw new JsonException($"no {recordName} '{id}'.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
        writer.WriteStringValue(idOf(value));
}
